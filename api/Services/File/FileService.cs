using api.Exceptions;
using api.Models.Database;
using api.Models.File;

namespace api.Services.File;

public class FileService(DatabaseContext context)
{
  private readonly DatabaseContext _context = context;

  public async Task<FileModel> CreateFromFormFileAsync(IFormFile file)
  {
    await using var stream = file.OpenReadStream();
    using var memoryStream = new MemoryStream();

    await stream.CopyToAsync(memoryStream);

    var model = new FileModel
    {
      Id = Guid.NewGuid(),
      Name = file.FileName,
      Data = memoryStream.ToArray(),
      Created_at = DateTime.UtcNow
    };

    _context.Files.Add(model);

    await _context.SaveChangesAsync();

    return model;
  }

  public async Task<FileModel> Get(Guid id)
  {
    return await _context.Files.FindAsync(id) ?? throw new NotFoundFileException();
  }
}
