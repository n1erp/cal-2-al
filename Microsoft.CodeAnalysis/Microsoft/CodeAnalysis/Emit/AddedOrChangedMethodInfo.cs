using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;

namespace Microsoft.CodeAnalysis.Emit;

internal struct AddedOrChangedMethodInfo(DebugId methodId, ImmutableArray<EncLocalInfo> locals, ImmutableArray<LambdaDebugInfo> lambdaDebugInfo, ImmutableArray<ClosureDebugInfo> closureDebugInfo, string stateMachineTypeNameOpt, ImmutableArray<EncHoistedLocalInfo> stateMachineHoistedLocalSlotsOpt, ImmutableArray<ITypeReference> stateMachineAwaiterSlotsOpt)
{
	public readonly DebugId MethodId = methodId;

	public readonly ImmutableArray<EncLocalInfo> Locals = locals;

	public readonly ImmutableArray<LambdaDebugInfo> LambdaDebugInfo = lambdaDebugInfo;

	public readonly ImmutableArray<ClosureDebugInfo> ClosureDebugInfo = closureDebugInfo;

	public readonly string StateMachineTypeNameOpt = stateMachineTypeNameOpt;

	public readonly ImmutableArray<EncHoistedLocalInfo> StateMachineHoistedLocalSlotsOpt = stateMachineHoistedLocalSlotsOpt;

	public readonly ImmutableArray<ITypeReference> StateMachineAwaiterSlotsOpt = stateMachineAwaiterSlotsOpt;

	public AddedOrChangedMethodInfo MapTypes(SymbolMatcher map)
	{
		ImmutableArray<EncLocalInfo> locals = ImmutableArray.CreateRange(Locals, MapLocalInfo, map);
		ImmutableArray<EncHoistedLocalInfo> stateMachineHoistedLocalSlotsOpt = (StateMachineHoistedLocalSlotsOpt.IsDefault ? StateMachineHoistedLocalSlotsOpt : ImmutableArray.CreateRange(StateMachineHoistedLocalSlotsOpt, MapHoistedLocalSlot, map));
		ImmutableArray<ITypeReference> stateMachineAwaiterSlotsOpt = (StateMachineAwaiterSlotsOpt.IsDefault ? StateMachineAwaiterSlotsOpt : ImmutableArray.CreateRange(StateMachineAwaiterSlotsOpt, map.MapReference));
		return new AddedOrChangedMethodInfo(MethodId, locals, LambdaDebugInfo, ClosureDebugInfo, StateMachineTypeNameOpt, stateMachineHoistedLocalSlotsOpt, stateMachineAwaiterSlotsOpt);
	}

	private static EncLocalInfo MapLocalInfo(EncLocalInfo info, SymbolMatcher map)
	{
		if (info.IsUnused)
		{
			return info;
		}
		return new EncLocalInfo(info.SlotInfo, map.MapReference(info.Type), info.Constraints, info.Signature);
	}

	private static EncHoistedLocalInfo MapHoistedLocalSlot(EncHoistedLocalInfo info, SymbolMatcher map)
	{
		if (info.IsUnused)
		{
			return info;
		}
		return new EncHoistedLocalInfo(info.SlotInfo, map.MapReference(info.Type));
	}
}
