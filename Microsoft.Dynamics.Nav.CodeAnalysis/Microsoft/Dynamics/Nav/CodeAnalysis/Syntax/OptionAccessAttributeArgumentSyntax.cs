using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class OptionAccessAttributeArgumentSyntax : AttributeArgumentSyntax
{
	private OptionAccessExpressionSyntax _optionAccess;

	public OptionAccessExpressionSyntax OptionAccess => GetRedAtZero(ref _optionAccess);

	internal OptionAccessAttributeArgumentSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _optionAccess);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _optionAccess;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitOptionAccessAttributeArgument(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitOptionAccessAttributeArgument(this);
	}

	public OptionAccessAttributeArgumentSyntax Update(OptionAccessExpressionSyntax optionAccess)
	{
		if (optionAccess != OptionAccess)
		{
			OptionAccessAttributeArgumentSyntax optionAccessAttributeArgumentSyntax = SyntaxFactory.OptionAccessAttributeArgument(optionAccess);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return optionAccessAttributeArgumentSyntax.WithAnnotations(annotations);
			}
			return optionAccessAttributeArgumentSyntax;
		}
		return this;
	}

	public OptionAccessAttributeArgumentSyntax WithOptionAccess(OptionAccessExpressionSyntax optionAccess)
	{
		return Update(optionAccess);
	}
}
