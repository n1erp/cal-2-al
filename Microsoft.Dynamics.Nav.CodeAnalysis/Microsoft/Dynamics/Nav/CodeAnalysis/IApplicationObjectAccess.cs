namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IApplicationObjectAccess : IOperation
{
	IOperation Instance { get; }

	IApplicationObjectTypeSymbol ApplicationObjectTypeSymbol { get; }
}
