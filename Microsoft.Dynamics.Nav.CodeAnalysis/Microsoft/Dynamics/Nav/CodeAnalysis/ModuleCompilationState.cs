using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class ModuleCompilationState
{
	internal bool Frozen { get; private set; }

	internal void Freeze()
	{
		DebugAssertHelper.Assert(!Frozen);
		Interlocked.MemoryBarrier();
		Frozen = true;
	}
}
