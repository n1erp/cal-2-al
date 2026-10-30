using System.Reflection.Metadata;

namespace Microsoft.Cci;

internal sealed class ExceptionHandlerRegionFault : ExceptionHandlerRegion
{
	public override ExceptionRegionKind HandlerKind => ExceptionRegionKind.Fault;

	public ExceptionHandlerRegionFault(int tryStartOffset, int tryEndOffset, int handlerStartOffset, int handlerEndOffset)
		: base(tryStartOffset, tryEndOffset, handlerStartOffset, handlerEndOffset)
	{
	}
}
