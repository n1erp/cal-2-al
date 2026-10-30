using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class TransactionModelSystemOptionTypeSymbol : SystemOptionTypeSymbol
{
	private ImmutableArray<Symbol> lazyMembers;

	internal static TransactionModelSystemOptionTypeSymbol Instance { get; } = new TransactionModelSystemOptionTypeSymbol();

	private TransactionModelSystemOptionTypeSymbol()
		: base(NavTypeKind.TransactionModel)
	{
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, ImmutableArray.Create((Symbol)SystemOptionSymbol.Create(this, "AutoCommit", 0), (Symbol)SystemOptionSymbol.Create(this, "AutoRollback", 1), (Symbol)SystemOptionSymbol.Create(this, "None", 2)), default);
		return lazyMembers;
	}
}
