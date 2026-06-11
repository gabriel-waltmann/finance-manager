using CsvHelper.Configuration.Attributes;

namespace api.Models.Files;

public class CreditCardNubankFile
{
    [Name("date")]
    public required string Date { get; set; }
    
    [Name("title")]
    public required string Title { get; set; }
    
    [Name("amount")]
    public decimal Amount { get; set; }

}