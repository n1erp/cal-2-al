namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface ITestFieldAccess : IOperation
{
	IOperation Instance { get; }

	IControlSymbol ControlSymbol { get; }
}
