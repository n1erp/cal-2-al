using System.Collections.Immutable;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class ModuleVersionIdField : SynthesizedStaticField
{
	public override ImmutableArray<byte> MappedData => default;

	internal ModuleVersionIdField(INamedTypeDefinition containingType, ITypeReference type)
		: base("MVID", containingType, type)
	{
	}
}
