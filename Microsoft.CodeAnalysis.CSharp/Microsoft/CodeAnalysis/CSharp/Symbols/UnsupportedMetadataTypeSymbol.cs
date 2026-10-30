using System;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class UnsupportedMetadataTypeSymbol : ErrorTypeSymbol
{
	private readonly BadImageFormatException _mrEx;

	internal override DiagnosticInfo ErrorInfo => new CSDiagnosticInfo(ErrorCode.ERR_BogusType, string.Empty);

	internal override bool MangleName => false;

	internal UnsupportedMetadataTypeSymbol(BadImageFormatException mrEx = null)
	{
		_mrEx = mrEx;
	}
}
