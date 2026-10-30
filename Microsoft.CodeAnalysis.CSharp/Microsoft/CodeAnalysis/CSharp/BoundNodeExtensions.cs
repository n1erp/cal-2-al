using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class BoundNodeExtensions
{
	public static bool HasErrors<T>(this ImmutableArray<T> nodeArray) where T : BoundNode
	{
		if (nodeArray.IsDefault)
		{
			return false;
		}
		int i = 0;
		for (int length = nodeArray.Length; i < length; i++)
		{
			if (nodeArray[i].HasErrors)
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasErrors(this BoundNode node)
	{
		return node?.HasErrors ?? false;
	}

	public static bool IsConstructorInitializer(this BoundStatement statement)
	{
		if (statement.Kind == BoundKind.ExpressionStatement)
		{
			BoundExpression boundExpression = ((BoundExpressionStatement)statement).Expression;
			if (boundExpression.Kind == BoundKind.Sequence && ((BoundSequence)boundExpression).SideEffects.IsDefaultOrEmpty)
			{
				boundExpression = ((BoundSequence)boundExpression).Value;
			}
			if (boundExpression.Kind == BoundKind.Call)
			{
				return ((BoundCall)boundExpression).IsConstructorInitializer();
			}
			return false;
		}
		return false;
	}

	public static bool IsConstructorInitializer(this BoundCall call)
	{
		MethodSymbol method = call.Method;
		BoundExpression receiverOpt = call.ReceiverOpt;
		if (method.MethodKind == MethodKind.Constructor && receiverOpt != null)
		{
			if (receiverOpt.Kind != BoundKind.ThisReference)
			{
				return receiverOpt.Kind == BoundKind.BaseReference;
			}
			return true;
		}
		return false;
	}

	public static T MakeCompilerGenerated<T>(this T node) where T : BoundNode
	{
		node.WasCompilerGenerated = true;
		return node;
	}
}
