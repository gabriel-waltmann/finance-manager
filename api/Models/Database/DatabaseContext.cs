using api.Models.File;
using api.Models.FileProcessing;
using api.Models.Transaction;
using api.Models.TransactionImport;
using Microsoft.EntityFrameworkCore;

namespace api.Models.Database;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
  public DbSet<TransactionModel> Transactions { get; set; } = null!;

  public DbSet<TransactionImportModel> TransactionsImport { get; set; } = null!;

  public DbSet<FileModel> Files { get; set; } = null!;

  public DbSet<FileProcessingModel> FilesProcessing { get; set; } = null!;
}