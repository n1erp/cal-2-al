namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal interface IObjectAccess : IOperation
{
	IOperation Instance { get; }

	IObjectTypeSymbol ObjectTypeSymbol { get; }
}
