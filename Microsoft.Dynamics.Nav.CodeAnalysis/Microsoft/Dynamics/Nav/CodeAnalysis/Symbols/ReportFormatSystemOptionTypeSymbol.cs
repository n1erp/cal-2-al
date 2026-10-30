using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class ReportFormatSystemOptionTypeSymbol : SystemOptionTypeSymbol
{
	private ImmutableArray<Symbol> lazyMembers;

	internal static ReportFormatSystemOptionTypeSymbol Instance { get; } = new ReportFormatSystemOptionTypeSymbol();

	private ReportFormatSystemOptionTypeSymbol()
		: base(NavTypeKind.ReportFormat)
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
			SystemOptionSymbol.Create(this, "Excel", 0),
			SystemOptionSymbol.Create(this, "Html", 1),
			SystemOptionSymbol.Create(this, "Pdf", 2),
			SystemOptionSymbol.Create(this, "Word", 3),
			SystemOptionSymbol.Create(this, "Xml", 4)
		}), default);
		return lazyMembers;
	}
}
