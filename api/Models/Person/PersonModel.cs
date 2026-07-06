using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models.Person;

[Table("persons")]
public class PersonModel
{
  [Column("id")]
  public required Guid Id { get; set; }

  [Column("name")]
  public required string Name { get; set; }

  [Column("email")]
  public required string Email { get; set; }

  [Column("phone_number")]
  public required string PhoneNumber { get; set; }

  [Column("created_at")]
  public required DateTime Created_at { get; set; }

  [Column("updated_at")]
  public DateTime? Updated_at { get; set; }

  [Column("deleted_at")]
  public DateTime? Deleted_at { get; set; }
}
