using System;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NavSequenceProviderExtension
{
	private static T GetNextValueFor<T>(this INavSequenceProvider sequence, string name)
	{
		long nextValue = sequence.GetNextValue(name);
		return (T)Convert.ChangeType(nextValue, typeof(T), CultureInfo.InvariantCulture);
	}

	public static T GetNextValueFor<T>(this INavSequenceProvider sequence, NCLMetaTable table, NCLMetaField field, CompanyId companyId)
	{
		string name = NavSqlStatementHelper.SequenceNameForAutoIncrementField(table, field, companyId);
		return sequence.GetNextValueFor<T>(name);
	}

	public static NavValue GetNextAutoIncrementValue(this INavSequenceProvider sequence, NCLMetaTable table, CompanyId companyId)
	{
		NCLMetaField autoIncrementField = table.AutoIncrementField;
		if (autoIncrementField.SqlDataType == SqlDataType.BigInt)
		{
			return NavBigInteger.Create(sequence.GetNextValueFor<long>(table, autoIncrementField, companyId));
		}
		return NavInteger.Create(sequence.GetNextValueFor<int>(table, autoIncrementField, companyId));
	}

	public static void UpdateSequenceCurrentValue(this INavSequenceProvider sequence, NCLMetaTable table, NCLMetaField field, CompanyId companyId, NavValue value)
	{
		string sequenceName = NavSqlStatementHelper.SequenceNameForAutoIncrementField(table, field, companyId);
		sequence.UpdateSequenceCurrentValue(sequenceName, value.ToInt64());
	}
}
