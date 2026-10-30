using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal struct Blender
{
	private struct Cursor
	{
		public readonly SyntaxNodeOrToken CurrentNodeOrToken;

		private readonly int indexInParent;

		public bool IsFinished
		{
			get
			{
				if (CurrentNodeOrToken.Kind != SyntaxKind.None)
				{
					return CurrentNodeOrToken.Kind == SyntaxKind.EndOfFileToken;
				}
				return true;
			}
		}

		private Cursor(SyntaxNodeOrToken node, int indexInParent)
		{
			CurrentNodeOrToken = node;
			this.indexInParent = indexInParent;
		}

		public static Cursor FromRoot(SyntaxNode node)
		{
			return new Cursor(node, 0);
		}

		private static bool IsNonZeroWidthOrIsEndOfFile(SyntaxNodeOrToken token)
		{
			if (token.Kind != SyntaxKind.EndOfFileToken)
			{
				return token.FullWidth != 0;
			}
			return true;
		}

		public Cursor MoveToNextSibling()
		{
			if (CurrentNodeOrToken.Parent != null)
			{
				ChildSyntaxList childSyntaxList = CurrentNodeOrToken.Parent.ChildNodesAndTokens();
				int i = indexInParent + 1;
				for (int count = childSyntaxList.Count; i < count; i++)
				{
					SyntaxNodeOrToken syntaxNodeOrToken = childSyntaxList[i];
					if (IsNonZeroWidthOrIsEndOfFile(syntaxNodeOrToken))
					{
						return new Cursor(syntaxNodeOrToken, i);
					}
				}
				return MoveToParent().MoveToNextSibling();
			}
			return default;
		}

		private Cursor MoveToParent()
		{
			SyntaxNode parent = CurrentNodeOrToken.Parent;
			return new Cursor(indexInParent: IndexOfNodeInParent(parent), node: parent);
		}

		private static int IndexOfNodeInParent(SyntaxNode node)
		{
			if (node.Parent == null)
			{
				return 0;
			}
			ChildSyntaxList list = node.Parent.ChildNodesAndTokens();
			int i = SyntaxNodeOrToken.GetFirstChildIndexSpanningPosition(list, node.Position);
			for (int count = list.Count; i < count; i++)
			{
				if (list[i] == node)
				{
					return i;
				}
			}
			throw ExceptionUtilities.Unreachable;
		}

		public Cursor MoveToFirstChild()
		{
			DebugAssertHelper.Assert(CurrentNodeOrToken.IsNode);
			SyntaxNode syntaxNode = CurrentNodeOrToken.AsNode();
			if (syntaxNode.SlotCount > 0)
			{
				SyntaxNodeOrToken syntaxNodeOrToken = ChildSyntaxList.ItemInternal(syntaxNode, 0);
				if (IsNonZeroWidthOrIsEndOfFile(syntaxNodeOrToken))
				{
					return new Cursor(syntaxNodeOrToken, 0);
				}
			}
			int num = 0;
			foreach (SyntaxNodeOrToken item in CurrentNodeOrToken.ChildNodesAndTokens())
			{
				if (IsNonZeroWidthOrIsEndOfFile(item))
				{
					return new Cursor(item, num);
				}
				num++;
			}
			return default;
		}

		public Cursor MoveToFirstToken()
		{
			Cursor result = this;
			if (!result.IsFinished)
			{
				SyntaxNodeOrToken currentNodeOrToken = result.CurrentNodeOrToken;
				while (currentNodeOrToken.Kind != SyntaxKind.None && !currentNodeOrToken.Kind.IsAnyToken())
				{
					result = result.MoveToFirstChild();
					currentNodeOrToken = result.CurrentNodeOrToken;
				}
			}
			return result;
		}
	}

	private struct Reader(Blender blender, bool forceLex)
	{
		private readonly Lexer lexer = blender.lexer;

		private readonly bool forceLex = forceLex;

		private Cursor oldTreeCursor = blender.oldTreeCursor;

		private ImmutableStack<TextChangeRange> changes = blender.changes;

		private int newPosition = blender.newPosition;

		private int changeDelta = blender.changeDelta;

		private LexerMode lexerMode = blender.LexerMode;

		internal BlendedNode ReadNodeOrToken(LexerMode mode, bool asToken)
		{
			if (forceLex)
			{
				return ReadNewToken(mode);
			}
			BlendedNode blendedNode;
			while (true)
			{
				if (oldTreeCursor.IsFinished)
				{
					return ReadNewToken(mode);
				}
				if (changeDelta < 0)
				{
					SkipOldToken();
					continue;
				}
				if (changeDelta > 0)
				{
					return ReadNewToken(mode);
				}
				if (TryTakeOldNodeOrToken(asToken, mode, out blendedNode))
				{
					break;
				}
				if (oldTreeCursor.CurrentNodeOrToken.IsNode)
				{
					oldTreeCursor = oldTreeCursor.MoveToFirstChild();
				}
				else
				{
					SkipOldToken();
				}
			}
			return blendedNode;
		}

		private void SkipOldToken()
		{
			DebugAssertHelper.Assert(!oldTreeCursor.IsFinished);
			oldTreeCursor = oldTreeCursor.MoveToFirstToken();
			changeDelta += oldTreeCursor.CurrentNodeOrToken.FullWidth;
			oldTreeCursor = oldTreeCursor.MoveToNextSibling();
			SkipPastChanges();
		}

		private void SkipPastChanges()
		{
			int position = oldTreeCursor.CurrentNodeOrToken.Position;
			while (!changes.IsEmpty && position >= changes.Peek().Span.End)
			{
				TextChangeRange textChangeRange = changes.Peek();
				changes = changes.Pop();
				changeDelta += textChangeRange.NewLength - textChangeRange.Span.Length;
			}
		}

		private BlendedNode ReadNewToken(LexerMode mode)
		{
			DebugAssertHelper.Assert(changeDelta > 0 || oldTreeCursor.IsFinished || forceLex);
			InternalSyntaxToken internalSyntaxToken = LexNewToken(mode);
			int fullWidth = internalSyntaxToken.FullWidth;
			newPosition += fullWidth;
			changeDelta -= fullWidth;
			SkipPastChanges();
			return CreateBlendedNode(null, internalSyntaxToken);
		}

		private InternalSyntaxToken LexNewToken(LexerMode mode)
		{
			if (lexer.TextWindow.Position != newPosition)
			{
				lexer.Reset(newPosition);
			}
			InternalSyntaxToken result = lexer.Lex(mode);
			lexerMode = mode;
			return result;
		}

		private bool TryTakeOldNodeOrToken(bool asToken, LexerMode mode, out BlendedNode blendedNode)
		{
			if (asToken)
			{
				oldTreeCursor = oldTreeCursor.MoveToFirstToken();
			}
			SyntaxNodeOrToken currentNodeOrToken = oldTreeCursor.CurrentNodeOrToken;
			if (!CanReuse(currentNodeOrToken, mode))
			{
				blendedNode = default;
				return false;
			}
			newPosition += currentNodeOrToken.FullWidth;
			oldTreeCursor = oldTreeCursor.MoveToNextSibling();
			blendedNode = CreateBlendedNode(currentNodeOrToken.AsNode(), (InternalSyntaxToken)currentNodeOrToken.AsToken().Node);
			return true;
		}

		private bool CanReuse(SyntaxNodeOrToken nodeOrToken, LexerMode mode)
		{
			if (nodeOrToken.FullWidth == 0)
			{
				return false;
			}
			if (mode != lexerMode)
			{
				return false;
			}
			if (nodeOrToken.LexedMode != LexerMode.Default && mode != LexerMode.Default && nodeOrToken.LexedMode != mode)
			{
				return false;
			}
			if (IntersectsNextChange(nodeOrToken))
			{
				return false;
			}
			if (nodeOrToken.ContainsDiagnostics || (nodeOrToken.IsToken && nodeOrToken.AsToken().Node.ContainsSkippedText && nodeOrToken.Parent.ContainsDiagnostics))
			{
				return false;
			}
			if ((nodeOrToken.IsToken && nodeOrToken.AsToken().IsMissing) || (nodeOrToken.IsNode && IsIncomplete(nodeOrToken.AsNode())))
			{
				return false;
			}
			return true;
		}

		private bool IntersectsNextChange(SyntaxNodeOrToken nodeOrToken)
		{
			if (changes.IsEmpty)
			{
				return false;
			}
			TextSpan fullSpan = nodeOrToken.FullSpan;
			TextSpan span = changes.Peek().Span;
			return fullSpan.IntersectsWith(span);
		}

		private static bool IsIncomplete(SyntaxNode node)
		{
			return node.Node.GetLastTerminal()?.IsMissing ?? false;
		}

		private BlendedNode CreateBlendedNode(SyntaxNode node, InternalSyntaxToken token)
		{
			return new BlendedNode(node, token, new Blender(lexer, oldTreeCursor, changes, newPosition, changeDelta, lexerMode));
		}
	}

	private readonly Lexer lexer;

	private readonly Cursor oldTreeCursor;

	private readonly ImmutableStack<TextChangeRange> changes;

	private readonly int newPosition;

	private readonly int changeDelta;

	public LexerMode LexerMode { get; }

	public Blender(Lexer lexer, SyntaxNode oldTree, IEnumerable<TextChangeRange> changes)
	{
		DebugAssertHelper.Assert(lexer != null);
		this.lexer = lexer;
		this.changes = ImmutableStack.Create<TextChangeRange>();
		if (changes != null)
		{
			TextChangeRange changeRange = TextChangeRange.Collapse(changes);
			TextChangeRange value = ExtendToAffectedRange(oldTree, changeRange);
			this.changes = this.changes.Push(value);
		}
		if (oldTree == null)
		{
			oldTreeCursor = default;
			newPosition = lexer.TextWindow.Position;
		}
		else
		{
			oldTreeCursor = Cursor.FromRoot(oldTree).MoveToFirstChild();
			newPosition = 0;
		}
		changeDelta = 0;
		LexerMode = LexerMode.Default;
	}

	private Blender(Lexer lexer, Cursor oldTreeCursor, ImmutableStack<TextChangeRange> changes, int newPosition, int changeDelta, LexerMode lexerMode)
	{
		DebugAssertHelper.Assert(lexer != null);
		DebugAssertHelper.Assert(changes != null);
		DebugAssertHelper.Assert(newPosition >= 0);
		this.lexer = lexer;
		this.oldTreeCursor = oldTreeCursor;
		this.changes = changes;
		this.newPosition = newPosition;
		this.changeDelta = changeDelta;
		LexerMode = lexerMode;
	}

	private static TextChangeRange ExtendToAffectedRange(SyntaxNode oldTree, TextChangeRange changeRange)
	{
		int val = oldTree.FullWidth - 1;
		int num = Math.Max(Math.Min(changeRange.Span.Start, val), 0);
		int num2 = 0;
		while (num > 0 && num2 <= 1)
		{
			SyntaxToken syntaxToken = oldTree.FindToken(num, findInsideTrivia: false);
			DebugAssertHelper.Assert(syntaxToken.Kind != SyntaxKind.None, "how could we not get a real token back?");
			num = Math.Max(0, syntaxToken.Position - 1);
			if (syntaxToken.FullWidth > 0)
			{
				num2++;
			}
		}
		TextSpan span = TextSpan.FromBounds(num, changeRange.Span.End);
		int newLength = changeRange.NewLength + (changeRange.Span.Start - num);
		return new TextChangeRange(span, newLength);
	}

	public BlendedNode ReadNode(LexerMode mode, bool forceLex = false)
	{
		return ReadNodeOrToken(mode, asToken: false, forceLex);
	}

	public BlendedNode ReadToken(LexerMode mode, bool forceLex = false)
	{
		return ReadNodeOrToken(mode, asToken: true, forceLex);
	}

	private BlendedNode ReadNodeOrToken(LexerMode mode, bool asToken, bool forceLex = false)
	{
		return new Reader(this, forceLex).ReadNodeOrToken(mode, asToken);
	}
}
