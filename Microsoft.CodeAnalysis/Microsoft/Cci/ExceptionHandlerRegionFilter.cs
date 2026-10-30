using System.Reflection.Metadata;

namespace Microsoft.Cci;

internal sealed class ExceptionHandlerRegionFilter : ExceptionHandlerRegion
{
	private readonly int _filterDecisionStartOffset;

	public override ExceptionRegionKind HandlerKind => ExceptionRegionKind.Filter;

	public override int FilterDecisionStartOffset => _filterDecisionStartOffset;

	public ExceptionHandlerRegionFilter(int tryStartOffset, int tryEndOffset, int handlerStartOffset, int handlerEndOffset, int filterDecisionStartOffset)
		: base(tryStartOffset, tryEndOffset, handlerStartOffset, handlerEndOffset)
	{
		_filterDecisionStartOffset = filterDecisionStartOffset;
	}
}
