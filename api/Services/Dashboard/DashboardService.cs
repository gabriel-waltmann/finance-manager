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
        transaction.Amount < 0
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

    var totalAmount = await query
      .SumAsync(transaction => (decimal?)-transaction.Amount) ?? 0;

    var topItemsQuery = query
      .GroupBy(transaction => transaction.Title.ToLower())
      .Select(group => new DashboardTopItemResponse
      {
        Title = group.Min(transaction => transaction.Title)!,
        TotalAmount = group.Sum(transaction => -transaction.Amount),
        TransactionCount = group.Count()
      });

    var total = await topItemsQuery.CountAsync();
    var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)request.Limit);

    var orderedTopItemsQuery = request.Order == "asc"
      ? topItemsQuery
        .OrderBy(item => item.TotalAmount)
        .ThenBy(item => item.Title)
      : topItemsQuery
        .OrderByDescending(item => item.TotalAmount)
        .ThenBy(item => item.Title);

    var topItems = await orderedTopItemsQuery
      .Skip((request.Page - 1) * request.Limit)
      .Take(request.Limit)
      .ToListAsync();

    return new GetDashboardResponse
    {
      TopItems = topItems,
      TotalAmount = totalAmount,
      Page = request.Page,
      Limit = request.Limit,
      Total = total,
      TotalPages = totalPages
    };
  }
}
