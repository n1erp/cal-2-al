using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class DecisionTreeBuilder
{
	protected delegate DecisionTree.Guarded DecisionMaker(BoundExpression expression, TypeSymbol type);

	protected readonly Symbol _enclosingSymbol;

	protected readonly Conversions _conversions;

	protected HashSet<DiagnosticInfo> _useSiteDiagnostics = new HashSet<DiagnosticInfo>();

	protected readonly SwitchStatementSyntax _switchSyntax;

	private Dictionary<TypeSymbol, LocalSymbol> localByType = new Dictionary<TypeSymbol, LocalSymbol>();

	protected DecisionTreeBuilder(Symbol enclosingSymbol, SwitchStatementSyntax switchSyntax, Conversions conversions)
	{
		_enclosingSymbol = enclosingSymbol;
		_switchSyntax = switchSyntax;
		_conversions = conversions;
	}

	private BoundLocal GetBoundPatternMatchingLocal(TypeSymbol type)
	{
		if (!localByType.TryGetValue(type, out var value))
		{
			value = new SynthesizedLocal(_enclosingSymbol as MethodSymbol, type, SynthesizedLocalKind.SwitchCasePatternMatching, _switchSyntax);
			localByType.Add(type, value);
		}
		return new BoundLocal(_switchSyntax, value, null, type);
	}

	protected DecisionTree CreateEmptyDecisionTree(BoundExpression expression)
	{
		TypeSymbol type = expression.Type;
		LocalSymbol temp = null;
		if (expression.ConstantValue == null)
		{
			temp = ((BoundLocal)(expression = GetBoundPatternMatchingLocal(type))).LocalSymbol;
		}
		if (type.CanContainNull() || type.SpecialType == SpecialType.None)
		{
			return new DecisionTree.ByType(expression, type, temp);
		}
		return new DecisionTree.ByValue(expression, type, temp);
	}

	protected DecisionTree AddToDecisionTree(DecisionTree decisionTree, SyntaxNode sectionSyntax, BoundPatternSwitchLabel label)
	{
		BoundPattern pattern = label.Pattern;
		BoundExpression guard = label.Guard;
		if (guard?.ConstantValue == ConstantValue.False)
		{
			return null;
		}
		switch (pattern.Kind)
		{
		case BoundKind.ConstantPattern:
		{
			BoundConstantPattern boundConstantPattern = (BoundConstantPattern)pattern;
			DecisionMaker makeDecision2 = (BoundExpression e, TypeSymbol t) => new DecisionTree.Guarded(e, t, default, sectionSyntax, guard, label);
			if (boundConstantPattern.ConstantValue == ConstantValue.Null)
			{
				return AddByNull(decisionTree, makeDecision2);
			}
			return AddByValue(decisionTree, boundConstantPattern, makeDecision2);
		}
		case BoundKind.DeclarationPattern:
		{
			BoundDeclarationPattern declarationPattern = (BoundDeclarationPattern)pattern;
			DecisionMaker makeDecision = (BoundExpression e, TypeSymbol t) => new DecisionTree.Guarded(e, t, ImmutableArray.Create(new KeyValuePair<BoundExpression, BoundExpression>(e, declarationPattern.VariableAccess)), sectionSyntax, guard, label);
			if (declarationPattern.IsVar)
			{
				return Add(decisionTree, makeDecision);
			}
			return AddByType(decisionTree, declarationPattern.DeclaredType.Type, makeDecision);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(pattern.Kind);
		}
	}

	private DecisionTree AddByValue(DecisionTree decision, BoundConstantPattern value, DecisionMaker makeDecision)
	{
		if (value.ConstantValue == null)
		{
			return null;
		}
		return decision.Kind switch
		{
			DecisionTree.DecisionKind.ByType => AddByValue((DecisionTree.ByType)decision, value, makeDecision), 
			DecisionTree.DecisionKind.ByValue => AddByValue((DecisionTree.ByValue)decision, value, makeDecision), 
			DecisionTree.DecisionKind.Guarded => AddByValue((DecisionTree.Guarded)decision, value, makeDecision), 
			_ => throw ExceptionUtilities.UnexpectedValue(decision.Kind), 
		};
	}

	private DecisionTree AddByValue(DecisionTree.Guarded guarded, BoundConstantPattern value, DecisionMaker makeDecision)
	{
		if (guarded.Default == null)
		{
			guarded.Default = new DecisionTree.ByType(guarded.Expression, guarded.Type, null);
		}
		return AddByValue(guarded.Default, value, makeDecision);
	}

	private DecisionTree AddByValue(DecisionTree.ByValue byValue, BoundConstantPattern value, DecisionMaker makeDecision)
	{
		if (byValue.Default != null)
		{
			return AddByValue(byValue.Default, value, makeDecision);
		}
		object value2 = value.ConstantValue.Value;
		if (byValue.ValueAndDecision.TryGetValue(value2, out var value3))
		{
			value3 = Add(value3, makeDecision);
		}
		else
		{
			value3 = makeDecision(byValue.Expression, byValue.Type);
			byValue.ValueAndDecision.Add(value2, value3);
		}
		if (byValue.Type.SpecialType == SpecialType.System_Boolean && byValue.ValueAndDecision.Count == 2 && byValue.ValueAndDecision.Values.All((DecisionTree d) => d.MatchIsComplete))
		{
			byValue.MatchIsComplete = true;
		}
		return value3;
	}

	private DecisionTree AddByValue(DecisionTree.ByType byType, BoundConstantPattern value, DecisionMaker makeDecision)
	{
		if (byType.Default != null)
		{
			try
			{
				return AddByValue(byType.Default, value, makeDecision);
			}
			finally
			{
				if (byType.Default.MatchIsComplete)
				{
					byType.MatchIsComplete = true;
				}
			}
		}
		if (value.ConstantValue == ConstantValue.Null)
		{
			throw ExceptionUtilities.Unreachable;
		}
		if ((object)value.Value.Type == null || value.ConstantValue == null)
		{
			return null;
		}
		foreach (KeyValuePair<TypeSymbol, DecisionTree> item in byType.TypeAndDecision)
		{
			TypeSymbol key = item.Key;
			DecisionTree value2 = item.Value;
			bool? flag = ExpressionOfTypeMatchesPatternType(value.Value.Type, key, ref _useSiteDiagnostics);
			if (flag.HasValue)
			{
				bool valueOrDefault = flag == true;
				if (valueOrDefault && valueOrDefault && value2.MatchIsComplete)
				{
					return null;
				}
			}
		}
		DecisionTree decisionTree = null;
		for (int num = byType.TypeAndDecision.Count - 1; num >= 0; num--)
		{
			KeyValuePair<TypeSymbol, DecisionTree> keyValuePair = byType.TypeAndDecision[num];
			TypeSymbol key2 = keyValuePair.Key;
			DecisionTree value3 = keyValuePair.Value;
			if (key2.Equals(value.Value.Type, TypeCompareKind.IgnoreDynamicAndTupleNames))
			{
				decisionTree = value3;
				break;
			}
			bool? flag = ExpressionOfTypeMatchesPatternType(value.Value.Type, key2, ref _useSiteDiagnostics);
			if (!flag.HasValue)
			{
				break;
			}
			bool valueOrDefault = flag == true;
			if (valueOrDefault && valueOrDefault)
			{
				if (!value3.MatchIsComplete)
				{
					break;
				}
				return null;
			}
		}
		if (decisionTree == null)
		{
			TypeSymbol type = value.Value.Type;
			if (byType.Type.Equals(type, TypeCompareKind.AllIgnoreOptions))
			{
				decisionTree = new DecisionTree.ByValue(byType.Expression, type.TupleUnderlyingTypeOrSelf(), null);
			}
			else
			{
				BoundLocal boundPatternMatchingLocal = GetBoundPatternMatchingLocal(type);
				decisionTree = new DecisionTree.ByValue(boundPatternMatchingLocal, type.TupleUnderlyingTypeOrSelf(), boundPatternMatchingLocal.LocalSymbol);
			}
			byType.TypeAndDecision.Add(new KeyValuePair<TypeSymbol, DecisionTree>(type, decisionTree));
		}
		return AddByValue(decisionTree, value, makeDecision);
	}

	internal bool? ExpressionOfTypeMatchesPatternType(TypeSymbol expressionType, TypeSymbol patternType, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		Conversion conversion;
		return Binder.ExpressionOfTypeMatchesPatternType(_conversions, expressionType, patternType, ref _useSiteDiagnostics, out conversion);
	}

	private DecisionTree AddByType(DecisionTree decision, TypeSymbol type, DecisionMaker makeDecision)
	{
		if (!decision.MatchIsComplete)
		{
			ConstantValue constantValue = decision.Expression.ConstantValue;
			if ((object)constantValue == null || !constantValue.IsNull)
			{
				switch (decision.Kind)
				{
				case DecisionTree.DecisionKind.ByType:
					return AddByType((DecisionTree.ByType)decision, type, makeDecision);
				case DecisionTree.DecisionKind.ByValue:
				{
					DecisionTree.ByValue byValue = (DecisionTree.ByValue)decision;
					DecisionTree result;
					if (byValue.Default == null)
					{
						if (byValue.Type.Equals(type, TypeCompareKind.IgnoreDynamicAndTupleNames))
						{
							result = (byValue.Default = makeDecision(byValue.Expression, byValue.Type));
						}
						else
						{
							byValue.Default = new DecisionTree.ByType(byValue.Expression, byValue.Type, null);
							result = AddByType(byValue.Default, type, makeDecision);
						}
					}
					else
					{
						result = AddByType(byValue.Default, type, makeDecision);
					}
					if (byValue.Default.MatchIsComplete)
					{
						byValue.MatchIsComplete = true;
					}
					return result;
				}
				case DecisionTree.DecisionKind.Guarded:
					return AddByType((DecisionTree.Guarded)decision, type, makeDecision);
				default:
					throw ExceptionUtilities.UnexpectedValue(decision.Kind);
				}
			}
		}
		return null;
	}

	private DecisionTree AddByType(DecisionTree.Guarded guarded, TypeSymbol type, DecisionMaker makeDecision)
	{
		if (guarded.Default == null)
		{
			guarded.Default = new DecisionTree.ByType(guarded.Expression, guarded.Type, null);
		}
		DecisionTree result = AddByType(guarded.Default, type, makeDecision);
		if (guarded.Default.MatchIsComplete)
		{
			guarded.MatchIsComplete = true;
		}
		return result;
	}

	private DecisionTree AddByType(DecisionTree.ByType byType, TypeSymbol type, DecisionMaker makeDecision)
	{
		if (byType.Default != null)
		{
			try
			{
				return AddByType(byType.Default, type, makeDecision);
			}
			finally
			{
				if (byType.Default.MatchIsComplete)
				{
					byType.MatchIsComplete = true;
				}
			}
		}
		DecisionTree decisionTree = null;
		if (byType.TypeAndDecision.Count != 0)
		{
			KeyValuePair<TypeSymbol, DecisionTree> keyValuePair = byType.TypeAndDecision.Last();
			if (keyValuePair.Key.Equals(type, TypeCompareKind.IgnoreDynamicAndTupleNames))
			{
				decisionTree = Add(keyValuePair.Value, makeDecision);
			}
		}
		if (decisionTree == null)
		{
			BoundLocal boundPatternMatchingLocal = GetBoundPatternMatchingLocal(type);
			decisionTree = makeDecision(boundPatternMatchingLocal, type);
			decisionTree.Temp = boundPatternMatchingLocal.LocalSymbol;
			byType.TypeAndDecision.Add(new KeyValuePair<TypeSymbol, DecisionTree>(type, decisionTree));
		}
		if (ExpressionOfTypeMatchesPatternType(byType.Type, type, ref _useSiteDiagnostics) == true && decisionTree.MatchIsComplete)
		{
			DecisionTree whenNull = byType.WhenNull;
			if (whenNull != null && whenNull.MatchIsComplete)
			{
				byType.MatchIsComplete = true;
			}
		}
		return decisionTree;
	}

	private DecisionTree AddByNull(DecisionTree decision, DecisionMaker makeDecision)
	{
		return decision.Kind switch
		{
			DecisionTree.DecisionKind.ByType => AddByNull((DecisionTree.ByType)decision, makeDecision), 
			DecisionTree.DecisionKind.ByValue => throw ExceptionUtilities.Unreachable, 
			DecisionTree.DecisionKind.Guarded => AddByNull((DecisionTree.Guarded)decision, makeDecision), 
			_ => throw ExceptionUtilities.UnexpectedValue(decision.Kind), 
		};
	}

	private DecisionTree AddByNull(DecisionTree.ByType byType, DecisionMaker makeDecision)
	{
		if (byType.Default != null)
		{
			try
			{
				return AddByNull(byType.Default, makeDecision);
			}
			finally
			{
				if (byType.Default.MatchIsComplete)
				{
					byType.MatchIsComplete = true;
				}
			}
		}
		DecisionTree result = ((byType.WhenNull != null) ? Add(byType.WhenNull, makeDecision) : (byType.WhenNull = makeDecision(byType.Expression, byType.Type)));
		if (byType.WhenNull.MatchIsComplete && NonNullHandled(byType))
		{
			byType.MatchIsComplete = true;
		}
		return result;
	}

	private bool NonNullHandled(DecisionTree.ByType byType)
	{
		TypeSymbol expressionType = byType.Type.StrippedType().TupleUnderlyingTypeOrSelf();
		foreach (KeyValuePair<TypeSymbol, DecisionTree> item in byType.TypeAndDecision)
		{
			TypeSymbol key = item.Key;
			DecisionTree value = item.Value;
			if (ExpressionOfTypeMatchesPatternType(expressionType, key, ref _useSiteDiagnostics) == true && value.MatchIsComplete)
			{
				return true;
			}
		}
		return false;
	}

	private DecisionTree AddByNull(DecisionTree.Guarded guarded, DecisionMaker makeDecision)
	{
		if (guarded.Default == null)
		{
			guarded.Default = new DecisionTree.ByType(guarded.Expression, guarded.Type, null);
		}
		DecisionTree result = AddByNull(guarded.Default, makeDecision);
		if (guarded.Default.MatchIsComplete)
		{
			guarded.MatchIsComplete = true;
		}
		return result;
	}

	protected DecisionTree Add(DecisionTree decision, DecisionMaker makeDecision)
	{
		return decision.Kind switch
		{
			DecisionTree.DecisionKind.ByType => Add((DecisionTree.ByType)decision, makeDecision), 
			DecisionTree.DecisionKind.ByValue => Add((DecisionTree.ByValue)decision, makeDecision), 
			DecisionTree.DecisionKind.Guarded => Add((DecisionTree.Guarded)decision, makeDecision), 
			_ => throw ExceptionUtilities.UnexpectedValue(decision.Kind), 
		};
	}

	private DecisionTree Add(DecisionTree.Guarded guarded, DecisionMaker makeDecision)
	{
		if (guarded.Default != null)
		{
			DecisionTree result = Add(guarded.Default, makeDecision);
			if (guarded.Default.MatchIsComplete)
			{
				guarded.MatchIsComplete = true;
			}
			return result;
		}
		DecisionTree result2 = (guarded.Default = makeDecision(guarded.Expression, guarded.Type));
		if (guarded.Default.MatchIsComplete)
		{
			guarded.MatchIsComplete = true;
		}
		return result2;
	}

	private DecisionTree Add(DecisionTree.ByValue byValue, DecisionMaker makeDecision)
	{
		DecisionTree result = ((byValue.Default == null) ? (byValue.Default = makeDecision(byValue.Expression, byValue.Type)) : Add(byValue.Default, makeDecision));
		if (byValue.Default.MatchIsComplete)
		{
			byValue.MatchIsComplete = true;
		}
		return result;
	}

	private DecisionTree Add(DecisionTree.ByType byType, DecisionMaker makeDecision)
	{
		try
		{
			if (byType.Default == null)
			{
				byType.Default = makeDecision(byType.Expression, byType.Type);
				return byType.Default;
			}
			return Add(byType.Default, makeDecision);
		}
		finally
		{
			if (byType.Default.MatchIsComplete)
			{
				byType.MatchIsComplete = true;
			}
		}
	}
}
