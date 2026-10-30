using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis.Emit;
using Roslyn.Utilities;

namespace Microsoft.Cci;

internal sealed class ModifiedTypeReference : IModifiedTypeReference, ITypeReference, IReference
{
	private readonly ITypeReference _modifiedType;

	private readonly ImmutableArray<ICustomModifier> _customModifiers;

	ImmutableArray<ICustomModifier> IModifiedTypeReference.CustomModifiers => _customModifiers;

	ITypeReference IModifiedTypeReference.UnmodifiedType => _modifiedType;

	bool ITypeReference.IsEnum
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	bool ITypeReference.IsValueType
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	PrimitiveTypeCode ITypeReference.TypeCode => PrimitiveTypeCode.NotPrimitive;

	TypeDefinitionHandle ITypeReference.TypeDef
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	IGenericMethodParameterReference ITypeReference.AsGenericMethodParameterReference => null;

	IGenericTypeInstanceReference ITypeReference.AsGenericTypeInstanceReference => null;

	IGenericTypeParameterReference ITypeReference.AsGenericTypeParameterReference => null;

	INamespaceTypeReference ITypeReference.AsNamespaceTypeReference => null;

	INestedTypeReference ITypeReference.AsNestedTypeReference => null;

	ISpecializedNestedTypeReference ITypeReference.AsSpecializedNestedTypeReference => null;

	public ModifiedTypeReference(ITypeReference modifiedType, ImmutableArray<ICustomModifier> customModifiers)
	{
		_modifiedType = modifiedType;
		_customModifiers = customModifiers;
	}

	ITypeDefinition ITypeReference.GetResolvedType(EmitContext context)
	{
		throw ExceptionUtilities.Unreachable;
	}

	IEnumerable<ICustomAttribute> IReference.GetAttributes(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<ICustomAttribute>();
	}

	void IReference.Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
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
}
