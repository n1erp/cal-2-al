using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.CodeGen;

internal sealed class StackOptimizerPass2 : BoundTreeRewriterWithStackGuard
{
	private int _nodeCounter;

	private readonly Dictionary<LocalSymbol, LocalDefUseInfo> _info;

	private StackOptimizerPass2(Dictionary<LocalSymbol, LocalDefUseInfo> info)
	{
		_info = info;
	}

	public static BoundStatement Rewrite(BoundStatement src, Dictionary<LocalSymbol, LocalDefUseInfo> info)
	{
		return (BoundStatement)new StackOptimizerPass2(info).Visit(src);
	}

	public override BoundNode Visit(BoundNode node)
	{
		BoundNode result = ((!(node is BoundExpression boundExpression) || !(boundExpression.ConstantValue != null)) ? base.Visit(node) : node);
		_nodeCounter++;
		return result;
	}

	public override BoundNode VisitBinaryOperator(BoundBinaryOperator node)
	{
		BoundExpression left = node.Left;
		if (left.Kind != BoundKind.BinaryOperator || left.ConstantValue != null)
		{
			return base.VisitBinaryOperator(node);
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
		BoundExpression boundExpression = (BoundExpression)Visit(left);
		while (true)
		{
			boundBinaryOperator = instance.Pop();
			BoundExpression right = (BoundExpression)Visit(boundBinaryOperator.Right);
			TypeSymbol type = VisitType(boundBinaryOperator.Type);
			boundExpression = boundBinaryOperator.Update(boundBinaryOperator.OperatorKind, boundExpression, right, boundBinaryOperator.ConstantValueOpt, boundBinaryOperator.MethodOpt, boundBinaryOperator.ResultKind, type);
			if (instance.Count == 0)
			{
				break;
			}
			_nodeCounter++;
		}
		instance.Free();
		return boundExpression;
	}

	private static bool IsLastAccess(LocalDefUseInfo locInfo, int counter)
	{
		return locInfo.LocalDefs.Any((LocalDefUseSpan d) => counter == d.Start && counter == d.End);
	}

	public override BoundNode VisitLocal(BoundLocal node)
	{
		if (!_info.TryGetValue(node.LocalSymbol, out var value))
		{
			return base.VisitLocal(node);
		}
		if (!IsLastAccess(value, _nodeCounter))
		{
			return new BoundDup(node.Syntax, node.LocalSymbol.RefKind, node.Type);
		}
		return base.VisitLocal(node);
	}

	public override BoundNode VisitObjectCreationExpression(BoundObjectCreationExpression node)
	{
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Constructor, arguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, node.Expanded, node.ArgsToParamsOpt, node.ConstantValueOpt, null, node.BinderOpt, type);
	}

	public override BoundNode VisitAssignmentOperator(BoundAssignmentOperator node)
	{
		if (!(node.Left is BoundLocal boundLocal) || !_info.TryGetValue(boundLocal.LocalSymbol, out var value))
		{
			return base.VisitAssignmentOperator(node);
		}
		if (boundLocal.LocalSymbol.RefKind != RefKind.None && !node.IsRef)
		{
			return base.VisitAssignmentOperator(node);
		}
		_nodeCounter++;
		BoundExpression boundExpression = (BoundExpression)Visit(node.Right);
		if (IsLastAccess(value, _nodeCounter))
		{
			return boundExpression;
		}
		return node.Update(boundLocal, boundExpression, node.IsRef, node.Type);
	}

	public override BoundNode VisitCatchBlock(BoundCatchBlock node)
	{
		BoundExpression boundExpression = node.ExceptionSourceOpt;
		TypeSymbol exceptionTypeOpt = node.ExceptionTypeOpt;
		BoundExpression boundExpression2 = node.ExceptionFilterOpt;
		BoundBlock body = node.Body;
		if (boundExpression != null)
		{
			_nodeCounter++;
			if (boundExpression.Kind == BoundKind.Local)
			{
				LocalSymbol localSymbol = ((BoundLocal)boundExpression).LocalSymbol;
				if (_info.TryGetValue(localSymbol, out var value) && IsLastAccess(value, _nodeCounter))
				{
					boundExpression = null;
				}
			}
			else
			{
				boundExpression = (BoundExpression)Visit(boundExpression);
			}
			_nodeCounter++;
		}
		if (boundExpression2 != null)
		{
			boundExpression2 = (BoundExpression)Visit(boundExpression2);
			_nodeCounter++;
		}
		body = (BoundBlock)Visit(body);
		exceptionTypeOpt = VisitType(exceptionTypeOpt);
		return node.Update(node.Locals, boundExpression, exceptionTypeOpt, boundExpression2, body, node.IsSynthesizedAsyncCatchAll);
	}
}
