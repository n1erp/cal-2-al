using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class TransactionTypeSystemOptionTypeSymbol : SystemOptionTypeSymbol
{
	private ImmutableArray<Symbol> lazyMembers;

	internal static TransactionTypeSystemOptionTypeSymbol Instance { get; } = new TransactionTypeSystemOptionTypeSymbol();

	private TransactionTypeSystemOptionTypeSymbol()
		: base(NavTypeKind.TransactionType)
	{
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, ImmutableArray.Create(new Symbol[5]
		{
			SystemOptionSymbol.Create(this, "UpdateNoLocks", 0),
			SystemOptionSymbol.Create(this, "Update", 1),
			SystemOptionSymbol.Create(this, "Snapshot", 2),
			SystemOptionSymbol.Create(this, "Browse", 3),
			SystemOptionSymbol.Create(this, "Report", 4)
		}), default);
		return lazyMembers;
	}
}
