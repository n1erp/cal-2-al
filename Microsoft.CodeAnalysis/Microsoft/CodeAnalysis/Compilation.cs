using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;
using Microsoft.DiaSymReader;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public abstract class Compilation
{
	internal abstract class EmitStreamProvider
	{
		public abstract Stream Stream { get; }

		public abstract Stream CreateStream(DiagnosticBag diagnostics);

		public Stream GetOrCreateStream(DiagnosticBag diagnostics)
		{
			return Stream ?? CreateStream(diagnostics);
		}
	}

	internal sealed class SimpleEmitStreamProvider : EmitStreamProvider
	{
		private readonly Stream _stream;

		public override Stream Stream => _stream;

		internal SimpleEmitStreamProvider(Stream stream)
		{
			_stream = stream;
		}

		public override Stream CreateStream(DiagnosticBag diagnostics)
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	internal enum Win32ResourceForm : byte
	{
		UNKNOWN,
		COFF,
		RES
	}

	private SmallDictionary<int, bool> _lazyMakeWellKnownTypeMissingMap;

	private SmallDictionary<int, bool> _lazyMakeMemberMissingMap;

	private readonly IReadOnlyDictionary<string, string> _features;

	internal const string UnspecifiedModuleAssemblyName = "?";

	private int _lazySubmissionSlotIndex;

	private const int SubmissionSlotIndexNotApplicable = -3;

	private const int SubmissionSlotIndexToBeAllocated = -2;

	internal readonly AsyncQueue<CompilationEvent> EventQueue;

	internal const CompilationStage DefaultDiagnosticsStage = CompilationStage.Compile;

	private ConcurrentDictionary<SyntaxTree, SmallConcurrentSetOfInts> _lazyTreeToUsedImportDirectivesMap;

	private static readonly Func<SyntaxTree, SmallConcurrentSetOfInts> s_createSetCallback = (SyntaxTree t) => new SmallConcurrentSetOfInts();

	private readonly WeakList<IAssemblySymbol> _retargetingAssemblySymbols = new WeakList<IAssemblySymbol>();

	public abstract bool IsCaseSensitive { get; }

	public ScriptCompilationInfo ScriptCompilationInfo => CommonScriptCompilationInfo;

	internal abstract ScriptCompilationInfo CommonScriptCompilationInfo { get; }

	public abstract string Language { get; }

	public string AssemblyName { get; }

	public CompilationOptions Options => CommonOptions;

	protected abstract CompilationOptions CommonOptions { get; }

	internal bool IsSubmission => _lazySubmissionSlotIndex != -3;

	internal Type SubmissionReturnType => ScriptCompilationInfo?.ReturnTypeOpt;

	internal Type HostObjectType => ScriptCompilationInfo?.GlobalsType;

	public IEnumerable<SyntaxTree> SyntaxTrees => CommonSyntaxTrees;

	protected abstract IEnumerable<SyntaxTree> CommonSyntaxTrees { get; }

	public ImmutableArray<MetadataReference> ExternalReferences { get; }

	public abstract ImmutableArray<MetadataReference> DirectiveReferences { get; }

	internal abstract IEnumerable<ReferenceDirective> ReferenceDirectives { get; }

	internal abstract IDictionary<(string path, string content), MetadataReference> ReferenceDirectiveMap { get; }

	public IEnumerable<MetadataReference> References
	{
		get
		{
			foreach (MetadataReference externalReference in ExternalReferences)
			{
				yield return externalReference;
			}
			foreach (MetadataReference directiveReference in DirectiveReferences)
			{
				yield return directiveReference;
			}
		}
	}

	public abstract IEnumerable<AssemblyIdentity> ReferencedAssemblyNames { get; }

	public IAssemblySymbol Assembly => CommonAssembly;

	protected abstract IAssemblySymbol CommonAssembly { get; }

	public IModuleSymbol SourceModule => CommonSourceModule;

	protected abstract IModuleSymbol CommonSourceModule { get; }

	public INamespaceSymbol GlobalNamespace => CommonGlobalNamespace;

	protected abstract INamespaceSymbol CommonGlobalNamespace { get; }

	internal abstract CommonAnonymousTypeManager CommonAnonymousTypeManager { get; }

	public INamedTypeSymbol ObjectType => CommonObjectType;

	protected abstract INamedTypeSymbol CommonObjectType { get; }

	public ITypeSymbol DynamicType => CommonDynamicType;

	protected abstract ITypeSymbol CommonDynamicType { get; }

	public INamedTypeSymbol ScriptClass => CommonScriptClass;

	protected abstract INamedTypeSymbol CommonScriptClass { get; }

	internal abstract CommonMessageProvider MessageProvider { get; }

	internal abstract byte LinkerMajorVersion { get; }

	internal bool HasStrongName
	{
		get
		{
			if (!IsDelaySigned && Options.OutputKind != OutputKind.NetModule)
			{
				return StrongNameKeys.CanProvideStrongName;
			}
			return false;
		}
	}

	internal bool IsRealSigned
	{
		get
		{
			if (!IsDelaySigned && !Options.PublicSign && Options.OutputKind != OutputKind.NetModule)
			{
				return StrongNameKeys.CanSign;
			}
			return false;
		}
	}

	internal abstract bool IsDelaySigned { get; }

	internal abstract StrongNameKeys StrongNameKeys { get; }

	internal abstract Guid DebugSourceDocumentLanguageId { get; }

	internal bool IsEmitDeterministic => Options.Deterministic;

	private ConcurrentDictionary<SyntaxTree, SmallConcurrentSetOfInts> TreeToUsedImportDirectivesMap => LazyInitializer.EnsureInitialized(ref _lazyTreeToUsedImportDirectivesMap);

	internal WeakList<IAssemblySymbol> RetargetingAssemblySymbols => _retargetingAssemblySymbols;

	internal Compilation(string name, ImmutableArray<MetadataReference> references, IReadOnlyDictionary<string, string> features, bool isSubmission, AsyncQueue<CompilationEvent> eventQueue)
	{
		AssemblyName = name;
		ExternalReferences = references;
		EventQueue = eventQueue;
		_lazySubmissionSlotIndex = (isSubmission ? (-2) : (-3));
		_features = features;
	}

	protected static IReadOnlyDictionary<string, string> SyntaxTreeCommonFeatures(IEnumerable<SyntaxTree> trees)
	{
		IReadOnlyDictionary<string, string> readOnlyDictionary = null;
		foreach (SyntaxTree tree in trees)
		{
			IReadOnlyDictionary<string, string> features = tree.Options.Features;
			if (readOnlyDictionary == null)
			{
				readOnlyDictionary = features;
			}
			else if (readOnlyDictionary != features && !readOnlyDictionary.SetEquals(features))
			{
				throw new ArgumentException(CodeAnalysisResources.InconsistentSyntaxTreeFeature, "trees");
			}
		}
		if (readOnlyDictionary == null)
		{
			readOnlyDictionary = ImmutableDictionary<string, string>.Empty;
		}
		return readOnlyDictionary;
	}

	internal abstract AnalyzerDriver AnalyzerForLanguage(ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerManager analyzerManager);

	internal static void ValidateScriptCompilationParameters(Compilation previousScriptCompilation, Type returnType, ref Type globalsType)
	{
		if ((object)globalsType != null && !IsValidHostObjectType(globalsType))
		{
			throw new ArgumentException(CodeAnalysisResources.ReturnTypeCannotBeValuePointerbyRefOrOpen, "globalsType");
		}
		if ((object)returnType != null && !IsValidSubmissionReturnType(returnType))
		{
			throw new ArgumentException(CodeAnalysisResources.ReturnTypeCannotBeVoidByRefOrOpen, "returnType");
		}
		if (previousScriptCompilation != null)
		{
			if ((object)globalsType == null)
			{
				globalsType = previousScriptCompilation.HostObjectType;
			}
			else if ((object)globalsType != previousScriptCompilation.HostObjectType)
			{
				throw new ArgumentException(CodeAnalysisResources.TypeMustBeSameAsHostObjectTypeOfPreviousSubmission, "globalsType");
			}
			if (previousScriptCompilation.GetDiagnostics().Any((Diagnostic d) => d.Severity == DiagnosticSeverity.Error))
			{
				throw new InvalidOperationException(CodeAnalysisResources.PreviousSubmissionHasErrors);
			}
		}
	}

	internal static void CheckSubmissionOptions(CompilationOptions options)
	{
		if (!(options == null))
		{
			if (options.OutputKind.IsValid() && options.OutputKind != OutputKind.DynamicallyLinkedLibrary)
			{
				throw new ArgumentException(CodeAnalysisResources.InvalidOutputKindForSubmission, "options");
			}
			if (options.CryptoKeyContainer != null || options.CryptoKeyFile != null || options.DelaySign.HasValue || !options.CryptoPublicKey.IsEmpty || (options.DelaySign == true && options.PublicSign))
			{
				throw new ArgumentException(CodeAnalysisResources.InvalidCompilationOptions, "options");
			}
		}
	}

	public Compilation Clone()
	{
		return CommonClone();
	}

	protected abstract Compilation CommonClone();

	internal abstract Compilation WithEventQueue(AsyncQueue<CompilationEvent> eventQueue);

	public SemanticModel GetSemanticModel(SyntaxTree syntaxTree, bool ignoreAccessibility = false)
	{
		return CommonGetSemanticModel(syntaxTree, ignoreAccessibility);
	}

	protected abstract SemanticModel CommonGetSemanticModel(SyntaxTree syntaxTree, bool ignoreAccessibility);

	public INamedTypeSymbol CreateErrorTypeSymbol(INamespaceOrTypeSymbol container, string name, int arity)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (arity < 0)
		{
			throw new ArgumentException("arity must be >= 0", "arity");
		}
		return CommonCreateErrorTypeSymbol(container, name, arity);
	}

	protected abstract INamedTypeSymbol CommonCreateErrorTypeSymbol(INamespaceOrTypeSymbol container, string name, int arity);

	public INamespaceSymbol CreateErrorNamespaceSymbol(INamespaceSymbol container, string name)
	{
		if (container == null)
		{
			throw new ArgumentNullException("container");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return CommonCreateErrorNamespaceSymbol(container, name);
	}

	protected abstract INamespaceSymbol CommonCreateErrorNamespaceSymbol(INamespaceSymbol container, string name);

	internal void CheckAssemblyName(DiagnosticBag diagnostics)
	{
		if (AssemblyName != null)
		{
			MetadataHelpers.CheckAssemblyOrModuleName(AssemblyName, MessageProvider, MessageProvider.ERR_BadAssemblyName, diagnostics);
		}
	}

	internal string MakeSourceAssemblySimpleName()
	{
		return AssemblyName ?? "?";
	}

	internal string MakeSourceModuleName()
	{
		string text = Options.ModuleName;
		if (text == null)
		{
			if (AssemblyName == null)
			{
				return "?";
			}
			text = AssemblyName + Options.OutputKind.GetDefaultExtension();
		}
		return text;
	}

	public Compilation WithAssemblyName(string assemblyName)
	{
		return CommonWithAssemblyName(assemblyName);
	}

	protected abstract Compilation CommonWithAssemblyName(string outputName);

	public Compilation WithOptions(CompilationOptions options)
	{
		return CommonWithOptions(options);
	}

	protected abstract Compilation CommonWithOptions(CompilationOptions options);

	internal int GetSubmissionSlotIndex()
	{
		if (_lazySubmissionSlotIndex == -2)
		{
			int num = ScriptCompilationInfo.PreviousScriptCompilation?.GetSubmissionSlotIndex() ?? 0;
			_lazySubmissionSlotIndex = (HasCodeToEmit() ? (num + 1) : num);
		}
		return _lazySubmissionSlotIndex;
	}

	internal static bool IsValidSubmissionReturnType(Type type)
	{
		if ((object)type != typeof(void) && !type.IsByRef)
		{
			return !type.GetTypeInfo().ContainsGenericParameters;
		}
		return false;
	}

	internal static bool IsValidHostObjectType(Type type)
	{
		System.Reflection.TypeInfo typeInfo = type.GetTypeInfo();
		if (!typeInfo.IsValueType && !typeInfo.IsPointer && !typeInfo.IsByRef)
		{
			return !typeInfo.ContainsGenericParameters;
		}
		return false;
	}

	internal abstract bool HasSubmissionResult();

	public Compilation WithScriptCompilationInfo(ScriptCompilationInfo info)
	{
		return CommonWithScriptCompilationInfo(info);
	}

	protected abstract Compilation CommonWithScriptCompilationInfo(ScriptCompilationInfo info);

	public Compilation AddSyntaxTrees(params SyntaxTree[] trees)
	{
		return CommonAddSyntaxTrees(trees);
	}

	public Compilation AddSyntaxTrees(IEnumerable<SyntaxTree> trees)
	{
		return CommonAddSyntaxTrees(trees);
	}

	protected abstract Compilation CommonAddSyntaxTrees(IEnumerable<SyntaxTree> trees);

	public Compilation RemoveSyntaxTrees(params SyntaxTree[] trees)
	{
		return CommonRemoveSyntaxTrees(trees);
	}

	public Compilation RemoveSyntaxTrees(IEnumerable<SyntaxTree> trees)
	{
		return CommonRemoveSyntaxTrees(trees);
	}

	protected abstract Compilation CommonRemoveSyntaxTrees(IEnumerable<SyntaxTree> trees);

	public Compilation RemoveAllSyntaxTrees()
	{
		return CommonRemoveAllSyntaxTrees();
	}

	protected abstract Compilation CommonRemoveAllSyntaxTrees();

	public Compilation ReplaceSyntaxTree(SyntaxTree oldTree, SyntaxTree newTree)
	{
		return CommonReplaceSyntaxTree(oldTree, newTree);
	}

	protected abstract Compilation CommonReplaceSyntaxTree(SyntaxTree oldTree, SyntaxTree newTree);

	public bool ContainsSyntaxTree(SyntaxTree syntaxTree)
	{
		return CommonContainsSyntaxTree(syntaxTree);
	}

	protected abstract bool CommonContainsSyntaxTree(SyntaxTree syntaxTree);

	internal static ImmutableArray<MetadataReference> ValidateReferences<T>(IEnumerable<MetadataReference> references) where T : CompilationReference
	{
		ImmutableArray<MetadataReference> result = references.AsImmutableOrEmpty();
		for (int i = 0; i < result.Length; i++)
		{
			MetadataReference metadataReference = result[i];
			if (metadataReference == null)
			{
				throw new ArgumentNullException(string.Format("{0}[{1}]", "references", i));
			}
			if (!(metadataReference is PortableExecutableReference) && !(metadataReference is T))
			{
				throw new ArgumentException(string.Format(CodeAnalysisResources.ReferenceOfTypeIsInvalid1, metadataReference.GetType()), string.Format("{0}[{1}]", "references", i));
			}
		}
		return result;
	}

	internal CommonReferenceManager GetBoundReferenceManager()
	{
		return CommonGetBoundReferenceManager();
	}

	internal abstract CommonReferenceManager CommonGetBoundReferenceManager();

	public abstract CompilationReference ToMetadataReference(ImmutableArray<string> aliases = default(ImmutableArray<string>), bool embedInteropTypes = false);

	public Compilation WithReferences(IEnumerable<MetadataReference> newReferences)
	{
		return CommonWithReferences(newReferences);
	}

	public Compilation WithReferences(params MetadataReference[] newReferences)
	{
		return WithReferences((IEnumerable<MetadataReference>)newReferences);
	}

	protected abstract Compilation CommonWithReferences(IEnumerable<MetadataReference> newReferences);

	public Compilation AddReferences(params MetadataReference[] references)
	{
		return AddReferences((IEnumerable<MetadataReference>)references);
	}

	public Compilation AddReferences(IEnumerable<MetadataReference> references)
	{
		if (references == null)
		{
			throw new ArgumentNullException("references");
		}
		if (references.IsEmpty())
		{
			return this;
		}
		return CommonWithReferences(ExternalReferences.Union(references));
	}

	public Compilation RemoveReferences(params MetadataReference[] references)
	{
		return RemoveReferences((IEnumerable<MetadataReference>)references);
	}

	public Compilation RemoveReferences(IEnumerable<MetadataReference> references)
	{
		if (references == null)
		{
			throw new ArgumentNullException("references");
		}
		if (references.IsEmpty())
		{
			return this;
		}
		HashSet<MetadataReference> hashSet = new HashSet<MetadataReference>(ExternalReferences);
		foreach (MetadataReference item in references.Distinct())
		{
			if (!hashSet.Remove(item))
			{
				throw new ArgumentException(string.Format(CodeAnalysisResources.MetadataRefNotFoundToRemove1, item), "references");
			}
		}
		return CommonWithReferences(hashSet);
	}

	public Compilation RemoveAllReferences()
	{
		return CommonWithReferences(SpecializedCollections.EmptyEnumerable<MetadataReference>());
	}

	public Compilation ReplaceReference(MetadataReference oldReference, MetadataReference newReference)
	{
		if (oldReference == null)
		{
			throw new ArgumentNullException("oldReference");
		}
		if (newReference == null)
		{
			return RemoveReferences(oldReference);
		}
		return RemoveReferences(oldReference).AddReferences(newReference);
	}

	public ISymbol GetAssemblyOrModuleSymbol(MetadataReference reference)
	{
		return CommonGetAssemblyOrModuleSymbol(reference);
	}

	protected abstract ISymbol CommonGetAssemblyOrModuleSymbol(MetadataReference reference);

	public MetadataReference GetMetadataReference(IAssemblySymbol assemblySymbol)
	{
		return GetBoundReferenceManager().GetMetadataReference(assemblySymbol);
	}

	public INamespaceSymbol GetCompilationNamespace(INamespaceSymbol namespaceSymbol)
	{
		return CommonGetCompilationNamespace(namespaceSymbol);
	}

	protected abstract INamespaceSymbol CommonGetCompilationNamespace(INamespaceSymbol namespaceSymbol);

	public IMethodSymbol GetEntryPoint(CancellationToken cancellationToken)
	{
		return CommonGetEntryPoint(cancellationToken);
	}

	protected abstract IMethodSymbol CommonGetEntryPoint(CancellationToken cancellationToken);

	public INamedTypeSymbol GetSpecialType(SpecialType specialType)
	{
		return CommonGetSpecialType(specialType);
	}

	internal abstract ISymbol CommonGetSpecialTypeMember(SpecialMember specialMember);

	internal abstract bool IsSystemTypeReference(ITypeSymbol type);

	protected abstract INamedTypeSymbol CommonGetSpecialType(SpecialType specialType);

	internal abstract ISymbol CommonGetWellKnownTypeMember(WellKnownMember member);

	internal abstract bool IsAttributeType(ITypeSymbol type);

	protected INamedTypeSymbol CommonBindScriptClass()
	{
		string[] array = (Options.ScriptClassName ?? "").Split(new char[1] { '.' });
		INamespaceSymbol namespaceSymbol = SourceModule.GlobalNamespace;
		for (int i = 0; i < array.Length - 1; i++)
		{
			INamespaceSymbol nestedNamespace = namespaceSymbol.GetNestedNamespace(array[i]);
			if (nestedNamespace == null)
			{
				return null;
			}
			namespaceSymbol = nestedNamespace;
		}
		foreach (INamedTypeSymbol typeMember in namespaceSymbol.GetTypeMembers(array[^1]))
		{
			if (typeMember.IsScriptClass)
			{
				return typeMember;
			}
		}
		return null;
	}

	[Conditional("DEBUG")]
	private void AssertNoScriptTrees()
	{
		foreach (SyntaxTree syntaxTree in SyntaxTrees)
		{
			_ = syntaxTree;
		}
	}

	public IArrayTypeSymbol CreateArrayTypeSymbol(ITypeSymbol elementType, int rank = 1)
	{
		return CommonCreateArrayTypeSymbol(elementType, rank);
	}

	protected abstract IArrayTypeSymbol CommonCreateArrayTypeSymbol(ITypeSymbol elementType, int rank);

	public IPointerTypeSymbol CreatePointerTypeSymbol(ITypeSymbol pointedAtType)
	{
		return CommonCreatePointerTypeSymbol(pointedAtType);
	}

	protected abstract IPointerTypeSymbol CommonCreatePointerTypeSymbol(ITypeSymbol elementType);

	public INamedTypeSymbol GetTypeByMetadataName(string fullyQualifiedMetadataName)
	{
		return CommonGetTypeByMetadataName(fullyQualifiedMetadataName);
	}

	protected abstract INamedTypeSymbol CommonGetTypeByMetadataName(string metadataName);

	public INamedTypeSymbol CreateTupleTypeSymbol(ImmutableArray<ITypeSymbol> elementTypes, ImmutableArray<string> elementNames = default(ImmutableArray<string>), ImmutableArray<Location> elementLocations = default(ImmutableArray<Location>))
	{
		if (elementTypes.IsDefault)
		{
			throw new ArgumentNullException("elementTypes");
		}
		if (elementTypes.Length <= 1)
		{
			throw new ArgumentException(CodeAnalysisResources.TuplesNeedAtLeastTwoElements, "elementNames");
		}
		elementNames = CheckTupleElementNames(elementTypes.Length, elementNames);
		CheckTupleElementLocations(elementTypes.Length, elementLocations);
		int i = 0;
		for (int length = elementTypes.Length; i < length; i++)
		{
			if (elementTypes[i] == null)
			{
				throw new ArgumentNullException(string.Format("{0}[{1}]", "elementTypes", i));
			}
			if (!elementLocations.IsDefault && elementLocations[i] == null)
			{
				throw new ArgumentNullException(string.Format("{0}[{1}]", "elementLocations", i));
			}
		}
		return CommonCreateTupleTypeSymbol(elementTypes, elementNames, elementLocations);
	}

	protected static ImmutableArray<string> CheckTupleElementNames(int cardinality, ImmutableArray<string> elementNames)
	{
		if (!elementNames.IsDefault)
		{
			if (elementNames.Length != cardinality)
			{
				throw new ArgumentException(CodeAnalysisResources.TupleElementNameCountMismatch, "elementNames");
			}
			for (int i = 0; i < elementNames.Length; i++)
			{
				if (elementNames[i] == "")
				{
					throw new ArgumentException(CodeAnalysisResources.TupleElementNameEmpty, string.Format("{0}[{1}]", "elementNames", i));
				}
			}
			if (elementNames.All((string n) => n == null))
			{
				return default;
			}
		}
		return elementNames;
	}

	protected static void CheckTupleElementLocations(int cardinality, ImmutableArray<Location> elementLocations)
	{
		if (!elementLocations.IsDefault && elementLocations.Length != cardinality)
		{
			throw new ArgumentException(CodeAnalysisResources.TupleElementLocationCountMismatch, "elementLocations");
		}
	}

	protected abstract INamedTypeSymbol CommonCreateTupleTypeSymbol(ImmutableArray<ITypeSymbol> elementTypes, ImmutableArray<string> elementNames, ImmutableArray<Location> elementLocations);

	public INamedTypeSymbol CreateTupleTypeSymbol(INamedTypeSymbol underlyingType, ImmutableArray<string> elementNames = default(ImmutableArray<string>), ImmutableArray<Location> elementLocations = default(ImmutableArray<Location>))
	{
		if (underlyingType == null)
		{
			throw new ArgumentNullException("underlyingType");
		}
		return CommonCreateTupleTypeSymbol(underlyingType, elementNames, elementLocations);
	}

	protected abstract INamedTypeSymbol CommonCreateTupleTypeSymbol(INamedTypeSymbol underlyingType, ImmutableArray<string> elementNames, ImmutableArray<Location> elementLocations);

	public INamedTypeSymbol CreateAnonymousTypeSymbol(ImmutableArray<ITypeSymbol> memberTypes, ImmutableArray<string> memberNames, ImmutableArray<bool> memberIsReadOnly = default(ImmutableArray<bool>), ImmutableArray<Location> memberLocations = default(ImmutableArray<Location>))
	{
		if (memberTypes.IsDefault)
		{
			throw new ArgumentNullException("memberTypes");
		}
		if (memberNames.IsDefault)
		{
			throw new ArgumentNullException("memberNames");
		}
		if (memberTypes.Length != memberNames.Length)
		{
			throw new ArgumentException(string.Format(CodeAnalysisResources.AnonymousTypeMemberAndNamesCountMismatch2, "memberTypes", "memberNames"));
		}
		if (!memberLocations.IsDefault && memberLocations.Length != memberTypes.Length)
		{
			throw new ArgumentException(string.Format(CodeAnalysisResources.AnonymousTypeArgumentCountMismatch2, "memberLocations", "memberNames"));
		}
		if (!memberIsReadOnly.IsDefault && memberIsReadOnly.Length != memberTypes.Length)
		{
			throw new ArgumentException(string.Format(CodeAnalysisResources.AnonymousTypeArgumentCountMismatch2, "memberIsReadOnly", "memberNames"));
		}
		int i = 0;
		for (int length = memberTypes.Length; i < length; i++)
		{
			if (memberTypes[i] == null)
			{
				throw new ArgumentNullException(string.Format("{0}[{1}]", "memberTypes", i));
			}
			if (memberNames[i] == null)
			{
				throw new ArgumentNullException(string.Format("{0}[{1}]", "memberNames", i));
			}
			if (!memberLocations.IsDefault && memberLocations[i] == null)
			{
				throw new ArgumentNullException(string.Format("{0}[{1}]", "memberLocations", i));
			}
		}
		return CommonCreateAnonymousTypeSymbol(memberTypes, memberNames, memberLocations, memberIsReadOnly);
	}

	protected abstract INamedTypeSymbol CommonCreateAnonymousTypeSymbol(ImmutableArray<ITypeSymbol> memberTypes, ImmutableArray<string> memberNames, ImmutableArray<Location> memberLocations, ImmutableArray<bool> memberIsReadOnly);

	public abstract CommonConversion ClassifyCommonConversion(ITypeSymbol source, ITypeSymbol destination);

	public bool HasImplicitConversion(ITypeSymbol fromType, ITypeSymbol toType)
	{
		if (fromType != null && toType != null)
		{
			return ClassifyCommonConversion(fromType, toType).IsImplicit;
		}
		return false;
	}

	internal abstract IConvertibleConversion ClassifyConvertibleConversion(IOperation source, ITypeSymbol destination, out Optional<object> constantValue);

	public abstract ImmutableArray<Diagnostic> GetParseDiagnostics(CancellationToken cancellationToken = default(CancellationToken));

	public abstract ImmutableArray<Diagnostic> GetDeclarationDiagnostics(CancellationToken cancellationToken = default(CancellationToken));

	public abstract ImmutableArray<Diagnostic> GetMethodBodyDiagnostics(CancellationToken cancellationToken = default(CancellationToken));

	public abstract ImmutableArray<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = default(CancellationToken));

	internal abstract void GetDiagnostics(CompilationStage stage, bool includeEarlierStages, DiagnosticBag diagnostics, CancellationToken cancellationToken = default(CancellationToken));

	internal void EnsureCompilationEventQueueCompleted()
	{
		lock (EventQueue)
		{
			if (!EventQueue.IsCompleted)
			{
				CompleteCompilationEventQueue_NoLock();
			}
		}
	}

	internal void CompleteCompilationEventQueue_NoLock()
	{
		EventQueue.TryEnqueue(new CompilationCompletedEvent(this));
		EventQueue.PromiseNotToEnqueue();
		EventQueue.TryComplete();
	}

	internal bool FilterAndAppendAndFreeDiagnostics(DiagnosticBag accumulator, ref DiagnosticBag incoming)
	{
		bool result = FilterAndAppendDiagnostics(accumulator, incoming.AsEnumerableWithoutResolution(), null);
		incoming.Free();
		incoming = null;
		return result;
	}

	internal bool FilterAndAppendDiagnostics(DiagnosticBag accumulator, IEnumerable<Diagnostic> incoming, HashSet<int> exclude)
	{
		bool flag = false;
		bool reportSuppressedDiagnostics = Options.ReportSuppressedDiagnostics;
		foreach (Diagnostic item in incoming)
		{
			if (exclude != null && exclude.Contains(item.Code))
			{
				continue;
			}
			Diagnostic diagnostic = Options.FilterDiagnostic(item);
			if (diagnostic != null && (reportSuppressedDiagnostics || !diagnostic.IsSuppressed))
			{
				if (diagnostic.Severity == DiagnosticSeverity.Error)
				{
					flag = true;
				}
				accumulator.Add(diagnostic);
			}
		}
		return !flag;
	}

	public Stream CreateDefaultWin32Resources(bool versionResource, bool noManifest, Stream manifestContents, Stream iconInIcoFormat)
	{
		MemoryStream memoryStream = new MemoryStream(1024);
		AppendNullResource(memoryStream);
		if (versionResource)
		{
			AppendDefaultVersionResource(memoryStream);
		}
		if (!noManifest)
		{
			if (Options.OutputKind.IsApplication() && manifestContents == null)
			{
				manifestContents = typeof(Compilation).GetTypeInfo().Assembly.GetManifestResourceStream("Microsoft.CodeAnalysis.Resources.default.win32manifest");
			}
			if (manifestContents != null)
			{
				Win32ResourceConversions.AppendManifestToResourceStream(memoryStream, manifestContents, !Options.OutputKind.IsApplication());
			}
		}
		if (iconInIcoFormat != null)
		{
			Win32ResourceConversions.AppendIconToResourceStream(memoryStream, iconInIcoFormat);
		}
		memoryStream.Position = 0L;
		return memoryStream;
	}

	internal static void AppendNullResource(Stream resourceStream)
	{
		BinaryWriter binaryWriter = new BinaryWriter(resourceStream);
		binaryWriter.Write(0u);
		binaryWriter.Write(32u);
		binaryWriter.Write(ushort.MaxValue);
		binaryWriter.Write((ushort)0);
		binaryWriter.Write(ushort.MaxValue);
		binaryWriter.Write((ushort)0);
		binaryWriter.Write(0u);
		binaryWriter.Write((ushort)0);
		binaryWriter.Write((ushort)0);
		binaryWriter.Write(0u);
		binaryWriter.Write(0u);
	}

	protected abstract void AppendDefaultVersionResource(Stream resourceStream);

	internal static Win32ResourceForm DetectWin32ResourceForm(Stream win32Resources)
	{
		BinaryReader binaryReader = new BinaryReader(win32Resources, Encoding.Unicode);
		long position = win32Resources.Position;
		uint num = binaryReader.ReadUInt32();
		win32Resources.Position = position;
		if (num == 0)
		{
			return Win32ResourceForm.RES;
		}
		if ((num & 0xFFFF0000u) != 0 || (num & 0xFFFF) != 65535)
		{
			return Win32ResourceForm.COFF;
		}
		return Win32ResourceForm.UNKNOWN;
	}

	internal ResourceSection MakeWin32ResourcesFromCOFF(Stream win32Resources, DiagnosticBag diagnostics)
	{
		if (win32Resources == null)
		{
			return null;
		}
		try
		{
			return COFFResourceReader.ReadWin32ResourcesFromCOFF(win32Resources);
		}
		catch (BadImageFormatException ex)
		{
			diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_BadWin32Resource, Location.None, ex.Message));
			return null;
		}
		catch (IOException ex2)
		{
			diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_BadWin32Resource, Location.None, ex2.Message));
			return null;
		}
		catch (ResourceException ex3)
		{
			diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_BadWin32Resource, Location.None, ex3.Message));
			return null;
		}
	}

	internal List<Win32Resource> MakeWin32ResourceList(Stream win32Resources, DiagnosticBag diagnostics)
	{
		if (win32Resources == null)
		{
			return null;
		}
		List<RESOURCE> list;
		try
		{
			list = CvtResFile.ReadResFile(win32Resources);
		}
		catch (ResourceException ex)
		{
			diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_BadWin32Resource, Location.None, ex.Message));
			return null;
		}
		if (list == null)
		{
			return null;
		}
		List<Win32Resource> list2 = new List<Win32Resource>();
		foreach (RESOURCE item2 in list)
		{
			Win32Resource item = new Win32Resource(item2.data, 0u, item2.LanguageId, (short)item2.pstringName.Ordinal, item2.pstringName.theString, (short)item2.pstringType.Ordinal, item2.pstringType.theString);
			list2.Add(item);
		}
		return list2;
	}

	internal void SetupWin32Resources(CommonPEModuleBuilder moduleBeingBuilt, Stream win32Resources, DiagnosticBag diagnostics)
	{
		if (win32Resources != null)
		{
			Win32ResourceForm win32ResourceForm;
			try
			{
				win32ResourceForm = DetectWin32ResourceForm(win32Resources);
			}
			catch (EndOfStreamException)
			{
				diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_BadWin32Resource, NoLocation.Singleton, CodeAnalysisResources.UnrecognizedResourceFileFormat));
				return;
			}
			catch (Exception ex2)
			{
				diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_BadWin32Resource, NoLocation.Singleton, ex2.Message));
				return;
			}
			switch (win32ResourceForm)
			{
			case Win32ResourceForm.COFF:
				moduleBeingBuilt.Win32ResourceSection = MakeWin32ResourcesFromCOFF(win32Resources, diagnostics);
				break;
			case Win32ResourceForm.RES:
				moduleBeingBuilt.Win32Resources = MakeWin32ResourceList(win32Resources, diagnostics);
				break;
			default:
				diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_BadWin32Resource, NoLocation.Singleton, CodeAnalysisResources.UnrecognizedResourceFileFormat));
				break;
			}
		}
	}

	internal void ReportManifestResourceDuplicates(IEnumerable<ResourceDescription> manifestResources, IEnumerable<string> addedModuleNames, IEnumerable<string> addedModuleResourceNames, DiagnosticBag diagnostics)
	{
		if (Options.OutputKind == OutputKind.NetModule && (manifestResources == null || !manifestResources.Any()))
		{
			return;
		}
		HashSet<string> hashSet = new HashSet<string>();
		if (manifestResources != null && manifestResources.Any())
		{
			HashSet<string> hashSet2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (ResourceDescription manifestResource in manifestResources)
			{
				if (!hashSet.Add(manifestResource.ResourceName))
				{
					diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_ResourceNotUnique, Location.None, manifestResource.ResourceName));
				}
				string fileName = manifestResource.FileName;
				if (fileName != null && !hashSet2.Add(fileName))
				{
					diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_ResourceFileNameNotUnique, Location.None, fileName));
				}
			}
			foreach (string addedModuleName in addedModuleNames)
			{
				if (!hashSet2.Add(addedModuleName))
				{
					diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_ResourceFileNameNotUnique, Location.None, addedModuleName));
				}
			}
		}
		if (Options.OutputKind == OutputKind.NetModule)
		{
			return;
		}
		foreach (string addedModuleResourceName in addedModuleResourceNames)
		{
			if (!hashSet.Add(addedModuleResourceName))
			{
				diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_ResourceNotUnique, Location.None, addedModuleResourceName));
			}
		}
	}

	internal ModulePropertiesForSerialization ConstructModuleSerializationProperties(EmitOptions emitOptions, string targetRuntimeVersion, Guid moduleVersionId = default(Guid))
	{
		CompilationOptions options = Options;
		Platform platform = options.Platform;
		OutputKind outputKind = options.OutputKind;
		if (!platform.IsValid())
		{
			platform = Platform.AnyCpu;
		}
		if (!outputKind.IsValid())
		{
			outputKind = OutputKind.DynamicallyLinkedLibrary;
		}
		bool flag = platform.Requires64Bit();
		bool requires32Bit = platform.Requires32Bit();
		ushort fileAlignment;
		if (emitOptions.FileAlignment != 0 && CompilationOptions.IsValidFileAlignment(emitOptions.FileAlignment))
		{
			fileAlignment = (ushort)emitOptions.FileAlignment;
		}
		else
		{
			fileAlignment = (ushort)(flag ? 512 : 512);
		}
		ulong num = (emitOptions.BaseAddress + 32768) & (ulong)(flag ? (-65536L) : 4294901760L);
		if (num == 0L)
		{
			if (outputKind != OutputKind.ConsoleApplication && outputKind != OutputKind.WindowsApplication && outputKind != OutputKind.WindowsRuntimeApplication)
			{
				num = (flag ? 6442450944uL : 268435456);
			}
			else
			{
				num = (flag ? 5368709120uL : 4194304);
			}
		}
		ulong sizeOfHeapCommit = (ulong)(flag ? 8192 : 4096);
		ulong sizeOfStackReserve = (ulong)(flag ? 4194304 : 1048576);
		ulong sizeOfStackCommit = (ulong)(flag ? 16384 : 4096);
		SubsystemVersion subsystemVersion = ((!emitOptions.SubsystemVersion.Equals(SubsystemVersion.None) && emitOptions.SubsystemVersion.IsValid) ? emitOptions.SubsystemVersion : SubsystemVersion.Default(outputKind, platform));
		Machine machine;
		switch (platform)
		{
		case Platform.Arm64:
			machine = Machine.Arm64;
			break;
		case Platform.Arm:
			machine = Machine.ArmThumb2;
			break;
		case Platform.X64:
			machine = Machine.Amd64;
			break;
		case Platform.Itanium:
			machine = Machine.IA64;
			break;
		case Platform.X86:
			machine = Machine.I386;
			break;
		case Platform.AnyCpu:
		case Platform.AnyCpu32BitPreferred:
			machine = Machine.Unknown;
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(platform);
		}
		return new ModulePropertiesForSerialization(moduleVersionId, GetCorHeaderFlags(machine, HasStrongName, platform == Platform.AnyCpu32BitPreferred), fileAlignment, 8192, targetRuntimeVersion, machine, num, 1048576uL, sizeOfHeapCommit, sizeOfStackReserve, sizeOfStackCommit, GetDllCharacteristics(emitOptions.HighEntropyVirtualAddressSpace, options.OutputKind == OutputKind.WindowsRuntimeApplication), GetCharacteristics(outputKind, requires32Bit), GetSubsystem(outputKind), (ushort)subsystemVersion.Major, (ushort)subsystemVersion.Minor, LinkerMajorVersion, 0);
	}

	private static CorFlags GetCorHeaderFlags(Machine machine, bool strongNameSigned, bool prefers32Bit)
	{
		CorFlags corFlags = CorFlags.ILOnly;
		if (machine == Machine.I386)
		{
			corFlags |= CorFlags.Requires32Bit;
		}
		if (strongNameSigned)
		{
			corFlags |= CorFlags.StrongNameSigned;
		}
		if (prefers32Bit)
		{
			corFlags |= CorFlags.Requires32Bit | CorFlags.Prefers32Bit;
		}
		return corFlags;
	}

	internal static DllCharacteristics GetDllCharacteristics(bool enableHighEntropyVA, bool configureToExecuteInAppContainer)
	{
		DllCharacteristics dllCharacteristics = DllCharacteristics.DynamicBase | DllCharacteristics.NxCompatible | DllCharacteristics.NoSeh | DllCharacteristics.TerminalServerAware;
		if (enableHighEntropyVA)
		{
			dllCharacteristics |= DllCharacteristics.HighEntropyVirtualAddressSpace;
		}
		if (configureToExecuteInAppContainer)
		{
			dllCharacteristics |= DllCharacteristics.AppContainer;
		}
		return dllCharacteristics;
	}

	private static Characteristics GetCharacteristics(OutputKind outputKind, bool requires32Bit)
	{
		Characteristics characteristics = Characteristics.ExecutableImage;
		characteristics = ((!requires32Bit) ? (characteristics | Characteristics.LargeAddressAware) : (characteristics | Characteristics.Bit32Machine));
		switch (outputKind)
		{
		case OutputKind.DynamicallyLinkedLibrary:
		case OutputKind.NetModule:
		case OutputKind.WindowsRuntimeMetadata:
			characteristics |= Characteristics.Dll;
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(outputKind);
		case OutputKind.ConsoleApplication:
		case OutputKind.WindowsApplication:
		case OutputKind.WindowsRuntimeApplication:
			break;
		}
		return characteristics;
	}

	private static Subsystem GetSubsystem(OutputKind outputKind)
	{
		switch (outputKind)
		{
		case OutputKind.ConsoleApplication:
		case OutputKind.DynamicallyLinkedLibrary:
		case OutputKind.NetModule:
		case OutputKind.WindowsRuntimeMetadata:
			return Subsystem.WindowsCui;
		case OutputKind.WindowsApplication:
		case OutputKind.WindowsRuntimeApplication:
			return Subsystem.WindowsGui;
		default:
			throw ExceptionUtilities.UnexpectedValue(outputKind);
		}
	}

	internal abstract bool HasCodeToEmit();

	internal abstract CommonPEModuleBuilder CreateModuleBuilder(EmitOptions emitOptions, IMethodSymbol debugEntryPoint, Stream sourceLinkStream, IEnumerable<EmbeddedText> embeddedTexts, IEnumerable<ResourceDescription> manifestResources, CompilationTestData testData, DiagnosticBag diagnostics, CancellationToken cancellationToken);

	internal abstract bool CompileMethods(CommonPEModuleBuilder moduleBuilder, bool emittingPdb, bool emitMetadataOnly, bool emitTestCoverageData, DiagnosticBag diagnostics, Predicate<ISymbol> filterOpt, CancellationToken cancellationToken);

	internal bool CreateDebugDocuments(DebugDocumentsBuilder documentsBuilder, IEnumerable<EmbeddedText> embeddedTexts, DiagnosticBag diagnostics)
	{
		bool flag = true;
		foreach (SyntaxTree syntaxTree in SyntaxTrees)
		{
			if (!string.IsNullOrEmpty(syntaxTree.FilePath) && syntaxTree.GetText().Encoding == null)
			{
				diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_EncodinglessSyntaxTree, syntaxTree.GetRoot().GetLocation()));
				flag = false;
			}
		}
		if (!flag)
		{
			return false;
		}
		if (!embeddedTexts.IsEmpty())
		{
			ArrayBuilder<DebugSourceDocument> instance = ArrayBuilder<DebugSourceDocument>.GetInstance();
			foreach (EmbeddedText text in embeddedTexts)
			{
				string text2 = documentsBuilder.NormalizeDebugDocumentPath(text.FilePath, null);
				if (documentsBuilder.TryGetDebugDocumentForNormalizedPath(text2) == null)
				{
					DebugSourceDocument debugSourceDocument = new DebugSourceDocument(text2, DebugSourceDocumentLanguageId, () => text.GetDebugSourceInfo());
					documentsBuilder.AddDebugDocument(debugSourceDocument);
					instance.Add(debugSourceDocument);
				}
			}
			documentsBuilder.EmbeddedDocuments = instance.ToImmutableAndFree();
		}
		foreach (SyntaxTree tree in SyntaxTrees)
		{
			if (string.IsNullOrEmpty(tree.FilePath))
			{
				continue;
			}
			string text3 = documentsBuilder.NormalizeDebugDocumentPath(tree.FilePath, null);
			if (documentsBuilder.TryGetDebugDocumentForNormalizedPath(text3) == null)
			{
				documentsBuilder.AddDebugDocument(new DebugSourceDocument(text3, DebugSourceDocumentLanguageId, () => tree.GetDebugSourceInfo()));
			}
		}
		foreach (SyntaxTree syntaxTree2 in SyntaxTrees)
		{
			AddDebugSourceDocumentsForChecksumDirectives(documentsBuilder, syntaxTree2, diagnostics);
		}
		return true;
	}

	internal abstract void AddDebugSourceDocumentsForChecksumDirectives(DebugDocumentsBuilder documentsBuilder, SyntaxTree tree, DiagnosticBag diagnostics);

	internal abstract bool GenerateResourcesAndDocumentationComments(CommonPEModuleBuilder moduleBeingBuilt, Stream xmlDocumentationStream, Stream win32ResourcesStream, string outputNameOverride, DiagnosticBag diagnostics, CancellationToken cancellationToken);

	internal abstract void ReportUnusedImports(SyntaxTree filterTree, DiagnosticBag diagnostics, CancellationToken cancellationToken);

	internal abstract void CompleteTrees(SyntaxTree filterTree);

	internal bool Compile(CommonPEModuleBuilder moduleBuilder, bool emittingPdb, DiagnosticBag diagnostics, Predicate<ISymbol> filterOpt, CancellationToken cancellationToken)
	{
		try
		{
			return CompileMethods(moduleBuilder, emittingPdb, emitMetadataOnly: false, emitTestCoverageData: false, diagnostics, filterOpt, cancellationToken);
		}
		finally
		{
			moduleBuilder.CompilationFinished();
		}
	}

	internal void EnsureAnonymousTypeTemplates(CancellationToken cancellationToken)
	{
		if (GetSubmissionSlotIndex() >= 0 && HasCodeToEmit())
		{
			if (!CommonAnonymousTypeManager.AreTemplatesSealed)
			{
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				CommonPEModuleBuilder commonPEModuleBuilder = CreateModuleBuilder(EmitOptions.Default, null, null, null, null, null, instance, cancellationToken);
				if (commonPEModuleBuilder != null)
				{
					Compile(commonPEModuleBuilder, emittingPdb: false, instance, null, cancellationToken);
				}
				instance.Free();
			}
		}
		else
		{
			ScriptCompilationInfo.PreviousScriptCompilation?.EnsureAnonymousTypeTemplates(cancellationToken);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public EmitResult Emit(Stream peStream, Stream pdbStream, Stream xmlDocumentationStream, Stream win32Resources, IEnumerable<ResourceDescription> manifestResources, EmitOptions options, CancellationToken cancellationToken)
	{
		return Emit(peStream, pdbStream, xmlDocumentationStream, win32Resources, manifestResources, options, null, null, null, cancellationToken);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public EmitResult Emit(Stream peStream, Stream pdbStream, Stream xmlDocumentationStream, Stream win32Resources, IEnumerable<ResourceDescription> manifestResources, EmitOptions options, IMethodSymbol debugEntryPoint, CancellationToken cancellationToken)
	{
		return Emit(peStream, pdbStream, xmlDocumentationStream, win32Resources, manifestResources, options, debugEntryPoint, null, null, cancellationToken);
	}

	public EmitResult Emit(Stream peStream, Stream pdbStream, Stream xmlDocumentationStream, Stream win32Resources, IEnumerable<ResourceDescription> manifestResources, EmitOptions options, IMethodSymbol debugEntryPoint, Stream sourceLinkStream, IEnumerable<EmbeddedText> embeddedTexts, CancellationToken cancellationToken)
	{
		return Emit(peStream, pdbStream, xmlDocumentationStream, win32Resources, manifestResources, options, debugEntryPoint, sourceLinkStream, embeddedTexts, null, cancellationToken);
	}

	public EmitResult Emit(Stream peStream, Stream pdbStream = null, Stream xmlDocumentationStream = null, Stream win32Resources = null, IEnumerable<ResourceDescription> manifestResources = null, EmitOptions options = null, IMethodSymbol debugEntryPoint = null, Stream sourceLinkStream = null, IEnumerable<EmbeddedText> embeddedTexts = null, Stream metadataPEStream = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (peStream == null)
		{
			throw new ArgumentNullException("peStream");
		}
		if (!peStream.CanWrite)
		{
			throw new ArgumentException(CodeAnalysisResources.StreamMustSupportWrite, "peStream");
		}
		if (pdbStream != null)
		{
			if ((object)options != null && options.DebugInformationFormat == DebugInformationFormat.Embedded)
			{
				throw new ArgumentException(CodeAnalysisResources.PdbStreamUnexpectedWhenEmbedding, "pdbStream");
			}
			if (!pdbStream.CanWrite)
			{
				throw new ArgumentException(CodeAnalysisResources.StreamMustSupportWrite, "pdbStream");
			}
			if ((object)options != null && options.EmitMetadataOnly)
			{
				throw new ArgumentException(CodeAnalysisResources.PdbStreamUnexpectedWhenEmittingMetadataOnly, "pdbStream");
			}
		}
		if (metadataPEStream != null && (object)options != null && options.EmitMetadataOnly)
		{
			throw new ArgumentException(CodeAnalysisResources.MetadataPeStreamUnexpectedWhenEmittingMetadataOnly, "metadataPEStream");
		}
		if (metadataPEStream != null && (object)options != null && options.IncludePrivateMembers)
		{
			throw new ArgumentException(CodeAnalysisResources.IncludingPrivateMembersUnexpectedWhenEmittingToMetadataPeStream, "metadataPEStream");
		}
		if (metadataPEStream == null && (object)options != null && !options.EmitMetadataOnly)
		{
			options = options.WithIncludePrivateMembers(value: true);
		}
		if ((object)options != null && options.DebugInformationFormat == DebugInformationFormat.Embedded && (object)options != null && options.EmitMetadataOnly)
		{
			throw new ArgumentException(CodeAnalysisResources.EmbeddingPdbUnexpectedWhenEmittingMetadata, "metadataPEStream");
		}
		if (Options.OutputKind == OutputKind.NetModule)
		{
			if (metadataPEStream != null)
			{
				throw new ArgumentException(CodeAnalysisResources.CannotTargetNetModuleWhenEmittingRefAssembly, "metadataPEStream");
			}
			if ((object)options != null && options.EmitMetadataOnly)
			{
				throw new ArgumentException(CodeAnalysisResources.CannotTargetNetModuleWhenEmittingRefAssembly, "EmitMetadataOnly");
			}
		}
		if (win32Resources != null && (!win32Resources.CanRead || !win32Resources.CanSeek))
		{
			throw new ArgumentException(CodeAnalysisResources.StreamMustSupportReadAndSeek, "win32Resources");
		}
		if (sourceLinkStream != null && !sourceLinkStream.CanRead)
		{
			throw new ArgumentException(CodeAnalysisResources.StreamMustSupportRead, "sourceLinkStream");
		}
		if (embeddedTexts != null && !embeddedTexts.IsEmpty() && pdbStream == null && ((object)options == null || options.DebugInformationFormat != DebugInformationFormat.Embedded))
		{
			throw new ArgumentException(CodeAnalysisResources.EmbeddedTextsRequirePdb, "embeddedTexts");
		}
		return Emit(peStream, metadataPEStream, pdbStream, xmlDocumentationStream, win32Resources, manifestResources, options, debugEntryPoint, sourceLinkStream, embeddedTexts, null, cancellationToken);
	}

	internal EmitResult Emit(Stream peStream, Stream metadataPEStream, Stream pdbStream, Stream xmlDocumentationStream, Stream win32Resources, IEnumerable<ResourceDescription> manifestResources, EmitOptions options, IMethodSymbol debugEntryPoint, Stream sourceLinkStream, IEnumerable<EmbeddedText> embeddedTexts, CompilationTestData testData, CancellationToken cancellationToken)
	{
		options = options ?? EmitOptions.Default.WithIncludePrivateMembers(metadataPEStream == null);
		bool flag = options.DebugInformationFormat == DebugInformationFormat.Embedded;
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		CommonPEModuleBuilder commonPEModuleBuilder = CheckOptionsAndCreateModuleBuilder(instance, manifestResources, options, debugEntryPoint, sourceLinkStream, embeddedTexts, testData, cancellationToken);
		bool flag2 = false;
		if (commonPEModuleBuilder != null)
		{
			try
			{
				flag2 = CompileMethods(commonPEModuleBuilder, (pdbStream != null) | flag, options.EmitMetadataOnly, options.EmitTestCoverageData, instance, null, cancellationToken);
				if (!options.EmitMetadataOnly)
				{
					if (!GenerateResourcesAndDocumentationComments(commonPEModuleBuilder, xmlDocumentationStream, win32Resources, options.OutputNameOverride, instance, cancellationToken))
					{
						flag2 = false;
					}
					if (flag2)
					{
						ReportUnusedImports(null, instance, cancellationToken);
					}
				}
			}
			finally
			{
				commonPEModuleBuilder.CompilationFinished();
			}
			RSAParameters? privateKeyOpt = null;
			StrongNameProvider strongNameProvider = Options.StrongNameProvider;
			if (strongNameProvider != null && strongNameProvider.Capability == SigningCapability.SignsPeBuilder && !Options.PublicSign)
			{
				privateKeyOpt = StrongNameKeys.PrivateKey;
			}
			if (flag2)
			{
				flag2 = SerializeToPeStream(commonPEModuleBuilder, new SimpleEmitStreamProvider(peStream), (metadataPEStream != null) ? new SimpleEmitStreamProvider(metadataPEStream) : null, (pdbStream != null) ? new SimpleEmitStreamProvider(pdbStream) : null, testData?.SymWriterFactory, instance, options.EmitMetadataOnly, options.IncludePrivateMembers, options.EmitTestCoverageData, options.PdbFilePath, privateKeyOpt, cancellationToken);
			}
		}
		return new EmitResult(flag2, instance.ToReadOnlyAndFree());
	}

	public EmitDifferenceResult EmitDifference(EmitBaseline baseline, IEnumerable<SemanticEdit> edits, Stream metadataStream, Stream ilStream, Stream pdbStream, ICollection<MethodDefinitionHandle> updatedMethods, CancellationToken cancellationToken = default(CancellationToken))
	{
		return EmitDifference(baseline, edits, (ISymbol s) => false, metadataStream, ilStream, pdbStream, updatedMethods, cancellationToken);
	}

	public EmitDifferenceResult EmitDifference(EmitBaseline baseline, IEnumerable<SemanticEdit> edits, Func<ISymbol, bool> isAddedSymbol, Stream metadataStream, Stream ilStream, Stream pdbStream, ICollection<MethodDefinitionHandle> updatedMethods, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (baseline == null)
		{
			throw new ArgumentNullException("baseline");
		}
		if (edits == null)
		{
			throw new ArgumentNullException("edits");
		}
		if (isAddedSymbol == null)
		{
			throw new ArgumentNullException("isAddedSymbol");
		}
		if (metadataStream == null)
		{
			throw new ArgumentNullException("metadataStream");
		}
		if (ilStream == null)
		{
			throw new ArgumentNullException("ilStream");
		}
		if (pdbStream == null)
		{
			throw new ArgumentNullException("pdbStream");
		}
		return EmitDifference(baseline, edits, isAddedSymbol, metadataStream, ilStream, pdbStream, updatedMethods, null, cancellationToken);
	}

	internal abstract EmitDifferenceResult EmitDifference(EmitBaseline baseline, IEnumerable<SemanticEdit> edits, Func<ISymbol, bool> isAddedSymbol, Stream metadataStream, Stream ilStream, Stream pdbStream, ICollection<MethodDefinitionHandle> updatedMethodHandles, CompilationTestData testData, CancellationToken cancellationToken);

	internal CommonPEModuleBuilder CheckOptionsAndCreateModuleBuilder(DiagnosticBag diagnostics, IEnumerable<ResourceDescription> manifestResources, EmitOptions options, IMethodSymbol debugEntryPoint, Stream sourceLinkStream, IEnumerable<EmbeddedText> embeddedTexts, CompilationTestData testData, CancellationToken cancellationToken)
	{
		options.ValidateOptions(diagnostics, MessageProvider, Options.Deterministic);
		if (debugEntryPoint != null)
		{
			ValidateDebugEntryPoint(debugEntryPoint, diagnostics);
		}
		if (Options.OutputKind == OutputKind.NetModule && manifestResources != null)
		{
			foreach (ResourceDescription manifestResource in manifestResources)
			{
				if (manifestResource.FileName != null)
				{
					diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_ResourceInModule, Location.None));
				}
			}
		}
		if (diagnostics.HasAnyErrors())
		{
			return null;
		}
		if (IsSubmission && !HasCodeToEmit())
		{
			diagnostics.AddRange(GetDiagnostics());
			return null;
		}
		return CreateModuleBuilder(options, debugEntryPoint, sourceLinkStream, embeddedTexts, manifestResources, testData, diagnostics, cancellationToken);
	}

	internal abstract void ValidateDebugEntryPoint(IMethodSymbol debugEntryPoint, DiagnosticBag diagnostics);

	internal bool SerializeToPeStream(CommonPEModuleBuilder moduleBeingBuilt, EmitStreamProvider peStreamProvider, EmitStreamProvider metadataPEStreamProvider, EmitStreamProvider pdbStreamProvider, Func<ISymWriterMetadataProvider, SymUnmanagedWriter> testSymWriterFactory, DiagnosticBag diagnostics, bool metadataOnly, bool includePrivateMembers, bool emitTestCoverageData, string pePdbFilePath, RSAParameters? privateKeyOpt, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		PdbWriter pdbWriter = null;
		Stream signingInputStream = null;
		DiagnosticBag metadataDiagnostics = null;
		DiagnosticBag diagnosticBag = null;
		Stream peStream = null;
		bool isEmitDeterministic = IsEmitDeterministic;
		pePdbFilePath = ((moduleBeingBuilt.DebugInformationFormat != DebugInformationFormat.Embedded && pdbStreamProvider == null) ? null : (pePdbFilePath ?? FileNameUtilities.ChangeExtension(SourceModule.Name, "pdb")));
		if (moduleBeingBuilt.DebugInformationFormat == DebugInformationFormat.Embedded && !string.IsNullOrEmpty(pePdbFilePath))
		{
			pePdbFilePath = PathUtilities.GetFileName(pePdbFilePath);
		}
		try
		{
			metadataDiagnostics = DiagnosticBag.GetInstance();
			if (moduleBeingBuilt.DebugInformationFormat == DebugInformationFormat.Pdb && pdbStreamProvider != null)
			{
				pdbWriter = new PdbWriter(pePdbFilePath, testSymWriterFactory, isEmitDeterministic ? moduleBeingBuilt.PdbChecksumAlgorithm : default(HashAlgorithmName));
			}
			Func<Stream> getPeStream = () =>
			{
				(Stream, Stream, Stream) peStream2 = GetPeStream(metadataDiagnostics, peStreamProvider, metadataOnly);
				(peStream, signingInputStream, _) = peStream2;
				return peStream2.Item3;
			};
			Func<Stream> getMetadataPeStreamOpt = ((metadataPEStreamProvider == null) ? null : ((Func<Stream>)(() => ConditionalGetOrCreateStream(metadataPEStreamProvider, metadataDiagnostics))));
			Func<Stream> getPortablePdbStreamOpt = ((moduleBeingBuilt.DebugInformationFormat != DebugInformationFormat.PortablePdb || pdbStreamProvider == null) ? null : ((Func<Stream>)(() => ConditionalGetOrCreateStream(pdbStreamProvider, metadataDiagnostics))));
			try
			{
				if (SerializePeToStream(moduleBeingBuilt, metadataDiagnostics, MessageProvider, getPeStream, getMetadataPeStreamOpt, getPortablePdbStreamOpt, pdbWriter, pePdbFilePath, metadataOnly, includePrivateMembers, isEmitDeterministic, emitTestCoverageData, privateKeyOpt, cancellationToken) && pdbWriter != null)
				{
					Stream orCreateStream = pdbStreamProvider.GetOrCreateStream(metadataDiagnostics);
					if (orCreateStream != null)
					{
						pdbWriter.WriteTo(orCreateStream);
					}
				}
			}
			catch (SymUnmanagedWriterException ex)
			{
				diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_PdbWritingFailed, Location.None, ex.Message));
				return false;
			}
			catch (PeWritingException ex2)
			{
				diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_PeWritingFailure, Location.None, ex2.InnerException.ToString()));
				return false;
			}
			catch (ResourceException ex3)
			{
				diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_CantReadResource, Location.None, ex3.Message, ex3.InnerException.Message));
				return false;
			}
			catch (PermissionSetFileReadException ex4)
			{
				diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_PermissionSetAttributeFileReadError, Location.None, ex4.FileName, ex4.PropertyName, ex4.Message));
				return false;
			}
			if (!FilterAndAppendAndFreeDiagnostics(diagnostics, ref metadataDiagnostics))
			{
				return false;
			}
			if (signingInputStream != null && peStream != null)
			{
				try
				{
					Options.StrongNameProvider.SignStream(StrongNameKeys, signingInputStream, peStream);
				}
				catch (DesktopStrongNameProvider.ClrStrongNameMissingException)
				{
					diagnostics.Add(StrongNameKeys.GetError(StrongNameKeys.KeyFilePath, StrongNameKeys.KeyContainer, new CodeAnalysisResourcesLocalizableErrorArgument("AssemblySigningNotSupported"), MessageProvider));
					return false;
				}
				catch (IOException ex6)
				{
					diagnostics.Add(StrongNameKeys.GetError(StrongNameKeys.KeyFilePath, StrongNameKeys.KeyContainer, ex6.Message, MessageProvider));
					return false;
				}
			}
		}
		finally
		{
			pdbWriter?.Dispose();
			signingInputStream?.Dispose();
			diagnosticBag?.Free();
			metadataDiagnostics?.Free();
		}
		return true;
	}

	private static Stream ConditionalGetOrCreateStream(EmitStreamProvider metadataPEStreamProvider, DiagnosticBag metadataDiagnostics)
	{
		if (metadataDiagnostics.HasAnyErrors())
		{
			return null;
		}
		return metadataPEStreamProvider.GetOrCreateStream(metadataDiagnostics);
	}

	private (Stream peStream, Stream signingStream, Stream selectedStream) GetPeStream(DiagnosticBag metadataDiagnostics, EmitStreamProvider peStreamProvider, bool metadataOnly)
	{
		Stream item = null;
		Stream item2 = null;
		Stream item3 = null;
		if (metadataDiagnostics.HasAnyErrors())
		{
			return (peStream: item, signingStream: item2, selectedStream: item3);
		}
		item = peStreamProvider.GetOrCreateStream(metadataDiagnostics);
		if (item == null)
		{
			return (peStream: item, signingStream: item2, selectedStream: item3);
		}
		if (!metadataOnly && IsRealSigned && Options.StrongNameProvider.Capability == SigningCapability.SignsStream)
		{
			try
			{
				item2 = Options.StrongNameProvider.CreateInputStream();
			}
			catch (IOException inner)
			{
				throw new PeWritingException(inner);
			}
			item3 = item2;
		}
		else
		{
			item2 = null;
			item3 = item;
		}
		return (peStream: item, signingStream: item2, selectedStream: item3);
	}

	internal static bool SerializePeToStream(CommonPEModuleBuilder moduleBeingBuilt, DiagnosticBag metadataDiagnostics, CommonMessageProvider messageProvider, Func<Stream> getPeStream, Func<Stream> getMetadataPeStreamOpt, Func<Stream> getPortablePdbStreamOpt, PdbWriter nativePdbWriterOpt, string pdbPathOpt, bool metadataOnly, bool includePrivateMembers, bool isDeterministic, bool emitTestCoverageData, RSAParameters? privateKeyOpt, CancellationToken cancellationToken)
	{
		bool flag = getMetadataPeStreamOpt != null;
		bool includePrivateMembers2 = !metadataOnly || includePrivateMembers;
		bool isDeterministic2 = (metadataOnly && !includePrivateMembers) | isDeterministic;
		if (!PeWriter.WritePeToStream(new EmitContext(moduleBeingBuilt, null, metadataDiagnostics, metadataOnly, includePrivateMembers2), messageProvider, getPeStream, getPortablePdbStreamOpt, nativePdbWriterOpt, pdbPathOpt, metadataOnly, isDeterministic2, emitTestCoverageData, privateKeyOpt, cancellationToken))
		{
			return false;
		}
		if (flag && !PeWriter.WritePeToStream(new EmitContext(moduleBeingBuilt, null, metadataDiagnostics, metadataOnly: true, includePrivateMembers: false), messageProvider, getMetadataPeStreamOpt, null, null, null, metadataOnly: true, isDeterministic: true, emitTestCoverageData: false, privateKeyOpt, cancellationToken))
		{
			return false;
		}
		return true;
	}

	internal EmitBaseline SerializeToDeltaStreams(CommonPEModuleBuilder moduleBeingBuilt, EmitBaseline baseline, DefinitionMap definitionMap, SymbolChanges changes, Stream metadataStream, Stream ilStream, Stream pdbStream, ICollection<MethodDefinitionHandle> updatedMethods, DiagnosticBag diagnostics, Func<ISymWriterMetadataProvider, SymUnmanagedWriter> testSymWriterFactory, string pdbFilePath, CancellationToken cancellationToken)
	{
		PdbWriter pdbWriter = ((moduleBeingBuilt.DebugInformationFormat != DebugInformationFormat.Pdb) ? null : new PdbWriter(pdbFilePath ?? FileNameUtilities.ChangeExtension(SourceModule.Name, "pdb"), testSymWriterFactory, default));
		using (pdbWriter)
		{
			EmitContext context = new EmitContext(moduleBeingBuilt, null, diagnostics, metadataOnly: false, includePrivateMembers: true);
			Guid encId = Guid.NewGuid();
			try
			{
				DeltaMetadataWriter deltaMetadataWriter = new DeltaMetadataWriter(context, MessageProvider, baseline, encId, definitionMap, changes, cancellationToken);
				deltaMetadataWriter.WriteMetadataAndIL(pdbWriter, metadataStream, ilStream, (pdbWriter == null) ? pdbStream : null, out var metadataSizes);
				deltaMetadataWriter.GetMethodTokens(updatedMethods);
				pdbWriter?.WriteTo(pdbStream);
				return diagnostics.HasAnyErrors() ? null : deltaMetadataWriter.GetDelta(baseline, this, encId, metadataSizes);
			}
			catch (SymUnmanagedWriterException ex)
			{
				diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_PdbWritingFailed, Location.None, ex.Message));
				return null;
			}
			catch (PeWritingException ex2)
			{
				diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_PeWritingFailure, Location.None, ex2.InnerException.ToString()));
				return null;
			}
			catch (PermissionSetFileReadException ex3)
			{
				diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_PermissionSetAttributeFileReadError, Location.None, ex3.FileName, ex3.PropertyName, ex3.Message));
				return null;
			}
		}
	}

	internal string Feature(string p)
	{
		if (!_features.TryGetValue(p, out var value))
		{
			return null;
		}
		return value;
	}

	internal void MarkImportDirectiveAsUsed(SyntaxNode node)
	{
		MarkImportDirectiveAsUsed(node.SyntaxTree, node.Span.Start);
	}

	internal void MarkImportDirectiveAsUsed(SyntaxTree syntaxTree, int position)
	{
		if (!IsSubmission && syntaxTree != null)
		{
			TreeToUsedImportDirectivesMap.GetOrAdd(syntaxTree, s_createSetCallback).Add(position);
		}
	}

	internal bool IsImportDirectiveUsed(SyntaxTree syntaxTree, int position)
	{
		if (IsSubmission)
		{
			return true;
		}
		if (syntaxTree != null && TreeToUsedImportDirectivesMap.TryGetValue(syntaxTree, out var value))
		{
			return value.Contains(position);
		}
		return false;
	}

	internal int CompareSyntaxTreeOrdering(SyntaxTree tree1, SyntaxTree tree2)
	{
		if (tree1 == tree2)
		{
			return 0;
		}
		return GetSyntaxTreeOrdinal(tree1) - GetSyntaxTreeOrdinal(tree2);
	}

	internal abstract int GetSyntaxTreeOrdinal(SyntaxTree tree);

	internal abstract int CompareSourceLocations(Location loc1, Location loc2);

	internal abstract int CompareSourceLocations(SyntaxReference loc1, SyntaxReference loc2);

	internal TLocation FirstSourceLocation<TLocation>(TLocation first, TLocation second) where TLocation : Location
	{
		if (CompareSourceLocations(first, second) <= 0)
		{
			return first;
		}
		return second;
	}

	internal TLocation FirstSourceLocation<TLocation>(ImmutableArray<TLocation> locations) where TLocation : Location
	{
		if (locations.IsEmpty)
		{
			return null;
		}
		TLocation val = locations[0];
		for (int i = 1; i < locations.Length; i++)
		{
			val = FirstSourceLocation(val, locations[i]);
		}
		return val;
	}

	internal string GetMessage(CompilationStage stage)
	{
		return $"{AssemblyName} ({stage.ToString()})";
	}

	internal string GetMessage(ITypeSymbol source, ITypeSymbol destination)
	{
		if (source == null || destination == null)
		{
			return AssemblyName;
		}
		return $"{AssemblyName}: {source.TypeKind.ToString()} {source.Name} -> {destination.TypeKind.ToString()} {destination.Name}";
	}

	public abstract bool ContainsSymbolsWithName(Func<string, bool> predicate, SymbolFilter filter = SymbolFilter.TypeAndMember, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IEnumerable<ISymbol> GetSymbolsWithName(Func<string, bool> predicate, SymbolFilter filter = SymbolFilter.TypeAndMember, CancellationToken cancellationToken = default(CancellationToken));

	public abstract bool ContainsSymbolsWithName(string name, SymbolFilter filter = SymbolFilter.TypeAndMember, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IEnumerable<ISymbol> GetSymbolsWithName(string name, SymbolFilter filter = SymbolFilter.TypeAndMember, CancellationToken cancellationToken = default(CancellationToken));

	internal void MakeMemberMissing(WellKnownMember member)
	{
		MakeMemberMissing((int)member);
	}

	internal void MakeMemberMissing(SpecialMember member)
	{
		MakeMemberMissing(0 - member - 1);
	}

	internal bool IsMemberMissing(WellKnownMember member)
	{
		return IsMemberMissing((int)member);
	}

	internal bool IsMemberMissing(SpecialMember member)
	{
		return IsMemberMissing(0 - member - 1);
	}

	private void MakeMemberMissing(int member)
	{
		if (_lazyMakeMemberMissingMap == null)
		{
			_lazyMakeMemberMissingMap = new SmallDictionary<int, bool>();
		}
		_lazyMakeMemberMissingMap[member] = true;
	}

	private bool IsMemberMissing(int member)
	{
		if (_lazyMakeMemberMissingMap != null)
		{
			return _lazyMakeMemberMissingMap.ContainsKey(member);
		}
		return false;
	}

	internal void MakeTypeMissing(WellKnownType type)
	{
		if (_lazyMakeWellKnownTypeMissingMap == null)
		{
			_lazyMakeWellKnownTypeMissingMap = new SmallDictionary<int, bool>();
		}
		_lazyMakeWellKnownTypeMissingMap[(int)type] = true;
	}

	internal bool IsTypeMissing(WellKnownType type)
	{
		if (_lazyMakeWellKnownTypeMissingMap != null)
		{
			return _lazyMakeWellKnownTypeMissingMap.ContainsKey((int)type);
		}
		return false;
	}

	public ImmutableArray<AssemblyIdentity> GetUnreferencedAssemblyIdentities(Diagnostic diagnostic)
	{
		if (diagnostic == null)
		{
			throw new ArgumentNullException("diagnostic");
		}
		if (!IsUnreferencedAssemblyIdentityDiagnosticCode(diagnostic.Code))
		{
			return ImmutableArray<AssemblyIdentity>.Empty;
		}
		ArrayBuilder<AssemblyIdentity> instance = ArrayBuilder<AssemblyIdentity>.GetInstance();
		foreach (object argument in diagnostic.Arguments)
		{
			if (argument is AssemblyIdentity item)
			{
				instance.Add(item);
			}
		}
		return instance.ToImmutableAndFree();
	}

	internal abstract bool IsUnreferencedAssemblyIdentityDiagnosticCode(int code);

	public static string GetRequiredLanguageVersion(Diagnostic diagnostic)
	{
		if (diagnostic == null)
		{
			throw new ArgumentNullException("diagnostic");
		}
		string result = null;
		if (diagnostic.Arguments != null)
		{
			foreach (object argument in diagnostic.Arguments)
			{
				if (argument is RequiredLanguageVersion requiredLanguageVersion)
				{
					result = requiredLanguageVersion.ToString();
				}
			}
		}
		return result;
	}

	internal void CacheRetargetingAssemblySymbolNoLock(IAssemblySymbol assembly)
	{
		_retargetingAssemblySymbols.Add(assembly);
	}

	internal void AddRetargetingAssemblySymbolsNoLock<T>(List<T> result) where T : IAssemblySymbol
	{
		foreach (IAssemblySymbol retargetingAssemblySymbol in _retargetingAssemblySymbols)
		{
			result.Add((T)retargetingAssemblySymbol);
		}
	}
}
