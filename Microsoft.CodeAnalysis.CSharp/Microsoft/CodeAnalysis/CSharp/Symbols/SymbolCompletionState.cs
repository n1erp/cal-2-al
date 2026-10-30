using System.Text;
using System.Threading;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal struct SymbolCompletionState
{
	private volatile int _completeParts;

	internal int IncompleteParts => ~_completeParts & 0x1FFFF;

	internal CompletionPart NextIncompletePart
	{
		get
		{
			int incompleteParts = IncompleteParts;
			return (CompletionPart)(incompleteParts & ~(incompleteParts - 1));
		}
	}

	internal void DefaultForceComplete(Symbol symbol)
	{
		if (!HasComplete(CompletionPart.Attributes))
		{
			symbol.GetAttributes();
		}
		NotePartComplete(CompletionPart.All);
	}

	internal bool HasComplete(CompletionPart part)
	{
		return ((uint)_completeParts & (uint)part) == (uint)part;
	}

	internal bool NotePartComplete(CompletionPart part)
	{
		return ThreadSafeFlagOperations.Set(ref _completeParts, (int)part);
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
			if ((num2 & _completeParts) != 0)
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
