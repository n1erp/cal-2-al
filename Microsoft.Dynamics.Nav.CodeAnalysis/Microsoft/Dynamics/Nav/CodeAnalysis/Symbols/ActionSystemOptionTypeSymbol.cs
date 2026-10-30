using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class ActionSystemOptionTypeSymbol : SystemOptionTypeSymbol
{
	private ImmutableArray<Symbol> lazyMembers;

	internal static ActionSystemOptionTypeSymbol Instance { get; } = new ActionSystemOptionTypeSymbol();

	private ActionSystemOptionTypeSymbol()
		: base(NavTypeKind.Action)
	{
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, ImmutableArray.Create(new Symbol[9]
		{
			SystemOptionSymbol.Create(this, "None", 0),
			SystemOptionSymbol.Create(this, "OK", 1),
			SystemOptionSymbol.Create(this, "Cancel", 2),
			SystemOptionSymbol.Create(this, "LookupOK", 3),
			SystemOptionSymbol.Create(this, "LookupCancel", 4),
			SystemOptionSymbol.Create(this, "Yes", 5),
			SystemOptionSymbol.Create(this, "No", 6),
			SystemOptionSymbol.Create(this, "RunObject", 7),
			SystemOptionSymbol.Create(this, "RunSystem", 8)
		}), default);
		return lazyMembers;
	}
}
