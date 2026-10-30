using System.Collections.Immutable;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.Emit;

internal sealed class AsyncMoveNextBodyDebugInfo : StateMachineMoveNextBodyDebugInfo
{
	public readonly int CatchHandlerOffset;

	public readonly ImmutableArray<int> YieldOffsets;

	public readonly ImmutableArray<int> ResumeOffsets;

	public AsyncMoveNextBodyDebugInfo(IMethodDefinition kickoffMethod, int catchHandlerOffset, ImmutableArray<int> yieldOffsets, ImmutableArray<int> resumeOffsets)
		: base(kickoffMethod)
	{
		CatchHandlerOffset = catchHandlerOffset;
		YieldOffsets = yieldOffsets;
		ResumeOffsets = resumeOffsets;
	}
}
