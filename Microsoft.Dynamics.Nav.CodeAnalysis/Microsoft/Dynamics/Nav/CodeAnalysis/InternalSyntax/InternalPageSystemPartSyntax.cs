using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalPageSystemPartSyntax : InternalControlBaseSyntax
{
	internal readonly InternalSyntaxToken _controlKeyword;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _semicolonToken;

	internal readonly InternalIdentifierNameSyntax _systemPartType;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalPropertyListSyntax _propertyList;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => PageSystemPartSyntaxDefinition.Instance.Value;

	public override InternalSyntaxToken ControlKeyword => _controlKeyword;

	public override InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public override InternalIdentifierNameSyntax Name => _name;

	public InternalSyntaxToken SemicolonToken => _semicolonToken;

	public InternalIdentifierNameSyntax SystemPartType => _systemPartType;

	public override InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	public override InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public override InternalPropertyListSyntax PropertyList => _propertyList;

	public override InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalPageSystemPartSyntax(SyntaxKind kind, InternalSyntaxToken controlKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax systemPartType, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(controlKeyword);
		_controlKeyword = controlKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		AdjustFlagsAndWidth(systemPartType);
		_systemPartType = systemPartType;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalPageSystemPartSyntax(SyntaxKind kind, InternalSyntaxToken controlKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax systemPartType, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(controlKeyword);
		_controlKeyword = controlKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		AdjustFlagsAndWidth(systemPartType);
		_systemPartType = systemPartType;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalPageSystemPartSyntax(SyntaxKind kind, InternalSyntaxToken controlKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax systemPartType, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(controlKeyword);
		_controlKeyword = controlKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		AdjustFlagsAndWidth(systemPartType);
		_systemPartType = systemPartType;
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
			0 => (InternalSyntaxNode)_controlKeyword, 
			1 => _openParenthesisToken, 
			2 => _name, 
			3 => _semicolonToken, 
			4 => _systemPartType, 
			5 => _closeParenthesisToken, 
			6 => _openBraceToken, 
			7 => _propertyList, 
			8 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new PageSystemPartSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPageSystemPart(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitPageSystemPart(this);
	}

	public InternalPageSystemPartSyntax Update(InternalSyntaxToken controlKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax systemPartType, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		if (controlKeyword != ControlKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || semicolonToken != SemicolonToken || systemPartType != SystemPartType || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.PageSystemPart(controlKeyword, openParenthesisToken, name, semicolonToken, systemPartType, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalPageSystemPartSyntax(Kind, _controlKeyword, _openParenthesisToken, _name, _semicolonToken, _systemPartType, _closeParenthesisToken, _openBraceToken, _propertyList, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalPageSystemPartSyntax(Kind, _controlKeyword, _openParenthesisToken, _name, _semicolonToken, _systemPartType, _closeParenthesisToken, _openBraceToken, _propertyList, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
