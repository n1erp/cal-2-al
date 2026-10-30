using System.Threading;

namespace Microsoft.CodeAnalysis;

internal class CommonModuleCompilationState
{
	private bool _frozen;

	internal bool Frozen => _frozen;

	internal void Freeze()
	{
		Interlocked.MemoryBarrier();
		_frozen = true;
	}
}
