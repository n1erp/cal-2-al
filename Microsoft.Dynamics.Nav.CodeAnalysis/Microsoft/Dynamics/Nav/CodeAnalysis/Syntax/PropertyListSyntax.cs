using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PropertyListSyntax : SyntaxNode
{
	private SyntaxNode _properties;

	public SyntaxList<PropertySyntaxOrEmpty> Properties => new SyntaxList<PropertySyntaxOrEmpty>(GetRed(ref _properties, 0));

	internal PropertyListSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _properties);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _properties;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPropertyList(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPropertyList(this);
	}

	public PropertyListSyntax Update(SyntaxList<PropertySyntaxOrEmpty> properties)
	{
		if (properties != Properties)
		{
			PropertyListSyntax propertyListSyntax = SyntaxFactory.PropertyList(properties);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return propertyListSyntax.WithAnnotations(annotations);
			}
			return propertyListSyntax;
		}
		return this;
	}

	public PropertyListSyntax WithProperties(SyntaxList<PropertySyntaxOrEmpty> properties)
	{
		return Update(properties);
	}

	public PropertyListSyntax AddProperties(params PropertySyntaxOrEmpty[] items)
	{
		return Update(Properties.AddRange(items));
	}
}
