using System.Collections.Immutable;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class InstrumentationPayloadRootField : SynthesizedStaticField
{
	public override ImmutableArray<byte> MappedData => default;

	internal InstrumentationPayloadRootField(INamedTypeDefinition containingType, int analysisIndex, ITypeReference payloadType)
		: base("PayloadRoot" + analysisIndex, containingType, payloadType)
	{
	}
}
