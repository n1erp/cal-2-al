using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class FieldTypeSystemOptionTypeSymbol : SystemOptionTypeSymbol
{
	private ImmutableArray<Symbol> lazyMembers;

	internal static FieldTypeSystemOptionTypeSymbol Instance { get; } = new FieldTypeSystemOptionTypeSymbol();

	private FieldTypeSystemOptionTypeSymbol()
		: base(NavTypeKind.FieldType)
	{
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, ImmutableArray.Create(new Symbol[18]
		{
			SystemOptionSymbol.Create(this, "Boolean", 0),
			SystemOptionSymbol.Create(this, "Integer", 1),
			SystemOptionSymbol.Create(this, "BigInteger", 2),
			SystemOptionSymbol.Create(this, "Decimal", 3),
			SystemOptionSymbol.Create(this, "Option", 4),
			SystemOptionSymbol.Create(this, "Text", 5),
			SystemOptionSymbol.Create(this, "Code", 6),
			SystemOptionSymbol.Create(this, "DateTime", 7),
			SystemOptionSymbol.Create(this, "Time", 8),
			SystemOptionSymbol.Create(this, "Date", 9),
			SystemOptionSymbol.Create(this, "DateFormula", 10),
			SystemOptionSymbol.Create(this, "Duration", 11),
			SystemOptionSymbol.Create(this, "Guid", 12, "GUID"),
			SystemOptionSymbol.Create(this, "RecordId", 13, "RecordID"),
			SystemOptionSymbol.Create(this, "TableFilter", 14),
			SystemOptionSymbol.Create(this, "Blob", 15, "BLOB"),
			SystemOptionSymbol.Create(this, "Media", 16),
			SystemOptionSymbol.Create(this, "MediaSet", 17)
		}), default);
		return lazyMembers;
	}
}
