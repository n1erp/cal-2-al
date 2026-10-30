using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class TableFilterPropertyValueSyntax : PropertyValueSyntax
{
	private SyntaxNode _conditions;

	public SeparatedSyntaxList<PropertyExpressionSyntax> Conditions
	{
		get
		{
			SyntaxNode red = GetRed(ref _conditions, 0);
			if (red != null)
			{
				return new SeparatedSyntaxList<PropertyExpressionSyntax>(red, 0);
			}
			return default;
		}
	}

	internal TableFilterPropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _conditions);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _conditions;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTableFilterPropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitTableFilterPropertyValue(this);
	}

	public TableFilterPropertyValueSyntax Update(SeparatedSyntaxList<PropertyExpressionSyntax> conditions)
	{
		if (conditions != Conditions)
		{
			TableFilterPropertyValueSyntax tableFilterPropertyValueSyntax = SyntaxFactory.TableFilterPropertyValue(conditions);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return tableFilterPropertyValueSyntax.WithAnnotations(annotations);
			}
			return tableFilterPropertyValueSyntax;
		}
		return this;
	}

	public TableFilterPropertyValueSyntax WithConditions(SeparatedSyntaxList<PropertyExpressionSyntax> conditions)
	{
		return Update(conditions);
	}

	public TableFilterPropertyValueSyntax AddConditions(params PropertyExpressionSyntax[] items)
	{
		return Update(Conditions.AddRange(items));
	}
}
