using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class DefaultLayoutSystemOptionTypeSymbol : SystemOptionTypeSymbol
{
	private ImmutableArray<Symbol> lazyMembers;

	internal static DefaultLayoutSystemOptionTypeSymbol Instance { get; } = new DefaultLayoutSystemOptionTypeSymbol();

	private DefaultLayoutSystemOptionTypeSymbol()
		: base(NavTypeKind.DefaultLayout)
	{
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, ImmutableArray.Create((Symbol)SystemOptionSymbol.Create(this, "None", 0), (Symbol)SystemOptionSymbol.Create(this, "RDLC", 1), (Symbol)SystemOptionSymbol.Create(this, "Word", 2)), default);
		return lazyMembers;
	}
}
