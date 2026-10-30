using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class BinaryOperatorOverloadResolutionResult
{
	public readonly ArrayBuilder<BinaryOperatorAnalysisResult> Results;

	public static readonly ObjectPool<BinaryOperatorOverloadResolutionResult> Pool = CreatePool();

	public BinaryOperatorAnalysisResult Best
	{
		get
		{
			BinaryOperatorAnalysisResult result = default;
			foreach (BinaryOperatorAnalysisResult result2 in Results)
			{
				if (result2.IsValid)
				{
					if (result.IsValid)
					{
						return default;
					}
					result = result2;
				}
			}
			return result;
		}
	}

	private BinaryOperatorOverloadResolutionResult()
	{
		Results = new ArrayBuilder<BinaryOperatorAnalysisResult>(10);
	}

	public bool AnyValid()
	{
		foreach (BinaryOperatorAnalysisResult result in Results)
		{
			if (result.IsValid)
			{
				return true;
			}
		}
		return false;
	}

	public int GetValidCount()
	{
		int num = 0;
		foreach (BinaryOperatorAnalysisResult result in Results)
		{
			if (result.IsValid)
			{
				num++;
			}
		}
		return num;
	}

	public static BinaryOperatorOverloadResolutionResult GetInstance()
	{
		return Pool.Allocate();
	}

	public void Free()
	{
		Results.Clear();
		Pool.Free(this);
	}

	private static ObjectPool<BinaryOperatorOverloadResolutionResult> CreatePool()
	{
		return new ObjectPool<BinaryOperatorOverloadResolutionResult>(() => new BinaryOperatorOverloadResolutionResult(), 10);
	}
}
