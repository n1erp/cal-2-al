using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

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

	public bool SingleValid()
	{
		bool flag = false;
		foreach (BinaryOperatorAnalysisResult result in Results)
		{
			if (result.IsValid)
			{
				if (flag)
				{
					return false;
				}
				flag = true;
			}
		}
		return flag;
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
