using System.Collections.Generic;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal sealed class PENetModuleBuilder : PEModuleBuilder
{
	public override int CurrentGenerationOrdinal => 0;

	public override ISourceAssemblySymbolInternal SourceAssemblyOpt => null;

	internal PENetModuleBuilder(SourceModuleSymbol sourceModule, EmitOptions emitOptions, ModulePropertiesForSerialization serializationProperties, IEnumerable<ResourceDescription> manifestResources)
		: base(sourceModule, emitOptions, OutputKind.NetModule, serializationProperties, manifestResources)
	{
	}

	protected override void AddEmbeddedResourcesFromAddedModules(ArrayBuilder<ManagedResource> builder, DiagnosticBag diagnostics)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override SynthesizedAttributeData SynthesizeEmbeddedAttribute()
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override IEnumerable<IFileReference> GetFiles(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<IFileReference>();
	}
}
