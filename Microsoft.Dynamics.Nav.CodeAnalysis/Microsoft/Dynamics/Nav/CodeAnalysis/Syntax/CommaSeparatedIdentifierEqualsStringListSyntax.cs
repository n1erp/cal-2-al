using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class CommaSeparatedIdentifierEqualsStringListSyntax : SyntaxNode
{
	private SyntaxNode _values;

	public SeparatedSyntaxList<IdentifierEqualsStringSyntax> Values
	{
		get
		{
			SyntaxNode red = GetRed(ref _values, 0);
			if (red != null)
			{
				return new SeparatedSyntaxList<IdentifierEqualsStringSyntax>(red, 0);
			}
			return default;
		}
	}

	internal CommaSeparatedIdentifierEqualsStringListSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _values);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _values;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCommaSeparatedIdentifierEqualsStringList(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitCommaSeparatedIdentifierEqualsStringList(this);
	}

	public CommaSeparatedIdentifierEqualsStringListSyntax Update(SeparatedSyntaxList<IdentifierEqualsStringSyntax> values)
	{
		if (values != Values)
		{
			CommaSeparatedIdentifierEqualsStringListSyntax commaSeparatedIdentifierEqualsStringListSyntax = SyntaxFactory.CommaSeparatedIdentifierEqualsStringList(values);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return commaSeparatedIdentifierEqualsStringListSyntax.WithAnnotations(annotations);
			}
			return commaSeparatedIdentifierEqualsStringListSyntax;
		}
		return this;
	}

	public CommaSeparatedIdentifierEqualsStringListSyntax WithValues(SeparatedSyntaxList<IdentifierEqualsStringSyntax> values)
	{
		return Update(values);
	}

	public CommaSeparatedIdentifierEqualsStringListSyntax AddValues(params IdentifierEqualsStringSyntax[] items)
	{
		return Update(Values.AddRange(items));
	}
}
