using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Text;

internal sealed class ChangedText : SourceText
{
	private readonly SourceText oldText;

	private readonly SourceText newText;

	private readonly ImmutableArray<TextChangeRange> changes;

	public override Encoding Encoding => oldText.Encoding;

	public SourceText OldText => oldText;

	public SourceText NewText => newText;

	public IEnumerable<TextChangeRange> Changes => changes;

	public override int Length => newText.Length;

	public override char this[int position] => newText[position];

	public ChangedText(SourceText oldText, ImmutableArray<TextChangeRange> changeRanges, ImmutableArray<SourceText> segments)
	{
		DebugAssertHelper.Assert(oldText != null);
		DebugAssertHelper.Assert(!changeRanges.IsDefault);
		DebugAssertHelper.Assert(!segments.IsDefault);
		this.oldText = oldText;
		newText = (segments.IsEmpty ? ((SourceText)new StringText(string.Empty, oldText.Encoding)) : ((SourceText)new CompositeText(segments)));
		changes = changeRanges;
	}

	public override string ToString(TextSpan span)
	{
		return newText.ToString(span);
	}

	public override SourceText GetSubText(TextSpan span)
	{
		return newText.GetSubText(span);
	}

	public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
	{
		newText.CopyTo(sourceIndex, destination, destinationIndex, count);
	}

	public override IReadOnlyList<TextChangeRange> GetChangeRanges(SourceText oldTextParameter)
	{
		if (oldTextParameter == null)
		{
			throw new ArgumentNullException("oldTextParameter");
		}
		if (oldText == oldTextParameter)
		{
			return changes;
		}
		if (oldText.GetChangeRanges(oldTextParameter).Count == 0)
		{
			return changes;
		}
		if (this == oldTextParameter)
		{
			return TextChangeRange.NoChanges;
		}
		return ImmutableArray.Create(new TextChangeRange(new TextSpan(0, oldTextParameter.Length), newText.Length));
	}

	protected override TextLineCollection GetLinesCore()
	{
		TextLineCollection lines = oldText.Lines;
		ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance();
		instance.Add(0);
		int num = 0;
		int num2 = 0;
		bool flag = false;
		foreach (TextChangeRange change in changes)
		{
			if (change.Span.Start > num)
			{
				if (flag && newText[num + num2] == '\n')
				{
					instance.RemoveLast();
				}
				LinePositionSpan linePositionSpan = lines.GetLinePositionSpan(TextSpan.FromBounds(num, change.Span.Start));
				for (int i = linePositionSpan.Start.Line + 1; i <= linePositionSpan.End.Line; i++)
				{
					instance.Add(lines[i].Start + num2);
				}
				flag = oldText[change.Span.Start - 1] == '\r';
				if (flag && change.Span.Start < oldText.Length && oldText[change.Span.Start] == '\n')
				{
					instance.Add(change.Span.Start + num2);
				}
			}
			if (change.NewLength > 0)
			{
				SourceText subText = GetSubText(new TextSpan(change.Span.Start + num2, change.NewLength));
				int num3 = 0;
				while (num3 < subText.Length)
				{
					char c = subText[num3++];
					if ((uint)(c - 14) > 113u)
					{
						if (flag && c == '\n')
						{
							instance.RemoveLast();
						}
						else if (c == '\r' && num3 < subText.Length && subText[num3] == '\n')
						{
							num3++;
						}
						else if (!TextUtilities.IsAnyLineBreakCharacter(c))
						{
							continue;
						}
						instance.Add(change.Span.Start + num2 + num3);
					}
				}
				flag = subText[change.NewLength - 1] == '\r';
			}
			num = change.Span.End;
			num2 += change.NewLength - change.Span.Length;
		}
		if (num < oldText.Length)
		{
			if (flag && newText[num + num2] == '\n')
			{
				instance.RemoveLast();
			}
			LinePositionSpan linePositionSpan2 = lines.GetLinePositionSpan(TextSpan.FromBounds(num, oldText.Length));
			for (int j = linePositionSpan2.Start.Line + 1; j <= linePositionSpan2.End.Line; j++)
			{
				instance.Add(lines[j].Start + num2);
			}
		}
		return new LineInfo(this, instance.ToArrayAndFree());
	}
}
