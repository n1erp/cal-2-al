using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Text;

internal sealed class CompositeText : SourceText
{
	private readonly ImmutableArray<SourceText> _segments;

	private readonly int _length;

	private readonly int _storageSize;

	private readonly int[] _segmentOffsets;

	private readonly Encoding _encoding;

	internal const int TARGET_SEGMENT_COUNT_AFTER_REDUCTION = 32;

	internal const int MAXIMUM_SEGMENT_COUNT_BEFORE_REDUCTION = 64;

	private const int INITIAL_SEGMENT_SIZE_FOR_COMBINING = 32;

	private const int MAXIMUM_SEGMENT_SIZE_FOR_COMBINING = 134217727;

	private static ObjectPool<HashSet<SourceText>> s_uniqueSourcesPool = new ObjectPool<HashSet<SourceText>>(() => new HashSet<SourceText>(), 5);

	public override Encoding Encoding => _encoding;

	public override int Length => _length;

	internal override int StorageSize => _storageSize;

	internal override ImmutableArray<SourceText> Segments => _segments;

	public override char this[int position]
	{
		get
		{
			GetIndexAndOffset(position, out var index, out var offset);
			return _segments[index][offset];
		}
	}

	private CompositeText(ImmutableArray<SourceText> segments, Encoding encoding, SourceHashAlgorithm checksumAlgorithm)
		: base(default, checksumAlgorithm)
	{
		_segments = segments;
		_encoding = encoding;
		ComputeLengthAndStorageSize(segments, out _length, out _storageSize);
		_segmentOffsets = new int[segments.Length];
		int num = 0;
		for (int i = 0; i < _segmentOffsets.Length; i++)
		{
			_segmentOffsets[i] = num;
			num += _segments[i].Length;
		}
	}

	public override SourceText GetSubText(TextSpan span)
	{
		CheckSubSpan(span);
		int start = span.Start;
		int num = span.Length;
		GetIndexAndOffset(start, out var index, out var offset);
		ArrayBuilder<SourceText> instance = ArrayBuilder<SourceText>.GetInstance();
		while (index < _segments.Length && num > 0)
		{
			SourceText sourceText = _segments[index];
			int num2 = Math.Min(num, sourceText.Length - offset);
			AddSegments(instance, sourceText.GetSubText(new TextSpan(offset, num2)));
			num -= num2;
			index++;
			offset = 0;
		}
		return ToSourceTextAndFree(instance, this, adjustSegments: false);
	}

	private void GetIndexAndOffset(int position, out int index, out int offset)
	{
		int num = _segmentOffsets.BinarySearch(position);
		index = ((num >= 0) ? num : (~num - 1));
		offset = position - _segmentOffsets[index];
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
			while (index < _segments.Length && count > 0)
			{
				SourceText sourceText = _segments[index];
				int num = Math.Min(count, sourceText.Length - offset);
				sourceText.CopyTo(offset, destination, destinationIndex, num);
				count -= num;
				destinationIndex += num;
				index++;
				offset = 0;
			}
		}
	}

	internal static void AddSegments(ArrayBuilder<SourceText> segments, SourceText text)
	{
		if (!(text is CompositeText compositeText))
		{
			segments.Add(text);
		}
		else
		{
			segments.AddRange(compositeText._segments);
		}
	}

	internal static SourceText ToSourceTextAndFree(ArrayBuilder<SourceText> segments, SourceText original, bool adjustSegments)
	{
		if (adjustSegments)
		{
			TrimInaccessibleText(segments);
			ReduceSegmentCountIfNecessary(segments);
		}
		if (segments.Count == 0)
		{
			segments.Free();
			return SourceText.From(string.Empty, original.Encoding, original.ChecksumAlgorithm);
		}
		if (segments.Count == 1)
		{
			SourceText result = segments[0];
			segments.Free();
			return result;
		}
		return new CompositeText(segments.ToImmutableAndFree(), original.Encoding, original.ChecksumAlgorithm);
	}

	private static void ReduceSegmentCountIfNecessary(ArrayBuilder<SourceText> segments)
	{
		if (segments.Count > 64)
		{
			int minimalSegmentSizeToUseForCombining = GetMinimalSegmentSizeToUseForCombining(segments);
			CombineSegments(segments, minimalSegmentSizeToUseForCombining);
		}
	}

	private static int GetMinimalSegmentSizeToUseForCombining(ArrayBuilder<SourceText> segments)
	{
		for (int num = 32; num <= 134217727; num *= 2)
		{
			if (GetSegmentCountIfCombined(segments, num) <= 32)
			{
				return num;
			}
		}
		return 134217727;
	}

	private static int GetSegmentCountIfCombined(ArrayBuilder<SourceText> segments, int segmentSize)
	{
		int num = 0;
		for (int i = 0; i < segments.Count - 1; i++)
		{
			if (segments[i].Length > segmentSize)
			{
				continue;
			}
			int num2 = 1;
			for (int j = i + 1; j < segments.Count; j++)
			{
				if (segments[j].Length <= segmentSize)
				{
					num2++;
				}
			}
			if (num2 > 1)
			{
				int num3 = num2 - 1;
				num += num3;
				i += num3;
			}
		}
		return segments.Count - num;
	}

	private static void CombineSegments(ArrayBuilder<SourceText> segments, int segmentSize)
	{
		for (int i = 0; i < segments.Count - 1; i++)
		{
			if (segments[i].Length > segmentSize)
			{
				continue;
			}
			int num = segments[i].Length;
			int num2 = 1;
			for (int j = i + 1; j < segments.Count; j++)
			{
				if (segments[j].Length <= segmentSize)
				{
					num2++;
					num += segments[j].Length;
				}
			}
			if (num2 > 1)
			{
				Encoding encoding = segments[i].Encoding;
				SourceHashAlgorithm checksumAlgorithm = segments[i].ChecksumAlgorithm;
				SourceTextWriter sourceTextWriter = SourceTextWriter.Create(encoding, checksumAlgorithm, num);
				while (num2 > 0)
				{
					segments[i].Write(sourceTextWriter);
					segments.RemoveAt(i);
					num2--;
				}
				SourceText item = sourceTextWriter.ToSourceText();
				segments.Insert(i, item);
			}
		}
	}

	private static void ComputeLengthAndStorageSize(IReadOnlyList<SourceText> segments, out int length, out int size)
	{
		HashSet<SourceText> hashSet = s_uniqueSourcesPool.Allocate();
		length = 0;
		for (int i = 0; i < segments.Count; i++)
		{
			SourceText sourceText = segments[i];
			length += sourceText.Length;
			hashSet.Add(sourceText.StorageKey);
		}
		size = 0;
		foreach (SourceText item in hashSet)
		{
			size += item.StorageSize;
		}
		hashSet.Clear();
		s_uniqueSourcesPool.Free(hashSet);
	}

	private static void TrimInaccessibleText(ArrayBuilder<SourceText> segments)
	{
		ComputeLengthAndStorageSize(segments, out var length, out var size);
		if (length >= size / 2)
		{
			return;
		}
		Encoding encoding = segments[0].Encoding;
		SourceHashAlgorithm checksumAlgorithm = segments[0].ChecksumAlgorithm;
		SourceTextWriter sourceTextWriter = SourceTextWriter.Create(encoding, checksumAlgorithm, length);
		foreach (SourceText segment in segments)
		{
			segment.Write(sourceTextWriter);
		}
		segments.Clear();
		segments.Add(sourceTextWriter.ToSourceText());
	}
}
