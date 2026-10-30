using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class SecurityFilterSystemOptionTypeSymbol : SystemOptionTypeSymbol
{
	private ImmutableArray<Symbol> lazyMembers;

	internal static SecurityFilterSystemOptionTypeSymbol Instance { get; } = new SecurityFilterSystemOptionTypeSymbol();

	private SecurityFilterSystemOptionTypeSymbol()
		: base(NavTypeKind.SecurityFilter)
	{
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, ImmutableArray.Create((Symbol)SystemOptionSymbol.Create(this, "Validated", 0), (Symbol)SystemOptionSymbol.Create(this, "Filtered", 1), (Symbol)SystemOptionSymbol.Create(this, "Ignored", 2), (Symbol)SystemOptionSymbol.Create(this, "Disallowed", 3)), default);
		return lazyMembers;
	}
}
