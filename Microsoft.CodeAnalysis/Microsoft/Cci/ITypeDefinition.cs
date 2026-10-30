using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.Cci;

internal interface ITypeDefinition : IDefinition, IReference, ITypeReference
{
	ushort Alignment { get; }

	IEnumerable<IGenericTypeParameter> GenericParameters { get; }

	ushort GenericParameterCount { get; }

	bool HasDeclarativeSecurity { get; }

	bool IsAbstract { get; }

	bool IsBeforeFieldInit { get; }

	bool IsComObject { get; }

	bool IsGeneric { get; }

	bool IsInterface { get; }

	bool IsRuntimeSpecial { get; }

	bool IsSerializable { get; }

	bool IsSpecialName { get; }

	bool IsWindowsRuntimeImport { get; }

	bool IsSealed { get; }

	LayoutKind Layout { get; }

	IEnumerable<SecurityAttribute> SecurityAttributes { get; }

	uint SizeOf { get; }

	CharSet StringFormat { get; }

	ITypeReference GetBaseClass(EmitContext context);

	IEnumerable<IEventDefinition> GetEvents(EmitContext context);

	IEnumerable<MethodImplementation> GetExplicitImplementationOverrides(EmitContext context);

	IEnumerable<IFieldDefinition> GetFields(EmitContext context);

	IEnumerable<TypeReferenceWithAttributes> Interfaces(EmitContext context);

	IEnumerable<IMethodDefinition> GetMethods(EmitContext context);

	IEnumerable<INestedTypeDefinition> GetNestedTypes(EmitContext context);

	IEnumerable<IPropertyDefinition> GetProperties(EmitContext context);
}
