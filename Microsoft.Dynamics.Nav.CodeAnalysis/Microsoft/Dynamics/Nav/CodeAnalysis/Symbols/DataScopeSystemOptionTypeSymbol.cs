using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class DataScopeSystemOptionTypeSymbol : SystemOptionTypeSymbol
{
	private ImmutableArray<Symbol> lazyMembers;

	internal static DataScopeSystemOptionTypeSymbol Instance { get; } = new DataScopeSystemOptionTypeSymbol();

	private DataScopeSystemOptionTypeSymbol()
		: base(NavTypeKind.DataScope, VersionCompatibility.Parse("2.0"))
	{
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, ImmutableArray.Create((Symbol)SystemOptionSymbol.Create(this, "Module", 0), (Symbol)SystemOptionSymbol.Create(this, "Company", 1), (Symbol)SystemOptionSymbol.Create(this, "User", 2), (Symbol)SystemOptionSymbol.Create(this, "CompanyAndUser", 3)), default);
		return lazyMembers;
	}
}
