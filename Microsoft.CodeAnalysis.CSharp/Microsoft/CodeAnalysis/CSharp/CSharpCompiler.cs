using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class CSharpCompiler : CommonCompiler
{
	internal const string ResponseFileName = "csc.rsp";

	private readonly CommandLineDiagnosticFormatter _diagnosticFormatter;

	private readonly string _tempDirectory;

	public override DiagnosticFormatter DiagnosticFormatter => _diagnosticFormatter;

	protected internal new CSharpCommandLineArguments Arguments => (CSharpCommandLineArguments)base.Arguments;

	internal override Type Type => typeof(CSharpCompiler);

	protected CSharpCompiler(CSharpCommandLineParser parser, string responseFile, string[] args, BuildPaths buildPaths, string additionalReferenceDirectories, IAnalyzerAssemblyLoader assemblyLoader)
		: base(parser, responseFile, args, buildPaths, additionalReferenceDirectories, assemblyLoader)
	{
		_diagnosticFormatter = new CommandLineDiagnosticFormatter(buildPaths.WorkingDirectory, Arguments.PrintFullPaths, Arguments.ShouldIncludeErrorEndLocation);
		_tempDirectory = buildPaths.TempDirectory;
	}

	public override Compilation CreateCompilation(TextWriter consoleOutput, TouchedFileLogger touchedFilesLogger, ErrorLogger errorLogger)
	{
		CSharpParseOptions parseOptions = Arguments.ParseOptions;
		CSharpParseOptions scriptParseOptions = parseOptions.WithKind(SourceCodeKind.Script);
		bool hadErrors = false;
		ImmutableArray<CommandLineSourceFile> sourceFiles = Arguments.SourceFiles;
		SyntaxTree[] trees = new SyntaxTree[sourceFiles.Length];
		string[] normalizedFilePaths = new string[sourceFiles.Length];
		DiagnosticBag diagnosticBag = DiagnosticBag.GetInstance();
		if (Arguments.CompilationOptions.ConcurrentBuild)
		{
			Parallel.For(0, sourceFiles.Length, UICultureUtilities.WithCurrentUICulture((int i) =>
			{
				trees[i] = ParseFile(parseOptions, scriptParseOptions, ref hadErrors, sourceFiles[i], diagnosticBag, out normalizedFilePaths[i]);
			}));
		}
		else
		{
			for (int num = 0; num < sourceFiles.Length; num++)
			{
				trees[num] = ParseFile(parseOptions, scriptParseOptions, ref hadErrors, sourceFiles[num], diagnosticBag, out normalizedFilePaths[num]);
			}
		}
		if (hadErrors)
		{
			ReportErrors(diagnosticBag.ToReadOnlyAndFree(), consoleOutput, errorLogger);
			return null;
		}
		diagnosticBag.Free();
		List<DiagnosticInfo> list = new List<DiagnosticInfo>();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		for (int num2 = 0; num2 < sourceFiles.Length; num2++)
		{
			string text = normalizedFilePaths[num2];
			if (!hashSet.Add(text))
			{
				list.Add(new DiagnosticInfo(MessageProvider, 2002, Arguments.PrintFullPaths ? text : _diagnosticFormatter.RelativizeNormalizedPath(text)));
				trees[num2] = null;
			}
		}
		if (Arguments.TouchedFilesPath != null)
		{
			foreach (string item in hashSet)
			{
				touchedFilesLogger.AddRead(item);
			}
		}
		DesktopAssemblyIdentityComparer comparer = DesktopAssemblyIdentityComparer.Default;
		string appConfigPath = Arguments.AppConfigPath;
		if (appConfigPath != null)
		{
			try
			{
				using (FileStream input = new FileStream(appConfigPath, FileMode.Open, FileAccess.Read))
				{
					comparer = DesktopAssemblyIdentityComparer.LoadFromXml(input);
				}
				touchedFilesLogger?.AddRead(appConfigPath);
			}
			catch (Exception ex)
			{
				list.Add(new DiagnosticInfo(MessageProvider, 7093, appConfigPath, ex.Message));
			}
		}
		LoggingXmlFileResolver resolver = new LoggingXmlFileResolver(Arguments.BaseDirectory, touchedFilesLogger);
		LoggingSourceFileResolver resolver2 = new LoggingSourceFileResolver(ImmutableArray<string>.Empty, Arguments.BaseDirectory, Arguments.PathMap, touchedFilesLogger);
		List<MetadataReference> references = ResolveMetadataReferences(list, touchedFilesLogger, out var referenceDirectiveResolver);
		if (ReportErrors(list, consoleOutput, errorLogger))
		{
			return null;
		}
		LoggingStrongNameFileSystem fileSystem = new LoggingStrongNameFileSystem(touchedFilesLogger);
		return CSharpCompilation.Create(Arguments.CompilationName, trees.WhereNotNull(), references, Arguments.CompilationOptions.WithMetadataReferenceResolver(referenceDirectiveResolver).WithAssemblyIdentityComparer(comparer).WithXmlReferenceResolver(resolver)
			.WithStrongNameProvider(Arguments.GetStrongNameProvider(fileSystem, _tempDirectory))
			.WithSourceReferenceResolver(resolver2));
	}

	private SyntaxTree ParseFile(CSharpParseOptions parseOptions, CSharpParseOptions scriptParseOptions, ref bool addedDiagnostics, CommandLineSourceFile file, DiagnosticBag diagnostics, out string normalizedFilePath)
	{
		List<DiagnosticInfo> list = new List<DiagnosticInfo>();
		SourceText sourceText = TryReadFileContent(file, list, out normalizedFilePath);
		if (sourceText == null)
		{
			foreach (DiagnosticInfo item in list)
			{
				diagnostics.Add(MessageProvider.CreateDiagnostic(item));
			}
			list.Clear();
			addedDiagnostics = true;
			return null;
		}
		return ParseFile(parseOptions, scriptParseOptions, sourceText, file);
	}

	private static SyntaxTree ParseFile(CSharpParseOptions parseOptions, CSharpParseOptions scriptParseOptions, SourceText content, CommandLineSourceFile file)
	{
		SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(content, file.IsScript ? scriptParseOptions : parseOptions, file.Path);
		syntaxTree.GetMappedLineSpanAndVisibility(default, out var _);
		return syntaxTree;
	}

	protected override string GetOutputFileName(Compilation compilation, CancellationToken cancellationToken)
	{
		if (Arguments.OutputFileName == null)
		{
			CSharpCompilation cSharpCompilation = (CSharpCompilation)compilation;
			Symbol symbol = cSharpCompilation.ScriptClass;
			if ((object)symbol == null)
			{
				MethodSymbol entryPoint = cSharpCompilation.GetEntryPoint(cancellationToken);
				if ((object)entryPoint != null)
				{
					symbol = entryPoint.PartialImplementationPart ?? entryPoint;
				}
			}
			if ((object)symbol != null)
			{
				return Path.ChangeExtension(PathUtilities.GetFileName(symbol.Locations.First().SourceTree.FilePath), ".exe");
			}
			return "error";
		}
		return base.GetOutputFileName(compilation, cancellationToken);
	}

	internal override bool SuppressDefaultResponseFile(IEnumerable<string> args)
	{
		return args.Any((string arg) => Roslyn.Utilities.EnumerableExtensions.Contains(new string[2] { "/noconfig", "-noconfig" }, arg.ToLowerInvariant()));
	}

	public override void PrintLogo(TextWriter consoleOutput)
	{
		consoleOutput.WriteLine(ErrorFacts.GetMessage(MessageID.IDS_LogoLine1, Culture), GetToolName(), GetAssemblyFileVersion());
		consoleOutput.WriteLine(ErrorFacts.GetMessage(MessageID.IDS_LogoLine2, Culture));
		consoleOutput.WriteLine();
	}

	public override void PrintLangVersions(TextWriter consoleOutput)
	{
		consoleOutput.WriteLine(ErrorFacts.GetMessage(MessageID.IDS_LangVersions, Culture));
		LanguageVersion languageVersion = LanguageVersion.Default.MapSpecifiedToEffectiveVersion();
		LanguageVersion languageVersion2 = LanguageVersion.Latest.MapSpecifiedToEffectiveVersion();
		foreach (LanguageVersion value in Enum.GetValues(typeof(LanguageVersion)))
		{
			if (value == languageVersion)
			{
				consoleOutput.WriteLine(value.ToDisplayString() + " (default)");
			}
			else if (value == languageVersion2)
			{
				consoleOutput.WriteLine(value.ToDisplayString() + " (latest)");
			}
			else
			{
				consoleOutput.WriteLine(value.ToDisplayString());
			}
		}
		consoleOutput.WriteLine();
	}

	internal override string GetToolName()
	{
		return ErrorFacts.GetMessage(MessageID.IDS_ToolName, Culture);
	}

	public override void PrintHelp(TextWriter consoleOutput)
	{
		consoleOutput.WriteLine(ErrorFacts.GetMessage(MessageID.IDS_CSCHelp, Culture));
	}

	protected override bool TryGetCompilerDiagnosticCode(string diagnosticId, out uint code)
	{
		return CommonCompiler.TryGetCompilerDiagnosticCode(diagnosticId, "CS", out code);
	}

	protected override ImmutableArray<DiagnosticAnalyzer> ResolveAnalyzersFromArguments(List<DiagnosticInfo> diagnostics, CommonMessageProvider messageProvider)
	{
		return Arguments.ResolveAnalyzersFromArguments("C#", diagnostics, messageProvider, AssemblyLoader);
	}

	protected override void ResolveEmbeddedFilesFromExternalSourceDirectives(SyntaxTree tree, SourceReferenceResolver resolver, OrderedSet<string> embeddedFiles, DiagnosticBag diagnostics)
	{
		foreach (LineDirectiveTriviaSyntax directive in tree.GetRoot().GetDirectives((DirectiveTriviaSyntax d) => d.IsActive && !d.HasErrors && d.Kind() == SyntaxKind.LineDirectiveTrivia))
		{
			string text = (string)directive.File.Value;
			if (text != null)
			{
				string text2 = resolver.ResolveReference(text, tree.FilePath);
				if (text2 == null)
				{
					diagnostics.Add(MessageProvider.CreateDiagnostic(1504, directive.File.GetLocation(), text, CSharpResources.CouldNotFindFile));
				}
				else
				{
					embeddedFiles.Add(text2);
				}
			}
		}
	}
}
