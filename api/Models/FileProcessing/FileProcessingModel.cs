using System.ComponentModel.DataAnnotations.Schema;
using api.Models.FileProcessingStatus;

namespace api.Models.FileProcessing;

[Table("files_processing")]
public class FileProcessingModel
{
  [Column("id")]
  public required Guid Id { get; set; }

  [Column("file_id")]
  public required Guid FileId { get; set; }

  [Column("job_id")]
  public required Guid JobId { get; set; }

  [Column("status")]
  public required FileProcessingStatusName Status { get; set; } 
  
  [Column("created_at")]
  public required DateTime Created_at { get; set; }

  [Column("updated_at")]
  public DateTime? Updated_at { get; set; }

  [Column("deleted_at")]
  public DateTime? Deleted_at { get; set; }
}