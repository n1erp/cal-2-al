using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class CodeunitSyntax : ApplicationObjectSyntax
{
	private ObjectIdSyntax _objectId;

	private IdentifierNameSyntax _name;

	private PropertyListSyntax _propertyList;

	private SyntaxNode _members;

	public SyntaxToken CodeunitKeyword => new SyntaxToken(this, ((InternalCodeunitSyntax)Node)._codeunitKeyword, Position, 0);

	public override ObjectIdSyntax ObjectId => GetRed(ref _objectId, 1);

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalCodeunitSyntax)Node)._openBraceToken, GetChildPosition(3), GetChildIndex(3));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 4);

	public override SyntaxList<MemberSyntax> Members => new SyntaxList<MemberSyntax>(GetRed(ref _members, 5));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalCodeunitSyntax)Node)._closeBraceToken, GetChildPosition(6), GetChildIndex(6));

	internal CodeunitSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => GetRed(ref _objectId, 1), 
			2 => GetRed(ref _name, 2), 
			4 => GetRed(ref _propertyList, 4), 
			5 => GetRed(ref _members, 5), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => _objectId, 
			2 => _name, 
			4 => _propertyList, 
			5 => _members, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCodeunit(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitCodeunit(this);
	}

	public CodeunitSyntax Update(SyntaxToken codeunitKeyword, ObjectIdSyntax objectId, IdentifierNameSyntax name, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		if (codeunitKeyword != CodeunitKeyword || objectId != ObjectId || name != Name || openBraceToken != OpenBraceToken || propertyList != PropertyList || members != Members || closeBraceToken != CloseBraceToken)
		{
			CodeunitSyntax codeunitSyntax = SyntaxFactory.Codeunit(codeunitKeyword, objectId, name, openBraceToken, propertyList, members, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return codeunitSyntax.WithAnnotations(annotations);
			}
			return codeunitSyntax;
		}
		return this;
	}

	public CodeunitSyntax WithCodeunitKeyword(SyntaxToken codeunitKeyword)
	{
		return Update(codeunitKeyword, ObjectId, Name, OpenBraceToken, PropertyList, Members, CloseBraceToken);
	}

	public CodeunitSyntax WithObjectId(ObjectIdSyntax objectId)
	{
		return Update(CodeunitKeyword, objectId, Name, OpenBraceToken, PropertyList, Members, CloseBraceToken);
	}

	public CodeunitSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(CodeunitKeyword, ObjectId, name, OpenBraceToken, PropertyList, Members, CloseBraceToken);
	}

	public CodeunitSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(CodeunitKeyword, ObjectId, Name, openBraceToken, PropertyList, Members, CloseBraceToken);
	}

	public CodeunitSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(CodeunitKeyword, ObjectId, Name, OpenBraceToken, propertyList, Members, CloseBraceToken);
	}

	public CodeunitSyntax WithMembers(SyntaxList<MemberSyntax> members)
	{
		return Update(CodeunitKeyword, ObjectId, Name, OpenBraceToken, PropertyList, members, CloseBraceToken);
	}

	public CodeunitSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(CodeunitKeyword, ObjectId, Name, OpenBraceToken, PropertyList, Members, closeBraceToken);
	}

	public CodeunitSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public CodeunitSyntax AddMembers(params MemberSyntax[] items)
	{
		return WithMembers(Members.AddRange(items));
	}
}
