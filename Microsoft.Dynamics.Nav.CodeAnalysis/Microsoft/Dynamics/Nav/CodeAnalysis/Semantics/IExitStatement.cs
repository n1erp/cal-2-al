namespace Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;

public interface IExitStatement : IOperation
{
	IOperation ReturnedValue { get; }
}
