using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
public class Document : TextDocument
{
	private WeakReference<SemanticModel> model;

	private Task<SyntaxTree> syntaxTreeResultTask;

	internal DocumentState State { get; }

	public bool SupportsSyntaxTree => State.SupportsSyntaxTree;

	public bool SupportsSemanticModel
	{
		get
		{
			if (SupportsSyntaxTree)
			{
				return Project.SupportsCompilation;
			}
			return false;
		}
	}

	internal Document(Project project, DocumentState state)
	{
		Contract.ThrowIfNull(project);
		Contract.ThrowIfNull(state);
		Project = project;
		State = state;
	}

	internal override TextDocumentState GetDocumentState()
	{
		return State;
	}

	public bool TryGetSyntaxTree(out SyntaxTree syntaxTree)
	{
		if (syntaxTreeResultTask != null)
		{
			syntaxTree = syntaxTreeResultTask.Result;
		}
		if (!State.TryGetSyntaxTree(out syntaxTree))
		{
			return false;
		}
		if (syntaxTreeResultTask == null)
		{
			Task<SyntaxTree> value = Task.FromResult(syntaxTree);
			Interlocked.CompareExchange(ref syntaxTreeResultTask, value, null);
		}
		return true;
	}

	public bool TryGetSyntaxVersion(out VersionStamp version)
	{
		version = default;
		if (!TryGetTextVersion(out var version2))
		{
			return false;
		}
		VersionStamp version3 = Project.Version;
		version = version2.GetNewerVersion(version3);
		return true;
	}

	internal bool TryGetTopLevelChangeTextVersion(out VersionStamp version)
	{
		return State.TryGetTopLevelChangeTextVersion(out version);
	}

	public async Task<VersionStamp> GetSyntaxVersionAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		VersionStamp versionStamp = await GetTextVersionAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		VersionStamp version = Project.Version;
		return versionStamp.GetNewerVersion(version);
	}

	public Task<SyntaxTree> GetSyntaxTreeAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!SupportsSyntaxTree)
		{
			return SpecializedTasks.Default<SyntaxTree>();
		}
		if (syntaxTreeResultTask != null)
		{
			return syntaxTreeResultTask;
		}
		if (TryGetSemanticModel(out var semanticModel))
		{
			Task<SyntaxTree> value = Task.FromResult(semanticModel.SyntaxTree);
			Interlocked.CompareExchange(ref syntaxTreeResultTask, value, null);
			return syntaxTreeResultTask;
		}
		if (TryGetSyntaxTree(out var syntaxTree))
		{
			if (syntaxTreeResultTask == null)
			{
				Task<SyntaxTree> value2 = Task.FromResult(syntaxTree);
				Interlocked.CompareExchange(ref syntaxTreeResultTask, value2, null);
			}
			return syntaxTreeResultTask;
		}
		return State.GetSyntaxTreeAsync(cancellationToken);
	}

	public bool TryGetSyntaxRoot(out SyntaxNode root)
	{
		root = null;
		if (TryGetSyntaxTree(out var syntaxTree) && syntaxTree.TryGetRoot(out root))
		{
			return root != null;
		}
		return false;
	}

	public async Task<SyntaxNode> GetSyntaxRootAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!SupportsSyntaxTree)
		{
			return null;
		}
		return await (await GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public bool TryGetSemanticModel(out SemanticModel semanticModel)
	{
		semanticModel = null;
		if (model != null)
		{
			return model.TryGetTarget(out semanticModel);
		}
		return false;
	}

	public async Task<SemanticModel> GetSemanticModelAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		try
		{
			if (!SupportsSemanticModel)
			{
				return null;
			}
			if (TryGetSemanticModel(out var semanticModel))
			{
				return semanticModel;
			}
			SyntaxTree syntaxTree = await GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			SemanticModel semanticModel2 = (await Project.GetCompilationAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetSemanticModel(syntaxTree);
			Contract.ThrowIfNull(semanticModel2);
			WeakReference<SemanticModel> weakReference = Interlocked.CompareExchange(ref model, new WeakReference<SemanticModel>(semanticModel2), null);
			if (weakReference == null)
			{
				return semanticModel2;
			}
			if (weakReference.TryGetTarget(out semanticModel))
			{
				return semanticModel;
			}
			weakReference.SetTarget(semanticModel2);
			return semanticModel2;
		}
		catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	public Document WithText(SourceText text)
	{
		return Project.Solution.WithDocumentText(Id, text, PreservationMode.PreserveIdentity).GetDocument(Id);
	}

	public Document WithSyntaxRoot(SyntaxNode root)
	{
		return Project.Solution.WithDocumentSyntaxRoot(Id, root, PreservationMode.PreserveIdentity).GetDocument(Id);
	}

	[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
	public async Task<IEnumerable<TextChange>> GetTextChangesAsync(Document oldDocument, CancellationToken cancellationToken = default(CancellationToken))
	{
		try
		{
			using (Logger.LogBlock(FunctionId.Workspace_Document_GetTextChanges, Name, cancellationToken))
			{
				if (oldDocument == this)
				{
					return SpecializedCollections.EmptyEnumerable<TextChange>();
				}
				if (Id != oldDocument.Id)
				{
					throw new ArgumentException(WorkspacesResources.DocumentVersionIsDifferent);
				}
				if (TryGetText(out var text) && oldDocument.TryGetText(out var text2))
				{
					if (text == text2)
					{
						return SpecializedCollections.EmptyEnumerable<TextChange>();
					}
					if (text.Container != null)
					{
						IList<TextChange> list = text.GetTextChanges(text2).ToList();
						if (list.Count > 1 || (list.Count == 1 && list[0].Span != new TextSpan(0, text2.Length)))
						{
							return list;
						}
					}
				}
				if (SupportsSyntaxTree)
				{
					return (await GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetChanges(await oldDocument.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
				}
				return (await GetTextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetTextChanges(await oldDocument.GetTextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToList();
			}
		}
		catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public ImmutableArray<DocumentId> GetLinkedDocumentIds()
	{
		return Project.Solution.GetDocumentIdsWithFilePath(FilePath).Remove(Id);
	}

	internal async Task<Document> WithFrozenPartialSemanticsAsync(CancellationToken cancellationToken)
	{
		Solution solution = Project.Solution;
		Workspace workspace = solution.Workspace;
		if (solution.BranchId == workspace.PrimaryBranchId && workspace.PartialSemanticsEnabled && Project.SupportsCompilation)
		{
			return (await Project.Solution.WithFrozenPartialCompilationIncludingSpecificDocumentAsync(Id, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetDocument(Id);
		}
		return this;
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	private string GetDebuggerDisplay()
	{
		return Name;
	}
}
