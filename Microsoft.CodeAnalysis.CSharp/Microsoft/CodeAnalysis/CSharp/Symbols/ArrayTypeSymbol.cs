using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class ArrayTypeSymbol : TypeSymbol, IArrayTypeReference, ITypeReference, IReference, IArrayTypeSymbol, ITypeSymbol, INamespaceOrTypeSymbol, ISymbol, IEquatable<ISymbol>
{
	private sealed class SZArray : ArrayTypeSymbol
	{
		private readonly ImmutableArray<NamedTypeSymbol> _interfaces;

		public override int Rank => 1;

		public override bool IsSZArray => true;

		internal override bool HasDefaultSizesAndLowerBounds => true;

		internal SZArray(TypeSymbol elementType, NamedTypeSymbol array, ImmutableArray<NamedTypeSymbol> constructedInterfaces, ImmutableArray<CustomModifier> customModifiers)
			: base(elementType, array, customModifiers)
		{
			_interfaces = constructedInterfaces;
		}

		internal override ArrayTypeSymbol WithElementType(TypeSymbol newElementType)
		{
			ImmutableArray<NamedTypeSymbol> immutableArray = _interfaces;
			if (immutableArray.Length > 0)
			{
				immutableArray = immutableArray.SelectAsArray((NamedTypeSymbol i) => i.OriginalDefinition.Construct(newElementType));
			}
			return new SZArray(newElementType, BaseTypeNoUseSiteDiagnostics, immutableArray, CustomModifiers);
		}

		internal override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<Symbol> basesBeingResolved = null)
		{
			return _interfaces;
		}
	}

	private abstract class MDArray : ArrayTypeSymbol
	{
		private readonly int _rank;

		public sealed override int Rank => _rank;

		public sealed override bool IsSZArray => false;

		internal MDArray(TypeSymbol elementType, int rank, NamedTypeSymbol array, ImmutableArray<CustomModifier> customModifiers)
			: base(elementType, array, customModifiers)
		{
			_rank = rank;
		}

		internal sealed override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<Symbol> basesBeingResolved = null)
		{
			return ImmutableArray<NamedTypeSymbol>.Empty;
		}
	}

	private sealed class MDArrayNoSizesOrBounds : MDArray
	{
		internal override bool HasDefaultSizesAndLowerBounds => true;

		internal MDArrayNoSizesOrBounds(TypeSymbol elementType, int rank, NamedTypeSymbol array, ImmutableArray<CustomModifier> customModifiers)
			: base(elementType, rank, array, customModifiers)
		{
		}

		internal override ArrayTypeSymbol WithElementType(TypeSymbol elementType)
		{
			return new MDArrayNoSizesOrBounds(elementType, Rank, BaseTypeNoUseSiteDiagnostics, CustomModifiers);
		}
	}

	private sealed class MDArrayWithSizesAndBounds : MDArray
	{
		private readonly ImmutableArray<int> _sizes;

		private readonly ImmutableArray<int> _lowerBounds;

		public override ImmutableArray<int> Sizes => _sizes;

		public override ImmutableArray<int> LowerBounds => _lowerBounds;

		internal override bool HasDefaultSizesAndLowerBounds => false;

		internal MDArrayWithSizesAndBounds(TypeSymbol elementType, int rank, ImmutableArray<int> sizes, ImmutableArray<int> lowerBounds, NamedTypeSymbol array, ImmutableArray<CustomModifier> customModifiers)
			: base(elementType, rank, array, customModifiers)
		{
			_sizes = sizes.NullToEmpty();
			_lowerBounds = lowerBounds;
		}

		internal override ArrayTypeSymbol WithElementType(TypeSymbol elementType)
		{
			return new MDArrayWithSizesAndBounds(elementType, Rank, _sizes, _lowerBounds, BaseTypeNoUseSiteDiagnostics, CustomModifiers);
		}
	}

	private readonly TypeSymbol _elementType;

	private readonly NamedTypeSymbol _baseType;

	private readonly ImmutableArray<CustomModifier> _customModifiers;

	bool IArrayTypeReference.IsSZArray => IsSZArray;

	ImmutableArray<int> IArrayTypeReference.LowerBounds => LowerBounds;

	int IArrayTypeReference.Rank => Rank;

	ImmutableArray<int> IArrayTypeReference.Sizes => Sizes;

	bool ITypeReference.IsEnum => false;

	bool ITypeReference.IsValueType => false;

	TypeDefinitionHandle ITypeReference.TypeDef => default;

	Microsoft.Cci.PrimitiveTypeCode ITypeReference.TypeCode => Microsoft.Cci.PrimitiveTypeCode.NotPrimitive;

	IGenericMethodParameterReference ITypeReference.AsGenericMethodParameterReference => null;

	IGenericTypeInstanceReference ITypeReference.AsGenericTypeInstanceReference => null;

	IGenericTypeParameterReference ITypeReference.AsGenericTypeParameterReference => null;

	INamespaceTypeReference ITypeReference.AsNamespaceTypeReference => null;

	INestedTypeReference ITypeReference.AsNestedTypeReference => null;

	ISpecializedNestedTypeReference ITypeReference.AsSpecializedNestedTypeReference => null;

	public ImmutableArray<CustomModifier> CustomModifiers => _customModifiers;

	public abstract int Rank { get; }

	public abstract bool IsSZArray { get; }

	public virtual ImmutableArray<int> Sizes => ImmutableArray<int>.Empty;

	public virtual ImmutableArray<int> LowerBounds => default;

	internal abstract bool HasDefaultSizesAndLowerBounds { get; }

	public TypeSymbol ElementType => _elementType;

	internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics => _baseType;

	public override bool IsReferenceType => true;

	public override bool IsValueType => false;

	internal sealed override bool IsManagedType => true;

	internal sealed override bool IsByRefLikeType => false;

	internal sealed override bool IsReadOnly => false;

	internal sealed override ObsoleteAttributeData ObsoleteAttributeData => null;

	public override SymbolKind Kind => SymbolKind.ArrayType;

	public override TypeKind TypeKind => TypeKind.Array;

	public override Symbol ContainingSymbol => null;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public override bool IsStatic => false;

	public override bool IsAbstract => false;

	public override bool IsSealed => false;

	ITypeSymbol IArrayTypeSymbol.ElementType => ElementType;

	ImmutableArray<CustomModifier> IArrayTypeSymbol.CustomModifiers => CustomModifiers;

	ITypeReference IArrayTypeReference.GetElementType(EmitContext context)
	{
		ITypeReference typeReference = ((PEModuleBuilder)context.Module).Translate(ElementType, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
		if (CustomModifiers.Length == 0)
		{
			return typeReference;
		}
		return new ModifiedTypeReference(typeReference, CustomModifiers.As<ICustomModifier>());
	}

	void IReference.Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}

	ITypeDefinition ITypeReference.GetResolvedType(EmitContext context)
	{
		return null;
	}

	INamespaceTypeDefinition ITypeReference.AsNamespaceTypeDefinition(EmitContext context)
	{
		return null;
	}

	INestedTypeDefinition ITypeReference.AsNestedTypeDefinition(EmitContext context)
	{
		return null;
	}

	ITypeDefinition ITypeReference.AsTypeDefinition(EmitContext context)
	{
		return null;
	}

	IDefinition IReference.AsDefinition(EmitContext context)
	{
		return null;
	}

	private ArrayTypeSymbol(TypeSymbol elementType, NamedTypeSymbol array, ImmutableArray<CustomModifier> customModifiers)
	{
		_elementType = elementType;
		_baseType = array;
		_customModifiers = customModifiers.NullToEmpty();
	}

	internal static ArrayTypeSymbol CreateCSharpArray(AssemblySymbol declaringAssembly, TypeSymbol elementType, ImmutableArray<CustomModifier> customModifiers = default(ImmutableArray<CustomModifier>), int rank = 1)
	{
		if (rank == 1)
		{
			return CreateSZArray(declaringAssembly, elementType, customModifiers);
		}
		return CreateMDArray(declaringAssembly, elementType, rank, default, default, customModifiers);
	}

	internal static ArrayTypeSymbol CreateMDArray(TypeSymbol elementType, int rank, ImmutableArray<int> sizes, ImmutableArray<int> lowerBounds, NamedTypeSymbol array, ImmutableArray<CustomModifier> customModifiers)
	{
		if (sizes.IsDefaultOrEmpty && lowerBounds.IsDefault)
		{
			return new MDArrayNoSizesOrBounds(elementType, rank, array, customModifiers);
		}
		return new MDArrayWithSizesAndBounds(elementType, rank, sizes, lowerBounds, array, customModifiers);
	}

	internal static ArrayTypeSymbol CreateMDArray(AssemblySymbol declaringAssembly, TypeSymbol elementType, int rank, ImmutableArray<int> sizes, ImmutableArray<int> lowerBounds, ImmutableArray<CustomModifier> customModifiers = default(ImmutableArray<CustomModifier>))
	{
		return CreateMDArray(elementType, rank, sizes, lowerBounds, declaringAssembly.GetSpecialType(SpecialType.System_Array), customModifiers);
	}

	internal static ArrayTypeSymbol CreateSZArray(TypeSymbol elementType, NamedTypeSymbol array, ImmutableArray<NamedTypeSymbol> constructedInterfaces, ImmutableArray<CustomModifier> customModifiers)
	{
		return new SZArray(elementType, array, constructedInterfaces, customModifiers);
	}

	internal static ArrayTypeSymbol CreateSZArray(AssemblySymbol declaringAssembly, TypeSymbol elementType, ImmutableArray<CustomModifier> customModifiers = default(ImmutableArray<CustomModifier>))
	{
		return CreateSZArray(elementType, declaringAssembly.GetSpecialType(SpecialType.System_Array), GetSZArrayInterfaces(elementType, declaringAssembly), customModifiers);
	}

	internal abstract ArrayTypeSymbol WithElementType(TypeSymbol elementType);

	private static ImmutableArray<NamedTypeSymbol> GetSZArrayInterfaces(TypeSymbol elementType, AssemblySymbol declaringAssembly)
	{
		ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance();
		NamedTypeSymbol specialType = declaringAssembly.GetSpecialType(SpecialType.System_Collections_Generic_IList_T);
		if (!specialType.IsErrorType())
		{
			instance.Add(new ConstructedNamedTypeSymbol(specialType, ImmutableArray.Create(new TypeWithModifiers(elementType))));
		}
		NamedTypeSymbol specialType2 = declaringAssembly.GetSpecialType(SpecialType.System_Collections_Generic_IReadOnlyList_T);
		if (!specialType2.IsErrorType())
		{
			instance.Add(new ConstructedNamedTypeSymbol(specialType2, ImmutableArray.Create(new TypeWithModifiers(elementType))));
		}
		return instance.ToImmutableAndFree();
	}

	internal bool HasSameShapeAs(ArrayTypeSymbol other)
	{
		if (Rank == other.Rank)
		{
			return IsSZArray == other.IsSZArray;
		}
		return false;
	}

	internal bool HasSameSizesAndLowerBoundsAs(ArrayTypeSymbol other)
	{
		if (Sizes.SequenceEqual(other.Sizes))
		{
			ImmutableArray<int> lowerBounds = LowerBounds;
			if (lowerBounds.IsDefault)
			{
				return other.LowerBounds.IsDefault;
			}
			ImmutableArray<int> lowerBounds2 = other.LowerBounds;
			if (!lowerBounds2.IsDefault)
			{
				return lowerBounds.SequenceEqual(lowerBounds2);
			}
			return false;
		}
		return false;
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return ImmutableArray<Symbol>.Empty;
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return ImmutableArray<Symbol>.Empty;
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

	internal override TResult Accept<TArgument, TResult>(CSharpSymbolVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitArrayType(this, argument);
	}

	public override void Accept(CSharpSymbolVisitor visitor)
	{
		visitor.VisitArrayType(this);
	}

	public override TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)
	{
		return visitor.VisitArrayType(this);
	}

	internal override bool Equals(TypeSymbol t2, TypeCompareKind comparison)
	{
		return Equals(t2 as ArrayTypeSymbol, comparison);
	}

	internal bool Equals(ArrayTypeSymbol other)
	{
		return Equals(other, TypeCompareKind.ConsiderEverything);
	}

	private bool Equals(ArrayTypeSymbol other, TypeCompareKind comparison)
	{
		if ((object)this == other)
		{
			return true;
		}
		if ((object)other == null || !other.HasSameShapeAs(this) || !other.ElementType.Equals(ElementType, comparison))
		{
			return false;
		}
		if ((comparison & TypeCompareKind.IgnoreCustomModifiersAndArraySizesAndLowerBounds) == 0)
		{
			ImmutableArray<CustomModifier> customModifiers = CustomModifiers;
			ImmutableArray<CustomModifier> customModifiers2 = other.CustomModifiers;
			int length = customModifiers.Length;
			if (length != customModifiers2.Length)
			{
				return false;
			}
			for (int i = 0; i < length; i++)
			{
				if (!customModifiers[i].Equals(customModifiers2[i]))
				{
					return false;
				}
			}
			if (!HasSameSizesAndLowerBoundsAs(other))
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		int currentKey = 0;
		TypeSymbol typeSymbol = this;
		while (typeSymbol.TypeKind == TypeKind.Array)
		{
			ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)typeSymbol;
			currentKey = Hash.Combine(arrayTypeSymbol.Rank, currentKey);
			typeSymbol = arrayTypeSymbol.ElementType;
		}
		return Hash.Combine(typeSymbol, currentKey);
	}

	internal override DiagnosticInfo GetUseSiteDiagnostic()
	{
		DiagnosticInfo result = null;
		if (DeriveUseSiteDiagnosticFromType(ref result, ElementType))
		{
			return result;
		}
		DeriveUseSiteDiagnosticFromCustomModifiers(ref result, CustomModifiers);
		return result;
	}

	internal override bool GetUnificationUseSiteDiagnosticRecursive(ref DiagnosticInfo result, Symbol owner, ref HashSet<TypeSymbol> checkedTypes)
	{
		if (!_elementType.GetUnificationUseSiteDiagnosticRecursive(ref result, owner, ref checkedTypes) && ((object)_baseType == null || !_baseType.GetUnificationUseSiteDiagnosticRecursive(ref result, owner, ref checkedTypes)) && !Symbol.GetUnificationUseSiteDiagnosticRecursive(ref result, InterfacesNoUseSiteDiagnostics(), owner, ref checkedTypes))
		{
			return Symbol.GetUnificationUseSiteDiagnosticRecursive(ref result, CustomModifiers, owner, ref checkedTypes);
		}
		return true;
	}

	bool IArrayTypeSymbol.Equals(IArrayTypeSymbol symbol)
	{
		return Equals(symbol as ArrayTypeSymbol);
	}

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitArrayType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitArrayType(this);
	}
}
