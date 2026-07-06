using api.Models.Person;

namespace api.Responses.Person;

public class ListPersonResponse
{
  public required List<PersonModel> Persons { get; set; }
}
