using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class SidAccountIdDataProvider : ComputedVirtualDataProvider
{
	public static readonly SidAccountIdDataProvider Instance = new SidAccountIdDataProvider();

	public override int TableId => 2000000055;

	private SidAccountIdDataProvider()
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetValuesWithinFilterForKeyField(NCLMetaField keyField, ReadOnlyRecordBuffer precedingKeyFieldValues, FilterExpression filter, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		if (nonPrimaryKeyFilters != null && nonPrimaryKeyFilters.Items.Length == 1)
		{
			ReadOnlyRecordBuffer readOnlyRecordBuffer = RecordFromId(nonPrimaryKeyFilters);
			if (readOnlyRecordBuffer != null && readOnlyRecordBuffer.PassesFilter(filter))
			{
				yield return readOnlyRecordBuffer;
			}
		}
		else if (filter != null && filter.ExpressionType == FilterExpressionType.Equal)
		{
			ReadOnlyRecordBuffer readOnlyRecordBuffer2 = RecordFromSid(filter);
			if (readOnlyRecordBuffer2 != null && readOnlyRecordBuffer2.PassesFieldFilters(nonPrimaryKeyFilters))
			{
				yield return readOnlyRecordBuffer2;
			}
		}
	}

	private static string GetPatternFromFilter(FilterExpression filter)
	{
		string result = null;
		switch (filter.ExpressionType)
		{
		case FilterExpressionType.Equal:
			result = ((UnaryFilterExpression)filter).Value.ToString();
			break;
		case FilterExpressionType.Wildcard:
			if (filter is WildcardFilterExpression { IsCaseAndAccentInsensitive: not false, Pattern: var pattern })
			{
				result = pattern.Replace("*", "");
				result = result.Replace("?", "");
			}
			break;
		}
		return result;
	}

	private ReadOnlyRecordBuffer RecordFromSid(FilterExpression filter)
	{
		string patternFromFilter = GetPatternFromFilter(filter);
		if (string.IsNullOrEmpty(patternFromFilter))
		{
			return null;
		}
		string userAccountFromSid = GetUserAccountFromSid(patternFromFilter);
		if (string.IsNullOrEmpty(userAccountFromSid))
		{
			return null;
		}
		return CreateReadOnlyRecordBuffer(patternFromFilter, userAccountFromSid);
	}

	private static string GetUserAccountFromSid(string sidString)
	{
		try
		{
			SecurityIdentifier securityIdentifier = new SecurityIdentifier(sidString);
			return securityIdentifier.Translate(typeof(NTAccount)).Value;
		}
		catch (ArgumentException)
		{
			return null;
		}
		catch (IdentityNotMappedException)
		{
			return null;
		}
	}

	private ReadOnlyRecordBuffer RecordFromId(FilterFieldDictionary nonPrimaryKeyFilters)
	{
		string patternFromFilter = GetPatternFromFilter(nonPrimaryKeyFilters[MetaTable.GetFieldByNo(2)]);
		if (string.IsNullOrEmpty(patternFromFilter))
		{
			return null;
		}
		NTAccount nTAccount = new NTAccount(patternFromFilter);
		string text;
		try
		{
			SecurityIdentifier securityIdentifier = (SecurityIdentifier)nTAccount.Translate(typeof(SecurityIdentifier));
			text = securityIdentifier.ToString();
			patternFromFilter = GetUserAccountFromSid(text);
		}
		catch (IdentityNotMappedException)
		{
			return null;
		}
		return CreateReadOnlyRecordBuffer(text, patternFromFilter);
	}

	private ReadOnlyRecordBuffer CreateReadOnlyRecordBuffer(string sid, string userAccount)
	{
		return new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, NavText.Create(sid), NavText.Create(userAccount));
	}
}
