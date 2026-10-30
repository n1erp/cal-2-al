using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PropertySyntax : PropertySyntaxOrEmpty
{
	private PropertyNameSyntax _name;

	private PropertyValueSyntax _value;

	public PropertyNameSyntax Name => GetRedAtZero(ref _name);

	public SyntaxToken EqualsToken => new SyntaxToken(this, ((InternalPropertySyntax)Node)._equalsToken, GetChildPosition(1), GetChildIndex(1));

	public PropertyValueSyntax Value => GetRed(ref _value, 2);

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((InternalPropertySyntax)Node)._semicolonToken, GetChildPosition(3), GetChildIndex(3));

	internal PropertySyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref _name), 
			2 => GetRed(ref _value, 2), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)_name, 
			2 => _value, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitProperty(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitProperty(this);
	}

	public PropertySyntax Update(PropertyNameSyntax name, SyntaxToken equalsToken, PropertyValueSyntax value, SyntaxToken semicolonToken)
	{
		if (name != Name || equalsToken != EqualsToken || value != Value || semicolonToken != SemicolonToken)
		{
			PropertySyntax propertySyntax = SyntaxFactory.Property(name, equalsToken, value, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return propertySyntax.WithAnnotations(annotations);
			}
			return propertySyntax;
		}
		return this;
	}

	public PropertySyntax WithName(PropertyNameSyntax name)
	{
		return Update(name, EqualsToken, Value, SemicolonToken);
	}

	public PropertySyntax WithEqualsToken(SyntaxToken equalsToken)
	{
		return Update(Name, equalsToken, Value, SemicolonToken);
	}

	public PropertySyntax WithValue(PropertyValueSyntax value)
	{
		return Update(Name, EqualsToken, value, SemicolonToken);
	}

	public PropertySyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(Name, EqualsToken, Value, semicolonToken);
	}
}
