using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class UnaryOperatorOverloadResolutionResult
{
	public readonly ArrayBuilder<UnaryOperatorAnalysisResult> Results;

	public static readonly ObjectPool<UnaryOperatorOverloadResolutionResult> Pool = CreatePool();

	public UnaryOperatorAnalysisResult Best
	{
		get
		{
			UnaryOperatorAnalysisResult result = default;
			foreach (UnaryOperatorAnalysisResult result2 in Results)
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

	public UnaryOperatorOverloadResolutionResult()
	{
		Results = new ArrayBuilder<UnaryOperatorAnalysisResult>(10);
	}

	public bool AnyValid()
	{
		foreach (UnaryOperatorAnalysisResult result in Results)
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
		foreach (UnaryOperatorAnalysisResult result in Results)
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

	public static UnaryOperatorOverloadResolutionResult GetInstance()
	{
		return Pool.Allocate();
	}

	public void Free()
	{
		Results.Clear();
		Pool.Free(this);
	}

	private static ObjectPool<UnaryOperatorOverloadResolutionResult> CreatePool()
	{
		return new ObjectPool<UnaryOperatorOverloadResolutionResult>(() => new UnaryOperatorOverloadResolutionResult(), 10);
	}
}
