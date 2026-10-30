using System.Collections.Immutable;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class ErrorPropertySymbol : PropertySymbol
{
	private readonly Symbol _containingSymbol;

	private readonly TypeSymbol _type;

	private readonly string _name;

	private readonly bool _isIndexer;

	private readonly bool _isIndexedProperty;

	public override Symbol ContainingSymbol => _containingSymbol;

	public override RefKind RefKind => RefKind.None;

	public override TypeSymbol Type => _type;

	public override string Name => _name;

	internal override bool HasSpecialName => false;

	public override bool IsIndexer => _isIndexer;

	public override bool IsIndexedProperty => _isIndexedProperty;

	public override MethodSymbol GetMethod => null;

	public override MethodSymbol SetMethod => null;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public override bool IsStatic => false;

	public override bool IsVirtual => false;

	public override bool IsOverride => false;

	public override bool IsAbstract => false;

	public override bool IsSealed => false;

	public override bool IsExtern => false;

	internal sealed override ObsoleteAttributeData ObsoleteAttributeData => null;

	public override ImmutableArray<ParameterSymbol> Parameters => ImmutableArray<ParameterSymbol>.Empty;

	internal override CallingConvention CallingConvention => CallingConvention.Default;

	internal override bool MustCallMethodsDirectly => false;

	public override ImmutableArray<PropertySymbol> ExplicitInterfaceImplementations => ImmutableArray<PropertySymbol>.Empty;

	public override ImmutableArray<CustomModifier> TypeCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public ErrorPropertySymbol(Symbol containingSymbol, TypeSymbol type, string name, bool isIndexer, bool isIndexedProperty)
	{
		_containingSymbol = containingSymbol;
		_type = type;
		_name = name;
		_isIndexer = isIndexer;
		_isIndexedProperty = isIndexedProperty;
	}
}
