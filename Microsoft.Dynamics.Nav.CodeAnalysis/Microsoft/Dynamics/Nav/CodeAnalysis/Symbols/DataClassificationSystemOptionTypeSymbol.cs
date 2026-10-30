using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class DataClassificationSystemOptionTypeSymbol : SystemOptionTypeSymbol
{
	private ImmutableArray<Symbol> lazyMembers;

	internal static DataClassificationSystemOptionTypeSymbol Instance { get; } = new DataClassificationSystemOptionTypeSymbol();

	private DataClassificationSystemOptionTypeSymbol()
		: base(NavTypeKind.DataClassification)
	{
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, ImmutableArray.Create(new Symbol[7]
		{
			SystemOptionSymbol.Create(this, "CustomerContent", 0),
			SystemOptionSymbol.Create(this, "ToBeClassified", 1),
			SystemOptionSymbol.Create(this, "EndUserIdentifiableInformation", 2),
			SystemOptionSymbol.Create(this, "AccountData", 3),
			SystemOptionSymbol.Create(this, "EndUserPseudonymousIdentifiers", 4),
			SystemOptionSymbol.Create(this, "OrganizationIdentifiableInformation", 5),
			SystemOptionSymbol.Create(this, "SystemMetadata", 6)
		}), default);
		return lazyMembers;
	}
}
