namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface ITestFilterAccess : IOperation
{
	IOperation Instance { get; }

	ISymbol TableSymbol { get; }
}
