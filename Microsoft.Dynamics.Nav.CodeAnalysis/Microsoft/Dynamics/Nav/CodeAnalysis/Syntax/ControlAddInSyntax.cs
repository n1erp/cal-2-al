using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ControlAddInSyntax : ObjectSyntax
{
	private IdentifierNameSyntax _name;

	private PropertyListSyntax _propertyList;

	private SyntaxNode _members;

	public SyntaxToken ControlAddInKeyword => new SyntaxToken(this, ((InternalControlAddInSyntax)Node)._controlAddInKeyword, Position, 0);

	public override IdentifierNameSyntax Name => GetRed(ref _name, 1);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalControlAddInSyntax)Node)._openBraceToken, GetChildPosition(2), GetChildIndex(2));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 3);

	public override SyntaxList<MemberSyntax> Members => new SyntaxList<MemberSyntax>(GetRed(ref _members, 4));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalControlAddInSyntax)Node)._closeBraceToken, GetChildPosition(5), GetChildIndex(5));

	internal ControlAddInSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => GetRed(ref _name, 1), 
			3 => GetRed(ref _propertyList, 3), 
			4 => GetRed(ref _members, 4), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => _name, 
			3 => _propertyList, 
			4 => _members, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitControlAddIn(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitControlAddIn(this);
	}

	public ControlAddInSyntax Update(SyntaxToken controlAddInKeyword, IdentifierNameSyntax name, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		if (controlAddInKeyword != ControlAddInKeyword || name != Name || openBraceToken != OpenBraceToken || propertyList != PropertyList || members != Members || closeBraceToken != CloseBraceToken)
		{
			ControlAddInSyntax controlAddInSyntax = SyntaxFactory.ControlAddIn(controlAddInKeyword, name, openBraceToken, propertyList, members, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return controlAddInSyntax.WithAnnotations(annotations);
			}
			return controlAddInSyntax;
		}
		return this;
	}

	public ControlAddInSyntax WithControlAddInKeyword(SyntaxToken controlAddInKeyword)
	{
		return Update(controlAddInKeyword, Name, OpenBraceToken, PropertyList, Members, CloseBraceToken);
	}

	public ControlAddInSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(ControlAddInKeyword, name, OpenBraceToken, PropertyList, Members, CloseBraceToken);
	}

	public ControlAddInSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ControlAddInKeyword, Name, openBraceToken, PropertyList, Members, CloseBraceToken);
	}

	public ControlAddInSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(ControlAddInKeyword, Name, OpenBraceToken, propertyList, Members, CloseBraceToken);
	}

	public ControlAddInSyntax WithMembers(SyntaxList<MemberSyntax> members)
	{
		return Update(ControlAddInKeyword, Name, OpenBraceToken, PropertyList, members, CloseBraceToken);
	}

	public ControlAddInSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ControlAddInKeyword, Name, OpenBraceToken, PropertyList, Members, closeBraceToken);
	}

	public ControlAddInSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public ControlAddInSyntax AddMembers(params MemberSyntax[] items)
	{
		return WithMembers(Members.AddRange(items));
	}
}
