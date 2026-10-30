using System;
using System.Collections.Immutable;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Text;

internal sealed class CompositeText : SourceText
{
	private readonly ImmutableArray<SourceText> SourceTexts;

	private readonly int length;

	public override Encoding Encoding => SourceTexts[0].Encoding;

	public override int Length => length;

	public override char this[int position]
	{
		get
		{
			GetIndexAndOffset(position, out var index, out var offset);
			return SourceTexts[index][offset];
		}
	}

	public CompositeText(ImmutableArray<SourceText> sourceTexts)
	{
		DebugAssertHelper.Assert(!sourceTexts.IsDefaultOrEmpty);
		SourceTexts = sourceTexts;
		int num = 0;
		foreach (SourceText item in sourceTexts)
		{
			num += item.Length;
		}
		length = num;
	}

	public override SourceText GetSubText(TextSpan span)
	{
		CheckSubSpan(span);
		int start = span.Start;
		int num = span.Length;
		GetIndexAndOffset(start, out var index, out var offset);
		ArrayBuilder<SourceText> instance = ArrayBuilder<SourceText>.GetInstance();
		while (index < SourceTexts.Length && num > 0)
		{
			SourceText sourceText = SourceTexts[index];
			int num2 = Math.Min(num, sourceText.Length - offset);
			AddSegments(instance, sourceText.GetSubText(new TextSpan(offset, num2)));
			num -= num2;
			index++;
			offset = 0;
		}
		if (instance.Count == 0)
		{
			instance.Free();
			return SourceText.From(string.Empty, Encoding);
		}
		if (instance.Count == 1)
		{
			SourceText result = instance[0];
			instance.Free();
			return result;
		}
		return new CompositeText(instance.ToImmutableAndFree());
	}

	private void GetIndexAndOffset(int position, out int index, out int offset)
	{
		for (int i = 0; i < SourceTexts.Length; i++)
		{
			SourceText sourceText = SourceTexts[i];
			if (position < sourceText.Length)
			{
				index = i;
				offset = position;
				return;
			}
			position -= sourceText.Length;
		}
		index = 0;
		offset = 0;
		throw new ArgumentException("position");
	}

	private bool CheckCopyToArguments(int sourceIndex, char[] destination, int destinationIndex, int count)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (sourceIndex < 0)
		{
			throw new ArgumentOutOfRangeException("sourceIndex");
		}
		if (destinationIndex < 0)
		{
			throw new ArgumentOutOfRangeException("destinationIndex");
		}
		if (count < 0 || count > Length - sourceIndex || count > destination.Length - destinationIndex)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		return count > 0;
	}

	public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
	{
		if (CheckCopyToArguments(sourceIndex, destination, destinationIndex, count))
		{
			GetIndexAndOffset(sourceIndex, out var index, out var offset);
			while (index < SourceTexts.Length && count > 0)
			{
				SourceText sourceText = SourceTexts[index];
				int num = Math.Min(count, sourceText.Length - offset);
				sourceText.CopyTo(offset, destination, destinationIndex, num);
				count -= num;
				destinationIndex += num;
				index++;
				offset = 0;
			}
		}
	}

	internal static void AddSegments(ArrayBuilder<SourceText> builder, SourceText text)
	{
		if (!(text is CompositeText compositeText))
		{
			builder.Add(text);
		}
		else
		{
			builder.AddRange(compositeText.SourceTexts);
		}
	}
}
