using System;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

public struct QueryClauseInfo : IEquatable<QueryClauseInfo>
{
	private readonly SymbolInfo _castInfo;

	private readonly SymbolInfo _operationInfo;

	public SymbolInfo CastInfo => _castInfo;

	public SymbolInfo OperationInfo => _operationInfo;

	internal QueryClauseInfo(SymbolInfo castInfo, SymbolInfo operationInfo)
	{
		_castInfo = castInfo;
		_operationInfo = operationInfo;
	}

	public override bool Equals(object obj)
	{
		if (obj is QueryClauseInfo)
		{
			return Equals((QueryClauseInfo)obj);
		}
		return false;
	}

	public bool Equals(QueryClauseInfo other)
	{
		if (_castInfo.Equals(other._castInfo))
		{
			return _operationInfo.Equals(other._operationInfo);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(CastInfo.GetHashCode(), _operationInfo.GetHashCode());
	}
}
