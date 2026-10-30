using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.CodeGen;

internal class LocalDefUseInfo
{
	private readonly ObjectPool<LocalDefUseInfo> _pool;

	private static readonly ObjectPool<LocalDefUseInfo> s_poolInstance = CreatePool();

	private ArrayBuilder<LocalDefUseSpan> _localDefs;

	public int StackAtDeclaration { get; private set; }

	public ArrayBuilder<LocalDefUseSpan> LocalDefs
	{
		get
		{
			ArrayBuilder<LocalDefUseSpan> arrayBuilder = _localDefs;
			if (arrayBuilder == null)
			{
				arrayBuilder = (_localDefs = ArrayBuilder<LocalDefUseSpan>.GetInstance());
			}
			return arrayBuilder;
		}
	}

	public bool CannotSchedule { get; private set; }

	public void ShouldNotSchedule()
	{
		CannotSchedule = true;
	}

	private LocalDefUseInfo(ObjectPool<LocalDefUseInfo> pool)
	{
		_pool = pool;
	}

	public void Free()
	{
		if (_localDefs != null)
		{
			_localDefs.Free();
			_localDefs = null;
		}
		_pool?.Free(this);
	}

	public static ObjectPool<LocalDefUseInfo> CreatePool()
	{
		ObjectPool<LocalDefUseInfo> pool = null;
		pool = new ObjectPool<LocalDefUseInfo>(() => new LocalDefUseInfo(pool), 128);
		return pool;
	}

	public static LocalDefUseInfo GetInstance(int stackAtDeclaration)
	{
		LocalDefUseInfo localDefUseInfo = s_poolInstance.Allocate();
		localDefUseInfo.StackAtDeclaration = stackAtDeclaration;
		localDefUseInfo.CannotSchedule = false;
		return localDefUseInfo;
	}
}
