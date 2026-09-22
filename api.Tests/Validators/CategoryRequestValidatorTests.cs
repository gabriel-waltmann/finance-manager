using api.Normalization.Category;
using api.Requests.Category;
using api.Validators.Category;
using FluentValidation.TestHelper;

namespace api.Tests.Validators;

public class CategoryRequestValidatorTests
{
  [Fact]
  public void Create_rejects_blank_title_and_overlong_description()
  {
    var request = new CreateCategoryRequest
    {
      Title = " ",
      Description = new string('d', 501)
    };

    var result = new CreateCategoryRequestValidator().TestValidate(request);

    result.ShouldHaveValidationErrorFor(item => item.Title);
    result.ShouldHaveValidationErrorFor(item => item.Description);
  }

  [Fact]
  public void Update_accepts_maximum_lengths_and_null_description()
  {
    var maximumResult = new UpdateCategoryRequestValidator().TestValidate(
      new UpdateCategoryRequest
      {
        Title = new string('t', 120),
        Description = new string('d', 500)
      }
    );
    var nullResult = new UpdateCategoryRequestValidator().TestValidate(
      new UpdateCategoryRequest
      {
        Title = "Housing",
        Description = null
      }
    );

    maximumResult.ShouldNotHaveAnyValidationErrors();
    nullResult.ShouldNotHaveAnyValidationErrors();
  }

  [Fact]
  public void List_rejects_invalid_unpaired_paging_search_and_order()
  {
    var request = new ListCategoryRequest
    {
      Page = 0,
      Limit = 101,
      Search = new string('s', 201),
      Order = "newest"
    };
    var unpairedRequest = new ListCategoryRequest { Page = 1 };

    var result = new ListCategoryRequestValidator().TestValidate(request);
    var unpairedResult = new ListCategoryRequestValidator().TestValidate(unpairedRequest);

    result.ShouldHaveValidationErrorFor(item => item.Page);
    result.ShouldHaveValidationErrorFor(item => item.Limit);
    result.ShouldHaveValidationErrorFor(item => item.Search);
    result.ShouldHaveValidationErrorFor(item => item.Order);
    Assert.Contains(unpairedResult.Errors, error => error.ErrorMessage.Contains("used together"));
  }

  [Fact]
  public void List_accepts_unpaged_and_trimmed_case_insensitive_order()
  {
    var unpagedRequest = new ListCategoryRequest();
    var pagedRequest = new ListCategoryRequest
    {
      Page = 1,
      Limit = 100,
      Order = " DESC "
    };
    var normalizer = new ListCategoryRequestNormalizer();
    normalizer.Normalize(unpagedRequest);
    normalizer.Normalize(pagedRequest);
    var unpagedResult = new ListCategoryRequestValidator().TestValidate(unpagedRequest);
    var pagedResult = new ListCategoryRequestValidator().TestValidate(pagedRequest);

    unpagedResult.ShouldNotHaveAnyValidationErrors();
    pagedResult.ShouldNotHaveAnyValidationErrors();
  }

  [Fact]
  public void Description_allows_multiline_text_but_rejects_other_controls()
  {
    var multiline = new CreateCategoryRequest
    {
      Title = "<Housing>",
      Description = "  First line\n\tSecond line  "
    };
    var unsafeRequest = new CreateCategoryRequest
    {
      Title = "Housing",
      Description = "Unsafe\0description"
    };

    var normalizer = new CreateCategoryRequestNormalizer();
    normalizer.Normalize(multiline);
    normalizer.Normalize(unsafeRequest);

    var multilineResult = new CreateCategoryRequestValidator().TestValidate(multiline);
    var unsafeResult = new CreateCategoryRequestValidator().TestValidate(unsafeRequest);

    multilineResult.ShouldNotHaveAnyValidationErrors();
    Assert.Equal("First line\n\tSecond line", multiline.Description);
    unsafeResult.ShouldHaveValidationErrorFor(item => item.Description);
  }
}
