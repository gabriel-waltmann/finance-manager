using Microsoft.EntityFrameworkCore;

namespace api.Models;

public class DatabaseContext : DbContext
{
  public DatabaseContext(DbContextOptions<DatabaseContext> options)
    : base(options) { }

  public DbSet<Transaction> Transactions { get; set; } = null!;
}