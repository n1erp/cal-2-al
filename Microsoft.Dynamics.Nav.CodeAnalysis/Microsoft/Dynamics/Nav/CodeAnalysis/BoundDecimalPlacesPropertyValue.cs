using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundDecimalPlacesPropertyValue : BoundPropertyValue
{
	public int Minimum { get; private set; }

	public int? Maximum { get; private set; }

	public BoundDecimalPlacesPropertyValue(SyntaxNode syntax, int minimum, int? maximum, TypeSymbol type, bool hasErrors)
		: base(BoundKind.DecimalPlacesPropertyValue, syntax, type, hasErrors)
	{
		Minimum = minimum;
		Maximum = maximum;
	}

	public BoundDecimalPlacesPropertyValue(SyntaxNode syntax, int minimum, int? maximum, TypeSymbol type)
		: base(BoundKind.DecimalPlacesPropertyValue, syntax, type)
	{
		Minimum = minimum;
		Maximum = maximum;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDecimalPlacesPropertyValue(this);
	}

	public BoundDecimalPlacesPropertyValue Update(int minimum, int? maximum, TypeSymbol type)
	{
		if (minimum == Minimum)
		{
			int? num = maximum;
			int? maximum2 = Maximum;
			if (num.GetValueOrDefault() == maximum2.GetValueOrDefault() && num.HasValue == maximum2.HasValue && !(type != Type))
			{
				return this;
			}
		}
		return new BoundDecimalPlacesPropertyValue(Syntax, minimum, maximum, type, HasErrors)
		{
			WasCompilerGenerated = WasCompilerGenerated
		};
	}
}
