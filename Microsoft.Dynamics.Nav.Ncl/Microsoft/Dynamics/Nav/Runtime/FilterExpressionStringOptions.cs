using System;

namespace Microsoft.Dynamics.Nav.Runtime;

[Flags]
public enum FilterExpressionStringOptions
{
	None = 0,
	OptionsAsIntegers = 1,
	OptionsNonLocalized = 2,
	AlwaysParenthesize = 4,
	ConstExpressionImplied = 8,
	TwoQuotesAsEmpty = 0x10,
	DoNotQuoteWildcards = 0x20,
	BooleanConstantTrueAsEmpty = 0x40,
	FormatCultureInvariant = 0x80
}
