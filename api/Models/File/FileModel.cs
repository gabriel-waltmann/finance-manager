using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models.File;

[Table("files")]
public class FileModel
{
  [Column("id")]
  public required Guid Id { get; set; }

  [Column("name")]
  public required string Name { get; set; }

  [Column("data")]
  public required byte[] Data { get; set; }

  [Column("created_at")]
  public required DateTime Created_at { get; set; }

  [Column("updated_at")]
  public DateTime? Updated_at { get; set; }

  [Column("deleted_at")]
  public DateTime? Deleted_at { get; set; }
}
