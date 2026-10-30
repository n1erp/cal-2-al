using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class SyntaxReplacer
{
	private class Replacer<TNode> : CSharpSyntaxRewriter where TNode : SyntaxNode
	{
		private readonly Func<TNode, TNode, SyntaxNode> _computeReplacementNode;

		private readonly Func<SyntaxToken, SyntaxToken, SyntaxToken> _computeReplacementToken;

		private readonly Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia> _computeReplacementTrivia;

		private readonly HashSet<SyntaxNode> _nodeSet;

		private readonly HashSet<SyntaxToken> _tokenSet;

		private readonly HashSet<SyntaxTrivia> _triviaSet;

		private readonly HashSet<TextSpan> _spanSet;

		private readonly TextSpan _totalSpan;

		private readonly bool _visitIntoStructuredTrivia;

		private readonly bool _shouldVisitTrivia;

		private static readonly HashSet<SyntaxNode> s_noNodes = new HashSet<SyntaxNode>();

		private static readonly HashSet<SyntaxToken> s_noTokens = new HashSet<SyntaxToken>();

		private static readonly HashSet<SyntaxTrivia> s_noTrivia = new HashSet<SyntaxTrivia>();

		public override bool VisitIntoStructuredTrivia => _visitIntoStructuredTrivia;

		public bool HasWork => _nodeSet.Count + _tokenSet.Count + _triviaSet.Count > 0;

		public Replacer(IEnumerable<TNode> nodes, Func<TNode, TNode, SyntaxNode> computeReplacementNode, IEnumerable<SyntaxToken> tokens, Func<SyntaxToken, SyntaxToken, SyntaxToken> computeReplacementToken, IEnumerable<SyntaxTrivia> trivia, Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia> computeReplacementTrivia)
		{
			_computeReplacementNode = computeReplacementNode;
			_computeReplacementToken = computeReplacementToken;
			_computeReplacementTrivia = computeReplacementTrivia;
			_nodeSet = ((nodes != null) ? new HashSet<SyntaxNode>(nodes) : s_noNodes);
			_tokenSet = ((tokens != null) ? new HashSet<SyntaxToken>(tokens) : s_noTokens);
			_triviaSet = ((trivia != null) ? new HashSet<SyntaxTrivia>(trivia) : s_noTrivia);
			_spanSet = new HashSet<TextSpan>(_nodeSet.Select((SyntaxNode n) => n.FullSpan).Concat(_tokenSet.Select((SyntaxToken t) => t.FullSpan).Concat(_triviaSet.Select((SyntaxTrivia t) => t.FullSpan))));
			_totalSpan = ComputeTotalSpan(_spanSet);
			_visitIntoStructuredTrivia = _nodeSet.Any((SyntaxNode n) => n.IsPartOfStructuredTrivia()) || _tokenSet.Any((SyntaxToken t) => t.IsPartOfStructuredTrivia()) || _triviaSet.Any((SyntaxTrivia t) => t.IsPartOfStructuredTrivia());
			_shouldVisitTrivia = _triviaSet.Count > 0 || _visitIntoStructuredTrivia;
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
			if (!span.IntersectsWith(_totalSpan))
			{
				return false;
			}
			foreach (TextSpan item in _spanSet)
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
				if (_nodeSet.Contains(node) && _computeReplacementNode != null)
				{
					syntaxNode = _computeReplacementNode((TNode)node, (TNode)syntaxNode);
				}
			}
			return syntaxNode;
		}

		public override SyntaxToken VisitToken(SyntaxToken token)
		{
			SyntaxToken syntaxToken = token;
			if (_shouldVisitTrivia && ShouldVisit(token.FullSpan))
			{
				syntaxToken = base.VisitToken(token);
			}
			if (_tokenSet.Contains(token) && _computeReplacementToken != null)
			{
				syntaxToken = _computeReplacementToken(token, syntaxToken);
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
			if (_triviaSet.Contains(trivia) && _computeReplacementTrivia != null)
			{
				syntaxTrivia = _computeReplacementTrivia(trivia, syntaxTrivia);
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

	private abstract class BaseListEditor : CSharpSyntaxRewriter
	{
		private readonly TextSpan _elementSpan;

		private readonly bool _visitTrivia;

		private readonly bool _visitIntoStructuredTrivia;

		protected readonly ListEditKind editKind;

		public override bool VisitIntoStructuredTrivia => _visitIntoStructuredTrivia;

		public BaseListEditor(TextSpan elementSpan, ListEditKind editKind, bool visitTrivia, bool visitIntoStructuredTrivia)
		{
			_elementSpan = elementSpan;
			this.editKind = editKind;
			_visitTrivia = visitTrivia | visitIntoStructuredTrivia;
			_visitIntoStructuredTrivia = visitIntoStructuredTrivia;
		}

		private bool ShouldVisit(TextSpan span)
		{
			if (span.IntersectsWith(_elementSpan))
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
			if (_visitTrivia && ShouldVisit(token.FullSpan))
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
		private readonly SyntaxNode _originalNode;

		private readonly IEnumerable<SyntaxNode> _newNodes;

		public NodeListEditor(SyntaxNode originalNode, IEnumerable<SyntaxNode> replacementNodes, ListEditKind editKind)
			: base(originalNode.Span, editKind, visitTrivia: false, originalNode.IsPartOfStructuredTrivia())
		{
			_originalNode = originalNode;
			_newNodes = replacementNodes;
		}

		public override SyntaxNode Visit(SyntaxNode node)
		{
			if (node == _originalNode)
			{
				throw GetItemNotListElementException();
			}
			return base.Visit(node);
		}

		public override SeparatedSyntaxList<TNode> VisitList<TNode>(SeparatedSyntaxList<TNode> list)
		{
			if (_originalNode is TNode)
			{
				int num = list.IndexOf((TNode)_originalNode);
				if (num >= 0 && num < list.Count)
				{
					switch (editKind)
					{
					case ListEditKind.Replace:
						return list.ReplaceRange((TNode)_originalNode, _newNodes.Cast<TNode>());
					case ListEditKind.InsertAfter:
						return list.InsertRange(num + 1, _newNodes.Cast<TNode>());
					case ListEditKind.InsertBefore:
						return list.InsertRange(num, _newNodes.Cast<TNode>());
					}
				}
			}
			return base.VisitList(list);
		}

		public override SyntaxList<TNode> VisitList<TNode>(SyntaxList<TNode> list)
		{
			if (_originalNode is TNode)
			{
				int num = list.IndexOf((TNode)_originalNode);
				if (num >= 0 && num < list.Count)
				{
					switch (editKind)
					{
					case ListEditKind.Replace:
						return list.ReplaceRange((TNode)_originalNode, _newNodes.Cast<TNode>());
					case ListEditKind.InsertAfter:
						return list.InsertRange(num + 1, _newNodes.Cast<TNode>());
					case ListEditKind.InsertBefore:
						return list.InsertRange(num, _newNodes.Cast<TNode>());
					}
				}
			}
			return base.VisitList(list);
		}
	}

	private class TokenListEditor : BaseListEditor
	{
		private readonly SyntaxToken _originalToken;

		private readonly IEnumerable<SyntaxToken> _newTokens;

		public TokenListEditor(SyntaxToken originalToken, IEnumerable<SyntaxToken> newTokens, ListEditKind editKind)
			: base(originalToken.Span, editKind, visitTrivia: false, originalToken.IsPartOfStructuredTrivia())
		{
			_originalToken = originalToken;
			_newTokens = newTokens;
		}

		public override SyntaxToken VisitToken(SyntaxToken token)
		{
			if (token == _originalToken)
			{
				throw GetItemNotListElementException();
			}
			return base.VisitToken(token);
		}

		public override SyntaxTokenList VisitList(SyntaxTokenList list)
		{
			int num = list.IndexOf(_originalToken);
			if (num >= 0 && num < list.Count)
			{
				switch (editKind)
				{
				case ListEditKind.Replace:
					return list.ReplaceRange(_originalToken, _newTokens);
				case ListEditKind.InsertAfter:
					return list.InsertRange(num + 1, _newTokens);
				case ListEditKind.InsertBefore:
					return list.InsertRange(num, _newTokens);
				}
			}
			return base.VisitList(list);
		}
	}

	private class TriviaListEditor : BaseListEditor
	{
		private readonly SyntaxTrivia _originalTrivia;

		private readonly IEnumerable<SyntaxTrivia> _newTrivia;

		public TriviaListEditor(SyntaxTrivia originalTrivia, IEnumerable<SyntaxTrivia> newTrivia, ListEditKind editKind)
			: base(originalTrivia.Span, editKind, visitTrivia: true, originalTrivia.IsPartOfStructuredTrivia())
		{
			_originalTrivia = originalTrivia;
			_newTrivia = newTrivia;
		}

		public override SyntaxTriviaList VisitList(SyntaxTriviaList list)
		{
			int num = list.IndexOf(_originalTrivia);
			if (num >= 0 && num < list.Count)
			{
				switch (editKind)
				{
				case ListEditKind.Replace:
					return list.ReplaceRange(_originalTrivia, _newTrivia);
				case ListEditKind.InsertAfter:
					return list.InsertRange(num + 1, _newTrivia);
				case ListEditKind.InsertBefore:
					return list.InsertRange(num, _newTrivia);
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
