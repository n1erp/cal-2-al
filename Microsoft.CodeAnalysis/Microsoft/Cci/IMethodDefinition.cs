using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.Cci;

internal interface IMethodDefinition : ITypeDefinitionMember, ITypeMemberReference, IReference, INamedEntity, IDefinition, IMethodReference, ISignature
{
	IEnumerable<IGenericMethodParameter> GenericParameters { get; }

	bool IsImplicitlyDeclared { get; }

	bool HasDeclarativeSecurity { get; }

	bool IsAbstract { get; }

	bool IsAccessCheckedOnOverride { get; }

	bool IsConstructor { get; }

	bool IsExternal { get; }

	bool IsHiddenBySignature { get; }

	bool IsNewSlot { get; }

	bool IsPlatformInvoke { get; }

	bool IsRuntimeSpecial { get; }

	bool IsSealed { get; }

	bool IsSpecialName { get; }

	bool IsStatic { get; }

	bool IsVirtual { get; }

	ImmutableArray<IParameterDefinition> Parameters { get; }

	IPlatformInvokeInformation PlatformInvokeData { get; }

	bool RequiresSecurityObject { get; }

	bool ReturnValueIsMarshalledExplicitly { get; }

	IMarshallingInformation ReturnValueMarshallingInformation { get; }

	ImmutableArray<byte> ReturnValueMarshallingDescriptor { get; }

	IEnumerable<SecurityAttribute> SecurityAttributes { get; }

	INamespace ContainingNamespace { get; }

	IMethodBody GetBody(EmitContext context);

	MethodImplAttributes GetImplementationAttributes(EmitContext context);

	IEnumerable<ICustomAttribute> GetReturnValueAttributes(EmitContext context);
}
