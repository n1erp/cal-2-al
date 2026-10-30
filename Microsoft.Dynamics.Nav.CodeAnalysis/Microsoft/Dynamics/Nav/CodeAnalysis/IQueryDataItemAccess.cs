namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IQueryDataItemAccess : IOperation
{
	IOperation Instance { get; }

	IQueryDataItemSymbol DataItemSymbol { get; }
}
