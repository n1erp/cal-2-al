using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class VariablesDeclaredWalker : AbstractRegionControlFlowPass
{
	private HashSet<Symbol> _variablesDeclared = new HashSet<Symbol>();

	internal static IEnumerable<Symbol> Analyze(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion)
	{
		VariablesDeclaredWalker variablesDeclaredWalker = new VariablesDeclaredWalker(compilation, member, node, firstInRegion, lastInRegion);
		try
		{
			bool badRegion = false;
			variablesDeclaredWalker.Analyze(ref badRegion);
			IEnumerable<Symbol> result;
			if (!badRegion)
			{
				IEnumerable<Symbol> variablesDeclared = variablesDeclaredWalker._variablesDeclared;
				result = variablesDeclared;
			}
			else
			{
				result = SpecializedCollections.EmptyEnumerable<Symbol>();
			}
			return result;
		}
		finally
		{
			variablesDeclaredWalker.Free();
		}
	}

	internal VariablesDeclaredWalker(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion)
		: base(compilation, member, node, firstInRegion, lastInRegion)
	{
	}

	protected override void Free()
	{
		base.Free();
		_variablesDeclared = null;
	}

	public override void VisitPattern(BoundExpression expression, BoundPattern pattern)
	{
		base.VisitPattern(expression, pattern);
		NoteDeclaredPatternVariables(pattern);
	}

	protected override void VisitPatternSwitchSection(BoundPatternSwitchSection node, BoundExpression switchExpression, bool isLastSection)
	{
		foreach (BoundPatternSwitchLabel switchLabel in node.SwitchLabels)
		{
			NoteDeclaredPatternVariables(switchLabel.Pattern);
		}
		base.VisitPatternSwitchSection(node, switchExpression, isLastSection);
	}

	private void NoteDeclaredPatternVariables(BoundPattern pattern)
	{
		if (IsInside && pattern.Kind == BoundKind.DeclarationPattern)
		{
			BoundDeclarationPattern boundDeclarationPattern = (BoundDeclarationPattern)pattern;
			Symbol variable = boundDeclarationPattern.Variable;
			if ((object)variable != null && variable.Kind == SymbolKind.Local)
			{
				_variablesDeclared.Add(boundDeclarationPattern.Variable);
			}
		}
	}

	public override BoundNode VisitLocalDeclaration(BoundLocalDeclaration node)
	{
		if (IsInside)
		{
			_variablesDeclared.Add(node.LocalSymbol);
		}
		return base.VisitLocalDeclaration(node);
	}

	public override BoundNode VisitLambda(BoundLambda node)
	{
		if (IsInside && !node.WasCompilerGenerated)
		{
			foreach (ParameterSymbol parameter in node.Symbol.Parameters)
			{
				_variablesDeclared.Add(parameter);
			}
		}
		return base.VisitLambda(node);
	}

	public override BoundNode VisitLocalFunctionStatement(BoundLocalFunctionStatement node)
	{
		if (IsInside && !node.WasCompilerGenerated)
		{
			foreach (ParameterSymbol parameter in node.Symbol.Parameters)
			{
				_variablesDeclared.Add(parameter);
			}
		}
		return base.VisitLocalFunctionStatement(node);
	}

	public override void VisitForEachIterationVariables(BoundForEachStatement node)
	{
		if (!IsInside)
		{
			return;
		}
		BoundDeconstructionAssignmentOperator boundDeconstructionAssignmentOperator = node.DeconstructionOpt?.DeconstructionAssignment;
		if (boundDeconstructionAssignmentOperator == null)
		{
			_variablesDeclared.AddAll(node.IterationVariables);
			return;
		}
		boundDeconstructionAssignmentOperator.Left.VisitAllElements((BoundExpression x, VariablesDeclaredWalker self) =>
		{
			self.Visit(x);
		}, this);
	}

	protected override void VisitCatchBlock(BoundCatchBlock catchBlock, ref LocalState finallyState)
	{
		if (IsInside)
		{
			LocalSymbol localSymbol = catchBlock.Locals.FirstOrDefault();
			if ((object)localSymbol != null && localSymbol.DeclarationKind == LocalDeclarationKind.CatchVariable)
			{
				_variablesDeclared.Add(localSymbol);
			}
		}
		base.VisitCatchBlock(catchBlock, ref finallyState);
	}

	public override BoundNode VisitQueryClause(BoundQueryClause node)
	{
		if (IsInside && (object)node.DefinedSymbol != null)
		{
			_variablesDeclared.Add(node.DefinedSymbol);
		}
		return base.VisitQueryClause(node);
	}

	protected override void VisitLvalue(BoundLocal node)
	{
		VisitLocal(node);
	}

	public override BoundNode VisitLocal(BoundLocal node)
	{
		if (IsInside && node.IsDeclaration)
		{
			_variablesDeclared.Add(node.LocalSymbol);
		}
		return null;
	}
}
