using System.Collections.Generic;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis.Emit;
using Roslyn.Utilities;

namespace Microsoft.Cci;

internal class InheritedTypeParameter : IGenericTypeParameter, IGenericParameter, IGenericParameterReference, ITypeReference, IReference, INamedEntity, IParameterListEntry, IGenericTypeParameterReference
{
	private readonly ushort _index;

	private readonly ITypeDefinition _inheritingType;

	private readonly IGenericTypeParameter _parentParameter;

	public ITypeDefinition DefiningType => _inheritingType;

	public bool MustBeReferenceType => _parentParameter.MustBeReferenceType;

	public bool MustBeValueType => _parentParameter.MustBeValueType;

	public bool MustHaveDefaultConstructor => _parentParameter.MustHaveDefaultConstructor;

	public TypeParameterVariance Variance => _parentParameter.Variance;

	public ushort Alignment => 0;

	public bool HasDeclarativeSecurity => false;

	public bool IsEnum => false;

	public IArrayTypeReference AsArrayTypeReference => this as IArrayTypeReference;

	public IGenericMethodParameter AsGenericMethodParameter => this as IGenericMethodParameter;

	public IGenericMethodParameterReference AsGenericMethodParameterReference => this as IGenericMethodParameterReference;

	public IGenericTypeInstanceReference AsGenericTypeInstanceReference => this as IGenericTypeInstanceReference;

	public IGenericTypeParameter AsGenericTypeParameter => this;

	public IGenericTypeParameterReference AsGenericTypeParameterReference => this;

	public INamespaceTypeReference AsNamespaceTypeReference => this as INamespaceTypeReference;

	public INestedTypeReference AsNestedTypeReference => this as INestedTypeReference;

	public ISpecializedNestedTypeReference AsSpecializedNestedTypeReference => this as ISpecializedNestedTypeReference;

	public IModifiedTypeReference AsModifiedTypeReference => this as IModifiedTypeReference;

	public IPointerTypeReference AsPointerTypeReference => this as IPointerTypeReference;

	public TypeDefinitionHandle TypeDef => default;

	public bool IsAlias => false;

	public bool IsValueType => false;

	public PrimitiveTypeCode TypeCode => PrimitiveTypeCode.NotPrimitive;

	public ushort Index => _index;

	public string Name => _parentParameter.Name;

	ITypeReference IGenericTypeParameterReference.DefiningType => _inheritingType;

	public bool MangleName => false;

	public bool IsNested
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	public bool IsSpecializedNested
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	public ITypeReference UnspecializedVersion
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	public bool IsNamespaceTypeReference
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	public bool IsGenericTypeInstance
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	internal InheritedTypeParameter(ushort index, ITypeDefinition inheritingType, IGenericTypeParameter parentParameter)
	{
		_index = index;
		_inheritingType = inheritingType;
		_parentParameter = parentParameter;
	}

	public IEnumerable<TypeReferenceWithAttributes> GetConstraints(EmitContext context)
	{
		return _parentParameter.GetConstraints(context);
	}

	public INamespaceTypeDefinition AsNamespaceTypeDefinition(EmitContext context)
	{
		return this as INamespaceTypeDefinition;
	}

	public INestedTypeDefinition AsNestedTypeDefinition(EmitContext context)
	{
		return this as INestedTypeDefinition;
	}

	public ITypeDefinition AsTypeDefinition(EmitContext context)
	{
		return this as ITypeDefinition;
	}

	public IDefinition AsDefinition(EmitContext context)
	{
		return this as IDefinition;
	}

	public IEnumerable<ICustomAttribute> GetAttributes(EmitContext context)
	{
		return _parentParameter.GetAttributes(context);
	}

	public void Dispatch(MetadataVisitor visitor)
	{
	}

	public ITypeDefinition GetResolvedType(EmitContext context)
	{
		throw ExceptionUtilities.Unreachable;
	}
}
