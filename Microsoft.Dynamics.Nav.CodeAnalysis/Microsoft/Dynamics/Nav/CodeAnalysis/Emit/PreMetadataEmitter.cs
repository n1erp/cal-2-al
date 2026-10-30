using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal abstract class PreMetadataEmitter<T> : PreMetadataEmitter where T : ObjectTypeSymbol
{
	protected T Symbol { get; }

	protected PreMetadataEmitter(T symbol, ModuleBuilder moduleBuilder)
		: base(moduleBuilder)
	{
		Symbol = symbol;
	}
}
internal abstract class PreMetadataEmitter
{
	protected ModuleBuilder ModuleBuilder { get; }

	public PreMetadataEmitter(ModuleBuilder moduleBuilder)
	{
		ModuleBuilder = moduleBuilder;
	}

	internal abstract void Emit(DiagnosticBag diagnostics);
}
