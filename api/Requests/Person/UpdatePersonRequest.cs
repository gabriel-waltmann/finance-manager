namespace api.Requests.Person;

public class UpdatePersonRequest
{
  public required string Name { get; set; }
  public required string Email { get; set; }
  public required string PhoneNumber { get; set; }
}
