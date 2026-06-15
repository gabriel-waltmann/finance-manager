using api.Models.Transaction;
using Microsoft.EntityFrameworkCore;

namespace api.Models.Database;

public class DatabaseContext : DbContext
{
  public DatabaseContext(DbContextOptions<DatabaseContext> options)
    : base(options) { }

  public DbSet<TransactionModel> Transactions { get; set; } = null!;
}