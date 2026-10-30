using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.Cci;

internal interface IParameterDefinition : IDefinition, IReference, INamedEntity, IParameterTypeInformation, IParameterListEntry
{
	bool HasDefaultValue { get; }

	bool IsIn { get; }

	bool IsMarshalledExplicitly { get; }

	bool IsOptional { get; }

	bool IsOut { get; }

	IMarshallingInformation MarshallingInformation { get; }

	ImmutableArray<byte> MarshallingDescriptor { get; }

	MetadataConstant GetDefaultValue(EmitContext context);
}
