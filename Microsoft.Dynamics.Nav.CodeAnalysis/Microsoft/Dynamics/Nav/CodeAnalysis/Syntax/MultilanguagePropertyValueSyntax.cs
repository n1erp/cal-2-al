using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class MultilanguagePropertyValueSyntax : PropertyValueSyntax
{
	private CommaSeparatedIdentifierEqualsStringListSyntax _value;

	public CommaSeparatedIdentifierEqualsStringListSyntax Value => GetRedAtZero(ref _value);

	internal MultilanguagePropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _value);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _value;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitMultilanguagePropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitMultilanguagePropertyValue(this);
	}

	public MultilanguagePropertyValueSyntax Update(CommaSeparatedIdentifierEqualsStringListSyntax value)
	{
		if (value != Value)
		{
			MultilanguagePropertyValueSyntax multilanguagePropertyValueSyntax = SyntaxFactory.MultilanguagePropertyValue(value);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return multilanguagePropertyValueSyntax.WithAnnotations(annotations);
			}
			return multilanguagePropertyValueSyntax;
		}
		return this;
	}

	public MultilanguagePropertyValueSyntax WithValue(CommaSeparatedIdentifierEqualsStringListSyntax value)
	{
		return Update(value);
	}

	public MultilanguagePropertyValueSyntax AddValueValues(params IdentifierEqualsStringSyntax[] items)
	{
		return Update(Value.WithValues(Value.Values.AddRange(items)));
	}
}
