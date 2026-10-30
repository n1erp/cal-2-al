using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal abstract class LoopBinder : LocalScopeBinder
{
	private readonly GeneratedLabelSymbol breakLabel;

	internal override GeneratedLabelSymbol BreakLabel => breakLabel;

	protected LoopBinder(Binder enclosing)
		: base(enclosing)
	{
		breakLabel = new GeneratedLabelSymbol("break");
	}
}
