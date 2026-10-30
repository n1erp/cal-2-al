using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedFieldSymbol : SynthesizedFieldSymbolBase
{
	private readonly TypeSymbol _type;

	internal override bool SuppressDynamicAttribute => true;

	public SynthesizedFieldSymbol(NamedTypeSymbol containingType, TypeSymbol type, string name, bool isPublic = false, bool isReadOnly = false, bool isStatic = false)
		: base(containingType, name, isPublic, isReadOnly, isStatic)
	{
		_type = type;
	}

	internal override TypeSymbol GetFieldType(ConsList<FieldSymbol> fieldsBeingBound)
	{
		return _type;
	}
}
