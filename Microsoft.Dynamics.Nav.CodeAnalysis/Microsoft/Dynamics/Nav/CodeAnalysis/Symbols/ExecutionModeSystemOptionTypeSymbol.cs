using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class ExecutionModeSystemOptionTypeSymbol : SystemOptionTypeSymbol
{
	private ImmutableArray<Symbol> lazyMembers;

	internal static ExecutionModeSystemOptionTypeSymbol Instance { get; } = new ExecutionModeSystemOptionTypeSymbol();

	private ExecutionModeSystemOptionTypeSymbol()
		: base(NavTypeKind.ExecutionMode)
	{
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, ImmutableArray.Create((Symbol)SystemOptionSymbol.Create(this, "Standard", 0), (Symbol)SystemOptionSymbol.Create(this, "Debug", 1)), default);
		return lazyMembers;
	}
}
