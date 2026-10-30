namespace Microsoft.CodeAnalysis.FlowAnalysis;

public enum ControlFlowRegionKind
{
	Root,
	LocalLifetime,
	Try,
	Filter,
	Catch,
	FilterAndHandler,
	TryAndCatch,
	Finally,
	TryAndFinally,
	StaticLocalInitializer,
	ErroneousBody
}
