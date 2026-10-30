using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class ThisParameterSymbol : ParameterSymbol
{
	internal const string SymbolName = "this";

	private readonly MethodSymbol _containingMethod;

	private readonly TypeSymbol _containingType;

	public override string Name => "this";

	public override TypeSymbol Type => _containingType;

	public override RefKind RefKind
	{
		get
		{
			NamedTypeSymbol containingType = ContainingType;
			if ((object)containingType == null || containingType.TypeKind != TypeKind.Struct)
			{
				return RefKind.None;
			}
			MethodSymbol containingMethod = _containingMethod;
			if ((object)containingMethod != null && containingMethod.MethodKind == MethodKind.Constructor)
			{
				return RefKind.Out;
			}
			if (ContainingType.IsReadOnly)
			{
				return RefKind.In;
			}
			return RefKind.Ref;
		}
	}

	public override ImmutableArray<Location> Locations
	{
		get
		{
			if ((object)_containingMethod == null)
			{
				return ImmutableArray<Location>.Empty;
			}
			return _containingMethod.Locations;
		}
	}

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override Symbol ContainingSymbol => (Symbol)(((object)_containingMethod) ?? ((object)_containingType));

	internal override ConstantValue ExplicitDefaultConstantValue => null;

	internal override bool IsMetadataOptional => false;

	public override bool IsParams => false;

	internal override bool IsIDispatchConstant => false;

	internal override bool IsIUnknownConstant => false;

	internal override bool IsCallerFilePath => false;

	internal override bool IsCallerLineNumber => false;

	internal override bool IsCallerMemberName => false;

	public override int Ordinal => -1;

	public override ImmutableArray<CustomModifier> CustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override bool IsThis => true;

	public override bool IsImplicitlyDeclared => true;

	internal override bool IsMetadataIn => false;

	internal override bool IsMetadataOut => false;

	internal override MarshalPseudoCustomAttributeData MarshallingInformation => null;

	internal ThisParameterSymbol(MethodSymbol forMethod)
		: this(forMethod, forMethod.ContainingType)
	{
	}

	internal ThisParameterSymbol(MethodSymbol forMethod, TypeSymbol containingType)
	{
		_containingMethod = forMethod;
		_containingType = containingType;
	}
}
