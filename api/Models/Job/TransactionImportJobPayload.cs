namespace api.Models.Job;

public class TransactionImportJobPayload
{
  public required Guid JobId { get; set; }
  public required Guid FileId { get; set; }
  public required Guid FileProcessingId { get; set; }
  public required DateTime CreatedAt { get; set; }
  public Guid? PersonId { get; set; }
  public List<Guid> CategoryIds { get; set; } = [];
}
