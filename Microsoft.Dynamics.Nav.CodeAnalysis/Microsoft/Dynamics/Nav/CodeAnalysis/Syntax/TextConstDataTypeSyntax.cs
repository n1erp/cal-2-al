using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class TextConstDataTypeSyntax : DataTypeSyntax
{
	private CommaSeparatedIdentifierEqualsStringListSyntax _multilanguage;

	public override SyntaxToken TypeName => new SyntaxToken(this, ((InternalTextConstDataTypeSyntax)Node)._typeName, Position, 0);

	public CommaSeparatedIdentifierEqualsStringListSyntax Multilanguage => GetRed(ref _multilanguage, 1);

	internal TextConstDataTypeSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref _multilanguage, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return _multilanguage;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTextConstDataType(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitTextConstDataType(this);
	}

	public TextConstDataTypeSyntax Update(SyntaxToken typeName, CommaSeparatedIdentifierEqualsStringListSyntax multilanguage)
	{
		if (typeName != TypeName || multilanguage != Multilanguage)
		{
			TextConstDataTypeSyntax textConstDataTypeSyntax = SyntaxFactory.TextConstDataType(typeName, multilanguage);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return textConstDataTypeSyntax.WithAnnotations(annotations);
			}
			return textConstDataTypeSyntax;
		}
		return this;
	}

	public TextConstDataTypeSyntax WithTypeName(SyntaxToken typeName)
	{
		return Update(typeName, Multilanguage);
	}

	public TextConstDataTypeSyntax WithMultilanguage(CommaSeparatedIdentifierEqualsStringListSyntax multilanguage)
	{
		return Update(TypeName, multilanguage);
	}

	public TextConstDataTypeSyntax AddMultilanguageValues(params IdentifierEqualsStringSyntax[] items)
	{
		CommaSeparatedIdentifierEqualsStringListSyntax commaSeparatedIdentifierEqualsStringListSyntax = Multilanguage ?? SyntaxFactory.CommaSeparatedIdentifierEqualsStringList();
		return WithMultilanguage(commaSeparatedIdentifierEqualsStringListSyntax.WithValues(commaSeparatedIdentifierEqualsStringListSyntax.Values.AddRange(items)));
	}
}
