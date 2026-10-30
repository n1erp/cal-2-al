using System;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Roslyn.Utilities;

internal sealed class JsonWriter : IDisposable
{
	private enum Pending
	{
		None,
		NewLineAndIndent,
		CommaNewLineAndIndent
	}

	private readonly TextWriter _output;

	private int _indent;

	private Pending _pending;

	private const string Indentation = "  ";

	public JsonWriter(TextWriter output)
	{
		_output = output;
		_pending = Pending.None;
	}

	public void WriteObjectStart()
	{
		WriteStart('{');
	}

	public void WriteObjectStart(string key)
	{
		WriteKey(key);
		WriteObjectStart();
	}

	public void WriteObjectEnd()
	{
		WriteEnd('}');
	}

	public void WriteArrayStart()
	{
		WriteStart('[');
	}

	public void WriteArrayStart(string key)
	{
		WriteKey(key);
		WriteArrayStart();
	}

	public void WriteArrayEnd()
	{
		WriteEnd(']');
	}

	public void WriteKey(string key)
	{
		Write(key);
		_output.Write(": ");
		_pending = Pending.None;
	}

	public void Write(string key, string value)
	{
		WriteKey(key);
		Write(value);
	}

	public void Write(string key, int value)
	{
		WriteKey(key);
		Write(value);
	}

	public void Write(string key, bool value)
	{
		WriteKey(key);
		Write(value);
	}

	public void Write(string value)
	{
		WritePending();
		_output.Write('"');
		_output.Write(EscapeString(value));
		_output.Write('"');
		_pending = Pending.CommaNewLineAndIndent;
	}

	public void Write(int value)
	{
		WritePending();
		_output.Write(value.ToString(CultureInfo.InvariantCulture));
		_pending = Pending.CommaNewLineAndIndent;
	}

	public void Write(bool value)
	{
		WritePending();
		_output.Write(value ? "true" : "false");
		_pending = Pending.CommaNewLineAndIndent;
	}

	private void WritePending()
	{
		if (_pending != Pending.None)
		{
			if (_pending == Pending.CommaNewLineAndIndent)
			{
				_output.Write(',');
			}
			_output.WriteLine();
			for (int i = 0; i < _indent; i++)
			{
				_output.Write("  ");
			}
		}
	}

	private void WriteStart(char c)
	{
		WritePending();
		_output.Write(c);
		_pending = Pending.NewLineAndIndent;
		_indent++;
	}

	private void WriteEnd(char c)
	{
		_pending = Pending.NewLineAndIndent;
		_indent--;
		WritePending();
		_output.Write(c);
		_pending = Pending.CommaNewLineAndIndent;
	}

	public void Dispose()
	{
		_output.Dispose();
	}

	private static string EscapeString(string value)
	{
		PooledStringBuilder pooledStringBuilder = null;
		StringBuilder stringBuilder = null;
		if (string.IsNullOrEmpty(value))
		{
			return string.Empty;
		}
		int startIndex = 0;
		int num = 0;
		for (int i = 0; i < value.Length; i++)
		{
			char c = value[i];
			if (c == '"' || c == '\\' || ShouldAppendAsUnicode(c))
			{
				if (stringBuilder == null)
				{
					pooledStringBuilder = PooledStringBuilder.GetInstance();
					stringBuilder = pooledStringBuilder.Builder;
				}
				if (num > 0)
				{
					stringBuilder.Append(value, startIndex, num);
				}
				startIndex = i + 1;
				num = 0;
			}
			switch (c)
			{
			case '"':
				stringBuilder.Append("\\\"");
				continue;
			case '\\':
				stringBuilder.Append("\\\\");
				continue;
			}
			if (ShouldAppendAsUnicode(c))
			{
				AppendCharAsUnicode(stringBuilder, c);
			}
			else
			{
				num++;
			}
		}
		if (stringBuilder == null)
		{
			return value;
		}
		if (num > 0)
		{
			stringBuilder.Append(value, startIndex, num);
		}
		return pooledStringBuilder.ToStringAndFree();
	}

	private static void AppendCharAsUnicode(StringBuilder builder, char c)
	{
		builder.Append("\\u");
		builder.AppendFormat(CultureInfo.InvariantCulture, "{0:x4}", (int)c);
	}

	private static bool ShouldAppendAsUnicode(char c)
	{
		if (c >= ' ' && c < '\ufffe' && (c < '\ud800' || c > '\udfff'))
		{
			if (c != '\u0085' && c != '\u2028')
			{
				return c == '\u2029';
			}
			return true;
		}
		return true;
	}
}
