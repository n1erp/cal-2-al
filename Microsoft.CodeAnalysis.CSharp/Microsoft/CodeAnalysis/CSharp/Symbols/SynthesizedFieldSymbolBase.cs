using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SynthesizedFieldSymbolBase : FieldSymbol
{
	private readonly NamedTypeSymbol _containingType;

	private readonly string _name;

	private readonly DeclarationModifiers _modifiers;

	internal abstract bool SuppressDynamicAttribute { get; }

	public override string Name => _name;

	public override ImmutableArray<CustomModifier> CustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override Symbol AssociatedSymbol => null;

	public override bool IsReadOnly => (_modifiers & DeclarationModifiers.ReadOnly) != 0;

	public override bool IsVolatile => false;

	public override bool IsConst => false;

	internal override bool IsNotSerialized => false;

	internal override MarshalPseudoCustomAttributeData MarshallingInformation => null;

	internal override int? TypeLayoutOffset => null;

	internal sealed override ObsoleteAttributeData ObsoleteAttributeData => null;

	public override Symbol ContainingSymbol => _containingType;

	public override NamedTypeSymbol ContainingType => _containingType;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override Accessibility DeclaredAccessibility => ModifierUtils.EffectiveAccessibility(_modifiers);

	public override bool IsStatic => (_modifiers & DeclarationModifiers.Static) != 0;

	internal override bool HasSpecialName => HasRuntimeSpecialName;

	internal override bool HasRuntimeSpecialName => Name == "value__";

	public override bool IsImplicitlyDeclared => true;

	public SynthesizedFieldSymbolBase(NamedTypeSymbol containingType, string name, bool isPublic, bool isReadOnly, bool isStatic)
	{
		_containingType = containingType;
		_name = name;
		_modifiers = (DeclarationModifiers)((isPublic ? 16 : 256) | (isReadOnly ? 1024 : 0) | (isStatic ? 4 : 0));
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<SynthesizedAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		if (!_containingType.IsImplicitlyDeclared)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_CompilerGeneratedAttribute__ctor));
		}
		if (!SuppressDynamicAttribute && Type.ContainsDynamic() && declaringCompilation.HasDynamicEmitAttributes() && declaringCompilation.CanEmitBoolean())
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeDynamicAttribute(Type, CustomModifiers.Length));
		}
		if (Type.ContainsTupleNames() && declaringCompilation.HasTupleNamesAttributes && declaringCompilation.CanEmitSpecialType(SpecialType.System_String))
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeTupleNamesAttribute(Type));
		}
	}

	internal abstract override TypeSymbol GetFieldType(ConsList<FieldSymbol> fieldsBeingBound);

	internal override ConstantValue GetConstantValue(ConstantFieldsInProgress inProgress, bool earlyDecodingWellKnownAttributes)
	{
		return null;
	}
}
