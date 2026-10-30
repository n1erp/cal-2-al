using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SynthesizedContainer : NamedTypeSymbol
{
	private readonly ImmutableArray<TypeParameterSymbol> _typeParameters;

	private readonly ImmutableArray<TypeParameterSymbol> _constructedFromTypeParameters;

	internal TypeMap TypeMap { get; }

	internal virtual MethodSymbol Constructor => null;

	internal sealed override bool IsInterface => TypeKind == TypeKind.Interface;

	internal ImmutableArray<TypeParameterSymbol> ConstructedFromTypeParameters => _constructedFromTypeParameters;

	public sealed override ImmutableArray<TypeParameterSymbol> TypeParameters => _typeParameters;

	public sealed override string Name { get; }

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override IEnumerable<string> MemberNames => SpecializedCollections.EmptyEnumerable<string>();

	public override NamedTypeSymbol ConstructedFrom => this;

	public override bool IsSealed => true;

	public override bool IsAbstract
	{
		get
		{
			if ((object)Constructor == null)
			{
				return TypeKind != TypeKind.Struct;
			}
			return false;
		}
	}

	internal override ImmutableArray<TypeSymbol> TypeArgumentsNoUseSiteDiagnostics => StaticCast<TypeSymbol>.From(TypeParameters);

	internal override bool HasTypeArgumentsCustomModifiers => false;

	internal override bool HasCodeAnalysisEmbeddedAttribute => false;

	public override Accessibility DeclaredAccessibility => Accessibility.Private;

	public override bool IsStatic => false;

	internal sealed override bool IsByRefLikeType => false;

	internal sealed override bool IsReadOnly => false;

	internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics => ContainingAssembly.GetSpecialType((TypeKind != TypeKind.Struct) ? SpecialType.System_Object : SpecialType.System_ValueType);

	public override bool MightContainExtensionMethods => false;

	public override int Arity => TypeParameters.Length;

	internal override bool MangleName => Arity > 0;

	public override bool IsImplicitlyDeclared => true;

	internal override bool ShouldAddWinRTMembers => false;

	internal override bool IsWindowsRuntimeImport => false;

	internal override bool IsComImport => false;

	internal sealed override ObsoleteAttributeData ObsoleteAttributeData => null;

	internal override bool HasDeclarativeSecurity => false;

	internal override CharSet MarshallingCharSet => DefaultMarshallingCharSet;

	public override bool IsSerializable => false;

	internal override TypeLayout Layout => default;

	internal override bool HasSpecialName => false;

	protected SynthesizedContainer(string name, int parameterCount, bool returnsVoid)
	{
		Name = name;
		TypeMap = TypeMap.Empty;
		_typeParameters = CreateTypeParameters(parameterCount, returnsVoid);
		_constructedFromTypeParameters = default;
	}

	protected SynthesizedContainer(string name, MethodSymbol containingMethod)
	{
		Name = name;
		if (containingMethod == null)
		{
			TypeMap = TypeMap.Empty;
			_typeParameters = ImmutableArray<TypeParameterSymbol>.Empty;
		}
		else
		{
			TypeMap = TypeMap.Empty.WithConcatAlphaRename(containingMethod, this, out _typeParameters, out _constructedFromTypeParameters);
		}
	}

	protected SynthesizedContainer(string name, ImmutableArray<TypeParameterSymbol> typeParameters, TypeMap typeMap)
	{
		Name = name;
		_typeParameters = typeParameters;
		TypeMap = typeMap;
	}

	private ImmutableArray<TypeParameterSymbol> CreateTypeParameters(int parameterCount, bool returnsVoid)
	{
		ArrayBuilder<TypeParameterSymbol> instance = ArrayBuilder<TypeParameterSymbol>.GetInstance(parameterCount + ((!returnsVoid) ? 1 : 0));
		for (int i = 0; i < parameterCount; i++)
		{
			instance.Add(new AnonymousTypeManager.AnonymousTypeParameterSymbol(this, i, "T" + (i + 1)));
		}
		if (!returnsVoid)
		{
			instance.Add(new AnonymousTypeManager.AnonymousTypeParameterSymbol(this, parameterCount, "TResult"));
		}
		return instance.ToImmutableAndFree();
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<SynthesizedAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		if (ContainingSymbol.Kind != SymbolKind.NamedType || !ContainingSymbol.IsImplicitlyDeclared)
		{
			CSharpCompilation declaringCompilation = ContainingSymbol.DeclaringCompilation;
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_CompilerGeneratedAttribute__ctor));
		}
	}

	public override ImmutableArray<CustomModifier> GetTypeArgumentCustomModifiers(int ordinal)
	{
		return GetEmptyTypeArgumentCustomModifiers(ordinal);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		Symbol constructor = Constructor;
		if ((object)constructor != null)
		{
			return ImmutableArray.Create(constructor);
		}
		return ImmutableArray<Symbol>.Empty;
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		MethodSymbol constructor = Constructor;
		if ((object)constructor == null || !(name == constructor.Name))
		{
			return ImmutableArray<Symbol>.Empty;
		}
		return ImmutableArray.Create((Symbol)constructor);
	}

	internal override IEnumerable<FieldSymbol> GetFieldsToEmit()
	{
		foreach (Symbol member in GetMembers())
		{
			SymbolKind kind = member.Kind;
			if (kind == SymbolKind.Field)
			{
				yield return (FieldSymbol)member;
			}
		}
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers()
	{
		return GetMembersUnordered();
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers(string name)
	{
		return GetMembers(name);
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name, int arity)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<Symbol> basesBeingResolved)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetInterfacesToEmit()
	{
		return CalculateInterfacesToEmit();
	}

	internal override NamedTypeSymbol GetDeclaredBaseType(ConsList<Symbol> basesBeingResolved)
	{
		return BaseTypeNoUseSiteDiagnostics;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetDeclaredInterfaces(ConsList<Symbol> basesBeingResolved)
	{
		return InterfacesNoUseSiteDiagnostics(basesBeingResolved);
	}

	internal sealed override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		return ImmutableArray<string>.Empty;
	}

	internal override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override AttributeUsageInfo GetAttributeUsageInfo()
	{
		return default;
	}
}
