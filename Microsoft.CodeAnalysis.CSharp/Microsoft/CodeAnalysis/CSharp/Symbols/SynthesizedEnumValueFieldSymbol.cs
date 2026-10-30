using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedEnumValueFieldSymbol : SynthesizedFieldSymbolBase
{
	internal override bool SuppressDynamicAttribute => true;

	public SynthesizedEnumValueFieldSymbol(SourceNamedTypeSymbol containingEnum)
		: base(containingEnum, "value__", isPublic: true, isReadOnly: false, isStatic: false)
	{
	}

	internal override TypeSymbol GetFieldType(ConsList<FieldSymbol> fieldsBeingBound)
	{
		return ((SourceNamedTypeSymbol)ContainingType).EnumUnderlyingType;
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<SynthesizedAttributeData> attributes)
	{
	}
}
