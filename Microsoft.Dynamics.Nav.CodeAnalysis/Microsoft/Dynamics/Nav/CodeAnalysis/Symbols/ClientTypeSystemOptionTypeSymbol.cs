using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class ClientTypeSystemOptionTypeSymbol : SystemOptionTypeSymbol
{
	private ImmutableArray<Symbol> lazyMembers;

	internal static ClientTypeSystemOptionTypeSymbol Instance { get; } = new ClientTypeSystemOptionTypeSymbol();

	private ClientTypeSystemOptionTypeSymbol()
		: base(NavTypeKind.ClientType)
	{
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, ImmutableArray.Create(new Symbol[14]
		{
			SystemOptionSymbol.Create(this, "Background", 0),
			SystemOptionSymbol.Create(this, "Desktop", 1),
			SystemOptionSymbol.Create(this, "Management", 2),
			SystemOptionSymbol.Create(this, "NAS", 3),
			SystemOptionSymbol.Create(this, "OData", 4),
			SystemOptionSymbol.Create(this, "Phone", 5),
			SystemOptionSymbol.Create(this, "SOAP", 6),
			SystemOptionSymbol.Create(this, "Tablet", 7),
			SystemOptionSymbol.Create(this, "Web", 8),
			SystemOptionSymbol.Create(this, "Windows", 9),
			SystemOptionSymbol.Create(this, "Current", 10),
			SystemOptionSymbol.Create(this, "Default", 11),
			SystemOptionSymbol.Create(this, "ODataV4", 12),
			SystemOptionSymbol.Create(this, "Api", 13)
		}), default);
		return lazyMembers;
	}
}
