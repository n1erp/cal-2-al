using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal abstract class SystemOptionTypeSymbol : NavTypeSymbol
{
	public override string Name { get; }

	public override TypeCategoryKind TypeCategory { get; } = TypeCategoryKind.Option;

	public override SymbolKind Kind { get; } = SymbolKind.OptionType;

	public override VersionCompatibility Compatibility { get; }

	internal SystemOptionTypeSymbol(NavTypeKind navTypeKind, VersionCompatibility compatibility = null)
		: base(navTypeKind)
	{
		Name = navTypeKind.ToString();
		Compatibility = compatibility;
	}
}
