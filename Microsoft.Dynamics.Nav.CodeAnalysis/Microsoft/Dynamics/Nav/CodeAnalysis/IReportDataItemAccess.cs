namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IReportDataItemAccess : IOperation
{
	IOperation Instance { get; }

	IReportDataItemSymbol DataItemSymbol { get; }
}
