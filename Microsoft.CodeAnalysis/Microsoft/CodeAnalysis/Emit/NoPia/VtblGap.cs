using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Cci;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Emit.NoPia;

internal sealed class VtblGap : IMethodDefinition, ITypeDefinitionMember, ITypeMemberReference, IReference, INamedEntity, IDefinition, IMethodReference, ISignature
{
	public readonly ITypeDefinition ContainingType;

	private readonly string _name;

	IEnumerable<IGenericMethodParameter> IMethodDefinition.GenericParameters => SpecializedCollections.EmptyEnumerable<IGenericMethodParameter>();

	bool IMethodDefinition.IsImplicitlyDeclared => true;

	bool IMethodDefinition.HasDeclarativeSecurity => false;

	bool IMethodDefinition.IsAbstract => false;

	bool IMethodDefinition.IsAccessCheckedOnOverride => false;

	bool IMethodDefinition.IsConstructor => false;

	bool IMethodDefinition.IsExternal => false;

	bool IMethodDefinition.IsHiddenBySignature => false;

	bool IMethodDefinition.IsNewSlot => false;

	bool IMethodDefinition.IsPlatformInvoke => false;

	bool IMethodDefinition.IsRuntimeSpecial => true;

	bool IMethodDefinition.IsSealed => false;

	bool IMethodDefinition.IsSpecialName => true;

	bool IMethodDefinition.IsStatic => false;

	bool IMethodDefinition.IsVirtual => false;

	ImmutableArray<IParameterDefinition> IMethodDefinition.Parameters => ImmutableArray<IParameterDefinition>.Empty;

	IPlatformInvokeInformation IMethodDefinition.PlatformInvokeData => null;

	bool IMethodDefinition.RequiresSecurityObject => false;

	bool IMethodDefinition.ReturnValueIsMarshalledExplicitly => false;

	IMarshallingInformation IMethodDefinition.ReturnValueMarshallingInformation => null;

	ImmutableArray<byte> IMethodDefinition.ReturnValueMarshallingDescriptor => default;

	IEnumerable<SecurityAttribute> IMethodDefinition.SecurityAttributes => SpecializedCollections.EmptyEnumerable<SecurityAttribute>();

	ITypeDefinition ITypeDefinitionMember.ContainingTypeDefinition => ContainingType;

	INamespace IMethodDefinition.ContainingNamespace => null;

	TypeMemberVisibility ITypeDefinitionMember.Visibility => TypeMemberVisibility.Public;

	string INamedEntity.Name => _name;

	bool IMethodReference.AcceptsExtraArguments => false;

	ushort IMethodReference.GenericParameterCount => 0;

	bool IMethodReference.IsGeneric => false;

	ImmutableArray<IParameterTypeInformation> IMethodReference.ExtraParameters => ImmutableArray<IParameterTypeInformation>.Empty;

	IGenericMethodInstanceReference IMethodReference.AsGenericMethodInstanceReference => null;

	ISpecializedMethodReference IMethodReference.AsSpecializedMethodReference => null;

	CallingConvention ISignature.CallingConvention => CallingConvention.HasThis;

	ushort ISignature.ParameterCount => 0;

	ImmutableArray<ICustomModifier> ISignature.ReturnValueCustomModifiers => ImmutableArray<ICustomModifier>.Empty;

	ImmutableArray<ICustomModifier> ISignature.RefCustomModifiers => ImmutableArray<ICustomModifier>.Empty;

	bool ISignature.ReturnValueIsByRef => false;

	public VtblGap(ITypeDefinition containingType, string name)
	{
		ContainingType = containingType;
		_name = name;
	}

	IMethodBody IMethodDefinition.GetBody(EmitContext context)
	{
		return null;
	}

	MethodImplAttributes IMethodDefinition.GetImplementationAttributes(EmitContext context)
	{
		return MethodImplAttributes.CodeTypeMask;
	}

	IEnumerable<ICustomAttribute> IMethodDefinition.GetReturnValueAttributes(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<ICustomAttribute>();
	}

	ITypeReference ITypeMemberReference.GetContainingType(EmitContext context)
	{
		return ContainingType;
	}

	IEnumerable<ICustomAttribute> IReference.GetAttributes(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<ICustomAttribute>();
	}

	void IReference.Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}

	IDefinition IReference.AsDefinition(EmitContext context)
	{
		return this;
	}

	IMethodDefinition IMethodReference.GetResolvedMethod(EmitContext context)
	{
		return this;
	}

	ImmutableArray<IParameterTypeInformation> ISignature.GetParameters(EmitContext context)
	{
		return ImmutableArray<IParameterTypeInformation>.Empty;
	}

	ITypeReference ISignature.GetType(EmitContext context)
	{
		return context.Module.GetPlatformType(PlatformType.SystemVoid, context);
	}
}
