using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models.TransactionPerson;

[Table("transactions_person")]
public class TransactionPersonModel
{
  [Column("id")]
  public required Guid Id { get; set; }

  [Column("person_id")]
  public required Guid PersonId { get; set; }

  [Column("transaction_id")]
  public required Guid TransactionId { get; set; }

  [Column("created_at")]
  public required DateTime Created_at { get; set; }

  [Column("updated_at")]
  public DateTime? Updated_at { get; set; }

  [Column("deleted_at")]
  public DateTime? Deleted_at { get; set; }
}
