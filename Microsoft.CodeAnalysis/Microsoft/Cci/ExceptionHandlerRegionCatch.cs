using System.Reflection.Metadata;

namespace Microsoft.Cci;

internal sealed class ExceptionHandlerRegionCatch : ExceptionHandlerRegion
{
	private readonly ITypeReference _exceptionType;

	public override ExceptionRegionKind HandlerKind => ExceptionRegionKind.Catch;

	public override ITypeReference ExceptionType => _exceptionType;

	public ExceptionHandlerRegionCatch(int tryStartOffset, int tryEndOffset, int handlerStartOffset, int handlerEndOffset, ITypeReference exceptionType)
		: base(tryStartOffset, tryEndOffset, handlerStartOffset, handlerEndOffset)
	{
		_exceptionType = exceptionType;
	}
}
