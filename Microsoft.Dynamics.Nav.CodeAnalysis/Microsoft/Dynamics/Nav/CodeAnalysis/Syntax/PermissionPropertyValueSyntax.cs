using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PermissionPropertyValueSyntax : PropertyValueSyntax
{
	private SyntaxNode _permissionProperties;

	public SeparatedSyntaxList<PermissionSyntax> PermissionProperties
	{
		get
		{
			SyntaxNode red = GetRed(ref _permissionProperties, 0);
			if (red != null)
			{
				return new SeparatedSyntaxList<PermissionSyntax>(red, 0);
			}
			return default;
		}
	}

	internal PermissionPropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _permissionProperties);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _permissionProperties;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPermissionPropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPermissionPropertyValue(this);
	}

	public PermissionPropertyValueSyntax Update(SeparatedSyntaxList<PermissionSyntax> permissionProperties)
	{
		if (permissionProperties != PermissionProperties)
		{
			PermissionPropertyValueSyntax permissionPropertyValueSyntax = SyntaxFactory.PermissionPropertyValue(permissionProperties);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return permissionPropertyValueSyntax.WithAnnotations(annotations);
			}
			return permissionPropertyValueSyntax;
		}
		return this;
	}

	public PermissionPropertyValueSyntax WithPermissionProperties(SeparatedSyntaxList<PermissionSyntax> permissionProperties)
	{
		return Update(permissionProperties);
	}

	public PermissionPropertyValueSyntax AddPermissionProperties(params PermissionSyntax[] items)
	{
		return Update(PermissionProperties.AddRange(items));
	}
}
