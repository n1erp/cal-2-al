namespace Microsoft.CodeAnalysis.Debugging;

internal enum CustomDebugInfoKind : byte
{
	UsingGroups,
	ForwardMethodInfo,
	ForwardModuleInfo,
	StateMachineHoistedLocalScopes,
	StateMachineTypeName,
	DynamicLocals,
	EditAndContinueLocalSlotMap,
	EditAndContinueLambdaMap,
	TupleElementNames
}
