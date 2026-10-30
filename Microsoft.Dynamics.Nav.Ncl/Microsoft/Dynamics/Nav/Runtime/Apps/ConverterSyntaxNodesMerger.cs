using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.Types.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class ConverterSyntaxNodesMerger
{
	private Dictionary<string, SyntaxNode> addedNodes = new Dictionary<string, SyntaxNode>();

	private Dictionary<string, SyntaxNode> deletedNodes = new Dictionary<string, SyntaxNode>();

	private Dictionary<string, SyntaxNode> movedNodes = new Dictionary<string, SyntaxNode>();

	private Dictionary<string, SyntaxNode> orderedNodes = new Dictionary<string, SyntaxNode>();

	private Dictionary<string, SyntaxNode> updatedNodes = new Dictionary<string, SyntaxNode>();

	public ConverterSyntaxNodesMerger(Dictionary<SyntaxNodeIdentifier, SyntaxNode> syntaxNodes)
	{
		if (syntaxNodes == null)
		{
			throw new ArgumentNullException("syntaxNodes");
		}
		foreach (KeyValuePair<SyntaxNodeIdentifier, SyntaxNode> syntaxNode in syntaxNodes)
		{
			switch (syntaxNode.Key.DeltaType)
			{
			case PersonalizationV1DeltaType.Add:
				addedNodes.Add(syntaxNode.Key.Id, syntaxNode.Value);
				break;
			case PersonalizationV1DeltaType.Delete:
				deletedNodes.Add(syntaxNode.Key.Id, syntaxNode.Value);
				break;
			case PersonalizationV1DeltaType.Move:
				movedNodes.Add(syntaxNode.Key.Id, syntaxNode.Value);
				break;
			case PersonalizationV1DeltaType.Order:
				orderedNodes.Add(syntaxNode.Key.Id, syntaxNode.Value);
				break;
			case PersonalizationV1DeltaType.Update:
				updatedNodes.Add(syntaxNode.Key.Id, syntaxNode.Value);
				break;
			default:
				throw new InvalidOperationException("Unsupported!");
			}
		}
	}

	public IEnumerable<SyntaxNode> MergeAllSyntaxNodes()
	{
		List<SyntaxNode> list = new List<SyntaxNode>();
		foreach (KeyValuePair<string, SyntaxNode> item in addedNodes.ToList())
		{
			if (orderedNodes.TryGetValue(item.Key, out var value))
			{
				addedNodes[item.Key] = (item.Value as ActionAddChangeSyntax).WithAnchor((value as ActionMoveChangeSyntax).Anchor).WithChangeKeyword(SyntaxFactory.Token(SyntaxKind.AddAfterKeyword));
				orderedNodes.Remove(item.Key);
			}
		}
		list.AddRange(addedNodes.Values.Concat(deletedNodes.Values.Concat(movedNodes.Values.Concat(orderedNodes.Values.Concat(updatedNodes.Values)))));
		return list;
	}
}
