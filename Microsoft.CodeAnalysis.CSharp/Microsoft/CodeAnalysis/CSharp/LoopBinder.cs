using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class LoopBinder : LocalScopeBinder
{
	private readonly GeneratedLabelSymbol _breakLabel;

	private readonly GeneratedLabelSymbol _continueLabel;

	internal override GeneratedLabelSymbol BreakLabel => _breakLabel;

	internal override GeneratedLabelSymbol ContinueLabel => _continueLabel;

	protected LoopBinder(Binder enclosing)
		: base(enclosing)
	{
		_breakLabel = new GeneratedLabelSymbol("break");
		_continueLabel = new GeneratedLabelSymbol("continue");
	}
}
