using System.Reflection.Metadata;

namespace Microsoft.Cci;

internal abstract class ExceptionHandlerRegion
{
	public int TryStartOffset { get; }

	public int TryEndOffset { get; }

	public int HandlerStartOffset { get; }

	public int HandlerEndOffset { get; }

	public int HandlerLength => HandlerEndOffset - HandlerStartOffset;

	public int TryLength => TryEndOffset - TryStartOffset;

	public abstract ExceptionRegionKind HandlerKind { get; }

	public virtual ITypeReference ExceptionType => null;

	public virtual int FilterDecisionStartOffset => 0;

	public ExceptionHandlerRegion(int tryStartOffset, int tryEndOffset, int handlerStartOffset, int handlerEndOffset)
	{
		TryStartOffset = tryStartOffset;
		TryEndOffset = tryEndOffset;
		HandlerStartOffset = handlerStartOffset;
		HandlerEndOffset = handlerEndOffset;
	}
}
