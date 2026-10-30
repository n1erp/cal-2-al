using System.Text;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal struct SymbolCompletionState
{
	private volatile int completeParts;

	internal int IncompleteParts => ~completeParts & 0x1FFFF;

	internal CompletionPart NextIncompletePart
	{
		get
		{
			int incompleteParts = IncompleteParts;
			int num = incompleteParts & ~(incompleteParts - 1);
			DebugAssertHelper.Assert(HasAtMostOneBitSet(num), "ForceComplete won't handle the result correctly if more than one bit is set.");
			return (CompletionPart)num;
		}
	}

	internal void DefaultForceComplete(Symbol symbol)
	{
		DebugAssertHelper.Assert(symbol.RequiresCompletion);
		NotePartComplete(CompletionPart.All);
	}

	internal bool HasComplete(CompletionPart part)
	{
		return ((uint)completeParts & (uint)part) == (uint)part;
	}

	internal bool NotePartComplete(CompletionPart part)
	{
		return ThreadSafeFlagOperations.Set(ref completeParts, (int)part);
	}

	internal static bool HasAtMostOneBitSet(int bits)
	{
		return (bits & (bits - 1)) == 0;
	}

	internal void SpinWaitComplete(CompletionPart part, CancellationToken cancellationToken)
	{
		if (!HasComplete(part))
		{
			SpinWait spinWait = default;
			while (!HasComplete(part))
			{
				cancellationToken.ThrowIfCancellationRequested();
				spinWait.SpinOnce();
			}
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("CompletionParts(");
		bool flag = false;
		int num = 0;
		while (true)
		{
			int num2 = 1 << num;
			if ((num2 & 0x1FFFF) == 0)
			{
				break;
			}
			if ((num2 & completeParts) != 0)
			{
				if (flag)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(num);
				flag = true;
			}
			num++;
		}
		stringBuilder.Append(")");
		return stringBuilder.ToString();
	}
}
