using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.Emit;

internal abstract class StateMachineMoveNextBodyDebugInfo
{
	public readonly IMethodDefinition KickoffMethod;

	public StateMachineMoveNextBodyDebugInfo(IMethodDefinition kickoffMethod)
	{
		KickoffMethod = kickoffMethod;
	}
}
