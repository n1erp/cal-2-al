using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class AsyncMethodToStateMachineRewriter : MethodToStateMachineRewriter
{
	private enum AwaitableDynamism
	{
		None,
		DynamicTask,
		FullDynamic
	}

	private readonly MethodSymbol _method;

	private readonly FieldSymbol _asyncMethodBuilderField;

	private readonly AsyncMethodBuilderMemberCollection _asyncMethodBuilderMemberCollection;

	private readonly LabelSymbol _exprReturnLabel;

	private readonly LabelSymbol _exitLabel;

	private readonly LocalSymbol _exprRetValue;

	private readonly LoweredDynamicOperationFactory _dynamicFactory;

	private readonly Dictionary<TypeSymbol, FieldSymbol> _awaiterFields;

	private int _nextAwaiterId;

	internal AsyncMethodToStateMachineRewriter(MethodSymbol method, int methodOrdinal, AsyncMethodBuilderMemberCollection asyncMethodBuilderMemberCollection, SyntheticBoundNodeFactory F, FieldSymbol state, FieldSymbol builder, Roslyn.Utilities.IReadOnlySet<Symbol> hoistedVariables, IReadOnlyDictionary<Symbol, CapturedSymbolReplacement> nonReusableLocalProxies, SynthesizedLocalOrdinalsDispenser synthesizedLocalOrdinals, VariableSlotAllocator slotAllocatorOpt, int nextFreeHoistedLocalSlot, DiagnosticBag diagnostics)
		: base(F, method, state, hoistedVariables, nonReusableLocalProxies, synthesizedLocalOrdinals, slotAllocatorOpt, nextFreeHoistedLocalSlot, diagnostics, useFinalizerBookkeeping: false)
	{
		_method = method;
		_asyncMethodBuilderMemberCollection = asyncMethodBuilderMemberCollection;
		_asyncMethodBuilderField = builder;
		_exprReturnLabel = F.GenerateLabel("exprReturn");
		_exitLabel = F.GenerateLabel("exitLabel");
		_exprRetValue = (method.IsGenericTaskReturningAsync(F.Compilation) ? F.SynthesizedLocal(asyncMethodBuilderMemberCollection.ResultType, F.Syntax, isPinned: false, RefKind.None, SynthesizedLocalKind.AsyncMethodReturnValue) : null);
		_dynamicFactory = new LoweredDynamicOperationFactory(F, methodOrdinal);
		_awaiterFields = new Dictionary<TypeSymbol, FieldSymbol>(TypeSymbol.EqualsIgnoringDynamicAndTupleNamesComparer);
		_nextAwaiterId = slotAllocatorOpt?.PreviousAwaiterSlotCount ?? 0;
	}

	private FieldSymbol GetAwaiterField(TypeSymbol awaiterType)
	{
		if (!_awaiterFields.TryGetValue(awaiterType, out var value))
		{
			if (slotAllocatorOpt == null || !slotAllocatorOpt.TryGetPreviousAwaiterSlotIndex(F.ModuleBuilderOpt.Translate(awaiterType, F.Syntax, F.Diagnostics), F.Diagnostics, out var slotIndex))
			{
				slotIndex = _nextAwaiterId++;
			}
			string name = GeneratedNames.AsyncAwaiterFieldName(slotIndex);
			value = F.StateMachineField(awaiterType, name, SynthesizedLocalKind.AwaiterField, slotIndex);
			_awaiterFields.Add(awaiterType, value);
		}
		return value;
	}

	internal void GenerateMoveNext(BoundStatement body, MethodSymbol moveNextMethod)
	{
		F.CurrentFunction = moveNextMethod;
		BoundStatement statement = (BoundStatement)Visit(body);
		MethodToStateMachineRewriter.TryUnwrapBoundStateMachineScope(ref statement, out var hoistedLocals);
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		instance.Add(F.HiddenSequencePoint());
		instance.Add(F.Assignment(F.Local(cachedState), F.Field(F.This(), stateField)));
		instance.Add(CacheThisIfNeeded());
		LocalSymbol localSymbol = F.SynthesizedLocal(F.WellKnownType(WellKnownType.System_Exception));
		instance.Add(F.Try(F.Block(ImmutableArray<LocalSymbol>.Empty, F.HiddenSequencePoint(), Dispatch(), statement), F.CatchBlocks(new BoundCatchBlock(F.Syntax, ImmutableArray.Create(localSymbol), F.Local(localSymbol), localSymbol.Type, null, F.Block(F.Assignment(F.Field(F.This(), stateField), F.Literal(-2)), F.ExpressionStatement(F.Call(F.Field(F.This(), _asyncMethodBuilderField), _asyncMethodBuilderMemberCollection.SetException, F.Local(localSymbol))), GenerateReturn(finished: false)), isSynthesizedAsyncCatchAll: true))));
		instance.Add(F.Label(_exprReturnLabel));
		BoundExpressionStatement boundExpressionStatement = F.Assignment(F.Field(F.This(), stateField), F.Literal(-2));
		if (!(body.Syntax is BlockSyntax blockSyntax))
		{
			instance.Add(boundExpressionStatement);
		}
		else
		{
			instance.Add(F.SequencePointWithSpan(blockSyntax, blockSyntax.CloseBraceToken.Span, boundExpressionStatement));
			instance.Add(F.HiddenSequencePoint());
		}
		instance.Add(F.ExpressionStatement(F.Call(F.Field(F.This(), _asyncMethodBuilderField), _asyncMethodBuilderMemberCollection.SetResult, _method.IsGenericTaskReturningAsync(F.Compilation) ? ImmutableArray.Create((BoundExpression)F.Local(_exprRetValue)) : ImmutableArray<BoundExpression>.Empty)));
		instance.Add(F.Label(_exitLabel));
		instance.Add(F.Return());
		ImmutableArray<BoundStatement> statements = instance.ToImmutableAndFree();
		ArrayBuilder<LocalSymbol> instance2 = ArrayBuilder<LocalSymbol>.GetInstance();
		instance2.Add(cachedState);
		if ((object)cachedThis != null)
		{
			instance2.Add(cachedThis);
		}
		if ((object)_exprRetValue != null)
		{
			instance2.Add(_exprRetValue);
		}
		BoundStatement boundStatement = F.SequencePoint(body.Syntax, F.Block(instance2.ToImmutableAndFree(), statements));
		if (hoistedLocals.Length > 0)
		{
			boundStatement = MakeStateMachineScope(hoistedLocals, boundStatement);
		}
		F.CloseMethod(boundStatement);
	}

	protected override BoundStatement GenerateReturn(bool finished)
	{
		return F.Goto(_exitLabel);
	}

	public override BoundNode VisitExpressionStatement(BoundExpressionStatement node)
	{
		if (node.Expression.Kind == BoundKind.AwaitExpression)
		{
			return VisitAwaitExpression((BoundAwaitExpression)node.Expression, null);
		}
		if (node.Expression.Kind == BoundKind.AssignmentOperator)
		{
			BoundAssignmentOperator boundAssignmentOperator = (BoundAssignmentOperator)node.Expression;
			if (boundAssignmentOperator.Right.Kind == BoundKind.AwaitExpression)
			{
				return VisitAwaitExpression((BoundAwaitExpression)boundAssignmentOperator.Right, boundAssignmentOperator.Left);
			}
		}
		BoundExpression boundExpression = (BoundExpression)Visit(node.Expression);
		if (boundExpression == null)
		{
			return F.StatementList();
		}
		return node.Update(boundExpression);
	}

	public override BoundNode VisitAwaitExpression(BoundAwaitExpression node)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override BoundNode VisitBadExpression(BoundBadExpression node)
	{
		return node;
	}

	private BoundBlock VisitAwaitExpression(BoundAwaitExpression node, BoundExpression resultPlace)
	{
		BoundExpression receiver = (BoundExpression)Visit(node.Expression);
		resultPlace = (BoundExpression)Visit(resultPlace);
		MethodSymbol methodSymbol = VisitMethodSymbol(node.GetAwaiter);
		MethodSymbol methodSymbol2 = VisitMethodSymbol(node.GetResult);
		MethodSymbol getIsCompletedMethod = (((object)node.IsCompleted != null) ? VisitMethodSymbol(node.IsCompleted.GetMethod) : null);
		TypeSymbol typeSymbol = VisitType(node.Type);
		TypeSymbol type = (node.IsDynamic ? DynamicTypeSymbol.Instance : methodSymbol.ReturnType);
		LocalSymbol localSymbol = F.SynthesizedLocal(type, node.Syntax, isPinned: false, RefKind.None, SynthesizedLocalKind.Awaiter);
		BoundBlock boundBlock = F.Block(F.Assignment(F.Local(localSymbol), MakeCallMaybeDynamic(receiver, methodSymbol, "GetAwaiter")), F.HiddenSequencePoint(), F.If(F.Not(GenerateGetIsCompleted(localSymbol, getIsCompletedMethod)), GenerateAwaitForIncompleteTask(localSymbol)));
		BoundExpression boundExpression = MakeCallMaybeDynamic(F.Local(localSymbol), methodSymbol2, "GetResult", resultPlace == null);
		BoundStatement boundStatement = ((resultPlace != null && typeSymbol.SpecialType != SpecialType.System_Void) ? F.Assignment(resultPlace, boundExpression) : F.ExpressionStatement(boundExpression));
		return F.Block(ImmutableArray.Create(localSymbol), boundBlock, boundStatement);
	}

	private BoundExpression MakeCallMaybeDynamic(BoundExpression receiver, MethodSymbol methodSymbol = null, string methodName = null, bool resultsDiscarded = false)
	{
		if ((object)methodSymbol != null)
		{
			if (!methodSymbol.IsStatic)
			{
				return F.Call(receiver, methodSymbol);
			}
			return F.StaticCall(methodSymbol.ContainingType, methodSymbol, receiver);
		}
		return _dynamicFactory.MakeDynamicMemberInvocation(methodName, receiver, ImmutableArray<TypeSymbol>.Empty, ImmutableArray<BoundExpression>.Empty, ImmutableArray<string>.Empty, ImmutableArray<RefKind>.Empty, hasImplicitReceiver: false, resultsDiscarded).ToExpression();
	}

	private BoundExpression GenerateGetIsCompleted(LocalSymbol awaiterTemp, MethodSymbol getIsCompletedMethod)
	{
		if (awaiterTemp.Type.IsDynamic())
		{
			return _dynamicFactory.MakeDynamicConversion(_dynamicFactory.MakeDynamicGetMember(F.Local(awaiterTemp), "IsCompleted", resultIndexed: false).ToExpression(), isExplicit: true, isArrayIndex: false, isChecked: false, F.SpecialType(SpecialType.System_Boolean)).ToExpression();
		}
		return F.Call(F.Local(awaiterTemp), getIsCompletedMethod);
	}

	private BoundBlock GenerateAwaitForIncompleteTask(LocalSymbol awaiterTemp)
	{
		AddState(out var stateNumber, out var resumeLabel);
		TypeSymbol typeSymbol = (awaiterTemp.Type.IsVerifierReference() ? F.SpecialType(SpecialType.System_Object) : awaiterTemp.Type);
		FieldSymbol awaiterField = GetAwaiterField(typeSymbol);
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		instance.Add(F.Assignment(F.Field(F.This(), stateField), F.AssignmentExpression(F.Local(cachedState), F.Literal(stateNumber))));
		instance.Add(F.NoOp(NoOpStatementFlavor.AwaitYieldPoint));
		instance.Add(F.Assignment(F.Field(F.This(), awaiterField), (awaiterField.Type == awaiterTemp.Type) ? F.Local(awaiterTemp) : F.Convert(typeSymbol, F.Local(awaiterTemp))));
		instance.Add(awaiterTemp.Type.IsDynamic() ? GenerateAwaitOnCompletedDynamic(awaiterTemp) : GenerateAwaitOnCompleted(awaiterTemp.Type, awaiterTemp));
		instance.Add(GenerateReturn(finished: false));
		instance.Add(F.Label(resumeLabel));
		instance.Add(F.NoOp(NoOpStatementFlavor.AwaitResumePoint));
		instance.Add(F.Assignment(F.Local(awaiterTemp), (awaiterTemp.Type == awaiterField.Type) ? F.Field(F.This(), awaiterField) : F.Convert(awaiterTemp.Type, F.Field(F.This(), awaiterField))));
		instance.Add(F.Assignment(F.Field(F.This(), awaiterField), F.NullOrDefault(awaiterField.Type)));
		instance.Add(F.Assignment(F.Field(F.This(), stateField), F.AssignmentExpression(F.Local(cachedState), F.Literal(-1))));
		return F.Block(instance.ToImmutableAndFree());
	}

	private BoundStatement GenerateAwaitOnCompletedDynamic(LocalSymbol awaiterTemp)
	{
		LocalSymbol localSymbol = F.SynthesizedLocal(F.WellKnownType(WellKnownType.System_Runtime_CompilerServices_ICriticalNotifyCompletion));
		LocalSymbol localSymbol2 = F.SynthesizedLocal(F.WellKnownType(WellKnownType.System_Runtime_CompilerServices_INotifyCompletion));
		LocalSymbol localSymbol3 = ((F.CurrentType.TypeKind == TypeKind.Class) ? F.SynthesizedLocal(F.CurrentType) : null);
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		instance.Add(F.Assignment(F.Local(localSymbol), F.As(F.Local(awaiterTemp), localSymbol.Type)));
		if (localSymbol3 != null)
		{
			instance.Add(F.Assignment(F.Local(localSymbol3), F.This()));
		}
		instance.Add(F.If(F.ObjectEqual(F.Local(localSymbol), F.Null(localSymbol.Type)), F.Block(ImmutableArray.Create(localSymbol2), F.Assignment(F.Local(localSymbol2), F.Convert(localSymbol2.Type, F.Local(awaiterTemp), Conversion.ExplicitReference)), F.ExpressionStatement(F.Call(F.Field(F.This(), _asyncMethodBuilderField), _asyncMethodBuilderMemberCollection.AwaitOnCompleted.Construct(localSymbol2.Type, F.This().Type), F.Local(localSymbol2), F.This(localSymbol3))), F.Assignment(F.Local(localSymbol2), F.NullOrDefault(localSymbol2.Type))), F.Block(F.ExpressionStatement(F.Call(F.Field(F.This(), _asyncMethodBuilderField), _asyncMethodBuilderMemberCollection.AwaitUnsafeOnCompleted.Construct(localSymbol.Type, F.This().Type), F.Local(localSymbol), F.This(localSymbol3))))));
		instance.Add(F.Assignment(F.Local(localSymbol), F.NullOrDefault(localSymbol.Type)));
		return F.Block(SingletonOrPair(localSymbol, localSymbol3), instance.ToImmutableAndFree());
	}

	private BoundStatement GenerateAwaitOnCompleted(TypeSymbol loweredAwaiterType, LocalSymbol awaiterTemp)
	{
		LocalSymbol localSymbol = ((F.CurrentType.TypeKind == TypeKind.Class) ? F.SynthesizedLocal(F.CurrentType) : null);
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		MethodSymbol method = (F.Compilation.Conversions.ClassifyImplicitConversionFromType(loweredAwaiterType, F.Compilation.GetWellKnownType(WellKnownType.System_Runtime_CompilerServices_ICriticalNotifyCompletion), ref useSiteDiagnostics).IsImplicit ? _asyncMethodBuilderMemberCollection.AwaitUnsafeOnCompleted : _asyncMethodBuilderMemberCollection.AwaitOnCompleted).Construct(loweredAwaiterType, F.This().Type);
		if (_asyncMethodBuilderMemberCollection.CheckGenericMethodConstraints)
		{
			method.CheckConstraints(F.Compilation.Conversions, F.Syntax, F.Compilation, Diagnostics);
		}
		BoundExpression boundExpression = F.Call(F.Field(F.This(), _asyncMethodBuilderField), method, F.Local(awaiterTemp), F.This(localSymbol));
		if (localSymbol != null)
		{
			boundExpression = F.Sequence(ImmutableArray.Create(localSymbol), ImmutableArray.Create((BoundExpression)F.AssignmentExpression(F.Local(localSymbol), F.This())), boundExpression);
		}
		return F.ExpressionStatement(boundExpression);
	}

	private static ImmutableArray<LocalSymbol> SingletonOrPair(LocalSymbol first, LocalSymbol secondOpt)
	{
		if (!(secondOpt == null))
		{
			return ImmutableArray.Create(first, secondOpt);
		}
		return ImmutableArray.Create(first);
	}

	public override BoundNode VisitReturnStatement(BoundReturnStatement node)
	{
		if (node.ExpressionOpt != null)
		{
			return F.Block(F.Assignment(F.Local(_exprRetValue), (BoundExpression)Visit(node.ExpressionOpt)), F.Goto(_exprReturnLabel));
		}
		return F.Goto(_exprReturnLabel);
	}
}
