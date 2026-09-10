using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models.Category;

[Table("categories")]
public class CategoryModel
{
  [Column("id")]
  public required Guid Id { get; set; }

  [Column("title")]
  public required string Title { get; set; }

  [Column("description")]
  public string? Description { get; set; }

  [Column("created_at")]
  public required DateTime Created_at { get; set; }

  [Column("updated_at")]
  public DateTime? Updated_at { get; set; }

  [Column("deleted_at")]
  public DateTime? Deleted_at { get; set; }
}
