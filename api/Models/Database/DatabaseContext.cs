using api.Models.File;
using api.Models.FileProcessing;
using api.Models.Person;
using api.Models.Transaction;
using api.Models.TransactionImport;
using api.Models.TransactionPerson;
using Microsoft.EntityFrameworkCore;

namespace api.Models.Database;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
  public DbSet<PersonModel> Persons { get; set; } = null!;

  public DbSet<TransactionModel> Transactions { get; set; } = null!;

  public DbSet<TransactionPersonModel> TransactionsPerson { get; set; } = null!;

  public DbSet<TransactionImportModel> TransactionsImport { get; set; } = null!;

  public DbSet<FileModel> Files { get; set; } = null!;

  public DbSet<FileProcessingModel> FilesProcessing { get; set; } = null!;

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<PersonModel>()
      .HasIndex(person => person.Email)
      .IsUnique()
      .HasFilter("deleted_at IS NULL");

    modelBuilder.Entity<TransactionPersonModel>()
      .HasIndex(transactionPerson => transactionPerson.TransactionId)
      .IsUnique()
      .HasFilter("deleted_at IS NULL");

    modelBuilder.Entity<TransactionPersonModel>()
      .HasOne<PersonModel>()
      .WithMany()
      .HasForeignKey(transactionPerson => transactionPerson.PersonId)
      .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<TransactionPersonModel>()
      .HasOne<TransactionModel>()
      .WithMany()
      .HasForeignKey(transactionPerson => transactionPerson.TransactionId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}
