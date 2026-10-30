using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class TableConnectionTypeSystemOptionTypeSymbol : SystemOptionTypeSymbol
{
	private ImmutableArray<Symbol> lazyMembers;

	internal static TableConnectionTypeSystemOptionTypeSymbol Instance { get; } = new TableConnectionTypeSystemOptionTypeSymbol();

	private TableConnectionTypeSystemOptionTypeSymbol()
		: base(NavTypeKind.TableConnectionType)
	{
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, ImmutableArray.Create((Symbol)SystemOptionSymbol.Create(this, "CRM", 0), (Symbol)SystemOptionSymbol.Create(this, "ExternalSQL", 1), (Symbol)SystemOptionSymbol.Create(this, "Exchange", 2), (Symbol)SystemOptionSymbol.Create(this, "MicrosoftGraph", 3)), default);
		return lazyMembers;
	}
}
