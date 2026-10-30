using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalKeySyntax : InternalKeyFieldGroupBaseSyntax
{
	internal readonly InternalSyntaxToken _keyKeyword;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalSyntaxToken _id;

	internal readonly InternalSyntaxToken _semicolonToken1;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _semicolonToken2;

	internal readonly InternalSyntaxNode _fields;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalPropertyListSyntax _propertyList;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => KeySyntaxDefinition.Instance.Value;

	public InternalSyntaxToken KeyKeyword => _keyKeyword;

	public override InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public override InternalSyntaxToken Id => _id;

	public override InternalSyntaxToken SemicolonToken1 => _semicolonToken1;

	public override InternalIdentifierNameSyntax Name => _name;

	public override InternalSyntaxToken SemicolonToken2 => _semicolonToken2;

	public override InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> Fields => new InternalSeparatedSyntaxList<InternalIdentifierNameSyntax>(new InternalSyntaxList<InternalSyntaxNode>(_fields));

	public override InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	public override InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public override InternalPropertyListSyntax PropertyList => _propertyList;

	public override InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalKeySyntax(SyntaxKind kind, InternalSyntaxToken keyKeyword, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken id, InternalSyntaxToken semicolonToken1, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken2, InternalSyntaxNode fields, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 11;
		AdjustFlagsAndWidth(keyKeyword);
		_keyKeyword = keyKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		if (id != null)
		{
			AdjustFlagsAndWidth(id);
			_id = id;
		}
		if (semicolonToken1 != null)
		{
			AdjustFlagsAndWidth(semicolonToken1);
			_semicolonToken1 = semicolonToken1;
		}
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken2);
		_semicolonToken2 = semicolonToken2;
		if (fields != null)
		{
			AdjustFlagsAndWidth(fields);
			_fields = fields;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalKeySyntax(SyntaxKind kind, InternalSyntaxToken keyKeyword, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken id, InternalSyntaxToken semicolonToken1, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken2, InternalSyntaxNode fields, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 11;
		AdjustFlagsAndWidth(keyKeyword);
		_keyKeyword = keyKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		if (id != null)
		{
			AdjustFlagsAndWidth(id);
			_id = id;
		}
		if (semicolonToken1 != null)
		{
			AdjustFlagsAndWidth(semicolonToken1);
			_semicolonToken1 = semicolonToken1;
		}
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken2);
		_semicolonToken2 = semicolonToken2;
		if (fields != null)
		{
			AdjustFlagsAndWidth(fields);
			_fields = fields;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalKeySyntax(SyntaxKind kind, InternalSyntaxToken keyKeyword, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken id, InternalSyntaxToken semicolonToken1, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken2, InternalSyntaxNode fields, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 11;
		AdjustFlagsAndWidth(keyKeyword);
		_keyKeyword = keyKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		if (id != null)
		{
			AdjustFlagsAndWidth(id);
			_id = id;
		}
		if (semicolonToken1 != null)
		{
			AdjustFlagsAndWidth(semicolonToken1);
			_semicolonToken1 = semicolonToken1;
		}
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken2);
		_semicolonToken2 = semicolonToken2;
		if (fields != null)
		{
			AdjustFlagsAndWidth(fields);
			_fields = fields;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _keyKeyword, 
			1 => _openParenthesisToken, 
			2 => _id, 
			3 => _semicolonToken1, 
			4 => _name, 
			5 => _semicolonToken2, 
			6 => _fields, 
			7 => _closeParenthesisToken, 
			8 => _openBraceToken, 
			9 => _propertyList, 
			10 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new KeySyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitKey(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitKey(this);
	}

	public InternalKeySyntax Update(InternalSyntaxToken keyKeyword, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken id, InternalSyntaxToken semicolonToken1, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken2, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> fields, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		if (keyKeyword != KeyKeyword || openParenthesisToken != OpenParenthesisToken || id != Id || semicolonToken1 != SemicolonToken1 || name != Name || semicolonToken2 != SemicolonToken2 || fields != Fields || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.Key(keyKeyword, openParenthesisToken, id, semicolonToken1, name, semicolonToken2, fields, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalKeySyntax(Kind, _keyKeyword, _openParenthesisToken, _id, _semicolonToken1, _name, _semicolonToken2, _fields, _closeParenthesisToken, _openBraceToken, _propertyList, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalKeySyntax(Kind, _keyKeyword, _openParenthesisToken, _id, _semicolonToken1, _name, _semicolonToken2, _fields, _closeParenthesisToken, _openBraceToken, _propertyList, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
