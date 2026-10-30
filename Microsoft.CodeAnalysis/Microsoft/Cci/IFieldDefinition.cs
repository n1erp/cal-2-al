using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.Cci;

internal interface IFieldDefinition : ITypeDefinitionMember, ITypeMemberReference, IReference, INamedEntity, IDefinition, IFieldReference
{
	ImmutableArray<byte> MappedData { get; }

	bool IsCompileTimeConstant { get; }

	bool IsMarshalledExplicitly { get; }

	bool IsNotSerialized { get; }

	bool IsReadOnly { get; }

	bool IsRuntimeSpecial { get; }

	bool IsSpecialName { get; }

	bool IsStatic { get; }

	IMarshallingInformation MarshallingInformation { get; }

	ImmutableArray<byte> MarshallingDescriptor { get; }

	int Offset { get; }

	MetadataConstant GetCompileTimeValue(EmitContext context);
}
