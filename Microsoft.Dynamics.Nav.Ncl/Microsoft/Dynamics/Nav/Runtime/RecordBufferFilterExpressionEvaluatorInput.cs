namespace Microsoft.Dynamics.Nav.Runtime;

internal class RecordBufferFilterExpressionEvaluatorInput
{
	internal IRecordBuffer RecordBuffer { get; private set; }

	internal bool CheckAgainstOriginalAndModified { get; private set; }

	internal bool BothShouldPass { get; private set; }

	internal RecordBufferFilterExpressionEvaluatorInput(IRecordBuffer recordBuffer, bool checkAgainstOriginalAndModified, bool bothShouldPass)
	{
		RecordBuffer = recordBuffer;
		CheckAgainstOriginalAndModified = checkAgainstOriginalAndModified;
		BothShouldPass = bothShouldPass;
	}
}
