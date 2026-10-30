using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

internal static class SyntaxReplacer
{
	private class Replacer<TNode> : SyntaxRewriter where TNode : SyntaxNode
	{
		private readonly Func<TNode, TNode, SyntaxNode> computeReplacementNode;

		private readonly Func<SyntaxToken, SyntaxToken, SyntaxToken> computeReplacementToken;

		private readonly Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia> computeReplacementTrivia;

		private readonly HashSet<SyntaxNode> nodeSet;

		private readonly HashSet<SyntaxToken> tokenSet;

		private readonly HashSet<SyntaxTrivia> triviaSet;

		private readonly HashSet<TextSpan> spanSet;

		private readonly TextSpan totalSpan;

		private readonly bool visitIntoStructuredTrivia;

		private readonly bool shouldVisitTrivia;

		private static readonly HashSet<SyntaxNode> noNodesSet = new HashSet<SyntaxNode>();

		private static readonly HashSet<SyntaxToken> noTokensSet = new HashSet<SyntaxToken>();

		private static readonly HashSet<SyntaxTrivia> noTriviaSet = new HashSet<SyntaxTrivia>();

		public override bool VisitIntoStructuredTrivia => visitIntoStructuredTrivia;

		public bool HasWork => nodeSet.Count + tokenSet.Count + triviaSet.Count > 0;

		public Replacer(IEnumerable<TNode> nodes, Func<TNode, TNode, SyntaxNode> computeReplacementNode, IEnumerable<SyntaxToken> tokens, Func<SyntaxToken, SyntaxToken, SyntaxToken> computeReplacementToken, IEnumerable<SyntaxTrivia> trivia, Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia> computeReplacementTrivia)
		{
			this.computeReplacementNode = computeReplacementNode;
			this.computeReplacementToken = computeReplacementToken;
			this.computeReplacementTrivia = computeReplacementTrivia;
			nodeSet = ((nodes != null) ? new HashSet<SyntaxNode>(nodes) : noNodesSet);
			tokenSet = ((tokens != null) ? new HashSet<SyntaxToken>(tokens) : noTokensSet);
			triviaSet = ((trivia != null) ? new HashSet<SyntaxTrivia>(trivia) : noTriviaSet);
			spanSet = new HashSet<TextSpan>(nodeSet.Select((SyntaxNode n) => n.FullSpan).Concat(tokenSet.Select((SyntaxToken t) => t.FullSpan).Concat(triviaSet.Select((SyntaxTrivia t) => t.FullSpan))));
			totalSpan = ComputeTotalSpan(spanSet);
			visitIntoStructuredTrivia = nodeSet.Any((SyntaxNode n) => n.IsPartOfStructuredTrivia()) || tokenSet.Any((SyntaxToken t) => t.IsPartOfStructuredTrivia()) || triviaSet.Any((SyntaxTrivia t) => t.IsPartOfStructuredTrivia());
			shouldVisitTrivia = triviaSet.Count > 0 || visitIntoStructuredTrivia;
		}

		private static TextSpan ComputeTotalSpan(IEnumerable<TextSpan> spans)
		{
			bool flag = true;
			int num = 0;
			int num2 = 0;
			foreach (TextSpan span in spans)
			{
				if (flag)
				{
					num = span.Start;
					num2 = span.End;
					flag = false;
				}
				else
				{
					num = Math.Min(num, span.Start);
					num2 = Math.Max(num2, span.End);
				}
			}
			return new TextSpan(num, num2 - num);
		}

		private bool ShouldVisit(TextSpan span)
		{
			if (!span.IntersectsWith(totalSpan))
			{
				return false;
			}
			foreach (TextSpan item in spanSet)
			{
				if (span.IntersectsWith(item))
				{
					return true;
				}
			}
			return false;
		}

		public override SyntaxNode Visit(SyntaxNode node)
		{
			SyntaxNode syntaxNode = node;
			if (node != null)
			{
				if (ShouldVisit(node.FullSpan))
				{
					syntaxNode = base.Visit(node);
				}
				if (nodeSet.Contains(node) && computeReplacementNode != null)
				{
					syntaxNode = computeReplacementNode((TNode)node, (TNode)syntaxNode);
				}
			}
			return syntaxNode;
		}

		public override SyntaxToken VisitToken(SyntaxToken token)
		{
			SyntaxToken syntaxToken = token;
			if (shouldVisitTrivia && ShouldVisit(token.FullSpan))
			{
				syntaxToken = base.VisitToken(token);
			}
			if (tokenSet.Contains(token) && computeReplacementToken != null)
			{
				syntaxToken = computeReplacementToken(token, syntaxToken);
			}
			return syntaxToken;
		}

		public override SyntaxTrivia VisitListElement(SyntaxTrivia trivia)
		{
			SyntaxTrivia syntaxTrivia = trivia;
			if (VisitIntoStructuredTrivia && trivia.HasStructure && ShouldVisit(trivia.FullSpan))
			{
				syntaxTrivia = VisitTrivia(trivia);
			}
			if (triviaSet.Contains(trivia) && computeReplacementTrivia != null)
			{
				syntaxTrivia = computeReplacementTrivia(trivia, syntaxTrivia);
			}
			return syntaxTrivia;
		}
	}

	private enum ListEditKind
	{
		InsertBefore,
		InsertAfter,
		Replace
	}

	private abstract class BaseListEditor : SyntaxRewriter
	{
		private readonly TextSpan elementSpan;

		private readonly bool visitTrivia;

		protected readonly ListEditKind EditKind;

		public override bool VisitIntoStructuredTrivia { get; }

		protected BaseListEditor(TextSpan elementSpan, ListEditKind editKind, bool visitTrivia, bool visitIntoStructuredTrivia)
		{
			this.elementSpan = elementSpan;
			EditKind = editKind;
			this.visitTrivia = visitTrivia | visitIntoStructuredTrivia;
			VisitIntoStructuredTrivia = visitIntoStructuredTrivia;
		}

		private bool ShouldVisit(TextSpan span)
		{
			if (span.IntersectsWith(elementSpan))
			{
				return true;
			}
			return false;
		}

		public override SyntaxNode Visit(SyntaxNode node)
		{
			SyntaxNode result = node;
			if (node != null && ShouldVisit(node.FullSpan))
			{
				result = base.Visit(node);
			}
			return result;
		}

		public override SyntaxToken VisitToken(SyntaxToken token)
		{
			SyntaxToken result = token;
			if (visitTrivia && ShouldVisit(token.FullSpan))
			{
				result = base.VisitToken(token);
			}
			return result;
		}

		public override SyntaxTrivia VisitListElement(SyntaxTrivia trivia)
		{
			SyntaxTrivia result = trivia;
			if (VisitIntoStructuredTrivia && trivia.HasStructure && ShouldVisit(trivia.FullSpan))
			{
				result = VisitTrivia(trivia);
			}
			return result;
		}
	}

	private class NodeListEditor : BaseListEditor
	{
		private readonly SyntaxNode originalNode;

		private readonly IEnumerable<SyntaxNode> newNodes;

		public NodeListEditor(SyntaxNode originalNode, IEnumerable<SyntaxNode> replacementNodes, ListEditKind editKind)
			: base(originalNode.Span, editKind, visitTrivia: false, originalNode.IsPartOfStructuredTrivia())
		{
			this.originalNode = originalNode;
			newNodes = replacementNodes;
		}

		public override SyntaxNode Visit(SyntaxNode node)
		{
			if (node == originalNode)
			{
				throw GetItemNotListElementException();
			}
			return base.Visit(node);
		}

		public override SeparatedSyntaxList<TNode> VisitList<TNode>(SeparatedSyntaxList<TNode> list)
		{
			if (originalNode is TNode)
			{
				int num = list.IndexOf((TNode)originalNode);
				if (num >= 0 && num < list.Count)
				{
					switch (EditKind)
					{
					case ListEditKind.Replace:
						return list.ReplaceRange((TNode)originalNode, newNodes.Cast<TNode>());
					case ListEditKind.InsertAfter:
						return list.InsertRange(num + 1, newNodes.Cast<TNode>());
					case ListEditKind.InsertBefore:
						return list.InsertRange(num, newNodes.Cast<TNode>());
					}
				}
			}
			return base.VisitList(list);
		}

		public override SyntaxList<TNode> VisitList<TNode>(SyntaxList<TNode> list)
		{
			if (originalNode is TNode)
			{
				int num = list.IndexOf((TNode)originalNode);
				if (num >= 0 && num < list.Count)
				{
					switch (EditKind)
					{
					case ListEditKind.Replace:
						return list.ReplaceRange((TNode)originalNode, newNodes.Cast<TNode>());
					case ListEditKind.InsertAfter:
						return list.InsertRange(num + 1, newNodes.Cast<TNode>());
					case ListEditKind.InsertBefore:
						return list.InsertRange(num, newNodes.Cast<TNode>());
					}
				}
			}
			return base.VisitList(list);
		}
	}

	private class TokenListEditor : BaseListEditor
	{
		private readonly SyntaxToken originalToken;

		private readonly IEnumerable<SyntaxToken> newTokens;

		public TokenListEditor(SyntaxToken originalToken, IEnumerable<SyntaxToken> newTokens, ListEditKind editKind)
			: base(originalToken.Span, editKind, visitTrivia: false, originalToken.IsPartOfStructuredTrivia())
		{
			this.originalToken = originalToken;
			this.newTokens = newTokens;
		}

		public override SyntaxToken VisitToken(SyntaxToken token)
		{
			if (token == originalToken)
			{
				throw GetItemNotListElementException();
			}
			return base.VisitToken(token);
		}

		public override SyntaxTokenList VisitList(SyntaxTokenList list)
		{
			int num = list.IndexOf(originalToken);
			if (num >= 0 && num < list.Count)
			{
				switch (EditKind)
				{
				case ListEditKind.Replace:
					return list.ReplaceRange(originalToken, newTokens);
				case ListEditKind.InsertAfter:
					return list.InsertRange(num + 1, newTokens);
				case ListEditKind.InsertBefore:
					return list.InsertRange(num, newTokens);
				}
			}
			return base.VisitList(list);
		}
	}

	private class TriviaListEditor : BaseListEditor
	{
		private readonly SyntaxTrivia originalTrivia;

		private readonly IEnumerable<SyntaxTrivia> newTrivia;

		public TriviaListEditor(SyntaxTrivia originalTrivia, IEnumerable<SyntaxTrivia> newTrivia, ListEditKind editKind)
			: base(originalTrivia.Span, editKind, visitTrivia: true, originalTrivia.IsPartOfStructuredTrivia())
		{
			this.originalTrivia = originalTrivia;
			this.newTrivia = newTrivia;
		}

		public override SyntaxTriviaList VisitList(SyntaxTriviaList list)
		{
			int num = list.IndexOf(originalTrivia);
			if (num >= 0 && num < list.Count)
			{
				switch (EditKind)
				{
				case ListEditKind.Replace:
					return list.ReplaceRange(originalTrivia, newTrivia);
				case ListEditKind.InsertAfter:
					return list.InsertRange(num + 1, newTrivia);
				case ListEditKind.InsertBefore:
					return list.InsertRange(num, newTrivia);
				}
			}
			return base.VisitList(list);
		}
	}

	internal static SyntaxNode Replace<TNode>(SyntaxNode root, IEnumerable<TNode> nodes = null, Func<TNode, TNode, SyntaxNode> computeReplacementNode = null, IEnumerable<SyntaxToken> tokens = null, Func<SyntaxToken, SyntaxToken, SyntaxToken> computeReplacementToken = null, IEnumerable<SyntaxTrivia> trivia = null, Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia> computeReplacementTrivia = null) where TNode : SyntaxNode
	{
		Replacer<TNode> replacer = new Replacer<TNode>(nodes, computeReplacementNode, tokens, computeReplacementToken, trivia, computeReplacementTrivia);
		if (replacer.HasWork)
		{
			return replacer.Visit(root);
		}
		return root;
	}

	internal static SyntaxToken Replace(SyntaxToken root, IEnumerable<SyntaxNode> nodes = null, Func<SyntaxNode, SyntaxNode, SyntaxNode> computeReplacementNode = null, IEnumerable<SyntaxToken> tokens = null, Func<SyntaxToken, SyntaxToken, SyntaxToken> computeReplacementToken = null, IEnumerable<SyntaxTrivia> trivia = null, Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia> computeReplacementTrivia = null)
	{
		Replacer<SyntaxNode> replacer = new Replacer<SyntaxNode>(nodes, computeReplacementNode, tokens, computeReplacementToken, trivia, computeReplacementTrivia);
		if (replacer.HasWork)
		{
			return replacer.VisitToken(root);
		}
		return root;
	}

	internal static SyntaxNode ReplaceNodeInList(SyntaxNode root, SyntaxNode originalNode, IEnumerable<SyntaxNode> newNodes)
	{
		return new NodeListEditor(originalNode, newNodes, ListEditKind.Replace).Visit(root);
	}

	internal static SyntaxNode InsertNodeInList(SyntaxNode root, SyntaxNode nodeInList, IEnumerable<SyntaxNode> nodesToInsert, bool insertBefore)
	{
		return new NodeListEditor(nodeInList, nodesToInsert, (!insertBefore) ? ListEditKind.InsertAfter : ListEditKind.InsertBefore).Visit(root);
	}

	public static SyntaxNode ReplaceTokenInList(SyntaxNode root, SyntaxToken tokenInList, IEnumerable<SyntaxToken> newTokens)
	{
		return new TokenListEditor(tokenInList, newTokens, ListEditKind.Replace).Visit(root);
	}

	public static SyntaxNode InsertTokenInList(SyntaxNode root, SyntaxToken tokenInList, IEnumerable<SyntaxToken> newTokens, bool insertBefore)
	{
		return new TokenListEditor(tokenInList, newTokens, (!insertBefore) ? ListEditKind.InsertAfter : ListEditKind.InsertBefore).Visit(root);
	}

	public static SyntaxNode ReplaceTriviaInList(SyntaxNode root, SyntaxTrivia triviaInList, IEnumerable<SyntaxTrivia> newTrivia)
	{
		return new TriviaListEditor(triviaInList, newTrivia, ListEditKind.Replace).Visit(root);
	}

	public static SyntaxNode InsertTriviaInList(SyntaxNode root, SyntaxTrivia triviaInList, IEnumerable<SyntaxTrivia> newTrivia, bool insertBefore)
	{
		return new TriviaListEditor(triviaInList, newTrivia, (!insertBefore) ? ListEditKind.InsertAfter : ListEditKind.InsertBefore).Visit(root);
	}

	public static SyntaxToken ReplaceTriviaInList(SyntaxToken root, SyntaxTrivia triviaInList, IEnumerable<SyntaxTrivia> newTrivia)
	{
		return new TriviaListEditor(triviaInList, newTrivia, ListEditKind.Replace).VisitToken(root);
	}

	public static SyntaxToken InsertTriviaInList(SyntaxToken root, SyntaxTrivia triviaInList, IEnumerable<SyntaxTrivia> newTrivia, bool insertBefore)
	{
		return new TriviaListEditor(triviaInList, newTrivia, (!insertBefore) ? ListEditKind.InsertAfter : ListEditKind.InsertBefore).VisitToken(root);
	}

	private static InvalidOperationException GetItemNotListElementException()
	{
		return new InvalidOperationException(CodeAnalysisResources.MissingListItem);
	}
}
