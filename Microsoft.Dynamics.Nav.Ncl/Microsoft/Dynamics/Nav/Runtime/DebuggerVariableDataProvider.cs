using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class DebuggerVariableDataProvider : RangeBasedComputedDataProvider
{
	public static readonly DebuggerVariableDataProvider Instance = new DebuggerVariableDataProvider();

	public override int TableId => 2000000102;

	private DebuggerVariableDataProvider()
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetValuesWithinRangeForKeyField(NCLMetaField keyField, ReadOnlyRecordBuffer precedingKeyFieldValues, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		return keyField.FieldNo switch
		{
			1 => EnumerateStackFrames(range, sortOrder), 
			3 => EnumerateVariablesInStackFrame((NavInteger)precedingKeyFieldValues[1], range, sortOrder, nonPrimaryKeyFilters), 
			_ => throw new NotSupportedException(), 
		};
	}

	private IEnumerable<ReadOnlyRecordBuffer> EnumerateStackFrames(Range range, SortOrder sortOrder)
	{
		ALDebugger instance = ALDebugger.Instance;
		if (instance == null)
		{
			yield break;
		}
		StackFrame[] currentCallStack = instance.GetCurrentCallStack();
		if (range.GetInclusiveIntegerBounds(1, currentCallStack.Length, out var low, out var high))
		{
			low--;
			high--;
			for (int i = ((sortOrder == SortOrder.Ascending) ? low : high); i >= low && i <= high; i += ((sortOrder == SortOrder.Ascending) ? 1 : (-1)))
			{
				yield return new ReadOnlyRecordBuffer(VirtualDataProvider.VirtualTimeStamp, NavInteger.Create(i + 1));
			}
		}
	}

	private IEnumerable<ReadOnlyRecordBuffer> EnumerateVariablesInStackFrame(NavInteger stackFrameNumber, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		ALDebugger instance = ALDebugger.Instance;
		if (instance == null)
		{
			yield break;
		}
		List<LocalNodeContext> nodes = instance.GetVariablesForStackFrame(stackFrameNumber.ToInt32()).ToList();
		if (!range.GetInclusiveIntegerBounds(1, nodes.Count, out var low, out var high))
		{
			yield break;
		}
		low--;
		high--;
		for (int i = ((sortOrder == SortOrder.Ascending) ? low : high); i >= low && i <= high; i += ((sortOrder == SortOrder.Ascending) ? 1 : (-1)))
		{
			LocalNodeContext localNodeContext = nodes[i];
			ReadOnlyRecordBuffer readOnlyRecordBuffer = new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, stackFrameNumber, NavInteger.Create(i + 1), NavInteger.Create(localNodeContext.Index), GetTruncatedTextValue(7, localNodeContext.Path), GetTruncatedTextValue(9, localNodeContext.Node.Name), GetTruncatedTextValue(11, localNodeContext.Node.Summary), GetTruncatedTextValue(13, localNodeContext.Node.TypeName));
			if (readOnlyRecordBuffer.PassesFieldFilters(nonPrimaryKeyFilters))
			{
				yield return readOnlyRecordBuffer;
			}
		}
	}
}
