using System.Globalization;
using api.Models.Files;
using api.Requests.Transaction;

namespace api.Mappers.Transaction;

public class UploadTransactionMapper
{
    public CreateTransactionRequest? MapCreateRequest(CreditCardNubankFile item)
    {
      try
      {
        var title = item.Title;

        var cultureInfo = new CultureInfo("yyyy-MM-dd");
        DateTime date = DateTime.Parse(item.Date, cultureInfo);
        var amount = decimal.Parse(item.Amount);

        return new CreateTransactionRequest
        {
            Title = title,
            Date = date,
            Amount = amount
        };
      }
      catch (Exception ex)
      {
          Console.WriteLine(ex.Message, ex.GetType());
          return null;
      }
    }
}