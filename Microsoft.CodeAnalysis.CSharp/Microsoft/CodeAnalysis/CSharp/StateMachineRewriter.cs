using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class StateMachineRewriter
{
	protected readonly BoundStatement body;

	protected readonly MethodSymbol method;

	protected readonly DiagnosticBag diagnostics;

	protected readonly SyntheticBoundNodeFactory F;

	protected readonly SynthesizedContainer stateMachineType;

	protected readonly VariableSlotAllocator slotAllocatorOpt;

	protected readonly SynthesizedLocalOrdinalsDispenser synthesizedLocalOrdinals;

	protected FieldSymbol stateField;

	protected IReadOnlyDictionary<Symbol, CapturedSymbolReplacement> nonReusableLocalProxies;

	protected int nextFreeHoistedLocalSlot;

	protected Roslyn.Utilities.IReadOnlySet<Symbol> hoistedVariables;

	protected Dictionary<Symbol, CapturedSymbolReplacement> initialParameters;

	protected abstract bool PreserveInitialParameterValues { get; }

	protected StateMachineRewriter(BoundStatement body, MethodSymbol method, SynthesizedContainer stateMachineType, VariableSlotAllocator slotAllocatorOpt, TypeCompilationState compilationState, DiagnosticBag diagnostics)
	{
		this.body = body;
		this.method = method;
		this.stateMachineType = stateMachineType;
		this.slotAllocatorOpt = slotAllocatorOpt;
		synthesizedLocalOrdinals = new SynthesizedLocalOrdinalsDispenser();
		this.diagnostics = diagnostics;
		F = new SyntheticBoundNodeFactory(method, body.Syntax, compilationState, diagnostics);
	}

	protected abstract void GenerateControlFields();

	protected abstract void InitializeStateMachine(ArrayBuilder<BoundStatement> bodyBuilder, NamedTypeSymbol frameType, LocalSymbol stateMachineLocal);

	protected abstract BoundStatement GenerateStateMachineCreation(LocalSymbol stateMachineVariable, NamedTypeSymbol frameType);

	protected abstract void GenerateMethodImplementations();

	protected BoundStatement Rewrite()
	{
		if (body.HasErrors)
		{
			return body;
		}
		F.OpenNestedType(stateMachineType);
		GenerateControlFields();
		if (PreserveInitialParameterValues)
		{
			initialParameters = new Dictionary<Symbol, CapturedSymbolReplacement>();
		}
		OrderedSet<Symbol> variablesToHoist = IteratorAndAsyncCaptureWalker.Analyze(F.Compilation, method, body, diagnostics);
		CreateNonReusableLocalProxies(variablesToHoist, out nonReusableLocalProxies, out nextFreeHoistedLocalSlot);
		hoistedVariables = variablesToHoist;
		GenerateMethodImplementations();
		return GenerateKickoffMethodBody();
	}

	private void CreateNonReusableLocalProxies(IEnumerable<Symbol> variablesToHoist, out IReadOnlyDictionary<Symbol, CapturedSymbolReplacement> proxies, out int nextFreeHoistedLocalSlot)
	{
		Dictionary<Symbol, CapturedSymbolReplacement> dictionary = new Dictionary<Symbol, CapturedSymbolReplacement>();
		TypeMap typeMap = stateMachineType.TypeMap;
		bool flag = F.Compilation.Options.OptimizationLevel == OptimizationLevel.Debug;
		bool flag2 = flag && slotAllocatorOpt != null;
		nextFreeHoistedLocalSlot = (flag2 ? slotAllocatorOpt.PreviousHoistedLocalSlotCount : 0);
		foreach (Symbol item in variablesToHoist)
		{
			if (item.Kind == SymbolKind.Local)
			{
				LocalSymbol localSymbol = (LocalSymbol)item;
				SynthesizedLocalKind synthesizedKind = localSymbol.SynthesizedKind;
				if (!synthesizedKind.MustSurviveStateMachineSuspension() || localSymbol.IsConst || localSymbol.RefKind != RefKind.None)
				{
					continue;
				}
				StateMachineFieldSymbol stateMachineFieldSymbol = null;
				if (ShouldPreallocateNonReusableProxy(localSymbol))
				{
					TypeSymbol type = typeMap.SubstituteType(localSymbol.Type).Type;
					int num = -1;
					LocalDebugId localDebugId;
					if (flag)
					{
						SyntaxNode declaratorSyntax = localSymbol.GetDeclaratorSyntax();
						int syntaxOffset = method.CalculateLocalSyntaxOffset(declaratorSyntax.SpanStart, declaratorSyntax.SyntaxTree);
						int ordinal = synthesizedLocalOrdinals.AssignLocalOrdinal(synthesizedKind, syntaxOffset);
						localDebugId = new LocalDebugId(syntaxOffset, ordinal);
						if (flag2 && slotAllocatorOpt.TryGetPreviousHoistedLocalSlotIndex(declaratorSyntax, F.ModuleBuilderOpt.Translate(type, declaratorSyntax, diagnostics), synthesizedKind, localDebugId, diagnostics, out var slotIndex))
						{
							num = slotIndex;
						}
					}
					else
					{
						localDebugId = LocalDebugId.None;
					}
					if (num == -1)
					{
						num = nextFreeHoistedLocalSlot++;
					}
					string name = GeneratedNames.MakeHoistedLocalFieldName(synthesizedKind, num, localSymbol.Name);
					stateMachineFieldSymbol = F.StateMachineField(type, name, new LocalSlotDebugInfo(synthesizedKind, localDebugId), num);
				}
				if (stateMachineFieldSymbol != null)
				{
					dictionary.Add(localSymbol, new CapturedToStateMachineFieldReplacement(stateMachineFieldSymbol, isReusable: false));
				}
				continue;
			}
			ParameterSymbol parameterSymbol = (ParameterSymbol)item;
			if (parameterSymbol.IsThis)
			{
				NamedTypeSymbol containingType = method.ContainingType;
				StateMachineFieldSymbol stateMachineFieldSymbol2 = F.StateMachineField(containingType, GeneratedNames.ThisProxyFieldName(), isPublic: true, isThis: true);
				dictionary.Add(parameterSymbol, new CapturedToStateMachineFieldReplacement(stateMachineFieldSymbol2, isReusable: false));
				if (PreserveInitialParameterValues)
				{
					StateMachineFieldSymbol hoistedField = (containingType.IsStructType() ? F.StateMachineField(containingType, GeneratedNames.StateMachineThisParameterProxyName(), isPublic: true, isThis: true) : stateMachineFieldSymbol2);
					initialParameters.Add(parameterSymbol, new CapturedToStateMachineFieldReplacement(hoistedField, isReusable: false));
				}
			}
			else
			{
				StateMachineFieldSymbol hoistedField2 = F.StateMachineField(typeMap.SubstituteType(parameterSymbol.Type).Type, parameterSymbol.Name, !PreserveInitialParameterValues);
				dictionary.Add(parameterSymbol, new CapturedToStateMachineFieldReplacement(hoistedField2, isReusable: false));
				if (PreserveInitialParameterValues)
				{
					StateMachineFieldSymbol hoistedField3 = F.StateMachineField(typeMap.SubstituteType(parameterSymbol.Type).Type, GeneratedNames.StateMachineParameterProxyFieldName(parameterSymbol.Name), isPublic: true);
					initialParameters.Add(parameterSymbol, new CapturedToStateMachineFieldReplacement(hoistedField3, isReusable: false));
				}
			}
		}
		proxies = dictionary;
	}

	private bool ShouldPreallocateNonReusableProxy(LocalSymbol local)
	{
		SynthesizedLocalKind synthesizedKind = local.SynthesizedKind;
		OptimizationLevel optimizationLevel = F.Compilation.Options.OptimizationLevel;
		if (optimizationLevel == OptimizationLevel.Release && synthesizedKind == SynthesizedLocalKind.UserDefined)
		{
			return false;
		}
		return !synthesizedKind.IsSlotReusable(optimizationLevel);
	}

	private BoundStatement GenerateKickoffMethodBody()
	{
		F.CurrentFunction = method;
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		NamedTypeSymbol namedTypeSymbol = (method.IsGenericMethod ? stateMachineType.Construct(method.TypeArguments) : stateMachineType);
		LocalSymbol stateMachineVariable = F.SynthesizedLocal(namedTypeSymbol);
		InitializeStateMachine(instance, namedTypeSymbol, stateMachineVariable);
		IReadOnlyDictionary<Symbol, CapturedSymbolReplacement> readOnlyDictionary;
		if (!PreserveInitialParameterValues)
		{
			readOnlyDictionary = nonReusableLocalProxies;
		}
		else
		{
			IReadOnlyDictionary<Symbol, CapturedSymbolReplacement> readOnlyDictionary2 = initialParameters;
			readOnlyDictionary = readOnlyDictionary2;
		}
		IReadOnlyDictionary<Symbol, CapturedSymbolReplacement> readOnlyDictionary3 = readOnlyDictionary;
		if (!method.IsStatic && readOnlyDictionary3.TryGetValue(method.ThisParameter, out var value))
		{
			instance.Add(F.Assignment(value.Replacement(F.Syntax, (NamedTypeSymbol frameType1) => F.Local(stateMachineVariable)), F.This()));
		}
		foreach (ParameterSymbol parameter in method.Parameters)
		{
			if (readOnlyDictionary3.TryGetValue(parameter, out var value2))
			{
				instance.Add(F.Assignment(value2.Replacement(F.Syntax, (NamedTypeSymbol frameType1) => F.Local(stateMachineVariable)), F.Parameter(parameter)));
			}
		}
		instance.Add(GenerateStateMachineCreation(stateMachineVariable, namedTypeSymbol));
		return F.Block(ImmutableArray.Create(stateMachineVariable), instance.ToImmutableAndFree());
	}

	protected SynthesizedImplementationMethod OpenMethodImplementation(MethodSymbol methodToImplement, string methodName = null, bool hasMethodBodyDependency = false)
	{
		SynthesizedStateMachineDebuggerHiddenMethod synthesizedStateMachineDebuggerHiddenMethod = new SynthesizedStateMachineDebuggerHiddenMethod(methodName, methodToImplement, (StateMachineTypeSymbol)F.CurrentType, null, hasMethodBodyDependency);
		F.ModuleBuilderOpt.AddSynthesizedDefinition(F.CurrentType, synthesizedStateMachineDebuggerHiddenMethod);
		F.CurrentFunction = synthesizedStateMachineDebuggerHiddenMethod;
		return synthesizedStateMachineDebuggerHiddenMethod;
	}

	protected MethodSymbol OpenPropertyImplementation(MethodSymbol getterToImplement)
	{
		SynthesizedStateMachineProperty synthesizedStateMachineProperty = new SynthesizedStateMachineProperty(getterToImplement, (StateMachineTypeSymbol)F.CurrentType);
		F.ModuleBuilderOpt.AddSynthesizedDefinition(F.CurrentType, synthesizedStateMachineProperty);
		MethodSymbol getMethod = synthesizedStateMachineProperty.GetMethod;
		F.ModuleBuilderOpt.AddSynthesizedDefinition(F.CurrentType, getMethod);
		F.CurrentFunction = getMethod;
		return getMethod;
	}

	protected SynthesizedImplementationMethod OpenMoveNextMethodImplementation(MethodSymbol methodToImplement)
	{
		SynthesizedStateMachineMoveNextMethod synthesizedStateMachineMoveNextMethod = new SynthesizedStateMachineMoveNextMethod(methodToImplement, (StateMachineTypeSymbol)F.CurrentType);
		F.ModuleBuilderOpt.AddSynthesizedDefinition(F.CurrentType, synthesizedStateMachineMoveNextMethod);
		F.CurrentFunction = synthesizedStateMachineMoveNextMethod;
		return synthesizedStateMachineMoveNextMethod;
	}
}
