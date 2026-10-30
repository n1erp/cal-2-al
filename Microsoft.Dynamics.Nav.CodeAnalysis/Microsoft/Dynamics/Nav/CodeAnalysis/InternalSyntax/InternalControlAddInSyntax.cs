using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalControlAddInSyntax : InternalObjectSyntax
{
	internal readonly InternalSyntaxToken _controlAddInKeyword;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalPropertyListSyntax _propertyList;

	internal readonly InternalSyntaxNode _members;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => ControlAddInSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken ControlAddInKeyword => _controlAddInKeyword;

	public override InternalIdentifierNameSyntax Name => _name;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public override InternalPropertyListSyntax PropertyList => _propertyList;

	public override InternalSyntaxList<InternalMemberSyntax> Members => new InternalSyntaxList<InternalMemberSyntax>(_members);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalControlAddInSyntax(SyntaxKind kind, InternalSyntaxToken controlAddInKeyword, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(controlAddInKeyword);
		_controlAddInKeyword = controlAddInKeyword;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			_members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalControlAddInSyntax(SyntaxKind kind, InternalSyntaxToken controlAddInKeyword, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(controlAddInKeyword);
		_controlAddInKeyword = controlAddInKeyword;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			_members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalControlAddInSyntax(SyntaxKind kind, InternalSyntaxToken controlAddInKeyword, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(controlAddInKeyword);
		_controlAddInKeyword = controlAddInKeyword;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			_members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _controlAddInKeyword, 
			1 => _name, 
			2 => _openBraceToken, 
			3 => _propertyList, 
			4 => _members, 
			5 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ControlAddInSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitControlAddIn(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitControlAddIn(this);
	}

	public InternalControlAddInSyntax Update(InternalSyntaxToken controlAddInKeyword, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		if (controlAddInKeyword != ControlAddInKeyword || name != Name || openBraceToken != OpenBraceToken || propertyList != PropertyList || members != Members || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.ControlAddIn(controlAddInKeyword, name, openBraceToken, propertyList, members, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalControlAddInSyntax(Kind, _controlAddInKeyword, _name, _openBraceToken, _propertyList, _members, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalControlAddInSyntax(Kind, _controlAddInKeyword, _name, _openBraceToken, _propertyList, _members, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
