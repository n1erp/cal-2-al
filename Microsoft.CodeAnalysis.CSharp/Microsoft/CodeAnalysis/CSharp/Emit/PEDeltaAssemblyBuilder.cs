using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Emit.NoPia;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal sealed class PEDeltaAssemblyBuilder : PEAssemblyBuilderBase, IPEDeltaAssemblyBuilder
{
	private readonly EmitBaseline _previousGeneration;

	private readonly CSharpDefinitionMap _previousDefinitions;

	private readonly SymbolChanges _changes;

	private readonly CSharpSymbolMatcher.DeepTranslator _deepTranslator;

	public override int CurrentGenerationOrdinal => _previousGeneration.Ordinal + 1;

	internal EmitBaseline PreviousGeneration => _previousGeneration;

	internal CSharpDefinitionMap PreviousDefinitions => _previousDefinitions;

	internal override bool SupportsPrivateImplClass => false;

	internal SymbolChanges Changes => _changes;

	internal override bool IsEncDelta => true;

	public PEDeltaAssemblyBuilder(SourceAssemblySymbol sourceAssembly, EmitOptions emitOptions, OutputKind outputKind, ModulePropertiesForSerialization serializationProperties, IEnumerable<ResourceDescription> manifestResources, EmitBaseline previousGeneration, IEnumerable<SemanticEdit> edits, Func<ISymbol, bool> isAddedSymbol)
		: base(sourceAssembly, emitOptions, outputKind, serializationProperties, manifestResources, ImmutableArray<NamedTypeSymbol>.Empty)
	{
		EmitBaseline initialBaseline = previousGeneration.InitialBaseline;
		EmitContext sourceContext = new EmitContext(this, null, new DiagnosticBag(), metadataOnly: false, includePrivateMembers: true);
		EmitBaseline.MetadataSymbols orCreateMetadataSymbols = GetOrCreateMetadataSymbols(initialBaseline, sourceAssembly.DeclaringCompilation);
		MetadataDecoder metadataDecoder = (MetadataDecoder)orCreateMetadataSymbols.MetadataDecoder;
		CSharpSymbolMatcher mapToMetadata = new CSharpSymbolMatcher(otherAssembly: (PEAssemblySymbol)metadataDecoder.ModuleSymbol.ContainingAssembly, anonymousTypeMap: orCreateMetadataSymbols.AnonymousTypes, sourceAssembly: sourceAssembly, sourceContext: sourceContext);
		CSharpSymbolMatcher mapToPrevious = null;
		if (previousGeneration.Ordinal > 0)
		{
			SourceAssemblySymbol sourceAssembly2 = ((CSharpCompilation)previousGeneration.Compilation).SourceAssembly;
			mapToPrevious = new CSharpSymbolMatcher(otherContext: new EmitContext((PEModuleBuilder)previousGeneration.PEModuleBuilder, null, new DiagnosticBag(), metadataOnly: false, includePrivateMembers: true), anonymousTypeMap: previousGeneration.AnonymousTypeMap, sourceAssembly: sourceAssembly, sourceContext: sourceContext, otherAssembly: sourceAssembly2, otherSynthesizedMembersOpt: previousGeneration.SynthesizedMembers);
		}
		_previousDefinitions = new CSharpDefinitionMap(previousGeneration.OriginalMetadata.Module, edits, metadataDecoder, mapToMetadata, mapToPrevious);
		_previousGeneration = previousGeneration;
		_changes = new SymbolChanges(_previousDefinitions, edits, isAddedSymbol);
		_deepTranslator = new CSharpSymbolMatcher.DeepTranslator(sourceAssembly.GetSpecialType(SpecialType.System_Object));
	}

	internal override ITypeReference EncTranslateLocalVariableType(TypeSymbol type, DiagnosticBag diagnostics)
	{
		TypeSymbol typeSymbol = (TypeSymbol)_deepTranslator.Visit(type);
		return Translate(typeSymbol ?? type, null, diagnostics);
	}

	private static EmitBaseline.MetadataSymbols GetOrCreateMetadataSymbols(EmitBaseline initialBaseline, CSharpCompilation compilation)
	{
		if (initialBaseline.LazyMetadataSymbols != null)
		{
			return initialBaseline.LazyMetadataSymbols;
		}
		ModuleMetadata originalMetadata = initialBaseline.OriginalMetadata;
		MetadataDecoder metadataDecoder = new MetadataDecoder(compilation.RemoveAllSyntaxTrees().GetBoundReferenceManager().CreatePEAssemblyForAssemblyMetadata(AssemblyMetadata.Create(originalMetadata), MetadataImportOptions.All, out var assemblyReferenceIdentityMap)
			.PrimaryModule);
		EmitBaseline.MetadataSymbols value = new EmitBaseline.MetadataSymbols(GetAnonymousTypeMapFromMetadata(originalMetadata.MetadataReader, metadataDecoder), metadataDecoder, assemblyReferenceIdentityMap);
		return InterlockedOperations.Initialize(ref initialBaseline.LazyMetadataSymbols, value);
	}

	internal static IReadOnlyDictionary<AnonymousTypeKey, AnonymousTypeValue> GetAnonymousTypeMapFromMetadata(MetadataReader reader, MetadataDecoder metadataDecoder)
	{
		Dictionary<AnonymousTypeKey, AnonymousTypeValue> dictionary = new Dictionary<AnonymousTypeKey, AnonymousTypeValue>();
		foreach (TypeDefinitionHandle typeDefinition2 in reader.TypeDefinitions)
		{
			TypeDefinition typeDefinition = reader.GetTypeDefinition(typeDefinition2);
			if (!typeDefinition.Namespace.IsNil || !reader.StringComparer.StartsWith(typeDefinition.Name, "<>f__AnonymousType"))
			{
				continue;
			}
			string name = MetadataHelpers.InferTypeArityAndUnmangleMetadataName(reader.GetString(typeDefinition.Name), out var _);
			if (GeneratedNames.TryParseAnonymousTypeTemplateName(name, out var index))
			{
				ArrayBuilder<AnonymousTypeKeyField> instance = ArrayBuilder<AnonymousTypeKeyField>.GetInstance();
				if (TryGetAnonymousTypeKey(reader, typeDefinition, instance))
				{
					NamedTypeSymbol type = (NamedTypeSymbol)metadataDecoder.GetTypeOfToken(typeDefinition2);
					AnonymousTypeKey key = new AnonymousTypeKey(instance.ToImmutable());
					AnonymousTypeValue value = new AnonymousTypeValue(name, index, type);
					dictionary.Add(key, value);
				}
				instance.Free();
			}
		}
		return dictionary;
	}

	private static bool TryGetAnonymousTypeKey(MetadataReader reader, TypeDefinition def, ArrayBuilder<AnonymousTypeKeyField> builder)
	{
		foreach (GenericParameterHandle genericParameter in def.GetGenericParameters())
		{
			if (!GeneratedNames.TryParseAnonymousTypeParameterName(reader.GetString(reader.GetGenericParameter(genericParameter).Name), out var propertyName))
			{
				return false;
			}
			builder.Add(new AnonymousTypeKeyField(propertyName, isKey: false, ignoreCase: false));
		}
		return true;
	}

	public IReadOnlyDictionary<AnonymousTypeKey, AnonymousTypeValue> GetAnonymousTypeMap()
	{
		return Compilation.AnonymousTypeManager.GetAnonymousTypeMap();
	}

	internal override VariableSlotAllocator TryCreateVariableSlotAllocator(MethodSymbol method, MethodSymbol topLevelMethod, DiagnosticBag diagnostics)
	{
		return _previousDefinitions.TryCreateVariableSlotAllocator(_previousGeneration, Compilation, method, topLevelMethod, diagnostics);
	}

	internal override ImmutableArray<AnonymousTypeKey> GetPreviousAnonymousTypes()
	{
		return ImmutableArray.CreateRange(_previousGeneration.AnonymousTypeMap.Keys);
	}

	internal override int GetNextAnonymousTypeIndex()
	{
		return _previousGeneration.GetNextAnonymousTypeIndex();
	}

	internal override bool TryGetAnonymousTypeName(NamedTypeSymbol template, out string name, out int index)
	{
		return _previousDefinitions.TryGetAnonymousTypeName(template, out name, out index);
	}

	internal override IEnumerable<INamespaceTypeDefinition> GetTopLevelTypesCore(EmitContext context)
	{
		return _changes.GetTopLevelTypes(context);
	}

	public void OnCreatedIndices(DiagnosticBag diagnostics)
	{
		EmbeddedTypesManager embeddedTypesManagerOpt = EmbeddedTypesManagerOpt;
		if (embeddedTypesManagerOpt == null)
		{
			return;
		}
		foreach (NamedTypeSymbol key in embeddedTypesManagerOpt.EmbeddedTypesMap.Keys)
		{
			diagnostics.Add(new CSDiagnosticInfo(ErrorCode.ERR_EncNoPIAReference, key), Location.None);
		}
	}
}
