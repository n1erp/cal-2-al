using System;

namespace Mono.Cecil.Cil;

public sealed class StateMachineScopeDebugInformation : CustomDebugInformation
{
	internal InstructionOffset start;

	internal InstructionOffset end;

	public static Guid KindIdentifier = new Guid("{6DA9A61E-F8C7-4874-BE62-68BC5630DF71}");

	public InstructionOffset Start
	{
		get
		{
			return start;
		}
		set
		{
			start = value;
		}
	}

	public InstructionOffset End
	{
		get
		{
			return end;
		}
		set
		{
			end = value;
		}
	}

	public override CustomDebugInformationKind Kind => CustomDebugInformationKind.StateMachineScope;

	internal StateMachineScopeDebugInformation(int start, int end)
		: base(KindIdentifier)
	{
		this.start = new InstructionOffset(start);
		this.end = new InstructionOffset(end);
	}

	public StateMachineScopeDebugInformation(Instruction start, Instruction end)
		: base(KindIdentifier)
	{
		this.start = new InstructionOffset(start);
		this.end = ((end != null) ? new InstructionOffset(end) : default(InstructionOffset));
	}
}
