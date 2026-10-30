using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class FixedFieldImplementationType : SynthesizedContainer
{
	internal const string FixedElementFieldName = "FixedElementField";

	private readonly SourceMemberFieldSymbol _field;

	private readonly MethodSymbol _constructor;

	private readonly FieldSymbol _internalField;

	public override Symbol ContainingSymbol => _field.ContainingType;

	public override TypeKind TypeKind => TypeKind.Struct;

	internal override MethodSymbol Constructor => _constructor;

	internal override TypeLayout Layout
	{
		get
		{
			int fixedSize = _field.FixedSize;
			int num = ((PointerTypeSymbol)_field.Type).PointedAtType.FixedBufferElementSizeInBytes();
			int size = fixedSize * num;
			return new TypeLayout(LayoutKind.Sequential, size, 0);
		}
	}

	internal override CharSet MarshallingCharSet => _field.ContainingType.MarshallingCharSet;

	internal override FieldSymbol FixedElementField => _internalField;

	public override IEnumerable<string> MemberNames => SpecializedCollections.SingletonEnumerable("FixedElementField");

	public override Accessibility DeclaredAccessibility => Accessibility.Public;

	internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics => ContainingAssembly.GetSpecialType(SpecialType.System_ValueType);

	public FixedFieldImplementationType(SourceMemberFieldSymbol field)
		: base(GeneratedNames.MakeFixedFieldImplementationName(field.Name), ImmutableArray<TypeParameterSymbol>.Empty, TypeMap.Empty)
	{
		_field = field;
		_constructor = new SynthesizedInstanceConstructor(this);
		_internalField = new SynthesizedFieldSymbol(this, ((PointerTypeSymbol)field.Type).PointedAtType, "FixedElementField", isPublic: true);
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<SynthesizedAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		CSharpCompilation declaringCompilation = ContainingSymbol.DeclaringCompilation;
		Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_UnsafeValueTypeAttribute__ctor));
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return ImmutableArray.Create<Symbol>(_constructor, _internalField);
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		if (!(name == _constructor.Name))
		{
			if (!(name == "FixedElementField"))
			{
				return ImmutableArray<Symbol>.Empty;
			}
			return ImmutableArray.Create((Symbol)_internalField);
		}
		return ImmutableArray.Create((Symbol)_constructor);
	}
}
