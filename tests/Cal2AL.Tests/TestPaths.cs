using System.Runtime.CompilerServices;
using Cal2AL.Converter;
using Cal2AL.Converter.Engine;

namespace Cal2AL.Tests;

internal static class TestPaths
{
    public static string Fixtures => Path.Combine(AppContext.BaseDirectory, "Fixtures");

    public static string Fixture(params string[] parts) => Path.Combine([Fixtures, .. parts]);

    /// <summary>The test project folder in the source tree (approved snapshots live in its Expected folder).</summary>
    public static string SourceTestsFolder() => Path.GetDirectoryName(ThisFile())!;

    private static string ThisFile([CallerFilePath] string path = "") => path;
}

internal static class TestEngine
{
    public static (ConversionRun Run, InMemoryOutputSink Sink) Convert(string source, ConversionOptions? options = null)
    {
        var sink = new InMemoryOutputSink();
        var run = ServiceCollectionExtensions.CreateDefaultEngine().Run(
            new ConversionRequest { Source = source, Target = "(memory)", Mode = RunMode.Convert, Options = options ?? new ConversionOptions() },
            sink);
        return (run, sink);
    }
}
