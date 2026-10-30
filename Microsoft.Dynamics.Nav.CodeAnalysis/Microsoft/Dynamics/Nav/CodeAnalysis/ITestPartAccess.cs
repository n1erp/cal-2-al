namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface ITestPartAccess : IOperation
{
	IOperation Instance { get; }

	IControlSymbol ControlSymbol { get; }
}
