using System;

namespace Microsoft.Dynamics.Nav.Types;

public class SqlStatementStatistics
{
	public string SqlStatement { get; set; }

	public DateTime ExecuteStart { get; set; }

	public double Duration { get; set; }

	public string SqlIOStats { get; set; }

	public string SqlExecutionPlan { get; set; }

	public int RowsRead { get; set; }

	public string CallStack { get; set; }
}
