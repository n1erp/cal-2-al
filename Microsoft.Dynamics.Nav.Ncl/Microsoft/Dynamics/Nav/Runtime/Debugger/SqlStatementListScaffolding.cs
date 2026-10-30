using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class SqlStatementListScaffolding : Scaffolding
{
	public override string TypeString => string.Empty;

	protected override bool ProbablyHasChildren => true;

	public SqlStatementListScaffolding(string name)
		: base(name, typeof(IEnumerable<SqlStatementStatistics>), new IdentityValueBinder(), childrenTypesCanChange: false, promotedChildren: true)
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
		if (!(instance is IEnumerable<SqlStatementStatistics> enumerable))
		{
			yield break;
		}
		int index = 0;
		foreach (SqlStatementStatistics item in enumerable)
		{
			SqlStatementStatisticsScaffolding sqlStatementStatisticsScaffolding = new SqlStatementStatisticsScaffolding("SQL" + index++);
			sqlStatementStatisticsScaffolding.Expand(item);
			yield return sqlStatementStatisticsScaffolding;
		}
	}
}
