using api.Models.Category;
using api.Models.TransactionCategory;

namespace api.Responses.TransactionCategory;

public class SetTransactionCategoriesResponse
{
  public required List<TransactionCategoryModel> TransactionCategories { get; set; }
  public required List<CategoryModel> Categories { get; set; }
}
