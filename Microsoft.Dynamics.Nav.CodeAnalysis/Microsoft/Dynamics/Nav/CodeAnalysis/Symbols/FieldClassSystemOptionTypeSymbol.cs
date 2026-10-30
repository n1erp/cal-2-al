using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class FieldClassSystemOptionTypeSymbol : SystemOptionTypeSymbol
{
	private ImmutableArray<Symbol> lazyMembers;

	internal static FieldClassSystemOptionTypeSymbol Instance { get; } = new FieldClassSystemOptionTypeSymbol();

	private FieldClassSystemOptionTypeSymbol()
		: base(NavTypeKind.FieldClass)
	{
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, ImmutableArray.Create((Symbol)SystemOptionSymbol.Create(this, "Normal", 0), (Symbol)SystemOptionSymbol.Create(this, "FlowField", 1), (Symbol)SystemOptionSymbol.Create(this, "FlowFilter", 2)), default);
		return lazyMembers;
	}
}
