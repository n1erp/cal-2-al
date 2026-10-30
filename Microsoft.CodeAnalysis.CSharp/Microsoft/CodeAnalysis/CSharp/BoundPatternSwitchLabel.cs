using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundPatternSwitchLabel : BoundNode
{
	public LabelSymbol Label { get; }

	public BoundPattern Pattern { get; }

	public BoundExpression Guard { get; }

	public bool IsReachable { get; }

	public BoundPatternSwitchLabel(SyntaxNode syntax, LabelSymbol label, BoundPattern pattern, BoundExpression guard, bool isReachable, bool hasErrors = false)
		: base(BoundKind.PatternSwitchLabel, syntax, hasErrors || pattern.HasErrors() || guard.HasErrors())
	{
		Label = label;
		Pattern = pattern;
		Guard = guard;
		IsReachable = isReachable;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitPatternSwitchLabel(this);
	}

	public BoundPatternSwitchLabel Update(LabelSymbol label, BoundPattern pattern, BoundExpression guard, bool isReachable)
	{
		if (label != Label || pattern != Pattern || guard != Guard || isReachable != IsReachable)
		{
			return new BoundPatternSwitchLabel(Syntax, label, pattern, guard, isReachable, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
