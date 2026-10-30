using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Emit.NoPia;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Emit;

internal abstract class CommonPEModuleBuilder : IUnit, IUnitReference, IReference, INamedEntity, IDefinition, IModuleReference
{
	internal readonly DebugDocumentsBuilder DebugDocumentsBuilder;

	internal readonly IEnumerable<ResourceDescription> ManifestResources;

	internal readonly ModulePropertiesForSerialization SerializationProperties;

	internal readonly OutputKind OutputKind;

	internal IEnumerable<IWin32Resource> Win32Resources;

	internal ResourceSection Win32ResourceSection;

	internal Stream SourceLinkStreamOpt;

	internal IMethodReference PEEntryPoint;

	internal IMethodReference DebugEntryPoint;

	private readonly ConcurrentDictionary<IMethodSymbol, IMethodBody> _methodBodyMap;

	private readonly TokenMap<IReference> _referencesInILMap = new TokenMap<IReference>();

	private readonly ItemTokenMap<string> _stringsInILMap = new ItemTokenMap<string>();

	private readonly ItemTokenMap<DebugSourceDocument> _sourceDocumentsInILMap = new ItemTokenMap<DebugSourceDocument>();

	private ImmutableArray<AssemblyReferenceAlias> _lazyAssemblyReferenceAliases;

	private ImmutableArray<ManagedResource> _lazyManagedResources;

	private IEnumerable<EmbeddedText> _embeddedTexts = SpecializedCollections.EmptyEnumerable<EmbeddedText>();

	internal readonly DebugInformationFormat DebugInformationFormat;

	internal readonly HashAlgorithmName PdbChecksumAlgorithm;

	internal ConcurrentDictionary<IMethodSymbol, CompilationTestData.MethodData> TestData { get; private set; }

	public abstract int CurrentGenerationOrdinal { get; }

	public abstract string Name { get; }

	internal abstract string ModuleName { get; }

	internal abstract bool SupportsPrivateImplClass { get; }

	internal abstract Compilation CommonCompilation { get; }

	internal abstract IModuleSymbol CommonSourceModule { get; }

	internal abstract IAssemblySymbol CommonCorLibrary { get; }

	internal abstract CommonModuleCompilationState CommonModuleCompilationState { get; }

	internal abstract CommonEmbeddedTypesManager CommonEmbeddedTypesManagerOpt { get; }

	public abstract bool GenerateVisualBasicStylePdb { get; }

	public abstract IEnumerable<string> LinkedAssembliesDebugInfo { get; }

	public abstract string DefaultNamespace { get; }

	public int DebugDocumentCount => DebugDocumentsBuilder.DebugDocumentCount;

	public abstract ISourceAssemblySymbolInternal SourceAssemblyOpt { get; }

	public int HintNumberOfMethodDefinitions => _methodBodyMap.Count;

	public IEnumerable<EmbeddedText> EmbeddedTexts
	{
		get
		{
			return _embeddedTexts;
		}
		set
		{
			_embeddedTexts = value;
		}
	}

	internal bool SaveTestData => TestData != null;

	public CommonPEModuleBuilder(IEnumerable<ResourceDescription> manifestResources, EmitOptions emitOptions, OutputKind outputKind, ModulePropertiesForSerialization serializationProperties, Compilation compilation)
	{
		ManifestResources = manifestResources;
		DebugDocumentsBuilder = new DebugDocumentsBuilder(compilation.Options.SourceReferenceResolver, compilation.IsCaseSensitive);
		OutputKind = outputKind;
		SerializationProperties = serializationProperties;
		_methodBodyMap = new ConcurrentDictionary<IMethodSymbol, IMethodBody>(Roslyn.Utilities.ReferenceEqualityComparer.Instance);
		DebugInformationFormat = emitOptions.DebugInformationFormat;
		PdbChecksumAlgorithm = emitOptions.PdbChecksumAlgorithm;
	}

	internal abstract IAssemblyReference Translate(IAssemblySymbol symbol, DiagnosticBag diagnostics);

	internal abstract ITypeReference Translate(ITypeSymbol symbol, SyntaxNode syntaxOpt, DiagnosticBag diagnostics);

	internal abstract IMethodReference Translate(IMethodSymbol symbol, DiagnosticBag diagnostics, bool needDeclaration);

	internal abstract ImmutableArray<INamespaceTypeDefinition> GetAnonymousTypes(EmitContext context);

	internal abstract void CompilationFinished();

	internal abstract ImmutableDictionary<ITypeDefinition, ImmutableArray<ITypeDefinitionMember>> GetSynthesizedMembers();

	internal abstract ITypeReference EncTranslateType(ITypeSymbol type, DiagnosticBag diagnostics);

	public abstract IEnumerable<ICustomAttribute> GetSourceAssemblyAttributes(bool isRefAssembly);

	public abstract IEnumerable<SecurityAttribute> GetSourceAssemblySecurityAttributes();

	public abstract IEnumerable<ICustomAttribute> GetSourceModuleAttributes();

	internal abstract ICustomAttribute SynthesizeAttribute(WellKnownMember attributeConstructor);

	public abstract ImmutableArray<ExportedType> GetExportedTypes(DiagnosticBag diagnostics);

	public abstract ImmutableArray<UsedNamespaceOrType> GetImports();

	protected abstract IAssemblyReference GetCorLibraryReferenceToEmit(EmitContext context);

	protected abstract IEnumerable<IAssemblyReference> GetAssemblyReferencesFromAddedModules(DiagnosticBag diagnostics);

	protected abstract void AddEmbeddedResourcesFromAddedModules(ArrayBuilder<ManagedResource> builder, DiagnosticBag diagnostics);

	public abstract ITypeReference GetPlatformType(PlatformType platformType, EmitContext context);

	public abstract bool IsPlatformType(ITypeReference typeRef, PlatformType platformType);

	public abstract IEnumerable<INamespaceTypeDefinition> GetTopLevelTypes(EmitContext context);

	public abstract IEnumerable<IFileReference> GetFiles(EmitContext context);

	public abstract MultiDictionary<DebugSourceDocument, DefinitionWithLocation> GetSymbolToLocationMap();

	public void Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}

	IEnumerable<ICustomAttribute> IReference.GetAttributes(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<ICustomAttribute>();
	}

	IDefinition IReference.AsDefinition(EmitContext context)
	{
		return this;
	}

	internal IMethodBody GetMethodBody(IMethodSymbol methodSymbol)
	{
		if (_methodBodyMap.TryGetValue(methodSymbol, out var value))
		{
			return value;
		}
		return null;
	}

	public void SetMethodBody(IMethodSymbol methodSymbol, IMethodBody body)
	{
		_methodBodyMap.Add(methodSymbol, body);
	}

	internal void SetPEEntryPoint(IMethodSymbol method, DiagnosticBag diagnostics)
	{
		PEEntryPoint = Translate(method, diagnostics, needDeclaration: true);
	}

	internal void SetDebugEntryPoint(IMethodSymbol method, DiagnosticBag diagnostics)
	{
		DebugEntryPoint = Translate(method, diagnostics, needDeclaration: true);
	}

	private bool IsSourceDefinition(IMethodSymbol method)
	{
		if (method.ContainingModule == CommonSourceModule)
		{
			return method.IsDefinition;
		}
		return false;
	}

	public IAssemblyReference GetCorLibrary(EmitContext context)
	{
		return Translate(CommonCorLibrary, context.Diagnostics);
	}

	public IAssemblyReference GetContainingAssembly(EmitContext context)
	{
		if (OutputKind != OutputKind.NetModule)
		{
			return (IAssemblyReference)this;
		}
		return null;
	}

	public IEnumerable<string> GetStrings()
	{
		return _stringsInILMap.GetAllItems();
	}

	public uint GetFakeSymbolTokenForIL(IReference symbol, SyntaxNode syntaxNode, DiagnosticBag diagnostics)
	{
		uint orAddTokenFor = _referencesInILMap.GetOrAddTokenFor(symbol, out var referenceAdded);
		if (referenceAdded)
		{
			ReferenceDependencyWalker.VisitReference(symbol, new EmitContext(this, syntaxNode, diagnostics, metadataOnly: false, includePrivateMembers: true));
		}
		return orAddTokenFor;
	}

	public uint GetSourceDocumentIndexForIL(DebugSourceDocument document)
	{
		return _sourceDocumentsInILMap.GetOrAddTokenFor(document);
	}

	internal DebugSourceDocument GetSourceDocumentFromIndex(uint token)
	{
		return _sourceDocumentsInILMap.GetItem(token);
	}

	public IReference GetReferenceFromToken(uint token)
	{
		return _referencesInILMap.GetItem(token);
	}

	public uint GetFakeStringTokenForIL(string str)
	{
		return _stringsInILMap.GetOrAddTokenFor(str);
	}

	public string GetStringFromToken(uint token)
	{
		return _stringsInILMap.GetItem(token);
	}

	public IEnumerable<IReference> ReferencesInIL(out int count)
	{
		return _referencesInILMap.GetAllItemsAndCount(out count);
	}

	public ImmutableArray<AssemblyReferenceAlias> GetAssemblyReferenceAliases(EmitContext context)
	{
		if (_lazyAssemblyReferenceAliases.IsDefault)
		{
			ImmutableInterlocked.InterlockedCompareExchange(ref _lazyAssemblyReferenceAliases, CalculateAssemblyReferenceAliases(context), default);
		}
		return _lazyAssemblyReferenceAliases;
	}

	private ImmutableArray<AssemblyReferenceAlias> CalculateAssemblyReferenceAliases(EmitContext context)
	{
		ArrayBuilder<AssemblyReferenceAlias> instance = ArrayBuilder<AssemblyReferenceAlias>.GetInstance();
		foreach (var referencedAssemblyAlias in CommonCompilation.GetBoundReferenceManager().GetReferencedAssemblyAliases())
		{
			IAssemblySymbol item = referencedAssemblyAlias.Item1;
			ImmutableArray<string> item2 = referencedAssemblyAlias.Item2;
			for (int i = 0; i < item2.Length; i++)
			{
				string text = item2[i];
				if (text != MetadataReferenceProperties.GlobalAlias && item2.IndexOf(text, 0, i) < 0)
				{
					instance.Add(new AssemblyReferenceAlias(text, Translate(item, context.Diagnostics)));
				}
			}
		}
		return instance.ToImmutableAndFree();
	}

	public IEnumerable<IAssemblyReference> GetAssemblyReferences(EmitContext context)
	{
		IAssemblyReference corLibraryReferenceToEmit = GetCorLibraryReferenceToEmit(context);
		if (corLibraryReferenceToEmit != null)
		{
			yield return corLibraryReferenceToEmit;
		}
		if (OutputKind == OutputKind.NetModule)
		{
			yield break;
		}
		foreach (IAssemblyReference assemblyReferencesFromAddedModule in GetAssemblyReferencesFromAddedModules(context.Diagnostics))
		{
			yield return assemblyReferencesFromAddedModule;
		}
	}

	public ImmutableArray<ManagedResource> GetResources(EmitContext context)
	{
		if (_lazyManagedResources.IsDefault)
		{
			ArrayBuilder<ManagedResource> instance = ArrayBuilder<ManagedResource>.GetInstance();
			foreach (ResourceDescription manifestResource in ManifestResources)
			{
				instance.Add(manifestResource.ToManagedResource(this));
			}
			if (OutputKind != OutputKind.NetModule)
			{
				AddEmbeddedResourcesFromAddedModules(instance, context.Diagnostics);
			}
			_lazyManagedResources = instance.ToImmutableAndFree();
		}
		return _lazyManagedResources;
	}

	internal void SetMethodTestData(IMethodSymbol method, ILBuilder builder)
	{
		TestData.Add(method, new CompilationTestData.MethodData(builder, method));
	}

	internal void SetMethodTestData(ConcurrentDictionary<IMethodSymbol, CompilationTestData.MethodData> methods)
	{
		TestData = methods;
	}
}
