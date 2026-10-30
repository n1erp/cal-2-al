using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class TextEncodingSystemOptionTypeSymbol : SystemOptionTypeSymbol
{
	private ImmutableArray<Symbol> lazyMembers;

	internal static TextEncodingSystemOptionTypeSymbol Instance { get; } = new TextEncodingSystemOptionTypeSymbol();

	private TextEncodingSystemOptionTypeSymbol()
		: base(NavTypeKind.TextEncoding)
	{
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, ImmutableArray.Create((Symbol)SystemOptionSymbol.Create(this, "MSDos", 0), (Symbol)SystemOptionSymbol.Create(this, "UTF8", 1), (Symbol)SystemOptionSymbol.Create(this, "UTF16", 2), (Symbol)SystemOptionSymbol.Create(this, "Windows", 3)), default);
		return lazyMembers;
	}
}
