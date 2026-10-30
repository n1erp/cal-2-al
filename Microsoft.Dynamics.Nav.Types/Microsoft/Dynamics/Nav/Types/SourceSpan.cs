using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract]
[DebuggerDisplay("({from.line}, {from.column}) => ({to.line}, {to.column})")]
[JsonObject]
public struct SourceSpan(SourcePosition from, SourcePosition to) : IComparable<SourceSpan>
{
	[DataMember]
	private SourcePosition to = to;

	[DataMember]
	private SourcePosition from = from;

	[JsonProperty]
	public SourcePosition To => to;

	[JsonProperty]
	public SourcePosition From => from;

	public int CompareTo(SourceSpan other)
	{
		return ((ulong)this).CompareTo((ulong)other);
	}

	public bool Equals(SourceSpan other)
	{
		return (ulong)this == (ulong)other;
	}

	public override bool Equals(object obj)
	{
		if (obj is SourceSpan)
		{
			return Equals((SourceSpan)obj);
		}
		return false;
	}

	public static bool operator ==(SourceSpan spanA, SourceSpan spanB)
	{
		return (ulong)spanA == (ulong)spanB;
	}

	public static bool operator !=(SourceSpan spanA, SourceSpan spanB)
	{
		return (ulong)spanA != (ulong)spanB;
	}

	public static bool operator >(SourceSpan spanA, SourceSpan spanB)
	{
		return (ulong)spanA > (ulong)spanB;
	}

	public static bool operator <(SourceSpan spanA, SourceSpan spanB)
	{
		return (ulong)spanA < (ulong)spanB;
	}

	public static bool operator >=(SourceSpan spanA, SourceSpan spanB)
	{
		return (ulong)spanA >= (ulong)spanB;
	}

	public static bool operator <=(SourceSpan spanA, SourceSpan spanB)
	{
		return (ulong)spanA <= (ulong)spanB;
	}

	public static explicit operator ulong(SourceSpan span)
	{
		return ToUInt64(span);
	}

	public unsafe static ulong ToUInt64(SourceSpan span)
	{
		return *(ulong*)(&span);
	}

	public static explicit operator SourceSpan(ulong value)
	{
		return FromUInt64(value);
	}

	public unsafe static SourceSpan FromUInt64(ulong value)
	{
		return *(SourceSpan*)(&value);
	}

	public static explicit operator SourceSpan(long value)
	{
		return ToInt64(value);
	}

	public unsafe static SourceSpan ToInt64(long value)
	{
		return *(SourceSpan*)(&value);
	}

	public override int GetHashCode()
	{
		return ((ulong)this).GetHashCode();
	}
}
