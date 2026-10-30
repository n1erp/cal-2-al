using System;

namespace Microsoft.Dynamics.Nav.Model.Tools;

public interface ILogger
{
	void WriteLine(string message, MessageType type = MessageType.Normal);

	void InvokeParallelProcessing(string activity, int noOfItems, Action<IProgressLogger> action);
}
