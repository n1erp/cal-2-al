namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface ITestActionAccess : IOperation
{
	IOperation Instance { get; }

	IActionSymbol ActionSymbol { get; }
}
