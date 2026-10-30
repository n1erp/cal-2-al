using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class DebuggerWatchValueDataProvider : RangeBasedComputedDataProvider
{
	public static readonly DebuggerWatchValueDataProvider Instance = new DebuggerWatchValueDataProvider();

	public override int TableId => 2000000103;

	private DebuggerWatchValueDataProvider()
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetValuesWithinRangeForKeyField(NCLMetaField keyField, ReadOnlyRecordBuffer precedingKeyFieldValues, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		return keyField.FieldNo switch
		{
			1 => EnumerateStackFrames(range, sortOrder), 
			3 => EnumerateWatchValuesForStackFrame((NavInteger)precedingKeyFieldValues[1], range, sortOrder, nonPrimaryKeyFilters), 
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

	private IEnumerable<ReadOnlyRecordBuffer> EnumerateWatchValuesForStackFrame(NavInteger stackFrameId, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		ALDebugger instance = ALDebugger.Instance;
		if (instance == null)
		{
			yield break;
		}
		List<LocalNode> nodes = instance.GetWatchesForStackFrame(stackFrameId.ToInt32()).ToList();
		if (!range.GetInclusiveIntegerBounds(1, nodes.Count, out var low, out var high))
		{
			yield break;
		}
		low--;
		high--;
		for (int i = ((sortOrder == SortOrder.Ascending) ? low : high); i >= low && i <= high; i += ((sortOrder == SortOrder.Ascending) ? 1 : (-1)))
		{
			LocalNode localNode = nodes[i];
			ReadOnlyRecordBuffer readOnlyRecordBuffer = new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, stackFrameId, NavInteger.Create(i + 1), GetTruncatedTextValue(7, localNode.Name), GetTruncatedTextValue(9, localNode.Summary), GetTruncatedTextValue(11, localNode.TypeName), NavBoolean.Create(localNode.Summary != "<Out Of Scope>"));
			if (readOnlyRecordBuffer.PassesFieldFilters(nonPrimaryKeyFilters))
			{
				yield return readOnlyRecordBuffer;
			}
		}
	}
}
