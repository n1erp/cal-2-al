using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal class ChangeSymbolInformation : IChangeSymbolInformation
{
	internal Symbol TargetSymbol { get; }

	internal Symbol ParentSymbol { get; }

	internal ChangeSymbol ChangeSymbol { get; }

	internal Location DiagnosticLocation => LazyDiagnosticLocation.Value;

	private Lazy<Location> LazyDiagnosticLocation { get; }

	ISymbol IChangeSymbolInformation.TargetSymbol => TargetSymbol;

	ISymbol IChangeSymbolInformation.ParentSymbol => ParentSymbol;

	IChangeSymbol IChangeSymbolInformation.ChangeSymbol => ChangeSymbol;

	internal ChangeSymbolInformation(Symbol targetSymbol, Symbol parentSymbol, ChangeSymbol changeSymbol)
	{
		TargetSymbol = targetSymbol;
		ParentSymbol = parentSymbol;
		ChangeSymbol = changeSymbol;
		LazyDiagnosticLocation = new Lazy<Location>(GetChangeSymbolLocation);
	}

	private Location GetChangeSymbolLocation()
	{
		if (ChangeSymbol.ChangeKind.IsMove())
		{
			return ChangeSymbolHelpers.GetMovedSymbolSyntax((SourceChangeMoveSymbol)ChangeSymbol, TargetSymbol).GetLocation();
		}
		return TargetSymbol.GetLocation();
	}

	Location IChangeSymbolInformation.GetDiagnosticLocation()
	{
		return LazyDiagnosticLocation.Value;
	}
}
