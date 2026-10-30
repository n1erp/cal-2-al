using System;
using Mono.Collections.Generic;

namespace Mono.Cecil.Cil;

public sealed class AsyncMethodBodyDebugInformation : CustomDebugInformation
{
	internal InstructionOffset catch_handler;

	internal Collection<InstructionOffset> yields;

	internal Collection<InstructionOffset> resumes;

	internal MethodDefinition move_next;

	public static Guid KindIdentifier = new Guid("{54FD2AC5-E925-401A-9C2A-F94F171072F8}");

	public InstructionOffset CatchHandler
	{
		get
		{
			return catch_handler;
		}
		set
		{
			catch_handler = value;
		}
	}

	public Collection<InstructionOffset> Yields => yields ?? (yields = new Collection<InstructionOffset>());

	public Collection<InstructionOffset> Resumes => resumes ?? (resumes = new Collection<InstructionOffset>());

	public MethodDefinition MoveNextMethod
	{
		get
		{
			return move_next;
		}
		set
		{
			move_next = value;
		}
	}

	public override CustomDebugInformationKind Kind => CustomDebugInformationKind.AsyncMethodBody;

	internal AsyncMethodBodyDebugInformation(int catchHandler)
		: base(KindIdentifier)
	{
		catch_handler = new InstructionOffset(catchHandler);
	}

	public AsyncMethodBodyDebugInformation(Instruction catchHandler)
		: base(KindIdentifier)
	{
		catch_handler = new InstructionOffset(catchHandler);
	}

	public AsyncMethodBodyDebugInformation()
		: base(KindIdentifier)
	{
		catch_handler = new InstructionOffset(-1);
	}
}
