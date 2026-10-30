using System;
using System.Diagnostics;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Emit;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal struct EncHoistedLocalInfo : IEquatable<EncHoistedLocalInfo>
{
	public readonly LocalSlotDebugInfo SlotInfo;

	public readonly ITypeReference Type;

	public bool IsUnused => Type == null;

	public EncHoistedLocalInfo(bool ignored)
	{
		SlotInfo = new LocalSlotDebugInfo(SynthesizedLocalKind.EmitterTemp, LocalDebugId.None);
		Type = null;
	}

	public EncHoistedLocalInfo(LocalSlotDebugInfo slotInfo, ITypeReference type)
	{
		SlotInfo = slotInfo;
		Type = type;
	}

	public bool Equals(EncHoistedLocalInfo other)
	{
		if (SlotInfo.Equals(other.SlotInfo))
		{
			return Type.Equals(other.Type);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is EncHoistedLocalInfo)
		{
			return Equals((EncHoistedLocalInfo)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Type, SlotInfo.GetHashCode());
	}

	private string GetDebuggerDisplay()
	{
		if (IsUnused)
		{
			return "[invalid]";
		}
		return $"[Id={SlotInfo.Id}, SynthesizedKind={SlotInfo.SynthesizedKind}, Type={Type}]";
	}
}
