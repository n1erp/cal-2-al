using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class KeyListSyntax : SyntaxNode
{
	private SyntaxNode _keys;

	public SyntaxToken KeysKeyword => new SyntaxToken(this, ((InternalKeyListSyntax)Node)._keysKeyword, Position, 0);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalKeyListSyntax)Node)._openBraceToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxList<KeySyntax> Keys => new SyntaxList<KeySyntax>(GetRed(ref _keys, 2));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalKeyListSyntax)Node)._closeBraceToken, GetChildPosition(3), GetChildIndex(3));

	internal KeyListSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 2)
		{
			return GetRed(ref _keys, 2);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 2)
		{
			return _keys;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitKeyList(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitKeyList(this);
	}

	public KeyListSyntax Update(SyntaxToken keysKeyword, SyntaxToken openBraceToken, SyntaxList<KeySyntax> keys, SyntaxToken closeBraceToken)
	{
		if (keysKeyword != KeysKeyword || openBraceToken != OpenBraceToken || keys != Keys || closeBraceToken != CloseBraceToken)
		{
			KeyListSyntax keyListSyntax = SyntaxFactory.KeyList(keysKeyword, openBraceToken, keys, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return keyListSyntax.WithAnnotations(annotations);
			}
			return keyListSyntax;
		}
		return this;
	}

	public KeyListSyntax WithKeysKeyword(SyntaxToken keysKeyword)
	{
		return Update(keysKeyword, OpenBraceToken, Keys, CloseBraceToken);
	}

	public KeyListSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(KeysKeyword, openBraceToken, Keys, CloseBraceToken);
	}

	public KeyListSyntax WithKeys(SyntaxList<KeySyntax> keys)
	{
		return Update(KeysKeyword, OpenBraceToken, keys, CloseBraceToken);
	}

	public KeyListSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(KeysKeyword, OpenBraceToken, Keys, closeBraceToken);
	}

	public KeyListSyntax AddKeys(params KeySyntax[] items)
	{
		return WithKeys(Keys.AddRange(items));
	}
}
