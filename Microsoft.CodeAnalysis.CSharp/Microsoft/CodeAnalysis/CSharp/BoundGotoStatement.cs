using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundGotoStatement : BoundStatement
{
	public LabelSymbol Label { get; }

	public BoundExpression CaseExpressionOpt { get; }

	public BoundLabel LabelExpressionOpt { get; }

	public BoundGotoStatement(SyntaxNode syntax, LabelSymbol label, bool hasErrors = false)
		: this(syntax, label, null, null, hasErrors)
	{
	}

	public BoundGotoStatement(SyntaxNode syntax, LabelSymbol label, BoundExpression caseExpressionOpt, BoundLabel labelExpressionOpt, bool hasErrors = false)
		: base(BoundKind.GotoStatement, syntax, hasErrors || caseExpressionOpt.HasErrors() || labelExpressionOpt.HasErrors())
	{
		Label = label;
		CaseExpressionOpt = caseExpressionOpt;
		LabelExpressionOpt = labelExpressionOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitGotoStatement(this);
	}

	public BoundGotoStatement Update(LabelSymbol label, BoundExpression caseExpressionOpt, BoundLabel labelExpressionOpt)
	{
		if (label != Label || caseExpressionOpt != CaseExpressionOpt || labelExpressionOpt != LabelExpressionOpt)
		{
			return new BoundGotoStatement(Syntax, label, caseExpressionOpt, labelExpressionOpt, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
