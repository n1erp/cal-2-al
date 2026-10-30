namespace Microsoft.CodeAnalysis.Debugging;

internal struct StateMachineHoistedLocalScope(int startOffset, int endOffset)
{
	public readonly int StartOffset = startOffset;

	public readonly int EndOffset = endOffset;

	public int Length => EndOffset - StartOffset;

	public bool IsDefault
	{
		get
		{
			if (StartOffset == 0)
			{
				return EndOffset == 0;
			}
			return false;
		}
	}
}
