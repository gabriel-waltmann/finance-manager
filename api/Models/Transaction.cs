using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

[Table("transactions")]
public class Transaction
{
  [Column("id")]
  public required Guid Id { get; set; }
  
  [Column("date")]
  public required DateTime Date { get; set; }

  [Column("title")]
  public required string Title { get; set; }

  [Column("amount")]
  public required decimal Amount { get; set; }

  [Column("created_at")]
  public required DateTime Created_at { get; set; }

  [Column("updated_at")]
  public DateTime? Updated_at { get; set; }

  [Column("deleted_at")]
  public DateTime? Deleted_at { get; set; }
}
