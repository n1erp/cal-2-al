using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class ExecutionContextSystemOptionTypeSymbol : SystemOptionTypeSymbol
{
	private ImmutableArray<Symbol> lazyMembers;

	internal static ExecutionContextSystemOptionTypeSymbol Instance { get; } = new ExecutionContextSystemOptionTypeSymbol();

	private ExecutionContextSystemOptionTypeSymbol()
		: base(NavTypeKind.ExecutionContext)
	{
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, ImmutableArray.Create((Symbol)SystemOptionSymbol.Create(this, "Normal", 0), (Symbol)SystemOptionSymbol.Create(this, "Install", 1), (Symbol)SystemOptionSymbol.Create(this, "Uninstall", 2), (Symbol)SystemOptionSymbol.Create(this, "Upgrade", 3)), default);
		return lazyMembers;
	}
}
