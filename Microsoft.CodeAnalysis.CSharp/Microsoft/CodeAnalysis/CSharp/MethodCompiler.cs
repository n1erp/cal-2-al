using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Debugging;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class MethodCompiler : CSharpSymbolVisitor<TypeCompilationState, object>
{
	private readonly CSharpCompilation _compilation;

	private readonly bool _emittingPdb;

	private readonly bool _emitTestCoverageData;

	private readonly CancellationToken _cancellationToken;

	private readonly DiagnosticBag _diagnostics;

	private readonly bool _hasDeclarationErrors;

	private readonly PEModuleBuilder _moduleBeingBuiltOpt;

	private readonly Predicate<Symbol> _filterOpt;

	private readonly DebugDocumentProvider _debugDocumentProvider;

	private ConcurrentStack<Task> _compilerTasks;

	private bool _globalHasErrors;

	private void SetGlobalErrorIfTrue(bool arg)
	{
		if (arg)
		{
			_globalHasErrors = true;
		}
	}

	internal MethodCompiler(CSharpCompilation compilation, PEModuleBuilder moduleBeingBuiltOpt, bool emittingPdb, bool emitTestCoverageData, bool hasDeclarationErrors, DiagnosticBag diagnostics, Predicate<Symbol> filterOpt, CancellationToken cancellationToken)
	{
		_compilation = compilation;
		_moduleBeingBuiltOpt = moduleBeingBuiltOpt;
		_emittingPdb = emittingPdb;
		_cancellationToken = cancellationToken;
		_diagnostics = diagnostics;
		_filterOpt = filterOpt;
		_hasDeclarationErrors = hasDeclarationErrors;
		SetGlobalErrorIfTrue(hasDeclarationErrors);
		if (emittingPdb | emitTestCoverageData)
		{
			_debugDocumentProvider = (string path, string basePath) => moduleBeingBuiltOpt.DebugDocumentsBuilder.GetOrAddDebugDocument(path, basePath, CreateDebugDocumentForFile);
		}
		_emitTestCoverageData = emitTestCoverageData;
	}

	public static void CompileMethodBodies(CSharpCompilation compilation, PEModuleBuilder moduleBeingBuiltOpt, bool emittingPdb, bool emitTestCoverageData, bool hasDeclarationErrors, DiagnosticBag diagnostics, Predicate<Symbol> filterOpt, CancellationToken cancellationToken)
	{
		if (compilation.PreviousSubmission != null)
		{
			compilation.PreviousSubmission.EnsureAnonymousTypeTemplates(cancellationToken);
		}
		MethodCompiler methodCompiler = new MethodCompiler(compilation, moduleBeingBuiltOpt, emittingPdb, emitTestCoverageData, hasDeclarationErrors, diagnostics, filterOpt, cancellationToken);
		if (compilation.Options.ConcurrentBuild)
		{
			methodCompiler._compilerTasks = new ConcurrentStack<Task>();
		}
		methodCompiler.CompileNamespace(compilation.SourceModule.GlobalNamespace);
		methodCompiler.WaitForWorkers();
		if (moduleBeingBuiltOpt != null)
		{
			ImmutableArray<NamedTypeSymbol> additionalTopLevelTypes = moduleBeingBuiltOpt.GetAdditionalTopLevelTypes(diagnostics);
			methodCompiler.CompileSynthesizedMethods(additionalTopLevelTypes, diagnostics);
			ImmutableArray<NamedTypeSymbol> embeddedTypes = moduleBeingBuiltOpt.GetEmbeddedTypes(diagnostics);
			methodCompiler.CompileSynthesizedMethods(embeddedTypes, diagnostics);
			compilation.AnonymousTypeManager.AssignTemplatesNamesAndCompile(methodCompiler, moduleBeingBuiltOpt, diagnostics);
			methodCompiler.WaitForWorkers();
			PrivateImplementationDetails privateImplClass = moduleBeingBuiltOpt.PrivateImplClass;
			if (privateImplClass != null)
			{
				privateImplClass.Freeze();
				methodCompiler.CompileSynthesizedMethods(privateImplClass, diagnostics);
			}
		}
		if (moduleBeingBuiltOpt != null && (methodCompiler._globalHasErrors || moduleBeingBuiltOpt.SourceModule.HasBadAttributes) && !diagnostics.HasAnyErrors() && !hasDeclarationErrors)
		{
			diagnostics.Add(ErrorCode.ERR_ModuleEmitFailure, NoLocation.Singleton, ((INamedEntity)moduleBeingBuiltOpt).Name);
		}
		diagnostics.AddRange(compilation.AdditionalCodegenWarnings);
		if (filterOpt == null)
		{
			WarnUnusedFields(compilation, diagnostics, cancellationToken);
			MethodSymbol entryPoint = GetEntryPoint(compilation, moduleBeingBuiltOpt, hasDeclarationErrors, diagnostics, cancellationToken);
			if (moduleBeingBuiltOpt != null && entryPoint != null && compilation.Options.OutputKind.IsApplication())
			{
				moduleBeingBuiltOpt.SetPEEntryPoint(entryPoint, diagnostics);
			}
		}
	}

	private static MethodSymbol GetEntryPoint(CSharpCompilation compilation, PEModuleBuilder moduleBeingBuilt, bool hasDeclarationErrors, DiagnosticBag diagnostics, CancellationToken cancellationToken)
	{
		CSharpCompilation.EntryPoint entryPointAndDiagnostics = compilation.GetEntryPointAndDiagnostics(cancellationToken);
		if (entryPointAndDiagnostics == null)
		{
			return null;
		}
		diagnostics.AddRange(entryPointAndDiagnostics.Diagnostics);
		MethodSymbol methodSymbol = entryPointAndDiagnostics.MethodSymbol;
		if ((object)methodSymbol == null)
		{
			return null;
		}
		SynthesizedEntryPointSymbol synthesizedEntryPointSymbol = methodSymbol as SynthesizedEntryPointSymbol;
		if ((object)synthesizedEntryPointSymbol == null && (methodSymbol.ReturnType.IsGenericTaskType(compilation) || methodSymbol.ReturnType.IsNonGenericTaskType(compilation)))
		{
			synthesizedEntryPointSymbol = new SynthesizedEntryPointSymbol.AsyncForwardEntryPoint(compilation, methodSymbol.ContainingType, methodSymbol);
			methodSymbol = synthesizedEntryPointSymbol;
			moduleBeingBuilt?.AddSynthesizedDefinition(methodSymbol.ContainingType, synthesizedEntryPointSymbol);
		}
		if ((object)synthesizedEntryPointSymbol != null && moduleBeingBuilt != null && !hasDeclarationErrors && !diagnostics.HasAnyErrors())
		{
			BoundStatement boundStatement = synthesizedEntryPointSymbol.CreateBody(diagnostics);
			if (boundStatement.HasErrors || diagnostics.HasAnyErrors())
			{
				return methodSymbol;
			}
			ImmutableArray<SourceSpan> dynamicAnalysisSpans = ImmutableArray<SourceSpan>.Empty;
			VariableSlotAllocator lazyVariableSlotAllocator = null;
			ArrayBuilder<LambdaDebugInfo> instance = ArrayBuilder<LambdaDebugInfo>.GetInstance();
			ArrayBuilder<ClosureDebugInfo> instance2 = ArrayBuilder<ClosureDebugInfo>.GetInstance();
			StateMachineTypeSymbol stateMachineTypeOpt = null;
			BoundStatement block = LowerBodyOrInitializer(synthesizedEntryPointSymbol, -1, boundStatement, null, new TypeCompilationState(synthesizedEntryPointSymbol.ContainingType, compilation, moduleBeingBuilt), instrumentForDynamicAnalysis: false, null, ref dynamicAnalysisSpans, diagnostics, ref lazyVariableSlotAllocator, instance, instance2, out stateMachineTypeOpt);
			instance.Free();
			instance2.Free();
			MethodBody body = GenerateMethodBody(moduleBeingBuilt, synthesizedEntryPointSymbol, -1, block, ImmutableArray<LambdaDebugInfo>.Empty, ImmutableArray<ClosureDebugInfo>.Empty, null, null, diagnostics, null, null, emittingPdb: false, emitTestCoverageData: false, ImmutableArray<SourceSpan>.Empty);
			moduleBeingBuilt.SetMethodBody(synthesizedEntryPointSymbol, body);
		}
		return methodSymbol;
	}

	private void WaitForWorkers()
	{
		ConcurrentStack<Task> compilerTasks = _compilerTasks;
		if (compilerTasks != null)
		{
			Task result;
			while (compilerTasks.TryPop(out result))
			{
				result.GetAwaiter().GetResult();
			}
		}
	}

	private static void WarnUnusedFields(CSharpCompilation compilation, DiagnosticBag diagnostics, CancellationToken cancellationToken)
	{
		SourceAssemblySymbol sourceAssemblySymbol = (SourceAssemblySymbol)compilation.Assembly;
		diagnostics.AddRange(sourceAssemblySymbol.GetUnusedFieldWarnings(cancellationToken));
	}

	public override object VisitNamespace(NamespaceSymbol symbol, TypeCompilationState arg)
	{
		if (!PassesFilter(_filterOpt, symbol))
		{
			return null;
		}
		arg = null;
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		if (_compilation.Options.ConcurrentBuild)
		{
			Task item = CompileNamespaceAsTask(symbol);
			_compilerTasks.Push(item);
		}
		else
		{
			CompileNamespace(symbol);
		}
		return null;
	}

	private Task CompileNamespaceAsTask(NamespaceSymbol symbol)
	{
		return Task.Run(UICultureUtilities.WithCurrentUICulture(() =>
		{
			try
			{
				CompileNamespace(symbol);
			}
			catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
			{
				throw ExceptionUtilities.Unreachable;
			}
		}), _cancellationToken);
	}

	private void CompileNamespace(NamespaceSymbol symbol)
	{
		foreach (Symbol item in symbol.GetMembersUnordered())
		{
			item.Accept(this, null);
		}
	}

	public override object VisitNamedType(NamedTypeSymbol symbol, TypeCompilationState arg)
	{
		if (!PassesFilter(_filterOpt, symbol))
		{
			return null;
		}
		arg = null;
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		if (_compilation.Options.ConcurrentBuild)
		{
			Task item = CompileNamedTypeAsTask(symbol);
			_compilerTasks.Push(item);
		}
		else
		{
			CompileNamedType(symbol);
		}
		return null;
	}

	private Task CompileNamedTypeAsTask(NamedTypeSymbol symbol)
	{
		return Task.Run(UICultureUtilities.WithCurrentUICulture(() =>
		{
			try
			{
				CompileNamedType(symbol);
			}
			catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
			{
				throw ExceptionUtilities.Unreachable;
			}
		}), _cancellationToken);
	}

	private void CompileNamedType(NamedTypeSymbol containingType)
	{
		TypeCompilationState typeCompilationState = new TypeCompilationState(containingType, _compilation, _moduleBeingBuiltOpt);
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		SynthesizedInstanceConstructor synthesizedInstanceConstructor = null;
		SynthesizedInteractiveInitializerMethod scriptInitializerOpt = null;
		SynthesizedEntryPointSymbol synthesizedEntryPointSymbol = null;
		int methodOrdinal = -1;
		if (containingType.IsScriptClass)
		{
			synthesizedInstanceConstructor = containingType.GetScriptConstructor();
			scriptInitializerOpt = containingType.GetScriptInitializer();
			synthesizedEntryPointSymbol = containingType.GetScriptEntryPoint();
		}
		SynthesizedSubmissionFields synthesizedSubmissionFields = (containingType.IsSubmissionClass ? new SynthesizedSubmissionFields(_compilation, containingType) : null);
		Binder.ProcessedFieldInitializers processedInitializers = default;
		Binder.ProcessedFieldInitializers processedInitializers2 = default;
		SourceMemberContainerTypeSymbol sourceMemberContainerTypeSymbol = containingType as SourceMemberContainerTypeSymbol;
		if ((object)sourceMemberContainerTypeSymbol != null)
		{
			cancellationToken = _cancellationToken;
			cancellationToken.ThrowIfCancellationRequested();
			Binder.BindFieldInitializers(_compilation, scriptInitializerOpt, sourceMemberContainerTypeSymbol.StaticInitializers, _diagnostics, ref processedInitializers);
			cancellationToken = _cancellationToken;
			cancellationToken.ThrowIfCancellationRequested();
			Binder.BindFieldInitializers(_compilation, scriptInitializerOpt, sourceMemberContainerTypeSymbol.InstanceInitializers, _diagnostics, ref processedInitializers2);
			if (typeCompilationState.Emitting)
			{
				CompileSynthesizedExplicitImplementations(sourceMemberContainerTypeSymbol, typeCompilationState);
			}
		}
		bool flag = false;
		ImmutableArray<Symbol> members = containingType.GetMembers();
		for (int i = 0; i < members.Length; i++)
		{
			Symbol symbol = members[i];
			if (!PassesFilter(_filterOpt, symbol))
			{
				continue;
			}
			switch (symbol.Kind)
			{
			case SymbolKind.NamedType:
				symbol.Accept(this, typeCompilationState);
				break;
			case SymbolKind.Method:
			{
				MethodSymbol methodSymbol = (MethodSymbol)symbol;
				if (methodSymbol.IsScriptConstructor)
				{
					methodOrdinal = i;
				}
				else
				{
					if ((object)methodSymbol == synthesizedEntryPointSymbol || IsFieldLikeEventAccessor(methodSymbol))
					{
						break;
					}
					if (methodSymbol.IsPartialDefinition())
					{
						methodSymbol = methodSymbol.PartialImplementationPart;
						if ((object)methodSymbol == null)
						{
							break;
						}
					}
					Binder.ProcessedFieldInitializers processedInitializers3;
					if (methodSymbol.MethodKind == MethodKind.Constructor || methodSymbol.IsScriptInitializer)
					{
						processedInitializers3 = processedInitializers2;
					}
					else
					{
						processedInitializers3 = ((methodSymbol.MethodKind == MethodKind.StaticConstructor) ? processedInitializers : default(Binder.ProcessedFieldInitializers));
					}
					CompileMethod(methodSymbol, i, ref processedInitializers3, synthesizedSubmissionFields, typeCompilationState);
					if (methodSymbol.MethodKind == MethodKind.StaticConstructor)
					{
						flag = true;
					}
				}
				break;
			}
			case SymbolKind.Property:
				if (symbol is SourcePropertySymbol { IsSealed: not false } sourcePropertySymbol && typeCompilationState.Emitting)
				{
					CompileSynthesizedSealedAccessors(sourcePropertySymbol, typeCompilationState);
				}
				break;
			case SymbolKind.Event:
				if (symbol is SourceEventSymbol { HasAssociatedField: not false, IsAbstract: false } sourceEventSymbol && typeCompilationState.Emitting)
				{
					CompileFieldLikeEventAccessor(sourceEventSymbol, isAddMethod: true);
					CompileFieldLikeEventAccessor(sourceEventSymbol, isAddMethod: false);
				}
				break;
			case SymbolKind.Field:
				if (symbol is SourceMemberFieldSymbol sourceMemberFieldSymbol)
				{
					if (sourceMemberFieldSymbol.IsConst)
					{
						ConstantValue constantValue = sourceMemberFieldSymbol.GetConstantValue(ConstantFieldsInProgress.Empty, earlyDecodingWellKnownAttributes: false);
						SetGlobalErrorIfTrue(constantValue == null || constantValue.IsBad);
					}
					if (sourceMemberFieldSymbol.IsFixed && typeCompilationState.Emitting)
					{
						sourceMemberFieldSymbol.FixedImplementationType(typeCompilationState.ModuleBuilderOpt);
					}
				}
				break;
			}
		}
		if (AnonymousTypeManager.IsAnonymousTypeTemplate(containingType))
		{
			Binder.ProcessedFieldInitializers processedInitializers4 = default;
			foreach (MethodSymbol anonymousTypeHiddenMethod in AnonymousTypeManager.GetAnonymousTypeHiddenMethods(containingType))
			{
				CompileMethod(anonymousTypeHiddenMethod, -1, ref processedInitializers4, synthesizedSubmissionFields, typeCompilationState);
			}
		}
		if (_moduleBeingBuiltOpt != null && !flag && !processedInitializers.BoundInitializers.IsDefaultOrEmpty)
		{
			MethodSymbol methodSymbol2 = new SynthesizedStaticConstructor(sourceMemberContainerTypeSymbol);
			if (PassesFilter(_filterOpt, methodSymbol2))
			{
				CompileMethod(methodSymbol2, -1, ref processedInitializers, synthesizedSubmissionFields, typeCompilationState);
				if (_moduleBeingBuiltOpt.GetMethodBody(methodSymbol2) != null)
				{
					_moduleBeingBuiltOpt.AddSynthesizedDefinition(sourceMemberContainerTypeSymbol, methodSymbol2);
				}
			}
		}
		if (synthesizedInstanceConstructor != null && typeCompilationState.Emitting)
		{
			Binder.ProcessedFieldInitializers processedInitializers5 = new Binder.ProcessedFieldInitializers
			{
				BoundInitializers = ImmutableArray<BoundInitializer>.Empty
			};
			CompileMethod(synthesizedInstanceConstructor, methodOrdinal, ref processedInitializers5, synthesizedSubmissionFields, typeCompilationState);
			synthesizedSubmissionFields?.AddToType(containingType, typeCompilationState.ModuleBuilderOpt);
		}
		if (_moduleBeingBuiltOpt != null)
		{
			CompileSynthesizedMethods(typeCompilationState);
		}
		typeCompilationState.Free();
	}

	private void CompileSynthesizedMethods(PrivateImplementationDetails privateImplClass, DiagnosticBag diagnostics)
	{
		TypeCompilationState typeCompilationState = new TypeCompilationState(null, _compilation, _moduleBeingBuiltOpt);
		foreach (MethodSymbol method in privateImplClass.GetMethods(new EmitContext(_moduleBeingBuiltOpt, null, diagnostics, metadataOnly: false, includePrivateMembers: true)))
		{
			method.GenerateMethodBody(typeCompilationState, diagnostics);
		}
		CompileSynthesizedMethods(typeCompilationState);
		typeCompilationState.Free();
	}

	private void CompileSynthesizedMethods(ImmutableArray<NamedTypeSymbol> additionalTypes, DiagnosticBag diagnostics)
	{
		foreach (NamedTypeSymbol item in additionalTypes)
		{
			TypeCompilationState typeCompilationState = new TypeCompilationState(item, _compilation, _moduleBeingBuiltOpt);
			foreach (MethodSymbol item2 in item.GetMethodsToEmit())
			{
				item2.GenerateMethodBody(typeCompilationState, diagnostics);
			}
			if (!diagnostics.HasAnyErrors())
			{
				CompileSynthesizedMethods(typeCompilationState);
			}
			typeCompilationState.Free();
		}
	}

	private void CompileSynthesizedMethods(TypeCompilationState compilationState)
	{
		ArrayBuilder<TypeCompilationState.MethodWithBody> synthesizedMethods = compilationState.SynthesizedMethods;
		if (synthesizedMethods == null)
		{
			return;
		}
		ImportChain currentImportChain = compilationState.CurrentImportChain;
		try
		{
			foreach (TypeCompilationState.MethodWithBody item in synthesizedMethods)
			{
				ImportChain importChain = (compilationState.CurrentImportChain = item.ImportChainOpt);
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				MethodSymbol method = item.Method;
				VariableSlotAllocator variableSlotAllocator = ((method is SynthesizedClosureMethod synthesizedClosureMethod) ? _moduleBeingBuiltOpt.TryCreateVariableSlotAllocator(synthesizedClosureMethod, synthesizedClosureMethod.TopLevelMethod, instance) : _moduleBeingBuiltOpt.TryCreateVariableSlotAllocator(method, method, instance));
				MethodBody methodBody = null;
				try
				{
					BoundStatement boundStatement = IteratorRewriter.Rewrite(item.Body, method, -1, variableSlotAllocator, compilationState, instance, out var stateMachineType);
					StateMachineTypeSymbol stateMachineTypeSymbol = stateMachineType;
					if (!boundStatement.HasErrors)
					{
						boundStatement = AsyncRewriter.Rewrite(boundStatement, method, -1, variableSlotAllocator, compilationState, instance, out var stateMachineType2);
						stateMachineTypeSymbol = stateMachineTypeSymbol ?? stateMachineType2;
					}
					if (!instance.HasAnyErrors() && !_globalHasErrors)
					{
						methodBody = GenerateMethodBody(_moduleBeingBuiltOpt, method, -1, boundStatement, ImmutableArray<LambdaDebugInfo>.Empty, ImmutableArray<ClosureDebugInfo>.Empty, stateMachineTypeSymbol, variableSlotAllocator, instance, _debugDocumentProvider, method.GenerateDebugInfo ? importChain : null, _emittingPdb, _emitTestCoverageData, ImmutableArray<SourceSpan>.Empty);
					}
				}
				catch (BoundTreeVisitor.CancelledByStackGuardException ex)
				{
					ex.AddAnError(_diagnostics);
				}
				_diagnostics.AddRange(instance);
				instance.Free();
				if (methodBody == null)
				{
					break;
				}
				_moduleBeingBuiltOpt.SetMethodBody(method, methodBody);
			}
		}
		finally
		{
			compilationState.CurrentImportChain = currentImportChain;
		}
	}

	private static bool IsFieldLikeEventAccessor(MethodSymbol method)
	{
		Symbol associatedSymbol = method.AssociatedSymbol;
		if ((object)associatedSymbol != null && associatedSymbol.Kind == SymbolKind.Event)
		{
			return ((EventSymbol)associatedSymbol).HasAssociatedField;
		}
		return false;
	}

	private void CompileSynthesizedExplicitImplementations(SourceMemberContainerTypeSymbol sourceTypeSymbol, TypeCompilationState compilationState)
	{
		if (!_globalHasErrors)
		{
			foreach (SynthesizedExplicitImplementationForwardingMethod synthesizedExplicitImplementation in sourceTypeSymbol.GetSynthesizedExplicitImplementations(_cancellationToken))
			{
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				synthesizedExplicitImplementation.GenerateMethodBody(compilationState, instance);
				instance.Free();
				_moduleBeingBuiltOpt.AddSynthesizedDefinition(sourceTypeSymbol, synthesizedExplicitImplementation);
			}
		}
	}

	private void CompileSynthesizedSealedAccessors(SourcePropertySymbol sourceProperty, TypeCompilationState compilationState)
	{
		SynthesizedSealedPropertyAccessor synthesizedSealedAccessorOpt = sourceProperty.SynthesizedSealedAccessorOpt;
		if ((object)synthesizedSealedAccessorOpt != null && !_globalHasErrors)
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			synthesizedSealedAccessorOpt.GenerateMethodBody(compilationState, instance);
			instance.Free();
			_moduleBeingBuiltOpt.AddSynthesizedDefinition(sourceProperty.ContainingType, synthesizedSealedAccessorOpt);
		}
	}

	private void CompileFieldLikeEventAccessor(SourceEventSymbol eventSymbol, bool isAddMethod)
	{
		MethodSymbol methodSymbol = (isAddMethod ? eventSymbol.AddMethod : eventSymbol.RemoveMethod);
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		try
		{
			BoundBlock block = MethodBodySynthesizer.ConstructFieldLikeEventAccessorBody(eventSymbol, isAddMethod, _compilation, instance);
			bool flag = instance.HasAnyErrors();
			SetGlobalErrorIfTrue(flag);
			if (!flag && !_hasDeclarationErrors)
			{
				MethodBody body = GenerateMethodBody(_moduleBeingBuiltOpt, methodSymbol, -1, block, ImmutableArray<LambdaDebugInfo>.Empty, ImmutableArray<ClosureDebugInfo>.Empty, null, null, instance, _debugDocumentProvider, null, emittingPdb: false, _emitTestCoverageData, ImmutableArray<SourceSpan>.Empty);
				_moduleBeingBuiltOpt.SetMethodBody(methodSymbol, body);
			}
		}
		finally
		{
			_diagnostics.AddRange(instance);
			instance.Free();
		}
	}

	public override object VisitMethod(MethodSymbol symbol, TypeCompilationState arg)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override object VisitProperty(PropertySymbol symbol, TypeCompilationState argument)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override object VisitEvent(EventSymbol symbol, TypeCompilationState argument)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override object VisitField(FieldSymbol symbol, TypeCompilationState argument)
	{
		throw ExceptionUtilities.Unreachable;
	}

	private void CompileMethod(MethodSymbol methodSymbol, int methodOrdinal, ref Binder.ProcessedFieldInitializers processedInitializers, SynthesizedSubmissionFields previousSubmissionFields, TypeCompilationState compilationState)
	{
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		SourceMemberMethodSymbol sourceMemberMethodSymbol = methodSymbol as SourceMemberMethodSymbol;
		if (methodSymbol.IsAbstract)
		{
			if ((object)sourceMemberMethodSymbol != null)
			{
				sourceMemberMethodSymbol.SetDiagnostics(ImmutableArray<Diagnostic>.Empty, out var diagsWritten);
				if (diagsWritten && !methodSymbol.IsImplicitlyDeclared && _compilation.EventQueue != null)
				{
					_compilation.SymbolDeclaredEvent(methodSymbol);
				}
			}
			return;
		}
		if (_moduleBeingBuiltOpt == null && (object)sourceMemberMethodSymbol != null)
		{
			ImmutableArray<Diagnostic> diagnostics = sourceMemberMethodSymbol.Diagnostics;
			if (!diagnostics.IsDefault)
			{
				_diagnostics.AddRange(diagnostics);
				return;
			}
		}
		ImportChain currentImportChain = compilationState.CurrentImportChain;
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		try
		{
			if (methodSymbol.SynthesizesLoweredBoundBody)
			{
				if (_moduleBeingBuiltOpt != null)
				{
					methodSymbol.GenerateMethodBody(compilationState, instance);
					_diagnostics.AddRange(instance);
				}
			}
			else
			{
				if (methodSymbol.IsDefaultValueTypeConstructor())
				{
					return;
				}
				bool flag = false;
				bool originalBodyNested = false;
				BoundStatementList boundStatementList = null;
				(SyntaxNode, BoundNode, ExecutableCodeBinder) forSemanticModel = default;
				ImportChain importChain = null;
				bool hasTrailingExpression = false;
				BoundBlock body;
				if (methodSymbol.IsScriptConstructor)
				{
					body = new BoundBlock(methodSymbol.GetNonNullSyntaxNode(), ImmutableArray<LocalSymbol>.Empty, ImmutableArray<BoundStatement>.Empty)
					{
						WasCompilerGenerated = true
					};
				}
				else if (methodSymbol.IsScriptInitializer)
				{
					BoundTypeOrInstanceInitializers boundTypeOrInstanceInitializers = InitializerRewriter.RewriteScriptInitializer(processedInitializers.BoundInitializers, (SynthesizedInteractiveInitializerMethod)methodSymbol, out hasTrailingExpression);
					body = BoundBlock.SynthesizedNoLocals(boundTypeOrInstanceInitializers.Syntax, boundTypeOrInstanceInitializers.Statements);
					DiagnosticBag instance2 = DiagnosticBag.GetInstance();
					DataFlowPass.Analyze(_compilation, methodSymbol, boundTypeOrInstanceInitializers, instance2, requireOutParamsAssigned: false);
					DiagnosticsPass.IssueDiagnostics(_compilation, boundTypeOrInstanceInitializers, instance2, methodSymbol);
					instance2.Free();
				}
				else
				{
					flag = !processedInitializers.BoundInitializers.IsDefaultOrEmpty && !HasThisConstructorInitializer(methodSymbol);
					body = BindMethodBody(methodSymbol, compilationState, instance, out importChain, out originalBodyNested, out forSemanticModel);
					if (flag && processedInitializers.LoweredInitializers == null)
					{
						boundStatementList = InitializerRewriter.RewriteConstructor(processedInitializers.BoundInitializers, methodSymbol);
						processedInitializers.HasErrors = processedInitializers.HasErrors || boundStatementList.HasAnyErrors;
						if (body != null && ((methodSymbol.ContainingType.IsStructType() && !methodSymbol.IsImplicitConstructor) || _emitTestCoverageData))
						{
							if (_emitTestCoverageData && methodSymbol.IsImplicitConstructor)
							{
								DataFlowPass.Analyze(_compilation, methodSymbol, boundStatementList, instance, requireOutParamsAssigned: false);
							}
							body = body.Update(body.Locals, body.LocalFunctions, body.Statements.Insert(0, boundStatementList));
							flag = false;
							boundStatementList = null;
						}
						else
						{
							DataFlowPass.Analyze(_compilation, methodSymbol, boundStatementList, instance, requireOutParamsAssigned: false);
							DiagnosticsPass.IssueDiagnostics(_compilation, boundStatementList, instance, methodSymbol);
						}
					}
				}
				importChain = (compilationState.CurrentImportChain = importChain ?? processedInitializers.FirstImportChain);
				if (body != null)
				{
					DiagnosticsPass.IssueDiagnostics(_compilation, body, instance, methodSymbol);
				}
				BoundBlock boundBlock = null;
				if (body != null)
				{
					boundBlock = FlowAnalysisPass.Rewrite(methodSymbol, body, instance, hasTrailingExpression, originalBodyNested);
				}
				bool flag2 = _hasDeclarationErrors || instance.HasAnyErrors() || processedInitializers.HasErrors;
				SetGlobalErrorIfTrue(flag2);
				bool diagsWritten2 = false;
				ImmutableArray<Diagnostic> immutableArray = instance.ToReadOnly();
				if (sourceMemberMethodSymbol != null)
				{
					immutableArray = sourceMemberMethodSymbol.SetDiagnostics(immutableArray, out diagsWritten2);
				}
				if (diagsWritten2 && !methodSymbol.IsImplicitlyDeclared && _compilation.EventQueue != null)
				{
					Lazy<SemanticModel> lazySemanticModel = null;
					if (body != null)
					{
						(SyntaxNode Syntax, BoundNode Body, ExecutableCodeBinder Binder) forSemanticModelToUseInLambda = forSemanticModel;
						lazySemanticModel = new Lazy<SemanticModel>(() =>
						{
							SyntaxNode syntax = body.Syntax;
							SyntaxTreeSemanticModel semanticModel = (SyntaxTreeSemanticModel)_compilation.GetSemanticModel(syntax.SyntaxTree);
							if (forSemanticModelToUseInLambda.Syntax != null)
							{
								semanticModel.GetOrAddModel((CSharpSyntaxNode)forSemanticModelToUseInLambda.Syntax, (CSharpSyntaxNode rootSyntax) => MethodBodySemanticModel.Create(semanticModel, methodSymbol, forSemanticModelToUseInLambda.Binder, rootSyntax, forSemanticModelToUseInLambda.Body));
							}
							return semanticModel;
						});
					}
					_compilation.EventQueue.TryEnqueue(new SymbolDeclaredCompilationEvent(_compilation, methodSymbol, lazySemanticModel));
				}
				if (!((_moduleBeingBuiltOpt == null) | flag2))
				{
					ImmutableArray<SourceSpan> dynamicAnalysisSpans = ImmutableArray<SourceSpan>.Empty;
					bool flag3 = boundBlock != null;
					VariableSlotAllocator lazyVariableSlotAllocator = null;
					StateMachineTypeSymbol stateMachineTypeOpt = null;
					ArrayBuilder<LambdaDebugInfo> instance3 = ArrayBuilder<LambdaDebugInfo>.GetInstance();
					ArrayBuilder<ClosureDebugInfo> instance4 = ArrayBuilder<ClosureDebugInfo>.GetInstance();
					try
					{
						BoundStatement boundStatement = ((!flag3) ? null : LowerBodyOrInitializer(methodSymbol, methodOrdinal, boundBlock, previousSubmissionFields, compilationState, _emitTestCoverageData, _debugDocumentProvider, ref dynamicAnalysisSpans, instance, ref lazyVariableSlotAllocator, instance3, instance4, out stateMachineTypeOpt));
						flag2 = flag2 || (flag3 && boundStatement.HasErrors) || instance.HasAnyErrors();
						SetGlobalErrorIfTrue(flag2);
						if (!flag2 && (flag3 | flag))
						{
							ImmutableArray<BoundStatement> immutableArray2;
							if (methodSymbol.IsScriptConstructor)
							{
								immutableArray2 = MethodBodySynthesizer.ConstructScriptConstructorBody(boundStatement, methodSymbol, previousSubmissionFields, _compilation);
							}
							else
							{
								immutableArray2 = ImmutableArray<BoundStatement>.Empty;
								if (boundStatementList != null)
								{
									BoundStatement boundStatement2 = (processedInitializers.LoweredInitializers = LowerBodyOrInitializer(methodSymbol, methodOrdinal, boundStatementList, previousSubmissionFields, compilationState, _emitTestCoverageData, _debugDocumentProvider, ref dynamicAnalysisSpans, instance, ref lazyVariableSlotAllocator, instance3, instance4, out var _));
									flag2 = boundStatement2.HasAnyErrors || instance.HasAnyErrors();
									SetGlobalErrorIfTrue(flag2);
									if (flag2)
									{
										_diagnostics.AddRange(instance);
										return;
									}
									processedInitializers.LoweredInitializers = (BoundStatementList)boundStatement2;
								}
								if (flag)
								{
									if (processedInitializers.LoweredInitializers.Kind == BoundKind.StatementList)
									{
										BoundStatementList boundStatementList2 = (BoundStatementList)processedInitializers.LoweredInitializers;
										immutableArray2 = immutableArray2.Concat(boundStatementList2.Statements);
									}
									else
									{
										immutableArray2 = immutableArray2.Add(processedInitializers.LoweredInitializers);
									}
								}
								if (flag3)
								{
									immutableArray2 = immutableArray2.Concat(ImmutableArray.Create(boundStatement));
								}
							}
							BoundStatementList block = BoundStatementList.Synthesized(methodSymbol.GetNonNullSyntaxNode(), immutableArray2);
							MethodBody body2 = GenerateMethodBody(_moduleBeingBuiltOpt, methodSymbol, methodOrdinal, block, instance3.ToImmutable(), instance4.ToImmutable(), stateMachineTypeOpt, lazyVariableSlotAllocator, instance, _debugDocumentProvider, importChain, _emittingPdb, _emitTestCoverageData, dynamicAnalysisSpans);
							_moduleBeingBuiltOpt.SetMethodBody(methodSymbol.PartialDefinitionPart ?? methodSymbol, body2);
						}
						_diagnostics.AddRange(instance);
						return;
					}
					finally
					{
						instance3.Free();
						instance4.Free();
					}
				}
				_diagnostics.AddRange(immutableArray);
			}
		}
		finally
		{
			instance.Free();
			compilationState.CurrentImportChain = currentImportChain;
		}
	}

	internal static BoundStatement LowerBodyOrInitializer(MethodSymbol method, int methodOrdinal, BoundStatement body, SynthesizedSubmissionFields previousSubmissionFields, TypeCompilationState compilationState, bool instrumentForDynamicAnalysis, DebugDocumentProvider debugDocumentProvider, ref ImmutableArray<SourceSpan> dynamicAnalysisSpans, DiagnosticBag diagnostics, ref VariableSlotAllocator lazyVariableSlotAllocator, ArrayBuilder<LambdaDebugInfo> lambdaDebugInfoBuilder, ArrayBuilder<ClosureDebugInfo> closureDebugInfoBuilder, out StateMachineTypeSymbol stateMachineTypeOpt)
	{
		stateMachineTypeOpt = null;
		if (body.HasErrors)
		{
			return body;
		}
		try
		{
			BoundStatement boundStatement = LocalRewriter.Rewrite(method.DeclaringCompilation, method, methodOrdinal, method.ContainingType, body, compilationState, previousSubmissionFields, allowOmissionOfConditionalCalls: true, instrumentForDynamicAnalysis, ref dynamicAnalysisSpans, debugDocumentProvider, diagnostics, out var sawLambdas, out var sawLocalFunctions, out var sawAwaitInExceptionHandler);
			if (boundStatement.HasErrors)
			{
				return boundStatement;
			}
			if (sawAwaitInExceptionHandler)
			{
				boundStatement = AsyncExceptionHandlerRewriter.Rewrite(method, method.ContainingType, boundStatement, compilationState, diagnostics);
			}
			if (boundStatement.HasErrors)
			{
				return boundStatement;
			}
			if (lazyVariableSlotAllocator == null)
			{
				lazyVariableSlotAllocator = compilationState.ModuleBuilderOpt.TryCreateVariableSlotAllocator(method, method, diagnostics);
			}
			BoundStatement boundStatement2 = boundStatement;
			if (sawLambdas | sawLocalFunctions)
			{
				boundStatement2 = LambdaRewriter.Rewrite(boundStatement, method.ContainingType, method.ThisParameter, method, methodOrdinal, null, lambdaDebugInfoBuilder, closureDebugInfoBuilder, lazyVariableSlotAllocator, compilationState, diagnostics, null);
			}
			if (boundStatement2.HasErrors)
			{
				return boundStatement2;
			}
			BoundStatement boundStatement3 = IteratorRewriter.Rewrite(boundStatement2, method, methodOrdinal, lazyVariableSlotAllocator, compilationState, diagnostics, out var stateMachineType);
			if (boundStatement3.HasErrors)
			{
				return boundStatement3;
			}
			BoundStatement result = AsyncRewriter.Rewrite(boundStatement3, method, methodOrdinal, lazyVariableSlotAllocator, compilationState, diagnostics, out var stateMachineType2);
			stateMachineTypeOpt = (StateMachineTypeSymbol)(((object)stateMachineType) ?? ((object)stateMachineType2));
			return result;
		}
		catch (BoundTreeVisitor.CancelledByStackGuardException ex)
		{
			ex.AddAnError(diagnostics);
			return new BoundBadStatement(body.Syntax, ImmutableArray.Create((BoundNode)body), hasErrors: true);
		}
	}

	private static MethodBody GenerateMethodBody(PEModuleBuilder moduleBuilder, MethodSymbol method, int methodOrdinal, BoundStatement block, ImmutableArray<LambdaDebugInfo> lambdaDebugInfo, ImmutableArray<ClosureDebugInfo> closureDebugInfo, StateMachineTypeSymbol stateMachineTypeOpt, VariableSlotAllocator variableSlotAllocatorOpt, DiagnosticBag diagnostics, DebugDocumentProvider debugDocumentProvider, ImportChain importChainOpt, bool emittingPdb, bool emitTestCoverageData, ImmutableArray<SourceSpan> dynamicAnalysisSpans)
	{
		CSharpCompilation compilation = moduleBuilder.Compilation;
		LocalSlotManager localSlotManager = new LocalSlotManager(variableSlotAllocatorOpt);
		OptimizationLevel optimizationLevel = compilation.Options.OptimizationLevel;
		ILBuilder iLBuilder = new ILBuilder(moduleBuilder, localSlotManager, optimizationLevel);
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		try
		{
			StateMachineMoveNextBodyDebugInfo stateMachineMoveNextDebugInfoOpt = null;
			CodeGenerator codeGenerator = new CodeGenerator(method, block, iLBuilder, moduleBuilder, instance, optimizationLevel, emittingPdb);
			if (instance.HasAnyErrors())
			{
				return null;
			}
			bool flag;
			MethodSymbol kickoffMethod;
			if (method is SynthesizedStateMachineMethod synthesizedStateMachineMethod && method.Name == "MoveNext")
			{
				kickoffMethod = synthesizedStateMachineMethod.StateMachineType.KickoffMethod;
				flag = kickoffMethod.IsAsync;
				kickoffMethod = kickoffMethod.PartialDefinitionPart ?? kickoffMethod;
			}
			else
			{
				kickoffMethod = null;
				flag = false;
			}
			if (flag)
			{
				codeGenerator.Generate(out var asyncCatchHandlerOffset, out var asyncYieldPoints, out var asyncResumePoints);
				stateMachineMoveNextDebugInfoOpt = new AsyncMoveNextBodyDebugInfo(kickoffMethod, kickoffMethod.ReturnsVoid ? asyncCatchHandlerOffset : (-1), asyncYieldPoints, asyncResumePoints);
			}
			else
			{
				codeGenerator.Generate();
				if ((object)kickoffMethod != null)
				{
					stateMachineMoveNextDebugInfoOpt = new IteratorMoveNextBodyDebugInfo(kickoffMethod);
				}
			}
			ImmutableArray<StateMachineHoistedLocalScope> stateMachineHoistedLocalScopes = (((object)kickoffMethod != null) ? iLBuilder.GetHoistedLocalScopes() : default(ImmutableArray<StateMachineHoistedLocalScope>));
			IImportScope importScopeOpt = importChainOpt?.Translate(moduleBuilder, instance);
			ImmutableArray<ILocalDefinition> locals = iLBuilder.LocalSlotManager.LocalsInOrder();
			if (locals.Length > 65534)
			{
				instance.Add(ErrorCode.ERR_TooManyLocals, method.Locations.First());
			}
			if (instance.HasAnyErrors())
			{
				return null;
			}
			if (moduleBuilder.SaveTestData)
			{
				moduleBuilder.SetMethodTestData(method, iLBuilder.GetSnapshot());
			}
			ImmutableArray<EncHoistedLocalInfo> hoistedVariableSlots = default;
			ImmutableArray<ITypeReference> awaiterSlots = default;
			if (optimizationLevel == OptimizationLevel.Debug && stateMachineTypeOpt != null)
			{
				GetStateMachineSlotDebugInfo(moduleBuilder, moduleBuilder.GetSynthesizedFields(stateMachineTypeOpt), variableSlotAllocatorOpt, instance, out hoistedVariableSlots, out awaiterSlots);
			}
			DynamicAnalysisMethodBodyData dynamicAnalysisDataOpt = null;
			if (emitTestCoverageData)
			{
				dynamicAnalysisDataOpt = new DynamicAnalysisMethodBodyData(dynamicAnalysisSpans);
			}
			return new MethodBody(iLBuilder.RealizedIL, iLBuilder.MaxStack, method.PartialDefinitionPart ?? method, variableSlotAllocatorOpt?.MethodId ?? new DebugId(methodOrdinal, moduleBuilder.CurrentGenerationOrdinal), locals, iLBuilder.RealizedSequencePoints, debugDocumentProvider, iLBuilder.RealizedExceptionHandlers, iLBuilder.GetAllScopes(), iLBuilder.HasDynamicLocal, importScopeOpt, lambdaDebugInfo, closureDebugInfo, stateMachineTypeOpt?.Name, stateMachineHoistedLocalScopes, hoistedVariableSlots, awaiterSlots, stateMachineMoveNextDebugInfoOpt, dynamicAnalysisDataOpt);
		}
		finally
		{
			iLBuilder.FreeBasicBlocks();
			diagnostics.AddRange(instance);
			instance.Free();
		}
	}

	private static void GetStateMachineSlotDebugInfo(PEModuleBuilder moduleBuilder, IEnumerable<IFieldDefinition> fieldDefs, VariableSlotAllocator variableSlotAllocatorOpt, DiagnosticBag diagnostics, out ImmutableArray<EncHoistedLocalInfo> hoistedVariableSlots, out ImmutableArray<ITypeReference> awaiterSlots)
	{
		ArrayBuilder<EncHoistedLocalInfo> instance = ArrayBuilder<EncHoistedLocalInfo>.GetInstance();
		ArrayBuilder<ITypeReference> instance2 = ArrayBuilder<ITypeReference>.GetInstance();
		foreach (StateMachineFieldSymbol fieldDef in fieldDefs)
		{
			int slotIndex = fieldDef.SlotIndex;
			if (fieldDef.SlotDebugInfo.SynthesizedKind == SynthesizedLocalKind.AwaiterField)
			{
				while (slotIndex >= instance2.Count)
				{
					instance2.Add(null);
				}
				instance2[slotIndex] = moduleBuilder.EncTranslateLocalVariableType(fieldDef.Type, diagnostics);
			}
			else if (!fieldDef.SlotDebugInfo.Id.IsNone)
			{
				while (slotIndex >= instance.Count)
				{
					instance.Add(new EncHoistedLocalInfo(ignored: true));
				}
				instance[slotIndex] = new EncHoistedLocalInfo(fieldDef.SlotDebugInfo, moduleBuilder.EncTranslateLocalVariableType(fieldDef.Type, diagnostics));
			}
		}
		if (variableSlotAllocatorOpt != null)
		{
			int previousAwaiterSlotCount = variableSlotAllocatorOpt.PreviousAwaiterSlotCount;
			while (instance2.Count < previousAwaiterSlotCount)
			{
				instance2.Add(null);
			}
			int previousHoistedLocalSlotCount = variableSlotAllocatorOpt.PreviousHoistedLocalSlotCount;
			while (instance.Count < previousHoistedLocalSlotCount)
			{
				instance.Add(new EncHoistedLocalInfo(ignored: true));
			}
		}
		hoistedVariableSlots = instance.ToImmutableAndFree();
		awaiterSlots = instance2.ToImmutableAndFree();
	}

	internal static BoundBlock BindMethodBody(MethodSymbol method, TypeCompilationState compilationState, DiagnosticBag diagnostics)
	{
		ImportChain importChain;
		bool originalBodyNested;
		(SyntaxNode, BoundNode, ExecutableCodeBinder) forSemanticModel;
		return BindMethodBody(method, compilationState, diagnostics, out importChain, out originalBodyNested, out forSemanticModel);
	}

	private static BoundBlock BindMethodBody(MethodSymbol method, TypeCompilationState compilationState, DiagnosticBag diagnostics, out ImportChain importChain, out bool originalBodyNested, out (SyntaxNode Syntax, BoundNode Body, ExecutableCodeBinder Binder) forSemanticModel)
	{
		originalBodyNested = false;
		importChain = null;
		forSemanticModel = default;
		BoundBlock boundBlock;
		if (method is SourceMemberMethodSymbol { SyntaxNode: var syntaxNode } sourceMemberMethodSymbol)
		{
			ConstructorDeclarationSyntax constructorDeclarationSyntax = syntaxNode as ConstructorDeclarationSyntax;
			if (method.MethodKind == MethodKind.StaticConstructor && constructorDeclarationSyntax?.Initializer != null)
			{
				diagnostics.Add(ErrorCode.ERR_StaticConstructorWithExplicitConstructorCall, constructorDeclarationSyntax.Initializer.ThisOrBaseKeyword.GetLocation(), constructorDeclarationSyntax.Identifier.ValueText);
			}
			ExecutableCodeBinder executableCodeBinder = sourceMemberMethodSymbol.TryGetBodyBinder();
			if (sourceMemberMethodSymbol.IsExtern)
			{
				if (executableCodeBinder == null)
				{
					GenerateExternalMethodWarnings(sourceMemberMethodSymbol, diagnostics);
				}
				return null;
			}
			if (sourceMemberMethodSymbol.IsDefaultValueTypeConstructor())
			{
				return null;
			}
			if (executableCodeBinder == null)
			{
				if (sourceMemberMethodSymbol.AssociatedSymbol is SourcePropertySymbol { IsAutoProperty: not false })
				{
					return MethodBodySynthesizer.ConstructAutoPropertyAccessorBody(sourceMemberMethodSymbol);
				}
				return null;
			}
			importChain = executableCodeBinder.ImportChain;
			executableCodeBinder.ValidateIteratorMethods(diagnostics);
			BoundNode boundNode = executableCodeBinder.BindMethodBody(syntaxNode, diagnostics);
			forSemanticModel = (Syntax: syntaxNode, Body: boundNode, Binder: executableCodeBinder);
			switch (boundNode.Kind)
			{
			case BoundKind.ConstructorMethodBody:
			{
				BoundConstructorMethodBody boundConstructorMethodBody = (BoundConstructorMethodBody)boundNode;
				boundBlock = boundConstructorMethodBody.BlockBody ?? boundConstructorMethodBody.ExpressionBody;
				if (boundConstructorMethodBody.Initializer != null)
				{
					ReportCtorInitializerCycles(method, boundConstructorMethodBody.Initializer.Expression, compilationState, diagnostics);
					if (boundBlock == null)
					{
						boundBlock = new BoundBlock(boundConstructorMethodBody.Syntax, boundConstructorMethodBody.Locals, ImmutableArray.Create((BoundStatement)boundConstructorMethodBody.Initializer));
					}
					else
					{
						boundBlock = new BoundBlock(boundConstructorMethodBody.Syntax, boundConstructorMethodBody.Locals, ImmutableArray.Create((BoundStatement)boundConstructorMethodBody.Initializer, (BoundStatement)boundBlock));
						originalBodyNested = true;
					}
					return boundBlock;
				}
				break;
			}
			case BoundKind.NonConstructorMethodBody:
			{
				BoundNonConstructorMethodBody boundNonConstructorMethodBody = (BoundNonConstructorMethodBody)boundNode;
				boundBlock = boundNonConstructorMethodBody.BlockBody ?? boundNonConstructorMethodBody.ExpressionBody;
				break;
			}
			case BoundKind.Block:
				boundBlock = (BoundBlock)boundNode;
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(boundNode.Kind);
			}
		}
		else
		{
			boundBlock = null;
		}
		if (method.MethodKind == MethodKind.Destructor && boundBlock != null)
		{
			return MethodBodySynthesizer.ConstructDestructorBody(method, boundBlock);
		}
		BoundStatement boundStatement = BindImplicitConstructorInitializerIfAny(method, compilationState, diagnostics);
		ImmutableArray<BoundStatement> statements;
		if (boundStatement == null)
		{
			if (boundBlock != null)
			{
				return boundBlock;
			}
			statements = ImmutableArray<BoundStatement>.Empty;
		}
		else if (boundBlock == null)
		{
			statements = ImmutableArray.Create(boundStatement);
		}
		else
		{
			statements = ImmutableArray.Create(boundStatement, boundBlock);
			originalBodyNested = true;
		}
		return BoundBlock.SynthesizedNoLocals(method.GetNonNullSyntaxNode(), statements);
	}

	private static BoundStatement BindImplicitConstructorInitializerIfAny(MethodSymbol method, TypeCompilationState compilationState, DiagnosticBag diagnostics)
	{
		if (method.MethodKind == MethodKind.Constructor && !method.ContainingType.IsDelegateType() && !method.IsExtern)
		{
			CSharpCompilation declaringCompilation = method.DeclaringCompilation;
			BoundExpression boundExpression = BindImplicitConstructorInitializer(method, diagnostics, declaringCompilation);
			if (boundExpression != null)
			{
				ReportCtorInitializerCycles(method, boundExpression, compilationState, diagnostics);
				return new BoundExpressionStatement(boundExpression.Syntax, boundExpression)
				{
					WasCompilerGenerated = method.IsImplicitlyDeclared
				};
			}
		}
		return null;
	}

	private static void ReportCtorInitializerCycles(MethodSymbol method, BoundExpression initializerInvocation, TypeCompilationState compilationState, DiagnosticBag diagnostics)
	{
		if (initializerInvocation is BoundCall { HasAnyErrors: false } boundCall && boundCall.Method != method && boundCall.Method.ContainingType == method.ContainingType)
		{
			compilationState.ReportCtorInitializerCycles(method, boundCall.Method, boundCall.Syntax, diagnostics);
		}
	}

	internal static BoundExpression BindImplicitConstructorInitializer(MethodSymbol constructor, DiagnosticBag diagnostics, CSharpCompilation compilation)
	{
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = constructor.ContainingType.BaseTypeNoUseSiteDiagnostics;
		SourceMemberMethodSymbol sourceMemberMethodSymbol = constructor as SourceMemberMethodSymbol;
		if ((object)baseTypeNoUseSiteDiagnostics != null)
		{
			if (baseTypeNoUseSiteDiagnostics.SpecialType == SpecialType.System_Object)
			{
				return GenerateBaseParameterlessConstructorInitializer(constructor, diagnostics);
			}
			if (baseTypeNoUseSiteDiagnostics.IsErrorType() || baseTypeNoUseSiteDiagnostics.IsStatic)
			{
				return null;
			}
		}
		Binder binder;
		if ((object)sourceMemberMethodSymbol == null)
		{
			CSharpSyntaxNode nonNullSyntaxNode = constructor.GetNonNullSyntaxNode();
			SyntaxToken implicitConstructorBodyToken = GetImplicitConstructorBodyToken(nonNullSyntaxNode);
			binder = compilation.GetBinderFactory(nonNullSyntaxNode.SyntaxTree).GetBinder(nonNullSyntaxNode, implicitConstructorBodyToken.Position);
		}
		else
		{
			binder = compilation.GetBinderFactory(sourceMemberMethodSymbol.SyntaxTree).GetBinder(((ConstructorDeclarationSyntax)sourceMemberMethodSymbol.SyntaxNode).ParameterList);
		}
		return binder.WithAdditionalFlagsAndContainingMemberOrLambda(BinderFlags.ConstructorInitializer, constructor).BindConstructorInitializer(null, constructor, diagnostics);
	}

	private static SyntaxToken GetImplicitConstructorBodyToken(CSharpSyntaxNode containerNode)
	{
		SyntaxKind syntaxKind = containerNode.Kind();
		return syntaxKind switch
		{
			SyntaxKind.ClassDeclaration => ((ClassDeclarationSyntax)containerNode).OpenBraceToken, 
			SyntaxKind.StructDeclaration => ((StructDeclarationSyntax)containerNode).OpenBraceToken, 
			SyntaxKind.EnumDeclaration => ((EnumDeclarationSyntax)containerNode).OpenBraceToken, 
			_ => throw ExceptionUtilities.UnexpectedValue(syntaxKind), 
		};
	}

	internal static BoundCall GenerateBaseParameterlessConstructorInitializer(MethodSymbol constructor, DiagnosticBag diagnostics)
	{
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = constructor.ContainingType.BaseTypeNoUseSiteDiagnostics;
		MethodSymbol methodSymbol = null;
		LookupResultKind resultKind = LookupResultKind.Viable;
		Location location = (constructor.Locations.IsEmpty ? NoLocation.Singleton : constructor.Locations[0]);
		foreach (MethodSymbol instanceConstructor in baseTypeNoUseSiteDiagnostics.InstanceConstructors)
		{
			if (instanceConstructor.ParameterCount == 0)
			{
				methodSymbol = instanceConstructor;
				break;
			}
		}
		if ((object)methodSymbol == null)
		{
			diagnostics.Add(ErrorCode.ERR_BadCtorArgCount, location, baseTypeNoUseSiteDiagnostics, 0);
			return null;
		}
		if (Binder.ReportUseSiteDiagnostics(methodSymbol, diagnostics, location))
		{
			return null;
		}
		bool hasErrors = false;
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if (!AccessCheck.IsSymbolAccessible(methodSymbol, constructor.ContainingType, ref useSiteDiagnostics))
		{
			diagnostics.Add(ErrorCode.ERR_BadAccess, location, methodSymbol);
			resultKind = LookupResultKind.Inaccessible;
			hasErrors = true;
		}
		if (!useSiteDiagnostics.IsNullOrEmpty())
		{
			diagnostics.Add(location, useSiteDiagnostics);
		}
		CSharpSyntaxNode nonNullSyntaxNode = constructor.GetNonNullSyntaxNode();
		BoundExpression receiverOpt = new BoundThisReference(nonNullSyntaxNode, constructor.ContainingType)
		{
			WasCompilerGenerated = true
		};
		return new BoundCall(nonNullSyntaxNode, receiverOpt, methodSymbol, ImmutableArray<BoundExpression>.Empty, ImmutableArray<string>.Empty, ImmutableArray<RefKind>.Empty, isDelegateCall: false, expanded: false, invokedAsExtensionMethod: false, ImmutableArray<int>.Empty, resultKind, null, baseTypeNoUseSiteDiagnostics, hasErrors)
		{
			WasCompilerGenerated = true
		};
	}

	private static void GenerateExternalMethodWarnings(SourceMemberMethodSymbol methodSymbol, DiagnosticBag diagnostics)
	{
		if (methodSymbol.GetAttributes().IsEmpty && !methodSymbol.ContainingType.IsComImport)
		{
			ErrorCode code = ((methodSymbol.MethodKind == MethodKind.Constructor || methodSymbol.MethodKind == MethodKind.StaticConstructor) ? ErrorCode.WRN_ExternCtorNoImplementation : ErrorCode.WRN_ExternMethodNoImplementation);
			diagnostics.Add(code, methodSymbol.Locations[0], methodSymbol);
		}
	}

	private static bool HasThisConstructorInitializer(MethodSymbol method)
	{
		if ((object)method != null && method.MethodKind == MethodKind.Constructor && method is SourceMemberMethodSymbol { SyntaxNode: ConstructorDeclarationSyntax { Initializer: { } initializer } })
		{
			return initializer.Kind() == SyntaxKind.ThisConstructorInitializer;
		}
		return false;
	}

	private static DebugSourceDocument CreateDebugDocumentForFile(string normalizedPath)
	{
		return new DebugSourceDocument(normalizedPath, DebugSourceDocument.CorSymLanguageTypeCSharp);
	}

	private static bool PassesFilter(Predicate<Symbol> filterOpt, Symbol symbol)
	{
		return filterOpt?.Invoke(symbol) ?? true;
	}
}
