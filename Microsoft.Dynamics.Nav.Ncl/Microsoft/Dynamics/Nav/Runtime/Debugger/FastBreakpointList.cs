using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class FastBreakpointList : IEnumerable<Breakpoint>, IEnumerable
{
	private static readonly Expression notFoundIndexExpression = Expression.Constant(-1);

	private static readonly ParameterExpression scopeIdParameterExpression = Expression.Parameter(typeof(int), "scopeId");

	private static readonly ParameterExpression statementNumberParameterExpression = Expression.Parameter(typeof(int), "statementNumber");

	private static readonly Func<int, int, int> noBreakpointsLookupFunction = Expression.Lambda<Func<int, int, int>>(notFoundIndexExpression, new ParameterExpression[2] { scopeIdParameterExpression, statementNumberParameterExpression }).Compile();

	private readonly Breakpoint[] breakpoints;

	private readonly Func<int, int, int> indexLookupFunction;

	public Breakpoint this[int scopeId, int statementId]
	{
		get
		{
			int num = indexLookupFunction(scopeId, statementId);
			if (num < 0)
			{
				return null;
			}
			return breakpoints[num];
		}
	}

	public FastBreakpointList()
		: this(new Breakpoint[0])
	{
	}

	public FastBreakpointList(IEnumerable<Breakpoint> breakpoints)
	{
		if (breakpoints == null)
		{
			throw new ArgumentNullException("breakpoints");
		}
		this.breakpoints = breakpoints.ToArray();
		indexLookupFunction = BuildLookupFunction();
	}

	public FastBreakpointList Add(Breakpoint breakpoint)
	{
		if (this[breakpoint.ScopeId, breakpoint.StatementNumber] != null)
		{
			return this;
		}
		return new FastBreakpointList(breakpoints.Concat(new Breakpoint[1] { breakpoint }));
	}

	public FastBreakpointList Remove(long breakpointId, out Breakpoint breakpointRemoved)
	{
		Breakpoint breakpoint = new Breakpoint(breakpointId);
		return Remove(breakpoint.ScopeId, breakpoint.StatementNumber, out breakpointRemoved);
	}

	public FastBreakpointList Remove(int scopeId, int statementNumber, out Breakpoint breakpointRemoved)
	{
		breakpointRemoved = this[scopeId, statementNumber];
		if (breakpointRemoved == null)
		{
			return this;
		}
		return new FastBreakpointList(breakpoints.Where((Breakpoint bp) => bp.ScopeId != scopeId || bp.StatementNumber != statementNumber));
	}

	public FastBreakpointList Replace(Breakpoint breakpoint)
	{
		return new FastBreakpointList(breakpoints.Select((Breakpoint bp) => (bp.Id != breakpoint.Id) ? bp : breakpoint));
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IEnumerator<Breakpoint> GetEnumerator()
	{
		return ((IEnumerable<Breakpoint>)breakpoints).GetEnumerator();
	}

	private Func<int, int, int> BuildLookupFunction()
	{
		if (breakpoints.Length == 0)
		{
			return noBreakpointsLookupFunction;
		}
		Dictionary<Breakpoint, int> breakpointToIndexMap = breakpoints.Select((Breakpoint bp, int index) => new { bp, index }).ToDictionary(p => p.bp, p => p.index);
		IEnumerable<IGrouping<int, Breakpoint>> source = from bp in breakpoints
			group bp by bp.ScopeId;
		IEnumerable<SwitchCase> cases = from scopeGroup in source
			let statementSwitchCases = scopeGroup.Select((Breakpoint bp) => Expression.SwitchCase(Expression.Constant(breakpointToIndexMap[bp]), Expression.Constant(bp.StatementNumber)))
			let switchExpression = Expression.Switch(statementNumberParameterExpression, notFoundIndexExpression, null, statementSwitchCases)
			select Expression.SwitchCase(switchExpression, Expression.Constant(scopeGroup.Key));
		SwitchExpression body = Expression.Switch(scopeIdParameterExpression, notFoundIndexExpression, null, cases);
		return Expression.Lambda<Func<int, int, int>>(body, new ParameterExpression[2] { scopeIdParameterExpression, statementNumberParameterExpression }).Compile();
	}
}
