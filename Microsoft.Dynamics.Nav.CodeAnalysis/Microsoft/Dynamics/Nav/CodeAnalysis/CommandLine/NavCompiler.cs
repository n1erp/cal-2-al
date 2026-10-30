using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.DiagnosticRules;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CommandLine;

internal abstract class NavCompiler : CommonCompiler
{
	internal const string ResponseFileName = "alc.rsp";

	private readonly CommandLineDiagnosticFormatter diagnosticFormatter;

	public override DiagnosticFormatter DiagnosticFormatter => diagnosticFormatter;

	protected virtual string RuleSetPath => Arguments.RuleSetPath;

	protected NavCompiler(CommandLineParser parser, string responseFile, IEnumerable<string> args, string clientDirectory, string baseDirectory, string sdkDirectoryOpt, string additionalReferenceDirectories, IAnalyzerAssemblyLoader analyzerLoader, IFileSystem fileSystem = null)
		: base(parser, responseFile, args, clientDirectory, baseDirectory, sdkDirectoryOpt, additionalReferenceDirectories, analyzerLoader, fileSystem)
	{
		diagnosticFormatter = new CommandLineDiagnosticFormatter(baseDirectory, Arguments.PrintFullPaths, displayEndLocations: false);
	}

	public override Compilation GetOrCreateCompilation(CompilerTextWriter consoleOutput, CompilationOptions compilationOptions, ErrorLogger errorLogger)
	{
		bool hadErrors = false;
		ImmutableArray<CommandLineSourceFile> sourceFiles = Arguments.SourceFiles;
		SyntaxTree[] trees = new SyntaxTree[sourceFiles.Length];
		string[] normalizedFilePaths = new string[sourceFiles.Length];
		ParseOptions parseOptions = new ParseOptions(Arguments.ProjectManifest?.AppManifest.Runtime ?? RuntimeVersion.CurrentRelease);
		if (Arguments.CompilationOptions.ConcurrentBuild)
		{
			Parallel.For(0, sourceFiles.Length, UICultureUtilities.WithCurrentUICulture((int i) =>
			{
				trees[i] = ParseFile(consoleOutput, ref hadErrors, sourceFiles[i], parseOptions, errorLogger, out normalizedFilePaths[i]);
			}));
		}
		else
		{
			for (int num = 0; num < sourceFiles.Length; num++)
			{
				trees[num] = ParseFile(consoleOutput, ref hadErrors, sourceFiles[num], parseOptions, errorLogger, out normalizedFilePaths[num]);
			}
		}
		if (hadErrors)
		{
			return null;
		}
		List<DiagnosticInfo> list = new List<DiagnosticInfo>();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		for (int num2 = 0; num2 < sourceFiles.Length; num2++)
		{
			string text = normalizedFilePaths[num2];
			DebugAssertHelper.Assert(text != null);
			DebugAssertHelper.Assert(PathUtilities.IsAbsolute(text));
			if (!hashSet.Add(text))
			{
				string text2 = (Arguments.PrintFullPaths ? text : diagnosticFormatter.RelativizeNormalizedPath(text));
				list.Add(new DiagnosticInfo(MessageProvider, 1011, text2));
				trees[num2] = null;
			}
		}
		Compilation compilation = Compilation.Create((!string.IsNullOrEmpty(Arguments.CompilationName)) ? Arguments.CompilationName : (Arguments.ProjectManifest?.AppManifest.AppName ?? string.Empty), fileSystem: new RelativeFileSystem(Arguments.ProjectPath), dotNetResolverFactory: GetDotNetResolverFactory(), options: TryReadRuleSetAndApplyToOptions(Arguments.CompilationOptions, list), publisher: Arguments?.ProjectManifest?.AppManifest?.AppPublisher, version: Arguments?.ProjectManifest?.AppManifest?.AppVersion, appId: Arguments?.ProjectManifest?.AppManifest?.AppId, syntaxTrees: trees.WhereNotNull());
		if (Arguments?.ProjectManifest != null)
		{
			compilation = AddSymbolReferences(compilation, Arguments.ProjectManifest, list);
		}
		ReportErrors(list, consoleOutput, errorLogger);
		return compilation;
	}

	protected virtual IDotNetResolverFactory GetDotNetResolverFactory()
	{
		return DotNetResolverFactoryHelper.CreateLocalDotNetResolverFactory(Arguments.AssemblyProbingPaths);
	}

	internal static CompilationOptions TryReadRuleSetAndApplyToOptions(CompilationOptions options, string ruleSetPath, List<Diagnostic> diagnostics)
	{
		RuleSet ruleSet = RuleSetResolver.DefaultRuleSet;
		if (!string.IsNullOrEmpty(ruleSetPath))
		{
			RuleSet ruleSet2 = RuleSetResolver.LoadFromFile(ruleSetPath, diagnostics);
			if (diagnostics.Count == 0)
			{
				ruleSet = ruleSet2;
			}
		}
		return options.WithGeneralDiagnosticOption(ruleSet.GeneralDiagnosticOption).WithSpecificDiagnosticOptions(ruleSet.SpecificDiagnosticOptions);
	}

	private CompilationOptions TryReadRuleSetAndApplyToOptions(CompilationOptions options, List<DiagnosticInfo> diagnostics)
	{
		List<Diagnostic> list = new List<Diagnostic>();
		options = TryReadRuleSetAndApplyToOptions(options, RuleSetPath, list);
		if (list.Count != 0)
		{
			foreach (Diagnostic item in list)
			{
				diagnostics.Add(new DiagnosticInfo(MessageProvider, item.Code, item.Arguments.ToArray()));
			}
		}
		return options;
	}

	protected virtual Compilation AddSymbolReferences(Compilation compilation, ProjectManifest manifest, List<DiagnosticInfo> diagnostics)
	{
		IEnumerable<SymbolReferenceSpecification> allReferences = manifest.GetAllReferences();
		if (allReferences.Any())
		{
			if (!string.IsNullOrEmpty(Arguments.PackageCacheDirectory))
			{
				compilation = compilation.AddReferences(allReferences);
				compilation = compilation.WithReferenceLoader(new LocalCacheSymbolReferenceLoader(Arguments.PackageCacheDirectory));
			}
			else
			{
				diagnostics.Add(new DiagnosticInfo(MessageProvider, 1021));
			}
		}
		return compilation;
	}

	private SyntaxTree ParseFile(CompilerTextWriter consoleOutput, ref bool hadErrors, CommandLineSourceFile file, ParseOptions options, ErrorLogger errorLogger, out string normalizedFilePath)
	{
		List<DiagnosticInfo> list = new List<DiagnosticInfo>();
		SourceText sourceText = ReadFileContent(file, list, out normalizedFilePath);
		if (sourceText == null)
		{
			ReportErrors(list, consoleOutput, errorLogger);
			list.Clear();
			hadErrors = true;
			return null;
		}
		return ParseFile(sourceText, file, options);
	}

	private static SyntaxTree ParseFile(SourceText content, CommandLineSourceFile file, ParseOptions options)
	{
		return SyntaxFactory.ParseSyntaxTree(content, file.Path, options);
	}

	protected override string GetOutputFileName(Compilation compilation, CancellationToken cancellationToken)
	{
		if (Arguments.OutputFileName == null)
		{
			return ProjectManifest.AppManifest.DefaultPackageName() + ".app";
		}
		return base.GetOutputFileName(compilation, cancellationToken);
	}

	[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
	internal override bool SuppressDefaultResponseFile(IEnumerable<string> args)
	{
		return args.Any((string arg) => Enumerable.Contains(new string[2] { "/noconfig", "-noconfig" }, arg.ToLowerInvariant()));
	}

	public override void PrintLogo(CompilerTextWriter consoleOutput)
	{
		consoleOutput.WriteLine(ErrorFacts.GetMessage(MessageID.IDS_LogoLine1, Culture), GetToolName(), GetAssemblyFileVersion());
		consoleOutput.WriteLine(ErrorFacts.GetMessage(MessageID.IDS_LogoLine2, Culture));
		consoleOutput.WriteLine();
	}

	internal override string GetToolName()
	{
		return ErrorFacts.GetMessage(MessageID.IDS_ToolName, Culture);
	}

	public override void PrintHelp(CompilerTextWriter consoleOutput)
	{
		consoleOutput.WriteLine(ErrorFacts.GetMessage(MessageID.IDS_ALCHelp, Culture));
	}

	protected override bool TryGetCompilerDiagnosticCode(string diagnosticId, out uint code)
	{
		return CommonCompiler.TryGetCompilerDiagnosticCode(diagnosticId, "AL", out code);
	}

	protected override ImmutableArray<DiagnosticAnalyzer> ResolveAnalyzersFromArguments(IList<DiagnosticInfo> diagnostics, CommonMessageProvider messageProvider)
	{
		return Arguments.ResolveAnalyzersFromArguments(diagnostics, messageProvider, AnalyzerLoader);
	}
}
