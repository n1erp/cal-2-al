using System;

namespace Microsoft.CodeAnalysis.FlowAnalysis;

public struct CaptureId : IEquatable<CaptureId>
{
	internal int Value { get; }

	internal CaptureId(int value)
	{
		Value = value;
	}

	public bool Equals(CaptureId other)
	{
		return Value == other.Value;
	}

	public override bool Equals(object obj)
	{
		if (obj is CaptureId)
		{
			return Equals((CaptureId)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}
}
