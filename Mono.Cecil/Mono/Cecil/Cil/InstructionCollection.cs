using Mono.Collections.Generic;

namespace Mono.Cecil.Cil;

internal class InstructionCollection : Collection<Instruction>
{
	private readonly MethodDefinition method;

	internal InstructionCollection(MethodDefinition method)
	{
		this.method = method;
	}

	internal InstructionCollection(MethodDefinition method, int capacity)
		: base(capacity)
	{
		this.method = method;
	}

	protected override void OnAdd(Instruction item, int index)
	{
		if (index != 0)
		{
			Instruction instruction = items[index - 1];
			instruction.next = item;
			item.previous = instruction;
		}
	}

	protected override void OnInsert(Instruction item, int index)
	{
		if (size == 0)
		{
			return;
		}
		Instruction instruction = items[index];
		if (instruction == null)
		{
			Instruction instruction2 = items[index - 1];
			instruction2.next = item;
			item.previous = instruction2;
			return;
		}
		Instruction previous = instruction.previous;
		if (previous != null)
		{
			previous.next = item;
			item.previous = previous;
		}
		instruction.previous = item;
		item.next = instruction;
	}

	protected override void OnSet(Instruction item, int index)
	{
		Instruction instruction = items[index];
		item.previous = instruction.previous;
		item.next = instruction.next;
		instruction.previous = null;
		instruction.next = null;
	}

	protected override void OnRemove(Instruction item, int index)
	{
		Instruction previous = item.previous;
		if (previous != null)
		{
			previous.next = item.next;
		}
		Instruction next = item.next;
		if (next != null)
		{
			next.previous = item.previous;
		}
		RemoveSequencePoint(item);
		item.previous = null;
		item.next = null;
	}

	private void RemoveSequencePoint(Instruction instruction)
	{
		MethodDebugInformation debug_info = method.debug_info;
		if (debug_info == null || !debug_info.HasSequencePoints)
		{
			return;
		}
		Collection<SequencePoint> sequence_points = debug_info.sequence_points;
		for (int i = 0; i < sequence_points.Count; i++)
		{
			if (sequence_points[i].Offset == instruction.offset)
			{
				sequence_points.RemoveAt(i);
				break;
			}
		}
	}
}
