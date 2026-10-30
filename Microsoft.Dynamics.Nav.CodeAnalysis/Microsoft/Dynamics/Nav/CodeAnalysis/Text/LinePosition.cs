using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Text;

public struct LinePosition : IEquatable<LinePosition>, IComparable<LinePosition>
{
	public static readonly LinePosition Zero = new LinePosition(0, 0);

	public int Line { get; }

	public int Character { get; }

	public LinePosition(int line, int character)
	{
		if (line < 0)
		{
			throw new ArgumentOutOfRangeException("line");
		}
		if (character < 0)
		{
			throw new ArgumentOutOfRangeException("character");
		}
		Line = line;
		Character = character;
	}

	internal LinePosition(int character)
	{
		if (character < 0)
		{
			throw new ArgumentOutOfRangeException("character");
		}
		Line = -1;
		Character = character;
	}

	public static bool operator ==(LinePosition left, LinePosition right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(LinePosition left, LinePosition right)
	{
		return !left.Equals(right);
	}

	public bool Equals(LinePosition other)
	{
		if (other.Line == Line)
		{
			return other.Character == Character;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is LinePosition)
		{
			return Equals((LinePosition)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (Line << 8) ^ Character;
	}

	public override string ToString()
	{
		return Line + "," + Character;
	}

	public int CompareTo(LinePosition other)
	{
		int num = Line.CompareTo(other.Line);
		if (num == 0)
		{
			return Character.CompareTo(other.Character);
		}
		return num;
	}

	public static bool operator >(LinePosition left, LinePosition right)
	{
		return left.CompareTo(right) > 0;
	}

	public static bool operator >=(LinePosition left, LinePosition right)
	{
		return left.CompareTo(right) >= 0;
	}

	public static bool operator <(LinePosition left, LinePosition right)
	{
		return left.CompareTo(right) < 0;
	}

	public static bool operator <=(LinePosition left, LinePosition right)
	{
		return left.CompareTo(right) <= 0;
	}
}
