using api.Models.Category;

namespace api.Responses.Category;

public class ListCategoryResponse
{
  public required List<CategoryModel> Categories { get; set; }
  public required int Page { get; set; }
  public required int Limit { get; set; }
  public required int Total { get; set; }
  public required int TotalPages { get; set; }
}
