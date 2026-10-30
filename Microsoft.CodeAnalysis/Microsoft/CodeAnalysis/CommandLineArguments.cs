using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public abstract class CommandLineArguments
{
	internal bool IsScriptRunner { get; set; }

	public bool InteractiveMode { get; internal set; }

	public string BaseDirectory { get; internal set; }

	public ImmutableArray<KeyValuePair<string, string>> PathMap { get; internal set; }

	public ImmutableArray<string> ReferencePaths { get; internal set; }

	public ImmutableArray<string> SourcePaths { get; internal set; }

	public ImmutableArray<string> KeyFileSearchPaths { get; internal set; }

	public bool Utf8Output { get; internal set; }

	public string CompilationName { get; internal set; }

	public EmitOptions EmitOptions { get; internal set; }

	public string OutputFileName { get; internal set; }

	public string OutputRefFilePath { get; internal set; }

	public string PdbPath { get; internal set; }

	public string SourceLink { get; internal set; }

	public string RuleSetPath { get; internal set; }

	public bool EmitPdb { get; internal set; }

	public string OutputDirectory { get; internal set; }

	public string DocumentationPath { get; internal set; }

	public string ErrorLogPath { get; internal set; }

	public string AppConfigPath { get; internal set; }

	public ImmutableArray<Diagnostic> Errors { get; internal set; }

	public ImmutableArray<CommandLineReference> MetadataReferences { get; internal set; }

	public ImmutableArray<CommandLineAnalyzerReference> AnalyzerReferences { get; internal set; }

	public ImmutableArray<CommandLineSourceFile> AdditionalFiles { get; internal set; }

	public ImmutableArray<CommandLineSourceFile> EmbeddedFiles { get; internal set; }

	public bool ReportAnalyzer { get; internal set; }

	public bool DisplayLogo { get; internal set; }

	public bool DisplayHelp { get; internal set; }

	public bool DisplayVersion { get; internal set; }

	public bool DisplayLangVersions { get; internal set; }

	public string Win32ResourceFile { get; internal set; }

	public string Win32Icon { get; internal set; }

	public string Win32Manifest { get; internal set; }

	public bool NoWin32Manifest { get; internal set; }

	public ImmutableArray<ResourceDescription> ManifestResources { get; internal set; }

	public Encoding Encoding { get; internal set; }

	public SourceHashAlgorithm ChecksumAlgorithm { get; internal set; }

	public ImmutableArray<string> ScriptArguments { get; internal set; }

	public ImmutableArray<CommandLineSourceFile> SourceFiles { get; internal set; }

	public string TouchedFilesPath { get; internal set; }

	public bool PrintFullPaths { get; internal set; }

	public ParseOptions ParseOptions => ParseOptionsCore;

	public CompilationOptions CompilationOptions => CompilationOptionsCore;

	protected abstract ParseOptions ParseOptionsCore { get; }

	protected abstract CompilationOptions CompilationOptionsCore { get; }

	public CultureInfo PreferredUILang { get; internal set; }

	internal StrongNameProvider GetStrongNameProvider(StrongNameFileSystem fileSystem, string tempDirectory)
	{
		if ((CoreClrShim.IsRunningOnCoreClr || PlatformInformation.IsRunningOnMono) && !ParseOptionsCore.Features.ContainsKey("UseLegacyStrongNameProvider") && CompilationOptionsCore.CryptoKeyContainer == null)
		{
			return new PortableStrongNameProvider(KeyFileSearchPaths, fileSystem);
		}
		return new DesktopStrongNameProvider(KeyFileSearchPaths, tempDirectory, fileSystem);
	}

	internal CommandLineArguments()
	{
	}

	public IEnumerable<MetadataReference> ResolveMetadataReferences(MetadataReferenceResolver metadataResolver)
	{
		if (metadataResolver == null)
		{
			throw new ArgumentNullException("metadataResolver");
		}
		return ResolveMetadataReferences(metadataResolver, null, null);
	}

	internal IEnumerable<MetadataReference> ResolveMetadataReferences(MetadataReferenceResolver metadataResolver, List<DiagnosticInfo> diagnosticsOpt, CommonMessageProvider messageProviderOpt)
	{
		List<MetadataReference> list = new List<MetadataReference>();
		ResolveMetadataReferences(metadataResolver, diagnosticsOpt, messageProviderOpt, list);
		return list;
	}

	internal virtual bool ResolveMetadataReferences(MetadataReferenceResolver metadataResolver, List<DiagnosticInfo> diagnosticsOpt, CommonMessageProvider messageProviderOpt, List<MetadataReference> resolved)
	{
		bool result = true;
		foreach (CommandLineReference metadataReference in MetadataReferences)
		{
			ImmutableArray<PortableExecutableReference> immutableArray = ResolveMetadataReference(metadataReference, metadataResolver, diagnosticsOpt, messageProviderOpt);
			if (!immutableArray.IsDefaultOrEmpty)
			{
				resolved.AddRange(immutableArray);
				continue;
			}
			result = false;
			if (diagnosticsOpt == null)
			{
				resolved.Add(new UnresolvedMetadataReference(metadataReference.Reference, metadataReference.Properties));
			}
		}
		return result;
	}

	internal static ImmutableArray<PortableExecutableReference> ResolveMetadataReference(CommandLineReference cmdReference, MetadataReferenceResolver metadataResolver, List<DiagnosticInfo> diagnosticsOpt, CommonMessageProvider messageProviderOpt)
	{
		ImmutableArray<PortableExecutableReference> result;
		try
		{
			result = metadataResolver.ResolveReference(cmdReference.Reference, null, cmdReference.Properties);
		}
		catch (Exception ex) when (diagnosticsOpt != null && (ex is BadImageFormatException || ex is IOException))
		{
			Diagnostic diagnostic = PortableExecutableReference.ExceptionToDiagnostic(ex, messageProviderOpt, Location.None, cmdReference.Reference, cmdReference.Properties.Kind);
			diagnosticsOpt.Add(((DiagnosticWithInfo)diagnostic).Info);
			return ImmutableArray<PortableExecutableReference>.Empty;
		}
		if (result.IsDefaultOrEmpty && diagnosticsOpt != null)
		{
			diagnosticsOpt.Add(new DiagnosticInfo(messageProviderOpt, messageProviderOpt.ERR_MetadataFileNotFound, cmdReference.Reference));
			return ImmutableArray<PortableExecutableReference>.Empty;
		}
		return result;
	}

	public IEnumerable<AnalyzerReference> ResolveAnalyzerReferences(IAnalyzerAssemblyLoader analyzerLoader)
	{
		foreach (CommandLineAnalyzerReference analyzerReference in AnalyzerReferences)
		{
			yield return (AnalyzerReference)(((object)ResolveAnalyzerReference(analyzerReference, analyzerLoader)) ?? ((object)new UnresolvedAnalyzerReference(analyzerReference.FilePath)));
		}
	}

	internal ImmutableArray<DiagnosticAnalyzer> ResolveAnalyzersFromArguments(string language, List<DiagnosticInfo> diagnostics, CommonMessageProvider messageProvider, IAnalyzerAssemblyLoader analyzerLoader)
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
				diagnosticInfo = new DiagnosticInfo(messageProvider, messageProvider.WRN_UnableToLoadAnalyzer, analyzerFileReference2.FullPath, e.Message);
				break;
			case AnalyzerLoadFailureEventArgs.FailureErrorCode.UnableToCreateAnalyzer:
				diagnosticInfo = new DiagnosticInfo(messageProvider, messageProvider.WRN_AnalyzerCannotBeCreated, e.TypeName, analyzerFileReference2.FullPath, e.Message);
				break;
			case AnalyzerLoadFailureEventArgs.FailureErrorCode.NoAnalyzers:
				diagnosticInfo = new DiagnosticInfo(messageProvider, messageProvider.WRN_NoAnalyzerInAssembly, analyzerFileReference2.FullPath);
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
		ArrayBuilder<AnalyzerFileReference> instance = ArrayBuilder<AnalyzerFileReference>.GetInstance();
		foreach (CommandLineAnalyzerReference analyzerReference in AnalyzerReferences)
		{
			AnalyzerFileReference analyzerFileReference = ResolveAnalyzerReference(analyzerReference, analyzerLoader);
			if (analyzerFileReference != null)
			{
				instance.Add(analyzerFileReference);
				analyzerLoader.AddDependencyLocation(analyzerFileReference.FullPath);
			}
			else
			{
				diagnostics.Add(new DiagnosticInfo(messageProvider, messageProvider.ERR_MetadataFileNotFound, analyzerReference.FilePath));
			}
		}
		foreach (AnalyzerFileReference item in instance)
		{
			item.AnalyzerLoadFailed += value;
			item.AddAnalyzers(builder, language);
			item.AnalyzerLoadFailed -= value;
		}
		instance.Free();
		return builder.ToImmutable();
	}

	private AnalyzerFileReference ResolveAnalyzerReference(CommandLineAnalyzerReference reference, IAnalyzerAssemblyLoader analyzerLoader)
	{
		string text = FileUtilities.ResolveRelativePath(reference.FilePath, null, BaseDirectory, ReferencePaths, File.Exists);
		if (text != null)
		{
			text = FileUtilities.TryNormalizeAbsolutePath(text);
		}
		if (text != null)
		{
			return new AnalyzerFileReference(text, analyzerLoader);
		}
		return null;
	}
}
