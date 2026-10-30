using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Rewriter;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class MethodCompiler : SymbolVisitor
{
	private readonly Compilation compilation;

	private readonly CancellationToken cancellationToken;

	private readonly DiagnosticBag diagnostics;

	private readonly bool hasDeclarationErrors;

	private readonly ModuleBuilder moduleBeingBuiltOpt;

	private readonly Predicate<Symbol> filterOpt;

	private readonly Predicate<Symbol> radCompilationFilter;

	private ConcurrentStack<Task> _compilerTasks;

	private bool globalHasErrors;

	private void SetGlobalErrorIfTrue(bool arg)
	{
		if (arg)
		{
			globalHasErrors = true;
		}
	}

	internal MethodCompiler(Compilation compilation, ModuleBuilder moduleBeingBuiltOpt, bool hasDeclarationErrors, DiagnosticBag diagnostics, Predicate<Symbol> filterOpt, Predicate<Symbol> radCompilationFilter, CancellationToken cancellationToken)
	{
		DebugAssertHelper.Assert(compilation != null);
		DebugAssertHelper.Assert(diagnostics != null);
		this.compilation = compilation;
		this.moduleBeingBuiltOpt = moduleBeingBuiltOpt;
		this.cancellationToken = cancellationToken;
		this.diagnostics = diagnostics;
		this.filterOpt = filterOpt;
		this.radCompilationFilter = radCompilationFilter;
		this.hasDeclarationErrors = hasDeclarationErrors;
		SetGlobalErrorIfTrue(hasDeclarationErrors);
	}

	internal static void CompileMethodBodies(Compilation compilation, ModuleBuilder moduleBeingBuiltOpt, bool hasDeclarationErrors, DiagnosticBag diagnostics, Predicate<Symbol> filterOpt, Predicate<Symbol> radCompilationFilter, CancellationToken cancellationToken)
	{
		DebugAssertHelper.Assert(compilation != null);
		DebugAssertHelper.Assert(diagnostics != null);
		MethodCompiler methodCompiler = new MethodCompiler(compilation, moduleBeingBuiltOpt, hasDeclarationErrors, diagnostics, filterOpt, radCompilationFilter, cancellationToken);
		if (compilation.Options.ConcurrentBuild)
		{
			methodCompiler._compilerTasks = new ConcurrentStack<Task>();
		}
		methodCompiler.CompileModule(compilation.CompiledModule);
		methodCompiler.WaitForWorkers();
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

	private void CompileModule(ModuleSymbol moduleSymbol)
	{
		foreach (Symbol item in moduleSymbol.GetMembersUnordered())
		{
			item.Accept(this);
		}
	}

	public override void VisitObjectType(ObjectTypeSymbol symbol)
	{
		if (PassesFilter(filterOpt, symbol))
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (compilation.Options.ConcurrentBuild)
			{
				Task item = CompileObjectAsTask(symbol);
				_compilerTasks.Push(item);
			}
			else
			{
				CompileObject(symbol);
			}
		}
	}

	private Task CompileObjectAsTask(ObjectTypeSymbol symbol)
	{
		return Task.Run(UICultureUtilities.WithCurrentUICulture(() =>
		{
			try
			{
				CompileObject(symbol);
			}
			catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
			{
				throw new InvalidOperationException("Unreachable");
			}
		}), cancellationToken);
	}

	private void CompileObject(ObjectTypeSymbol containingType)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (PassesFilter(radCompilationFilter, containingType))
		{
			CompileProperties(containingType);
			CompileMembers(containingType);
		}
		if (moduleBeingBuiltOpt != null && !globalHasErrors)
		{
			PreMetadataEmit(containingType);
			if ((compilation.Options.CompilationGenerateOptions & CompilationGenerationOptions.Code) != 0)
			{
				EmitObjectCode(containingType);
				EmitMetadata(containingType);
			}
		}
	}

	private void EmitNonObjectCode(ObjectTypeSymbol containingType, Action<DiagnosticBag> emitter)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		try
		{
			emitter(instance);
		}
		finally
		{
			bool globalErrorIfTrue = instance.HasAnyErrors();
			diagnostics.AddRange(instance.ToReadOnlyAndFree());
			SetGlobalErrorIfTrue(globalErrorIfTrue);
		}
	}

	private void PreMetadataEmit(ObjectTypeSymbol containingType)
	{
		EmitNonObjectCode(containingType, (DiagnosticBag d) =>
		{
			containingType.GetPreMetadataEmitter(moduleBeingBuiltOpt)?.Emit(d);
		});
	}

	private void EmitMetadata(ObjectTypeSymbol containingType)
	{
		EmitNonObjectCode(containingType, (DiagnosticBag d) =>
		{
			containingType.GetMetadataEmitter(moduleBeingBuiltOpt)?.EmitMetadata(d);
		});
	}

	private void EmitObjectCode(ObjectTypeSymbol containingType)
	{
		ICodeEmitter<MemberDeclarationSyntax> emitter = containingType.GetEmitter(moduleBeingBuiltOpt);
		if (emitter != null)
		{
			new ApplicationObjectEmitter(containingType, moduleBeingBuiltOpt, emitter).EmitObjectCode();
		}
		else if (containingType.GetContainingSymbolOfType<ApplicationObjectTypeSymbol>() != null)
		{
			string alSourceCode = containingType.DeclaringSyntaxReferences.Single().GetSyntax().ToFullString();
			moduleBeingBuiltOpt.SetApplicationObjectCode(containingType.GetContainingSymbolOfType<ApplicationObjectTypeSymbol>(), null, alSourceCode);
		}
	}

	private void CompileProperties(ContainerSymbol containingType)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		try
		{
			SymbolWithPropertiesVisitor instance2 = SymbolWithPropertiesVisitor.Instance;
			containingType.Accept(instance2, instance);
		}
		finally
		{
			diagnostics.AddRange(instance);
			bool flag = instance.HasAnyErrors();
			if (flag)
			{
				SetGlobalErrorIfTrue(flag);
			}
		}
	}

	private void CompileMembers(ContainerSymbol containingType)
	{
		ImmutableArray<Symbol> immutableArray = containingType.GetMembers().CastArray<Symbol>();
		for (int i = 0; i < immutableArray.Length; i++)
		{
			Symbol symbol = immutableArray[i];
			if (!PassesFilter(filterOpt, symbol))
			{
				continue;
			}
			switch (symbol.Kind)
			{
			case SymbolKind.Method:
			{
				MethodSymbol methodSymbol = (MethodSymbol)symbol;
				if (!methodSymbol.IsImplicitlyDeclared)
				{
					CompileMethod(methodSymbol);
				}
				break;
			}
			case SymbolKind.XmlPortNode:
			{
				SourceXmlPortNodeSymbol containingType8 = (SourceXmlPortNodeSymbol)symbol;
				CompileMembers(containingType8);
				break;
			}
			case SymbolKind.ReportDataItem:
			{
				ReportDataItemSymbol containingType7 = (ReportDataItemSymbol)symbol;
				CompileMembers(containingType7);
				break;
			}
			case SymbolKind.QueryDataItem:
			{
				SourceQueryDataItemSymbol containingType6 = (SourceQueryDataItemSymbol)symbol;
				CompileMembers(containingType6);
				break;
			}
			case SymbolKind.Field:
			{
				FieldSymbol containingType5 = (FieldSymbol)symbol;
				CompileMembers(containingType5);
				break;
			}
			case SymbolKind.Control:
			{
				ControlSymbol containingType4 = (ControlSymbol)symbol;
				CompileMembers(containingType4);
				break;
			}
			case SymbolKind.Action:
			{
				ActionSymbol containingType3 = (ActionSymbol)symbol;
				CompileMembers(containingType3);
				break;
			}
			case SymbolKind.Change:
			{
				ChangeSymbol changeSymbol = (ChangeSymbol)symbol;
				if (!changeSymbol.ChangeKind.IsMove())
				{
					CompileMembers(changeSymbol);
				}
				break;
			}
			case SymbolKind.RequestPage:
			{
				RequestPageTypeSymbol containingType2 = (RequestPageTypeSymbol)symbol;
				CompileMembers(containingType2);
				break;
			}
			}
		}
	}

	private void CompileMethod(MethodSymbol methodSymbol)
	{
		cancellationToken.ThrowIfCancellationRequested();
		SourceMethodOrTriggerSymbol sourceMethodOrTriggerSymbol = methodSymbol as SourceMethodOrTriggerSymbol;
		if (methodSymbol.MethodKind == MethodKind.BuiltInMethod)
		{
			return;
		}
		if (moduleBeingBuiltOpt == null && (object)sourceMethodOrTriggerSymbol != null)
		{
			ImmutableArray<Diagnostic> immutableArray = sourceMethodOrTriggerSymbol.Diagnostics;
			if (!immutableArray.IsDefault)
			{
				diagnostics.AddRange(immutableArray);
				return;
			}
		}
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		try
		{
			BoundBlock body = BindMethodBody(methodSymbol, instance);
			bool flag = hasDeclarationErrors || instance.HasAnyErrors();
			SetGlobalErrorIfTrue(flag);
			bool diagsWritten = false;
			ImmutableArray<Diagnostic> newSet = instance.ToReadOnly();
			if (sourceMethodOrTriggerSymbol != null)
			{
				newSet = sourceMethodOrTriggerSymbol.SetDiagnostics(newSet, out diagsWritten);
			}
			if (diagsWritten && compilation.EventQueue != null)
			{
				Lazy<SemanticModel> lazySemanticModel = ((body == null) ? null : new Lazy<SemanticModel>(() =>
				{
					SyntaxNode syntax = body.Syntax;
					SemanticModel semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);
					semanticModel.GetMemberModel(syntax)?.UnguardedAddBoundTreeForStandaloneSyntax(syntax, body);
					return semanticModel;
				}));
				compilation.EventQueue.Enqueue(new SymbolDeclaredCompilationEvent(compilation, methodSymbol, lazySemanticModel));
			}
			if ((moduleBeingBuiltOpt == null) | flag)
			{
				diagnostics.AddRange(newSet);
			}
			else
			{
				MethodCodeGenerator.Create(TriggerToEventRewriter.Rewrite(moduleBeingBuiltOpt.CompilationState, methodSymbol), body, moduleBeingBuiltOpt, compilation.Options).Generate();
			}
		}
		finally
		{
			instance.Free();
		}
	}

	private static BoundBlock BindMethodBody(MethodSymbol method, DiagnosticBag diagnostics)
	{
		BoundBlock result = null;
		Compilation declaringCompilation = method.DeclaringCompilation;
		if (method is SourceMethodOrTriggerSymbol sourceMethodOrTriggerSymbol)
		{
			BinderFactory binderFactory = declaringCompilation.GetBinderFactory(sourceMethodOrTriggerSymbol.SyntaxTree);
			if (sourceMethodOrTriggerSymbol.BodySyntax is Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.BlockSyntax blockSyntax)
			{
				Binder binder = binderFactory.GetBinder(blockSyntax);
				ExecutableCodeBinder executableCodeBinder = new ExecutableCodeBinder(blockSyntax, sourceMethodOrTriggerSymbol, binder);
				result = executableCodeBinder.BindBlock(blockSyntax, diagnostics);
				executableCodeBinder.ReportDiagnosticsIfObsoleteDeprecatedOrIncompatible(diagnostics, method, sourceMethodOrTriggerSymbol.GetSyntax().Name);
			}
		}
		return result;
	}

	private static bool PassesFilter(Predicate<Symbol> filterOpt, Symbol symbol)
	{
		return filterOpt?.Invoke(symbol) ?? true;
	}
}
