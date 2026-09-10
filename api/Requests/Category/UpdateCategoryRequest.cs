namespace api.Requests.Category;

public class UpdateCategoryRequest
{
  public required string Title { get; set; }
  public string? Description { get; set; }
}
