using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public static class SyntaxNodeExtensions
{
	private class CurrentNodes
	{
		private readonly Dictionary<SyntaxAnnotation, IReadOnlyList<SyntaxNode>> _idToNodeMap;

		public CurrentNodes(SyntaxNode root)
		{
			Dictionary<SyntaxAnnotation, List<SyntaxNode>> dictionary = new Dictionary<SyntaxAnnotation, List<SyntaxNode>>();
			foreach (SyntaxNode item in from n in root.GetAnnotatedNodesAndTokens("Id")
				select n.AsNode())
			{
				foreach (SyntaxAnnotation annotation in item.GetAnnotations("Id"))
				{
					if (!dictionary.TryGetValue(annotation, out var value))
					{
						value = new List<SyntaxNode>();
						dictionary.Add(annotation, value);
					}
					value.Add(item);
				}
			}
			_idToNodeMap = ((IEnumerable<KeyValuePair<SyntaxAnnotation, List<SyntaxNode>>>)dictionary).ToDictionary((Func<KeyValuePair<SyntaxAnnotation, List<SyntaxNode>>, SyntaxAnnotation>)((KeyValuePair<SyntaxAnnotation, List<SyntaxNode>> kv) => kv.Key), (Func<KeyValuePair<SyntaxAnnotation, List<SyntaxNode>>, IReadOnlyList<SyntaxNode>>)((KeyValuePair<SyntaxAnnotation, List<SyntaxNode>> kv) => ImmutableArray.CreateRange(kv.Value)));
		}

		public IReadOnlyList<SyntaxNode> GetNodes(SyntaxAnnotation id)
		{
			if (_idToNodeMap.TryGetValue(id, out var value))
			{
				return value;
			}
			return SpecializedCollections.EmptyReadOnlyList<SyntaxNode>();
		}
	}

	internal const string DefaultIndentation = "    ";

	internal const string DefaultEOL = "\r\n";

	private static readonly ConditionalWeakTable<SyntaxNode, SyntaxAnnotation> s_nodeToIdMap = new ConditionalWeakTable<SyntaxNode, SyntaxAnnotation>();

	private static readonly ConditionalWeakTable<SyntaxNode, CurrentNodes> s_rootToCurrentNodesMap = new ConditionalWeakTable<SyntaxNode, CurrentNodes>();

	internal const string IdAnnotationKind = "Id";

	public static TRoot ReplaceSyntax<TRoot>(this TRoot root, IEnumerable<SyntaxNode> nodes, Func<SyntaxNode, SyntaxNode, SyntaxNode> computeReplacementNode, IEnumerable<SyntaxToken> tokens, Func<SyntaxToken, SyntaxToken, SyntaxToken> computeReplacementToken, IEnumerable<SyntaxTrivia> trivia, Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia> computeReplacementTrivia) where TRoot : SyntaxNode
	{
		return (TRoot)root.ReplaceCore(nodes, computeReplacementNode, tokens, computeReplacementToken, trivia, computeReplacementTrivia);
	}

	public static TRoot ReplaceNodes<TRoot, TNode>(this TRoot root, IEnumerable<TNode> nodes, Func<TNode, TNode, SyntaxNode> computeReplacementNode) where TRoot : SyntaxNode where TNode : SyntaxNode
	{
		return (TRoot)root.ReplaceCore(nodes, computeReplacementNode);
	}

	public static TRoot ReplaceNode<TRoot>(this TRoot root, SyntaxNode oldNode, SyntaxNode newNode) where TRoot : SyntaxNode
	{
		if (oldNode == newNode)
		{
			return root;
		}
		return (TRoot)root.ReplaceCore(new SyntaxNode[1] { oldNode }, (SyntaxNode o, SyntaxNode r) => newNode);
	}

	public static TRoot ReplaceNode<TRoot>(this TRoot root, SyntaxNode oldNode, IEnumerable<SyntaxNode> newNodes) where TRoot : SyntaxNode
	{
		return (TRoot)root.ReplaceNodeInListCore(oldNode, newNodes);
	}

	public static TRoot InsertNodesBefore<TRoot>(this TRoot root, SyntaxNode nodeInList, IEnumerable<SyntaxNode> newNodes) where TRoot : SyntaxNode
	{
		return (TRoot)root.InsertNodesInListCore(nodeInList, newNodes, insertBefore: true);
	}

	public static TRoot InsertNodesAfter<TRoot>(this TRoot root, SyntaxNode nodeInList, IEnumerable<SyntaxNode> newNodes) where TRoot : SyntaxNode
	{
		return (TRoot)root.InsertNodesInListCore(nodeInList, newNodes, insertBefore: false);
	}

	public static TRoot ReplaceToken<TRoot>(this TRoot root, SyntaxToken tokenInList, IEnumerable<SyntaxToken> newTokens) where TRoot : SyntaxNode
	{
		return (TRoot)root.ReplaceTokenInListCore(tokenInList, newTokens);
	}

	public static TRoot InsertTokensBefore<TRoot>(this TRoot root, SyntaxToken tokenInList, IEnumerable<SyntaxToken> newTokens) where TRoot : SyntaxNode
	{
		return (TRoot)root.InsertTokensInListCore(tokenInList, newTokens, insertBefore: true);
	}

	public static TRoot InsertTokensAfter<TRoot>(this TRoot root, SyntaxToken tokenInList, IEnumerable<SyntaxToken> newTokens) where TRoot : SyntaxNode
	{
		return (TRoot)root.InsertTokensInListCore(tokenInList, newTokens, insertBefore: false);
	}

	public static TRoot ReplaceTrivia<TRoot>(this TRoot root, SyntaxTrivia oldTrivia, IEnumerable<SyntaxTrivia> newTrivia) where TRoot : SyntaxNode
	{
		return (TRoot)root.ReplaceTriviaInListCore(oldTrivia, newTrivia);
	}

	public static TRoot InsertTriviaBefore<TRoot>(this TRoot root, SyntaxTrivia trivia, IEnumerable<SyntaxTrivia> newTrivia) where TRoot : SyntaxNode
	{
		return (TRoot)root.InsertTriviaInListCore(trivia, newTrivia, insertBefore: true);
	}

	public static TRoot InsertTriviaAfter<TRoot>(this TRoot root, SyntaxTrivia trivia, IEnumerable<SyntaxTrivia> newTrivia) where TRoot : SyntaxNode
	{
		return (TRoot)root.InsertTriviaInListCore(trivia, newTrivia, insertBefore: false);
	}

	public static TRoot ReplaceTokens<TRoot>(this TRoot root, IEnumerable<SyntaxToken> tokens, Func<SyntaxToken, SyntaxToken, SyntaxToken> computeReplacementToken) where TRoot : SyntaxNode
	{
		return (TRoot)root.ReplaceCore<SyntaxNode>(null, null, tokens, computeReplacementToken);
	}

	public static TRoot ReplaceToken<TRoot>(this TRoot root, SyntaxToken oldToken, SyntaxToken newToken) where TRoot : SyntaxNode
	{
		return (TRoot)root.ReplaceCore<SyntaxNode>(null, null, new SyntaxToken[1] { oldToken }, (SyntaxToken o, SyntaxToken r) => newToken);
	}

	public static TRoot ReplaceTrivia<TRoot>(this TRoot root, IEnumerable<SyntaxTrivia> trivia, Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia> computeReplacementTrivia) where TRoot : SyntaxNode
	{
		return (TRoot)root.ReplaceCore<SyntaxNode>(null, null, null, null, trivia, computeReplacementTrivia);
	}

	public static TRoot ReplaceTrivia<TRoot>(this TRoot root, SyntaxTrivia trivia, SyntaxTrivia newTrivia) where TRoot : SyntaxNode
	{
		return (TRoot)root.ReplaceCore<SyntaxNode>(null, null, null, null, new SyntaxTrivia[1] { trivia }, (SyntaxTrivia o, SyntaxTrivia r) => newTrivia);
	}

	public static TRoot RemoveNode<TRoot>(this TRoot root, SyntaxNode node, SyntaxRemoveOptions options) where TRoot : SyntaxNode
	{
		return (TRoot)root.RemoveNodesCore(new SyntaxNode[1] { node }, options);
	}

	public static TRoot RemoveNodes<TRoot>(this TRoot root, IEnumerable<SyntaxNode> nodes, SyntaxRemoveOptions options) where TRoot : SyntaxNode
	{
		return (TRoot)root.RemoveNodesCore(nodes, options);
	}

	public static TNode NormalizeWhitespace<TNode>(this TNode node, string indentation, bool elasticTrivia) where TNode : SyntaxNode
	{
		return (TNode)node.NormalizeWhitespaceCore(indentation, "\r\n", elasticTrivia);
	}

	public static TNode NormalizeWhitespace<TNode>(this TNode node, string indentation = "    ", string eol = "\r\n", bool elasticTrivia = false) where TNode : SyntaxNode
	{
		return (TNode)node.NormalizeWhitespaceCore(indentation, eol, elasticTrivia);
	}

	public static TSyntax WithTriviaFrom<TSyntax>(this TSyntax syntax, SyntaxNode node) where TSyntax : SyntaxNode
	{
		return syntax.WithLeadingTrivia(node.GetLeadingTrivia()).WithTrailingTrivia(node.GetTrailingTrivia());
	}

	public static TSyntax WithoutTrivia<TSyntax>(this TSyntax syntax) where TSyntax : SyntaxNode
	{
		return syntax.WithoutLeadingTrivia().WithoutTrailingTrivia();
	}

	public static SyntaxToken WithoutTrivia(this SyntaxToken token)
	{
		return token.WithTrailingTrivia(default(SyntaxTriviaList)).WithLeadingTrivia(default(SyntaxTriviaList));
	}

	public static TSyntax WithLeadingTrivia<TSyntax>(this TSyntax node, SyntaxTriviaList trivia) where TSyntax : SyntaxNode
	{
		SyntaxToken firstToken = node.GetFirstToken(includeZeroWidth: true);
		SyntaxToken newToken = firstToken.WithLeadingTrivia(trivia);
		return node.ReplaceToken(firstToken, newToken);
	}

	public static TSyntax WithLeadingTrivia<TSyntax>(this TSyntax node, IEnumerable<SyntaxTrivia> trivia) where TSyntax : SyntaxNode
	{
		SyntaxToken firstToken = node.GetFirstToken(includeZeroWidth: true);
		SyntaxToken newToken = firstToken.WithLeadingTrivia(trivia);
		return node.ReplaceToken(firstToken, newToken);
	}

	public static TSyntax WithoutLeadingTrivia<TSyntax>(this TSyntax node) where TSyntax : SyntaxNode
	{
		return node.WithLeadingTrivia((IEnumerable<SyntaxTrivia>)null);
	}

	public static TSyntax WithLeadingTrivia<TSyntax>(this TSyntax node, params SyntaxTrivia[] trivia) where TSyntax : SyntaxNode
	{
		return node.WithLeadingTrivia((IEnumerable<SyntaxTrivia>)trivia);
	}

	public static TSyntax WithTrailingTrivia<TSyntax>(this TSyntax node, SyntaxTriviaList trivia) where TSyntax : SyntaxNode
	{
		SyntaxToken lastToken = node.GetLastToken(includeZeroWidth: true);
		SyntaxToken newToken = lastToken.WithTrailingTrivia(trivia);
		return node.ReplaceToken(lastToken, newToken);
	}

	public static TSyntax WithTrailingTrivia<TSyntax>(this TSyntax node, IEnumerable<SyntaxTrivia> trivia) where TSyntax : SyntaxNode
	{
		SyntaxToken lastToken = node.GetLastToken(includeZeroWidth: true);
		SyntaxToken newToken = lastToken.WithTrailingTrivia(trivia);
		return node.ReplaceToken(lastToken, newToken);
	}

	public static TSyntax WithoutTrailingTrivia<TSyntax>(this TSyntax node) where TSyntax : SyntaxNode
	{
		return node.WithTrailingTrivia((IEnumerable<SyntaxTrivia>)null);
	}

	public static TSyntax WithTrailingTrivia<TSyntax>(this TSyntax node, params SyntaxTrivia[] trivia) where TSyntax : SyntaxNode
	{
		return node.WithTrailingTrivia((IEnumerable<SyntaxTrivia>)trivia);
	}

	internal static SyntaxNode AsRootOfNewTreeWithOptionsFrom(this SyntaxNode node, SyntaxTree oldTree)
	{
		return oldTree.WithRootAndOptions(node, oldTree.Options).GetRoot();
	}

	public static TRoot TrackNodes<TRoot>(this TRoot root, IEnumerable<SyntaxNode> nodes) where TRoot : SyntaxNode
	{
		if (nodes == null)
		{
			throw new ArgumentNullException("nodes");
		}
		foreach (SyntaxNode node in nodes)
		{
			if (!IsDescendant(root, node))
			{
				throw new ArgumentException(CodeAnalysisResources.InvalidNodeToTrack);
			}
			s_nodeToIdMap.GetValue(node, (SyntaxNode n) => new SyntaxAnnotation("Id"));
		}
		return root.ReplaceNodes(nodes, (SyntaxNode n, SyntaxNode r) => (!n.HasAnnotation(GetId(n))) ? r.WithAdditionalAnnotations(GetId(n)) : r);
	}

	public static TRoot TrackNodes<TRoot>(this TRoot root, params SyntaxNode[] nodes) where TRoot : SyntaxNode
	{
		return root.TrackNodes((IEnumerable<SyntaxNode>)nodes);
	}

	public static IEnumerable<TNode> GetCurrentNodes<TNode>(this SyntaxNode root, TNode node) where TNode : SyntaxNode
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		return GetCurrentNodeFromTrueRoots(GetRoot(root), node).OfType<TNode>();
	}

	public static TNode GetCurrentNode<TNode>(this SyntaxNode root, TNode node) where TNode : SyntaxNode
	{
		return root.GetCurrentNodes(node).SingleOrDefault();
	}

	public static IEnumerable<TNode> GetCurrentNodes<TNode>(this SyntaxNode root, IEnumerable<TNode> nodes) where TNode : SyntaxNode
	{
		if (nodes == null)
		{
			throw new ArgumentNullException("nodes");
		}
		SyntaxNode trueRoot = GetRoot(root);
		foreach (TNode node in nodes)
		{
			foreach (TNode item in GetCurrentNodeFromTrueRoots(trueRoot, node).OfType<TNode>())
			{
				yield return item;
			}
		}
	}

	private static IReadOnlyList<SyntaxNode> GetCurrentNodeFromTrueRoots(SyntaxNode trueRoot, SyntaxNode node)
	{
		SyntaxAnnotation id = GetId(node);
		if (id != null)
		{
			return s_rootToCurrentNodesMap.GetValue(trueRoot, (SyntaxNode r) => new CurrentNodes(r)).GetNodes(id);
		}
		return SpecializedCollections.EmptyReadOnlyList<SyntaxNode>();
	}

	private static SyntaxAnnotation GetId(SyntaxNode original)
	{
		s_nodeToIdMap.TryGetValue(original, out var value);
		return value;
	}

	private static SyntaxNode GetRoot(SyntaxNode node)
	{
		while (true)
		{
			if (node.Parent != null)
			{
				node = node.Parent;
				continue;
			}
			if (!node.IsStructuredTrivia)
			{
				break;
			}
			node = ((IStructuredTriviaSyntax)node).ParentTrivia.Token.Parent;
		}
		return node;
	}

	private static bool IsDescendant(SyntaxNode root, SyntaxNode node)
	{
		while (node != null)
		{
			if (node == root)
			{
				return true;
			}
			if (node.Parent != null)
			{
				node = node.Parent;
				continue;
			}
			if (!node.IsStructuredTrivia)
			{
				break;
			}
			node = ((IStructuredTriviaSyntax)node).ParentTrivia.Token.Parent;
		}
		return false;
	}
}
