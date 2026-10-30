using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class CommaSeparatedIdentifierEqualsStringListPropertyValueSyntax : SeparatedListPropertyValueSyntax
{
	private CommaSeparatedIdentifierEqualsStringListSyntax _list;

	public CommaSeparatedIdentifierEqualsStringListSyntax List => GetRedAtZero(ref _list);

	internal CommaSeparatedIdentifierEqualsStringListPropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _list);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _list;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCommaSeparatedIdentifierEqualsStringListPropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitCommaSeparatedIdentifierEqualsStringListPropertyValue(this);
	}

	public CommaSeparatedIdentifierEqualsStringListPropertyValueSyntax Update(CommaSeparatedIdentifierEqualsStringListSyntax list)
	{
		if (list != List)
		{
			CommaSeparatedIdentifierEqualsStringListPropertyValueSyntax commaSeparatedIdentifierEqualsStringListPropertyValueSyntax = SyntaxFactory.CommaSeparatedIdentifierEqualsStringListPropertyValue(list);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return commaSeparatedIdentifierEqualsStringListPropertyValueSyntax.WithAnnotations(annotations);
			}
			return commaSeparatedIdentifierEqualsStringListPropertyValueSyntax;
		}
		return this;
	}

	public CommaSeparatedIdentifierEqualsStringListPropertyValueSyntax WithList(CommaSeparatedIdentifierEqualsStringListSyntax list)
	{
		return Update(list);
	}

	public CommaSeparatedIdentifierEqualsStringListPropertyValueSyntax AddListValues(params IdentifierEqualsStringSyntax[] items)
	{
		return Update(List.WithValues(List.Values.AddRange(items)));
	}
}
