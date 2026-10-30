using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.Emit;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class DynamicTypeSymbol : TypeSymbol, ITypeReference, IReference, INamedTypeReference, INamedEntity, INamespaceTypeReference, IDynamicTypeSymbol, ITypeSymbol, INamespaceOrTypeSymbol, ISymbol, IEquatable<ISymbol>
{
	internal static readonly DynamicTypeSymbol Instance = new DynamicTypeSymbol();

	bool ITypeReference.IsEnum => false;

	bool ITypeReference.IsValueType => false;

	Microsoft.Cci.PrimitiveTypeCode ITypeReference.TypeCode => Microsoft.Cci.PrimitiveTypeCode.NotPrimitive;

	TypeDefinitionHandle ITypeReference.TypeDef => default;

	IGenericMethodParameterReference ITypeReference.AsGenericMethodParameterReference => null;

	IGenericTypeInstanceReference ITypeReference.AsGenericTypeInstanceReference => null;

	IGenericTypeParameterReference ITypeReference.AsGenericTypeParameterReference => null;

	INamespaceTypeReference ITypeReference.AsNamespaceTypeReference => this;

	INestedTypeReference ITypeReference.AsNestedTypeReference => null;

	ISpecializedNestedTypeReference ITypeReference.AsSpecializedNestedTypeReference => null;

	ushort INamedTypeReference.GenericParameterCount => 0;

	bool INamedTypeReference.MangleName => false;

	string INamedEntity.Name => "Object";

	string INamespaceTypeReference.NamespaceName => "System";

	public override string Name => "dynamic";

	public override bool IsAbstract => false;

	public override bool IsReferenceType => true;

	public override bool IsSealed => false;

	public override SymbolKind Kind => SymbolKind.DynamicType;

	public override TypeKind TypeKind => TypeKind.Dynamic;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics => null;

	public override bool IsStatic => false;

	public override bool IsValueType => false;

	internal sealed override bool IsManagedType => true;

	internal sealed override bool IsByRefLikeType => false;

	internal sealed override bool IsReadOnly => false;

	internal sealed override ObsoleteAttributeData ObsoleteAttributeData => null;

	public override Symbol ContainingSymbol => null;

	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

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
		throw ExceptionUtilities.Unreachable;
	}

	IDefinition IReference.AsDefinition(EmitContext context)
	{
		return null;
	}

	IUnitReference INamespaceTypeReference.GetUnit(EmitContext context)
	{
		return ((INamespaceTypeReference)((PEModuleBuilder)context.Module).GetSpecialType(SpecialType.System_Object, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics)).GetUnit(context);
	}

	private DynamicTypeSymbol()
	{
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

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal override TResult Accept<TArgument, TResult>(CSharpSymbolVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitDynamicType(this, argument);
	}

	public override void Accept(CSharpSymbolVisitor visitor)
	{
		visitor.VisitDynamicType(this);
	}

	public override TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)
	{
		return visitor.VisitDynamicType(this);
	}

	internal override bool GetUnificationUseSiteDiagnosticRecursive(ref DiagnosticInfo result, Symbol owner, ref HashSet<TypeSymbol> checkedTypes)
	{
		return false;
	}

	public override int GetHashCode()
	{
		return 1;
	}

	internal override bool Equals(TypeSymbol t2, TypeCompareKind comparison)
	{
		if ((object)t2 == null)
		{
			return false;
		}
		if ((object)this == t2 || t2.TypeKind == TypeKind.Dynamic)
		{
			return true;
		}
		if ((comparison & TypeCompareKind.IgnoreDynamic) != 0)
		{
			if (t2 is NamedTypeSymbol namedTypeSymbol)
			{
				return namedTypeSymbol.SpecialType == SpecialType.System_Object;
			}
			return false;
		}
		return false;
	}

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitDynamicType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitDynamicType(this);
	}
}
