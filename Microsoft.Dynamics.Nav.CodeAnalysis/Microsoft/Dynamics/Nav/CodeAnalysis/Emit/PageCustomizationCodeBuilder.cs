using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal sealed class PageCustomizationCodeBuilder : PageExtensionBaseCodeBuilder<PageCustomizationTypeSymbol>
{
	protected override ImmutableArray<ControlSymbol> FlattenedControls { get; }

	public PageCustomizationCodeBuilder(PageCustomizationTypeSymbol symbol, ModuleBuilder moduleBuilder)
		: base(symbol, moduleBuilder)
	{
		FlattenedControls = ImmutableArray<ControlSymbol>.Empty;
	}
}
