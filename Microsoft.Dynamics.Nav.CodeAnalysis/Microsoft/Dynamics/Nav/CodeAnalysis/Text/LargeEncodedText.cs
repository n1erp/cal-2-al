using System;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Text;

internal sealed class LargeEncodedText : SourceText
{
	internal const int ChunkSize = 40960;

	private readonly ImmutableArray<char[]> chunks;

	private readonly int[] chunkStartOffsets;

	private readonly int length;

	private readonly Encoding encoding;

	public override char this[int position]
	{
		get
		{
			int indexFromPosition = GetIndexFromPosition(position);
			return chunks[indexFromPosition][position - chunkStartOffsets[indexFromPosition]];
		}
	}

	public override Encoding Encoding => encoding;

	public override int Length => length;

	private LargeEncodedText(ImmutableArray<char[]> chunks, Encoding encoding)
	{
		this.chunks = chunks;
		this.encoding = encoding;
		chunkStartOffsets = new int[chunks.Length];
		int num = 0;
		for (int i = 0; i < chunks.Length; i++)
		{
			chunkStartOffsets[i] = num;
			num += chunks[i].Length;
		}
		length = num;
	}

	internal static SourceText Decode(Stream stream, Encoding encoding, bool throwIfBinaryDetected)
	{
		stream.Seek(0L, SeekOrigin.Begin);
		int num = (int)stream.Length;
		if (num == 0)
		{
			return SourceText.From(string.Empty, encoding);
		}
		using StreamReader streamReader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, Math.Min(num, 4096), leaveOpen: true);
		ArrayBuilder<char[]> instance = ArrayBuilder<char[]>.GetInstance(1 + num / 40960);
		while (!streamReader.EndOfStream)
		{
			char[] array = new char[40960];
			int num2 = streamReader.ReadBlock(array, 0, 40960);
			if (num2 == 0)
			{
				break;
			}
			if (num2 < 40960)
			{
				Array.Resize(ref array, num2);
			}
			if (throwIfBinaryDetected && IsBinary(array))
			{
				throw new InvalidDataException();
			}
			instance.Add(array);
		}
		return new LargeEncodedText(instance.ToImmutableAndFree(), streamReader.CurrentEncoding);
	}

	private static bool IsBinary(char[] chunk)
	{
		int num = 1;
		while (num < chunk.Length)
		{
			if (chunk[num] == '\0')
			{
				if (chunk[num - 1] == '\0')
				{
					return true;
				}
				num++;
			}
			else
			{
				num += 2;
			}
		}
		return false;
	}

	private int GetIndexFromPosition(int position)
	{
		int num = Array.BinarySearch(chunkStartOffsets, position);
		if (num < 0)
		{
			return ~num - 1;
		}
		return num;
	}

	public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
	{
		if (count == 0)
		{
			return;
		}
		int num = GetIndexFromPosition(sourceIndex);
		int num2 = sourceIndex - chunkStartOffsets[num];
		while (true)
		{
			char[] array = chunks[num];
			int num3 = Math.Min(array.Length - num2, count);
			Array.Copy(array, num2, destination, destinationIndex, num3);
			count -= num3;
			if (count > 0)
			{
				destinationIndex += num3;
				num2 = 0;
				num++;
				continue;
			}
			break;
		}
	}

	public override void Write(TextWriter writer, TextSpan span, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (span.Start < 0 || span.Start > length || span.End > length)
		{
			throw new ArgumentOutOfRangeException("span");
		}
		int num = span.Length;
		if (num == 0)
		{
			return;
		}
		int num2 = GetIndexFromPosition(span.Start);
		int num3 = span.Start - chunkStartOffsets[num2];
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			char[] array = chunks[num2];
			int num4 = Math.Min(array.Length - num3, num);
			writer.Write(array, num3, num4);
			num -= num4;
			if (num > 0)
			{
				num3 = 0;
				num2++;
				continue;
			}
			break;
		}
	}

	protected override TextLineCollection GetLinesCore()
	{
		return new LineInfo(this, ParseLineStarts());
	}

	private int[] ParseLineStarts()
	{
		int item = 0;
		int num = 0;
		int num2 = -1;
		ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance();
		foreach (char[] chunk in chunks)
		{
			foreach (char c in chunk)
			{
				num++;
				if ((uint)(c - 14) <= 113u)
				{
					continue;
				}
				if ((uint)c <= 13u)
				{
					if (c != '\n')
					{
						if (c != '\r')
						{
							continue;
						}
						num2 = num;
					}
					else if (num2 == num - 1)
					{
						item = num;
						continue;
					}
				}
				else if (c != '\u0085' && c != '\u2028' && c != '\u2029')
				{
					continue;
				}
				instance.Add(item);
				item = num;
			}
		}
		instance.Add(item);
		return instance.ToArrayAndFree();
	}
}
