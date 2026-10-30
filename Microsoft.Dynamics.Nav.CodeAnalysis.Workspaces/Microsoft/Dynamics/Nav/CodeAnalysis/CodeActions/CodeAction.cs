using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;

public abstract class CodeAction
{
	internal abstract class SimpleCodeAction : CodeAction
	{
		public sealed override string Title { get; }

		public sealed override string EquivalenceKey { get; }

		public SimpleCodeAction(string title, string equivalenceKey)
		{
			Title = title;
			EquivalenceKey = equivalenceKey;
		}

		protected override Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
		{
			return Task.FromResult<Document>(null);
		}
	}

	internal class CodeActionWithNestedActions : SimpleCodeAction
	{
		public sealed override ImmutableArray<CodeAction> NestedCodeActions { get; }

		public CodeActionWithNestedActions(string title, ImmutableArray<CodeAction> nestedActions)
			: base(title, ComputeEquivalenceKey(nestedActions))
		{
			NestedCodeActions = nestedActions;
		}

		private static string ComputeEquivalenceKey(ImmutableArray<CodeAction> nestedActions)
		{
			StringBuilder stringBuilder = StringBuilderPool.Allocate();
			try
			{
				foreach (CodeAction item in nestedActions)
				{
					stringBuilder.Append((item.EquivalenceKey ?? item.GetHashCode().ToString(CultureInfo.InvariantCulture)) + ";");
				}
				return (stringBuilder.Length > 0) ? stringBuilder.ToString() : null;
			}
			finally
			{
				StringBuilderPool.ReturnAndFree(stringBuilder);
			}
		}
	}

	internal class DocumentChangeAction : SimpleCodeAction
	{
		private readonly Func<CancellationToken, Task<Document>> createChangedDocument;

		public DocumentChangeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey = null)
			: base(title, equivalenceKey)
		{
			this.createChangedDocument = createChangedDocument;
		}

		protected override Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
		{
			return createChangedDocument(cancellationToken);
		}
	}

	internal class SolutionChangeAction : SimpleCodeAction
	{
		private readonly Func<CancellationToken, Task<Solution>> createChangedSolution;

		public SolutionChangeAction(string title, Func<CancellationToken, Task<Solution>> createChangedSolution, string equivalenceKey = null)
			: base(title, equivalenceKey)
		{
			this.createChangedSolution = createChangedSolution;
		}

		protected override Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken)
		{
			return createChangedSolution(cancellationToken);
		}
	}

	public abstract string Title { get; }

	internal virtual string Message => Title;

	public virtual string EquivalenceKey => null;

	internal virtual CodeActionPriority Priority => CodeActionPriority.Medium;

	public virtual ImmutableArray<string> Tags => ImmutableArray<string>.Empty;

	public virtual ImmutableArray<CodeAction> NestedCodeActions => ImmutableArray<CodeAction>.Empty;

	internal virtual bool PerformFinalApplicabilityCheck => false;

	public Task<ImmutableArray<CodeActionOperation>> GetOperationsAsync(CancellationToken cancellationToken)
	{
		return GetOperationsAsync(new StreamingProgressTracker(), cancellationToken);
	}

	internal Task<ImmutableArray<CodeActionOperation>> GetOperationsAsync(IStreamingProgressTracker progressTracker, CancellationToken cancellationToken)
	{
		return GetOperationsCoreAsync(progressTracker, cancellationToken);
	}

	internal virtual async Task<ImmutableArray<CodeActionOperation>> GetOperationsCoreAsync(IStreamingProgressTracker progressTracker, CancellationToken cancellationToken)
	{
		ImmutableArray<CodeActionOperation> immutableArray = await ComputeOperationsAsync(progressTracker, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (immutableArray != null)
		{
			return await PostProcessAsync(immutableArray, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return ImmutableArray<CodeActionOperation>.Empty;
	}

	protected virtual async Task<IEnumerable<CodeActionOperation>> ComputeOperationsAsync(CancellationToken cancellationToken)
	{
		Solution solution = await GetChangedSolutionAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (solution == null)
		{
			return null;
		}
		return new CodeActionOperation[1]
		{
			new ApplyChangesOperation(solution)
		};
	}

	internal virtual async Task<ImmutableArray<CodeActionOperation>> ComputeOperationsAsync(IStreamingProgressTracker progressTracker, CancellationToken cancellationToken)
	{
		return (await ComputeOperationsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToImmutableArrayOrEmpty();
	}

	protected virtual async Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken)
	{
		return (await GetChangedDocumentAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))?.Project.Solution;
	}

	internal virtual Task<Solution> GetChangedSolutionAsync(IStreamingProgressTracker progressTracker, CancellationToken cancellationToken)
	{
		return GetChangedSolutionAsync(cancellationToken);
	}

	protected virtual Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	protected async Task<ImmutableArray<CodeActionOperation>> PostProcessAsync(IEnumerable<CodeActionOperation> operations, CancellationToken cancellationToken)
	{
		ArrayBuilder<CodeActionOperation> arrayBuilder = new ArrayBuilder<CodeActionOperation>();
		foreach (CodeActionOperation operation in operations)
		{
			if (operation is ApplyChangesOperation applyChangesOperation)
			{
				ArrayBuilder<CodeActionOperation> arrayBuilder2 = arrayBuilder;
				arrayBuilder2.Add(new ApplyChangesOperation(await PostProcessChangesAsync(applyChangesOperation.ChangedSolution, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)));
			}
			else
			{
				arrayBuilder.Add(operation);
			}
		}
		return arrayBuilder.ToImmutableAndFree();
	}

	protected async Task<Solution> PostProcessChangesAsync(Solution changedSolution, CancellationToken cancellationToken)
	{
		SolutionChanges solutionChanges = changedSolution.GetChanges(changedSolution.Workspace.CurrentSolution);
		Solution solution = changedSolution;
		foreach (ProjectChanges projectChange in solutionChanges.GetProjectChanges())
		{
			IEnumerable<DocumentId> enumerable = projectChange.GetChangedDocuments().Concat(projectChange.GetAddedDocuments());
			foreach (DocumentId item in enumerable)
			{
				Document document = solution.GetDocument(item);
				solution = (await PostProcessChangesAsync(document, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Project.Solution;
			}
		}
		foreach (Project addedProject in solutionChanges.GetAddedProjects())
		{
			IEnumerable<DocumentId> documentIds = addedProject.DocumentIds;
			foreach (DocumentId item2 in documentIds)
			{
				Document document2 = solution.GetDocument(item2);
				solution = (await PostProcessChangesAsync(document2, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Project.Solution;
			}
		}
		return solution;
	}

	protected virtual Task<Document> PostProcessChangesAsync(Document document, CancellationToken cancellationToken)
	{
		return CleanupDocumentAsync(document, cancellationToken);
	}

	internal static async Task<Document> CleanupDocumentAsync(Document document, CancellationToken cancellationToken)
	{
		if (document.SupportsSyntaxTree)
		{
			document = await Formatter.FormatAsync(document, Formatter.Annotation, null, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			document = await Formatter.FormatAsync(document, SyntaxAnnotation.ElasticAnnotation, null, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return document;
	}

	internal virtual bool IsApplicable(Workspace workspace)
	{
		return true;
	}

	public static CodeAction Create(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey = null)
	{
		if (title == null)
		{
			throw new ArgumentNullException("title");
		}
		if (createChangedDocument == null)
		{
			throw new ArgumentNullException("createChangedDocument");
		}
		return new DocumentChangeAction(title, createChangedDocument, equivalenceKey);
	}

	public static CodeAction Create(string title, Func<CancellationToken, Task<Solution>> createChangedSolution, string equivalenceKey = null)
	{
		if (title == null)
		{
			throw new ArgumentNullException("title");
		}
		if (createChangedSolution == null)
		{
			throw new ArgumentNullException("createChangedSolution");
		}
		return new SolutionChangeAction(title, createChangedSolution, equivalenceKey);
	}
}
