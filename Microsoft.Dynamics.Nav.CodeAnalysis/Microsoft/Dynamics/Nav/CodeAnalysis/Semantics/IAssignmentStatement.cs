namespace Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;

public interface IAssignmentStatement : IOperation
{
	IOperation Target { get; }

	IOperation Value { get; }
}
