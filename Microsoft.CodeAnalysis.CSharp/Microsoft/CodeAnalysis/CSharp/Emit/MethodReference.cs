using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal abstract class MethodReference : TypeMemberReference, IMethodReference, ISignature, ITypeMemberReference, IReference, INamedEntity
{
	protected readonly MethodSymbol UnderlyingMethod;

	protected override Symbol UnderlyingSymbol => UnderlyingMethod;

	bool IMethodReference.AcceptsExtraArguments => UnderlyingMethod.IsVararg;

	ushort IMethodReference.GenericParameterCount => (ushort)UnderlyingMethod.Arity;

	bool IMethodReference.IsGeneric => UnderlyingMethod.IsGenericMethod;

	ushort ISignature.ParameterCount => (ushort)UnderlyingMethod.ParameterCount;

	ImmutableArray<IParameterTypeInformation> IMethodReference.ExtraParameters => ImmutableArray<IParameterTypeInformation>.Empty;

	CallingConvention ISignature.CallingConvention => UnderlyingMethod.CallingConvention;

	ImmutableArray<ICustomModifier> ISignature.ReturnValueCustomModifiers => UnderlyingMethod.ReturnTypeCustomModifiers.As<ICustomModifier>();

	ImmutableArray<ICustomModifier> ISignature.RefCustomModifiers => UnderlyingMethod.RefCustomModifiers.As<ICustomModifier>();

	bool ISignature.ReturnValueIsByRef => UnderlyingMethod.RefKind.IsManagedReference();

	public virtual IGenericMethodInstanceReference AsGenericMethodInstanceReference => null;

	public virtual ISpecializedMethodReference AsSpecializedMethodReference => null;

	public MethodReference(MethodSymbol underlyingMethod)
	{
		UnderlyingMethod = underlyingMethod;
	}

	IMethodDefinition IMethodReference.GetResolvedMethod(EmitContext context)
	{
		return null;
	}

	ImmutableArray<IParameterTypeInformation> ISignature.GetParameters(EmitContext context)
	{
		return ((PEModuleBuilder)context.Module).Translate(UnderlyingMethod.Parameters);
	}

	ITypeReference ISignature.GetType(EmitContext context)
	{
		return ((PEModuleBuilder)context.Module).Translate(UnderlyingMethod.ReturnType, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
	}
}
