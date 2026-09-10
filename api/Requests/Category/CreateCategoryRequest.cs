namespace api.Requests.Category;

public class CreateCategoryRequest
{
  public required string Title { get; set; }
  public string? Description { get; set; }
}
