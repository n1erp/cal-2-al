using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundCaseStatement : BoundStatement, ICaseStatement, IOperation
{
	public BoundExpression Expression { get; private set; }

	public ImmutableArray<BoundCaseLine> CaseLines { get; private set; }

	public BoundStatementList CaseElse { get; private set; }

	IOperation ICaseStatement.Value => Expression;

	ImmutableArray<ICaseLineOperation> ICaseStatement.CaseLines => CaseLines.As<ICaseLineOperation>();

	IOperation ICaseStatement.ElseStatement => CaseElse;

	protected override OperationKind StatementKind => OperationKind.CaseStatement;

	public BoundCaseStatement(SyntaxNode syntax, BoundExpression expression, ImmutableArray<BoundCaseLine> caseLines, BoundStatementList caseElse, bool hasErrors = false)
		: base(BoundKind.CaseStatement, syntax, hasErrors || expression.HasErrors() || caseLines.HasErrors() || caseElse.HasErrors())
	{
		Expression = expression;
		CaseLines = caseLines;
		CaseElse = caseElse;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitCaseStatement(this);
	}

	public BoundCaseStatement Update(BoundExpression expression, ImmutableArray<BoundCaseLine> caseLines, BoundStatementList caseElse)
	{
		if (expression != Expression || caseLines != CaseLines || caseElse != CaseElse)
		{
			return new BoundCaseStatement(Syntax, expression, caseLines, caseElse, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitCaseStatement(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitCaseStatement(this, argument);
	}
}
