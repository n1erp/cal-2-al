using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class WebServiceActionResultCodeSystemOptionTypeSymbol : SystemOptionTypeSymbol
{
	private ImmutableArray<Symbol> lazyMembers;

	internal static WebServiceActionResultCodeSystemOptionTypeSymbol Instance { get; } = new WebServiceActionResultCodeSystemOptionTypeSymbol();

	private WebServiceActionResultCodeSystemOptionTypeSymbol()
		: base(NavTypeKind.WebServiceActionResultCode, VersionCompatibility.Parse("2.0"))
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
			SystemOptionSymbol.Create(this, "None", 0),
			SystemOptionSymbol.Create(this, "Get", 1),
			SystemOptionSymbol.Create(this, "Created", 2),
			SystemOptionSymbol.Create(this, "Updated", 3),
			SystemOptionSymbol.Create(this, "Deleted", 4)
		}), default);
		return lazyMembers;
	}
}
