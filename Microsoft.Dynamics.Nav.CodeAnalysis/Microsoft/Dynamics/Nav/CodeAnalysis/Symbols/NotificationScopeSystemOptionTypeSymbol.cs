using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class NotificationScopeSystemOptionTypeSymbol : SystemOptionTypeSymbol
{
	private ImmutableArray<Symbol> lazyMembers;

	internal static NotificationScopeSystemOptionTypeSymbol Instance { get; } = new NotificationScopeSystemOptionTypeSymbol();

	private NotificationScopeSystemOptionTypeSymbol()
		: base(NavTypeKind.NotificationScope)
	{
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, ImmutableArray.Create((Symbol)SystemOptionSymbol.Create(this, "GlobalScope", 0), (Symbol)SystemOptionSymbol.Create(this, "LocalScope", 1)), default);
		return lazyMembers;
	}
}
