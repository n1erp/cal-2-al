using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal readonly struct Blender
{
	private struct Cursor
	{
		public readonly SyntaxNodeOrToken CurrentNodeOrToken;

		private readonly int _indexInParent;

		public bool IsFinished
		{
			get
			{
				if (CurrentNodeOrToken.Kind() != SyntaxKind.None)
				{
					return CurrentNodeOrToken.Kind() == SyntaxKind.EndOfFileToken;
				}
				return true;
			}
		}

		private Cursor(SyntaxNodeOrToken node, int indexInParent)
		{
			CurrentNodeOrToken = node;
			_indexInParent = indexInParent;
		}

		public static Cursor FromRoot(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode node)
		{
			return new Cursor(node, 0);
		}

		private static bool IsNonZeroWidthOrIsEndOfFile(SyntaxNodeOrToken token)
		{
			if (token.Kind() != SyntaxKind.EndOfFileToken)
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
				int i = _indexInParent + 1;
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
			int i = SyntaxNodeOrToken.GetFirstChildIndexSpanningPosition(list, ((Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode)node).Position);
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
			SyntaxNode syntaxNode = CurrentNodeOrToken.AsNode();
			if (syntaxNode.Kind() == SyntaxKind.InterpolatedStringExpression)
			{
				SyntaxToken token = Lexer.RescanInterpolatedString((InterpolatedStringExpressionSyntax)syntaxNode.Green);
				return new Cursor(new Microsoft.CodeAnalysis.SyntaxToken(syntaxNode.Parent, token, syntaxNode.Position, _indexInParent), _indexInParent);
			}
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
				while (currentNodeOrToken.Kind() != SyntaxKind.None && !SyntaxFacts.IsAnyToken(currentNodeOrToken.Kind()))
				{
					result = result.MoveToFirstChild();
					currentNodeOrToken = result.CurrentNodeOrToken;
				}
			}
			return result;
		}
	}

	private struct Reader(Blender blender)
	{
		private readonly Lexer _lexer = blender._lexer;

		private Cursor _oldTreeCursor = blender._oldTreeCursor;

		private ImmutableStack<TextChangeRange> _changes = blender._changes;

		private int _newPosition = blender._newPosition;

		private int _changeDelta = blender._changeDelta;

		private DirectiveStack _newDirectives = blender._newDirectives;

		private DirectiveStack _oldDirectives = blender._oldDirectives;

		private LexerMode _newLexerDrivenMode = blender._newLexerDrivenMode;

		internal BlendedNode ReadNodeOrToken(LexerMode mode, bool asToken)
		{
			BlendedNode blendedNode;
			while (true)
			{
				if (_oldTreeCursor.IsFinished)
				{
					return ReadNewToken(mode);
				}
				if (_changeDelta < 0)
				{
					SkipOldToken();
					continue;
				}
				if (_changeDelta > 0)
				{
					return ReadNewToken(mode);
				}
				if (TryTakeOldNodeOrToken(asToken, out blendedNode))
				{
					break;
				}
				if (_oldTreeCursor.CurrentNodeOrToken.IsNode)
				{
					_oldTreeCursor = _oldTreeCursor.MoveToFirstChild();
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
			_oldTreeCursor = _oldTreeCursor.MoveToFirstToken();
			SyntaxNodeOrToken currentNodeOrToken = _oldTreeCursor.CurrentNodeOrToken;
			_changeDelta += currentNodeOrToken.FullWidth;
			_oldDirectives = currentNodeOrToken.ApplyDirectives(_oldDirectives);
			_oldTreeCursor = _oldTreeCursor.MoveToNextSibling();
			SkipPastChanges();
		}

		private void SkipPastChanges()
		{
			int position = _oldTreeCursor.CurrentNodeOrToken.Position;
			while (!_changes.IsEmpty && position >= _changes.Peek().Span.End)
			{
				TextChangeRange textChangeRange = _changes.Peek();
				_changes = _changes.Pop();
				_changeDelta += textChangeRange.NewLength - textChangeRange.Span.Length;
			}
		}

		private BlendedNode ReadNewToken(LexerMode mode)
		{
			SyntaxToken syntaxToken = LexNewToken(mode);
			int fullWidth = syntaxToken.FullWidth;
			_newPosition += fullWidth;
			_changeDelta -= fullWidth;
			SkipPastChanges();
			return CreateBlendedNode(null, syntaxToken);
		}

		private SyntaxToken LexNewToken(LexerMode mode)
		{
			if (_lexer.TextWindow.Position != _newPosition)
			{
				_lexer.Reset(_newPosition, _newDirectives);
			}
			if (mode >= LexerMode.XmlDocComment)
			{
				mode |= _newLexerDrivenMode;
			}
			SyntaxToken result = _lexer.Lex(ref mode);
			_newDirectives = _lexer.Directives;
			_newLexerDrivenMode = mode & (LexerMode.MaskXmlDocCommentLocation | LexerMode.MaskXmlDocCommentStyle);
			return result;
		}

		private bool TryTakeOldNodeOrToken(bool asToken, out BlendedNode blendedNode)
		{
			if (asToken)
			{
				_oldTreeCursor = _oldTreeCursor.MoveToFirstToken();
			}
			SyntaxNodeOrToken currentNodeOrToken = _oldTreeCursor.CurrentNodeOrToken;
			if (!CanReuse(currentNodeOrToken))
			{
				blendedNode = default;
				return false;
			}
			_newPosition += currentNodeOrToken.FullWidth;
			_oldTreeCursor = _oldTreeCursor.MoveToNextSibling();
			_newDirectives = currentNodeOrToken.ApplyDirectives(_newDirectives);
			_oldDirectives = currentNodeOrToken.ApplyDirectives(_oldDirectives);
			blendedNode = CreateBlendedNode((Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode)currentNodeOrToken.AsNode(), (SyntaxToken)currentNodeOrToken.AsToken().Node);
			return true;
		}

		private bool CanReuse(SyntaxNodeOrToken nodeOrToken)
		{
			if (nodeOrToken.FullWidth == 0)
			{
				return false;
			}
			if (nodeOrToken.ContainsAnnotations)
			{
				return false;
			}
			if (IntersectsNextChange(nodeOrToken))
			{
				return false;
			}
			if (nodeOrToken.ContainsDiagnostics || (nodeOrToken.IsToken && ((CSharpSyntaxNode)nodeOrToken.AsToken().Node).ContainsSkippedText && nodeOrToken.Parent.ContainsDiagnostics))
			{
				return false;
			}
			if (IsFabricatedToken(nodeOrToken.Kind()))
			{
				return false;
			}
			if ((nodeOrToken.IsToken && nodeOrToken.AsToken().IsMissing) || (nodeOrToken.IsNode && IsIncomplete((Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode)nodeOrToken.AsNode())))
			{
				return false;
			}
			if (!nodeOrToken.ContainsDirectives)
			{
				return true;
			}
			return _newDirectives.IncrementallyEquivalent(_oldDirectives);
		}

		private bool IntersectsNextChange(SyntaxNodeOrToken nodeOrToken)
		{
			if (_changes.IsEmpty)
			{
				return false;
			}
			TextSpan fullSpan = nodeOrToken.FullSpan;
			TextSpan span = _changes.Peek().Span;
			return fullSpan.IntersectsWith(span);
		}

		private static bool IsIncomplete(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode node)
		{
			return node.Green.GetLastTerminal().IsMissing;
		}

		private static bool IsFabricatedToken(SyntaxKind kind)
		{
			if (kind - 8274 <= SyntaxKind.List)
			{
				return true;
			}
			return SyntaxFacts.IsContextualKeyword(kind);
		}

		private BlendedNode CreateBlendedNode(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode node, SyntaxToken token)
		{
			return new BlendedNode(node, token, new Blender(_lexer, _oldTreeCursor, _changes, _newPosition, _changeDelta, _newDirectives, _oldDirectives, _newLexerDrivenMode));
		}
	}

	private readonly Lexer _lexer;

	private readonly Cursor _oldTreeCursor;

	private readonly ImmutableStack<TextChangeRange> _changes;

	private readonly int _newPosition;

	private readonly int _changeDelta;

	private readonly DirectiveStack _newDirectives;

	private readonly DirectiveStack _oldDirectives;

	private readonly LexerMode _newLexerDrivenMode;

	public Blender(Lexer lexer, Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode oldTree, IEnumerable<TextChangeRange> changes)
	{
		_lexer = lexer;
		_changes = ImmutableStack.Create<TextChangeRange>();
		if (changes != null)
		{
			TextChangeRange changeRange = TextChangeRange.Collapse(changes);
			TextChangeRange value = ExtendToAffectedRange(oldTree, changeRange);
			_changes = _changes.Push(value);
		}
		if (oldTree == null)
		{
			_oldTreeCursor = default;
			_newPosition = lexer.TextWindow.Position;
		}
		else
		{
			_oldTreeCursor = Cursor.FromRoot(oldTree).MoveToFirstChild();
			_newPosition = 0;
		}
		_changeDelta = 0;
		_newDirectives = default;
		_oldDirectives = default;
		_newLexerDrivenMode = LexerMode.XmlDocCommentLocationStart;
	}

	private Blender(Lexer lexer, Cursor oldTreeCursor, ImmutableStack<TextChangeRange> changes, int newPosition, int changeDelta, DirectiveStack newDirectives, DirectiveStack oldDirectives, LexerMode newLexerDrivenMode)
	{
		_lexer = lexer;
		_oldTreeCursor = oldTreeCursor;
		_changes = changes;
		_newPosition = newPosition;
		_changeDelta = changeDelta;
		_newDirectives = newDirectives;
		_oldDirectives = oldDirectives;
		_newLexerDrivenMode = newLexerDrivenMode & (LexerMode.MaskXmlDocCommentLocation | LexerMode.MaskXmlDocCommentStyle);
	}

	private static TextChangeRange ExtendToAffectedRange(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode oldTree, TextChangeRange changeRange)
	{
		int val = oldTree.FullWidth - 1;
		int num = Math.Max(Math.Min(changeRange.Span.Start, val), 0);
		int num2 = 0;
		while (num > 0 && num2 <= 1)
		{
			Microsoft.CodeAnalysis.SyntaxToken syntaxToken = oldTree.FindToken(num);
			num = Math.Max(0, syntaxToken.Position - 1);
			if (syntaxToken.FullWidth > 0)
			{
				num2++;
			}
		}
		if (IsInsideInterpolation(oldTree, num))
		{
			int character = oldTree.SyntaxTree.GetLineSpan(new TextSpan(num, 0)).Span.Start.Character;
			num = Math.Max(num - character, 0);
		}
		TextSpan span = TextSpan.FromBounds(num, changeRange.Span.End);
		int newLength = changeRange.NewLength + (changeRange.Span.Start - num);
		return new TextChangeRange(span, newLength);
	}

	private static bool IsInsideInterpolation(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode oldTree, int start)
	{
		for (SyntaxNode parent = oldTree.FindToken(start).Parent; parent != null; parent = parent.Parent)
		{
			if (parent.Kind() == SyntaxKind.InterpolatedStringExpression)
			{
				return true;
			}
		}
		return false;
	}

	public BlendedNode ReadNode(LexerMode mode)
	{
		return ReadNodeOrToken(mode, asToken: false);
	}

	public BlendedNode ReadToken(LexerMode mode)
	{
		return ReadNodeOrToken(mode, asToken: true);
	}

	private BlendedNode ReadNodeOrToken(LexerMode mode, bool asToken)
	{
		return new Reader(this).ReadNodeOrToken(mode, asToken);
	}
}
