using System.Collections.Generic;
using System.Linq;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class TreeDumperNode
{
	public object Value { get; }

	public string Text { get; }

	public IEnumerable<TreeDumperNode> Children { get; }

	public TreeDumperNode this[string child] => Children.FirstOrDefault((TreeDumperNode c) => c.Text == child);

	public TreeDumperNode(string text, object value, IEnumerable<TreeDumperNode> children)
	{
		Text = text;
		Value = value;
		Children = children ?? SpecializedCollections.EmptyEnumerable<TreeDumperNode>();
	}

	public TreeDumperNode(string text)
		: this(text, null, null)
	{
	}

	public IEnumerable<KeyValuePair<TreeDumperNode, TreeDumperNode>> PreorderTraversal()
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
