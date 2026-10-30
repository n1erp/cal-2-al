using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavRecordIdEvaluator : NavValueEvaluator<NavRecordId>
{
	private static readonly NavRecordIdEvaluator instance = new NavRecordIdEvaluator();

	internal static NavRecordIdEvaluator Instance => instance;

	private NavRecordIdEvaluator()
	{
	}

	protected override bool InternalEvaluate(DataError dataError, out NavRecordId value, INavValueMetadata metadata, string source, int number)
	{
		if (string.IsNullOrWhiteSpace(source))
		{
			value = NavRecordId.Default;
			return true;
		}
		int position = 0;
		bool flag = source[0] == '\'' && source[source.Length - 1] == '\'';
		if (!NavValueEvaluator.TryReadTokenUntilDelimiter(source, ref position, ':', delimiterRequired: false, out var result))
		{
			throw NavNCLEvaluateException.Create(CultureInfo.CurrentCulture, NavType.RecordID, source);
		}
		int num = GetTableIdentifierFromTableName(result);
		if (num <= 0)
		{
			num = NavValueEvaluator.EvaluateTableIdentifierString(result, flag);
			if (num <= 0)
			{
				throw NavNCLEvaluateException.Create(CultureInfo.CurrentCulture, NavType.RecordID, source);
			}
		}
		if (!NavGlobal.NCLMetadata.TryGetMetaTableById(num, out var metaTable, -1, requireCompiled: false))
		{
			byte[] array = new byte[NavRecordId.Default.GetBytesSize];
			BitConverter.GetBytes(num).CopyTo(array, 0);
			value = new NavRecordId(array);
			return true;
		}
		List<NavValue> list = new List<NavValue>();
		ReadOnlyCollection<NCLMetaField> keyFieldsList = metaTable.PrimaryKey.KeyFieldsList;
		int num2 = 0;
		NavValueEvaluator.ConsumeWhitespace(source, ref position);
		while (position < source.Length)
		{
			if (NavValueEvaluator.TryReadTokenUntilDelimiter(source, ref position, ',', delimiterRequired: false, out var result2))
			{
				if (num2 >= keyFieldsList.Count)
				{
					throw NavNCLEvaluateException.Create(CultureInfo.CurrentCulture, NavType.RecordID, source);
				}
				int length = result2.Length;
				if (flag && position == source.Length && length >= 1 && result2[length - 1] == '\'')
				{
					result2 = result2.Substring(0, length - 1);
				}
				list.Add(ALSystemVariable.EvaluateIntoNavValue(DataError.ThrowError, keyFieldsList[num2++], result2, number));
				continue;
			}
			throw NavNCLEvaluateException.Create(CultureInfo.CurrentCulture, NavType.RecordID, source);
		}
		value = new NavRecordId(num, list.ToArray());
		return true;
	}

	protected override NavRecordId GetDefaultValue(INavValueMetadata metadata)
	{
		return NavRecordId.Default;
	}

	protected static int GetTableIdentifierFromTableName(string tableName)
	{
		if (string.IsNullOrEmpty(tableName))
		{
			return -1;
		}
		NavInteger fromValue = NavInteger.Create(1);
		NavText fromValue2 = NavText.Create(tableName);
		using (NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000038))
		{
			navRecord.ALSetRange(1, fromValue);
			navRecord.ALSetRange(4, fromValue2);
			if (navRecord.ALFindFirst(DataError.TrapError))
			{
				return navRecord.GetFieldValue(3).ToInt32();
			}
		}
		return -1;
	}
}
