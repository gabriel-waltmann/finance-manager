using api.Exceptions;
using api.Models.Database;
using api.Models.FileProcessing;
using api.Models.FileProcessingStatus;

namespace api.Services.FileProcessing;

public class FileProcessingService(DatabaseContext context)
{
  private readonly DatabaseContext _context = context;

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

  private async Task UpdateStatus(Guid id, FileProcessingStatusName status)
  {
    var model = await Get(id);

    model.Status = status;
    model.Updated_at = DateTime.UtcNow;

    await _context.SaveChangesAsync();
  }
}
