using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class IteratorRewriter : StateMachineRewriter
{
	private readonly TypeSymbol _elementType;

	private readonly bool _isEnumerable;

	private FieldSymbol _currentField;

	private FieldSymbol _initialThreadIdField;

	protected override bool PreserveInitialParameterValues => _isEnumerable;

	private IteratorRewriter(BoundStatement body, MethodSymbol method, bool isEnumerable, IteratorStateMachine stateMachineType, VariableSlotAllocator slotAllocatorOpt, TypeCompilationState compilationState, DiagnosticBag diagnostics)
		: base(body, method, stateMachineType, slotAllocatorOpt, compilationState, diagnostics)
	{
		_elementType = stateMachineType.ElementType;
		_isEnumerable = isEnumerable;
	}

	internal static BoundStatement Rewrite(BoundStatement body, MethodSymbol method, int methodOrdinal, VariableSlotAllocator slotAllocatorOpt, TypeCompilationState compilationState, DiagnosticBag diagnostics, out IteratorStateMachine stateMachineType)
	{
		TypeSymbol iteratorElementType = method.IteratorElementType;
		if ((object)iteratorElementType == null)
		{
			stateMachineType = null;
			return body;
		}
		bool isEnumerable;
		switch (method.ReturnType.OriginalDefinition.SpecialType)
		{
		case SpecialType.System_Collections_IEnumerable:
		case SpecialType.System_Collections_Generic_IEnumerable_T:
			isEnumerable = true;
			break;
		case SpecialType.System_Collections_IEnumerator:
		case SpecialType.System_Collections_Generic_IEnumerator_T:
			isEnumerable = false;
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(method.ReturnType.OriginalDefinition.SpecialType);
		}
		stateMachineType = new IteratorStateMachine(slotAllocatorOpt, compilationState, method, methodOrdinal, isEnumerable, iteratorElementType);
		compilationState.ModuleBuilderOpt.CompilationState.SetStateMachineType(method, stateMachineType);
		IteratorRewriter iteratorRewriter = new IteratorRewriter(body, method, isEnumerable, stateMachineType, slotAllocatorOpt, compilationState, diagnostics);
		if (!iteratorRewriter.VerifyPresenceOfRequiredAPIs())
		{
			return body;
		}
		return iteratorRewriter.Rewrite();
	}

	protected bool VerifyPresenceOfRequiredAPIs()
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		EnsureSpecialType(SpecialType.System_Int32, instance);
		EnsureSpecialType(SpecialType.System_IDisposable, instance);
		EnsureSpecialMember(SpecialMember.System_IDisposable__Dispose, instance);
		EnsureSpecialType(SpecialType.System_Collections_IEnumerator, instance);
		EnsureSpecialPropertyGetter(SpecialMember.System_Collections_IEnumerator__Current, instance);
		EnsureSpecialMember(SpecialMember.System_Collections_IEnumerator__MoveNext, instance);
		EnsureSpecialMember(SpecialMember.System_Collections_IEnumerator__Reset, instance);
		EnsureSpecialType(SpecialType.System_Collections_Generic_IEnumerator_T, instance);
		EnsureSpecialPropertyGetter(SpecialMember.System_Collections_Generic_IEnumerator_T__Current, instance);
		if (_isEnumerable)
		{
			EnsureSpecialType(SpecialType.System_Collections_IEnumerable, instance);
			EnsureSpecialMember(SpecialMember.System_Collections_IEnumerable__GetEnumerator, instance);
			EnsureSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T, instance);
			EnsureSpecialMember(SpecialMember.System_Collections_Generic_IEnumerable_T__GetEnumerator, instance);
		}
		bool flag = instance.HasAnyErrors();
		if (flag)
		{
			diagnostics.AddRange(instance);
		}
		instance.Free();
		return !flag;
	}

	private Symbol EnsureSpecialMember(SpecialMember member, DiagnosticBag bag)
	{
		Binder.TryGetSpecialTypeMember<Symbol>(F.Compilation, member, body.Syntax, bag, out var symbol);
		return symbol;
	}

	private void EnsureSpecialType(SpecialType type, DiagnosticBag bag)
	{
		Binder.GetSpecialType(F.Compilation, type, body.Syntax, bag);
	}

	private void EnsureSpecialPropertyGetter(SpecialMember member, DiagnosticBag bag)
	{
		PropertySymbol propertySymbol = (PropertySymbol)EnsureSpecialMember(member, bag);
		if ((object)propertySymbol == null)
		{
			return;
		}
		MethodSymbol getMethod = propertySymbol.GetMethod;
		if ((object)getMethod == null)
		{
			Binder.Error(bag, ErrorCode.ERR_PropertyLacksGet, body.Syntax, propertySymbol);
			return;
		}
		DiagnosticInfo useSiteDiagnostic = getMethod.GetUseSiteDiagnostic();
		if (useSiteDiagnostic != null)
		{
			bag.Add(new CSDiagnostic(useSiteDiagnostic, body.Syntax.Location));
		}
	}

	protected override void GenerateControlFields()
	{
		stateField = F.StateMachineField(F.SpecialType(SpecialType.System_Int32), GeneratedNames.MakeStateMachineStateFieldName());
		_currentField = F.StateMachineField(_elementType, GeneratedNames.MakeIteratorCurrentFieldName());
		bool flag = _isEnumerable && ((object)F.WellKnownMember(WellKnownMember.System_Threading_Thread__ManagedThreadId, isOptional: true) != null || (object)F.WellKnownMember(WellKnownMember.System_Environment__CurrentManagedThreadId, isOptional: true) != null);
		_initialThreadIdField = (flag ? F.StateMachineField(F.SpecialType(SpecialType.System_Int32), GeneratedNames.MakeIteratorCurrentThreadIdFieldName()) : null);
	}

	protected override void GenerateMethodImplementations()
	{
		try
		{
			BoundExpression managedThreadId = null;
			GenerateEnumeratorImplementation();
			if (_isEnumerable)
			{
				GenerateEnumerableImplementation(ref managedThreadId);
			}
			GenerateConstructor(managedThreadId);
		}
		catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
		{
			diagnostics.Add(missingPredefinedMember.Diagnostic);
		}
	}

	private void GenerateEnumeratorImplementation()
	{
		MethodSymbol methodToImplement = F.SpecialMethod(SpecialMember.System_IDisposable__Dispose);
		MethodSymbol methodToImplement2 = F.SpecialMethod(SpecialMember.System_Collections_IEnumerator__MoveNext);
		MethodSymbol methodToImplement3 = F.SpecialMethod(SpecialMember.System_Collections_IEnumerator__Reset);
		MethodSymbol getMethod = F.SpecialProperty(SpecialMember.System_Collections_IEnumerator__Current).GetMethod;
		NamedTypeSymbol newOwner = F.SpecialType(SpecialType.System_Collections_Generic_IEnumerator_T).Construct(_elementType);
		MethodSymbol getterToImplement = F.SpecialProperty(SpecialMember.System_Collections_Generic_IEnumerator_T__Current).GetMethod.AsMember(newOwner);
		SynthesizedImplementationMethod disposeMethod = OpenMethodImplementation(methodToImplement, null, hasMethodBodyDependency: true);
		SynthesizedImplementationMethod moveNextMethod = OpenMoveNextMethodImplementation(methodToImplement2);
		GenerateMoveNextAndDispose(moveNextMethod, disposeMethod);
		OpenPropertyImplementation(getterToImplement);
		F.CloseMethod(F.Return(F.Field(F.This(), _currentField)));
		OpenMethodImplementation(methodToImplement3);
		F.CloseMethod(F.Throw(F.New(F.WellKnownType(WellKnownType.System_NotSupportedException))));
		OpenPropertyImplementation(getMethod);
		F.CloseMethod(F.Return(F.Field(F.This(), _currentField)));
	}

	private void GenerateEnumerableImplementation(ref BoundExpression managedThreadId)
	{
		MethodSymbol methodToImplement = F.SpecialMethod(SpecialMember.System_Collections_IEnumerable__GetEnumerator);
		NamedTypeSymbol newOwner = F.SpecialType(SpecialType.System_Collections_Generic_IEnumerable_T).Construct(_elementType);
		MethodSymbol methodToImplement2 = F.SpecialMethod(SpecialMember.System_Collections_Generic_IEnumerable_T__GetEnumerator).AsMember(newOwner);
		SynthesizedImplementationMethod synthesizedImplementationMethod = OpenMethodImplementation(methodToImplement2);
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		LocalSymbol resultVariable = F.SynthesizedLocal(stateMachineType);
		BoundStatement boundStatement = F.Assignment(F.Local(resultVariable), F.New(stateMachineType.Constructor, F.Literal(0)));
		GeneratedLabelSymbol label = F.GenerateLabel("thisInitialized");
		if ((object)_initialThreadIdField != null)
		{
			MethodSymbol methodSymbol = null;
			if (F.WellKnownMember(WellKnownMember.System_Environment__CurrentManagedThreadId, isOptional: true) is PropertySymbol propertySymbol)
			{
				methodSymbol = propertySymbol.GetMethod;
			}
			if ((object)methodSymbol != null)
			{
				managedThreadId = F.Call(null, methodSymbol);
			}
			else
			{
				managedThreadId = F.Property(F.Property(WellKnownMember.System_Threading_Thread__CurrentThread), WellKnownMember.System_Threading_Thread__ManagedThreadId);
			}
			boundStatement = F.If(F.LogicalAnd(F.IntEqual(F.Field(F.This(), stateField), F.Literal(-2)), F.IntEqual(F.Field(F.This(), _initialThreadIdField), managedThreadId)), F.Block(F.Assignment(F.Field(F.This(), stateField), F.Literal(0)), F.Assignment(F.Local(resultVariable), F.This()), (method.IsStatic || method.ThisParameter.Type.IsReferenceType) ? ((BoundStatement)F.Goto(label)) : ((BoundStatement)F.StatementList())), boundStatement);
		}
		instance.Add(boundStatement);
		Dictionary<Symbol, CapturedSymbolReplacement> dictionary = initialParameters;
		IReadOnlyDictionary<Symbol, CapturedSymbolReplacement> readOnlyDictionary = nonReusableLocalProxies;
		if (!method.IsStatic && readOnlyDictionary.TryGetValue(method.ThisParameter, out var value))
		{
			instance.Add(F.Assignment(value.Replacement(F.Syntax, (NamedTypeSymbol stateMachineType) => F.Local(resultVariable)), dictionary[method.ThisParameter].Replacement(F.Syntax, (NamedTypeSymbol stateMachineType) => F.This())));
		}
		instance.Add(F.Label(label));
		foreach (ParameterSymbol parameter in method.Parameters)
		{
			if (readOnlyDictionary.TryGetValue(parameter, out var value2))
			{
				instance.Add(F.Assignment(value2.Replacement(F.Syntax, (NamedTypeSymbol stateMachineType) => F.Local(resultVariable)), dictionary[parameter].Replacement(F.Syntax, (NamedTypeSymbol stateMachineType) => F.This())));
			}
		}
		instance.Add(F.Return(F.Local(resultVariable)));
		F.CloseMethod(F.Block(ImmutableArray.Create(resultVariable), instance.ToImmutableAndFree()));
		OpenMethodImplementation(methodToImplement);
		F.CloseMethod(F.Return(F.Call(F.This(), synthesizedImplementationMethod)));
	}

	private void GenerateConstructor(BoundExpression managedThreadId)
	{
		F.CurrentFunction = stateMachineType.Constructor;
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		instance.Add(F.BaseInitialization());
		instance.Add(F.Assignment(F.Field(F.This(), stateField), F.Parameter(F.CurrentFunction.Parameters[0])));
		if (managedThreadId != null)
		{
			instance.Add(F.Assignment(F.Field(F.This(), _initialThreadIdField), managedThreadId));
		}
		instance.Add(F.Return());
		F.CloseMethod(F.Block(instance.ToImmutableAndFree()));
	}

	protected override void InitializeStateMachine(ArrayBuilder<BoundStatement> bodyBuilder, NamedTypeSymbol frameType, LocalSymbol stateMachineLocal)
	{
		int value = (_isEnumerable ? (-2) : 0);
		bodyBuilder.Add(F.Assignment(F.Local(stateMachineLocal), F.New(stateMachineType.Constructor.AsMember(frameType), F.Literal(value))));
	}

	protected override BoundStatement GenerateStateMachineCreation(LocalSymbol stateMachineVariable, NamedTypeSymbol frameType)
	{
		return F.Return(F.Local(stateMachineVariable));
	}

	private void GenerateMoveNextAndDispose(SynthesizedImplementationMethod moveNextMethod, SynthesizedImplementationMethod disposeMethod)
	{
		new IteratorMethodToStateMachineRewriter(F, method, stateField, _currentField, hoistedVariables, nonReusableLocalProxies, synthesizedLocalOrdinals, slotAllocatorOpt, nextFreeHoistedLocalSlot, diagnostics).GenerateMoveNextAndDispose(body, moveNextMethod, disposeMethod);
	}
}
