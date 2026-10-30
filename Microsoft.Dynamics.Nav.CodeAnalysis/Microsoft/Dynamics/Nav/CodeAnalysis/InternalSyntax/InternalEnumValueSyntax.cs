using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalEnumValueSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _enumValueToken;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalSyntaxToken _id;

	internal readonly InternalSyntaxToken _semicolonToken;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalPropertyListSyntax _propertyList;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => EnumValueSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken EnumValueToken => _enumValueToken;

	public InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalSyntaxToken Id => _id;

	public InternalSyntaxToken SemicolonToken => _semicolonToken;

	public InternalIdentifierNameSyntax Name => _name;

	public InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public InternalPropertyListSyntax PropertyList => _propertyList;

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalEnumValueSyntax(SyntaxKind kind, InternalSyntaxToken enumValueToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken id, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(enumValueToken);
		_enumValueToken = enumValueToken;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(id);
		_id = id;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (propertyList != null)
		{
			AdjustFlagsAndWidth(propertyList);
			_propertyList = propertyList;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalEnumValueSyntax(SyntaxKind kind, InternalSyntaxToken enumValueToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken id, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(enumValueToken);
		_enumValueToken = enumValueToken;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(id);
		_id = id;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (propertyList != null)
		{
			AdjustFlagsAndWidth(propertyList);
			_propertyList = propertyList;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalEnumValueSyntax(SyntaxKind kind, InternalSyntaxToken enumValueToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken id, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(enumValueToken);
		_enumValueToken = enumValueToken;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(id);
		_id = id;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (propertyList != null)
		{
			AdjustFlagsAndWidth(propertyList);
			_propertyList = propertyList;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_enumValueToken, 
			1 => _openParenthesisToken, 
			2 => _id, 
			3 => _semicolonToken, 
			4 => _name, 
			5 => _closeParenthesisToken, 
			6 => _openBraceToken, 
			7 => _propertyList, 
			8 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new EnumValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitEnumValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitEnumValue(this);
	}

	public InternalEnumValueSyntax Update(InternalSyntaxToken enumValueToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken id, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		if (enumValueToken != EnumValueToken || openParenthesisToken != OpenParenthesisToken || id != Id || semicolonToken != SemicolonToken || name != Name || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.EnumValue(enumValueToken, openParenthesisToken, id, semicolonToken, name, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalEnumValueSyntax(Kind, _enumValueToken, _openParenthesisToken, _id, _semicolonToken, _name, _closeParenthesisToken, _openBraceToken, _propertyList, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalEnumValueSyntax(Kind, _enumValueToken, _openParenthesisToken, _id, _semicolonToken, _name, _closeParenthesisToken, _openBraceToken, _propertyList, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
