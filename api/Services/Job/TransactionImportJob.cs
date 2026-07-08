using System.Globalization;
using System.Text;
using System.Text.Json;
using api.Exceptions;
using api.Models.FileCategory;
using api.Models.Files;
using api.Models.Job;
using api.Requests.Transaction;
using api.Services.File;
using api.Services.FileProcessing;
using api.Services.Transaction;
using api.Services.TransactionImport;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using DatabaseContext = api.Models.Database.DatabaseContext;

namespace api.Services.Job;

public class TransactionImportJob(
  IServiceScopeFactory serviceScopeFactory,
  IConnectionMultiplexer connectionMultiplexer,
  ILogger<TransactionImportJob> logger
) : BackgroundService
{
  private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
  private readonly IDatabase _redis = connectionMultiplexer.GetDatabase();
  private readonly ILogger<TransactionImportJob> _logger = logger;
  private static readonly string[] IgnoredImportTitleTexts =
  [
    "Pagamento recebido",
    "Pagamento de fatura"
  ];

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      var job = await _redis.ListLeftPopAsync(JobService.TransactionImportQueueKey);

      if (!job.HasValue)
      {
        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        continue;
      }

      await ProcessJob(job.ToString(), stoppingToken);
    }
  }

  private async Task ProcessJob(string json, CancellationToken cancellationToken)
  {
    TransactionImportJobPayload? payload = null;

    try
    {
      payload = JsonSerializer.Deserialize<TransactionImportJobPayload>(json);

      if (payload is null)
      {
        throw new InvalidOperationException("Transaction import job payload is empty.");
      }

      using var scope = _serviceScopeFactory.CreateScope();
      var fileService = scope.ServiceProvider.GetRequiredService<FileService>();
      var fileProcessingService = scope.ServiceProvider.GetRequiredService<FileProcessingService>();
      var transactionService = scope.ServiceProvider.GetRequiredService<TransactionService>();
      var transactionImportService = scope.ServiceProvider.GetRequiredService<TransactionImportService>();
      var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

      await fileProcessingService.MarkProcessing(payload.FileProcessingId);

      var file = await fileService.Get(payload.FileId);
      var requests = MapTransactionRequests(file.Category, file.Data);

      await using var importTransaction = await context.Database.BeginTransactionAsync(cancellationToken);

      foreach (var request in requests)
      {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
          var transaction = await transactionService.Create(request);

          await transactionImportService.Create(payload.FileProcessingId, transaction.Id, cancellationToken);
        }
        catch (ExistsTransactionException)
        {
          continue;
        }
      }

      await importTransaction.CommitAsync(cancellationToken);
      await fileProcessingService.MarkFinished(payload.FileProcessingId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "[TransactionImportJob]");

      if (payload is not null)
      {
        await MarkFailed(payload.FileProcessingId);
      }
    }
  }

  private async Task MarkFailed(Guid fileProcessingId)
  {
    try
    {
      using var scope = _serviceScopeFactory.CreateScope();
      var fileProcessingService = scope.ServiceProvider.GetRequiredService<FileProcessingService>();

      await fileProcessingService.MarkFailed(fileProcessingId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "[TransactionImportJob] Failed to mark file processing as failed.");
    }
  }

  private static List<CreateTransactionRequest> MapTransactionRequests(
    FileCategoryName category,
    byte[] data
  )
  {
    return category switch
    {
      FileCategoryName.CreditCard => ParseCreditCardTransactionRequests(data),
      FileCategoryName.Extrato => ParseExtratoTransactionRequests(data),
      _ => throw new InvalidOperationException($"Unsupported file category: {category}")
    };
  }

  private static List<CreateTransactionRequest> ParseCreditCardTransactionRequests(byte[] data)
  {
    return ParseCsv<CreditCardNubankFile>(data)
      .Select(MapTransactionRequest)
      .Where(request => !ShouldIgnoreImport(request.Title))
      .ToList();
  }

  private static List<CreateTransactionRequest> ParseExtratoTransactionRequests(byte[] data)
  {
    return ParseCsv<NubankExtratoFile>(data)
      .Select(MapTransactionRequest)
      .Where(request => !ShouldIgnoreImport(request.Title))
      .ToList();
  }

  private static List<ModelFile> ParseCsv<ModelFile>(byte[] data)
  {
    using var stream = new MemoryStream(data);
    using var reader = new StreamReader(stream, Encoding.UTF8);
    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

    return csv.GetRecords<ModelFile>().ToList();
  }

  private static CreateTransactionRequest MapTransactionRequest(CreditCardNubankFile record)
  {
    return new CreateTransactionRequest
    {
      Date = DateTime.ParseExact(record.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture),
      Title = record.Title,
      Amount = ParseAmount(record.Amount)
    };
  }

  private static CreateTransactionRequest MapTransactionRequest(NubankExtratoFile record)
  {
    return new CreateTransactionRequest
    {
      Date = DateTime.ParseExact(record.Data, "dd/MM/yyyy", CultureInfo.InvariantCulture),
      Title = record.Descricao,
      Amount = ParseAmount(record.Valor)
    };
  }

  private static decimal ParseAmount(string amount)
  {
    var normalized = amount.Trim();

    if (normalized.StartsWith("-", StringComparison.Ordinal))
    {
      normalized = $"-{normalized[1..].TrimStart()}";
    }

    var culture = normalized.Contains(',')
      ? new CultureInfo("pt-BR")
      : CultureInfo.InvariantCulture;

    return decimal.Parse(normalized, NumberStyles.Number, culture);
  }

  private static bool ShouldIgnoreImport(string title)
  {
    return IgnoredImportTitleTexts.Any(ignoredText =>
      title.Contains(ignoredText, StringComparison.OrdinalIgnoreCase));
  }
}
