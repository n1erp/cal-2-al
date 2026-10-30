using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class DiagnosticInfoWithSymbols : DiagnosticInfo
{
	internal readonly ImmutableArray<Symbol> Symbols;

	internal DiagnosticInfoWithSymbols(ErrorCode errorCode, object[] arguments, ImmutableArray<Symbol> symbols)
		: base(Microsoft.Dynamics.Nav.CodeAnalysis.MessageProvider.Instance, (int)errorCode, arguments)
	{
		Symbols = symbols;
	}

	internal DiagnosticInfoWithSymbols(bool isWarningAsError, ErrorCode errorCode, object[] arguments, ImmutableArray<Symbol> symbols)
		: base(Microsoft.Dynamics.Nav.CodeAnalysis.MessageProvider.Instance, isWarningAsError, (int)errorCode, arguments)
	{
		Symbols = symbols;
	}
}
