using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace api.Normalization;

public static class RequestNormalizationMvcBuilderExtensions
{
  public static IMvcBuilder AddRequestNormalization(this IMvcBuilder mvcBuilder)
  {
    var services = mvcBuilder.Services;
    var normalizerTypes = typeof(RequestNormalizationMvcBuilderExtensions).Assembly
      .GetTypes()
      .Where(type =>
        type is { IsAbstract: false, IsInterface: false } &&
        typeof(IRequestNormalizer).IsAssignableFrom(type)
      )
      .OrderBy(type => type.FullName);

    foreach (var normalizerType in normalizerTypes)
    {
      services.TryAddEnumerable(
        ServiceDescriptor.Singleton(typeof(IRequestNormalizer), normalizerType)
      );
    }

    services.TryAddSingleton<RequestNormalizerDispatcher>();
    services.TryAddScoped<NormalizeAndValidateRequestFilter>();

    mvcBuilder.AddMvcOptions(options =>
      options.Filters.AddService<NormalizeAndValidateRequestFilter>(
        NormalizeAndValidateRequestFilter.FilterOrder
      )
    );

    return mvcBuilder;
  }
}
