using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class CodeCoverageScopeCounter
{
	private const int InitialSize = 8;

	private readonly int scopeId;

	private uint[] statementHitCounts;

	public int ScopeId => scopeId;

	public uint[] StatementHitCounts => statementHitCounts;

	public CodeCoverageScopeCounter(int scopeId)
	{
		this.scopeId = scopeId;
		statementHitCounts = new uint[8];
	}

	public void IncrementHitCount(int statementNumber, uint amount)
	{
		if (statementNumber >= statementHitCounts.Length)
		{
			int num = statementHitCounts.Length;
			do
			{
				num <<= 1;
			}
			while (num <= statementNumber);
			uint[] dst = new uint[num];
			Buffer.BlockCopy(statementHitCounts, 0, dst, 0, statementHitCounts.Length * 4);
			statementHitCounts = dst;
		}
		statementHitCounts[statementNumber] += amount;
	}

	public void SetHitCount(int statmentNumber, uint hitCount)
	{
		if (statmentNumber >= statementHitCounts.Length)
		{
			if (hitCount == 0)
			{
				return;
			}
			IncrementHitCount(statmentNumber, 0u);
		}
		statementHitCounts[statmentNumber] = hitCount;
	}

	public void IncrementHitCount(int statementNumber)
	{
		IncrementHitCount(statementNumber, 1u);
	}

	public void DestructiveBulkAddHitCounts(uint[] additionalStatementCounts)
	{
		uint[] array;
		uint[] array2;
		if (statementHitCounts.Length > additionalStatementCounts.Length)
		{
			array = additionalStatementCounts;
			array2 = statementHitCounts;
		}
		else
		{
			array = statementHitCounts;
			array2 = (statementHitCounts = additionalStatementCounts);
		}
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] += array[i];
		}
	}

	public CodeCoverageScopeCounter Clone()
	{
		uint[] dst = new uint[statementHitCounts.Length];
		Buffer.BlockCopy(statementHitCounts, 0, dst, 0, statementHitCounts.Length * 4);
		return new CodeCoverageScopeCounter(scopeId)
		{
			statementHitCounts = dst
		};
	}
}
