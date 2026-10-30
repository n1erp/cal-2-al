using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.Text;

internal sealed class ChangedText : SourceText
{
	private class ChangeInfo
	{
		public ImmutableArray<TextChangeRange> ChangeRanges { get; }

		public WeakReference<SourceText> WeakOldText { get; }

		public ChangeInfo Previous { get; private set; }

		public ChangeInfo(ImmutableArray<TextChangeRange> changeRanges, WeakReference<SourceText> weakOldText, ChangeInfo previous)
		{
			ChangeRanges = changeRanges;
			WeakOldText = weakOldText;
			Previous = previous;
			Clean();
		}

		private void Clean()
		{
			ChangeInfo changeInfo = this;
			for (ChangeInfo changeInfo2 = this; changeInfo2 != null; changeInfo2 = changeInfo2.Previous)
			{
				if (changeInfo2.WeakOldText.TryGetTarget(out var _))
				{
					changeInfo = changeInfo2;
				}
			}
			while (changeInfo != null)
			{
				ChangeInfo previous = changeInfo.Previous;
				changeInfo.Previous = null;
				changeInfo = previous;
			}
		}
	}

	private readonly SourceText _newText;

	private readonly ChangeInfo _info;

	public override Encoding Encoding => _newText.Encoding;

	public IEnumerable<TextChangeRange> Changes => _info.ChangeRanges;

	public override int Length => _newText.Length;

	internal override int StorageSize => _newText.StorageSize;

	internal override ImmutableArray<SourceText> Segments => _newText.Segments;

	internal override SourceText StorageKey => _newText.StorageKey;

	public override char this[int position] => _newText[position];

	public ChangedText(SourceText oldText, SourceText newText, ImmutableArray<TextChangeRange> changeRanges)
		: base(default, oldText.ChecksumAlgorithm)
	{
		_newText = newText;
		_info = new ChangeInfo(changeRanges, new WeakReference<SourceText>(oldText), (oldText as ChangedText)?._info);
	}

	public override string ToString(TextSpan span)
	{
		return _newText.ToString(span);
	}

	public override SourceText GetSubText(TextSpan span)
	{
		return _newText.GetSubText(span);
	}

	public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
	{
		_newText.CopyTo(sourceIndex, destination, destinationIndex, count);
	}

	public override SourceText WithChanges(IEnumerable<TextChange> changes)
	{
		if (_newText.WithChanges(changes) is ChangedText changedText)
		{
			return new ChangedText(this, changedText._newText, changedText._info.ChangeRanges);
		}
		return this;
	}

	public override IReadOnlyList<TextChangeRange> GetChangeRanges(SourceText oldText)
	{
		if (oldText == null)
		{
			throw new ArgumentNullException("oldText");
		}
		if (this == oldText)
		{
			return TextChangeRange.NoChanges;
		}
		if (_info.WeakOldText.TryGetTarget(out var target) && target == oldText)
		{
			return _info.ChangeRanges;
		}
		if (IsChangedFrom(oldText))
		{
			IReadOnlyList<ImmutableArray<TextChangeRange>> changesBetween = GetChangesBetween(oldText, this);
			if (changesBetween.Count > 1)
			{
				return Merge(changesBetween);
			}
		}
		if (target != null && target.GetChangeRanges(oldText).Count == 0)
		{
			return _info.ChangeRanges;
		}
		return ImmutableArray.Create(new TextChangeRange(new TextSpan(0, oldText.Length), _newText.Length));
	}

	private bool IsChangedFrom(SourceText oldText)
	{
		for (ChangeInfo changeInfo = _info; changeInfo != null; changeInfo = changeInfo.Previous)
		{
			if (changeInfo.WeakOldText.TryGetTarget(out var target) && target == oldText)
			{
				return true;
			}
		}
		return false;
	}

	private static IReadOnlyList<ImmutableArray<TextChangeRange>> GetChangesBetween(SourceText oldText, ChangedText newText)
	{
		List<ImmutableArray<TextChangeRange>> list = new List<ImmutableArray<TextChangeRange>>();
		ChangeInfo changeInfo = newText._info;
		list.Add(changeInfo.ChangeRanges);
		while (changeInfo != null)
		{
			changeInfo.WeakOldText.TryGetTarget(out var target);
			if (target == oldText)
			{
				return list;
			}
			changeInfo = changeInfo.Previous;
			if (changeInfo != null)
			{
				list.Insert(0, changeInfo.ChangeRanges);
			}
		}
		list.Clear();
		return list;
	}

	private static ImmutableArray<TextChangeRange> Merge(IReadOnlyList<ImmutableArray<TextChangeRange>> changeSets)
	{
		ImmutableArray<TextChangeRange> immutableArray = changeSets[0];
		for (int i = 1; i < changeSets.Count; i++)
		{
			immutableArray = Merge(immutableArray, changeSets[i]);
		}
		return immutableArray;
	}

	private static ImmutableArray<TextChangeRange> Merge(ImmutableArray<TextChangeRange> oldChanges, ImmutableArray<TextChangeRange> newChanges)
	{
		List<TextChangeRange> list = new List<TextChangeRange>(oldChanges.Length + newChanges.Length);
		int i = 0;
		int num = 0;
		int num2 = 0;
		while (true)
		{
			TextChangeRange textChangeRange;
			TextChangeRange range;
			if (num < newChanges.Length)
			{
				textChangeRange = newChanges[num];
				while (i < oldChanges.Length)
				{
					range = oldChanges[i];
					while (range.Span.Length != 0 || range.NewLength != 0)
					{
						if (textChangeRange.Span.Length == 0 && textChangeRange.NewLength == 0)
						{
							goto IL_0085;
						}
						if (textChangeRange.Span.End < range.Span.Start + num2)
						{
							goto IL_00af;
						}
						if (textChangeRange.Span.Start > range.Span.Start + num2 + range.NewLength)
						{
							goto IL_0121;
						}
						if (textChangeRange.Span.Start < range.Span.Start + num2)
						{
							int num3 = range.Span.Start + num2 - textChangeRange.Span.Start;
							AddRange(list, new TextChangeRange(new TextSpan(textChangeRange.Span.Start - num2, num3), 0));
							textChangeRange = new TextChangeRange(new TextSpan(range.Span.Start + num2, textChangeRange.Span.Length - num3), textChangeRange.NewLength);
							continue;
						}
						if (textChangeRange.Span.Start > range.Span.Start + num2)
						{
							int num4 = textChangeRange.Span.Start - (range.Span.Start + num2);
							AddRange(list, new TextChangeRange(range.Span, num4));
							num2 = num2 - range.Span.Length + num4;
							range = new TextChangeRange(new TextSpan(range.Span.Start, 0), range.NewLength - num4);
							textChangeRange = new TextChangeRange(new TextSpan(range.Span.Start + num2, textChangeRange.Span.Length), textChangeRange.NewLength);
							continue;
						}
						goto IL_02d2;
					}
					i++;
					continue;
					IL_02d2:
					if (textChangeRange.Span.Start != range.Span.Start + num2)
					{
						goto end_IL_001b;
					}
					if (range.NewLength == 0)
					{
						AddRange(list, range);
						num2 = num2 - range.Span.Length + range.NewLength;
						i++;
						continue;
					}
					if (textChangeRange.Span.Length != 0)
					{
						int num5 = Math.Min(range.NewLength, textChangeRange.Span.Length);
						AddRange(list, new TextChangeRange(range.Span, range.NewLength - num5));
						num2 = num2 - range.Span.Length + (range.NewLength - num5);
						i++;
						textChangeRange = new TextChangeRange(new TextSpan(range.Span.Start + num2, textChangeRange.Span.Length - num5), textChangeRange.NewLength);
						continue;
					}
					goto IL_0340;
					IL_0121:
					AddRange(list, range);
					num2 = num2 - range.Span.Length + range.NewLength;
					i++;
				}
				TextChangeRange range2 = new TextChangeRange(new TextSpan(textChangeRange.Span.Start - num2, textChangeRange.Span.Length), textChangeRange.NewLength);
				AddRange(list, range2);
				num++;
				continue;
			}
			for (; i < oldChanges.Length; i++)
			{
				AddRange(list, oldChanges[i]);
			}
			break;
			IL_00af:
			TextChangeRange range3 = new TextChangeRange(new TextSpan(textChangeRange.Span.Start - num2, textChangeRange.Span.Length), textChangeRange.NewLength);
			AddRange(list, range3);
			num++;
			continue;
			IL_0340:
			AddRange(list, new TextChangeRange(range.Span, range.NewLength + textChangeRange.NewLength));
			num2 = num2 - range.Span.Length + range.NewLength;
			i++;
			num++;
			continue;
			IL_0085:
			num++;
			continue;
			end_IL_001b:
			break;
		}
		return list.ToImmutableArray();
	}

	private static void AddRange(List<TextChangeRange> list, TextChangeRange range)
	{
		if (list.Count > 0)
		{
			TextChangeRange textChangeRange = list[list.Count - 1];
			if (textChangeRange.Span.End == range.Span.Start)
			{
				list[list.Count - 1] = new TextChangeRange(new TextSpan(textChangeRange.Span.Start, textChangeRange.Span.Length + range.Span.Length), textChangeRange.NewLength + range.NewLength);
				return;
			}
		}
		list.Add(range);
	}

	protected override TextLineCollection GetLinesCore()
	{
		if (!_info.WeakOldText.TryGetTarget(out var target) || !target.TryGetLines(out var lines))
		{
			return base.GetLinesCore();
		}
		ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance();
		instance.Add(0);
		int num = 0;
		int num2 = 0;
		bool flag = false;
		foreach (TextChangeRange changeRange in _info.ChangeRanges)
		{
			if (changeRange.Span.Start > num)
			{
				if (flag && _newText[num + num2] == '\n')
				{
					instance.RemoveLast();
				}
				LinePositionSpan linePositionSpan = lines.GetLinePositionSpan(TextSpan.FromBounds(num, changeRange.Span.Start));
				for (int i = linePositionSpan.Start.Line + 1; i <= linePositionSpan.End.Line; i++)
				{
					instance.Add(lines[i].Start + num2);
				}
				flag = target[changeRange.Span.Start - 1] == '\r';
				if (flag && changeRange.Span.Start < target.Length && target[changeRange.Span.Start] == '\n')
				{
					instance.Add(changeRange.Span.Start + num2);
				}
			}
			if (changeRange.NewLength > 0)
			{
				int num3 = changeRange.Span.Start + num2;
				SourceText subText = GetSubText(new TextSpan(num3, changeRange.NewLength));
				if (flag && subText[0] == '\n')
				{
					instance.RemoveLast();
				}
				for (int j = 1; j < subText.Lines.Count; j++)
				{
					instance.Add(num3 + subText.Lines[j].Start);
				}
				flag = subText[changeRange.NewLength - 1] == '\r';
			}
			num = changeRange.Span.End;
			num2 += changeRange.NewLength - changeRange.Span.Length;
		}
		if (num < target.Length)
		{
			if (flag && _newText[num + num2] == '\n')
			{
				instance.RemoveLast();
			}
			LinePositionSpan linePositionSpan2 = lines.GetLinePositionSpan(TextSpan.FromBounds(num, target.Length));
			for (int k = linePositionSpan2.Start.Line + 1; k <= linePositionSpan2.End.Line; k++)
			{
				instance.Add(lines[k].Start + num2);
			}
		}
		return new LineInfo(this, instance.ToArrayAndFree());
	}
}
