using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class DotNetPackageSyntax : ObjectSyntax
{
	private IdentifierNameSyntax _name;

	private PropertyListSyntax _propertyList;

	private SyntaxNode _assemblies;

	private SyntaxNode _members;

	public SyntaxToken DotNetKeyword => new SyntaxToken(this, ((InternalDotNetPackageSyntax)Node)._dotNetKeyword, Position, 0);

	public override IdentifierNameSyntax Name => GetRed(ref _name, 1);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalDotNetPackageSyntax)Node)._openBraceToken, GetChildPosition(2), GetChildIndex(2));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 3);

	public SyntaxList<DotNetAssemblySyntax> Assemblies => new SyntaxList<DotNetAssemblySyntax>(GetRed(ref _assemblies, 4));

	public override SyntaxList<MemberSyntax> Members => new SyntaxList<MemberSyntax>(GetRed(ref _members, 5));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalDotNetPackageSyntax)Node)._closeBraceToken, GetChildPosition(6), GetChildIndex(6));

	internal DotNetPackageSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => GetRed(ref _name, 1), 
			3 => GetRed(ref _propertyList, 3), 
			4 => GetRed(ref _assemblies, 4), 
			5 => GetRed(ref _members, 5), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => _name, 
			3 => _propertyList, 
			4 => _assemblies, 
			5 => _members, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDotNetPackage(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitDotNetPackage(this);
	}

	public DotNetPackageSyntax Update(SyntaxToken dotNetKeyword, IdentifierNameSyntax name, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<DotNetAssemblySyntax> assemblies, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		if (dotNetKeyword != DotNetKeyword || name != Name || openBraceToken != OpenBraceToken || propertyList != PropertyList || assemblies != Assemblies || members != Members || closeBraceToken != CloseBraceToken)
		{
			DotNetPackageSyntax dotNetPackageSyntax = SyntaxFactory.DotNetPackage(dotNetKeyword, name, openBraceToken, propertyList, assemblies, members, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return dotNetPackageSyntax.WithAnnotations(annotations);
			}
			return dotNetPackageSyntax;
		}
		return this;
	}

	public DotNetPackageSyntax WithDotNetKeyword(SyntaxToken dotNetKeyword)
	{
		return Update(dotNetKeyword, Name, OpenBraceToken, PropertyList, Assemblies, Members, CloseBraceToken);
	}

	public DotNetPackageSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(DotNetKeyword, name, OpenBraceToken, PropertyList, Assemblies, Members, CloseBraceToken);
	}

	public DotNetPackageSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(DotNetKeyword, Name, openBraceToken, PropertyList, Assemblies, Members, CloseBraceToken);
	}

	public DotNetPackageSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(DotNetKeyword, Name, OpenBraceToken, propertyList, Assemblies, Members, CloseBraceToken);
	}

	public DotNetPackageSyntax WithAssemblies(SyntaxList<DotNetAssemblySyntax> assemblies)
	{
		return Update(DotNetKeyword, Name, OpenBraceToken, PropertyList, assemblies, Members, CloseBraceToken);
	}

	public DotNetPackageSyntax WithMembers(SyntaxList<MemberSyntax> members)
	{
		return Update(DotNetKeyword, Name, OpenBraceToken, PropertyList, Assemblies, members, CloseBraceToken);
	}

	public DotNetPackageSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(DotNetKeyword, Name, OpenBraceToken, PropertyList, Assemblies, Members, closeBraceToken);
	}

	public DotNetPackageSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public DotNetPackageSyntax AddAssemblies(params DotNetAssemblySyntax[] items)
	{
		return WithAssemblies(Assemblies.AddRange(items));
	}

	public DotNetPackageSyntax AddMembers(params MemberSyntax[] items)
	{
		return WithMembers(Members.AddRange(items));
	}
}
