using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class TestPermissionsSystemOptionTypeSymbol : SystemOptionTypeSymbol
{
	private ImmutableArray<Symbol> lazyMembers;

	internal static TestPermissionsSystemOptionTypeSymbol Instance { get; } = new TestPermissionsSystemOptionTypeSymbol();

	private TestPermissionsSystemOptionTypeSymbol()
		: base(NavTypeKind.TestPermissions)
	{
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, ImmutableArray.Create((Symbol)SystemOptionSymbol.Create(this, "InheritFromTestCodeunit", 0), (Symbol)SystemOptionSymbol.Create(this, "Restrictive", 1), (Symbol)SystemOptionSymbol.Create(this, "NonRestrictive", 2), (Symbol)SystemOptionSymbol.Create(this, "Disabled", 3)), default);
		return lazyMembers;
	}
}
