using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
public sealed class ProjectInfo
{
	public ProjectId Id { get; }

	public VersionStamp Version { get; }

	public string Name { get; }

	public string AssemblyName { get; }

	public string Language { get; }

	public string FilePath { get; }

	public string OutputFilePath { get; }

	public CompilationOptions CompilationOptions { get; }

	public ParseOptions ParseOptions { get; }

	public IReadOnlyList<DocumentInfo> Documents { get; }

	public IReadOnlyList<ProjectReference> ProjectReferences { get; }

	public IReadOnlyList<SymbolReferenceSpecification> SymbolReferences { get; }

	public string PackageCachePath { get; }

	public IReadOnlyList<string> AssemblyProbingPaths { get; }

	public string RuleSetPath { get; }

	public IReadOnlyList<AnalyzerReference> AnalyzerReferences { get; }

	public bool BackgroundCodeAnalysisEnabled { get; }

	public ImmutableArray<DiagnosticAnalyzer> DiagnosticAnalyzers { get; }

	public IReadOnlyList<DocumentInfo> AdditionalDocuments { get; }

	public Type HostObjectType { get; }

	public bool EnableCodeActions { get; }

	public bool IncrementalBuild { get; }

	private ProjectInfo(ProjectId id, VersionStamp version, string name, string assemblyName, string language, string filePath, string outputFilePath, string packageCachePath, CompilationOptions compilationOptions, ParseOptions parseOptions, IEnumerable<DocumentInfo> documents, IEnumerable<ProjectReference> projectReferences, IEnumerable<SymbolReferenceSpecification> symbolReferences, IEnumerable<AnalyzerReference> analyzerReferences, bool backgroundCodeAnalysisEnabled, IEnumerable<DocumentInfo> additionalDocuments, Type hostObjectType, IEnumerable<string> assemblyProbingPaths, string ruleSetPath, bool enableCodeActions, bool incrementalBuild)
	{
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (assemblyName == null)
		{
			throw new ArgumentNullException("assemblyName");
		}
		if (language == null)
		{
			throw new ArgumentNullException("language");
		}
		Id = id;
		Version = version;
		Name = name;
		AssemblyName = assemblyName;
		Language = language;
		FilePath = filePath;
		OutputFilePath = outputFilePath;
		PackageCachePath = CreatePackageCachePath(packageCachePath);
		CompilationOptions = compilationOptions;
		ParseOptions = parseOptions;
		AssemblyProbingPaths = assemblyProbingPaths.ToImmutableReadOnlyListOrEmpty();
		Documents = documents.ToImmutableReadOnlyListOrEmpty();
		ProjectReferences = projectReferences.ToImmutableReadOnlyListOrEmpty();
		SymbolReferences = symbolReferences.ToImmutableReadOnlyListOrEmpty();
		AnalyzerReferences = analyzerReferences.ToImmutableReadOnlyListOrEmpty();
		BackgroundCodeAnalysisEnabled = backgroundCodeAnalysisEnabled;
		DiagnosticAnalyzers = AnalyzerReferences.SelectMany((AnalyzerReference a) => a.GetAnalyzers()).ToImmutableArray();
		AdditionalDocuments = additionalDocuments.ToImmutableReadOnlyListOrEmpty();
		HostObjectType = hostObjectType;
		RuleSetPath = ruleSetPath;
		EnableCodeActions = enableCodeActions;
		IncrementalBuild = incrementalBuild;
	}

	public static ProjectInfo Create(ProjectId id, VersionStamp version, string name, string assemblyName, string language, string filePath = null, string outputFilePath = null, string packageCachePath = null, CompilationOptions compilationOptions = null, ParseOptions parseOptions = null, IEnumerable<DocumentInfo> documents = null, IEnumerable<ProjectReference> projectReferences = null, IEnumerable<SymbolReferenceSpecification> symbolReferences = null, IEnumerable<AnalyzerReference> analyzerReferences = null, bool enableCodeAnalyzersInBackground = true, IEnumerable<DocumentInfo> additionalDocuments = null, Type hostObjectType = null, IEnumerable<string> assemblyProbingPaths = null, string ruleSetPath = null, bool enableCodeActions = false, bool incrementalBuild = false)
	{
		return new ProjectInfo(id, version, name, assemblyName, language, filePath, outputFilePath, packageCachePath, compilationOptions, parseOptions, documents, projectReferences, symbolReferences, analyzerReferences, enableCodeAnalyzersInBackground, additionalDocuments, hostObjectType, assemblyProbingPaths, ruleSetPath, enableCodeActions, incrementalBuild);
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	private ProjectInfo With(ProjectId id = null, VersionStamp? version = null, string name = null, string assemblyName = null, string language = null, Optional<string> filePath = default(Optional<string>), Optional<string> outputPath = default(Optional<string>), Optional<string> packageCachePath = default(Optional<string>), CompilationOptions compilationOptions = null, ParseOptions parseOptions = null, IEnumerable<DocumentInfo> documents = null, IEnumerable<ProjectReference> projectReferences = null, IEnumerable<SymbolReferenceSpecification> symbolReferences = null, IEnumerable<AnalyzerReference> analyzerReferences = null, Optional<bool> backgroundCodeAnalysisEnabled = default(Optional<bool>), IEnumerable<DocumentInfo> additionalDocuments = null, Optional<Type> hostObjectType = default(Optional<Type>), IEnumerable<string> assemblyProbingPaths = null, Optional<string> ruleSetPath = default(Optional<string>), Optional<bool> enableCodeActions = default(Optional<bool>), Optional<bool> incrementalBuild = default(Optional<bool>))
	{
		ProjectId projectId = id ?? Id;
		VersionStamp versionStamp = (version.HasValue ? version.Value : Version);
		string text = name ?? Name;
		string text2 = assemblyName ?? AssemblyName;
		string text3 = language ?? Language;
		string text4 = (filePath.HasValue ? filePath.Value : FilePath);
		string text5 = (outputPath.HasValue ? outputPath.Value : OutputFilePath);
		string text6 = (packageCachePath.HasValue ? CreatePackageCachePath(packageCachePath.Value) : PackageCachePath);
		CompilationOptions compilationOptions2 = compilationOptions ?? CompilationOptions;
		ParseOptions parseOptions2 = parseOptions ?? ParseOptions;
		IEnumerable<DocumentInfo> enumerable = documents ?? Documents;
		IEnumerable<ProjectReference> enumerable2 = projectReferences ?? ProjectReferences;
		IEnumerable<SymbolReferenceSpecification> enumerable3 = symbolReferences ?? SymbolReferences;
		IEnumerable<AnalyzerReference> enumerable4 = analyzerReferences ?? AnalyzerReferences;
		bool flag = (backgroundCodeAnalysisEnabled.HasValue ? backgroundCodeAnalysisEnabled.Value : BackgroundCodeAnalysisEnabled);
		IEnumerable<DocumentInfo> enumerable5 = additionalDocuments ?? AdditionalDocuments;
		Type type = (hostObjectType.HasValue ? hostObjectType.Value : HostObjectType);
		IEnumerable<string> enumerable6 = assemblyProbingPaths ?? AssemblyProbingPaths;
		string text7 = (ruleSetPath.HasValue ? ruleSetPath.Value : RuleSetPath);
		bool flag2 = (enableCodeActions.HasValue ? enableCodeActions.Value : EnableCodeActions);
		bool flag3 = (incrementalBuild.HasValue ? incrementalBuild.Value : IncrementalBuild);
		if (projectId == Id && versionStamp == Version && text == Name && text2 == AssemblyName && text3 == Language && text4 == FilePath && text5 == OutputFilePath && text6 == PackageCachePath && compilationOptions2 == CompilationOptions && parseOptions2 == ParseOptions && enumerable == Documents && enumerable2 == ProjectReferences && enumerable3 == SymbolReferences && enumerable4 == AnalyzerReferences && flag == BackgroundCodeAnalysisEnabled && enumerable5 == AdditionalDocuments && type == HostObjectType && enumerable6 == AssemblyProbingPaths && text7 == RuleSetPath && flag2 == EnableCodeActions && flag3 == IncrementalBuild)
		{
			return this;
		}
		return new ProjectInfo(projectId, versionStamp, text, text2, text3, text4, text5, text6, compilationOptions2, parseOptions2, enumerable, enumerable2, enumerable3, enumerable4, flag, enumerable5, type, enumerable6, text7, flag2, flag3);
	}

	public ProjectInfo WithDocuments(IEnumerable<DocumentInfo> documents)
	{
		return With(null, null, null, null, null, default, default, default, null, null, documents.ToImmutableReadOnlyListOrEmpty());
	}

	public ProjectInfo WithAdditionalDocuments(IEnumerable<DocumentInfo> additionalDocuments)
	{
		return With(null, null, null, null, null, default, default, default, null, null, null, null, null, null, default, additionalDocuments.ToImmutableReadOnlyListOrEmpty());
	}

	public ProjectInfo WithVersion(VersionStamp version)
	{
		return With(null, version);
	}

	public ProjectInfo WithName(string name)
	{
		return With(null, null, name);
	}

	public ProjectInfo WithFilePath(string filePath)
	{
		return With(null, null, null, null, null, filePath);
	}

	public ProjectInfo WithAssemblyName(string assemblyName)
	{
		return With(null, null, null, assemblyName);
	}

	public ProjectInfo WithOutputFilePath(string outputFilePath)
	{
		return With(null, null, null, null, null, default, outputFilePath);
	}

	public ProjectInfo WithPackageCachePath(string packageCachePath)
	{
		return With(null, null, null, null, null, default, default, packageCachePath);
	}

	public ProjectInfo WithAssemblyProbingPaths(IReadOnlyList<string> assemblyProbingPaths)
	{
		return With(null, null, null, null, null, default, default, default, null, null, null, null, null, null, default, null, default, assemblyProbingPaths);
	}

	public ProjectInfo WithRuleSetPath(string ruleSetPath)
	{
		return With(null, null, null, null, null, default, default, default, null, null, null, null, null, null, default, null, default, null, ruleSetPath);
	}

	public ProjectInfo WithCompilationOptions(CompilationOptions compilationOptions)
	{
		return With(null, null, null, null, null, default, default, default, compilationOptions);
	}

	public ProjectInfo WithParseOptions(ParseOptions parseOptions)
	{
		return With(null, null, null, null, null, default, default, default, null, parseOptions);
	}

	public ProjectInfo WithProjectReferences(IEnumerable<ProjectReference> projectReferences)
	{
		return With(null, null, null, null, null, default, default, default, null, null, null, projectReferences.ToImmutableReadOnlyListOrEmpty());
	}

	public ProjectInfo WithSymbolReferences(IEnumerable<SymbolReferenceSpecification> symbolReferences)
	{
		return With(null, null, null, null, null, default, default, default, null, null, null, null, symbolReferences.ToImmutableReadOnlyListOrEmpty());
	}

	public ProjectInfo WithAnalyzerReferences(IEnumerable<AnalyzerReference> analyzerReferences)
	{
		return With(null, null, null, null, null, default, default, default, null, null, null, null, null, analyzerReferences.ToImmutableReadOnlyListOrEmpty());
	}

	public ProjectInfo WithBackgroundCodeAnalysisEnabled(bool backgroundCodeAnalysisEnabled)
	{
		return With(null, null, null, null, null, default, default, default, null, null, null, null, null, null, backgroundCodeAnalysisEnabled);
	}

	public ProjectInfo WithEnableCodeActions(bool enableCodeActions)
	{
		return With(null, null, null, null, null, default, default, default, null, null, null, null, null, null, default, null, default, null, default, enableCodeActions);
	}

	public ProjectInfo WithIncrementalBuild(bool incrementalBuild)
	{
		return With(null, null, null, null, null, default, default, default, null, null, null, null, null, null, default, null, default, null, default, default, incrementalBuild);
	}

	internal string GetDebuggerDisplay()
	{
		return "ProjectInfo " + Name + ((!string.IsNullOrWhiteSpace(FilePath)) ? (" " + FilePath) : string.Empty);
	}

	private string CreatePackageCachePath(string path)
	{
		if (PathUtilities.IsAbsolute(path))
		{
			return path;
		}
		return PathUtilities.ResolveRelativePath(path, Path.GetDirectoryName(FilePath));
	}
}
