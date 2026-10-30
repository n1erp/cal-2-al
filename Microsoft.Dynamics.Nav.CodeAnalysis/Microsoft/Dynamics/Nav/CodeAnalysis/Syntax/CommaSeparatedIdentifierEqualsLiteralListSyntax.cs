using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class CommaSeparatedIdentifierEqualsLiteralListSyntax : SyntaxNode
{
	private SyntaxNode _values;

	public SeparatedSyntaxList<IdentifierEqualsLiteralSyntax> Values
	{
		get
		{
			SyntaxNode red = GetRed(ref _values, 0);
			if (red != null)
			{
				return new SeparatedSyntaxList<IdentifierEqualsLiteralSyntax>(red, 0);
			}
			return default;
		}
	}

	internal CommaSeparatedIdentifierEqualsLiteralListSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitCommaSeparatedIdentifierEqualsLiteralList(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitCommaSeparatedIdentifierEqualsLiteralList(this);
	}

	public CommaSeparatedIdentifierEqualsLiteralListSyntax Update(SeparatedSyntaxList<IdentifierEqualsLiteralSyntax> values)
	{
		if (values != Values)
		{
			CommaSeparatedIdentifierEqualsLiteralListSyntax commaSeparatedIdentifierEqualsLiteralListSyntax = SyntaxFactory.CommaSeparatedIdentifierEqualsLiteralList(values);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return commaSeparatedIdentifierEqualsLiteralListSyntax.WithAnnotations(annotations);
			}
			return commaSeparatedIdentifierEqualsLiteralListSyntax;
		}
		return this;
	}

	public CommaSeparatedIdentifierEqualsLiteralListSyntax WithValues(SeparatedSyntaxList<IdentifierEqualsLiteralSyntax> values)
	{
		return Update(values);
	}

	public CommaSeparatedIdentifierEqualsLiteralListSyntax AddValues(params IdentifierEqualsLiteralSyntax[] items)
	{
		return Update(Values.AddRange(items));
	}
}
