using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedLambdaCacheFieldSymbol : SynthesizedFieldSymbolBase, ISynthesizedMethodBodyImplementationSymbol
{
	private readonly TypeSymbol _type;

	private readonly MethodSymbol _topLevelMethod;

	internal override bool SuppressDynamicAttribute => true;

	IMethodSymbol ISynthesizedMethodBodyImplementationSymbol.Method => _topLevelMethod;

	bool ISynthesizedMethodBodyImplementationSymbol.HasMethodBodyDependency => false;

	public SynthesizedLambdaCacheFieldSymbol(NamedTypeSymbol containingType, TypeSymbol type, string name, MethodSymbol topLevelMethod, bool isReadOnly, bool isStatic)
		: base(containingType, name, isPublic: true, isReadOnly, isStatic)
	{
		_type = type;
		_topLevelMethod = topLevelMethod;
	}

	internal override TypeSymbol GetFieldType(ConsList<FieldSymbol> fieldsBeingBound)
	{
		return _type;
	}
}
