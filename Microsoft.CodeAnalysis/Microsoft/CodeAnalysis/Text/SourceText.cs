using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Text;

public abstract class SourceText
{
	internal sealed class LineInfo : TextLineCollection
	{
		private readonly SourceText _text;

		private readonly int[] _lineStarts;

		private int _lastLineNumber;

		public override int Count => _lineStarts.Length;

		public override TextLine this[int index]
		{
			get
			{
				if (index < 0 || index >= _lineStarts.Length)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				int start = _lineStarts[index];
				if (index == _lineStarts.Length - 1)
				{
					return TextLine.FromSpan(_text, TextSpan.FromBounds(start, _text.Length));
				}
				int end = _lineStarts[index + 1];
				return TextLine.FromSpan(_text, TextSpan.FromBounds(start, end));
			}
		}

		public LineInfo(SourceText text, int[] lineStarts)
		{
			_text = text;
			_lineStarts = lineStarts;
		}

		public override int IndexOf(int position)
		{
			if (position < 0 || position > _text.Length)
			{
				throw new ArgumentOutOfRangeException("position");
			}
			int lastLineNumber = _lastLineNumber;
			if (position >= _lineStarts[lastLineNumber])
			{
				int num = Math.Min(_lineStarts.Length, lastLineNumber + 4);
				for (int i = lastLineNumber; i < num; i++)
				{
					if (position < _lineStarts[i])
					{
						return _lastLineNumber = i - 1;
					}
				}
			}
			int num2 = _lineStarts.BinarySearch(position);
			if (num2 < 0)
			{
				num2 = ~num2 - 1;
			}
			_lastLineNumber = num2;
			return num2;
		}

		public override TextLine GetLineFromPosition(int position)
		{
			return this[IndexOf(position)];
		}
	}

	private class StaticContainer : SourceTextContainer
	{
		private readonly SourceText _text;

		public override SourceText CurrentText => _text;

		public override event EventHandler<TextChangeEventArgs> TextChanged
		{
			add
			{
			}
			remove
			{
			}
		}

		public StaticContainer(SourceText text)
		{
			_text = text;
		}
	}

	private const int CharBufferSize = 32768;

	private const int CharBufferCount = 5;

	internal const int LargeObjectHeapLimitInChars = 40960;

	private static readonly ObjectPool<char[]> s_charArrayPool = new ObjectPool<char[]>(() => new char[32768], 5);

	private readonly SourceHashAlgorithm _checksumAlgorithm;

	private SourceTextContainer _lazyContainer;

	private TextLineCollection _lazyLineInfo;

	private ImmutableArray<byte> _lazyChecksum;

	private ImmutableArray<byte> _precomputedEmbeddedTextBlob;

	private static readonly Encoding s_utf8EncodingWithNoBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false);

	public SourceHashAlgorithm ChecksumAlgorithm => _checksumAlgorithm;

	public abstract Encoding Encoding { get; }

	public abstract int Length { get; }

	internal virtual int StorageSize => Length;

	internal virtual ImmutableArray<SourceText> Segments => ImmutableArray<SourceText>.Empty;

	internal virtual SourceText StorageKey => this;

	public bool CanBeEmbedded
	{
		get
		{
			if (_precomputedEmbeddedTextBlob.IsDefault)
			{
				return Encoding != null;
			}
			return !_precomputedEmbeddedTextBlob.IsEmpty;
		}
	}

	internal ImmutableArray<byte> PrecomputedEmbeddedTextBlob => _precomputedEmbeddedTextBlob;

	public abstract char this[int position] { get; }

	public virtual SourceTextContainer Container
	{
		get
		{
			if (_lazyContainer == null)
			{
				Interlocked.CompareExchange(ref _lazyContainer, new StaticContainer(this), null);
			}
			return _lazyContainer;
		}
	}

	public TextLineCollection Lines
	{
		get
		{
			TextLineCollection lazyLineInfo = _lazyLineInfo;
			return lazyLineInfo ?? Interlocked.CompareExchange(ref _lazyLineInfo, lazyLineInfo = GetLinesCore(), null) ?? lazyLineInfo;
		}
	}

	protected SourceText(ImmutableArray<byte> checksum = default(ImmutableArray<byte>), SourceHashAlgorithm checksumAlgorithm = SourceHashAlgorithm.Sha1, SourceTextContainer container = null)
	{
		ValidateChecksumAlgorithm(checksumAlgorithm);
		if (!checksum.IsDefault && checksum.Length != CryptographicHashProvider.GetHashSize(checksumAlgorithm))
		{
			throw new ArgumentException(CodeAnalysisResources.InvalidHash, "checksum");
		}
		_checksumAlgorithm = checksumAlgorithm;
		_lazyChecksum = checksum;
		_lazyContainer = container;
	}

	internal SourceText(ImmutableArray<byte> checksum, SourceHashAlgorithm checksumAlgorithm, ImmutableArray<byte> embeddedTextBlob)
		: this(checksum, checksumAlgorithm)
	{
		if (!checksum.IsDefault && embeddedTextBlob.IsDefault)
		{
			_precomputedEmbeddedTextBlob = ImmutableArray<byte>.Empty;
		}
		else
		{
			_precomputedEmbeddedTextBlob = embeddedTextBlob;
		}
	}

	internal static void ValidateChecksumAlgorithm(SourceHashAlgorithm checksumAlgorithm)
	{
		if (!DebugSourceDocument.IsSupportedAlgorithm(checksumAlgorithm))
		{
			throw new ArgumentException(CodeAnalysisResources.UnsupportedHashAlgorithm, "checksumAlgorithm");
		}
	}

	public static SourceText From(string text, Encoding encoding = null, SourceHashAlgorithm checksumAlgorithm = SourceHashAlgorithm.Sha1)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		return new StringText(text, encoding, default, checksumAlgorithm);
	}

	public static SourceText From(TextReader reader, int length, Encoding encoding = null, SourceHashAlgorithm checksumAlgorithm = SourceHashAlgorithm.Sha1)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (length >= 40960)
		{
			return LargeText.Decode(reader, length, encoding, checksumAlgorithm);
		}
		return From(reader.ReadToEnd(), encoding, checksumAlgorithm);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static SourceText From(Stream stream, Encoding encoding, SourceHashAlgorithm checksumAlgorithm, bool throwIfBinaryDetected)
	{
		return From(stream, encoding, checksumAlgorithm, throwIfBinaryDetected, false);
	}

	public static SourceText From(Stream stream, Encoding encoding = null, SourceHashAlgorithm checksumAlgorithm = SourceHashAlgorithm.Sha1, bool throwIfBinaryDetected = false, bool canBeEmbedded = false)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (!stream.CanRead || !stream.CanSeek)
		{
			throw new ArgumentException(CodeAnalysisResources.StreamMustSupportReadAndSeek, "stream");
		}
		ValidateChecksumAlgorithm(checksumAlgorithm);
		encoding = encoding ?? s_utf8EncodingWithNoBOM;
		if (encoding.GetMaxCharCountOrThrowIfHuge(stream) >= 40960)
		{
			return LargeText.Decode(stream, encoding, checksumAlgorithm, throwIfBinaryDetected, canBeEmbedded);
		}
		string text = Decode(stream, encoding, out encoding);
		if (throwIfBinaryDetected && IsBinary(text))
		{
			throw new InvalidDataException();
		}
		ImmutableArray<byte> checksum = CalculateChecksum(stream, checksumAlgorithm);
		ImmutableArray<byte> embeddedTextBlob = (canBeEmbedded ? EmbeddedText.CreateBlob(stream) : default(ImmutableArray<byte>));
		return new StringText(text, encoding, checksum, checksumAlgorithm, embeddedTextBlob);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static SourceText From(byte[] buffer, int length, Encoding encoding, SourceHashAlgorithm checksumAlgorithm, bool throwIfBinaryDetected)
	{
		return From(buffer, length, encoding, checksumAlgorithm, throwIfBinaryDetected, false);
	}

	public static SourceText From(byte[] buffer, int length, Encoding encoding = null, SourceHashAlgorithm checksumAlgorithm = SourceHashAlgorithm.Sha1, bool throwIfBinaryDetected = false, bool canBeEmbedded = false)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (length < 0 || length > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		ValidateChecksumAlgorithm(checksumAlgorithm);
		string text = Decode(buffer, length, encoding ?? s_utf8EncodingWithNoBOM, out encoding);
		if (throwIfBinaryDetected && IsBinary(text))
		{
			throw new InvalidDataException();
		}
		ImmutableArray<byte> checksum = CalculateChecksum(buffer, 0, length, checksumAlgorithm);
		ImmutableArray<byte> embeddedTextBlob = (canBeEmbedded ? EmbeddedText.CreateBlob(new ArraySegment<byte>(buffer, 0, length)) : default(ImmutableArray<byte>));
		return new StringText(text, encoding, checksum, checksumAlgorithm, embeddedTextBlob);
	}

	private static string Decode(Stream stream, Encoding encoding, out Encoding actualEncoding)
	{
		stream.Seek(0L, SeekOrigin.Begin);
		int num = (int)stream.Length;
		if (num == 0)
		{
			actualEncoding = encoding;
			return string.Empty;
		}
		using StreamReader streamReader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, Math.Min(4096, num), leaveOpen: true);
		string result = streamReader.ReadToEnd();
		actualEncoding = streamReader.CurrentEncoding;
		return result;
	}

	private static string Decode(byte[] buffer, int length, Encoding encoding, out Encoding actualEncoding)
	{
		actualEncoding = TryReadByteOrderMark(buffer, length, out var preambleLength) ?? encoding;
		return actualEncoding.GetString(buffer, preambleLength, length - preambleLength);
	}

	internal static bool IsBinary(string text)
	{
		int num = 1;
		while (num < text.Length)
		{
			if (text[num] == '\0')
			{
				if (text[num - 1] == '\0')
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

	public abstract void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count);

	internal void CheckSubSpan(TextSpan span)
	{
		if (span.Start < 0 || span.Start > Length || span.End > Length)
		{
			throw new ArgumentOutOfRangeException("span");
		}
	}

	public virtual SourceText GetSubText(TextSpan span)
	{
		CheckSubSpan(span);
		int length = span.Length;
		if (length == 0)
		{
			return From(string.Empty, Encoding, ChecksumAlgorithm);
		}
		if (length == Length && span.Start == 0)
		{
			return this;
		}
		return new SubText(this, span);
	}

	public SourceText GetSubText(int start)
	{
		if (start < 0 || start > Length)
		{
			throw new ArgumentOutOfRangeException("start");
		}
		if (start == 0)
		{
			return this;
		}
		return GetSubText(new TextSpan(start, Length - start));
	}

	public void Write(TextWriter textWriter, CancellationToken cancellationToken = default(CancellationToken))
	{
		Write(textWriter, new TextSpan(0, Length), cancellationToken);
	}

	public virtual void Write(TextWriter writer, TextSpan span, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSubSpan(span);
		char[] array = s_charArrayPool.Allocate();
		try
		{
			int i = Math.Min(Length, span.Start);
			int num2;
			for (int num = Math.Min(Length, span.End) - i; i < num; i += num2)
			{
				cancellationToken.ThrowIfCancellationRequested();
				num2 = Math.Min(array.Length, num - i);
				CopyTo(i, array, 0, num2);
				writer.Write(array, 0, num2);
			}
		}
		finally
		{
			s_charArrayPool.Free(array);
		}
	}

	public ImmutableArray<byte> GetChecksum()
	{
		if (_lazyChecksum.IsDefault)
		{
			using SourceTextStream stream = new SourceTextStream(this, 2048, useDefaultEncodingIfNull: true);
			ImmutableInterlocked.InterlockedInitialize(ref _lazyChecksum, CalculateChecksum(stream, _checksumAlgorithm));
		}
		return _lazyChecksum;
	}

	internal static ImmutableArray<byte> CalculateChecksum(byte[] buffer, int offset, int count, SourceHashAlgorithm algorithmId)
	{
		using HashAlgorithm hashAlgorithm = CryptographicHashProvider.TryGetAlgorithm(algorithmId);
		return ImmutableArray.Create(hashAlgorithm.ComputeHash(buffer, offset, count));
	}

	internal static ImmutableArray<byte> CalculateChecksum(Stream stream, SourceHashAlgorithm algorithmId)
	{
		using HashAlgorithm hashAlgorithm = CryptographicHashProvider.TryGetAlgorithm(algorithmId);
		if (stream.CanSeek)
		{
			stream.Seek(0L, SeekOrigin.Begin);
		}
		return ImmutableArray.Create(hashAlgorithm.ComputeHash(stream));
	}

	public override string ToString()
	{
		return ToString(new TextSpan(0, Length));
	}

	public virtual string ToString(TextSpan span)
	{
		CheckSubSpan(span);
		StringBuilder stringBuilder = new StringBuilder();
		char[] array = new char[Math.Min(span.Length, 1024)];
		int i = Math.Max(Math.Min(span.Start, Length), 0);
		int num = Math.Min(span.End, Length) - i;
		int num2;
		for (; i < Length; i += num2)
		{
			if (num <= 0)
			{
				break;
			}
			num2 = Math.Min(array.Length, num);
			CopyTo(i, array, 0, num2);
			stringBuilder.Append(array, 0, num2);
			num -= num2;
		}
		return stringBuilder.ToString();
	}

	public virtual SourceText WithChanges(IEnumerable<TextChange> changes)
	{
		if (changes == null)
		{
			throw new ArgumentNullException("changes");
		}
		if (!changes.Any())
		{
			return this;
		}
		ArrayBuilder<SourceText> instance = ArrayBuilder<SourceText>.GetInstance();
		ArrayBuilder<TextChangeRange> instance2 = ArrayBuilder<TextChangeRange>.GetInstance();
		int num = 0;
		foreach (TextChange change in changes)
		{
			if (change.Span.Start < num)
			{
				if (change.Span.End <= instance2.Last().Span.Start)
				{
					changes = changes.Where((TextChange c) =>
					{
						TextChange textChange = c;
						if (textChange.Span.IsEmpty)
						{
							textChange = c;
							string newText = textChange.NewText;
							if (newText == null)
							{
								return false;
							}
							return newText.Length > 0;
						}
						return true;
					}).OrderBy((TextChange c) =>
					{
						TextChange textChange = c;
						return textChange.Span;
					}).ToList();
					return WithChanges(changes);
				}
				throw new ArgumentException(CodeAnalysisResources.ChangesMustNotOverlap, "changes");
			}
			int num2 = change.NewText?.Length ?? 0;
			if (change.Span.Length != 0 || num2 != 0)
			{
				if (change.Span.Start > num)
				{
					SourceText subText = GetSubText(new TextSpan(num, change.Span.Start - num));
					CompositeText.AddSegments(instance, subText);
				}
				if (num2 > 0)
				{
					SourceText text = From(change.NewText, Encoding, ChecksumAlgorithm);
					CompositeText.AddSegments(instance, text);
				}
				num = change.Span.End;
				instance2.Add(new TextChangeRange(change.Span, num2));
			}
		}
		if (num == 0 && instance.Count == 0)
		{
			instance2.Free();
			return this;
		}
		if (num < Length)
		{
			SourceText subText2 = GetSubText(new TextSpan(num, Length - num));
			CompositeText.AddSegments(instance, subText2);
		}
		SourceText sourceText = CompositeText.ToSourceTextAndFree(instance, this, adjustSegments: true);
		if (sourceText != this)
		{
			return new ChangedText(this, sourceText, instance2.ToImmutableAndFree());
		}
		return this;
	}

	public SourceText WithChanges(params TextChange[] changes)
	{
		return WithChanges((IEnumerable<TextChange>)changes);
	}

	public SourceText Replace(TextSpan span, string newText)
	{
		return WithChanges(new TextChange(span, newText));
	}

	public SourceText Replace(int start, int length, string newText)
	{
		return Replace(new TextSpan(start, length), newText);
	}

	public virtual IReadOnlyList<TextChangeRange> GetChangeRanges(SourceText oldText)
	{
		if (oldText == null)
		{
			throw new ArgumentNullException("oldText");
		}
		if (oldText == this)
		{
			return TextChangeRange.NoChanges;
		}
		return ImmutableArray.Create(new TextChangeRange(new TextSpan(0, oldText.Length), Length));
	}

	public virtual IReadOnlyList<TextChange> GetTextChanges(SourceText oldText)
	{
		int num = 0;
		List<TextChangeRange> list = GetChangeRanges(oldText).ToList();
		List<TextChange> list2 = new List<TextChange>(list.Count);
		foreach (TextChangeRange item in list)
		{
			int start = item.Span.Start + num;
			string newText;
			if (item.NewLength > 0)
			{
				TextSpan textSpan = new TextSpan(start, item.NewLength);
				newText = ToString(textSpan);
			}
			else
			{
				newText = string.Empty;
			}
			list2.Add(new TextChange(item.Span, newText));
			num += item.NewLength - item.Span.Length;
		}
		return list2.ToImmutableArrayOrEmpty();
	}

	internal bool TryGetLines(out TextLineCollection lines)
	{
		lines = _lazyLineInfo;
		return lines != null;
	}

	protected virtual TextLineCollection GetLinesCore()
	{
		return new LineInfo(this, ParseLineStarts());
	}

	private void EnumerateChars(Action<int, char[], int> action)
	{
		int i = 0;
		char[] array = s_charArrayPool.Allocate();
		int num;
		for (int length = Length; i < length; i += num)
		{
			num = Math.Min(length - i, array.Length);
			CopyTo(i, array, 0, num);
			action(i, array, num);
		}
		action(i, array, 0);
		s_charArrayPool.Free(array);
	}

	private int[] ParseLineStarts()
	{
		if (Length == 0)
		{
			return new int[1];
		}
		ArrayBuilder<int> lineStarts = ArrayBuilder<int>.GetInstance();
		lineStarts.Add(0);
		bool lastWasCR = false;
		EnumerateChars((int position, char[] buffer, int length) =>
		{
			int num = 0;
			if (lastWasCR)
			{
				if (length > 0 && buffer[0] == '\n')
				{
					num++;
				}
				lineStarts.Add(position + num);
				lastWasCR = false;
			}
			while (num < length)
			{
				char c = buffer[num];
				num++;
				if ((uint)(c - 14) > 113u)
				{
					if (c == '\r')
					{
						if (num < length && buffer[num] == '\n')
						{
							num++;
						}
						else if (num >= length)
						{
							lastWasCR = true;
							continue;
						}
					}
					else if (!TextUtilities.IsAnyLineBreakCharacter(c))
					{
						continue;
					}
					lineStarts.Add(position + num);
				}
			}
		});
		return lineStarts.ToArrayAndFree();
	}

	public bool ContentEquals(SourceText other)
	{
		if (this == other)
		{
			return true;
		}
		ImmutableArray<byte> lazyChecksum = _lazyChecksum;
		ImmutableArray<byte> lazyChecksum2 = other._lazyChecksum;
		if (!lazyChecksum.IsDefault && !lazyChecksum2.IsDefault && Encoding == other.Encoding && ChecksumAlgorithm == other.ChecksumAlgorithm)
		{
			return lazyChecksum.SequenceEqual(lazyChecksum2);
		}
		return ContentEqualsImpl(other);
	}

	protected virtual bool ContentEqualsImpl(SourceText other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (Length != other.Length)
		{
			return false;
		}
		char[] array = s_charArrayPool.Allocate();
		char[] array2 = s_charArrayPool.Allocate();
		try
		{
			int num;
			for (int i = 0; i < Length; i += num)
			{
				num = Math.Min(Length - i, array.Length);
				CopyTo(i, array, 0, num);
				other.CopyTo(i, array2, 0, num);
				for (int j = 0; j < num; j++)
				{
					if (array[j] != array2[j])
					{
						return false;
					}
				}
			}
			return true;
		}
		finally
		{
			s_charArrayPool.Free(array2);
			s_charArrayPool.Free(array);
		}
	}

	internal static Encoding TryReadByteOrderMark(byte[] source, int length, out int preambleLength)
	{
		if (length >= 2)
		{
			switch (source[0])
			{
			case 254:
				if (source[1] == byte.MaxValue)
				{
					preambleLength = 2;
					return Encoding.BigEndianUnicode;
				}
				break;
			case byte.MaxValue:
				if (source[1] == 254)
				{
					preambleLength = 2;
					return Encoding.Unicode;
				}
				break;
			case 239:
				if (source[1] == 187 && length >= 3 && source[2] == 191)
				{
					preambleLength = 3;
					return Encoding.UTF8;
				}
				break;
			}
		}
		preambleLength = 0;
		return null;
	}
}
