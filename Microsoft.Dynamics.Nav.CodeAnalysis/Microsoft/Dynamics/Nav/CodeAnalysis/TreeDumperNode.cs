using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class TreeDumperNode
{
	internal object Value { get; private set; }

	internal string Text { get; private set; }

	internal IEnumerable<TreeDumperNode> Children { get; private set; }

	internal TreeDumperNode this[string child] => Children.FirstOrDefault((TreeDumperNode c) => c.Text == child);

	internal TreeDumperNode(string text, object value, IEnumerable<TreeDumperNode> children)
	{
		Text = text;
		Value = value;
		Children = children ?? SpecializedCollections.EmptyEnumerable<TreeDumperNode>();
	}

	internal IEnumerable<KeyValuePair<TreeDumperNode, TreeDumperNode>> PreorderTraversal()
	{
		Stack<KeyValuePair<TreeDumperNode, TreeDumperNode>> stack = new Stack<KeyValuePair<TreeDumperNode, TreeDumperNode>>();
		stack.Push(new KeyValuePair<TreeDumperNode, TreeDumperNode>(null, this));
		while (stack.Count != 0)
		{
			KeyValuePair<TreeDumperNode, TreeDumperNode> currentEdge = stack.Pop();
			yield return currentEdge;
			TreeDumperNode value = currentEdge.Value;
			foreach (TreeDumperNode item in value.Children.Where((TreeDumperNode x) => x != null).Reverse())
			{
				stack.Push(new KeyValuePair<TreeDumperNode, TreeDumperNode>(value, item));
			}
		}
	}
}
