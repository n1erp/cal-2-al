using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class SqlStatementStatisticsScaffolding : Scaffolding
{
	public override string TypeString => string.Empty;

	protected override bool ProbablyHasChildren => true;

	public SqlStatementStatisticsScaffolding(string name)
		: base(name, typeof(SqlStatementStatistics), new IdentityValueBinder(), childrenTypesCanChange: false, promotedChildren: true)
	{
	}

	protected internal override string GetStringValue(object instance)
	{
		return string.Empty;
	}

	protected override bool IsCompatibleOverride(object instance)
	{
		if (instance != null)
		{
			return instance.GetType() == TargetType;
		}
		return false;
	}

	protected override IEnumerable<Scaffolding> CreateChildren(object instance)
	{
		SqlStatementStatistics sqlStatementStatistics = instance as SqlStatementStatistics;
		if (sqlStatementStatistics == null)
		{
			yield break;
		}
		yield return new TypeScaffolding("Statement", typeof(string), new DelegateValueBinder((object input) => sqlStatementStatistics.SqlStatement, typeof(string)));
		if (!string.IsNullOrWhiteSpace(sqlStatementStatistics.CallStack))
		{
			yield return new TypeScaffolding("AL Call Stack", typeof(string), new DelegateValueBinder((object input) => sqlStatementStatistics.CallStack, typeof(string)));
		}
		yield return new TypeScaffolding("Execution Time (UTC)", typeof(DateTime), new DelegateValueBinder((object input) => sqlStatementStatistics.ExecuteStart, typeof(DateTime)));
		yield return new TypeScaffolding("Duration (ms)", typeof(double), new DelegateValueBinder((object input) => sqlStatementStatistics.Duration, typeof(double)));
		yield return new TypeScaffolding("Approx. Rows Read", typeof(long), new DelegateValueBinder((object input) => sqlStatementStatistics.RowsRead, typeof(long)));
	}
}
