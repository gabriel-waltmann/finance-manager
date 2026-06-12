using api.Requests;

namespace api.Services;

public class UploadTransactionService
{
  public async Task ExecuteAsync(UpladTransactionRequest request)
  {
    Console.WriteLine(request.File);
  }
}