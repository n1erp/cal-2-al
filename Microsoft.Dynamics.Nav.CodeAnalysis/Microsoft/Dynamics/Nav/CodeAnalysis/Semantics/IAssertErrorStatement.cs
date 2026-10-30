namespace Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;

public interface IAssertErrorStatement : IOperation
{
	IOperation Statement { get; }
}
