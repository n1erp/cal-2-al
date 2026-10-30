using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Text;

public abstract class SourceText
{
	internal sealed class LineInfo : TextLineCollection
	{
		private readonly SourceText text;

		private readonly int[] lineStarts;

		private int lastLineNumber;

		public override int Count => lineStarts.Length;

		public override TextLine this[int index]
		{
			get
			{
				if (index < 0 || index >= lineStarts.Length)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				int start = lineStarts[index];
				if (index == lineStarts.Length - 1)
				{
					return TextLine.FromSpan(text, TextSpan.FromBounds(start, text.Length));
				}
				int end = lineStarts[index + 1];
				return TextLine.FromSpan(text, TextSpan.FromBounds(start, end));
			}
		}

		public LineInfo(SourceText text, int[] lineStarts)
		{
			this.text = text;
			this.lineStarts = lineStarts;
		}

		public override int IndexOf(int position)
		{
			if (position < 0 || position > text.Length)
			{
				throw new ArgumentOutOfRangeException("position");
			}
			int num = lastLineNumber;
			if (position >= lineStarts[num])
			{
				int num2 = Math.Min(lineStarts.Length, num + 4);
				for (int i = num; i < num2; i++)
				{
					if (position < lineStarts[i])
					{
						return lastLineNumber = i - 1;
					}
				}
			}
			int num3 = Array.BinarySearch(lineStarts, position);
			if (num3 < 0)
			{
				num3 = ~num3 - 1;
			}
			lastLineNumber = num3;
			return num3;
		}

		public override TextLine GetLineFromPosition(int position)
		{
			return this[IndexOf(position)];
		}
	}

	private class StaticContainer : SourceTextContainer
	{
		public override SourceText CurrentText { get; }

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
			CurrentText = text;
		}
	}

	private const int CharBufferSize = 32768;

	private const int CharBufferCount = 5;

	private const int LargeObjectHeapLimitInChars = 40960;

	private static readonly ObjectPool<char[]> charArrayPool = new ObjectPool<char[]>(() => new char[32768], 5);

	private SourceTextContainer lazyContainer;

	private TextLineCollection lazyLineInfo;

	private readonly ImmutableArray<byte> lazyChecksum;

	private static readonly Encoding utf8EncodingWithNoBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false);

	public abstract Encoding Encoding { get; }

	public abstract int Length { get; }

	public abstract char this[int position] { get; }

	public virtual SourceTextContainer Container
	{
		get
		{
			if (lazyContainer == null)
			{
				Interlocked.CompareExchange(ref lazyContainer, new StaticContainer(this), null);
			}
			return lazyContainer;
		}
	}

	public TextLineCollection Lines
	{
		get
		{
			TextLineCollection textLineCollection = lazyLineInfo;
			return textLineCollection ?? Interlocked.CompareExchange(ref lazyLineInfo, textLineCollection = GetLinesCore(), null) ?? textLineCollection;
		}
	}

	protected SourceText(ImmutableArray<byte> checksum = default(ImmutableArray<byte>), SourceTextContainer container = null)
	{
		lazyChecksum = checksum;
		lazyContainer = container;
	}

	public static SourceText From(string text, Encoding encoding = null)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		return new StringText(text, encoding);
	}

	public static SourceText From(Stream stream, Encoding encoding = null, bool throwIfBinaryDetected = false)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (!stream.CanRead || !stream.CanSeek)
		{
			throw new ArgumentException(CodeAnalysisResources.StreamMustSupportReadAndSeek, "stream");
		}
		encoding = encoding ?? utf8EncodingWithNoBOM;
		if (encoding.GetMaxCharCount((int)stream.Length) >= 40960)
		{
			return LargeEncodedText.Decode(stream, encoding, throwIfBinaryDetected);
		}
		string text = Decode(stream, encoding, out encoding);
		if (throwIfBinaryDetected && IsBinary(text))
		{
			throw new InvalidDataException();
		}
		return new StringText(text, encoding);
	}

	public static SourceText From(byte[] buffer, int length, Encoding encoding = null, bool throwIfBinaryDetected = false)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (length < 0 || length > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		string text = Decode(buffer, length, encoding ?? utf8EncodingWithNoBOM, out encoding);
		if (throwIfBinaryDetected && IsBinary(text))
		{
			throw new InvalidDataException();
		}
		return new StringText(text, encoding);
	}

	private static string Decode(Stream stream, Encoding encoding, out Encoding actualEncoding)
	{
		DebugAssertHelper.Assert(stream != null);
		DebugAssertHelper.Assert(encoding != null);
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
		DebugAssertHelper.Assert(buffer != null);
		DebugAssertHelper.Assert(encoding != null);
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
			throw new ArgumentOutOfRangeException("span", string.Format(CultureInfo.InvariantCulture, "Span: {0}, Length: {1}", span.ToString(), Length));
		}
	}

	public virtual SourceText GetSubText(TextSpan span)
	{
		CheckSubSpan(span);
		int length = span.Length;
		if (length == 0)
		{
			return From(string.Empty, Encoding);
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
		char[] array = charArrayPool.Allocate();
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
			charArrayPool.Free(array);
		}
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
				throw new ArgumentException(CodeAnalysisResources.ChangesMustBeOrderedAndNotOverlapping, "changes");
			}
			if (change.Span.Start > num)
			{
				SourceText subText = GetSubText(new TextSpan(num, change.Span.Start - num));
				CompositeText.AddSegments(instance, subText);
			}
			if (!string.IsNullOrEmpty(change.NewText))
			{
				SourceText text = From(change.NewText, Encoding);
				CompositeText.AddSegments(instance, text);
			}
			num = change.Span.End;
			instance2.Add(new TextChangeRange(change.Span, (change.NewText != null) ? change.NewText.Length : 0));
		}
		if (num < Length)
		{
			SourceText subText2 = GetSubText(new TextSpan(num, Length - num));
			CompositeText.AddSegments(instance, subText2);
		}
		return new ChangedText(this, instance2.ToImmutableAndFree(), instance.ToImmutableAndFree());
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
		IReadOnlyList<TextChangeRange> changeRanges = GetChangeRanges(oldText);
		List<TextChange> list = new List<TextChange>(changeRanges.Count);
		foreach (TextChangeRange item in changeRanges)
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
			list.Add(new TextChange(item.Span, newText));
			num += item.NewLength - item.Span.Length;
		}
		return list;
	}

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	protected virtual TextLineCollection GetLinesCore()
	{
		return new LineInfo(this, ParseLineStarts());
	}

	private int[] ParseLineStarts()
	{
		int length = Length;
		if (Length == 0)
		{
			return new int[1];
		}
		int item = 0;
		int num = 0;
		ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance();
		while (num < length)
		{
			char c = this[num++];
			if ((uint)(c - 14) > 113u)
			{
				if (c == '\r' && num < length && this[num] == '\n')
				{
					num++;
				}
				else if (!TextUtilities.IsAnyLineBreakCharacter(c))
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

	public bool ContentEquals(SourceText other)
	{
		if (this == other)
		{
			return true;
		}
		ImmutableArray<byte> immutableArray = lazyChecksum;
		ImmutableArray<byte> items = other.lazyChecksum;
		if (!immutableArray.IsDefault && !items.IsDefault && Encoding == other.Encoding)
		{
			return immutableArray.SequenceEqual(items);
		}
		return ContentEqualsCore(other);
	}

	protected virtual bool ContentEqualsCore(SourceText other)
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
		char[] array = charArrayPool.Allocate();
		char[] array2 = charArrayPool.Allocate();
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
			charArrayPool.Free(array2);
			charArrayPool.Free(array);
		}
	}

	internal static Encoding TryReadByteOrderMark(byte[] source, int length, out int preambleLength)
	{
		DebugAssertHelper.Assert(source != null);
		DebugAssertHelper.Assert(length <= source.Length);
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
