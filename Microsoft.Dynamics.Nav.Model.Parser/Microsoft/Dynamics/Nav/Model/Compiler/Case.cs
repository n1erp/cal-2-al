using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class Case : Statement
{
	public Expression CaseExpr { get; set; }

	public List<CaseLine> CaseLines { get; set; }

	public List<Statement> ElseStmts { get; set; }

	public Case(Expression caseExpr, List<CaseLine> caseLines, List<Statement> elseStmts, TextPosition position = null)
		: base(AstNodeType.CaseNode, position)
	{
		CaseExpr = caseExpr;
		CaseLines = caseLines;
		ElseStmts = elseStmts;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		AcceptAstNode(CaseExpr, visitor);
		if (CaseLines != null)
		{
			foreach (CaseLine caseLine in CaseLines)
			{
				AcceptExpressions(caseLine.Exprs, visitor);
				AcceptAstNode(caseLine.Stmt, visitor);
			}
		}
		AcceptStatements(ElseStmts, visitor);
		visitor.VisitCaseNode(this);
	}
}
