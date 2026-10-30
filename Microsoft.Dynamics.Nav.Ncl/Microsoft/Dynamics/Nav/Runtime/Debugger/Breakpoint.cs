using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

[DebuggerDisplay("{scopeId}, {statementNumber}, {conditionExpression}")]
public class Breakpoint
{
	private readonly string conditionExpression;

	private readonly int scopeId;

	private readonly int statementNumber;

	public string ConditionExpression
	{
		[DebuggerStepThrough]
		get
		{
			return conditionExpression;
		}
	}

	public long Id => BreakpointId(ScopeId, StatementNumber);

	public int ScopeId
	{
		[DebuggerStepThrough]
		get
		{
			return scopeId;
		}
	}

	public int StatementNumber
	{
		[DebuggerStepThrough]
		get
		{
			return statementNumber;
		}
	}

	public Breakpoint(int scopeId, int statementNumber, string conditionExpression)
	{
		this.scopeId = scopeId;
		this.statementNumber = statementNumber;
		this.conditionExpression = conditionExpression?.Trim();
	}

	public Breakpoint(long breakpointId, string conditionExpression)
	{
		scopeId = (int)(breakpointId >> 32);
		statementNumber = (int)breakpointId;
		this.conditionExpression = conditionExpression?.Trim();
	}

	public Breakpoint(long breakpointId)
		: this(breakpointId, null)
	{
	}

	public static long BreakpointId(int scopeId, int statementNumber)
	{
		return ((long)scopeId << 32) + statementNumber;
	}
}
