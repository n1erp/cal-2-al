using System.Collections.Immutable;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class MappedField : SynthesizedStaticField
{
	private readonly ImmutableArray<byte> _block;

	public override ImmutableArray<byte> MappedData => _block;

	internal MappedField(string name, INamedTypeDefinition containingType, ITypeReference type, ImmutableArray<byte> block)
		: base(name, containingType, type)
	{
		_block = block;
	}
}
