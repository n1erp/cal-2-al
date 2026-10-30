namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IPagePartAccess : IOperation
{
	IOperation Instance { get; }

	IControlSymbol ControlSymbol { get; }

	IApplicationObjectTypeSymbol PageSymbol { get; }
}
