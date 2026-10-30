using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalKeyListSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _keysKeyword;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalSyntaxNode _keys;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => KeyListSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken KeysKeyword => _keysKeyword;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public InternalSyntaxList<InternalKeySyntax> Keys => new InternalSyntaxList<InternalKeySyntax>(_keys);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalKeyListSyntax(SyntaxKind kind, InternalSyntaxToken keysKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode keys, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(keysKeyword);
		_keysKeyword = keysKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (keys != null)
		{
			AdjustFlagsAndWidth(keys);
			_keys = keys;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalKeyListSyntax(SyntaxKind kind, InternalSyntaxToken keysKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode keys, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(keysKeyword);
		_keysKeyword = keysKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (keys != null)
		{
			AdjustFlagsAndWidth(keys);
			_keys = keys;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalKeyListSyntax(SyntaxKind kind, InternalSyntaxToken keysKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode keys, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(keysKeyword);
		_keysKeyword = keysKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (keys != null)
		{
			AdjustFlagsAndWidth(keys);
			_keys = keys;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _keysKeyword, 
			1 => _openBraceToken, 
			2 => _keys, 
			3 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new KeyListSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitKeyList(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitKeyList(this);
	}

	public InternalKeyListSyntax Update(InternalSyntaxToken keysKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalKeySyntax> keys, InternalSyntaxToken closeBraceToken)
	{
		if (keysKeyword != KeysKeyword || openBraceToken != OpenBraceToken || keys != Keys || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.KeyList(keysKeyword, openBraceToken, keys, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalKeyListSyntax(Kind, _keysKeyword, _openBraceToken, _keys, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalKeyListSyntax(Kind, _keysKeyword, _openBraceToken, _keys, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
