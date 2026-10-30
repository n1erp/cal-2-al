using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.CodeGen;

internal sealed class StackOptimizerPass1 : BoundTreeRewriter
{
	private sealed class LocalUsedWalker : BoundTreeWalkerWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator
	{
		private readonly LocalSymbol _local;

		private bool _found;

		internal LocalUsedWalker(LocalSymbol local, int recursionDepth)
			: base(recursionDepth)
		{
			_local = local;
		}

		public bool IsLocalUsedIn(BoundExpression node)
		{
			_found = false;
			Visit(node);
			return _found;
		}

		public override BoundNode Visit(BoundNode node)
		{
			if (!_found)
			{
				return base.Visit(node);
			}
			return null;
		}

		public override BoundNode VisitLocal(BoundLocal node)
		{
			if (node.LocalSymbol == _local)
			{
				_found = true;
			}
			return null;
		}
	}

	private readonly bool _debugFriendly;

	private readonly ArrayBuilder<(BoundExpression, ExprContext)> _evalStack;

	private int _counter;

	private ExprContext _context;

	private BoundLocal _assignmentLocal;

	private readonly Dictionary<LocalSymbol, LocalDefUseInfo> _locals = new Dictionary<LocalSymbol, LocalDefUseInfo>();

	private readonly SmallDictionary<object, DummyLocal> _dummyVariables = new SmallDictionary<object, DummyLocal>(Roslyn.Utilities.ReferenceEqualityComparer.Instance);

	public static readonly DummyLocal empty = new DummyLocal();

	private int _recursionDepth;

	private StackOptimizerPass1(Dictionary<LocalSymbol, LocalDefUseInfo> locals, ArrayBuilder<(BoundExpression, ExprContext)> evalStack, bool debugFriendly)
	{
		_locals = locals;
		_evalStack = evalStack;
		_debugFriendly = debugFriendly;
		DeclareLocal(empty, 0);
		RecordDummyWrite(empty);
	}

	public static BoundNode Analyze(BoundNode node, Dictionary<LocalSymbol, LocalDefUseInfo> locals, bool debugFriendly)
	{
		ArrayBuilder<(BoundExpression, ExprContext)> instance = ArrayBuilder<(BoundExpression, ExprContext)>.GetInstance();
		BoundNode result = new StackOptimizerPass1(locals, instance, debugFriendly).Visit(node);
		instance.Free();
		return result;
	}

	public override BoundNode Visit(BoundNode node)
	{
		if (node is BoundExpression node2)
		{
			return VisitExpression(node2, ExprContext.Value);
		}
		return VisitStatement(node);
	}

	private BoundExpression VisitExpressionCore(BoundExpression node, ExprContext context)
	{
		ExprContext context2 = _context;
		int stackDepth = StackDepth();
		_context = context;
		BoundExpression result = ((!(node.ConstantValue == null)) ? node : (node = (BoundExpression)base.Visit(node)));
		_context = context2;
		_counter++;
		switch (context)
		{
		case ExprContext.Sideeffects:
			SetStackDepth(stackDepth);
			break;
		case ExprContext.Value:
		case ExprContext.Address:
		case ExprContext.Box:
			SetStackDepth(stackDepth);
			PushEvalStack(node, context);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(context);
		case ExprContext.AssignmentTarget:
			break;
		}
		return result;
	}

	private BoundExpression VisitExpression(BoundExpression node, ExprContext context)
	{
		_recursionDepth++;
		BoundExpression result;
		if (_recursionDepth > 1)
		{
			StackGuard.EnsureSufficientExecutionStack(_recursionDepth);
			result = VisitExpressionCore(node, context);
		}
		else
		{
			result = VisitExpressionCoreWithStackGuard(node, context);
		}
		_recursionDepth--;
		return result;
	}

	private BoundExpression VisitExpressionCoreWithStackGuard(BoundExpression node, ExprContext context)
	{
		try
		{
			return VisitExpressionCore(node, context);
		}
		catch (Exception ex) when (StackGuard.IsInsufficientExecutionStackException(ex))
		{
			throw new CancelledByStackGuardException(ex, node);
		}
	}

	protected override BoundExpression VisitExpressionWithoutStackGuard(BoundExpression node)
	{
		throw ExceptionUtilities.Unreachable;
	}

	private void PushEvalStack(BoundExpression result, ExprContext context)
	{
		_evalStack.Add((result, context));
	}

	private int StackDepth()
	{
		return _evalStack.Count;
	}

	private bool EvalStackIsEmpty()
	{
		return StackDepth() == 0;
	}

	private void SetStackDepth(int depth)
	{
		_evalStack.Clip(depth);
	}

	private void PopEvalStack()
	{
		SetStackDepth(_evalStack.Count - 1);
	}

	private void ClearEvalStack()
	{
		_evalStack.Clear();
	}

	public BoundNode VisitStatement(BoundNode node)
	{
		int stackDepth = StackDepth();
		ExprContext context = _context;
		BoundNode result = base.Visit(node);
		if (_debugFriendly)
		{
			EnsureOnlyEvalStack();
		}
		_context = context;
		SetStackDepth(stackDepth);
		_counter++;
		return result;
	}

	public override BoundNode VisitConversion(BoundConversion node)
	{
		ExprContext context = ((_context == ExprContext.Sideeffects && !node.ConversionHasSideEffects()) ? ExprContext.Sideeffects : ExprContext.Value);
		return node.Update(VisitExpression(node.Operand, context), node.Conversion, node.IsBaseConversion, node.Checked, node.ExplicitCastInCode, node.ConstantValue, node.Type);
	}

	public override BoundNode VisitPassByCopy(BoundPassByCopy node)
	{
		ExprContext context = ((_context == ExprContext.Sideeffects) ? ExprContext.Sideeffects : ExprContext.Value);
		return node.Update(VisitExpression(node.Expression, context), node.Type);
	}

	public override BoundNode VisitBlock(BoundBlock node)
	{
		DeclareLocals(node.Locals, 0);
		return base.VisitBlock(node);
	}

	public override BoundNode VisitSequence(BoundSequence node)
	{
		int num = StackDepth();
		ImmutableArray<LocalSymbol> locals = node.Locals;
		if (!locals.IsDefaultOrEmpty)
		{
			if (_context == ExprContext.Sideeffects)
			{
				foreach (LocalSymbol item in locals)
				{
					if (IsNestedLocalOfCompoundOperator(item, node))
					{
						DeclareLocal(item, num + 1);
					}
					else
					{
						DeclareLocal(item, num);
					}
				}
			}
			else
			{
				DeclareLocals(locals, num);
			}
		}
		ExprContext context = _context;
		ImmutableArray<BoundExpression> sideEffects = node.SideEffects;
		ArrayBuilder<BoundExpression> arrayBuilder = null;
		if (!sideEffects.IsDefault)
		{
			for (int i = 0; i < sideEffects.Length; i++)
			{
				BoundExpression boundExpression = sideEffects[i];
				BoundExpression boundExpression2 = VisitExpression(boundExpression, ExprContext.Sideeffects);
				if (arrayBuilder == null && boundExpression2 != boundExpression)
				{
					arrayBuilder = ArrayBuilder<BoundExpression>.GetInstance();
					arrayBuilder.AddRange(sideEffects, i);
				}
				arrayBuilder?.Add(boundExpression2);
			}
		}
		BoundExpression value = VisitExpression(node.Value, context);
		return node.Update(node.Locals, arrayBuilder?.ToImmutableAndFree() ?? sideEffects, value, node.Type);
	}

	private bool IsNestedLocalOfCompoundOperator(LocalSymbol local, BoundSequence node)
	{
		BoundExpression value = node.Value;
		if (value != null && value.Kind == BoundKind.Local && ((BoundLocal)value).LocalSymbol == local)
		{
			ImmutableArray<BoundExpression> sideEffects = node.SideEffects;
			BoundExpression boundExpression = sideEffects.LastOrDefault();
			if (boundExpression != null && boundExpression.Kind == BoundKind.AssignmentOperator)
			{
				BoundAssignmentOperator boundAssignmentOperator = (BoundAssignmentOperator)boundExpression;
				if (IsIndirectOrInstanceFieldAssignment(boundAssignmentOperator) && boundAssignmentOperator.Right.Kind == BoundKind.Sequence)
				{
					LocalUsedWalker localUsedWalker = new LocalUsedWalker(local, _recursionDepth);
					for (int i = 0; i < sideEffects.Length - 1; i++)
					{
						if (localUsedWalker.IsLocalUsedIn(sideEffects[i]))
						{
							return false;
						}
					}
					if (localUsedWalker.IsLocalUsedIn(boundAssignmentOperator.Left))
					{
						return false;
					}
					return true;
				}
			}
		}
		return false;
	}

	public override BoundNode VisitExpressionStatement(BoundExpressionStatement node)
	{
		return node.Update(VisitExpression(node.Expression, ExprContext.Sideeffects));
	}

	public override BoundNode VisitLocal(BoundLocal node)
	{
		if (node.ConstantValueOpt == null)
		{
			switch (_context)
			{
			case ExprContext.Address:
				if (node.LocalSymbol.RefKind != RefKind.None)
				{
					RecordVarRead(node.LocalSymbol);
				}
				else
				{
					RecordVarRef(node.LocalSymbol);
				}
				break;
			case ExprContext.AssignmentTarget:
				_assignmentLocal = node;
				break;
			case ExprContext.Value:
			case ExprContext.Box:
				RecordVarRead(node.LocalSymbol);
				break;
			}
		}
		return base.VisitLocal(node);
	}

	public override BoundNode VisitAssignmentOperator(BoundAssignmentOperator node)
	{
		if (node.Left is BoundSequence boundSequence)
		{
			BoundExpression node2 = boundSequence.Update(boundSequence.Locals, boundSequence.SideEffects, node.Update(boundSequence.Value, node.Right, node.IsRef, node.Type), boundSequence.Type);
			node2 = (BoundExpression)Visit(node2);
			_counter--;
			return node2;
		}
		bool flag = IsIndirectAssignment(node);
		BoundExpression left = VisitExpression(node.Left, flag ? ExprContext.Address : ExprContext.AssignmentTarget);
		BoundLocal assignmentLocal = _assignmentLocal;
		_assignmentLocal = null;
		ExprContext context = ((!node.IsRef && _context != ExprContext.Address) ? ExprContext.Value : ExprContext.Address);
		BoundExpression right = node.Right;
		int num;
		if (right.Kind == BoundKind.ObjectCreationExpression && right.Type.IsVerifierValue())
		{
			num = ((((BoundObjectCreationExpression)right).Constructor.ParameterCount != 0) ? 1 : 0);
			if (num != 0)
			{
				PushEvalStack(null, ExprContext.None);
			}
		}
		else
		{
			num = 0;
		}
		right = VisitExpression(node.Right, context);
		if (num != 0)
		{
			PopEvalStack();
		}
		if (assignmentLocal != null)
		{
			LocalSymbol localSymbol = assignmentLocal.LocalSymbol;
			if (localSymbol.RefKind == RefKind.In && (_context == ExprContext.Address || _context == ExprContext.Value))
			{
				ShouldNotSchedule(localSymbol);
			}
			if (CanScheduleToStack(localSymbol) && assignmentLocal.Type.IsPointerType() && right.Kind == BoundKind.Conversion && ((BoundConversion)right).ConversionKind.IsPointerConversion())
			{
				ShouldNotSchedule(localSymbol);
			}
			RecordVarWrite(localSymbol);
		}
		return node.Update(left, right, node.IsRef, node.Type);
	}

	internal static bool IsFixedBufferAssignmentToRefLocal(BoundExpression left, BoundExpression right, bool isRef)
	{
		if (isRef && right is BoundFieldAccess boundFieldAccess && boundFieldAccess.FieldSymbol.IsFixed)
		{
			return left.Type.Equals(((PointerTypeSymbol)right.Type).PointedAtType, TypeCompareKind.AllIgnoreOptions);
		}
		return false;
	}

	private static bool IsIndirectAssignment(BoundAssignmentOperator node)
	{
		BoundExpression left = node.Left;
		switch (left.Kind)
		{
		case BoundKind.ThisReference:
			return true;
		case BoundKind.Parameter:
			if (((BoundParameter)left).ParameterSymbol.RefKind != RefKind.None)
			{
				return !node.IsRef;
			}
			return false;
		case BoundKind.Local:
			if (((BoundLocal)left).LocalSymbol.RefKind != RefKind.None)
			{
				return !node.IsRef;
			}
			return false;
		case BoundKind.Call:
			return true;
		case BoundKind.ConditionalOperator:
			return true;
		case BoundKind.AssignmentOperator:
			return true;
		case BoundKind.Sequence:
			return false;
		case BoundKind.PointerIndirectionOperator:
		case BoundKind.RefValueOperator:
		case BoundKind.PseudoVariable:
			return true;
		case BoundKind.ArrayAccess:
		case BoundKind.InstrumentationPayloadRoot:
		case BoundKind.ModuleVersionId:
		case BoundKind.FieldAccess:
			return false;
		default:
			throw ExceptionUtilities.UnexpectedValue(left.Kind);
		}
	}

	private static bool IsIndirectOrInstanceFieldAssignment(BoundAssignmentOperator node)
	{
		BoundExpression left = node.Left;
		if (left.Kind == BoundKind.FieldAccess)
		{
			return !((BoundFieldAccess)left).FieldSymbol.IsStatic;
		}
		return IsIndirectAssignment(node);
	}

	public override BoundNode VisitCall(BoundCall node)
	{
		BoundExpression receiverOpt = node.ReceiverOpt;
		if (!node.Method.IsStatic)
		{
			receiverOpt = VisitCallReceiver(receiverOpt);
		}
		else
		{
			_counter++;
			receiverOpt = null;
		}
		MethodSymbol method = node.Method;
		ImmutableArray<BoundExpression> arguments = VisitArguments(node.Arguments, method.Parameters, node.ArgumentRefKindsOpt);
		return node.Update(receiverOpt, method, arguments);
	}

	private BoundExpression VisitCallReceiver(BoundExpression receiver)
	{
		TypeSymbol type = receiver.Type;
		ExprContext context;
		if (type.IsReferenceType)
		{
			context = ((!type.IsTypeParameter()) ? ExprContext.Value : ExprContext.Box);
		}
		else
		{
			context = ExprContext.Address;
		}
		receiver = VisitExpression(receiver, context);
		return receiver;
	}

	private ImmutableArray<BoundExpression> VisitArguments(ImmutableArray<BoundExpression> arguments, ImmutableArray<ParameterSymbol> parameters, ImmutableArray<RefKind> argRefKindsOpt)
	{
		ArrayBuilder<BoundExpression> rewrittenArguments = null;
		for (int i = 0; i < arguments.Length; i++)
		{
			RefKind argumentRefKind = CodeGenerator.GetArgumentRefKind(arguments, parameters, argRefKindsOpt, i);
			VisitArgument(arguments, ref rewrittenArguments, i, argumentRefKind);
		}
		return rewrittenArguments?.ToImmutableAndFree() ?? arguments;
	}

	private void VisitArgument(ImmutableArray<BoundExpression> arguments, ref ArrayBuilder<BoundExpression> rewrittenArguments, int i, RefKind argRefKind)
	{
		ExprContext context = ((argRefKind == RefKind.None) ? ExprContext.Value : ExprContext.Address);
		BoundExpression boundExpression = arguments[i];
		BoundExpression boundExpression2 = VisitExpression(boundExpression, context);
		if (rewrittenArguments == null && boundExpression != boundExpression2)
		{
			rewrittenArguments = ArrayBuilder<BoundExpression>.GetInstance();
			rewrittenArguments.AddRange(arguments, i);
		}
		if (rewrittenArguments != null)
		{
			rewrittenArguments.Add(boundExpression2);
		}
	}

	public override BoundNode VisitArgListOperator(BoundArgListOperator node)
	{
		ArrayBuilder<BoundExpression> rewrittenArguments = null;
		ImmutableArray<BoundExpression> arguments = node.Arguments;
		ImmutableArray<RefKind> argumentRefKindsOpt = node.ArgumentRefKindsOpt;
		for (int i = 0; i < arguments.Length; i++)
		{
			RefKind argRefKind = ((!argumentRefKindsOpt.IsDefaultOrEmpty) ? argumentRefKindsOpt[i] : RefKind.None);
			VisitArgument(arguments, ref rewrittenArguments, i, argRefKind);
		}
		return node.Update(rewrittenArguments?.ToImmutableAndFree() ?? arguments, argumentRefKindsOpt, node.Type);
	}

	public override BoundNode VisitMakeRefOperator(BoundMakeRefOperator node)
	{
		BoundExpression operand = VisitExpression(node.Operand, ExprContext.Address);
		return node.Update(operand, node.Type);
	}

	public override BoundNode VisitObjectCreationExpression(BoundObjectCreationExpression node)
	{
		MethodSymbol constructor = node.Constructor;
		ImmutableArray<BoundExpression> arguments = VisitArguments(node.Arguments, constructor.Parameters, node.ArgumentRefKindsOpt);
		return node.Update(constructor, arguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, node.Expanded, node.ArgsToParamsOpt, node.ConstantValue, null, node.BinderOpt, node.Type);
	}

	public override BoundNode VisitArrayAccess(BoundArrayAccess node)
	{
		ExprContext context = _context;
		_context = ExprContext.Value;
		BoundNode result = base.VisitArrayAccess(node);
		_context = context;
		return result;
	}

	public override BoundNode VisitFieldAccess(BoundFieldAccess node)
	{
		FieldSymbol fieldSymbol = node.FieldSymbol;
		BoundExpression receiverOpt = node.ReceiverOpt;
		if (!fieldSymbol.IsStatic)
		{
			if (receiverOpt.Type.IsTypeParameter())
			{
				receiverOpt = VisitExpression(receiverOpt, ExprContext.Box);
			}
			else
			{
				receiverOpt = ((!receiverOpt.Type.IsValueType || (_context != ExprContext.AssignmentTarget && _context != ExprContext.Address && !CodeGenerator.FieldLoadMustUseRef(receiverOpt))) ? VisitExpression(receiverOpt, ExprContext.Value) : VisitExpression(receiverOpt, ExprContext.Address));
			}
		}
		else
		{
			_counter++;
			receiverOpt = null;
		}
		return node.Update(receiverOpt, fieldSymbol, node.ConstantValueOpt, node.ResultKind, node.Type);
	}

	public override BoundNode VisitLabelStatement(BoundLabelStatement node)
	{
		RecordLabel(node.Label);
		return base.VisitLabelStatement(node);
	}

	public override BoundNode VisitLabel(BoundLabel node)
	{
		return node;
	}

	public override BoundNode VisitIsPatternExpression(BoundIsPatternExpression node)
	{
		return node;
	}

	public override BoundNode VisitGotoStatement(BoundGotoStatement node)
	{
		BoundNode result = base.VisitGotoStatement(node);
		RecordBranch(node.Label);
		return result;
	}

	public override BoundNode VisitConditionalGoto(BoundConditionalGoto node)
	{
		BoundNode result = base.VisitConditionalGoto(node);
		PopEvalStack();
		RecordBranch(node.Label);
		return result;
	}

	public override BoundNode VisitConditionalOperator(BoundConditionalOperator node)
	{
		int stackDepth = StackDepth();
		BoundExpression condition = VisitExpression(node.Condition, ExprContext.Value);
		object stackStateCookie = GetStackStateCookie();
		ExprContext context = (node.IsRef ? ExprContext.Address : ExprContext.Value);
		SetStackDepth(stackDepth);
		BoundExpression consequence = VisitExpression(node.Consequence, context);
		EnsureStackState(stackStateCookie);
		SetStackDepth(stackDepth);
		BoundExpression alternative = VisitExpression(node.Alternative, context);
		EnsureStackState(stackStateCookie);
		return node.Update(node.IsRef, condition, consequence, alternative, node.ConstantValueOpt, node.Type);
	}

	public override BoundNode VisitBinaryOperator(BoundBinaryOperator node)
	{
		BoundExpression left = node.Left;
		if (left.Kind != BoundKind.BinaryOperator || left.ConstantValue != null)
		{
			return VisitBinaryOperatorSimple(node);
		}
		ArrayBuilder<BoundBinaryOperator> instance = ArrayBuilder<BoundBinaryOperator>.GetInstance();
		instance.Push(node);
		BoundBinaryOperator boundBinaryOperator = (BoundBinaryOperator)left;
		while (true)
		{
			instance.Push(boundBinaryOperator);
			left = boundBinaryOperator.Left;
			if (left.Kind != BoundKind.BinaryOperator || left.ConstantValue != null)
			{
				break;
			}
			boundBinaryOperator = (BoundBinaryOperator)left;
		}
		ExprContext context = _context;
		int stackDepth = StackDepth();
		BoundExpression boundExpression = (BoundExpression)Visit(left);
		while (true)
		{
			boundBinaryOperator = instance.Pop();
			bool flag = (boundBinaryOperator.OperatorKind & BinaryOperatorKind.Logical) != 0;
			object cookie = null;
			if (flag)
			{
				cookie = GetStackStateCookie();
				SetStackDepth(stackDepth);
			}
			BoundExpression right = (BoundExpression)Visit(boundBinaryOperator.Right);
			if (flag)
			{
				EnsureStackState(cookie);
			}
			TypeSymbol type = VisitType(boundBinaryOperator.Type);
			boundExpression = boundBinaryOperator.Update(boundBinaryOperator.OperatorKind, boundExpression, right, boundBinaryOperator.ConstantValueOpt, boundBinaryOperator.MethodOpt, boundBinaryOperator.ResultKind, type);
			if (instance.Count == 0)
			{
				break;
			}
			_context = context;
			_counter++;
			SetStackDepth(stackDepth);
			PushEvalStack(boundBinaryOperator, ExprContext.Value);
		}
		instance.Free();
		return boundExpression;
	}

	private BoundNode VisitBinaryOperatorSimple(BoundBinaryOperator node)
	{
		if ((node.OperatorKind & BinaryOperatorKind.Logical) != 0)
		{
			int stackDepth = StackDepth();
			BoundExpression left = (BoundExpression)Visit(node.Left);
			object stackStateCookie = GetStackStateCookie();
			SetStackDepth(stackDepth);
			BoundExpression right = (BoundExpression)Visit(node.Right);
			EnsureStackState(stackStateCookie);
			return node.Update(node.OperatorKind, left, right, node.ConstantValueOpt, node.MethodOpt, node.ResultKind, node.Type);
		}
		return base.VisitBinaryOperator(node);
	}

	public override BoundNode VisitNullCoalescingOperator(BoundNullCoalescingOperator node)
	{
		int stackDepth = StackDepth();
		BoundExpression leftOperand = (BoundExpression)Visit(node.LeftOperand);
		object stackStateCookie = GetStackStateCookie();
		SetStackDepth(stackDepth);
		BoundExpression rightOperand = (BoundExpression)Visit(node.RightOperand);
		EnsureStackState(stackStateCookie);
		return node.Update(leftOperand, rightOperand, node.LeftConversion, node.Type);
	}

	public override BoundNode VisitLoweredConditionalAccess(BoundLoweredConditionalAccess node)
	{
		int stackDepth = StackDepth();
		BoundExpression receiver = VisitCallReceiver(node.Receiver);
		object stackStateCookie = GetStackStateCookie();
		SetStackDepth(stackDepth);
		BoundExpression whenNotNull = (BoundExpression)Visit(node.WhenNotNull);
		EnsureStackState(stackStateCookie);
		BoundExpression boundExpression = node.WhenNullOpt;
		if (boundExpression != null)
		{
			SetStackDepth(stackDepth);
			boundExpression = (BoundExpression)Visit(boundExpression);
			EnsureStackState(stackStateCookie);
		}
		else
		{
			_counter++;
		}
		return node.Update(receiver, node.HasValueMethodOpt, whenNotNull, boundExpression, node.Id, node.Type);
	}

	public override BoundNode VisitComplexConditionalReceiver(BoundComplexConditionalReceiver node)
	{
		EnsureOnlyEvalStack();
		int stackDepth = StackDepth();
		PushEvalStack(null, ExprContext.None);
		object stackStateCookie = GetStackStateCookie();
		SetStackDepth(stackDepth);
		BoundExpression valueTypeReceiver = (BoundExpression)Visit(node.ValueTypeReceiver);
		EnsureStackState(stackStateCookie);
		SetStackDepth(stackDepth);
		BoundExpression referenceTypeReceiver = (BoundExpression)Visit(node.ReferenceTypeReceiver);
		EnsureStackState(stackStateCookie);
		return node.Update(valueTypeReceiver, referenceTypeReceiver, node.Type);
	}

	public override BoundNode VisitUnaryOperator(BoundUnaryOperator node)
	{
		if (node.OperatorKind.IsChecked() && node.OperatorKind.Operator() == UnaryOperatorKind.UnaryMinus)
		{
			StackDepth();
			PushEvalStack(new BoundDefaultExpression(node.Syntax, node.Operand.Type), ExprContext.Value);
			BoundExpression operand = (BoundExpression)Visit(node.Operand);
			return node.Update(node.OperatorKind, operand, node.ConstantValueOpt, node.MethodOpt, node.ResultKind, node.Type);
		}
		return base.VisitUnaryOperator(node);
	}

	public override BoundNode VisitSwitchStatement(BoundSwitchStatement node)
	{
		BoundStatement loweredPreambleOpt = (BoundStatement)Visit(node.LoweredPreambleOpt);
		DeclareLocals(node.InnerLocals, 0);
		BoundExpression expression = node.Expression;
		if (expression.Kind == BoundKind.Local)
		{
			LocalSymbol localSymbol = ((BoundLocal)expression).LocalSymbol;
			if (localSymbol.RefKind == RefKind.None)
			{
				ShouldNotSchedule(localSymbol);
			}
		}
		expression = (BoundExpression)Visit(expression);
		PopEvalStack();
		EnsureOnlyEvalStack();
		ImmutableArray<BoundSwitchSection> switchSections = VisitList(node.SwitchSections);
		GeneratedLabelSymbol breakLabel = node.BreakLabel;
		if (breakLabel != null)
		{
			RecordLabel(breakLabel);
		}
		BoundSwitchStatement result = node.Update(loweredPreambleOpt, expression, node.ConstantTargetOpt, node.InnerLocals, node.InnerLocalFunctions, switchSections, breakLabel, node.StringEquality);
		EnsureOnlyEvalStack();
		return result;
	}

	public override BoundNode VisitSwitchSection(BoundSwitchSection node)
	{
		EnsureOnlyEvalStack();
		return base.VisitSwitchSection(node);
	}

	public override BoundNode VisitSwitchLabel(BoundSwitchLabel node)
	{
		RecordLabel(node.Label);
		return base.VisitSwitchLabel(node);
	}

	public override BoundNode VisitTryStatement(BoundTryStatement node)
	{
		EnsureOnlyEvalStack();
		BoundBlock tryBlock = (BoundBlock)Visit(node.TryBlock);
		ImmutableArray<BoundCatchBlock> catchBlocks = VisitList(node.CatchBlocks);
		EnsureOnlyEvalStack();
		BoundBlock finallyBlockOpt = (BoundBlock)Visit(node.FinallyBlockOpt);
		EnsureOnlyEvalStack();
		return node.Update(tryBlock, catchBlocks, finallyBlockOpt, node.PreferFaultHandler);
	}

	public override BoundNode VisitCatchBlock(BoundCatchBlock node)
	{
		EnsureOnlyEvalStack();
		BoundExpression boundExpression = node.ExceptionSourceOpt;
		DeclareLocals(node.Locals, 0);
		if (boundExpression != null)
		{
			PushEvalStack(null, ExprContext.None);
			_counter++;
			if (boundExpression.Kind == BoundKind.Local)
			{
				RecordVarWrite(((BoundLocal)boundExpression).LocalSymbol);
			}
			else
			{
				int stackDepth = StackDepth();
				boundExpression = VisitExpression(boundExpression, ExprContext.AssignmentTarget);
				_assignmentLocal = null;
				SetStackDepth(stackDepth);
			}
			PopEvalStack();
			_counter++;
		}
		BoundExpression exceptionFilterOpt;
		if (node.ExceptionFilterOpt != null)
		{
			exceptionFilterOpt = (BoundExpression)Visit(node.ExceptionFilterOpt);
			PopEvalStack();
			_counter++;
			EnsureOnlyEvalStack();
		}
		else
		{
			exceptionFilterOpt = null;
		}
		BoundBlock body = (BoundBlock)Visit(node.Body);
		TypeSymbol exceptionTypeOpt = VisitType(node.ExceptionTypeOpt);
		return node.Update(node.Locals, boundExpression, exceptionTypeOpt, exceptionFilterOpt, body, node.IsSynthesizedAsyncCatchAll);
	}

	public override BoundNode VisitConvertedStackAllocExpression(BoundConvertedStackAllocExpression node)
	{
		EnsureOnlyEvalStack();
		return base.VisitConvertedStackAllocExpression(node);
	}

	public override BoundNode VisitArrayInitialization(BoundArrayInitialization node)
	{
		EnsureOnlyEvalStack();
		ImmutableArray<BoundExpression> initializers = node.Initializers;
		ArrayBuilder<BoundExpression> arrayBuilder = null;
		if (!initializers.IsDefault)
		{
			for (int i = 0; i < initializers.Length; i++)
			{
				EnsureOnlyEvalStack();
				BoundExpression boundExpression = initializers[i];
				BoundExpression boundExpression2 = VisitExpression(boundExpression, ExprContext.Value);
				if (arrayBuilder == null && boundExpression2 != boundExpression)
				{
					arrayBuilder = ArrayBuilder<BoundExpression>.GetInstance();
					arrayBuilder.AddRange(initializers, i);
				}
				arrayBuilder?.Add(boundExpression2);
			}
		}
		return node.Update(arrayBuilder?.ToImmutableAndFree() ?? initializers);
	}

	public override BoundNode VisitAddressOfOperator(BoundAddressOfOperator node)
	{
		BoundExpression operand = VisitExpression(node.Operand, ExprContext.Address);
		return node.Update(operand, node.IsManaged, node.Type);
	}

	public override BoundNode VisitReturnStatement(BoundReturnStatement node)
	{
		BoundExpression expressionOpt = (BoundExpression)Visit(node.ExpressionOpt);
		EnsureOnlyEvalStack();
		return node.Update(node.RefKind, expressionOpt);
	}

	private void EnsureOnlyEvalStack()
	{
		RecordVarRead(empty);
	}

	private object GetStackStateCookie()
	{
		DummyLocal dummyLocal = new DummyLocal();
		_dummyVariables.Add(dummyLocal, dummyLocal);
		_locals.Add(dummyLocal, LocalDefUseInfo.GetInstance(StackDepth()));
		RecordDummyWrite(dummyLocal);
		return dummyLocal;
	}

	private void EnsureStackState(object cookie)
	{
		RecordVarRead(_dummyVariables[cookie]);
	}

	private void RecordBranch(LabelSymbol label)
	{
		if (_dummyVariables.TryGetValue(label, out var value))
		{
			RecordVarRead(value);
			return;
		}
		value = new DummyLocal();
		_dummyVariables.Add(label, value);
		_locals.Add(value, LocalDefUseInfo.GetInstance(StackDepth()));
		RecordDummyWrite(value);
	}

	private void RecordLabel(LabelSymbol label)
	{
		if (_dummyVariables.TryGetValue(label, out var value))
		{
			RecordVarRead(value);
			return;
		}
		value = empty;
		_dummyVariables.Add(label, value);
		RecordVarRead(value);
	}

	private void ShouldNotSchedule(LocalSymbol localSymbol)
	{
		if (_locals.TryGetValue(localSymbol, out var value))
		{
			value.ShouldNotSchedule();
		}
	}

	private void RecordVarRef(LocalSymbol local)
	{
		if (CanScheduleToStack(local))
		{
			ShouldNotSchedule(local);
		}
	}

	private void RecordVarRead(LocalSymbol local)
	{
		if (!CanScheduleToStack(local))
		{
			return;
		}
		LocalDefUseInfo localDefUseInfo = _locals[local];
		if (!localDefUseInfo.CannotSchedule)
		{
			ArrayBuilder<LocalDefUseSpan> localDefs = localDefUseInfo.LocalDefs;
			if (localDefs.Count == 0)
			{
				localDefUseInfo.ShouldNotSchedule();
				return;
			}
			if (local.SynthesizedKind != SynthesizedLocalKind.OptimizerTemp && localDefUseInfo.StackAtDeclaration != StackDepth() && !EvalStackHasLocal(local))
			{
				localDefUseInfo.ShouldNotSchedule();
				return;
			}
			int index = localDefs.Count - 1;
			localDefs[index] = localDefs[index].WithEnd(_counter);
			LocalDefUseSpan item = new LocalDefUseSpan(_counter);
			localDefs.Add(item);
		}
	}

	private bool EvalStackHasLocal(LocalSymbol local)
	{
		(BoundExpression, ExprContext) tuple = _evalStack.Last();
		if (tuple.Item2 == (ExprContext)((local.RefKind == RefKind.None) ? 2 : 3) && tuple.Item1.Kind == BoundKind.Local)
		{
			return ((BoundLocal)tuple.Item1).LocalSymbol == local;
		}
		return false;
	}

	private void RecordDummyWrite(LocalSymbol local)
	{
		LocalDefUseInfo localDefUseInfo = _locals[local];
		LocalDefUseSpan item = new LocalDefUseSpan(_counter);
		localDefUseInfo.LocalDefs.Add(item);
	}

	private void RecordVarWrite(LocalSymbol local)
	{
		if (!CanScheduleToStack(local))
		{
			return;
		}
		LocalDefUseInfo localDefUseInfo = _locals[local];
		if (!localDefUseInfo.CannotSchedule)
		{
			int num = StackDepth() - 1;
			if (localDefUseInfo.StackAtDeclaration != num)
			{
				localDefUseInfo.ShouldNotSchedule();
				return;
			}
			LocalDefUseSpan item = new LocalDefUseSpan(_counter);
			localDefUseInfo.LocalDefs.Add(item);
		}
	}

	private bool CanScheduleToStack(LocalSymbol local)
	{
		if (local.CanScheduleToStack)
		{
			if (_debugFriendly)
			{
				return !local.SynthesizedKind.IsLongLived();
			}
			return true;
		}
		return false;
	}

	private void DeclareLocals(ImmutableArray<LocalSymbol> locals, int stack)
	{
		foreach (LocalSymbol item in locals)
		{
			DeclareLocal(item, stack);
		}
	}

	private void DeclareLocal(LocalSymbol local, int stack)
	{
		if ((object)local != null && CanScheduleToStack(local))
		{
			if (!_locals.TryGetValue(local, out var value))
			{
				_locals.Add(local, LocalDefUseInfo.GetInstance(stack));
			}
			else if (value.StackAtDeclaration != stack)
			{
				value.ShouldNotSchedule();
			}
		}
	}
}
