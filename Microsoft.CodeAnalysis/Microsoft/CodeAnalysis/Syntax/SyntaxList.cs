using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Syntax;

internal abstract class SyntaxList : SyntaxNode
{
	internal class SeparatedWithManyChildren : SyntaxList
	{
		private readonly ArrayElement<SyntaxNode>[] _children;

		internal SeparatedWithManyChildren(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList green, SyntaxNode parent, int position)
			: base(green, parent, position)
		{
			_children = new ArrayElement<SyntaxNode>[green.SlotCount + 1 >> 1];
		}

		internal override SyntaxNode GetNodeSlot(int i)
		{
			if ((i & 1) != 0)
			{
				return null;
			}
			return GetRedElement(ref _children[i >> 1].Value, i);
		}

		internal override SyntaxNode GetCachedSlot(int i)
		{
			if ((i & 1) != 0)
			{
				return null;
			}
			return _children[i >> 1].Value;
		}
	}

	internal class SeparatedWithManyWeakChildren : SyntaxList
	{
		private readonly ArrayElement<WeakReference<SyntaxNode>>[] _children;

		internal SeparatedWithManyWeakChildren(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList green, SyntaxNode parent, int position)
			: base(green, parent, position)
		{
			_children = new ArrayElement<WeakReference<SyntaxNode>>[(green.SlotCount + 1 >> 1) - 1];
		}

		internal override SyntaxNode GetNodeSlot(int i)
		{
			SyntaxNode result = null;
			if ((i & 1) == 0)
			{
				result = GetWeakRedElement(ref _children[i >> 1].Value, i);
			}
			return result;
		}

		internal override SyntaxNode GetCachedSlot(int i)
		{
			SyntaxNode target = null;
			if ((i & 1) == 0)
			{
				_children[i >> 1].Value?.TryGetTarget(out target);
			}
			return target;
		}
	}

	internal class WithManyChildren : SyntaxList
	{
		private readonly ArrayElement<SyntaxNode>[] _children;

		internal WithManyChildren(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList green, SyntaxNode parent, int position)
			: base(green, parent, position)
		{
			_children = new ArrayElement<SyntaxNode>[green.SlotCount];
		}

		internal override SyntaxNode GetNodeSlot(int index)
		{
			return GetRedElement(ref _children[index].Value, index);
		}

		internal override SyntaxNode GetCachedSlot(int index)
		{
			return _children[index];
		}
	}

	internal class WithManyWeakChildren : SyntaxList
	{
		private readonly ArrayElement<WeakReference<SyntaxNode>>[] _children;

		private readonly int[] _childPositions;

		internal WithManyWeakChildren(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.WithManyChildrenBase green, SyntaxNode parent, int position)
			: base(green, parent, position)
		{
			int slotCount = green.SlotCount;
			_children = new ArrayElement<WeakReference<SyntaxNode>>[slotCount];
			int[] array = new int[slotCount];
			int num = position;
			ArrayElement<GreenNode>[] children = green.children;
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = num;
				num += children[i].Value.FullWidth;
			}
			_childPositions = array;
		}

		internal override int GetChildPosition(int index)
		{
			return _childPositions[index];
		}

		internal override SyntaxNode GetNodeSlot(int index)
		{
			return GetWeakRedElement(ref _children[index].Value, index);
		}

		internal override SyntaxNode GetCachedSlot(int index)
		{
			SyntaxNode target = null;
			_children[index].Value?.TryGetTarget(out target);
			return target;
		}
	}

	internal class WithThreeChildren : SyntaxList
	{
		private SyntaxNode _child0;

		private SyntaxNode _child1;

		private SyntaxNode _child2;

		internal WithThreeChildren(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList green, SyntaxNode parent, int position)
			: base(green, parent, position)
		{
		}

		internal override SyntaxNode GetNodeSlot(int index)
		{
			return index switch
			{
				0 => GetRedElement(ref _child0, 0), 
				1 => GetRedElementIfNotToken(ref _child1), 
				2 => GetRedElement(ref _child2, 2), 
				_ => null, 
			};
		}

		internal override SyntaxNode GetCachedSlot(int index)
		{
			return index switch
			{
				0 => _child0, 
				1 => _child1, 
				2 => _child2, 
				_ => null, 
			};
		}
	}

	internal class WithTwoChildren : SyntaxList
	{
		private SyntaxNode _child0;

		private SyntaxNode _child1;

		internal WithTwoChildren(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList green, SyntaxNode parent, int position)
			: base(green, parent, position)
		{
		}

		internal override SyntaxNode GetNodeSlot(int index)
		{
			return index switch
			{
				0 => GetRedElement(ref _child0, 0), 
				1 => GetRedElementIfNotToken(ref _child1), 
				_ => null, 
			};
		}

		internal override SyntaxNode GetCachedSlot(int index)
		{
			return index switch
			{
				0 => _child0, 
				1 => _child1, 
				_ => null, 
			};
		}
	}

	public override string Language
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	protected override SyntaxTree SyntaxTreeCore => Parent.SyntaxTree;

	internal SyntaxList(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	protected internal override SyntaxNode ReplaceCore<TNode>(IEnumerable<TNode> nodes = null, Func<TNode, TNode, SyntaxNode> computeReplacementNode = null, IEnumerable<SyntaxToken> tokens = null, Func<SyntaxToken, SyntaxToken, SyntaxToken> computeReplacementToken = null, IEnumerable<SyntaxTrivia> trivia = null, Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia> computeReplacementTrivia = null)
	{
		throw ExceptionUtilities.Unreachable;
	}

	protected internal override SyntaxNode ReplaceNodeInListCore(SyntaxNode originalNode, IEnumerable<SyntaxNode> replacementNodes)
	{
		throw ExceptionUtilities.Unreachable;
	}

	protected internal override SyntaxNode InsertNodesInListCore(SyntaxNode nodeInList, IEnumerable<SyntaxNode> nodesToInsert, bool insertBefore)
	{
		throw ExceptionUtilities.Unreachable;
	}

	protected internal override SyntaxNode ReplaceTokenInListCore(SyntaxToken originalToken, IEnumerable<SyntaxToken> newTokens)
	{
		throw ExceptionUtilities.Unreachable;
	}

	protected internal override SyntaxNode InsertTokensInListCore(SyntaxToken originalToken, IEnumerable<SyntaxToken> newTokens, bool insertBefore)
	{
		throw ExceptionUtilities.Unreachable;
	}

	protected internal override SyntaxNode ReplaceTriviaInListCore(SyntaxTrivia originalTrivia, IEnumerable<SyntaxTrivia> newTrivia)
	{
		throw ExceptionUtilities.Unreachable;
	}

	protected internal override SyntaxNode InsertTriviaInListCore(SyntaxTrivia originalTrivia, IEnumerable<SyntaxTrivia> newTrivia, bool insertBefore)
	{
		throw ExceptionUtilities.Unreachable;
	}

	protected internal override SyntaxNode RemoveNodesCore(IEnumerable<SyntaxNode> nodes, SyntaxRemoveOptions options)
	{
		throw ExceptionUtilities.Unreachable;
	}

	protected internal override SyntaxNode NormalizeWhitespaceCore(string indentation, string eol, bool elasticTrivia)
	{
		throw ExceptionUtilities.Unreachable;
	}

	protected override bool IsEquivalentToCore(SyntaxNode node, bool topLevel = false)
	{
		throw ExceptionUtilities.Unreachable;
	}
}
