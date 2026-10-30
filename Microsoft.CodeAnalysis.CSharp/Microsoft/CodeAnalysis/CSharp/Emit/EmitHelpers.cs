using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection.Metadata;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal static class EmitHelpers
{
	internal static EmitDifferenceResult EmitDifference(CSharpCompilation compilation, EmitBaseline baseline, IEnumerable<SemanticEdit> edits, Func<ISymbol, bool> isAddedSymbol, Stream metadataStream, Stream ilStream, Stream pdbStream, ICollection<MethodDefinitionHandle> updatedMethods, CompilationTestData testData, CancellationToken cancellationToken)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		EmitOptions emitOptions = EmitOptions.Default.WithDebugInformationFormat((!baseline.HasPortablePdb) ? DebugInformationFormat.Pdb : DebugInformationFormat.PortablePdb);
		string runtimeMetadataVersion = compilation.GetRuntimeMetadataVersion(emitOptions, instance);
		ModulePropertiesForSerialization serializationProperties = compilation.ConstructModuleSerializationProperties(emitOptions, runtimeMetadataVersion, baseline.ModuleVersionId);
		IEnumerable<ResourceDescription> manifestResources = SpecializedCollections.EmptyEnumerable<ResourceDescription>();
		PEDeltaAssemblyBuilder pEDeltaAssemblyBuilder;
		try
		{
			pEDeltaAssemblyBuilder = new PEDeltaAssemblyBuilder(compilation.SourceAssembly, emitOptions, compilation.Options.OutputKind, serializationProperties, manifestResources, baseline, edits, isAddedSymbol);
		}
		catch (NotSupportedException)
		{
			instance.Add(ErrorCode.ERR_ModuleEmitFailure, NoLocation.Singleton, compilation.AssemblyName);
			return new EmitDifferenceResult(success: false, instance.ToReadOnlyAndFree(), null);
		}
		if (testData != null)
		{
			pEDeltaAssemblyBuilder.SetMethodTestData(testData.Methods);
			testData.Module = pEDeltaAssemblyBuilder;
		}
		CSharpDefinitionMap previousDefinitions = pEDeltaAssemblyBuilder.PreviousDefinitions;
		SymbolChanges changes = pEDeltaAssemblyBuilder.Changes;
		EmitBaseline emitBaseline = null;
		if (compilation.Compile(pEDeltaAssemblyBuilder, emittingPdb: true, instance, changes.RequiresCompilation, cancellationToken))
		{
			EmitBaseline baseline2 = MapToCompilation(compilation, pEDeltaAssemblyBuilder);
			emitBaseline = compilation.SerializeToDeltaStreams(pEDeltaAssemblyBuilder, baseline2, previousDefinitions, changes, metadataStream, ilStream, pdbStream, updatedMethods, instance, testData?.SymWriterFactory, emitOptions.PdbFilePath, cancellationToken);
		}
		return new EmitDifferenceResult(emitBaseline != null, instance.ToReadOnlyAndFree(), emitBaseline);
	}

	private static EmitBaseline MapToCompilation(CSharpCompilation compilation, PEDeltaAssemblyBuilder moduleBeingBuilt)
	{
		EmitBaseline previousGeneration = moduleBeingBuilt.PreviousGeneration;
		if (previousGeneration.Ordinal == 0)
		{
			return previousGeneration;
		}
		ImmutableDictionary<ITypeDefinition, ImmutableArray<ITypeDefinitionMember>> synthesizedMembers = moduleBeingBuilt.GetSynthesizedMembers();
		IReadOnlyDictionary<AnonymousTypeKey, AnonymousTypeValue> anonymousTypeMap = moduleBeingBuilt.GetAnonymousTypeMap();
		SourceAssemblySymbol sourceAssembly = ((CSharpCompilation)previousGeneration.Compilation).SourceAssembly;
		EmitContext sourceContext = new EmitContext((PEModuleBuilder)previousGeneration.PEModuleBuilder, null, new DiagnosticBag(), metadataOnly: false, includePrivateMembers: true);
		EmitContext otherContext = new EmitContext(moduleBeingBuilt, null, new DiagnosticBag(), metadataOnly: false, includePrivateMembers: true);
		ImmutableDictionary<ITypeDefinition, ImmutableArray<ITypeDefinitionMember>> immutableDictionary = new CSharpSymbolMatcher(anonymousTypeMap, sourceAssembly, sourceContext, compilation.SourceAssembly, otherContext, synthesizedMembers).MapSynthesizedMembers(previousGeneration.SynthesizedMembers, synthesizedMembers);
		return new CSharpSymbolMatcher(anonymousTypeMap, sourceAssembly, sourceContext, compilation.SourceAssembly, otherContext, immutableDictionary).MapBaselineToCompilation(previousGeneration, compilation, moduleBeingBuilt, immutableDictionary);
	}
}
