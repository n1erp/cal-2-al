using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NavSqlMissingTableSyncSetupValidator
{
	private const string SeparatorChar = ",";

	private const string DetailsSeparator = " - ";

	public const string SetTableSyncSetupTemplate = "DataUpgradeMgt.SetTableSyncSetup(DATABASE::\"{0}\",[Upgrade Table Id],TableSynchSetup.Mode::[Mode]);";

	public static void ValidateNoSyncSetupErrors(IEnumerable<NavSqlTableAndChange> tableChangesAffectingSqlSchema)
	{
		if (tableChangesAffectingSqlSchema == null || !tableChangesAffectingSqlSchema.Any())
		{
			return;
		}
		IEnumerable<NavSqlTableAndChange> source = tableChangesAffectingSqlSchema.Where((NavSqlTableAndChange tableAndChange) => tableAndChange.ChangeCanCauseDataLoss && tableAndChange.NavTableSyncSetup == null);
		if (!source.Any((NavSqlTableAndChange change) => change.OldTable.TableId != 2000000079 && change.OldTable.TableId != 2000000173))
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (NavSqlTableAndChange item in source.Where((NavSqlTableAndChange change) => change.OldTable.TableId != 2000000079 && change.OldTable.TableId != 2000000173))
		{
			stringBuilder.AppendLine(CreateDetailsTableRow(item));
			stringBuilder.AppendLine(Environment.NewLine);
		}
		string message = string.Format(CultureInfo.CurrentCulture, Lang.MissingTableSynchSetup, stringBuilder.ToString().TrimEnd(), CreateTableSyncSetup(source.Select((NavSqlTableAndChange table) => table.OldTable.TableName)));
		throw new NavMissingSynchOptionException(message);
	}

	public static string CreateDetailsTableRow(NavSqlTableAndChange tableAndChange)
	{
		if (tableAndChange == null || tableAndChange.OldTable == null)
		{
			return string.Empty;
		}
		if (!tableAndChange.Excessive && tableAndChange.FieldsAndChanges == null)
		{
			return string.Empty;
		}
		if (tableAndChange.Excessive)
		{
			return string.Format(CultureInfo.CurrentCulture, Lang.TableDeletedRepresentation, tableAndChange.OldTable.TableId, tableAndChange.OldTable.TableName);
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(string.Format(CultureInfo.CurrentCulture, Lang.TableRepresentation, tableAndChange.OldTable.TableId, tableAndChange.OldTable.TableName));
		IEnumerable<NavSqlFieldAndChanges> enumerable = tableAndChange.FieldsAndChanges.Where((NavSqlFieldAndChanges field) => field.IsUnsafeChange);
		foreach (NavSqlFieldAndChanges item in enumerable)
		{
			stringBuilder.AppendLine(CreateDetailsField(item));
		}
		IEnumerable<NavSqlIndexAndChanges> enumerable2 = tableAndChange.IndexesAndChanges.Where((NavSqlIndexAndChanges index) => index.IsPotentialUniqueConstraintViolation);
		foreach (NavSqlIndexAndChanges item2 in enumerable2)
		{
			stringBuilder.AppendLine(CreateDetailsIndex(item2));
		}
		return stringBuilder.ToString().TrimEnd();
	}

	public static string CreateDetailsField(NavSqlFieldAndChanges fieldAndChanges)
	{
		if (fieldAndChanges == null)
		{
			return string.Empty;
		}
		return string.Format(CultureInfo.CurrentCulture, Lang.FieldRepresentation, fieldAndChanges.OldField.FieldNo, fieldAndChanges.OldField.FieldName, CreateDescriptionColumn(fieldAndChanges));
	}

	private static string CreateDetailsIndex(NavSqlIndexAndChanges indexChanged)
	{
		if (indexChanged == null)
		{
			return string.Empty;
		}
		return string.Format(CultureInfo.CurrentCulture, Lang.KeyRepresentation, string.IsNullOrEmpty(indexChanged.NewKey.KeyName) ? indexChanged.NewKey.KeyIndex.ToString(CultureInfo.CurrentCulture) : indexChanged.NewKey.KeyName, Lang.SyncErrorKeyUniqueChanged);
	}

	public static string CreateDescriptionColumn(NavSqlFieldAndChanges fieldAndChanges)
	{
		if (fieldAndChanges == null)
		{
			return string.Empty;
		}
		if (fieldAndChanges.Excessive)
		{
			return Lang.SyncErrorFieldDeleted;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (fieldAndChanges.LengthChanged && fieldAndChanges.OldField.FieldDefinedLength > fieldAndChanges.NewField.FieldDefinedLength)
		{
			stringBuilder.Append(Lang.SyncErrorFieldReducedLength);
		}
		if (fieldAndChanges.OldField != null && fieldAndChanges.NewField != null && fieldAndChanges.OldField.FieldIsPartOfPrimaryKey && !fieldAndChanges.NewField.FieldIsPartOfPrimaryKey)
		{
			stringBuilder.Append((stringBuilder.Length > 0) ? " - " : string.Empty);
			stringBuilder.Append(Lang.SyncErrorFieldRemovedFromKey);
		}
		if (fieldAndChanges.SqlDataTypeChanged)
		{
			stringBuilder.Append((stringBuilder.Length > 0) ? " - " : string.Empty);
			stringBuilder.Append(Lang.SyncErrorFieldSqlDataTypeChanged);
		}
		if (fieldAndChanges.DataTypeChanged)
		{
			stringBuilder.Append((stringBuilder.Length > 0) ? " - " : string.Empty);
			stringBuilder.Append(Lang.SyncErrorFieldDataTypeChanged);
		}
		return stringBuilder.ToString();
	}

	public static string CreateTableSyncSetup(IEnumerable<string> tableNames)
	{
		if (tableNames == null || !tableNames.Any())
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string tableName in tableNames)
		{
			stringBuilder.AppendLine(string.Format(CultureInfo.CurrentCulture, "DataUpgradeMgt.SetTableSyncSetup(DATABASE::\"{0}\",[Upgrade Table Id],TableSynchSetup.Mode::[Mode]);", tableName));
		}
		return stringBuilder.ToString().TrimEnd();
	}
}
