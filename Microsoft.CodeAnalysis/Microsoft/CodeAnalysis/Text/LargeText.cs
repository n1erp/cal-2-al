using System;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Text;

internal sealed class LargeText : SourceText
{
	internal const int ChunkSize = 40960;

	private readonly ImmutableArray<char[]> _chunks;

	private readonly int[] _chunkStartOffsets;

	private readonly int _length;

	private readonly Encoding _encodingOpt;

	public override char this[int position]
	{
		get
		{
			int indexFromPosition = GetIndexFromPosition(position);
			return _chunks[indexFromPosition][position - _chunkStartOffsets[indexFromPosition]];
		}
	}

	public override Encoding Encoding => _encodingOpt;

	public override int Length => _length;

	internal LargeText(ImmutableArray<char[]> chunks, Encoding encodingOpt, ImmutableArray<byte> checksum, SourceHashAlgorithm checksumAlgorithm, ImmutableArray<byte> embeddedTextBlob)
		: base(checksum, checksumAlgorithm, embeddedTextBlob)
	{
		_chunks = chunks;
		_encodingOpt = encodingOpt;
		_chunkStartOffsets = new int[chunks.Length];
		int num = 0;
		for (int i = 0; i < chunks.Length; i++)
		{
			_chunkStartOffsets[i] = num;
			num += chunks[i].Length;
		}
		_length = num;
	}

	internal LargeText(ImmutableArray<char[]> chunks, Encoding encodingOpt, SourceHashAlgorithm checksumAlgorithm)
		: this(chunks, encodingOpt, default, checksumAlgorithm, default)
	{
	}

	internal static SourceText Decode(Stream stream, Encoding encoding, SourceHashAlgorithm checksumAlgorithm, bool throwIfBinaryDetected, bool canBeEmbedded)
	{
		stream.Seek(0L, SeekOrigin.Begin);
		long length = stream.Length;
		if (length == 0L)
		{
			return SourceText.From(string.Empty, encoding, checksumAlgorithm);
		}
		int maxCharCountOrThrowIfHuge = encoding.GetMaxCharCountOrThrowIfHuge(stream);
		int val = (int)length;
		using StreamReader streamReader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, Math.Min(val, 4096), leaveOpen: true);
		return new LargeText(ReadChunksFromTextReader(streamReader, maxCharCountOrThrowIfHuge, throwIfBinaryDetected), checksum: SourceText.CalculateChecksum(stream, checksumAlgorithm), embeddedTextBlob: canBeEmbedded ? EmbeddedText.CreateBlob(stream) : default(ImmutableArray<byte>), encodingOpt: streamReader.CurrentEncoding, checksumAlgorithm: checksumAlgorithm);
	}

	internal static SourceText Decode(TextReader reader, int length, Encoding encodingOpt, SourceHashAlgorithm checksumAlgorithm)
	{
		if (length == 0)
		{
			return SourceText.From(string.Empty, encodingOpt, checksumAlgorithm);
		}
		return new LargeText(ReadChunksFromTextReader(reader, length, throwIfBinaryDetected: false), encodingOpt, checksumAlgorithm);
	}

	private static ImmutableArray<char[]> ReadChunksFromTextReader(TextReader reader, int maxCharRemainingGuess, bool throwIfBinaryDetected)
	{
		ArrayBuilder<char[]> instance = ArrayBuilder<char[]>.GetInstance(1 + maxCharRemainingGuess / 40960);
		while (reader.Peek() != -1)
		{
			int num = 40960;
			if (maxCharRemainingGuess < 40960)
			{
				num = Math.Max(maxCharRemainingGuess - 64, 64);
			}
			char[] array = new char[num];
			int num2 = reader.ReadBlock(array, 0, array.Length);
			if (num2 == 0)
			{
				break;
			}
			maxCharRemainingGuess -= num2;
			if (num2 < array.Length)
			{
				Array.Resize(ref array, num2);
			}
			if (throwIfBinaryDetected && IsBinary(array))
			{
				throw new InvalidDataException();
			}
			instance.Add(array);
		}
		return instance.ToImmutableAndFree();
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
		int num = _chunkStartOffsets.BinarySearch(position);
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
		int num2 = sourceIndex - _chunkStartOffsets[num];
		while (true)
		{
			char[] array = _chunks[num];
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
		if (span.Start < 0 || span.Start > _length || span.End > _length)
		{
			throw new ArgumentOutOfRangeException("span");
		}
		int num = span.Length;
		if (num == 0)
		{
			return;
		}
		LargeTextWriter largeTextWriter = writer as LargeTextWriter;
		int num2 = GetIndexFromPosition(span.Start);
		int num3 = span.Start - _chunkStartOffsets[num2];
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			char[] array = _chunks[num2];
			int num4 = Math.Min(array.Length - num3, num);
			if (largeTextWriter != null && num3 == 0 && num4 == array.Length)
			{
				largeTextWriter.AppendChunk(array);
			}
			else
			{
				writer.Write(array, num3, num4);
			}
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
		foreach (char[] chunk in _chunks)
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
