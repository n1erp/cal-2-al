using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlParameterHandler
{
	private int?[] valueTokenMap;

	private int nextParameterNo;

	private Dictionary<CompanyId, int> companyIdParameters = new Dictionary<CompanyId, int>(1);

	public int GetNextParameterNo => nextParameterNo++;

	public NavSqlParameterHandler(int nextParameterNo)
	{
		this.nextParameterNo = nextParameterNo;
	}

	public NavSqlParameterHandler()
	{
	}

	public int GetParameterNoFromValueToken(int valueToken)
	{
		if (valueToken == -1)
		{
			return GetNextParameterNo;
		}
		int num = ((valueTokenMap != null) ? valueTokenMap.Length : 0);
		if (num <= valueToken)
		{
			int?[] destinationArray = new int?[valueToken + 28];
			if (num > 0)
			{
				Array.Copy(valueTokenMap, destinationArray, num);
			}
			valueTokenMap = destinationArray;
		}
		if (!valueTokenMap[valueToken].HasValue)
		{
			int getNextParameterNo = GetNextParameterNo;
			valueTokenMap[valueToken] = getNextParameterNo;
			return getNextParameterNo;
		}
		return valueTokenMap[valueToken].Value;
	}

	public int GetParameterNoForEmptyValue(INavFieldMetadata fieldMetadata)
	{
		if (fieldMetadata is NCLMetaQueryColumn nCLMetaQueryColumn && nCLMetaQueryColumn.SourceColumn.AggregationType == AggregationType.Count)
		{
			return GetParameterNoFromValueToken(3);
		}
		switch (fieldMetadata.SqlDataType)
		{
		case SqlDataType.Default:
			return GetParameterNoFromValueToken((int)fieldMetadata.NclType);
		case SqlDataType.Timestamp:
			return GetParameterNoFromValueToken(26);
		case SqlDataType.Variant:
			return GetParameterNoFromValueToken(27);
		case SqlDataType.VarChar:
		{
			int valueToken = ((fieldMetadata.NclType == NavNclType.NavCode) ? 12 : 8);
			return GetParameterNoFromValueToken(valueToken);
		}
		case SqlDataType.BigInt:
			return GetParameterNoFromValueToken(4);
		case SqlDataType.Integer:
			return GetParameterNoFromValueToken(3);
		default:
			throw new ArgumentException(fieldMetadata.SqlDataType.ToString());
		}
	}

	public int GetCompanyParameterNo(CompanyId companyId)
	{
		if (!companyIdParameters.TryGetValue(companyId, out var value))
		{
			value = GetNextParameterNo;
			companyIdParameters.Add(companyId, value);
		}
		return value;
	}
}
