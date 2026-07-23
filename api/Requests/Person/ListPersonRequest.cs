namespace api.Requests.Person;

public class ListPersonRequest
{
  public string? Search { get; set; }
  public string Order { get; set; } = "asc";
  public int? Page { get; set; }
  public int? Limit { get; set; }
  public bool WithDeleted { get; set; }
}
