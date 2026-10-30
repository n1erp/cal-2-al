using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.Emit;

internal sealed class IteratorMoveNextBodyDebugInfo : StateMachineMoveNextBodyDebugInfo
{
	public IteratorMoveNextBodyDebugInfo(IMethodDefinition kickoffMethod)
		: base(kickoffMethod)
	{
	}
}
