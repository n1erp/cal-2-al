using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class SubsumptionDiagnosticBuilder : DecisionTreeBuilder
{
	private readonly DecisionTree _subsumptionTree;

	internal bool IsComplete => _subsumptionTree.MatchIsComplete;

	internal SubsumptionDiagnosticBuilder(Symbol enclosingSymbol, SwitchStatementSyntax syntax, Conversions conversions, TypeSymbol switchGoverningType)
		: base(enclosingSymbol, syntax, conversions)
	{
		BoundDup expression = new BoundDup(syntax, RefKind.None, switchGoverningType);
		_subsumptionTree = CreateEmptyDecisionTree(expression);
	}

	internal bool AddLabel(BoundPatternSwitchLabel label, DiagnosticBag diagnostics)
	{
		if (label.Syntax.Kind() == SyntaxKind.DefaultSwitchLabel)
		{
			return true;
		}
		try
		{
			bool inputCouldBeNull = _subsumptionTree.Type.CanContainNull();
			ErrorCode errorCode = CheckSubsumed(label.Pattern, _subsumptionTree, inputCouldBeNull);
			if (errorCode != 0)
			{
				if (!label.HasErrors && errorCode != ErrorCode.ERR_NoImplicitConvCast)
				{
					diagnostics.Add(errorCode, label.Pattern.Syntax.Location);
				}
				return false;
			}
			if ((label.Guard == null || label.Guard.ConstantValue == ConstantValue.True) && AddToDecisionTree(_subsumptionTree, null, label) == null && !label.Pattern.HasErrors)
			{
				throw ExceptionUtilities.Unreachable;
			}
			return true;
		}
		finally
		{
			diagnostics.Add(label.Syntax.Location, _useSiteDiagnostics);
			_useSiteDiagnostics.Clear();
		}
	}

	private ErrorCode CheckSubsumed(BoundPattern pattern, DecisionTree decisionTree, bool inputCouldBeNull)
	{
		if (decisionTree.MatchIsComplete)
		{
			return ErrorCode.ERR_PatternIsSubsumed;
		}
		switch (pattern.Kind)
		{
		case BoundKind.ConstantPattern:
		{
			BoundConstantPattern boundConstantPattern = (BoundConstantPattern)pattern;
			if (boundConstantPattern.Value.HasErrors || boundConstantPattern.Value.ConstantValue == null || boundConstantPattern.Value.ConstantValue.IsBad)
			{
				return ErrorCode.ERR_NoImplicitConvCast;
			}
			bool isNull = boundConstantPattern.Value.ConstantValue.IsNull;
			if (isNull && !inputCouldBeNull)
			{
				return ErrorCode.ERR_PatternIsSubsumed;
			}
			switch (decisionTree.Kind)
			{
			case DecisionTree.DecisionKind.ByValue:
			{
				DecisionTree.ByValue byValue2 = (DecisionTree.ByValue)decisionTree;
				if (isNull)
				{
					throw ExceptionUtilities.Unreachable;
				}
				if (byValue2.ValueAndDecision.TryGetValue(boundConstantPattern.Value.ConstantValue.Value, out var value3))
				{
					ErrorCode errorCode4 = CheckSubsumed(pattern, value3, inputCouldBeNull);
					if (errorCode4 != 0)
					{
						return errorCode4;
					}
				}
				if (byValue2.Default != null)
				{
					return CheckSubsumed(pattern, byValue2.Default, inputCouldBeNull);
				}
				return (ErrorCode)0;
			}
			case DecisionTree.DecisionKind.ByType:
			{
				DecisionTree.ByType byType2 = (DecisionTree.ByType)decisionTree;
				if (isNull)
				{
					if (byType2.WhenNull != null)
					{
						ErrorCode errorCode2 = CheckSubsumed(pattern, byType2.WhenNull, inputCouldBeNull);
						if (errorCode2 != 0)
						{
							return errorCode2;
						}
					}
				}
				else
				{
					foreach (KeyValuePair<TypeSymbol, DecisionTree> item in byType2.TypeAndDecision)
					{
						TypeSymbol key2 = item.Key;
						DecisionTree value2 = item.Value;
						if (ExpressionOfTypeMatchesPatternType(boundConstantPattern.Value.Type, key2, ref _useSiteDiagnostics) == true)
						{
							ErrorCode errorCode3 = CheckSubsumed(pattern, value2, inputCouldBeNull: false);
							if (errorCode3 != 0)
							{
								return errorCode3;
							}
						}
					}
				}
				if (byType2.Default == null)
				{
					return (ErrorCode)0;
				}
				return CheckSubsumed(pattern, byType2.Default, inputCouldBeNull);
			}
			case DecisionTree.DecisionKind.Guarded:
				throw ExceptionUtilities.Unreachable;
			default:
				throw ExceptionUtilities.UnexpectedValue(decisionTree.Kind);
			}
		}
		case BoundKind.DeclarationPattern:
		{
			BoundDeclarationPattern boundDeclarationPattern = (BoundDeclarationPattern)pattern;
			switch (decisionTree.Kind)
			{
			case DecisionTree.DecisionKind.ByValue:
			{
				if (inputCouldBeNull)
				{
					return (ErrorCode)0;
				}
				DecisionTree.ByValue byValue = (DecisionTree.ByValue)decisionTree;
				if (byValue.Default != null)
				{
					return CheckSubsumed(pattern, byValue.Default, inputCouldBeNull: false);
				}
				return (ErrorCode)0;
			}
			case DecisionTree.DecisionKind.ByType:
			{
				DecisionTree.ByType byType = (DecisionTree.ByType)decisionTree;
				if ((boundDeclarationPattern.IsVar & inputCouldBeNull) && (byType.WhenNull == null || CheckSubsumed(pattern, byType.WhenNull, inputCouldBeNull) == 0) && (byType.Default == null || CheckSubsumed(pattern, byType.Default, inputCouldBeNull) == 0))
				{
					return (ErrorCode)0;
				}
				inputCouldBeNull = false;
				foreach (KeyValuePair<TypeSymbol, DecisionTree> item2 in byType.TypeAndDecision)
				{
					TypeSymbol key = item2.Key;
					DecisionTree value = item2.Value;
					if (ExpressionOfTypeMatchesPatternType(boundDeclarationPattern.DeclaredType.Type.TupleUnderlyingTypeOrSelf(), key, ref _useSiteDiagnostics) == true || ExpressionOfTypeMatchesPatternType(byType.Type, key, ref _useSiteDiagnostics) == true)
					{
						ErrorCode errorCode = CheckSubsumed(pattern, value, inputCouldBeNull);
						if (errorCode != 0)
						{
							return errorCode;
						}
					}
				}
				if (byType.Default != null)
				{
					return CheckSubsumed(pattern, byType.Default, inputCouldBeNull);
				}
				return (ErrorCode)0;
			}
			case DecisionTree.DecisionKind.Guarded:
				throw ExceptionUtilities.Unreachable;
			default:
				throw ExceptionUtilities.UnexpectedValue(decisionTree.Kind);
			}
		}
		case BoundKind.WildcardPattern:
			throw ExceptionUtilities.UnexpectedValue(decisionTree.Kind);
		default:
			throw ExceptionUtilities.UnexpectedValue(pattern.Kind);
		}
	}
}
