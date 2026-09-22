using api.Models.Database;
using api.Models.FileCategory;
using api.Services.File;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace api.Tests.Services;

public class FileServiceTests
{
  [Fact]
  public async Task Create_stores_only_the_normalized_file_basename()
  {
    await using var context = CreateContext();
    var service = new FileService(context);
    var data = new byte[] { 1, 2, 3 };
    var file = new FormFile(
      new MemoryStream(data),
      0,
      data.Length,
      "File",
      " C:\\fakepath\\Cafe\u0301.csv "
    );

    var stored = await service.CreateFromFormFileAsync(file, FileCategoryName.Extrato);

    Assert.Equal("Café.csv", stored.Name);
    Assert.Equal(data, stored.Data);
  }

  private static DatabaseContext CreateContext()
  {
    var options = new DbContextOptionsBuilder<DatabaseContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString())
      .Options;

    return new DatabaseContext(options);
  }
}
