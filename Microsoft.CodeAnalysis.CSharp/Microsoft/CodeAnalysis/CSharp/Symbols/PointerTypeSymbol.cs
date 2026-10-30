using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.Emit;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class PointerTypeSymbol : TypeSymbol, IPointerTypeReference, ITypeReference, IReference, IPointerTypeSymbol, ITypeSymbol, INamespaceOrTypeSymbol, ISymbol, IEquatable<ISymbol>
{
	private readonly TypeSymbol _pointedAtType;

	private readonly ImmutableArray<CustomModifier> _customModifiers;

	bool ITypeReference.IsEnum => false;

	bool ITypeReference.IsValueType => false;

	Microsoft.Cci.PrimitiveTypeCode ITypeReference.TypeCode => Microsoft.Cci.PrimitiveTypeCode.Pointer;

	TypeDefinitionHandle ITypeReference.TypeDef => default;

	IGenericMethodParameterReference ITypeReference.AsGenericMethodParameterReference => null;

	IGenericTypeInstanceReference ITypeReference.AsGenericTypeInstanceReference => null;

	IGenericTypeParameterReference ITypeReference.AsGenericTypeParameterReference => null;

	INamespaceTypeReference ITypeReference.AsNamespaceTypeReference => null;

	INestedTypeReference ITypeReference.AsNestedTypeReference => null;

	ISpecializedNestedTypeReference ITypeReference.AsSpecializedNestedTypeReference => null;

	public ImmutableArray<CustomModifier> CustomModifiers => _customModifiers;

	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public override bool IsStatic => false;

	public override bool IsAbstract => false;

	public override bool IsSealed => false;

	public TypeSymbol PointedAtType => _pointedAtType;

	internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics => null;

	public override bool IsReferenceType => false;

	public override bool IsValueType => true;

	internal sealed override bool IsManagedType => false;

	internal sealed override bool IsByRefLikeType => false;

	internal sealed override bool IsReadOnly => false;

	internal sealed override ObsoleteAttributeData ObsoleteAttributeData => null;

	public override SymbolKind Kind => SymbolKind.PointerType;

	public override TypeKind TypeKind => TypeKind.Pointer;

	public override Symbol ContainingSymbol => null;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	ITypeSymbol IPointerTypeSymbol.PointedAtType => PointedAtType;

	ImmutableArray<CustomModifier> IPointerTypeSymbol.CustomModifiers => CustomModifiers;

	ITypeReference IPointerTypeReference.GetTargetType(EmitContext context)
	{
		ITypeReference typeReference = ((PEModuleBuilder)context.Module).Translate(PointedAtType, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
		if (CustomModifiers.Length == 0)
		{
			return typeReference;
		}
		return new ModifiedTypeReference(typeReference, CustomModifiers.As<ICustomModifier>());
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

	void IReference.Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}

	IDefinition IReference.AsDefinition(EmitContext context)
	{
		return null;
	}

	internal PointerTypeSymbol(TypeSymbol pointedAtType)
		: this(pointedAtType, ImmutableArray<CustomModifier>.Empty)
	{
	}

	internal PointerTypeSymbol(TypeSymbol pointedAtType, ImmutableArray<CustomModifier> customModifiers)
	{
		_pointedAtType = pointedAtType;
		_customModifiers = customModifiers.NullToEmpty();
	}

	internal override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<Symbol> basesBeingResolved)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
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
		return visitor.VisitPointerType(this, argument);
	}

	public override void Accept(CSharpSymbolVisitor visitor)
	{
		visitor.VisitPointerType(this);
	}

	public override TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)
	{
		return visitor.VisitPointerType(this);
	}

	public override int GetHashCode()
	{
		int num = 0;
		TypeSymbol typeSymbol = this;
		while (typeSymbol.TypeKind == TypeKind.Pointer)
		{
			num++;
			typeSymbol = ((PointerTypeSymbol)typeSymbol).PointedAtType;
		}
		return Hash.Combine(typeSymbol, num);
	}

	internal override bool Equals(TypeSymbol t2, TypeCompareKind comparison)
	{
		return Equals(t2 as PointerTypeSymbol, comparison);
	}

	internal bool Equals(PointerTypeSymbol other)
	{
		return Equals(other, TypeCompareKind.IgnoreTupleNames);
	}

	private bool Equals(PointerTypeSymbol other, TypeCompareKind comparison)
	{
		if ((object)this == other)
		{
			return true;
		}
		if ((object)other == null || !other._pointedAtType.Equals(_pointedAtType, comparison))
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
		}
		return true;
	}

	internal override DiagnosticInfo GetUseSiteDiagnostic()
	{
		DiagnosticInfo result = null;
		if (!DeriveUseSiteDiagnosticFromType(ref result, PointedAtType))
		{
			DeriveUseSiteDiagnosticFromCustomModifiers(ref result, CustomModifiers);
		}
		return result;
	}

	internal override bool GetUnificationUseSiteDiagnosticRecursive(ref DiagnosticInfo result, Symbol owner, ref HashSet<TypeSymbol> checkedTypes)
	{
		if (!PointedAtType.GetUnificationUseSiteDiagnosticRecursive(ref result, owner, ref checkedTypes))
		{
			return Symbol.GetUnificationUseSiteDiagnosticRecursive(ref result, CustomModifiers, owner, ref checkedTypes);
		}
		return true;
	}

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitPointerType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitPointerType(this);
	}
}
