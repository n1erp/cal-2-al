namespace Microsoft.CodeAnalysis.FlowAnalysis;

public enum ControlFlowBranchSemantics
{
	None,
	Regular,
	Return,
	StructuredExceptionHandling,
	ProgramTermination,
	Throw,
	Rethrow,
	Error
}
