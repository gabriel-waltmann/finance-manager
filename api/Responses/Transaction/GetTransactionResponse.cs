using api.Models.Category;
using api.Models.Person;
using api.Models.Transaction;
using api.Models.TransactionCategory;
using api.Models.TransactionPerson;

namespace api.Responses.Transaction;

public class GetTransactionResponse
{
  public required TransactionModel Transaction { get; set; }
  public TransactionPersonModel? TransactionPerson { get; set; }
  public PersonModel? Person { get; set; }
  public required List<TransactionCategoryModel> TransactionCategories { get; set; }
  public required List<CategoryModel> Categories { get; set; }
}
