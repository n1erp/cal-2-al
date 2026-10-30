namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal struct TypeParameterDiagnosticInfo(TypeParameterSymbol typeParameter, DiagnosticInfo diagnosticInfo)
{
	public readonly TypeParameterSymbol TypeParameter = typeParameter;

	public readonly DiagnosticInfo DiagnosticInfo = diagnosticInfo;
}
