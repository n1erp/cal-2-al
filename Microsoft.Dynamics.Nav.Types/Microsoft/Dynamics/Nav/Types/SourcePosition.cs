using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract]
[DebuggerDisplay("({line}, {column})")]
[JsonObject]
public struct SourcePosition(ushort line, ushort column) : IEquatable<SourcePosition>, IComparable<SourcePosition>
{
	[DataMember]
	private readonly ushort column = column;

	[DataMember]
	private readonly ushort line = line;

	[JsonProperty]
	public ushort Line => line;

	[JsonProperty]
	public ushort Column => column;

	public int CompareTo(SourcePosition other)
	{
		return ((uint)this).CompareTo((uint)other);
	}

	public bool Equals(SourcePosition other)
	{
		return (uint)this == (uint)other;
	}

	public override bool Equals(object obj)
	{
		if (obj is SourcePosition)
		{
			return Equals((SourcePosition)obj);
		}
		return false;
	}

	public static bool operator ==(SourcePosition positionA, SourcePosition positionB)
	{
		return (uint)positionA == (uint)positionB;
	}

	public static bool operator !=(SourcePosition positionA, SourcePosition positionB)
	{
		return (uint)positionA != (uint)positionB;
	}

	public static bool operator >(SourcePosition positionA, SourcePosition positionB)
	{
		return (uint)positionA > (uint)positionB;
	}

	public static bool operator <(SourcePosition positionA, SourcePosition positionB)
	{
		return (uint)positionA < (uint)positionB;
	}

	public static bool operator >=(SourcePosition positionA, SourcePosition positionB)
	{
		return (uint)positionA >= (uint)positionB;
	}

	public static bool operator <=(SourcePosition positionA, SourcePosition positionB)
	{
		return (uint)positionA <= (uint)positionB;
	}

	public static explicit operator uint(SourcePosition position)
	{
		return ToUInt32(position);
	}

	public unsafe static uint ToUInt32(SourcePosition position)
	{
		return *(uint*)(&position);
	}

	public override int GetHashCode()
	{
		return ((uint)this).GetHashCode();
	}
}
