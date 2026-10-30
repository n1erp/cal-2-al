using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class VerbositySystemOptionTypeSymbol : SystemOptionTypeSymbol
{
	private ImmutableArray<Symbol> lazyMembers;

	internal static VerbositySystemOptionTypeSymbol Instance { get; } = new VerbositySystemOptionTypeSymbol();

	private VerbositySystemOptionTypeSymbol()
		: base(NavTypeKind.Verbosity)
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
			SystemOptionSymbol.Create(this, "Critical", 0),
			SystemOptionSymbol.Create(this, "Error", 1),
			SystemOptionSymbol.Create(this, "Warning", 2),
			SystemOptionSymbol.Create(this, "Normal", 3),
			SystemOptionSymbol.Create(this, "Verbose", 4)
		}), default);
		return lazyMembers;
	}
}
