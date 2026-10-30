using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class ErrorTypeSystemOptionTypeSymbol : SystemOptionTypeSymbol
{
	private ImmutableArray<Symbol> lazyMembers;

	internal static ErrorTypeSystemOptionTypeSymbol Instance { get; } = new ErrorTypeSystemOptionTypeSymbol();

	private ErrorTypeSystemOptionTypeSymbol()
		: base(NavTypeKind.ErrorType, VersionCompatibility.Parse("3.0"))
	{
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, ImmutableArray.Create((Symbol)SystemOptionSymbol.Create(this, "Client", 0), (Symbol)SystemOptionSymbol.Create(this, "Internal", 1)), default);
		return lazyMembers;
	}
}
