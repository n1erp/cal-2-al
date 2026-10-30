using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class DebuggerCallStackDataProvider : EagerVirtualDataProvider
{
	public static readonly DebuggerCallStackDataProvider Instance = new DebuggerCallStackDataProvider();

	public override int TableId => 2000000101;

	private DebuggerCallStackDataProvider()
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetAllItems(out bool isOrderedByPrimaryKeyAscending)
	{
		isOrderedByPrimaryKeyAscending = true;
		ALDebugger instance = ALDebugger.Instance;
		if (instance == null)
		{
			return new ReadOnlyRecordBuffer[0];
		}
		StackFrame[] currentCallStack = instance.GetCurrentCallStack();
		ReadOnlyRecordBuffer[] array = new ReadOnlyRecordBuffer[currentCallStack.Length];
		for (int i = 0; i < currentCallStack.Length; i++)
		{
			StackFrame stackFrame = currentCallStack[i];
			array[i] = new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, NavInteger.Create(i + 1), GetOptionValue(3, (int)stackFrame.ObjectId.ObjectType), NavInteger.Create(stackFrame.ObjectId.ObjectNumber), NavInteger.Create(stackFrame.StatementSpan.From.Line), GetTruncatedTextValue(9, stackFrame.MethodName), GetTruncatedTextValue(11, stackFrame.ObjectName));
		}
		return array;
	}
}
