using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal class UnassignedAddressTakenVariablesWalker : DataFlowPass
{
	private readonly HashSet<PrefixUnaryExpressionSyntax> _result = new HashSet<PrefixUnaryExpressionSyntax>();

	private UnassignedAddressTakenVariablesWalker(CSharpCompilation compilation, Symbol member, BoundNode node)
		: base(compilation, member, node)
	{
	}

	internal static HashSet<PrefixUnaryExpressionSyntax> Analyze(CSharpCompilation compilation, Symbol member, BoundNode node)
	{
		UnassignedAddressTakenVariablesWalker unassignedAddressTakenVariablesWalker = new UnassignedAddressTakenVariablesWalker(compilation, member, node);
		try
		{
			bool badRegion = false;
			return unassignedAddressTakenVariablesWalker.Analyze(ref badRegion);
		}
		finally
		{
			unassignedAddressTakenVariablesWalker.Free();
		}
	}

	private new HashSet<PrefixUnaryExpressionSyntax> Analyze(ref bool badRegion)
	{
		Analyze(ref badRegion, null);
		return _result;
	}

	protected override void ReportUnassigned(Symbol symbol, SyntaxNode node, int slot, bool skipIfUseBeforeDeclaration)
	{
		if (node.Parent.Kind() == SyntaxKind.AddressOfExpression)
		{
			_result.Add((PrefixUnaryExpressionSyntax)node.Parent);
		}
	}

	public override BoundNode VisitAddressOfOperator(BoundAddressOfOperator node)
	{
		VisitRvalue(node.Operand);
		return null;
	}
}
