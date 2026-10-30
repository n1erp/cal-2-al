using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class AttributeArgumentSymbol : Symbol, IAttributeArgumentSymbol, ISymbol, IEquatable<ISymbol>
{
	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public override SymbolKind Kind => SymbolKind.AttributeArgument;

	public abstract TypeSymbol Type { get; }

	public abstract string ValueText { get; }

	public bool? ValueAsBoolean
	{
		get
		{
			if (!bool.TryParse(ValueText, out var result))
			{
				return null;
			}
			return result;
		}
	}

	public int? ValueAsInteger
	{
		get
		{
			if (!int.TryParse(ValueText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
			{
				return null;
			}
			return result;
		}
	}

	ITypeSymbol IAttributeArgumentSymbol.Type => Type;

	string IAttributeArgumentSymbol.ValueText => ValueText;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitAttributeArgument(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitAttributeArgument(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitAttributeArgument(this, a);
	}
}
