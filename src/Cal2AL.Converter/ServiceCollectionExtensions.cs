using Cal2AL.Converter.Engine;
using Cal2AL.Converter.Objects;
using Microsoft.Extensions.DependencyInjection;

namespace Cal2AL.Converter;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers the object converters and the conversion engine.</summary>
    public static IServiceCollection AddCal2AL(this IServiceCollection services)
    {
        services.AddSingleton<IObjectConverter, TableConverter>();
        services.AddSingleton<IObjectConverter, PageConverter>();
        services.AddSingleton<IObjectConverter, ReportConverter>();
        services.AddSingleton<IObjectConverter, CodeunitConverter>();
        services.AddSingleton<IObjectConverter, XmlPortConverter>();
        services.AddSingleton<IObjectConverter, QueryConverter>();
        services.AddSingleton<IObjectConverter, ModificationConverter>();
        services.AddSingleton<IConversionEngine, ConversionEngine>();
        return services;
    }

    /// <summary>An engine with all converters, for callers that do not use dependency injection.</summary>
    public static IConversionEngine CreateDefaultEngine() =>
        new ConversionEngine(
        [
            new TableConverter(), new PageConverter(), new ReportConverter(), new CodeunitConverter(),
            new XmlPortConverter(), new QueryConverter(), new ModificationConverter(),
        ]);
}
