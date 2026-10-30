namespace Microsoft.CodeAnalysis.CodeGen;

internal enum ScopeType
{
	Variable,
	TryCatchFinally,
	Try,
	Catch,
	Filter,
	Finally,
	Fault,
	StateMachineVariable
}
