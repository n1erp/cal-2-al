using System.Reflection.Metadata;

namespace Microsoft.Cci;

internal sealed class ExceptionHandlerRegionFinally : ExceptionHandlerRegion
{
	public override ExceptionRegionKind HandlerKind => ExceptionRegionKind.Finally;

	public ExceptionHandlerRegionFinally(int tryStartOffset, int tryEndOffset, int handlerStartOffset, int handlerEndOffset)
		: base(tryStartOffset, tryEndOffset, handlerStartOffset, handlerEndOffset)
	{
	}
}
