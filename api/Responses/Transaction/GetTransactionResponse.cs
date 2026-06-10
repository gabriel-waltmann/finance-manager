using api.Models;

namespace api.Responses;

public class GetTransactionResponse
{
  public required Transaction Transaction { get; set; }
}