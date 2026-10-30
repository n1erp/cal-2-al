using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Utilities;

internal static class JavaScriptUtils
{
	internal static readonly bool[] SingleQuoteCharEscapeFlags;

	internal static readonly bool[] DoubleQuoteCharEscapeFlags;

	internal static readonly bool[] HtmlCharEscapeFlags;

	private const int UnicodeTextLength = 6;

	private const string EscapedUnicodeText = "!";

	static JavaScriptUtils()
	{
		SingleQuoteCharEscapeFlags = new bool[128];
		DoubleQuoteCharEscapeFlags = new bool[128];
		HtmlCharEscapeFlags = new bool[128];
		IList<char> list = new List<char> { '\n', '\r', '\t', '\\', '\f', '\b' };
		for (int i = 0; i < 32; i++)
		{
			list.Add((char)i);
		}
		foreach (char item in list.Union(new char[1] { '\'' }))
		{
			SingleQuoteCharEscapeFlags[(uint)item] = true;
		}
		foreach (char item2 in list.Union(new char[1] { '"' }))
		{
			DoubleQuoteCharEscapeFlags[(uint)item2] = true;
		}
		foreach (char item3 in list.Union(new char[5] { '"', '\'', '<', '>', '&' }))
		{
			HtmlCharEscapeFlags[(uint)item3] = true;
		}
	}

	public static bool[] GetCharEscapeFlags(StringEscapeHandling stringEscapeHandling, char quoteChar)
	{
		if (stringEscapeHandling == StringEscapeHandling.EscapeHtml)
		{
			return HtmlCharEscapeFlags;
		}
		if (quoteChar == '"')
		{
			return DoubleQuoteCharEscapeFlags;
		}
		return SingleQuoteCharEscapeFlags;
	}

	public static bool ShouldEscapeJavaScriptString(string s, bool[] charEscapeFlags)
	{
		if (s == null)
		{
			return false;
		}
		foreach (char c in s)
		{
			if (c >= charEscapeFlags.Length || charEscapeFlags[(uint)c])
			{
				return true;
			}
		}
		return false;
	}

	public static void WriteEscapedJavaScriptString(TextWriter writer, string s, char delimiter, bool appendDelimiters, bool[] charEscapeFlags, StringEscapeHandling stringEscapeHandling, IArrayPool<char> bufferPool, ref char[] writeBuffer)
	{
		if (appendDelimiters)
		{
			writer.Write(delimiter);
		}
		if (!string.IsNullOrEmpty(s))
		{
			int num = FirstCharToEscape(s, charEscapeFlags, stringEscapeHandling);
			if (num == -1)
			{
				writer.Write(s);
			}
			else
			{
				if (num != 0)
				{
					if (writeBuffer == null || writeBuffer.Length < num)
					{
						writeBuffer = BufferUtils.EnsureBufferSize(bufferPool, num, writeBuffer);
					}
					s.CopyTo(0, writeBuffer, 0, num);
					writer.Write(writeBuffer, 0, num);
				}
				int num2;
				for (int i = num; i < s.Length; i++)
				{
					char c = s[i];
					if (c < charEscapeFlags.Length && !charEscapeFlags[(uint)c])
					{
						continue;
					}
					string text;
					switch (c)
					{
					case '\t':
						text = "\\t";
						break;
					case '\n':
						text = "\\n";
						break;
					case '\r':
						text = "\\r";
						break;
					case '\f':
						text = "\\f";
						break;
					case '\b':
						text = "\\b";
						break;
					case '\\':
						text = "\\\\";
						break;
					case '\u0085':
						text = "\\u0085";
						break;
					case '\u2028':
						text = "\\u2028";
						break;
					case '\u2029':
						text = "\\u2029";
						break;
					default:
						if (c < charEscapeFlags.Length || stringEscapeHandling == StringEscapeHandling.EscapeNonAscii)
						{
							if (c == '\'' && stringEscapeHandling != StringEscapeHandling.EscapeHtml)
							{
								text = "\\'";
								break;
							}
							if (c == '"' && stringEscapeHandling != StringEscapeHandling.EscapeHtml)
							{
								text = "\\\"";
								break;
							}
							if (writeBuffer == null || writeBuffer.Length < 6)
							{
								writeBuffer = BufferUtils.EnsureBufferSize(bufferPool, 6, writeBuffer);
							}
							StringUtils.ToCharAsUnicode(c, writeBuffer);
							text = "!";
						}
						else
						{
							text = null;
						}
						break;
					}
					if (text == null)
					{
						continue;
					}
					bool flag = string.Equals(text, "!");
					if (i > num)
					{
						num2 = i - num + (flag ? 6 : 0);
						int num3 = (flag ? 6 : 0);
						if (writeBuffer == null || writeBuffer.Length < num2)
						{
							char[] array = BufferUtils.RentBuffer(bufferPool, num2);
							if (flag)
							{
								Array.Copy(writeBuffer, array, 6);
							}
							BufferUtils.ReturnBuffer(bufferPool, writeBuffer);
							writeBuffer = array;
						}
						s.CopyTo(num, writeBuffer, num3, num2 - num3);
						writer.Write(writeBuffer, num3, num2 - num3);
					}
					num = i + 1;
					if (!flag)
					{
						writer.Write(text);
					}
					else
					{
						writer.Write(writeBuffer, 0, 6);
					}
				}
				num2 = s.Length - num;
				if (num2 > 0)
				{
					if (writeBuffer == null || writeBuffer.Length < num2)
					{
						writeBuffer = BufferUtils.EnsureBufferSize(bufferPool, num2, writeBuffer);
					}
					s.CopyTo(num, writeBuffer, 0, num2);
					writer.Write(writeBuffer, 0, num2);
				}
			}
		}
		if (appendDelimiters)
		{
			writer.Write(delimiter);
		}
	}

	public static string ToEscapedJavaScriptString(string value, char delimiter, bool appendDelimiters, StringEscapeHandling stringEscapeHandling)
	{
		bool[] charEscapeFlags = GetCharEscapeFlags(stringEscapeHandling, delimiter);
		using StringWriter stringWriter = StringUtils.CreateStringWriter(value?.Length ?? 16);
		char[] writeBuffer = null;
		WriteEscapedJavaScriptString(stringWriter, value, delimiter, appendDelimiters, charEscapeFlags, stringEscapeHandling, null, ref writeBuffer);
		return stringWriter.ToString();
	}

	private static int FirstCharToEscape(string s, bool[] charEscapeFlags, StringEscapeHandling stringEscapeHandling)
	{
		for (int i = 0; i != s.Length; i++)
		{
			char c = s[i];
			if (c < charEscapeFlags.Length)
			{
				if (charEscapeFlags[(uint)c])
				{
					return i;
				}
				continue;
			}
			if (stringEscapeHandling == StringEscapeHandling.EscapeNonAscii)
			{
				return i;
			}
			if (c == '\u0085' || c == '\u2028' || c == '\u2029')
			{
				return i;
			}
		}
		return -1;
	}

	public static Task WriteEscapedJavaScriptStringAsync(TextWriter writer, string s, char delimiter, bool appendDelimiters, bool[] charEscapeFlags, StringEscapeHandling stringEscapeHandling, JsonTextWriter client, char[] writeBuffer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		if (appendDelimiters)
		{
			return WriteEscapedJavaScriptStringWithDelimitersAsync(writer, s, delimiter, charEscapeFlags, stringEscapeHandling, client, writeBuffer, cancellationToken);
		}
		if (string.IsNullOrEmpty(s))
		{
			return cancellationToken.CancelIfRequestedAsync() ?? AsyncUtils.CompletedTask;
		}
		return WriteEscapedJavaScriptStringWithoutDelimitersAsync(writer, s, charEscapeFlags, stringEscapeHandling, client, writeBuffer, cancellationToken);
	}

	private static Task WriteEscapedJavaScriptStringWithDelimitersAsync(TextWriter writer, string s, char delimiter, bool[] charEscapeFlags, StringEscapeHandling stringEscapeHandling, JsonTextWriter client, char[] writeBuffer, CancellationToken cancellationToken)
	{
		Task task = writer.WriteAsync(delimiter, cancellationToken);
		if (task.Status != TaskStatus.RanToCompletion)
		{
			return WriteEscapedJavaScriptStringWithDelimitersAsync(task, writer, s, delimiter, charEscapeFlags, stringEscapeHandling, client, writeBuffer, cancellationToken);
		}
		if (!string.IsNullOrEmpty(s))
		{
			task = WriteEscapedJavaScriptStringWithoutDelimitersAsync(writer, s, charEscapeFlags, stringEscapeHandling, client, writeBuffer, cancellationToken);
			if (task.Status == TaskStatus.RanToCompletion)
			{
				return writer.WriteAsync(delimiter, cancellationToken);
			}
		}
		return WriteCharAsync(task, writer, delimiter, cancellationToken);
	}

	private static async Task WriteEscapedJavaScriptStringWithDelimitersAsync(Task task, TextWriter writer, string s, char delimiter, bool[] charEscapeFlags, StringEscapeHandling stringEscapeHandling, JsonTextWriter client, char[] writeBuffer, CancellationToken cancellationToken)
	{
		await task.ConfigureAwait(continueOnCapturedContext: false);
		if (!string.IsNullOrEmpty(s))
		{
			await WriteEscapedJavaScriptStringWithoutDelimitersAsync(writer, s, charEscapeFlags, stringEscapeHandling, client, writeBuffer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		await writer.WriteAsync(delimiter).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task WriteCharAsync(Task task, TextWriter writer, char c, CancellationToken cancellationToken)
	{
		await task.ConfigureAwait(continueOnCapturedContext: false);
		await writer.WriteAsync(c, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private static Task WriteEscapedJavaScriptStringWithoutDelimitersAsync(TextWriter writer, string s, bool[] charEscapeFlags, StringEscapeHandling stringEscapeHandling, JsonTextWriter client, char[] writeBuffer, CancellationToken cancellationToken)
	{
		int num = FirstCharToEscape(s, charEscapeFlags, stringEscapeHandling);
		if (num != -1)
		{
			return WriteDefinitelyEscapedJavaScriptStringWithoutDelimitersAsync(writer, s, num, charEscapeFlags, stringEscapeHandling, client, writeBuffer, cancellationToken);
		}
		return writer.WriteAsync(s, cancellationToken);
	}

	private static async Task WriteDefinitelyEscapedJavaScriptStringWithoutDelimitersAsync(TextWriter writer, string s, int lastWritePosition, bool[] charEscapeFlags, StringEscapeHandling stringEscapeHandling, JsonTextWriter client, char[] writeBuffer, CancellationToken cancellationToken)
	{
		if (writeBuffer == null || writeBuffer.Length < lastWritePosition)
		{
			writeBuffer = client.EnsureWriteBuffer(lastWritePosition, 6);
		}
		if (lastWritePosition != 0)
		{
			s.CopyTo(0, writeBuffer, 0, lastWritePosition);
			await writer.WriteAsync(writeBuffer, 0, lastWritePosition, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		bool isEscapedUnicodeText = false;
		string escapedValue = null;
		int num;
		for (int i = lastWritePosition; i < s.Length; i++)
		{
			char c = s[i];
			if (c < charEscapeFlags.Length && !charEscapeFlags[(uint)c])
			{
				continue;
			}
			switch (c)
			{
			case '\t':
				escapedValue = "\\t";
				break;
			case '\n':
				escapedValue = "\\n";
				break;
			case '\r':
				escapedValue = "\\r";
				break;
			case '\f':
				escapedValue = "\\f";
				break;
			case '\b':
				escapedValue = "\\b";
				break;
			case '\\':
				escapedValue = "\\\\";
				break;
			case '\u0085':
				escapedValue = "\\u0085";
				break;
			case '\u2028':
				escapedValue = "\\u2028";
				break;
			case '\u2029':
				escapedValue = "\\u2029";
				break;
			default:
				if (c >= charEscapeFlags.Length && stringEscapeHandling != StringEscapeHandling.EscapeNonAscii)
				{
					continue;
				}
				if (c == '\'' && stringEscapeHandling != StringEscapeHandling.EscapeHtml)
				{
					escapedValue = "\\'";
					break;
				}
				if (c == '"' && stringEscapeHandling != StringEscapeHandling.EscapeHtml)
				{
					escapedValue = "\\\"";
					break;
				}
				if (writeBuffer.Length < 6)
				{
					writeBuffer = client.EnsureWriteBuffer(6, 0);
				}
				StringUtils.ToCharAsUnicode(c, writeBuffer);
				isEscapedUnicodeText = true;
				break;
			}
			if (i > lastWritePosition)
			{
				num = i - lastWritePosition + (isEscapedUnicodeText ? 6 : 0);
				int num2 = (isEscapedUnicodeText ? 6 : 0);
				if (writeBuffer.Length < num)
				{
					writeBuffer = client.EnsureWriteBuffer(num, 6);
				}
				s.CopyTo(lastWritePosition, writeBuffer, num2, num - num2);
				await writer.WriteAsync(writeBuffer, num2, num - num2, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			lastWritePosition = i + 1;
			if (!isEscapedUnicodeText)
			{
				await writer.WriteAsync(escapedValue, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				continue;
			}
			await writer.WriteAsync(writeBuffer, 0, 6, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			isEscapedUnicodeText = false;
		}
		num = s.Length - lastWritePosition;
		if (num != 0)
		{
			if (writeBuffer.Length < num)
			{
				writeBuffer = client.EnsureWriteBuffer(num, 0);
			}
			s.CopyTo(lastWritePosition, writeBuffer, 0, num);
			await writer.WriteAsync(writeBuffer, 0, num, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}
}
