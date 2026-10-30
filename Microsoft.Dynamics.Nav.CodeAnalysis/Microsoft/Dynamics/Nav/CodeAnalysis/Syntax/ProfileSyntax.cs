using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ProfileSyntax : ObjectSyntax
{
	private IdentifierNameSyntax _name;

	private PropertyListSyntax _propertyList;

	private SyntaxNode _members;

	public SyntaxToken ProfileToken => new SyntaxToken(this, ((InternalProfileSyntax)Node)._profileToken, Position, 0);

	public override IdentifierNameSyntax Name => GetRed(ref _name, 1);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalProfileSyntax)Node)._openBraceToken, GetChildPosition(2), GetChildIndex(2));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 3);

	public override SyntaxList<MemberSyntax> Members => new SyntaxList<MemberSyntax>(GetRed(ref _members, 4));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalProfileSyntax)Node)._closeBraceToken, GetChildPosition(5), GetChildIndex(5));

	internal ProfileSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitProfile(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitProfile(this);
	}

	public ProfileSyntax Update(SyntaxToken profileToken, IdentifierNameSyntax name, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		if (profileToken != ProfileToken || name != Name || openBraceToken != OpenBraceToken || propertyList != PropertyList || members != Members || closeBraceToken != CloseBraceToken)
		{
			ProfileSyntax profileSyntax = SyntaxFactory.Profile(profileToken, name, openBraceToken, propertyList, members, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return profileSyntax.WithAnnotations(annotations);
			}
			return profileSyntax;
		}
		return this;
	}

	public ProfileSyntax WithProfileToken(SyntaxToken profileToken)
	{
		return Update(profileToken, Name, OpenBraceToken, PropertyList, Members, CloseBraceToken);
	}

	public ProfileSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(ProfileToken, name, OpenBraceToken, PropertyList, Members, CloseBraceToken);
	}

	public ProfileSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ProfileToken, Name, openBraceToken, PropertyList, Members, CloseBraceToken);
	}

	public ProfileSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(ProfileToken, Name, OpenBraceToken, propertyList, Members, CloseBraceToken);
	}

	public ProfileSyntax WithMembers(SyntaxList<MemberSyntax> members)
	{
		return Update(ProfileToken, Name, OpenBraceToken, PropertyList, members, CloseBraceToken);
	}

	public ProfileSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ProfileToken, Name, OpenBraceToken, PropertyList, Members, closeBraceToken);
	}

	public ProfileSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public ProfileSyntax AddMembers(params MemberSyntax[] items)
	{
		return WithMembers(Members.AddRange(items));
	}
}
