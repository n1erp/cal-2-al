using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
public class Project
{
	private ImmutableHashMap<DocumentId, Document> idToDocumentMap = ImmutableHashMap<DocumentId, Document>.Empty;

	private ImmutableHashMap<DocumentId, TextDocument> idToAdditionalDocumentMap = ImmutableHashMap<DocumentId, TextDocument>.Empty;

	private static readonly Func<DocumentId, Project, Document> createDocumentFunction = CreateDocument;

	private static readonly Func<DocumentId, Project, TextDocument> createAdditionalDocumentFunction = CreateAdditionalDocument;

	internal ProjectState State { get; }

	internal IObjectChangeManager ObjectChangeManager => State.ObjectChangeManager;

	public Solution Solution { get; }

	public ProjectId Id => State.Id;

	public string FilePath => State.FilePath;

	public string OutputFilePath => State.OutputFilePath;

	public string PackageCachePath => State.PackageCachePath;

	public IReadOnlyList<string> AssemblyProbingPaths => State.AssemblyProbingPaths;

	public string RuleSetPath => State.RuleSetPath;

	public bool SupportsCompilation => LanguageServices.GetService<ICompilationFactoryService>() != null;

	public AbstractHostLanguageServices LanguageServices => State.LanguageServices;

	public string Language => State.LanguageServices.Language;

	public string AssemblyName => State.AssemblyName;

	public string Name => State.Name;

	public IReadOnlyList<SymbolReferenceSpecification> SymbolReferences => State.SymbolReferences;

	public IEnumerable<ProjectReference> ProjectReferences => State.ProjectReferences.Where((ProjectReference pr) => Solution.ContainsProject(pr.ProjectId));

	public IReadOnlyList<ProjectReference> AllProjectReferences => State.ProjectReferences;

	public IReadOnlyList<AnalyzerReference> AnalyzerReferences => State.AnalyzerReferences;

	public bool BackgroundCodeAnalysisEnabled => State.BackgroundCodeAnalysisEnabled;

	public bool EnableCodeActions => State.EnableCodeActions;

	public bool IncrementalBuild => State.IncrementalBuild;

	public ImmutableArray<DiagnosticAnalyzer> DiagnosticAnalyzers => State.DiagnosticAnalyzers;

	public AnalyzerOptions AnalyzerOptions => State.AnalyzerOptions;

	public CompilationOptions CompilationOptions => State.CompilationOptions;

	public ParseOptions ParseOptions => State.ParseOptions;

	public bool HasDocuments => State.HasDocuments;

	public IReadOnlyList<DocumentId> DocumentIds => State.DocumentIds;

	public IReadOnlyList<DocumentId> AdditionalDocumentIds => State.AdditionalDocumentIds;

	public IEnumerable<Document> Documents => State.DocumentIds.Select(GetDocument);

	public IEnumerable<TextDocument> AdditionalDocuments => State.AdditionalDocumentIds.Select(GetAdditionalDocument);

	public VersionStamp Version => State.Version;

	internal Project(Solution solution, ProjectState projectState)
	{
		Contract.ThrowIfNull(solution);
		Contract.ThrowIfNull(projectState);
		Solution = solution;
		State = projectState;
	}

	public bool ContainsDocument(DocumentId documentId)
	{
		return State.ContainsDocument(documentId);
	}

	public bool ContainsAdditionalDocument(DocumentId documentId)
	{
		return State.ContainsAdditionalDocument(documentId);
	}

	public DocumentId GetDocumentId(SyntaxTree syntaxTree)
	{
		return Solution.GetDocumentId(syntaxTree, Id);
	}

	public Document GetDocument(SyntaxTree syntaxTree)
	{
		return Solution.GetDocument(syntaxTree, Id);
	}

	public Document GetDocument(DocumentId documentId)
	{
		if (!ContainsDocument(documentId))
		{
			return null;
		}
		return ImmutableHashMapExtensions.GetOrAdd(ref idToDocumentMap, documentId, createDocumentFunction, this);
	}

	public TextDocument GetAdditionalDocument(DocumentId documentId)
	{
		if (!ContainsAdditionalDocument(documentId))
		{
			return null;
		}
		return ImmutableHashMapExtensions.GetOrAdd(ref idToAdditionalDocumentMap, documentId, createAdditionalDocumentFunction, this);
	}

	internal DocumentState GetDocumentState(DocumentId documentId)
	{
		return State.GetDocumentState(documentId);
	}

	internal TextDocumentState GetAdditionalDocumentState(DocumentId documentId)
	{
		return State.GetAdditionalDocumentState(documentId);
	}

	internal Task<bool> ContainsSymbolsWithNameAsync(Func<string, bool> predicate, SymbolFilter filter, CancellationToken cancellationToken)
	{
		return Solution.ContainsSymbolsWithNameAsync(Id, predicate, filter, cancellationToken);
	}

	internal Task<IEnumerable<Document>> GetDocumentsWithName(Func<string, bool> predicate, SymbolFilter filter, CancellationToken cancellationToken)
	{
		return Solution.GetDocumentsWithName(Id, predicate, filter, cancellationToken);
	}

	private static Document CreateDocument(DocumentId documentId, Project project)
	{
		return new Document(project, project.State.GetDocumentState(documentId));
	}

	private static TextDocument CreateAdditionalDocument(DocumentId documentId, Project project)
	{
		return new TextDocument(project, project.State.GetAdditionalDocumentState(documentId));
	}

	public bool TryGetCompilation(out Compilation compilation)
	{
		return Solution.TryGetCompilation(Id, out compilation);
	}

	public Task<Compilation> GetCompilationAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return Solution.GetCompilationAsync(this, cancellationToken);
	}

	public ProjectChanges GetChanges(Project oldProject)
	{
		if (oldProject == null)
		{
			throw new ArgumentNullException("oldProject");
		}
		return new ProjectChanges(this, oldProject);
	}

	public Task<VersionStamp> GetLatestDocumentVersionAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return State.GetLatestDocumentVersionAsync(cancellationToken);
	}

	public Task<VersionStamp> GetDependentVersionAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return Solution.GetDependentVersionAsync(Id, cancellationToken);
	}

	public Task<VersionStamp> GetDependentSemanticVersionAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return Solution.GetDependentSemanticVersionAsync(Id, cancellationToken);
	}

	public async Task<VersionStamp> GetSemanticVersionAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		VersionStamp projVersion = Version;
		return (await State.GetLatestDocumentTopLevelChangeVersionAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetNewerVersion(projVersion);
	}

	public Project WithAssemblyName(string assemblyName)
	{
		return Solution.WithProjectAssemblyName(Id, assemblyName).GetProject(Id);
	}

	public Project WithCompilationOptions(CompilationOptions options)
	{
		return Solution.WithProjectCompilationOptions(Id, options).GetProject(Id);
	}

	public Project AddProjectReference(ProjectReference projectReference)
	{
		return Solution.AddProjectReference(Id, projectReference).GetProject(Id);
	}

	public Project AddProjectReferences(IEnumerable<ProjectReference> projectReferences)
	{
		return Solution.AddProjectReferences(Id, projectReferences).GetProject(Id);
	}

	public Project RemoveProjectReference(ProjectReference projectReference)
	{
		return Solution.RemoveProjectReference(Id, projectReference).GetProject(Id);
	}

	public Project WithProjectReferences(IEnumerable<ProjectReference> projectReferences)
	{
		return Solution.WithProjectReferences(Id, projectReferences).GetProject(Id);
	}

	public Project AddSymbolReference(SymbolReferenceSpecification symbolReference)
	{
		return Solution.AddSymbolReference(Id, symbolReference).GetProject(Id);
	}

	public Project AddSymbolReferences(IEnumerable<SymbolReferenceSpecification> symbolReferences)
	{
		return Solution.AddSymbolReferences(Id, symbolReferences).GetProject(Id);
	}

	public Project RemoveSymbolReference(SymbolReferenceSpecification symbolReference)
	{
		return Solution.RemoveSymbolReference(Id, symbolReference).GetProject(Id);
	}

	public Project WithSymbolReferences(IEnumerable<SymbolReferenceSpecification> symbolReferences)
	{
		return Solution.WithProjectSymbolReferences(Id, symbolReferences).GetProject(Id);
	}

	public Project AddAnalyzerReference(AnalyzerReference analyzerReference)
	{
		return Solution.AddAnalyzerReference(Id, analyzerReference).GetProject(Id);
	}

	public Project AddAnalyzerReferences(IEnumerable<AnalyzerReference> analyzerReferences)
	{
		return Solution.AddAnalyzerReferences(Id, analyzerReferences).GetProject(Id);
	}

	public Project RemoveAnalyzerReference(AnalyzerReference analyzerReference)
	{
		return Solution.RemoveAnalyzerReference(Id, analyzerReference).GetProject(Id);
	}

	public Project WithAnalyzerReferences(IEnumerable<AnalyzerReference> analyzerReferences)
	{
		return Solution.WithProjectAnalyzerReferences(Id, analyzerReferences).GetProject(Id);
	}

	public Document AddDocument(string name, SyntaxNode syntaxRoot, IEnumerable<string> folders = null, string filePath = null)
	{
		DocumentId documentId = DocumentId.CreateNewId(Id);
		return Solution.AddDocument(documentId, name, syntaxRoot, folders, filePath, isGenerated: false, PreservationMode.PreserveIdentity).GetDocument(documentId);
	}

	public Document AddDocument(string name, SourceText text, IEnumerable<string> folders = null, string filePath = null)
	{
		DocumentId documentId = DocumentId.CreateNewId(Id);
		return Solution.AddDocument(documentId, name, text, folders, filePath).GetDocument(documentId);
	}

	public Document AddDocument(string name, string text, IEnumerable<string> folders = null, string filePath = null)
	{
		DocumentId documentId = DocumentId.CreateNewId(Id, name);
		return Solution.AddDocument(documentId, name, text, folders, filePath).GetDocument(documentId);
	}

	public TextDocument AddAdditionalDocument(string name, SourceText text, IEnumerable<string> folders = null, string filePath = null)
	{
		DocumentId documentId = DocumentId.CreateNewId(Id);
		return Solution.AddAdditionalDocument(documentId, name, text, folders, filePath).GetAdditionalDocument(documentId);
	}

	public TextDocument AddAdditionalDocument(string name, string text, IEnumerable<string> folders = null, string filePath = null)
	{
		DocumentId documentId = DocumentId.CreateNewId(Id);
		return Solution.AddAdditionalDocument(documentId, name, text, folders, filePath).GetAdditionalDocument(documentId);
	}

	public Project RemoveDocument(DocumentId documentId)
	{
		return Solution.RemoveDocument(documentId).GetProject(Id);
	}

	public Project RemoveAdditionalDocument(DocumentId documentId)
	{
		return Solution.RemoveAdditionalDocument(documentId).GetProject(Id);
	}

	public bool IsFileFromProjectFolder(string textDocumentFileName)
	{
		try
		{
			DirectoryInfo parent = new DirectoryInfo(FilePath).Parent;
			DirectoryInfo directoryInfo = new DirectoryInfo(textDocumentFileName);
			return parent != null && directoryInfo.FullName.CreateUriFilePath().StartsWith(parent.FullName.CreateUriFilePath(), StringComparison.Ordinal);
		}
		catch (NotSupportedException)
		{
		}
		catch (ArgumentException)
		{
		}
		catch (PathTooLongException)
		{
		}
		catch (SecurityException)
		{
		}
		return false;
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	private string GetDebuggerDisplay()
	{
		return Name;
	}
}
