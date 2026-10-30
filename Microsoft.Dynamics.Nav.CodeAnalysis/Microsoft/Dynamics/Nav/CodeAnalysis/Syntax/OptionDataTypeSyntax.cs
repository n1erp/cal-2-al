using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class OptionDataTypeSyntax : DataTypeSyntax
{
	private OptionValuesSyntax _optionValues;

	public override SyntaxToken TypeName => new SyntaxToken(this, ((InternalOptionDataTypeSyntax)Node)._typeName, Position, 0);

	public OptionValuesSyntax OptionValues => GetRed(ref _optionValues, 1);

	internal OptionDataTypeSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref _optionValues, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return _optionValues;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitOptionDataType(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitOptionDataType(this);
	}

	public OptionDataTypeSyntax Update(SyntaxToken typeName, OptionValuesSyntax optionValues)
	{
		if (typeName != TypeName || optionValues != OptionValues)
		{
			OptionDataTypeSyntax optionDataTypeSyntax = SyntaxFactory.OptionDataType(typeName, optionValues);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return optionDataTypeSyntax.WithAnnotations(annotations);
			}
			return optionDataTypeSyntax;
		}
		return this;
	}

	public OptionDataTypeSyntax WithTypeName(SyntaxToken typeName)
	{
		return Update(typeName, OptionValues);
	}

	public OptionDataTypeSyntax WithOptionValues(OptionValuesSyntax optionValues)
	{
		return Update(TypeName, optionValues);
	}

	public OptionDataTypeSyntax AddOptionValuesOptions(params IdentifierNameOrEmptySyntax[] items)
	{
		OptionValuesSyntax optionValuesSyntax = OptionValues ?? SyntaxFactory.OptionValues();
		return WithOptionValues(optionValuesSyntax.WithOptions(optionValuesSyntax.Options.AddRange(items)));
	}
}
