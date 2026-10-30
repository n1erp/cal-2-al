using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class TextPosition : IComparable<TextPosition>
{
	internal static readonly TextPosition Undefined = new TextPosition(0, 0, 0, 0);

	public int StartLine { get; }

	public int StartCol { get; }

	public int EndLine { get; }

	public int EndCol { get; }

	public TextPosition(int startLine, int startCol, int endLine, int endCol)
	{
		StartLine = startLine;
		StartCol = startCol;
		EndLine = endLine;
		EndCol = endCol;
	}

	public static bool operator ==(TextPosition left, TextPosition right)
	{
		if ((object)left == null && (object)right == null)
		{
			return true;
		}
		if ((object)left == null)
		{
			return false;
		}
		if ((object)right == null)
		{
			return false;
		}
		if (left.StartCol == right.StartCol && left.StartLine == right.StartLine && left.EndCol == right.EndCol)
		{
			return left.EndLine == right.EndLine;
		}
		return false;
	}

	public static bool operator !=(TextPosition left, TextPosition right)
	{
		return !(left == right);
	}

	public static bool operator >(TextPosition left, TextPosition right)
	{
		return left.CompareTo(right) > 0;
	}

	public static bool operator <(TextPosition left, TextPosition right)
	{
		return left.CompareTo(right) < 0;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "[({0},{1}),({2},{3})]", StartLine, StartCol, EndLine, EndCol);
	}

	public override int GetHashCode()
	{
		return StartLine.GetHashCode() ^ StartCol.GetHashCode() ^ EndLine.GetHashCode() ^ EndCol.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		TextPosition textPosition = obj as TextPosition;
		if (textPosition != null)
		{
			return textPosition == this;
		}
		return false;
	}

	public int CompareTo(TextPosition other)
	{
		if ((object)other == null)
		{
			return 1;
		}
		int num = Comparer<int>.Default.Compare(StartLine, other.StartLine);
		if (num == 0)
		{
			num = Comparer<int>.Default.Compare(StartCol, other.StartCol);
			if (num == 0)
			{
				num = Comparer<int>.Default.Compare(EndLine, other.EndLine);
				if (num == 0)
				{
					num = Comparer<int>.Default.Compare(EndCol, other.EndCol);
				}
			}
		}
		return num;
	}
}
