using System;
using System.Diagnostics;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Emit;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal struct EncLocalInfo : IEquatable<EncLocalInfo>
{
	public readonly LocalSlotDebugInfo SlotInfo;

	public readonly ITypeReference Type;

	public readonly LocalSlotConstraints Constraints;

	public readonly byte[] Signature;

	public readonly bool isUnused;

	public bool IsDefault
	{
		get
		{
			if (Type == null)
			{
				return Signature == null;
			}
			return false;
		}
	}

	public bool IsUnused => isUnused;

	public EncLocalInfo(byte[] signature)
	{
		SlotInfo = new LocalSlotDebugInfo(SynthesizedLocalKind.EmitterTemp, LocalDebugId.None);
		Type = null;
		Constraints = LocalSlotConstraints.None;
		Signature = signature;
		isUnused = true;
	}

	public EncLocalInfo(LocalSlotDebugInfo slotInfo, ITypeReference type, LocalSlotConstraints constraints, byte[] signature)
	{
		SlotInfo = slotInfo;
		Type = type;
		Constraints = constraints;
		Signature = signature;
		isUnused = false;
	}

	public bool Equals(EncLocalInfo other)
	{
		if (SlotInfo.Equals(other.SlotInfo) && Type.Equals(other.Type) && Constraints == other.Constraints)
		{
			return isUnused == other.isUnused;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is EncLocalInfo)
		{
			return Equals((EncLocalInfo)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(SlotInfo.GetHashCode(), Hash.Combine(Type.GetHashCode(), Hash.Combine((int)Constraints, Hash.Combine(isUnused, 0))));
	}

	private string GetDebuggerDisplay()
	{
		if (IsDefault)
		{
			return "[default]";
		}
		if (isUnused)
		{
			return "[invalid]";
		}
		return string.Format("[Id={0}, SynthesizedKind={1}, Type={2}, Constraints={3}, Sig={4}]", SlotInfo.Id, SlotInfo.SynthesizedKind, Type, Constraints, (Signature != null) ? BitConverter.ToString(Signature) : "null");
	}
}
