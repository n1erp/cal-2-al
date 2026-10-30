namespace Microsoft.CodeAnalysis.CSharp;

public sealed class CSharpCommandLineArguments : CommandLineArguments
{
	public new CSharpCompilationOptions CompilationOptions { get; internal set; }

	public new CSharpParseOptions ParseOptions { get; internal set; }

	protected override ParseOptions ParseOptionsCore => ParseOptions;

	protected override CompilationOptions CompilationOptionsCore => CompilationOptions;

	internal bool ShouldIncludeErrorEndLocation { get; set; }

	internal CSharpCommandLineArguments()
	{
	}
}
