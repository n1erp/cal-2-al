using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class AwaitExpressionSpiller : BoundTreeRewriterWithStackGuard
{
	private sealed class BoundSpillSequenceBuilder : BoundExpression
	{
		public readonly BoundExpression Value;

		private ArrayBuilder<LocalSymbol> _locals;

		private ArrayBuilder<BoundStatement> _statements;

		public bool HasStatements => _statements != null;

		public bool HasLocals => _locals != null;

		public BoundSpillSequenceBuilder(BoundExpression value = null)
			: base(BoundKind.SequencePoint, null, value?.Type)
		{
			Value = value;
		}

		public ImmutableArray<LocalSymbol> GetLocals()
		{
			if (_locals != null)
			{
				return _locals.ToImmutable();
			}
			return ImmutableArray<LocalSymbol>.Empty;
		}

		public ImmutableArray<BoundStatement> GetStatements(LocalSubstituter substituterOpt = null)
		{
			if (_statements == null)
			{
				return ImmutableArray<BoundStatement>.Empty;
			}
			if (substituterOpt == null)
			{
				return _statements.ToImmutable();
			}
			return _statements.SelectAsArray((BoundStatement statement, LocalSubstituter substituter) => (BoundStatement)substituter.Visit(statement), substituterOpt);
		}

		internal BoundSpillSequenceBuilder Update(BoundExpression value)
		{
			return new BoundSpillSequenceBuilder(value)
			{
				_locals = _locals,
				_statements = _statements
			};
		}

		public void Free()
		{
			if (_locals != null)
			{
				_locals.Free();
			}
			if (_statements != null)
			{
				_statements.Free();
			}
		}

		internal void Include(BoundSpillSequenceBuilder other)
		{
			if (other != null)
			{
				IncludeAndFree(ref _locals, ref other._locals);
				IncludeAndFree(ref _statements, ref other._statements);
			}
		}

		private static void IncludeAndFree<T>(ref ArrayBuilder<T> left, ref ArrayBuilder<T> right)
		{
			if (right != null)
			{
				if (left == null)
				{
					left = right;
					return;
				}
				left.AddRange(right);
				right.Free();
			}
		}

		public void AddLocal(LocalSymbol local, DiagnosticBag diagnostics)
		{
			if (_locals == null)
			{
				_locals = ArrayBuilder<LocalSymbol>.GetInstance();
			}
			if (local.Type.IsRestrictedType())
			{
				diagnostics.Add(ErrorCode.ERR_ByRefTypeAndAwait, local.Locations[0], local.Type);
			}
			_locals.Add(local);
		}

		public void AddStatement(BoundStatement statement)
		{
			if (_statements == null)
			{
				_statements = ArrayBuilder<BoundStatement>.GetInstance();
			}
			_statements.Add(statement);
		}

		internal void AddExpressions(ImmutableArray<BoundExpression> expressions)
		{
			foreach (BoundExpression item in expressions)
			{
				AddStatement(new BoundExpressionStatement(item.Syntax, item)
				{
					WasCompilerGenerated = true
				});
			}
		}
	}

	private sealed class LocalSubstituter : BoundTreeRewriterWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator
	{
		private readonly PooledDictionary<LocalSymbol, LocalSymbol> _tempSubstitution;

		public LocalSubstituter(PooledDictionary<LocalSymbol, LocalSymbol> tempSubstitution, int recursionDepth)
			: base(recursionDepth)
		{
			_tempSubstitution = tempSubstitution;
		}

		public override BoundNode VisitLocal(BoundLocal node)
		{
			if (!node.LocalSymbol.SynthesizedKind.IsLongLived() && _tempSubstitution.TryGetValue(node.LocalSymbol, out var value))
			{
				return node.Update(value, node.ConstantValueOpt, node.Type);
			}
			return base.VisitLocal(node);
		}
	}

	private sealed class ConditionalReceiverReplacer : BoundTreeRewriterWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator
	{
		private readonly BoundExpression _receiver;

		private readonly int _receiverId;

		private ConditionalReceiverReplacer(BoundExpression receiver, int receiverId, int recursionDepth)
			: base(recursionDepth)
		{
			_receiver = receiver;
			_receiverId = receiverId;
		}

		public static BoundStatement Replace(BoundNode node, BoundExpression receiver, int receiverID, int recursionDepth)
		{
			return (BoundStatement)new ConditionalReceiverReplacer(receiver, receiverID, recursionDepth).Visit(node);
		}

		public override BoundNode VisitConditionalReceiver(BoundConditionalReceiver node)
		{
			if (node.Id == _receiverId)
			{
				return _receiver;
			}
			return node;
		}
	}

	private const BoundKind SpillSequenceBuilder = BoundKind.SequencePoint;

	private readonly SyntheticBoundNodeFactory _F;

	private readonly PooledDictionary<LocalSymbol, LocalSymbol> _tempSubstitution;

	private AwaitExpressionSpiller(MethodSymbol method, SyntaxNode syntaxNode, TypeCompilationState compilationState, PooledDictionary<LocalSymbol, LocalSymbol> tempSubstitution, DiagnosticBag diagnostics)
	{
		_F = new SyntheticBoundNodeFactory(method, syntaxNode, compilationState, diagnostics);
		_tempSubstitution = tempSubstitution;
	}

	internal static BoundStatement Rewrite(BoundStatement body, MethodSymbol method, TypeCompilationState compilationState, DiagnosticBag diagnostics)
	{
		PooledDictionary<LocalSymbol, LocalSymbol> instance = PooledDictionary<LocalSymbol, LocalSymbol>.GetInstance();
		BoundStatement result = (BoundStatement)new AwaitExpressionSpiller(method, body.Syntax, compilationState, instance, diagnostics).Visit(body);
		instance.Free();
		return result;
	}

	private BoundExpression VisitExpression(ref BoundSpillSequenceBuilder builder, BoundExpression expression)
	{
		if (expression != null && expression.Kind == BoundKind.AwaitExpression)
		{
			BoundAwaitExpression boundAwaitExpression = (BoundAwaitExpression)expression;
			boundAwaitExpression = boundAwaitExpression.Update(VisitExpression(ref builder, boundAwaitExpression.Expression), boundAwaitExpression.GetAwaiter, boundAwaitExpression.IsCompleted, boundAwaitExpression.GetResult, boundAwaitExpression.Type);
			SyntaxNode syntax = boundAwaitExpression.Syntax;
			_F.Syntax = syntax;
			BoundLocal boundLocal = _F.StoreToTemp(boundAwaitExpression, out var store, RefKind.None, SynthesizedLocalKind.AwaitSpill, syntax);
			if (builder == null)
			{
				builder = new BoundSpillSequenceBuilder();
			}
			builder.AddLocal(boundLocal.LocalSymbol, _F.Diagnostics);
			builder.AddStatement(_F.ExpressionStatement(store));
			return boundLocal;
		}
		BoundExpression boundExpression = (BoundExpression)Visit(expression);
		if (boundExpression == null || boundExpression.Kind != BoundKind.SequencePoint)
		{
			return boundExpression;
		}
		BoundSpillSequenceBuilder boundSpillSequenceBuilder = (BoundSpillSequenceBuilder)boundExpression;
		if (builder == null)
		{
			builder = boundSpillSequenceBuilder.Update(null);
		}
		else
		{
			builder.Include(boundSpillSequenceBuilder);
		}
		return boundSpillSequenceBuilder.Value;
	}

	private static BoundExpression UpdateExpression(BoundSpillSequenceBuilder builder, BoundExpression expression)
	{
		if (builder == null)
		{
			return expression;
		}
		if (!builder.HasLocals && !builder.HasStatements)
		{
			builder.Free();
			return expression;
		}
		return builder.Update(expression);
	}

	private BoundStatement UpdateStatement(BoundSpillSequenceBuilder builder, BoundStatement statement, bool substituteTemps)
	{
		if (builder == null)
		{
			return statement;
		}
		if (statement != null)
		{
			builder.AddStatement(statement);
		}
		LocalSubstituter substituterOpt = ((substituteTemps && _tempSubstitution.Count > 0) ? new LocalSubstituter(_tempSubstitution, RecursionDepth) : null);
		BoundBlock result = _F.Block(builder.GetLocals(), builder.GetStatements(substituterOpt));
		builder.Free();
		return result;
	}

	private BoundExpression Spill(BoundSpillSequenceBuilder builder, BoundExpression expression, RefKind refKind = RefKind.None, bool sideEffectsOnly = false)
	{
		while (true)
		{
			switch (expression.Kind)
			{
			case BoundKind.ArrayInitialization:
			{
				BoundArrayInitialization boundArrayInitialization = (BoundArrayInitialization)expression;
				ImmutableArray<BoundExpression> initializers = VisitExpressionList(ref builder, boundArrayInitialization.Initializers, default, forceSpill: true);
				return boundArrayInitialization.Update(initializers);
			}
			case BoundKind.ArgListOperator:
			{
				BoundArgListOperator boundArgListOperator = (BoundArgListOperator)expression;
				ImmutableArray<BoundExpression> arguments = VisitExpressionList(ref builder, boundArgListOperator.Arguments, boundArgListOperator.ArgumentRefKindsOpt, forceSpill: true);
				return boundArgListOperator.Update(arguments, boundArgListOperator.ArgumentRefKindsOpt, boundArgListOperator.Type);
			}
			case BoundKind.SequencePoint:
				goto IL_00fd;
			case BoundKind.Sequence:
				if (refKind != RefKind.None)
				{
					return expression;
				}
				break;
			case BoundKind.ThisReference:
			case BoundKind.BaseReference:
				if (refKind != RefKind.None || expression.Type.IsReferenceType)
				{
					return expression;
				}
				break;
			case BoundKind.Parameter:
				if (refKind != RefKind.None)
				{
					return expression;
				}
				break;
			case BoundKind.Local:
			{
				BoundLocal boundLocal = (BoundLocal)expression;
				if (boundLocal.LocalSymbol.SynthesizedKind == SynthesizedLocalKind.AwaitSpill || refKind != RefKind.None)
				{
					return boundLocal;
				}
				break;
			}
			case BoundKind.FieldAccess:
			{
				BoundFieldAccess boundFieldAccess = (BoundFieldAccess)expression;
				FieldSymbol fieldSymbol = boundFieldAccess.FieldSymbol;
				if (fieldSymbol.IsStatic)
				{
					if (refKind != RefKind.None || fieldSymbol.IsReadOnly)
					{
						return boundFieldAccess;
					}
				}
				else if (refKind != RefKind.None)
				{
					BoundExpression receiver = Spill(builder, boundFieldAccess.ReceiverOpt, fieldSymbol.ContainingType.IsValueType ? refKind : RefKind.None);
					return boundFieldAccess.Update(receiver, fieldSymbol, boundFieldAccess.ConstantValueOpt, boundFieldAccess.ResultKind, boundFieldAccess.Type);
				}
				break;
			}
			case BoundKind.Call:
			{
				BoundCall boundCall = (BoundCall)expression;
				if (refKind != RefKind.None && refKind != RefKind.In)
				{
					_F.Diagnostics.Add(ErrorCode.ERR_RefReturningCallAndAwait, _F.Syntax.Location, boundCall.Method);
				}
				refKind = RefKind.None;
				break;
			}
			case BoundKind.ConditionalOperator:
				_ = (BoundConditionalOperator)expression;
				if (refKind != RefKind.None && refKind != RefKind.In)
				{
					_F.Diagnostics.Add(ErrorCode.ERR_RefConditionalAndAwait, _F.Syntax.Location);
				}
				refKind = RefKind.None;
				break;
			case BoundKind.TypeExpression:
			case BoundKind.Literal:
				return expression;
			case BoundKind.ConditionalReceiver:
				return expression;
			}
			break;
			IL_00fd:
			BoundSpillSequenceBuilder boundSpillSequenceBuilder = (BoundSpillSequenceBuilder)expression;
			builder.Include(boundSpillSequenceBuilder);
			expression = boundSpillSequenceBuilder.Value;
		}
		if ((expression.Type.SpecialType == SpecialType.System_Void) | sideEffectsOnly)
		{
			builder.AddStatement(_F.ExpressionStatement(expression));
			return null;
		}
		BoundLocal boundLocal2 = _F.StoreToTemp(expression, out var store, refKind, SynthesizedLocalKind.AwaitSpill, _F.Syntax);
		builder.AddLocal(boundLocal2.LocalSymbol, _F.Diagnostics);
		builder.AddStatement(_F.ExpressionStatement(store));
		return boundLocal2;
	}

	private ImmutableArray<BoundExpression> VisitExpressionList(ref BoundSpillSequenceBuilder builder, ImmutableArray<BoundExpression> args, ImmutableArray<RefKind> refKinds = default(ImmutableArray<RefKind>), bool forceSpill = false, bool sideEffectsOnly = false)
	{
		if (args.Length == 0)
		{
			return args;
		}
		ImmutableArray<BoundExpression> result = VisitList(args);
		int num;
		if (forceSpill)
		{
			num = result.Length;
		}
		else
		{
			num = -1;
			for (int num2 = result.Length - 1; num2 >= 0; num2--)
			{
				if (result[num2].Kind == BoundKind.SequencePoint)
				{
					num = num2;
					break;
				}
			}
		}
		if (num == -1)
		{
			return result;
		}
		if (builder == null)
		{
			builder = new BoundSpillSequenceBuilder();
		}
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(result.Length);
		for (int i = 0; i < num; i++)
		{
			RefKind refKind = ((!refKinds.IsDefault) ? refKinds[i] : RefKind.None);
			BoundExpression item = Spill(builder, result[i], refKind, sideEffectsOnly);
			if (!sideEffectsOnly)
			{
				instance.Add(item);
			}
		}
		if (num < result.Length)
		{
			BoundSpillSequenceBuilder boundSpillSequenceBuilder = (BoundSpillSequenceBuilder)result[num];
			builder.Include(boundSpillSequenceBuilder);
			instance.Add(boundSpillSequenceBuilder.Value);
			for (int j = num + 1; j < result.Length; j++)
			{
				instance.Add(result[j]);
			}
		}
		return instance.ToImmutableAndFree();
	}

	private void EnterStatement(BoundNode boundStatement)
	{
		_tempSubstitution.Clear();
	}

	public override BoundNode VisitSwitchStatement(BoundSwitchStatement node)
	{
		EnterStatement(node);
		BoundSpillSequenceBuilder builder = null;
		BoundStatement loweredPreambleOpt = (BoundStatement)Visit(node.LoweredPreambleOpt);
		BoundExpression expression = VisitExpression(ref builder, node.Expression);
		ImmutableArray<BoundSwitchSection> switchSections = VisitList(node.SwitchSections);
		return UpdateStatement(builder, node.Update(loweredPreambleOpt, expression, node.ConstantTargetOpt, node.InnerLocals, node.InnerLocalFunctions, switchSections, node.BreakLabel, node.StringEquality), substituteTemps: true);
	}

	public override BoundNode VisitThrowStatement(BoundThrowStatement node)
	{
		EnterStatement(node);
		BoundSpillSequenceBuilder builder = null;
		BoundExpression expressionOpt = VisitExpression(ref builder, node.ExpressionOpt);
		return UpdateStatement(builder, node.Update(expressionOpt), substituteTemps: true);
	}

	public override BoundNode VisitExpressionStatement(BoundExpressionStatement node)
	{
		EnterStatement(node);
		BoundSpillSequenceBuilder builder = null;
		BoundExpression expression2;
		if (node.Expression.Kind == BoundKind.AwaitExpression)
		{
			BoundAwaitExpression boundAwaitExpression = (BoundAwaitExpression)node.Expression;
			BoundExpression expression = VisitExpression(ref builder, boundAwaitExpression.Expression);
			expression2 = boundAwaitExpression.Update(expression, boundAwaitExpression.GetAwaiter, boundAwaitExpression.IsCompleted, boundAwaitExpression.GetResult, boundAwaitExpression.Type);
		}
		else
		{
			expression2 = VisitExpression(ref builder, node.Expression);
		}
		return UpdateStatement(builder, node.Update(expression2), substituteTemps: true);
	}

	public override BoundNode VisitConditionalGoto(BoundConditionalGoto node)
	{
		EnterStatement(node);
		BoundSpillSequenceBuilder builder = null;
		BoundExpression condition = VisitExpression(ref builder, node.Condition);
		return UpdateStatement(builder, node.Update(condition, node.JumpIfTrue, node.Label), substituteTemps: true);
	}

	public override BoundNode VisitReturnStatement(BoundReturnStatement node)
	{
		EnterStatement(node);
		BoundSpillSequenceBuilder builder = null;
		BoundExpression expressionOpt = VisitExpression(ref builder, node.ExpressionOpt);
		return UpdateStatement(builder, node.Update(node.RefKind, expressionOpt), substituteTemps: true);
	}

	public override BoundNode VisitAwaitExpression(BoundAwaitExpression node)
	{
		BoundSpillSequenceBuilder builder = new BoundSpillSequenceBuilder();
		BoundExpression value = VisitExpression(ref builder, node);
		return builder.Update(value);
	}

	public override BoundNode VisitAddressOfOperator(BoundAddressOfOperator node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression operand = VisitExpression(ref builder, node.Operand);
		return UpdateExpression(builder, node.Update(operand, node.IsManaged, node.Type));
	}

	public override BoundNode VisitArgListOperator(BoundArgListOperator node)
	{
		BoundSpillSequenceBuilder builder = null;
		ImmutableArray<BoundExpression> arguments = VisitExpressionList(ref builder, node.Arguments);
		return UpdateExpression(builder, node.Update(arguments, node.ArgumentRefKindsOpt, node.Type));
	}

	public override BoundNode VisitArrayAccess(BoundArrayAccess node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression expression = VisitExpression(ref builder, node.Expression);
		BoundSpillSequenceBuilder builder2 = null;
		ImmutableArray<BoundExpression> indices = VisitExpressionList(ref builder2, node.Indices);
		if (builder2 != null)
		{
			if (builder == null)
			{
				builder = new BoundSpillSequenceBuilder();
			}
			expression = Spill(builder, expression);
		}
		if (builder != null)
		{
			builder.Include(builder2);
			builder2 = builder;
		}
		return UpdateExpression(builder2, node.Update(expression, indices, node.Type));
	}

	public override BoundNode VisitArrayCreation(BoundArrayCreation node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundArrayInitialization initializerOpt = (BoundArrayInitialization)VisitExpression(ref builder, node.InitializerOpt);
		ImmutableArray<BoundExpression> bounds;
		if (builder == null)
		{
			bounds = VisitExpressionList(ref builder, node.Bounds);
		}
		else
		{
			BoundSpillSequenceBuilder builder2 = new BoundSpillSequenceBuilder();
			bounds = VisitExpressionList(ref builder2, node.Bounds, default, forceSpill: true);
			builder2.Include(builder);
			builder = builder2;
		}
		return UpdateExpression(builder, node.Update(bounds, initializerOpt, node.Type));
	}

	public override BoundNode VisitArrayInitialization(BoundArrayInitialization node)
	{
		BoundSpillSequenceBuilder builder = null;
		ImmutableArray<BoundExpression> initializers = VisitExpressionList(ref builder, node.Initializers);
		return UpdateExpression(builder, node.Update(initializers));
	}

	public override BoundNode VisitArrayLength(BoundArrayLength node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression expression = VisitExpression(ref builder, node.Expression);
		return UpdateExpression(builder, node.Update(expression, node.Type));
	}

	public override BoundNode VisitAsOperator(BoundAsOperator node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression operand = VisitExpression(ref builder, node.Operand);
		return UpdateExpression(builder, node.Update(operand, node.TargetType, node.Conversion, node.Type));
	}

	public override BoundNode VisitAssignmentOperator(BoundAssignmentOperator node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression right = VisitExpression(ref builder, node.Right);
		BoundExpression boundExpression = node.Left;
		if (builder == null)
		{
			boundExpression = VisitExpression(ref builder, boundExpression);
		}
		else
		{
			BoundSpillSequenceBuilder builder2 = new BoundSpillSequenceBuilder();
			switch (boundExpression.Kind)
			{
			case BoundKind.FieldAccess:
			{
				BoundFieldAccess boundFieldAccess = (BoundFieldAccess)boundExpression;
				if (!boundFieldAccess.FieldSymbol.IsStatic)
				{
					BoundExpression expression2 = VisitExpression(ref builder2, boundFieldAccess.ReceiverOpt);
					expression2 = Spill(builder, expression2, boundFieldAccess.FieldSymbol.ContainingType.IsValueType ? RefKind.Ref : RefKind.None);
					boundExpression = boundFieldAccess.Update(expression2, boundFieldAccess.FieldSymbol, boundFieldAccess.ConstantValueOpt, boundFieldAccess.ResultKind, boundFieldAccess.Type);
				}
				break;
			}
			case BoundKind.ArrayAccess:
			{
				BoundArrayAccess boundArrayAccess = (BoundArrayAccess)boundExpression;
				BoundExpression expression = VisitExpression(ref builder2, boundArrayAccess.Expression);
				expression = Spill(builder, expression);
				ImmutableArray<BoundExpression> indices = VisitExpressionList(ref builder, boundArrayAccess.Indices, default, forceSpill: true);
				boundExpression = boundArrayAccess.Update(expression, indices, boundArrayAccess.Type);
				break;
			}
			default:
				boundExpression = Spill(builder2, VisitExpression(ref builder2, boundExpression), RefKind.Ref);
				break;
			case BoundKind.Local:
			case BoundKind.Parameter:
				break;
			}
			builder2.Include(builder);
			builder = builder2;
		}
		return UpdateExpression(builder, node.Update(boundExpression, right, node.IsRef, node.Type));
	}

	public override BoundNode VisitBadExpression(BoundBadExpression node)
	{
		return node;
	}

	public override BoundNode VisitUserDefinedConditionalLogicalOperator(BoundUserDefinedConditionalLogicalOperator node)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override BoundNode VisitBinaryOperator(BoundBinaryOperator node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression right = VisitExpression(ref builder, node.Right);
		BoundExpression boundExpression;
		if (builder == null)
		{
			boundExpression = VisitExpression(ref builder, node.Left);
		}
		else
		{
			BoundSpillSequenceBuilder builder2 = new BoundSpillSequenceBuilder();
			boundExpression = VisitExpression(ref builder2, node.Left);
			boundExpression = Spill(builder2, boundExpression);
			if (node.OperatorKind == BinaryOperatorKind.LogicalBoolOr || node.OperatorKind == BinaryOperatorKind.LogicalBoolAnd)
			{
				LocalSymbol local = _F.SynthesizedLocal(node.Type, _F.Syntax, isPinned: false, RefKind.None, SynthesizedLocalKind.AwaitSpill);
				builder2.AddLocal(local, _F.Diagnostics);
				builder2.AddStatement(_F.Assignment(_F.Local(local), boundExpression));
				builder2.AddStatement(_F.If((node.OperatorKind == BinaryOperatorKind.LogicalBoolAnd) ? _F.Local(local) : _F.Not(_F.Local(local)), UpdateStatement(builder, _F.Assignment(_F.Local(local), right), substituteTemps: false)));
				return UpdateExpression(builder2, _F.Local(local));
			}
			builder2.Include(builder);
			builder = builder2;
		}
		return UpdateExpression(builder, node.Update(node.OperatorKind, boundExpression, right, node.ConstantValue, node.MethodOpt, node.ResultKind, node.Type));
	}

	public override BoundNode VisitCall(BoundCall node)
	{
		BoundSpillSequenceBuilder builder = null;
		ImmutableArray<BoundExpression> arguments = VisitExpressionList(ref builder, node.Arguments, node.ArgumentRefKindsOpt);
		BoundExpression receiverOpt = null;
		if (builder == null)
		{
			receiverOpt = VisitExpression(ref builder, node.ReceiverOpt);
		}
		else if (!node.Method.IsStatic)
		{
			BoundSpillSequenceBuilder builder2 = new BoundSpillSequenceBuilder();
			receiverOpt = node.ReceiverOpt;
			RefKind refKind = ReceiverSpillRefKind(receiverOpt);
			receiverOpt = Spill(builder2, VisitExpression(ref builder2, receiverOpt), refKind);
			builder2.Include(builder);
			builder = builder2;
		}
		return UpdateExpression(builder, node.Update(receiverOpt, node.Method, arguments));
	}

	private static RefKind ReceiverSpillRefKind(BoundExpression receiver)
	{
		RefKind result = RefKind.None;
		if (!receiver.Type.IsReferenceType && LocalRewriter.CanBePassedByReference(receiver))
		{
			result = ((!receiver.Type.IsReadOnly) ? RefKind.Ref : RefKind.In);
		}
		return result;
	}

	public override BoundNode VisitConditionalOperator(BoundConditionalOperator node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression condition = VisitExpression(ref builder, node.Condition);
		BoundSpillSequenceBuilder builder2 = null;
		BoundExpression boundExpression = VisitExpression(ref builder2, node.Consequence);
		BoundSpillSequenceBuilder builder3 = null;
		BoundExpression boundExpression2 = VisitExpression(ref builder3, node.Alternative);
		if (builder2 == null && builder3 == null)
		{
			return UpdateExpression(builder, node.Update(node.IsRef, condition, boundExpression, boundExpression2, node.ConstantValueOpt, node.Type));
		}
		if (builder == null)
		{
			builder = new BoundSpillSequenceBuilder();
		}
		if (builder2 == null)
		{
			builder2 = new BoundSpillSequenceBuilder();
		}
		if (builder3 == null)
		{
			builder3 = new BoundSpillSequenceBuilder();
		}
		if (node.Type.SpecialType == SpecialType.System_Void)
		{
			builder.AddStatement(_F.If(condition, UpdateStatement(builder2, _F.ExpressionStatement(boundExpression), substituteTemps: false), UpdateStatement(builder3, _F.ExpressionStatement(boundExpression2), substituteTemps: false)));
			return builder.Update(_F.Default(node.Type));
		}
		LocalSymbol local = _F.SynthesizedLocal(node.Type, _F.Syntax, isPinned: false, RefKind.None, SynthesizedLocalKind.AwaitSpill);
		builder.AddLocal(local, _F.Diagnostics);
		builder.AddStatement(_F.If(condition, UpdateStatement(builder2, _F.Assignment(_F.Local(local), boundExpression), substituteTemps: false), UpdateStatement(builder3, _F.Assignment(_F.Local(local), boundExpression2), substituteTemps: false)));
		return builder.Update(_F.Local(local));
	}

	public override BoundNode VisitConversion(BoundConversion node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression operand = VisitExpression(ref builder, node.Operand);
		return UpdateExpression(builder, node.Update(operand, node.Conversion, node.IsBaseConversion, node.Checked, node.ExplicitCastInCode, node.ConstantValueOpt, node.Type));
	}

	public override BoundNode VisitPassByCopy(BoundPassByCopy node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression expression = VisitExpression(ref builder, node.Expression);
		return UpdateExpression(builder, node.Update(expression, node.Type));
	}

	public override BoundNode VisitMethodGroup(BoundMethodGroup node)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override BoundNode VisitDelegateCreationExpression(BoundDelegateCreationExpression node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression argument = VisitExpression(ref builder, node.Argument);
		return UpdateExpression(builder, node.Update(argument, node.MethodOpt, node.IsExtensionMethod, node.Type));
	}

	public override BoundNode VisitFieldAccess(BoundFieldAccess node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression receiver = VisitExpression(ref builder, node.ReceiverOpt);
		return UpdateExpression(builder, node.Update(receiver, node.FieldSymbol, node.ConstantValueOpt, node.ResultKind, node.Type));
	}

	public override BoundNode VisitIsOperator(BoundIsOperator node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression operand = VisitExpression(ref builder, node.Operand);
		return UpdateExpression(builder, node.Update(operand, node.TargetType, node.Conversion, node.Type));
	}

	public override BoundNode VisitMakeRefOperator(BoundMakeRefOperator node)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override BoundNode VisitNullCoalescingOperator(BoundNullCoalescingOperator node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression boundExpression = VisitExpression(ref builder, node.RightOperand);
		BoundExpression leftOperand;
		if (builder == null)
		{
			leftOperand = VisitExpression(ref builder, node.LeftOperand);
			return UpdateExpression(builder, node.Update(leftOperand, boundExpression, node.LeftConversion, node.Type));
		}
		BoundSpillSequenceBuilder builder2 = new BoundSpillSequenceBuilder();
		leftOperand = VisitExpression(ref builder2, node.LeftOperand);
		leftOperand = Spill(builder2, leftOperand);
		LocalSymbol local = _F.SynthesizedLocal(node.Type, _F.Syntax, isPinned: false, RefKind.None, SynthesizedLocalKind.AwaitSpill);
		builder2.AddLocal(local, _F.Diagnostics);
		builder2.AddStatement(_F.Assignment(_F.Local(local), leftOperand));
		builder2.AddStatement(_F.If(_F.ObjectEqual(_F.Local(local), _F.Null(leftOperand.Type)), UpdateStatement(builder, _F.Assignment(_F.Local(local), boundExpression), substituteTemps: false)));
		return UpdateExpression(builder2, _F.Local(local));
	}

	public override BoundNode VisitLoweredConditionalAccess(BoundLoweredConditionalAccess node)
	{
		RefKind refKind = ReceiverSpillRefKind(node.Receiver);
		BoundSpillSequenceBuilder builder = null;
		BoundExpression boundExpression = VisitExpression(ref builder, node.Receiver);
		BoundSpillSequenceBuilder builder2 = null;
		BoundExpression boundExpression2 = VisitExpression(ref builder2, node.WhenNotNull);
		BoundSpillSequenceBuilder builder3 = null;
		BoundExpression boundExpression3 = VisitExpression(ref builder3, node.WhenNullOpt);
		if (builder2 == null && builder3 == null)
		{
			return UpdateExpression(builder, node.Update(boundExpression, node.HasValueMethodOpt, boundExpression2, boundExpression3, node.Id, node.Type));
		}
		if (builder == null)
		{
			builder = new BoundSpillSequenceBuilder();
		}
		if (builder2 == null)
		{
			builder2 = new BoundSpillSequenceBuilder();
		}
		if (builder3 == null)
		{
			builder3 = new BoundSpillSequenceBuilder();
		}
		BoundExpression condition;
		if (boundExpression.Type.IsReferenceType || boundExpression.Type.IsValueType || refKind == RefKind.None)
		{
			boundExpression = Spill(builder, boundExpression);
			MethodSymbol hasValueMethodOpt = node.HasValueMethodOpt;
			condition = ((!(hasValueMethodOpt == null)) ? ((BoundExpression)_F.Call(boundExpression, hasValueMethodOpt)) : ((BoundExpression)_F.ObjectNotEqual(_F.Convert(_F.SpecialType(SpecialType.System_Object), boundExpression), _F.Null(_F.SpecialType(SpecialType.System_Object)))));
		}
		else
		{
			boundExpression = Spill(builder, boundExpression, RefKind.Ref);
			LocalSymbol local = _F.SynthesizedLocal(boundExpression.Type, _F.Syntax, isPinned: false, RefKind.None, SynthesizedLocalKind.AwaitSpill);
			builder.AddLocal(local, _F.Diagnostics);
			BoundBinaryOperator left = _F.ObjectNotEqual(_F.Convert(_F.SpecialType(SpecialType.System_Object), _F.Default(boundExpression.Type)), _F.Null(_F.SpecialType(SpecialType.System_Object)));
			condition = _F.LogicalOr(left, _F.MakeSequence(_F.AssignmentExpression(_F.Local(local), boundExpression), _F.ObjectNotEqual(_F.Convert(_F.SpecialType(SpecialType.System_Object), _F.Local(local)), _F.Null(_F.SpecialType(SpecialType.System_Object)))));
			boundExpression = _F.ComplexConditionalReceiver(boundExpression, _F.Local(local));
		}
		if (node.Type.SpecialType == SpecialType.System_Void)
		{
			BoundStatement node2 = UpdateStatement(builder2, _F.ExpressionStatement(boundExpression2), substituteTemps: false);
			node2 = ConditionalReceiverReplacer.Replace(node2, boundExpression, node.Id, RecursionDepth);
			builder.AddStatement(_F.If(condition, node2));
			return builder.Update(_F.Default(node.Type));
		}
		LocalSymbol local2 = _F.SynthesizedLocal(node.Type, _F.Syntax, isPinned: false, RefKind.None, SynthesizedLocalKind.AwaitSpill);
		BoundStatement node3 = UpdateStatement(builder2, _F.Assignment(_F.Local(local2), boundExpression2), substituteTemps: false);
		node3 = ConditionalReceiverReplacer.Replace(node3, boundExpression, node.Id, RecursionDepth);
		boundExpression3 = boundExpression3 ?? _F.Default(node.Type);
		builder.AddLocal(local2, _F.Diagnostics);
		builder.AddStatement(_F.If(condition, node3, UpdateStatement(builder3, _F.Assignment(_F.Local(local2), boundExpression3), substituteTemps: false)));
		return builder.Update(_F.Local(local2));
	}

	public override BoundNode VisitObjectCreationExpression(BoundObjectCreationExpression node)
	{
		BoundSpillSequenceBuilder builder = null;
		ImmutableArray<BoundExpression> arguments = VisitExpressionList(ref builder, node.Arguments, node.ArgumentRefKindsOpt);
		return UpdateExpression(builder, node.Update(node.Constructor, arguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, node.Expanded, node.ArgsToParamsOpt, node.ConstantValueOpt, node.InitializerExpressionOpt, node.BinderOpt, node.Type));
	}

	public override BoundNode VisitPointerElementAccess(BoundPointerElementAccess node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression index = VisitExpression(ref builder, node.Index);
		BoundExpression expression;
		if (builder == null)
		{
			expression = VisitExpression(ref builder, node.Expression);
		}
		else
		{
			BoundSpillSequenceBuilder builder2 = new BoundSpillSequenceBuilder();
			expression = VisitExpression(ref builder2, node.Expression);
			expression = Spill(builder2, expression);
			builder2.Include(builder);
			builder = builder2;
		}
		return UpdateExpression(builder, node.Update(expression, index, node.Checked, node.Type));
	}

	public override BoundNode VisitPointerIndirectionOperator(BoundPointerIndirectionOperator node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression operand = VisitExpression(ref builder, node.Operand);
		return UpdateExpression(builder, node.Update(operand, node.Type));
	}

	public override BoundNode VisitSequence(BoundSequence node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression value = VisitExpression(ref builder, node.Value);
		BoundSpillSequenceBuilder builder2 = null;
		ImmutableArray<BoundExpression> immutableArray = VisitExpressionList(ref builder2, node.SideEffects, default, builder != null, sideEffectsOnly: true);
		if (builder2 == null && builder == null)
		{
			return node.Update(node.Locals, immutableArray, value, node.Type);
		}
		if (builder2 == null)
		{
			builder2 = new BoundSpillSequenceBuilder();
		}
		PromoteAndAddLocals(builder2, node.Locals);
		builder2.AddExpressions(immutableArray);
		builder2.Include(builder);
		return builder2.Update(value);
	}

	public override BoundNode VisitThrowExpression(BoundThrowExpression node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression expression = VisitExpression(ref builder, node.Expression);
		return UpdateExpression(builder, node.Update(expression, node.Type));
	}

	private void PromoteAndAddLocals(BoundSpillSequenceBuilder builder, ImmutableArray<LocalSymbol> locals)
	{
		foreach (LocalSymbol item in locals)
		{
			if (item.SynthesizedKind.IsLongLived())
			{
				builder.AddLocal(item, _F.Diagnostics);
				continue;
			}
			LocalSymbol localSymbol = item.WithSynthesizedLocalKindAndSyntax(SynthesizedLocalKind.AwaitSpill, _F.Syntax);
			_tempSubstitution.Add(item, localSymbol);
			builder.AddLocal(localSymbol, _F.Diagnostics);
		}
	}

	public override BoundNode VisitUnaryOperator(BoundUnaryOperator node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression operand = VisitExpression(ref builder, node.Operand);
		return UpdateExpression(builder, node.Update(node.OperatorKind, operand, node.ConstantValueOpt, node.MethodOpt, node.ResultKind, node.Type));
	}
}
