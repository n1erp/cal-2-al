using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class ObjectTypeSystemOptionTypeSymbol : SystemOptionTypeSymbol
{
	private ImmutableArray<Symbol> lazyMembers;

	internal static ObjectTypeSystemOptionTypeSymbol Instance { get; } = new ObjectTypeSystemOptionTypeSymbol();

	private ObjectTypeSystemOptionTypeSymbol()
		: base(NavTypeKind.ObjectType)
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
			SystemOptionSymbol.Create(this, "Codeunit", 0),
			SystemOptionSymbol.Create(this, "MenuSuite", 1),
			SystemOptionSymbol.Create(this, "Page", 2),
			SystemOptionSymbol.Create(this, "Query", 3),
			SystemOptionSymbol.Create(this, "Report", 4),
			SystemOptionSymbol.Create(this, "Table", 5),
			SystemOptionSymbol.Create(this, "XmlPort", 6)
		}), default);
		return lazyMembers;
	}
}
