using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class CaseLine
{
	public List<Expression> Exprs { get; set; }

	public Statement Stmt { get; set; }

	public bool ConstsOnly { get; set; }

	public CaseLine(List<Expression> exprs, Statement stmt)
	{
		Exprs = exprs;
		Stmt = stmt;
		ConstsOnly = true;
	}
}
