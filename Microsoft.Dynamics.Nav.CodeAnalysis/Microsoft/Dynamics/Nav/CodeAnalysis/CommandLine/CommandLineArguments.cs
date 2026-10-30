using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CommandLine;

internal class CommandLineArguments
{
	public ProjectManifest ProjectManifest { get; internal set; }

	public string BaseDirectory { get; internal set; }

	public string CompilationName { get; internal set; }

	public string OutputFileName { get; internal set; }

	public string OutputDirectory { get; internal set; }

	public string PackageCacheDirectory { get; internal set; }

	public string ErrorLogPath { get; internal set; }

	public ImmutableArray<Diagnostic> Errors { get; internal set; } = ImmutableArray<Diagnostic>.Empty;

	public ImmutableArray<CommandLineAnalyzerReference> AnalyzerReferences { get; internal set; } = ImmutableArray<CommandLineAnalyzerReference>.Empty;

	public ImmutableArray<CommandLineSourceFile> AdditionalFiles { get; internal set; } = ImmutableArray<CommandLineSourceFile>.Empty;

	public ImmutableArray<string> AssemblyProbingPaths { get; internal set; } = ImmutableArray<string>.Empty;

	public string RuleSetPath { get; internal set; }

	public bool ReportAnalyzer { get; internal set; }

	public bool DisplayLogo { get; internal set; }

	public bool DisplayHelp { get; internal set; }

	public bool PrintFullPaths { get; internal set; }

	public Encoding Encoding { get; internal set; }

	public ImmutableArray<CommandLineSourceFile> SourceFiles { get; internal set; }

	public CompilationOptions CompilationOptions { get; internal set; }

	public string ProjectPath { get; internal set; }

	public LogLevel? LogLevel { get; internal set; }

	internal CommandLineArguments()
	{
	}

	public IEnumerable<AnalyzerReference> ResolveAnalyzerReferences(IAnalyzerAssemblyLoader analyzerLoader)
	{
		foreach (CommandLineAnalyzerReference analyzerReference2 in AnalyzerReferences)
		{
			AnalyzerReference analyzerReference = ResolveAnalyzerReference(analyzerReference2, analyzerLoader);
			if (analyzerReference == null)
			{
				analyzerReference = new UnresolvedAnalyzerReference(analyzerReference2.FilePath, string.Format(CultureInfo.InvariantCulture, CodeAnalysisResources.FileNotFoundException, analyzerReference2.FilePath));
			}
			yield return analyzerReference;
		}
	}

	internal ImmutableArray<DiagnosticAnalyzer> ResolveAnalyzersFromArguments(IList<DiagnosticInfo> diagnostics, CommonMessageProvider messageProvider, IAnalyzerAssemblyLoader analyzerLoader)
	{
		ImmutableArray<DiagnosticAnalyzer>.Builder builder = ImmutableArray.CreateBuilder<DiagnosticAnalyzer>();
		EventHandler<AnalyzerLoadFailureEventArgs> value = (object o, AnalyzerLoadFailureEventArgs e) =>
		{
			AnalyzerFileReference analyzerFileReference2 = o as AnalyzerFileReference;
			DiagnosticInfo diagnosticInfo;
			switch (e.ErrorCode)
			{
			default:
				return;
			case AnalyzerLoadFailureEventArgs.FailureErrorCode.UnableToLoadAnalyzer:
				diagnosticInfo = new DiagnosticInfo(messageProvider, 1005, analyzerFileReference2.FullPath, e.Message);
				break;
			case AnalyzerLoadFailureEventArgs.FailureErrorCode.UnableToCreateAnalyzer:
				diagnosticInfo = new DiagnosticInfo(messageProvider, 1003, e.TypeName, analyzerFileReference2.FullPath, e.Message);
				break;
			case AnalyzerLoadFailureEventArgs.FailureErrorCode.NoAnalyzers:
				diagnosticInfo = new DiagnosticInfo(messageProvider, 1004, analyzerFileReference2.FullPath);
				break;
			case AnalyzerLoadFailureEventArgs.FailureErrorCode.None:
				return;
			}
			diagnosticInfo = messageProvider.FilterDiagnosticInfo(diagnosticInfo, CompilationOptions);
			if (diagnosticInfo != null)
			{
				diagnostics.Add(diagnosticInfo);
			}
		};
		foreach (CommandLineAnalyzerReference analyzerReference in AnalyzerReferences)
		{
			AnalyzerFileReference analyzerFileReference = ResolveAnalyzerReference(analyzerReference, analyzerLoader);
			if (analyzerFileReference != null)
			{
				analyzerFileReference.AnalyzerLoadFailed += value;
				analyzerFileReference.AddAnalyzers(builder);
				analyzerFileReference.AnalyzerLoadFailed -= value;
			}
			else
			{
				diagnostics.Add(new DiagnosticInfo(messageProvider, 1006, analyzerReference.FilePath));
			}
		}
		return builder.ToImmutable();
	}

	private AnalyzerFileReference ResolveAnalyzerReference(CommandLineAnalyzerReference reference, IAnalyzerAssemblyLoader analyzerLoader)
	{
		string text = PathUtilities.ResolveRelativePath(reference.FilePath, null, BaseDirectory, null, File.Exists);
		if (text != null)
		{
			text = PathUtilities.TryNormalizeAbsolutePath(text);
		}
		if (text != null)
		{
			return new AnalyzerFileReference(text, analyzerLoader);
		}
		return null;
	}
}
