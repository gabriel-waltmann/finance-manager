using api.Models.Database;
using api.Requests.Dashboard;
using api.Responses.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Dashboard;

public class DashboardService(DatabaseContext context)
{
  private readonly DatabaseContext _context = context;

  public async Task<GetDashboardResponse> Get(GetDashboardRequest request)
  {
    var query = _context.Transactions
      .Where(transaction =>
        transaction.Deleted_at == null &&
        transaction.Amount > 0
      );

    if (request.StartDate.HasValue)
    {
      var startDate = request.StartDate.Value.Date;

      query = query.Where(transaction => transaction.Date >= startDate);
    }

    if (request.EndDate.HasValue)
    {
      var nextEndDate = request.EndDate.Value.Date.AddDays(1);

      query = query.Where(transaction => transaction.Date < nextEndDate);
    }

    if (request.PersonId.HasValue)
    {
      var personId = request.PersonId.Value;

      query =
        from transaction in query
        join transactionPerson in _context.TransactionsPerson
          on transaction.Id equals transactionPerson.TransactionId
        where
          transactionPerson.Deleted_at == null &&
          transactionPerson.PersonId == personId
        select transaction;
    }

    var topItems = await query
      .GroupBy(transaction => transaction.Title)
      .Select(group => new DashboardTopItemResponse
      {
        Title = group.Key,
        TotalAmount = group.Sum(transaction => transaction.Amount),
        TransactionCount = group.Count()
      })
      .OrderByDescending(item => item.TotalAmount)
      .ThenBy(item => item.Title)
      .Take(10)
      .ToListAsync();

    if (request.Order == "asc")
    {
      topItems = topItems
        .OrderBy(item => item.TotalAmount)
        .ThenBy(item => item.Title)
        .ToList();
    }

    return new GetDashboardResponse
    {
      TopItems = topItems
    };
  }
}
