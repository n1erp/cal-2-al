using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp;

internal class DiagnosticInfoWithSymbols : DiagnosticInfo
{
	internal readonly ImmutableArray<Symbol> Symbols;

	internal DiagnosticInfoWithSymbols(ErrorCode errorCode, object[] arguments, ImmutableArray<Symbol> symbols)
		: base(Microsoft.CodeAnalysis.CSharp.MessageProvider.Instance, (int)errorCode, arguments)
	{
		Symbols = symbols;
	}

	internal DiagnosticInfoWithSymbols(bool isWarningAsError, ErrorCode errorCode, object[] arguments, ImmutableArray<Symbol> symbols)
		: base(Microsoft.CodeAnalysis.CSharp.MessageProvider.Instance, isWarningAsError, (int)errorCode, arguments)
	{
		Symbols = symbols;
	}
}
