using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class DecisionTree
{
	public enum DecisionKind
	{
		ByType,
		ByValue,
		Guarded
	}

	public class ByType : DecisionTree
	{
		public DecisionTree WhenNull;

		public readonly ArrayBuilder<KeyValuePair<TypeSymbol, DecisionTree>> TypeAndDecision = new ArrayBuilder<KeyValuePair<TypeSymbol, DecisionTree>>();

		public DecisionTree Default;

		public override DecisionKind Kind => DecisionKind.ByType;

		public ByType(BoundExpression expression, TypeSymbol type, LocalSymbol temp)
			: base(expression, type, temp)
		{
		}
	}

	public class ByValue : DecisionTree
	{
		public readonly Dictionary<object, DecisionTree> ValueAndDecision = new Dictionary<object, DecisionTree>();

		public DecisionTree Default;

		public override DecisionKind Kind => DecisionKind.ByValue;

		public ByValue(BoundExpression expression, TypeSymbol type, LocalSymbol temp)
			: base(expression, type, temp)
		{
		}
	}

	public class Guarded : DecisionTree
	{
		public readonly ImmutableArray<KeyValuePair<BoundExpression, BoundExpression>> Bindings;

		public readonly SyntaxNode SectionSyntax;

		public readonly BoundExpression Guard;

		public readonly BoundPatternSwitchLabel Label;

		public DecisionTree Default;

		public override DecisionKind Kind => DecisionKind.Guarded;

		public Guarded(BoundExpression expression, TypeSymbol type, ImmutableArray<KeyValuePair<BoundExpression, BoundExpression>> bindings, SyntaxNode sectionSyntax, BoundExpression guard, BoundPatternSwitchLabel label)
			: base(expression, type, null)
		{
			Guard = guard;
			Label = label;
			Bindings = bindings;
			SectionSyntax = sectionSyntax;
			MatchIsComplete = guard == null || guard.ConstantValue == ConstantValue.True;
		}
	}

	public readonly BoundExpression Expression;

	public readonly TypeSymbol Type;

	public LocalSymbol Temp;

	public bool MatchIsComplete;

	public abstract DecisionKind Kind { get; }

	public DecisionTree(BoundExpression expression, TypeSymbol type, LocalSymbol temp)
	{
		Expression = expression;
		Type = type;
		Temp = temp;
	}
}
