using api.Models.Person;

namespace api.Responses.Person;

public class ListPersonResponse
{
  public required List<PersonModel> Persons { get; set; }
  public required int Page { get; set; }
  public required int Limit { get; set; }
  public required int Total { get; set; }
  public required int TotalPages { get; set; }
}
