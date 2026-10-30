using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlWhereClauseVisitorInputOutput
{
	private readonly bool createTextualWhereClause;

	private readonly StringBuilder whereClause;

	private readonly string fixedParentName;

	private readonly SqlParameterCollection sqlParameters;

	private readonly Dictionary<NCLMetaQueryDataItem, NCLMetaKey> coveringSIFTIndexes;

	private readonly bool treatNullAsDefaultValue;

	private readonly SqlSortingProperties sqlSortingProperties;

	private NavSqlParameterHandler sqlParameterHandler;

	public bool CreateTextualWhereClause => createTextualWhereClause;

	public StringBuilder WhereClause => whereClause;

	public SqlParameterCollection Parameters => sqlParameters;

	public Dictionary<NCLMetaQueryDataItem, NCLMetaKey> CoveringSIFTIndexes => coveringSIFTIndexes;

	public bool TreatNullAsDefaultValue => treatNullAsDefaultValue;

	public string FixedParentName => fixedParentName;

	public NavSqlParameterHandler SqlParameterHandling => sqlParameterHandler;

	public SqlSortingProperties SqlSortingProperties => sqlSortingProperties;

	public bool ShouldAliasQualifyFields { get; }

	private NavSqlWhereClauseVisitorInputOutput(bool createTextualWhereClause, SqlParameterCollection parameters, StringBuilder whereClause, NavSqlParameterHandler sqlParameterHandling, Dictionary<NCLMetaQueryDataItem, NCLMetaKey> coveringSIFTIndexes, bool treatNullAsDefaultValue, string fixedParentName, SqlSortingProperties sqlSortingProperties, bool shouldAliasQualifyFields)
	{
		this.createTextualWhereClause = createTextualWhereClause;
		sqlParameters = parameters;
		this.whereClause = whereClause;
		sqlParameterHandler = sqlParameterHandling;
		this.treatNullAsDefaultValue = treatNullAsDefaultValue;
		this.coveringSIFTIndexes = coveringSIFTIndexes;
		this.fixedParentName = fixedParentName;
		this.sqlSortingProperties = sqlSortingProperties;
		ShouldAliasQualifyFields = shouldAliasQualifyFields;
	}

	public static NavSqlWhereClauseVisitorInputOutput CreateForTextualWhereClauseGeneration(StringBuilder whereClause, NavSqlParameterHandler sqlParameterHandling, Dictionary<NCLMetaQueryDataItem, NCLMetaKey> coveringSiftIndexes, bool treatNullAsDefaultValue, string fixedParentName, SqlSortingProperties sqlSortingProperties, bool shouldAliasQualifyFields = false)
	{
		if (whereClause == null)
		{
			throw new ArgumentNullException("whereClause");
		}
		return new NavSqlWhereClauseVisitorInputOutput(createTextualWhereClause: true, null, whereClause, sqlParameterHandling, coveringSiftIndexes, treatNullAsDefaultValue, fixedParentName, sqlSortingProperties, shouldAliasQualifyFields);
	}

	public static NavSqlWhereClauseVisitorInputOutput CreateForParameterAssignment(SqlParameterCollection parameters, NavSqlParameterHandler sqlParameterHandling, Dictionary<NCLMetaQueryDataItem, NCLMetaKey> coveringSiftIndexes, bool treatNullAsDefaultValue, SqlSortingProperties sqlSortingProperties, bool shouldAliasQualifyFields = false)
	{
		if (parameters == null)
		{
			throw new ArgumentNullException("parameters");
		}
		return new NavSqlWhereClauseVisitorInputOutput(createTextualWhereClause: false, parameters, null, sqlParameterHandling, coveringSiftIndexes, treatNullAsDefaultValue, string.Empty, sqlSortingProperties, shouldAliasQualifyFields);
	}

	public SqlParameter GetSqlParameter(int parameterNo)
	{
		return NavSqlStatementHelper.GetOrCreateSqlParameterNo(sqlParameters, parameterNo);
	}
}
