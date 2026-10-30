using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal class AsyncRewriter : StateMachineRewriter
{
	private readonly AsyncMethodBuilderMemberCollection _asyncMethodBuilderMemberCollection;

	private readonly bool _constructedSuccessfully;

	private readonly int _methodOrdinal;

	private readonly bool _ignoreAccessibility;

	private FieldSymbol _builderField;

	protected override bool PreserveInitialParameterValues => false;

	private AsyncRewriter(BoundStatement body, MethodSymbol method, int methodOrdinal, AsyncStateMachine stateMachineType, VariableSlotAllocator slotAllocatorOpt, TypeCompilationState compilationState, DiagnosticBag diagnostics)
		: base(body, method, stateMachineType, slotAllocatorOpt, compilationState, diagnostics)
	{
		_constructedSuccessfully = AsyncMethodBuilderMemberCollection.TryCreate(F, method, base.stateMachineType.TypeMap, out _asyncMethodBuilderMemberCollection);
		_methodOrdinal = methodOrdinal;
		_ignoreAccessibility = compilationState.ModuleBuilderOpt.IgnoreAccessibility;
	}

	internal static BoundStatement Rewrite(BoundStatement body, MethodSymbol method, int methodOrdinal, VariableSlotAllocator slotAllocatorOpt, TypeCompilationState compilationState, DiagnosticBag diagnostics, out AsyncStateMachine stateMachineType)
	{
		if (!method.IsAsync)
		{
			stateMachineType = null;
			return body;
		}
		TypeKind typeKind = (compilationState.Compilation.Options.EnableEditAndContinue ? TypeKind.Class : TypeKind.Struct);
		BoundStatement boundStatement = AwaitExpressionSpiller.Rewrite(body, method, compilationState, diagnostics);
		stateMachineType = new AsyncStateMachine(slotAllocatorOpt, compilationState, method, methodOrdinal, typeKind);
		compilationState.ModuleBuilderOpt.CompilationState.SetStateMachineType(method, stateMachineType);
		AsyncRewriter asyncRewriter = new AsyncRewriter(boundStatement, method, methodOrdinal, stateMachineType, slotAllocatorOpt, compilationState, diagnostics);
		if (!asyncRewriter.VerifyPresenceOfRequiredAPIs())
		{
			return body;
		}
		try
		{
			return asyncRewriter.Rewrite();
		}
		catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
		{
			diagnostics.Add(missingPredefinedMember.Diagnostic);
			return new BoundBadStatement(body.Syntax, ImmutableArray.Create((BoundNode)body), hasErrors: true);
		}
	}

	protected bool VerifyPresenceOfRequiredAPIs()
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		EnsureWellKnownMember(WellKnownMember.System_Runtime_CompilerServices_IAsyncStateMachine_MoveNext, instance);
		EnsureWellKnownMember(WellKnownMember.System_Runtime_CompilerServices_IAsyncStateMachine_SetStateMachine, instance);
		bool flag = instance.HasAnyErrors();
		if (flag)
		{
			diagnostics.AddRange(instance);
		}
		instance.Free();
		if (!flag)
		{
			return _constructedSuccessfully;
		}
		return false;
	}

	private Symbol EnsureWellKnownMember(WellKnownMember member, DiagnosticBag bag)
	{
		return Binder.GetWellKnownTypeMember(F.Compilation, member, bag, body.Syntax.Location);
	}

	protected override void GenerateControlFields()
	{
		stateField = F.StateMachineField(F.SpecialType(SpecialType.System_Int32), GeneratedNames.MakeStateMachineStateFieldName(), isPublic: true);
		_builderField = F.StateMachineField(_asyncMethodBuilderMemberCollection.BuilderType, GeneratedNames.AsyncBuilderFieldName(), isPublic: true);
	}

	protected override void GenerateMethodImplementations()
	{
		MethodSymbol methodToImplement = F.WellKnownMethod(WellKnownMember.System_Runtime_CompilerServices_IAsyncStateMachine_MoveNext);
		MethodSymbol methodToImplement2 = F.WellKnownMethod(WellKnownMember.System_Runtime_CompilerServices_IAsyncStateMachine_SetStateMachine);
		SynthesizedImplementationMethod moveNextMethod = OpenMoveNextMethodImplementation(methodToImplement);
		GenerateMoveNext(moveNextMethod);
		OpenMethodImplementation(methodToImplement2, "SetStateMachine");
		if (F.CurrentType.TypeKind == TypeKind.Class)
		{
			F.CloseMethod(F.Return());
		}
		else
		{
			F.CloseMethod(F.Block(F.ExpressionStatement(F.Call(F.Field(F.This(), _builderField), _asyncMethodBuilderMemberCollection.SetStateMachine, new BoundExpression[1] { F.Parameter(F.CurrentFunction.Parameters[0]) })), F.Return()));
		}
		if (stateMachineType.TypeKind == TypeKind.Class)
		{
			F.CurrentFunction = stateMachineType.Constructor;
			F.CloseMethod(F.Block(ImmutableArray.Create(F.BaseInitialization(), F.Return())));
		}
	}

	protected override void InitializeStateMachine(ArrayBuilder<BoundStatement> bodyBuilder, NamedTypeSymbol frameType, LocalSymbol stateMachineLocal)
	{
		if (frameType.TypeKind == TypeKind.Class)
		{
			bodyBuilder.Add(F.Assignment(F.Local(stateMachineLocal), F.New(frameType.InstanceConstructors[0])));
		}
	}

	protected override BoundStatement GenerateStateMachineCreation(LocalSymbol stateMachineVariable, NamedTypeSymbol frameType)
	{
		if (!AsyncMethodBuilderMemberCollection.TryCreate(F, method, null, out var collection))
		{
			return new BoundBadStatement(F.Syntax, ImmutableArray<BoundNode>.Empty, hasErrors: true);
		}
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		LocalSymbol localSymbol = F.SynthesizedLocal(collection.BuilderType);
		instance.Add(F.Assignment(F.Field(F.Local(stateMachineVariable), _builderField.AsMember(frameType)), F.StaticCall(null, collection.CreateBuilder)));
		instance.Add(F.Assignment(F.Field(F.Local(stateMachineVariable), stateField.AsMember(frameType)), F.Literal(-1)));
		instance.Add(F.Assignment(F.Local(localSymbol), F.Field(F.Local(stateMachineVariable), _builderField.AsMember(frameType))));
		MethodSymbol methodSymbol = collection.Start.Construct(frameType);
		if (collection.CheckGenericMethodConstraints)
		{
			methodSymbol.CheckConstraints(F.Compilation.Conversions, F.Syntax, F.Compilation, diagnostics);
		}
		instance.Add(F.ExpressionStatement(F.Call(F.Local(localSymbol), methodSymbol, ImmutableArray.Create((BoundExpression)F.Local(stateMachineVariable)))));
		instance.Add(method.IsVoidReturningAsync() ? F.Return() : F.Return(F.Property(F.Field(F.Local(stateMachineVariable), _builderField.AsMember(frameType)), collection.Task)));
		return F.Block(ImmutableArray.Create(localSymbol), instance.ToImmutableAndFree());
	}

	private void GenerateMoveNext(SynthesizedImplementationMethod moveNextMethod)
	{
		new AsyncMethodToStateMachineRewriter(method, _methodOrdinal, _asyncMethodBuilderMemberCollection, F, stateField, _builderField, hoistedVariables, nonReusableLocalProxies, synthesizedLocalOrdinals, slotAllocatorOpt, nextFreeHoistedLocalSlot, diagnostics).GenerateMoveNext(body, moveNextMethod);
	}
}
