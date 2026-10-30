using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Translation;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
public class Compilation
{
	internal static readonly ParallelOptions DefaultParallelOptions = new ParallelOptions();

	private static readonly CompilationOptions defaultOptions = new CompilationOptions();

	private HashSet<SyntaxTree> lazyCompilationUnitCompletedTrees;

	internal readonly AsyncQueue<CompilationEvent> EventQueue;

	private readonly SyntaxAndDeclarationManager syntaxAndDeclarations;

	private ModuleSymbol lazyModuleSymbol;

	internal readonly BuiltInOperators BuiltInOperators;

	private DiagnosticBag lazyDeclarationDiagnostics;

	private bool declarationDiagnosticsFrozen;

	internal static readonly CompilationStage DefaultDiagnosticsStage = CompilationStage.Compile;

	private WeakReference<BinderFactory>[] binderFactories;

	public string ModuleName => ModuleInfo.Name;

	public SymbolReferenceSpecification ModuleInfo { get; }

	internal LanguagesMetadata LanguagesMetadata { get; private set; }

	internal IDotNetResolverFactory DotNetResolverFactory { get; }

	internal ISymbolReferenceLoader ReferenceLoader { get; }

	public IFileSystem FileSystem { get; }

	internal IReferenceManager ReferenceManager { get; private set; }

	internal IEnumerable<SymbolReferenceSpecification> SymbolReferences { get; }

	internal ModuleSymbol CompiledModule
	{
		get
		{
			if (lazyModuleSymbol == null)
			{
				lazyModuleSymbol = new SourceModuleSymbol(this, syntaxAndDeclarations.GetLazyState().DeclarationTable, ModuleName, ModuleInfo.Publisher, ModuleInfo.Version, ModuleInfo.AppId, ReferenceManager.GetLoadedModules());
			}
			return lazyModuleSymbol;
		}
	}

	public CompilationOptions Options { get; }

	public ImmutableArray<SyntaxTree> SyntaxTrees => syntaxAndDeclarations.GetLazyState().SyntaxTrees;

	internal CommonMessageProvider MessageProvider => syntaxAndDeclarations.MessageProvider;

	internal DiagnosticBag DeclarationDiagnostics
	{
		get
		{
			DebugAssertHelper.Assert(!declarationDiagnosticsFrozen || true);
			if (lazyDeclarationDiagnostics == null)
			{
				DiagnosticBag value = new DiagnosticBag();
				Interlocked.CompareExchange(ref lazyDeclarationDiagnostics, value, null);
			}
			return lazyDeclarationDiagnostics;
		}
	}

	internal DeclarationTable Declarations => syntaxAndDeclarations.GetLazyState().DeclarationTable;

	private Compilation(SymbolReferenceSpecification moduleInfo, CompilationOptions options, SyntaxAndDeclarationManager syntaxAndDeclarations, IEnumerable<SymbolReferenceSpecification> referenceSpecifications, ISymbolReferenceLoader referenceLoader, IReferenceManager referenceManager, IFileSystem fileSystem, LanguagesMetadata languagesMetadata, AsyncQueue<CompilationEvent> eventQueue, IDotNetResolverFactory resolverFactory)
	{
		ModuleInfo = moduleInfo;
		BuiltInOperators = new BuiltInOperators();
		Options = options;
		this.syntaxAndDeclarations = syntaxAndDeclarations;
		SymbolReferences = referenceSpecifications ?? SpecializedCollections.EmptyList<SymbolReferenceSpecification>();
		ReferenceLoader = referenceLoader;
		FileSystem = fileSystem;
		DotNetResolverFactory = resolverFactory;
		ReferenceManager = referenceManager ?? Microsoft.Dynamics.Nav.CodeAnalysis.ReferenceManager.Create(SymbolReferences, ReferenceLoader, DotNetResolverFactory);
		EventQueue = eventQueue;
		EventQueue?.TryEnqueue(new CompilationStartedEvent(this));
		LanguagesMetadata = languagesMetadata;
	}

	public Compilation AddReferences(params SymbolReferenceSpecification[] references)
	{
		return AddReferences((IEnumerable<SymbolReferenceSpecification>)references);
	}

	public Compilation AddReferences(IEnumerable<SymbolReferenceSpecification> references)
	{
		if (references == null)
		{
			throw new ArgumentNullException("references");
		}
		if (references.IsEmpty())
		{
			return this;
		}
		return WithReferences(SymbolReferences.Union(references));
	}

	public Compilation WithFileSystem(IFileSystem fileSystem)
	{
		return new Compilation(ModuleInfo, Options, syntaxAndDeclarations, SymbolReferences, ReferenceLoader, ReferenceManager, fileSystem, null, null, DotNetResolverFactory);
	}

	public Compilation WithReferences(IEnumerable<SymbolReferenceSpecification> references)
	{
		return new Compilation(ModuleInfo, Options, syntaxAndDeclarations, references, ReferenceLoader, null, FileSystem, LanguagesMetadata, null, DotNetResolverFactory);
	}

	public Compilation WithReferenceLoader(ISymbolReferenceLoader referenceLoader)
	{
		return new Compilation(ModuleInfo, Options, syntaxAndDeclarations, SymbolReferences, referenceLoader, null, FileSystem, LanguagesMetadata, null, DotNetResolverFactory);
	}

	public Compilation WithDotNetResolverFactory(IDotNetResolverFactory dotNetResolverFactory)
	{
		DotNetResolverFactory?.Dispose();
		return new Compilation(ModuleInfo, Options, syntaxAndDeclarations, SymbolReferences, ReferenceLoader, null, FileSystem, LanguagesMetadata, null, dotNetResolverFactory);
	}

	public Compilation WithSupportedLocales(NavAppManifest manifest)
	{
		return new Compilation(ModuleInfo, Options.WithSupportedLocales(manifest.AppSupportedLocales), syntaxAndDeclarations, SymbolReferences, ReferenceLoader, null, FileSystem, LanguagesMetadata, null, DotNetResolverFactory);
	}

	public Compilation AddSyntaxTrees(params SyntaxTree[] trees)
	{
		return AddSyntaxTrees((IEnumerable<SyntaxTree>)trees);
	}

	public Compilation AddSyntaxTrees(IEnumerable<SyntaxTree> trees)
	{
		if (trees == null)
		{
			throw new ArgumentNullException("trees");
		}
		IList<SyntaxTree> list = trees.ToArray();
		if (list.Count == 0)
		{
			return this;
		}
		PooledHashSet<SyntaxTree> instance = PooledHashSet<SyntaxTree>.GetInstance();
		SyntaxAndDeclarationManager syntaxAndDeclarationManager = syntaxAndDeclarations;
		try
		{
			instance.AddAll(syntaxAndDeclarationManager.ExternalSyntaxTrees);
			int num = 0;
			foreach (SyntaxTree item in list)
			{
				if (item == null)
				{
					throw new ArgumentNullException("trees[" + num + "]");
				}
				if (!item.HasCompilationUnitRoot)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, CodeAnalysisResources.TreeMustHaveARootNodeWith, num));
				}
				if (instance.Contains(item))
				{
					throw new ArgumentException(CodeAnalysisResources.SyntaxTreeAlreadyPresent, string.Format(CultureInfo.InvariantCulture, CodeAnalysisResources.Trees0, num));
				}
				instance.Add(item);
				num++;
			}
		}
		finally
		{
			instance.Free();
		}
		syntaxAndDeclarationManager = syntaxAndDeclarationManager.AddSyntaxTrees(list);
		return Update(syntaxAndDeclarationManager);
	}

	public Compilation RemoveSyntaxTrees(params SyntaxTree[] trees)
	{
		return RemoveSyntaxTrees((IEnumerable<SyntaxTree>)trees);
	}

	public Compilation RemoveSyntaxTrees(IEnumerable<SyntaxTree> trees)
	{
		if (trees == null)
		{
			throw new ArgumentNullException("trees");
		}
		if (trees.IsEmpty())
		{
			return this;
		}
		PooledHashSet<SyntaxTree> instance = PooledHashSet<SyntaxTree>.GetInstance();
		PooledHashSet<SyntaxTree> instance2 = PooledHashSet<SyntaxTree>.GetInstance();
		instance2.AddAll(syntaxAndDeclarations.ExternalSyntaxTrees);
		SyntaxAndDeclarationManager syntaxAndDeclarationManager = syntaxAndDeclarations;
		int num = 0;
		foreach (SyntaxTree tree in trees)
		{
			if (!instance2.Contains(tree))
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, CodeAnalysisResources.SyntaxTreeNotFoundTo, tree), string.Format(CultureInfo.InvariantCulture, "{0}[{1}]", "trees", num));
			}
			instance.Add(tree);
			num++;
		}
		instance2.Free();
		syntaxAndDeclarationManager = syntaxAndDeclarationManager.RemoveSyntaxTrees(instance);
		instance.Free();
		return Update(syntaxAndDeclarationManager);
	}

	public Compilation RemoveAllSyntaxTrees()
	{
		SyntaxAndDeclarationManager syntaxAndDeclarationManager = syntaxAndDeclarations;
		return Update(syntaxAndDeclarationManager.WithExternalSyntaxTrees(ImmutableArray<SyntaxTree>.Empty));
	}

	public Compilation ReplaceSyntaxTree(SyntaxTree oldTree, SyntaxTree newTree)
	{
		if (oldTree == null)
		{
			throw new ArgumentNullException("oldTree");
		}
		if (newTree == null)
		{
			return RemoveSyntaxTrees(oldTree);
		}
		if (newTree == oldTree)
		{
			return this;
		}
		if (!newTree.HasCompilationUnitRoot)
		{
			throw new ArgumentException(CodeAnalysisResources.TreeMustHaveARootNodeWith, "newTree");
		}
		SyntaxAndDeclarationManager syntaxAndDeclarationManager = syntaxAndDeclarations;
		ImmutableArray<SyntaxTree> externalSyntaxTrees = syntaxAndDeclarationManager.ExternalSyntaxTrees;
		if (!externalSyntaxTrees.Contains(oldTree))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, CodeAnalysisResources.SyntaxTreeNotFoundTo, oldTree), "oldTree");
		}
		if (externalSyntaxTrees.Contains(newTree))
		{
			throw new ArgumentException(CodeAnalysisResources.SyntaxTreeAlreadyPresent, "newTree");
		}
		syntaxAndDeclarationManager = syntaxAndDeclarationManager.ReplaceSyntaxTree(oldTree, newTree);
		return Update(syntaxAndDeclarationManager);
	}

	public Compilation Clone()
	{
		return new Compilation(ModuleInfo, Options, syntaxAndDeclarations, SymbolReferences, ReferenceLoader, ReferenceManager, FileSystem, LanguagesMetadata, EventQueue, DotNetResolverFactory);
	}

	private Compilation Update(SyntaxAndDeclarationManager syntaxAndDeclarationsParameter)
	{
		return new Compilation(ModuleInfo, Options, syntaxAndDeclarationsParameter, SymbolReferences, ReferenceLoader, ReferenceManager, FileSystem, LanguagesMetadata, EventQueue, DotNetResolverFactory);
	}

	public bool ContainsSyntaxTree(SyntaxTree syntaxTree)
	{
		return syntaxAndDeclarations.GetLazyState().RootObjectCollections.ContainsKey(syntaxTree);
	}

	internal int GetSyntaxTreeOrdinal(SyntaxTree tree)
	{
		DebugAssertHelper.Assert(ContainsSyntaxTree(tree));
		return syntaxAndDeclarations.GetLazyState().OrdinalMap[tree];
	}

	public ImmutableArray<Diagnostic> GetParseDiagnostics(CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDiagnostics(CompilationStage.Parse, includeEarlierStages: false, cancellationToken);
	}

	public ImmutableArray<Diagnostic> GetDeclarationDiagnostics(CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDiagnostics(CompilationStage.Declare, includeEarlierStages: false, cancellationToken);
	}

	public ImmutableArray<Diagnostic> GetMethodBodyDiagnostics(CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDiagnostics(CompilationStage.Compile, includeEarlierStages: false, cancellationToken);
	}

	public ImmutableArray<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDiagnostics(DefaultDiagnosticsStage, includeEarlierStages: true, cancellationToken);
	}

	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	internal ImmutableArray<Diagnostic> GetDiagnostics(CompilationStage stage, bool includeEarlierStages, CancellationToken cancellationToken)
	{
		DiagnosticBag builder = DiagnosticBag.GetInstance();
		try
		{
			builder.AddRange(ReferenceManager.GetDiagnostics(cancellationToken));
			if (stage == CompilationStage.Parse || ((stage > CompilationStage.Parse) & includeEarlierStages))
			{
				if (Options.ConcurrentBuild)
				{
					ParallelOptions parallelOptions = (cancellationToken.CanBeCanceled ? new ParallelOptions
					{
						CancellationToken = cancellationToken
					} : DefaultParallelOptions);
					Parallel.For(0, SyntaxTrees.Length, parallelOptions, UICultureUtilities.WithCurrentUICulture((int i) =>
					{
						builder.AddRange(SyntaxTrees[i].GetDiagnostics(cancellationToken));
					}));
				}
				else
				{
					foreach (SyntaxTree syntaxTree in SyntaxTrees)
					{
						cancellationToken.ThrowIfCancellationRequested();
						builder.AddRange(syntaxTree.GetDiagnostics(cancellationToken));
					}
				}
			}
			if (stage == CompilationStage.Declare || ((stage > CompilationStage.Declare) & includeEarlierStages))
			{
				cancellationToken.ThrowIfCancellationRequested();
				cancellationToken.ThrowIfCancellationRequested();
				builder.AddRange(GetSourceDeclarationDiagnostics(null, null, null, cancellationToken));
			}
			cancellationToken.ThrowIfCancellationRequested();
			if (stage == CompilationStage.Compile || ((stage > CompilationStage.Compile) & includeEarlierStages))
			{
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				GetDiagnosticsForAllMethodBodies(instance, cancellationToken);
				builder.AddRangeAndFree(instance);
			}
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception e)
		{
			builder.Add(LogExceptionAndCreateDiagnostic(e));
		}
		DiagnosticBag instance2 = DiagnosticBag.GetInstance();
		FilterAndAppendAndFreeDiagnostics(instance2, ref builder);
		return instance2.ToReadOnlyAndFree<Diagnostic>();
	}

	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	internal ImmutableArray<Diagnostic> GetDiagnosticsForSyntaxTree(CompilationStage stage, SyntaxTree syntaxTree, TextSpan? filterSpanWithinTree, bool includeEarlierStages, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		DiagnosticBag incoming = DiagnosticBag.GetInstance();
		try
		{
			if (stage == CompilationStage.Parse || ((stage > CompilationStage.Parse) & includeEarlierStages))
			{
				IEnumerable<Diagnostic> diagnostics = syntaxTree.GetDiagnostics();
				diagnostics = FilterDiagnosticsByLocation(diagnostics, syntaxTree, filterSpanWithinTree);
				incoming.AddRange(diagnostics);
			}
			cancellationToken.ThrowIfCancellationRequested();
			if (stage == CompilationStage.Declare || ((stage > CompilationStage.Declare) & includeEarlierStages))
			{
				ImmutableArray<Diagnostic> sourceDeclarationDiagnostics = GetSourceDeclarationDiagnostics(syntaxTree, filterSpanWithinTree, FilterDiagnosticsByLocation, cancellationToken);
				DebugAssertHelper.Assert(sourceDeclarationDiagnostics.All((Diagnostic d) => d.ContainsLocation(syntaxTree, filterSpanWithinTree)));
				incoming.AddRange(sourceDeclarationDiagnostics);
			}
			cancellationToken.ThrowIfCancellationRequested();
			if (stage == CompilationStage.Compile || ((stage > CompilationStage.Compile) & includeEarlierStages))
			{
				IEnumerable<Diagnostic> diagnostics2 = GetDiagnosticsForMethodBodiesInTree(syntaxTree, filterSpanWithinTree, cancellationToken);
				diagnostics2 = FilterDiagnosticsByLocation(diagnostics2, syntaxTree, filterSpanWithinTree);
				incoming.AddRange(diagnostics2);
			}
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception e)
		{
			incoming.Add(LogExceptionAndCreateDiagnostic(e));
		}
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		FilterAndAppendAndFreeDiagnostics(instance, ref incoming);
		return instance.ToReadOnlyAndFree<Diagnostic>();
	}

	private static Diagnostic LogExceptionAndCreateDiagnostic(Exception e)
	{
		GlobalLogger.LogException(e);
		string text = e.ToString();
		return Diagnostic.Create(new NavDiagnosticInfo(ErrorCode.ERR_InternalError, text));
	}

	private ImmutableArray<Diagnostic> GetDiagnosticsForMethodBodiesInTree(SyntaxTree tree, TextSpan? span, CancellationToken cancellationToken)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		MethodCompiler.CompileMethodBodies(this, null, hasDeclarationErrors: false, instance, (Symbol s) => IsDefinedOrImplementedInSourceTree(s, tree, span), null, cancellationToken);
		if (!span.HasValue || span.Value == tree.GetRoot(cancellationToken).FullSpan)
		{
			CompleteTrees(tree);
		}
		return instance.ToReadOnlyAndFree();
	}

	private static bool IsDefinedOrImplementedInSourceTree(Symbol symbol, SyntaxTree tree, TextSpan? span)
	{
		if (symbol.IsDefinedInSourceTree(tree, span))
		{
			return true;
		}
		return false;
	}

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Intended spelling")]
	public EmitResult Emit(EmitOptions options, ModuleOutputter outputter, CancellationToken cancellationToken = default(CancellationToken))
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		if (options == null)
		{
			options = EmitOptions.Default;
		}
		ModuleBuilder moduleBuilder = CreateModuleBuilder(options, instance, Options);
		if (moduleBuilder == null)
		{
			return ToEmitResultAndFree(instance, success: false);
		}
		if (!Compile(moduleBuilder, emittingPdb: false, instance, null, null, cancellationToken))
		{
			return ToEmitResultAndFree(instance, success: false);
		}
		WriteToOutput(outputter, moduleBuilder, CompiledModule, instance);
		return ToEmitResultAndFree(instance, success: true);
	}

	private static EmitResult ToEmitResultAndFree(DiagnosticBag diagnostics, bool success)
	{
		return new EmitResult(success, diagnostics.ToReadOnlyAndFree());
	}

	internal static IEnumerable<string> GetProfilePageCustomizationsList(IEnumerable<IProfileTypeSymbol> profiles)
	{
		List<string> list = new List<string>();
		foreach (ProfileTypeSymbol profile in profiles)
		{
			if (profile.GetProperty(PropertyKind.Customizations)?.Property.PropertyValue is BoundIdentifierListPropertyValue boundIdentifierListPropertyValue)
			{
				IEnumerable<string> collection = from BoundApplicationObjectAccess access in boundIdentifierListPropertyValue.Value
					select access.ApplicationObjectTypeSymbol.Name;
				list.AddRange(collection);
			}
		}
		return list;
	}

	internal static void WriteToOutput(ModuleOutputter outputter, ModuleBuilder moduleBuilder, ModuleSymbol compiledModuleSymbol, DiagnosticBag diagnostics)
	{
		outputter.InitializeModule(compiledModuleSymbol);
		ImmutableDictionary<IProfileTypeSymbol, Tuple<MemoryStream, string>> profileObjectMetadata = moduleBuilder.GetProfileObjectMetadata();
		IEnumerable<string> profileCustomizationsNames = GetProfilePageCustomizationsList(profileObjectMetadata.Keys);
		foreach (KeyValuePair<IProfileTypeSymbol, Tuple<MemoryStream, string>> item in profileObjectMetadata)
		{
			outputter.AddProfileObject(item.Key, null, item.Value.Item1, item.Value.Item2);
		}
		ImmutableDictionary<IApplicationObjectTypeSymbol, (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax, string)> applicationObjectCode = moduleBuilder.GetApplicationObjectCode();
		if (moduleBuilder.Options.ConcurrentEmit)
		{
			ParallelOptions parallelOptions = new ParallelOptions
			{
				MaxDegreeOfParallelism = ((applicationObjectCode.Count > Environment.ProcessorCount || applicationObjectCode.Count == 0) ? Environment.ProcessorCount : applicationObjectCode.Count)
			};
			Parallel.ForEach(applicationObjectCode, parallelOptions, (KeyValuePair<IApplicationObjectTypeSymbol, (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax CompilationUnit, string AlSourceCode)> objectTypeAndCode) =>
			{
				EmitCode(objectTypeAndCode, profileCustomizationsNames, outputter, moduleBuilder);
			});
		}
		else
		{
			foreach (KeyValuePair<IApplicationObjectTypeSymbol, (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax, string)> item2 in applicationObjectCode)
			{
				EmitCode(item2, profileCustomizationsNames, outputter, moduleBuilder);
			}
		}
		outputter.FinalizeModule();
		diagnostics.AddRange(outputter.GetDiagnostics());
	}

	internal bool Compile(ModuleBuilder moduleBuilder, bool emittingPdb, DiagnosticBag diagnostics, Predicate<ISymbol> filterOpt = null, Predicate<ISymbol> radCompilationFilter = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		try
		{
			return CompileImpl(moduleBuilder, emittingPdb, diagnostics, filterOpt, radCompilationFilter, cancellationToken);
		}
		finally
		{
			moduleBuilder.CompilationFinished();
		}
	}

	private bool CompileImpl(ModuleBuilder moduleBuilder, bool emittingPdb, DiagnosticBag diagnostics, Predicate<ISymbol> filterOpt, Predicate<ISymbol> radCompilationFilter, CancellationToken cancellationToken)
	{
		if (!FilterAndAppendDiagnostics(diagnostics, GetDiagnostics(CompilationStage.Declare, includeEarlierStages: true, cancellationToken)))
		{
			return false;
		}
		DiagnosticBag incoming = DiagnosticBag.GetInstance();
		LanguagesMetadata = LanguagesMetadata ?? LanguagesMetadata.Extract(FileSystem, incoming, ModuleInfo.Name);
		MethodCompiler.CompileMethodBodies(this, moduleBuilder, emittingPdb, incoming, filterOpt, radCompilationFilter, cancellationToken);
		if (!FilterAndAppendAndFreeDiagnostics(diagnostics, ref incoming))
		{
			return false;
		}
		cancellationToken.ThrowIfCancellationRequested();
		return true;
	}

	private static void EmitCode(KeyValuePair<IApplicationObjectTypeSymbol, (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax CompilationUnit, string AlSourceCode)> objectTypeAndCode, IEnumerable<string> profileCustomizationsNames, ModuleOutputter outputter, ModuleBuilder moduleBuilder)
	{
		moduleBuilder.TryGetApplicationObjectMetadata(objectTypeAndCode.Key, out var metadata);
		if (objectTypeAndCode.Key.Kind == SymbolKind.PageCustomization)
		{
			if (profileCustomizationsNames.Contains(objectTypeAndCode.Key.Name))
			{
				outputter.AddProfileObject(objectTypeAndCode.Key, objectTypeAndCode.Value.CompilationUnit, metadata, objectTypeAndCode.Value.AlSourceCode);
			}
		}
		else
		{
			outputter.AddApplicationObject(objectTypeAndCode.Key, objectTypeAndCode.Value.CompilationUnit, metadata, objectTypeAndCode.Value.AlSourceCode);
		}
	}

	private static IEnumerable<Diagnostic> FilterDiagnosticsByLocation(IEnumerable<Diagnostic> diagnostics, SyntaxTree tree, TextSpan? filterSpanWithinTree)
	{
		foreach (Diagnostic diagnostic in diagnostics)
		{
			if (diagnostic.ContainsLocation(tree, filterSpanWithinTree))
			{
				yield return diagnostic;
			}
		}
	}

	private ImmutableArray<Diagnostic> GetSourceDeclarationDiagnostics(SyntaxTree syntaxTree = null, TextSpan? filterSpanWithinTree = null, Func<IEnumerable<Diagnostic>, SyntaxTree, TextSpan?, IEnumerable<Diagnostic>> locationFilterOpt = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		SourceLocation locationOpt = null;
		if (syntaxTree != null)
		{
			SyntaxNode root = syntaxTree.GetRoot(cancellationToken);
			locationOpt = (filterSpanWithinTree.HasValue ? new SourceLocation(syntaxTree, filterSpanWithinTree.Value) : new SourceLocation(root));
		}
		CompiledModule.ForceComplete(locationOpt, cancellationToken);
		IEnumerable<Diagnostic> enumerable = ((lazyDeclarationDiagnostics != null) ? lazyDeclarationDiagnostics.AsEnumerable() : Enumerable.Empty<Diagnostic>());
		if (locationFilterOpt != null)
		{
			DebugAssertHelper.Assert(syntaxTree != null);
			enumerable = locationFilterOpt(enumerable, syntaxTree, filterSpanWithinTree);
		}
		if (syntaxTree == null)
		{
			declarationDiagnosticsFrozen = true;
		}
		return enumerable.ToImmutableArray();
	}

	private void GetDiagnosticsForAllMethodBodies(DiagnosticBag diagnostics, CancellationToken cancellationToken)
	{
		MethodCompiler.CompileMethodBodies(this, null, hasDeclarationErrors: false, diagnostics, null, null, cancellationToken);
		CompleteTrees(null);
	}

	internal bool FilterAndAppendAndFreeDiagnostics(DiagnosticBag accumulator, ref DiagnosticBag incoming)
	{
		bool result = FilterAndAppendDiagnostics(accumulator, incoming.AsEnumerableWithoutResolution());
		incoming.Free();
		incoming = null;
		return result;
	}

	private bool FilterAndAppendDiagnostics(DiagnosticBag accumulator, IEnumerable<Diagnostic> incoming)
	{
		bool flag = false;
		foreach (Diagnostic item in incoming)
		{
			Diagnostic diagnostic = Options.FilterDiagnostic(item);
			if (diagnostic != null)
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

	public static Compilation Create(string moduleName, string publisher = null, Version version = null, Guid? appId = null, IEnumerable<SyntaxTree> syntaxTrees = null, CompilationOptions options = null, IFileSystem fileSystem = null, IDotNetResolverFactory dotNetResolverFactory = null, IReferenceManager referenceManager = null)
	{
		SyntaxAndDeclarationManager syntaxAndDeclarationManager = new SyntaxAndDeclarationManager(ImmutableArray<SyntaxTree>.Empty, Microsoft.Dynamics.Nav.CodeAnalysis.MessageProvider.Instance, null);
		Compilation compilation = new Compilation(new SymbolReferenceSpecification(publisher ?? "Default Publisher", moduleName, version ?? Version.Parse("1.0.0.0"), exact: false, appId), options ?? defaultOptions, syntaxAndDeclarationManager, null, null, referenceManager, fileSystem, null, null, dotNetResolverFactory ?? NullDotNetResolverFactory.Instance);
		if (syntaxTrees != null)
		{
			compilation = compilation.AddSyntaxTrees(syntaxTrees);
		}
		return compilation;
	}

	public static Compilation CreateForRad(string moduleName, ObjectChangeModelDefinition objectChangeModelDefinition, ModuleDefinition packagedModuleDefinition, ISymbolReferenceLoader symbolReferenceLoader, IEnumerable<SymbolReferenceSpecification> symbolReferences, string publisher = null, Version version = null, Guid? appId = null, IEnumerable<SyntaxTree> syntaxTrees = null, CompilationOptions options = null, IFileSystem fileSystem = null, IDotNetResolverFactory dotNetResolverFactory = null)
	{
		Compilation compilation = Create(moduleName, publisher, version, appId, syntaxTrees, options, fileSystem, dotNetResolverFactory).WithReferenceLoader(symbolReferenceLoader).AddReferences(symbolReferences);
		compilation.CreateRadReferenceManager(objectChangeModelDefinition, packagedModuleDefinition);
		return compilation;
	}

	public SemanticModel GetSemanticModel(SyntaxTree syntaxTree)
	{
		if (syntaxTree == null)
		{
			throw new ArgumentNullException("syntaxTree");
		}
		if (!syntaxAndDeclarations.GetLazyState().RootObjectCollections.ContainsKey(syntaxTree))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, CodeAnalysisResources.SyntaxTreeNotFoundTo, syntaxTree), "syntaxTree");
		}
		return new SyntaxTreeSemanticModel(this, syntaxTree);
	}

	internal static ITypeSymbol CreateErrorTypeSymbol(TypeSymbol container, string name)
	{
		return new ExtendedErrorTypeSymbol(container, name, null);
	}

	internal BinderFactory GetBinderFactory(SyntaxTree syntaxTree)
	{
		int syntaxTreeOrdinal = GetSyntaxTreeOrdinal(syntaxTree);
		WeakReference<BinderFactory>[] array = binderFactories;
		if (array == null)
		{
			array = new WeakReference<BinderFactory>[SyntaxTrees.Length];
			array = Interlocked.CompareExchange(ref binderFactories, array, null) ?? array;
		}
		WeakReference<BinderFactory> weakReference = array[syntaxTreeOrdinal];
		if (weakReference != null && weakReference.TryGetTarget(out var target))
		{
			return target;
		}
		return AddNewFactory(syntaxTree, ref array[syntaxTreeOrdinal]);
	}

	private BinderFactory AddNewFactory(SyntaxTree syntaxTree, ref WeakReference<BinderFactory> slot)
	{
		BinderFactory binderFactory = new BinderFactory(this, syntaxTree);
		WeakReference<BinderFactory> value = new WeakReference<BinderFactory>(binderFactory);
		WeakReference<BinderFactory> weakReference;
		do
		{
			weakReference = slot;
			if (weakReference != null && weakReference.TryGetTarget(out var target))
			{
				return target;
			}
		}
		while (Interlocked.CompareExchange(ref slot, value, weakReference) != weakReference);
		return binderFactory;
	}

	internal Binder GetBinder(SyntaxReference reference)
	{
		return GetBinderFactory(reference.SyntaxTree).GetBinder(reference.GetSyntax());
	}

	internal Binder GetBinder(SyntaxNode syntax)
	{
		return GetBinderFactory(syntax.SyntaxTree).GetBinder(syntax);
	}

	internal Binder GetBinder(SyntaxNode syntax, int position)
	{
		return GetBinderFactory(syntax.SyntaxTree).GetBinder(syntax, position);
	}

	internal static TypeSymbol GetNavTypeSymbol(NavTypeKind navTypeKind)
	{
		return TypeSymbol.GetNavTypeSymbol(navTypeKind);
	}

	internal static IGenericLanguageClassTypeSymbolFactory GetGenericNavTypeSymbolFactory(NavTypeKind navTypeKind)
	{
		return TypeSymbol.GetGenericNavTypeSymbolFactory(navTypeKind);
	}

	public Compilation WithModuleName(string moduleName)
	{
		return new Compilation(ModuleInfo.WithName(moduleName), Options, syntaxAndDeclarations, SymbolReferences, ReferenceLoader, ReferenceManager, FileSystem, LanguagesMetadata, null, DotNetResolverFactory);
	}

	public Compilation WithOptions(CompilationOptions options)
	{
		return new Compilation(ModuleInfo, options, syntaxAndDeclarations, SymbolReferences, ReferenceLoader, ReferenceManager, FileSystem, LanguagesMetadata, null, DotNetResolverFactory);
	}

	internal Compilation WithEventQueue(AsyncQueue<CompilationEvent> eventQueue)
	{
		return new Compilation(ModuleInfo, Options, syntaxAndDeclarations, SymbolReferences, ReferenceLoader, ReferenceManager, FileSystem, LanguagesMetadata, eventQueue, DotNetResolverFactory);
	}

	internal Compilation WithReferenceManager(IReferenceManager referenceManager)
	{
		return new Compilation(ModuleInfo, Options, syntaxAndDeclarations, SymbolReferences, ReferenceLoader, referenceManager, FileSystem, LanguagesMetadata, EventQueue, DotNetResolverFactory);
	}

	internal void CreateRadReferenceManager(ObjectChangeModelDefinition objectChangeModelDefinition, ModuleDefinition packagedModuleDefinition)
	{
		IReferenceManager referenceManager = ReferenceManager;
		ReferenceManager = new RadReferenceManager(referenceManager, ReferenceLoader, DotNetResolverFactory, objectChangeModelDefinition, CompiledModule.ReferenceModules, packagedModuleDefinition);
	}

	internal static AnalyzerDriver AnalyzerForLanguage(ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerManager analyzerManager)
	{
		Func<SyntaxTrivia, bool> isComment = (SyntaxTrivia trivia) => trivia.Kind == SyntaxKind.LineCommentTrivia || trivia.Kind == SyntaxKind.CommentTrivia;
		return new AnalyzerDriver(analyzers, (SyntaxNode n) => n.Kind, analyzerManager, isComment);
	}

	internal void SymbolDeclaredEvent(Symbol symbol)
	{
		if (EventQueue != null)
		{
			EventQueue.Enqueue(new SymbolDeclaredCompilationEvent(this, symbol));
		}
	}

	internal void EnsureCompilationEventQueueCompleted()
	{
		DebugAssertHelper.Assert(EventQueue != null);
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
		DebugAssertHelper.Assert(EventQueue != null);
		EventQueue.TryEnqueue(new CompilationCompletedEvent(this));
		EventQueue.PromiseNotToEnqueue();
		EventQueue.TryComplete();
	}

	internal void CompleteTrees(SyntaxTree filterTree)
	{
		if (EventQueue == null)
		{
			return;
		}
		if (filterTree != null)
		{
			CompleteTree(filterTree);
			return;
		}
		foreach (SyntaxTree syntaxTree in SyntaxTrees)
		{
			CompleteTree(syntaxTree);
		}
	}

	private void CompleteTree(SyntaxTree tree)
	{
		if (lazyCompilationUnitCompletedTrees == null)
		{
			Interlocked.CompareExchange(ref lazyCompilationUnitCompletedTrees, new HashSet<SyntaxTree>(), null);
		}
		lock (lazyCompilationUnitCompletedTrees)
		{
			if (lazyCompilationUnitCompletedTrees.Add(tree))
			{
				EventQueue.TryEnqueue(new CompilationUnitCompletedEvent(this, tree));
				if (lazyCompilationUnitCompletedTrees.Count == SyntaxTrees.Length)
				{
					CompleteCompilationEventQueue_NoLock();
				}
			}
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cancellationToken")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "filter")]
	public bool ContainsSymbolsWithName(Func<string, bool> predicate, SymbolFilter filter = SymbolFilter.TypeAndMember, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		throw ExceptionUtilities.SkippedForNow;
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "filter")]
	[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cancellationToken")]
	public IEnumerable<ISymbol> GetSymbolsWithName(Func<string, bool> predicate, SymbolFilter filter = SymbolFilter.TypeAndMember, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		throw ExceptionUtilities.SkippedForNow;
	}

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public ImmutableArray<IApplicationObjectTypeSymbol> GetDeclaredApplicationObjectSymbols()
	{
		return CompiledModule.GetDeclaredObjectSymbols().OfType<IApplicationObjectTypeSymbol>().ToImmutableArray();
	}

	public Compilation RemoveAllReferences()
	{
		return WithReferences(SpecializedCollections.EmptyEnumerable<SymbolReferenceSpecification>());
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "diagnostics")]
	internal static ModuleBuilder CreateModuleBuilder(EmitOptions emitOptions, DiagnosticBag diagnostics, CompilationOptions compilationOptions)
	{
		return new ModuleBuilder(emitOptions, compilationOptions);
	}

	internal IEnumerable<ISymbol> GetObjectSymbolsByKindAcrossModules(SymbolKind kind)
	{
		return ReferenceManager.GetObjectSymbolsByKindAcrossModules(CompiledModule, kind);
	}

	internal ImmutableArray<ISymbolWithId> GetObjectSymbolsByIdAcrossModules(SymbolKind kind, int id)
	{
		return ReferenceManager.GetObjectSymbolsByIdAcrossModules(CompiledModule, kind, id);
	}

	internal ImmutableArray<ISymbol> GetObjectSymbolsByNameAcrossModules(SymbolKind kind, string name)
	{
		return ReferenceManager.GetObjectSymbolsByNameAcrossModules(CompiledModule, kind, name);
	}

	internal IEnumerable<IApplicationObjectTypeSymbol> GetApplicationObjectTypeSymbols(SymbolKind kind)
	{
		return CompiledModule.GetObjectSymbols(kind).OfType<IApplicationObjectTypeSymbol>();
	}

	internal ImmutableArray<IApplicationObjectTypeSymbol> GetApplicationObjectTypeSymbolsAcrossModules(SymbolKind kind)
	{
		return GetObjectSymbolsByKindAcrossModules(kind).OfType<IApplicationObjectTypeSymbol>().ToImmutableArray();
	}

	internal ImmutableArray<IApplicationObjectTypeSymbol> GetApplicationObjectTypeSymbolsByIdAcrossModules(SymbolKind kind, int id)
	{
		return GetObjectSymbolsByIdAcrossModules(kind, id).OfType<IApplicationObjectTypeSymbol>().ToImmutableArray();
	}

	internal ImmutableArray<IApplicationObjectTypeSymbol> GetApplicationObjectTypeSymbolsByNameAcrossModules(SymbolKind kind, string name)
	{
		return GetObjectSymbolsByNameAcrossModules(kind, name).OfType<IApplicationObjectTypeSymbol>().ToImmutableArray();
	}

	internal ImmutableArray<IObjectTypeSymbol> GetObjectTypeSymbolsByNameAcrossModules(SymbolKind kind, string name)
	{
		return GetObjectSymbolsByNameAcrossModules(kind, name).OfType<IObjectTypeSymbol>().ToImmutableArray();
	}

	internal ImmutableArray<IApplicationObjectExtensionTypeSymbol> GetApplicationObjectExtensionTypeSymbolsAcrossModules(IApplicationObjectTypeSymbol symbol)
	{
		return CompiledModule.GetApplicationObjectExtensionTypeSymbolsAcrossModules(symbol);
	}
}
