using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal sealed class PEAssemblyBuilder : PEAssemblyBuilderBase
{
	public override int CurrentGenerationOrdinal => 0;

	public PEAssemblyBuilder(SourceAssemblySymbol sourceAssembly, EmitOptions emitOptions, OutputKind outputKind, ModulePropertiesForSerialization serializationProperties, IEnumerable<ResourceDescription> manifestResources)
		: base(sourceAssembly, emitOptions, outputKind, serializationProperties, manifestResources, ImmutableArray<NamedTypeSymbol>.Empty)
	{
	}
}
