using api.Models.Database;
using api.Requests.Dashboard;
using api.Responses.Dashboard;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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

    var fixedSpendRows = await query
      .Select(transaction => new
      {
        transaction.Title,
        NormalizedTitle = transaction.Title.ToLower(),
        transaction.Date,
        transaction.Created_at,
        transaction.Amount
      })
      .ToListAsync();

    var fixedSpendsQuery = fixedSpendRows
      .GroupBy(transaction => transaction.NormalizedTitle)
      .Select(group =>
      {
        var latestTransaction = group
          .OrderByDescending(transaction => transaction.Date)
          .ThenByDescending(transaction => transaction.Created_at)
          .First();

        return new DashboardFixedSpendResponse
        {
          Title = latestTransaction.Title,
          MonthCount = group
            .Select(transaction => new { transaction.Date.Year, transaction.Date.Month })
            .Distinct()
            .Count(),
          LastMonth = latestTransaction.Date.ToString("yyyy-MM", CultureInfo.InvariantCulture),
          LastAmount = -latestTransaction.Amount
        };
      })
      .Where(item => item.MonthCount >= 2);

    var fixedSpends = request.Order == "asc"
      ? fixedSpendsQuery
        .OrderBy(item => item.LastAmount)
        .ThenBy(item => item.Title)
        .ToList()
      : fixedSpendsQuery
        .OrderByDescending(item => item.LastAmount)
        .ThenBy(item => item.Title)
        .ToList();

    return new GetDashboardResponse
    {
      TopItems = topItems,
      FixedSpends = fixedSpends,
      TotalAmount = totalAmount,
      Page = request.Page,
      Limit = request.Limit,
      Total = total,
      TotalPages = totalPages
    };
  }
}
