using api.Models.Category;
using api.Models.File;
using api.Models.FileProcessing;
using api.Models.Person;
using api.Models.Transaction;
using api.Models.TransactionCategory;
using api.Models.TransactionImport;
using api.Models.TransactionPerson;
using Microsoft.EntityFrameworkCore;

namespace api.Models.Database;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
  public DbSet<CategoryModel> Categories { get; set; } = null!;

  public DbSet<PersonModel> Persons { get; set; } = null!;

  public DbSet<TransactionModel> Transactions { get; set; } = null!;

  public DbSet<TransactionPersonModel> TransactionsPerson { get; set; } = null!;

  public DbSet<TransactionCategoryModel> TransactionsCategory { get; set; } = null!;

  public DbSet<TransactionImportModel> TransactionsImport { get; set; } = null!;

  public DbSet<FileModel> Files { get; set; } = null!;

  public DbSet<FileProcessingModel> FilesProcessing { get; set; } = null!;

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<CategoryModel>()
      .HasIndex(category => category.Title)
      .IsUnique()
      .HasFilter("deleted_at IS NULL");

    modelBuilder.Entity<TransactionCategoryModel>()
      .HasIndex(link => new { link.TransactionId, link.CategoryId })
      .IsUnique()
      .HasFilter("deleted_at IS NULL");

    modelBuilder.Entity<TransactionCategoryModel>()
      .HasIndex(link => link.CategoryId);

    modelBuilder.Entity<TransactionCategoryModel>()
      .HasOne<CategoryModel>()
      .WithMany()
      .HasForeignKey(link => link.CategoryId)
      .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<TransactionCategoryModel>()
      .HasOne<TransactionModel>()
      .WithMany()
      .HasForeignKey(link => link.TransactionId)
      .OnDelete(DeleteBehavior.Restrict);

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
