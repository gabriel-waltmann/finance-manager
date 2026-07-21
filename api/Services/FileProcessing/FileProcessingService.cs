using api.Exceptions;
using api.Models.Database;
using api.Models.FileProcessing;
using api.Models.FileProcessingStatus;
using api.Requests.Transaction;
using api.Responses.Transaction;
using api.Services.TransactionImport;
using Microsoft.EntityFrameworkCore;

namespace api.Services.FileProcessing;

public class FileProcessingService(
  DatabaseContext context,
  TransactionImportEventBroadcaster eventBroadcaster
)
{
  private readonly DatabaseContext _context = context;
  private readonly TransactionImportEventBroadcaster _eventBroadcaster = eventBroadcaster;

  public async Task<FileProcessingModel> CreateSubmitted(Guid fileId, Guid jobId)
  {
    var model = new FileProcessingModel
    {
      Id = Guid.NewGuid(),
      FileId = fileId,
      JobId = jobId,
      Status = FileProcessingStatusName.Submitted,
      Created_at = DateTime.UtcNow
    };

    _context.FilesProcessing.Add(model);

    await _context.SaveChangesAsync();

    _eventBroadcaster.Publish(await GetResponse(model.Id));

    return model;
  }

  public async Task<FileProcessingModel> Get(Guid id)
  {
    return await _context.FilesProcessing.FindAsync(id) ?? throw new NotFoundFileProcessingException();
  }

  public async Task MarkProcessing(Guid id)
  {
    await UpdateStatus(id, FileProcessingStatusName.Processing);
  }

  public async Task MarkFinished(Guid id)
  {
    await UpdateStatus(id, FileProcessingStatusName.Finished);
  }

  public async Task MarkFailed(Guid id)
  {
    await UpdateStatus(id, FileProcessingStatusName.Failed);
  }

  public async Task<TransactionImportResponse> GetResponse(Guid id)
  {
    var response = await BuildResponseQuery()
      .FirstOrDefaultAsync(item => item.Id == id)
      ?? throw new NotFoundFileProcessingException();

    return MapResponse(response);
  }

  public async Task<ListTransactionImportResponse> List(ListTransactionImportRequest request)
  {
    var query = BuildResponseQuery();

    if (!string.IsNullOrWhiteSpace(request.Search))
    {
      var searchPattern = $"%{EscapeLikePattern(request.Search)}%";
      query = query.Where(item => EF.Functions.ILike(item.FileName, searchPattern, "\\"));
    }

    if (!string.IsNullOrWhiteSpace(request.Status))
    {
      var status = Enum.Parse<FileProcessingStatusName>(request.Status, ignoreCase: true);
      query = query.Where(item => item.Status == status);
    }

    var total = await query.CountAsync();
    var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)request.Limit);
    var orderedQuery = request.Order == "asc"
      ? query.OrderBy(item => item.CreatedAt).ThenBy(item => item.Id)
      : query.OrderByDescending(item => item.CreatedAt).ThenByDescending(item => item.Id);
    var rows = await orderedQuery
      .Skip((request.Page - 1) * request.Limit)
      .Take(request.Limit)
      .ToListAsync();

    return new ListTransactionImportResponse
    {
      Imports = rows.Select(MapResponse).ToList(),
      Page = request.Page,
      Limit = request.Limit,
      Total = total,
      TotalPages = totalPages
    };
  }

  private async Task UpdateStatus(Guid id, FileProcessingStatusName status)
  {
    var model = await Get(id);

    model.Status = status;
    model.Updated_at = DateTime.UtcNow;

    await _context.SaveChangesAsync();

    _eventBroadcaster.Publish(await GetResponse(id));
  }

  private IQueryable<TransactionImportQueryRow> BuildResponseQuery()
  {
    return
      from processing in _context.FilesProcessing.AsNoTracking()
      join file in _context.Files.AsNoTracking() on processing.FileId equals file.Id
      where processing.Deleted_at == null && file.Deleted_at == null
      select new TransactionImportQueryRow
      {
        Id = processing.Id,
        FileName = file.Name,
        Category = file.Category,
        Status = processing.Status,
        TransactionCount = _context.TransactionsImport.Count(transactionImport =>
          transactionImport.FileProcessingId == processing.Id &&
          transactionImport.Deleted_at == null
        ),
        CreatedAt = processing.Created_at,
        UpdatedAt = processing.Updated_at
      };
  }

  private static TransactionImportResponse MapResponse(TransactionImportQueryRow row)
  {
    return new TransactionImportResponse
    {
      Id = row.Id,
      FileName = row.FileName,
      Category = row.Category.ToString(),
      Status = row.Status.ToString(),
      TransactionCount = row.TransactionCount,
      CreatedAt = DateTime.SpecifyKind(row.CreatedAt, DateTimeKind.Utc),
      UpdatedAt = row.UpdatedAt.HasValue
        ? DateTime.SpecifyKind(row.UpdatedAt.Value, DateTimeKind.Utc)
        : null
    };
  }

  private static string EscapeLikePattern(string value)
  {
    return value
      .Replace("\\", "\\\\", StringComparison.Ordinal)
      .Replace("%", "\\%", StringComparison.Ordinal)
      .Replace("_", "\\_", StringComparison.Ordinal);
  }

  private sealed class TransactionImportQueryRow
  {
    public required Guid Id { get; set; }
    public required string FileName { get; set; }
    public required api.Models.FileCategory.FileCategoryName Category { get; set; }
    public required FileProcessingStatusName Status { get; set; }
    public required int TransactionCount { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
  }
}
