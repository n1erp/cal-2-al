using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

public interface IChangeSymbolInformation
{
	ISymbol TargetSymbol { get; }

	ISymbol ParentSymbol { get; }

	IChangeSymbol ChangeSymbol { get; }

	Location GetDiagnosticLocation();
}
