using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PermissionSyntax : SyntaxNode
{
	private ObjectNameOrIdSyntax _objectReference;

	public SyntaxToken ObjectType => new SyntaxToken(this, ((InternalPermissionSyntax)Node)._objectType, Position, 0);

	public ObjectNameOrIdSyntax ObjectReference => GetRed(ref _objectReference, 1);

	public SyntaxToken EqualsToken => new SyntaxToken(this, ((InternalPermissionSyntax)Node)._equalsToken, GetChildPosition(2), GetChildIndex(2));

	public SyntaxToken Permissions
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalPermissionSyntax)Node)._permissions;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(3), GetChildIndex(3));
			}
			return default;
		}
	}

	internal PermissionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref _objectReference, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return _objectReference;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPermission(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPermission(this);
	}

	public PermissionSyntax Update(SyntaxToken objectType, ObjectNameOrIdSyntax objectReference, SyntaxToken equalsToken, SyntaxToken permissions)
	{
		if (objectType != ObjectType || objectReference != ObjectReference || equalsToken != EqualsToken || permissions != Permissions)
		{
			PermissionSyntax permissionSyntax = SyntaxFactory.Permission(objectType, objectReference, equalsToken, permissions);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return permissionSyntax.WithAnnotations(annotations);
			}
			return permissionSyntax;
		}
		return this;
	}

	public PermissionSyntax WithObjectType(SyntaxToken objectType)
	{
		return Update(objectType, ObjectReference, EqualsToken, Permissions);
	}

	public PermissionSyntax WithObjectReference(ObjectNameOrIdSyntax objectReference)
	{
		return Update(ObjectType, objectReference, EqualsToken, Permissions);
	}

	public PermissionSyntax WithEqualsToken(SyntaxToken equalsToken)
	{
		return Update(ObjectType, ObjectReference, equalsToken, Permissions);
	}

	public PermissionSyntax WithPermissions(SyntaxToken permissions)
	{
		return Update(ObjectType, ObjectReference, EqualsToken, permissions);
	}
}
