using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Classifiers;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Classification;

internal abstract class AbstractClassificationService : IClassificationService, ILanguageService
{
	private struct Worker
	{
		private readonly SemanticModel semanticModel;

		private readonly SyntaxTree syntaxTree;

		private readonly TextSpan textSpan;

		private readonly List<ClassifiedSpan> list;

		private readonly CancellationToken cancellationToken;

		private readonly Func<SyntaxNode, List<ISyntaxClassifier>> getNodeClassifiers;

		private readonly Func<SyntaxToken, List<ISyntaxClassifier>> getTokenClassifiers;

		private readonly HashSet<ClassifiedSpan> set;

		private readonly Stack<SyntaxNodeOrToken> pendingNodes;

		private Worker(SemanticModel semanticModel, TextSpan textSpan, List<ClassifiedSpan> list, Func<SyntaxNode, List<ISyntaxClassifier>> getNodeClassifiers, Func<SyntaxToken, List<ISyntaxClassifier>> getTokenClassifiers, CancellationToken cancellationToken)
		{
			this.getNodeClassifiers = getNodeClassifiers;
			this.getTokenClassifiers = getTokenClassifiers;
			this.semanticModel = semanticModel;
			syntaxTree = semanticModel.SyntaxTree;
			this.textSpan = textSpan;
			this.list = list;
			this.cancellationToken = cancellationToken;
			set = SharedPools.Default<HashSet<ClassifiedSpan>>().AllocateAndClear();
			pendingNodes = SharedPools.Default<Stack<SyntaxNodeOrToken>>().AllocateAndClear();
		}

		internal static void Classify(Workspace workspace, SemanticModel semanticModel, TextSpan textSpan, List<ClassifiedSpan> list, Func<SyntaxNode, List<ISyntaxClassifier>> getNodeClassifiers, Func<SyntaxToken, List<ISyntaxClassifier>> getTokenClassifiers, CancellationToken cancellationToken)
		{
			Worker worker = new Worker(semanticModel, textSpan, list, getNodeClassifiers, getTokenClassifiers, cancellationToken);
			try
			{
				worker.pendingNodes.Push(worker.syntaxTree.GetRoot(cancellationToken));
				worker.ProcessNodes();
			}
			finally
			{
				SharedPools.Default<HashSet<ClassifiedSpan>>().ClearAndFree(worker.set);
				SharedPools.Default<Stack<SyntaxNodeOrToken>>().ClearAndFree(worker.pendingNodes);
			}
		}

		private void AddClassification(TextSpan span, string type)
		{
			ClassifiedSpan item = new ClassifiedSpan(type, span);
			if (!set.Contains(item))
			{
				list.Add(item);
				set.Add(item);
			}
		}

		private void ProcessNodes()
		{
			while (pendingNodes.Count > 0)
			{
				cancellationToken.ThrowIfCancellationRequested();
				SyntaxNodeOrToken nodeOrToken = pendingNodes.Pop();
				if (nodeOrToken.Span.IntersectsWith(textSpan))
				{
					ClassifyNodeOrToken(nodeOrToken);
					foreach (SyntaxNodeOrToken item in nodeOrToken.ChildNodesAndTokens())
					{
						pendingNodes.Push(item);
					}
				}
			}
		}

		private void ClassifyNodeOrToken(SyntaxNodeOrToken nodeOrToken)
		{
			SyntaxNode syntaxNode = nodeOrToken.AsNode();
			if (syntaxNode != null)
			{
				ClassifyNode(syntaxNode);
			}
			else
			{
				ClassifyToken(nodeOrToken.AsToken());
			}
		}

		private void ClassifyNode(SyntaxNode syntax)
		{
			foreach (ISyntaxClassifier item in getNodeClassifiers(syntax))
			{
				cancellationToken.ThrowIfCancellationRequested();
				IEnumerable<ClassifiedSpan> classifications = item.ClassifyNode(syntax, semanticModel, cancellationToken);
				AddClassifications(classifications);
			}
		}

		private void AddClassifications(IEnumerable<ClassifiedSpan> classifications)
		{
			if (classifications == null)
			{
				return;
			}
			foreach (ClassifiedSpan classification in classifications)
			{
				AddClassification(classification);
			}
		}

		private void AddClassification(ClassifiedSpan classification)
		{
			if (classification.ClassificationType != null)
			{
				AddClassification(classification.TextSpan, classification.ClassificationType);
			}
		}

		private void ClassifyToken(SyntaxToken syntax)
		{
			ClassifyStructuredTrivia(syntax.LeadingTrivia);
			foreach (ISyntaxClassifier item in getTokenClassifiers(syntax))
			{
				cancellationToken.ThrowIfCancellationRequested();
				IEnumerable<ClassifiedSpan> classifications = item.ClassifyToken(syntax, semanticModel, cancellationToken);
				AddClassifications(classifications);
			}
			ClassifyStructuredTrivia(syntax.TrailingTrivia);
		}

		private void ClassifyStructuredTrivia(SyntaxTriviaList triviaList)
		{
			foreach (SyntaxTrivia item in triviaList)
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (item.HasStructure)
				{
					pendingNodes.Push(item.GetStructure());
				}
			}
		}
	}

	public abstract void AddLexicalClassifications(SourceText text, TextSpan textSpan, List<ClassifiedSpan> result, CancellationToken cancellationToken);

	public abstract void AddSyntacticClassifications(SyntaxTree syntaxTree, TextSpan textSpan, List<ClassifiedSpan> result, CancellationToken cancellationToken);

	public abstract IEnumerable<ISyntaxClassifier> GetDefaultSyntaxClassifiers();

	public abstract ClassifiedSpan FixClassification(SourceText text, ClassifiedSpan classifiedSpan);

	public async Task AddSemanticClassificationsAsync(Document document, TextSpan textSpan, Func<SyntaxNode, List<ISyntaxClassifier>> getNodeClassifiers, Func<SyntaxToken, List<ISyntaxClassifier>> getTokenClassifiers, List<ClassifiedSpan> result, CancellationToken cancellationToken)
	{
		try
		{
			AddSemanticClassifications(await document.GetSemanticModelForSpanAsync(textSpan, cancellationToken).ConfigureAwait(continueOnCapturedContext: false), textSpan, document.Project.Solution.Workspace, getNodeClassifiers, getTokenClassifiers, result, cancellationToken);
		}
		catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	public void AddSemanticClassifications(SemanticModel semanticModel, TextSpan textSpan, Workspace workspace, Func<SyntaxNode, List<ISyntaxClassifier>> getNodeClassifiers, Func<SyntaxToken, List<ISyntaxClassifier>> getTokenClassifiers, List<ClassifiedSpan> result, CancellationToken cancellationToken = default(CancellationToken))
	{
		Worker.Classify(workspace, semanticModel, textSpan, result, getNodeClassifiers, getTokenClassifiers, cancellationToken);
	}
}
