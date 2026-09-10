using api.Exceptions;
using api.Models.Category;
using api.Models.Database;
using api.Requests.Category;
using api.Responses.Category;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Category;

public class CategoryService(DatabaseContext context)
{
  private readonly DatabaseContext _context = context;

  public async Task<CategoryModel> Get(Guid id)
  {
    return await _context.Categories.FirstOrDefaultAsync(category =>
      category.Id == id &&
      category.Deleted_at == null
    ) ?? throw new NotFoundCategoryException();
  }

  public async Task<ListCategoryResponse> List(ListCategoryRequest request)
  {
    var query = _context.Categories
      .Where(category => request.WithDeleted || category.Deleted_at == null);

    if (!string.IsNullOrWhiteSpace(request.Search))
    {
      var searchPattern = $"%{EscapeLikePattern(request.Search)}%";

      query = query.Where(category =>
        EF.Functions.ILike(category.Title, searchPattern, "\\") ||
        (category.Description != null &&
          EF.Functions.ILike(category.Description, searchPattern, "\\"))
      );
    }

    var total = await query.CountAsync();
    var orderedQuery = request.Order == "desc"
      ? query.OrderByDescending(category => category.Title).ThenByDescending(category => category.Id)
      : query.OrderBy(category => category.Title).ThenBy(category => category.Id);

    if (request.Page.HasValue && request.Limit.HasValue)
    {
      var page = request.Page.Value;
      var limit = request.Limit.Value;
      var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)limit);

      return new ListCategoryResponse
      {
        Categories = await orderedQuery.Skip((page - 1) * limit).Take(limit).ToListAsync(),
        Page = page,
        Limit = limit,
        Total = total,
        TotalPages = totalPages
      };
    }

    return new ListCategoryResponse
    {
      Categories = await orderedQuery.ToListAsync(),
      Page = 1,
      Limit = total,
      Total = total,
      TotalPages = total == 0 ? 0 : 1
    };
  }

  public async Task<CategoryModel> Create(CreateCategoryRequest request)
  {
    var title = request.Title.Trim();

    if (await ActiveTitleExists(title))
    {
      throw new ExistsCategoryException();
    }

    var category = new CategoryModel
    {
      Id = Guid.NewGuid(),
      Title = title,
      Description = NormalizeDescription(request.Description),
      Created_at = DateTime.UtcNow
    };

    _context.Categories.Add(category);
    await _context.SaveChangesAsync();

    return category;
  }

  public async Task Update(Guid id, UpdateCategoryRequest request)
  {
    var category = await Get(id);
    var title = request.Title.Trim();

    if (await ActiveTitleExists(title, id))
    {
      throw new ExistsCategoryException();
    }

    category.Title = title;
    category.Description = NormalizeDescription(request.Description);
    category.Updated_at = DateTime.UtcNow;

    await _context.SaveChangesAsync();
  }

  public async Task Delete(Guid id)
  {
    var category = await _context.Categories.FirstOrDefaultAsync(category =>
      category.Id == id &&
      category.Deleted_at == null
    );

    if (category == null)
    {
      return;
    }

    category.Deleted_at = DateTime.UtcNow;
    await _context.SaveChangesAsync();
  }

  private Task<bool> ActiveTitleExists(string title, Guid? currentId = null)
  {
    return _context.Categories.AnyAsync(category =>
      category.Deleted_at == null &&
      category.Title == title &&
      (!currentId.HasValue || category.Id != currentId.Value)
    );
  }

  private static string? NormalizeDescription(string? description)
  {
    return string.IsNullOrWhiteSpace(description) ? null : description.Trim();
  }

  private static string EscapeLikePattern(string value)
  {
    return value
      .Replace("\\", "\\\\", StringComparison.Ordinal)
      .Replace("%", "\\%", StringComparison.Ordinal)
      .Replace("_", "\\_", StringComparison.Ordinal);
  }
}
