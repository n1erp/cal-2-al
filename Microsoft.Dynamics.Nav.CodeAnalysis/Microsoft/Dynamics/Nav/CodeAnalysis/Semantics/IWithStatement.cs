namespace Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;

public interface IWithStatement : IOperation
{
	IOperation Body { get; }

	IOperation Value { get; }
}
