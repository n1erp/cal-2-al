using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalFieldSyntax : InternalFieldBaseSyntax
{
	internal readonly InternalSyntaxToken _fieldKeyword;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalSyntaxToken _no;

	internal readonly InternalSyntaxToken _semicolonToken1;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _semicolonToken2;

	internal readonly InternalDataTypeSyntax _type;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalPropertyListSyntax _propertyList;

	internal readonly InternalSyntaxNode _triggers;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => FieldSyntaxDefinition.Instance.Value;

	public override InternalSyntaxToken FieldKeyword => _fieldKeyword;

	public override InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalSyntaxToken No => _no;

	public InternalSyntaxToken SemicolonToken1 => _semicolonToken1;

	public override InternalIdentifierNameSyntax Name => _name;

	public InternalSyntaxToken SemicolonToken2 => _semicolonToken2;

	public InternalDataTypeSyntax Type => _type;

	public override InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	public override InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public override InternalPropertyListSyntax PropertyList => _propertyList;

	public override InternalSyntaxList<InternalTriggerDeclarationSyntax> Triggers => new InternalSyntaxList<InternalTriggerDeclarationSyntax>(_triggers);

	public override InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalFieldSyntax(SyntaxKind kind, InternalSyntaxToken fieldKeyword, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken no, InternalSyntaxToken semicolonToken1, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken2, InternalDataTypeSyntax type, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode triggers, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 12;
		AdjustFlagsAndWidth(fieldKeyword);
		_fieldKeyword = fieldKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(no);
		_no = no;
		AdjustFlagsAndWidth(semicolonToken1);
		_semicolonToken1 = semicolonToken1;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken2);
		_semicolonToken2 = semicolonToken2;
		AdjustFlagsAndWidth(type);
		_type = type;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (triggers != null)
		{
			AdjustFlagsAndWidth(triggers);
			_triggers = triggers;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalFieldSyntax(SyntaxKind kind, InternalSyntaxToken fieldKeyword, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken no, InternalSyntaxToken semicolonToken1, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken2, InternalDataTypeSyntax type, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode triggers, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 12;
		AdjustFlagsAndWidth(fieldKeyword);
		_fieldKeyword = fieldKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(no);
		_no = no;
		AdjustFlagsAndWidth(semicolonToken1);
		_semicolonToken1 = semicolonToken1;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken2);
		_semicolonToken2 = semicolonToken2;
		AdjustFlagsAndWidth(type);
		_type = type;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (triggers != null)
		{
			AdjustFlagsAndWidth(triggers);
			_triggers = triggers;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalFieldSyntax(SyntaxKind kind, InternalSyntaxToken fieldKeyword, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken no, InternalSyntaxToken semicolonToken1, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken2, InternalDataTypeSyntax type, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode triggers, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 12;
		AdjustFlagsAndWidth(fieldKeyword);
		_fieldKeyword = fieldKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(no);
		_no = no;
		AdjustFlagsAndWidth(semicolonToken1);
		_semicolonToken1 = semicolonToken1;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken2);
		_semicolonToken2 = semicolonToken2;
		AdjustFlagsAndWidth(type);
		_type = type;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (triggers != null)
		{
			AdjustFlagsAndWidth(triggers);
			_triggers = triggers;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _fieldKeyword, 
			1 => _openParenthesisToken, 
			2 => _no, 
			3 => _semicolonToken1, 
			4 => _name, 
			5 => _semicolonToken2, 
			6 => _type, 
			7 => _closeParenthesisToken, 
			8 => _openBraceToken, 
			9 => _propertyList, 
			10 => _triggers, 
			11 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new FieldSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitField(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitField(this);
	}

	public InternalFieldSyntax Update(InternalSyntaxToken fieldKeyword, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken no, InternalSyntaxToken semicolonToken1, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken2, InternalDataTypeSyntax type, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		if (fieldKeyword != FieldKeyword || openParenthesisToken != OpenParenthesisToken || no != No || semicolonToken1 != SemicolonToken1 || name != Name || semicolonToken2 != SemicolonToken2 || type != Type || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || triggers != Triggers || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.Field(fieldKeyword, openParenthesisToken, no, semicolonToken1, name, semicolonToken2, type, closeParenthesisToken, openBraceToken, propertyList, triggers, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalFieldSyntax(Kind, _fieldKeyword, _openParenthesisToken, _no, _semicolonToken1, _name, _semicolonToken2, _type, _closeParenthesisToken, _openBraceToken, _propertyList, _triggers, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalFieldSyntax(Kind, _fieldKeyword, _openParenthesisToken, _no, _semicolonToken1, _name, _semicolonToken2, _type, _closeParenthesisToken, _openBraceToken, _propertyList, _triggers, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
