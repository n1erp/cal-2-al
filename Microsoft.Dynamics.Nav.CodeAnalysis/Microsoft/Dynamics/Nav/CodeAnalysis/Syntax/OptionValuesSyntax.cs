using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class OptionValuesSyntax : LiteralValueSyntax
{
	private SyntaxNode _options;

	public SeparatedSyntaxList<IdentifierNameOrEmptySyntax> Options
	{
		get
		{
			SyntaxNode red = GetRed(ref _options, 0);
			if (red != null)
			{
				return new SeparatedSyntaxList<IdentifierNameOrEmptySyntax>(red, 0);
			}
			return default;
		}
	}

	internal OptionValuesSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _options);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _options;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitOptionValues(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitOptionValues(this);
	}

	public OptionValuesSyntax Update(SeparatedSyntaxList<IdentifierNameOrEmptySyntax> options)
	{
		if (options != Options)
		{
			OptionValuesSyntax optionValuesSyntax = SyntaxFactory.OptionValues(options);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return optionValuesSyntax.WithAnnotations(annotations);
			}
			return optionValuesSyntax;
		}
		return this;
	}

	public OptionValuesSyntax WithOptions(SeparatedSyntaxList<IdentifierNameOrEmptySyntax> options)
	{
		return Update(options);
	}

	public OptionValuesSyntax AddOptions(params IdentifierNameOrEmptySyntax[] items)
	{
		return Update(Options.AddRange(items));
	}
}
