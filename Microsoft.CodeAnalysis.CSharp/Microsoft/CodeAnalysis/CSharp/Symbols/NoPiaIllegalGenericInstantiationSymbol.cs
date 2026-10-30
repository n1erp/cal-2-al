using System.Runtime.CompilerServices;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal class NoPiaIllegalGenericInstantiationSymbol : ErrorTypeSymbol
{
	private readonly ModuleSymbol _exposingModule;

	private readonly NamedTypeSymbol _underlyingSymbol;

	internal override bool MangleName => false;

	public NamedTypeSymbol UnderlyingSymbol => _underlyingSymbol;

	internal override DiagnosticInfo ErrorInfo
	{
		get
		{
			if (_underlyingSymbol.IsErrorType())
			{
				DiagnosticInfo errorInfo = ((ErrorTypeSymbol)_underlyingSymbol).ErrorInfo;
				if (errorInfo != null)
				{
					return errorInfo;
				}
			}
			return new CSDiagnosticInfo(ErrorCode.ERR_GenericsUsedAcrossAssemblies, _underlyingSymbol, _exposingModule.ContainingAssembly);
		}
	}

	public NoPiaIllegalGenericInstantiationSymbol(ModuleSymbol exposingModule, NamedTypeSymbol underlyingSymbol)
	{
		_exposingModule = exposingModule;
		_underlyingSymbol = underlyingSymbol;
	}

	public override int GetHashCode()
	{
		return RuntimeHelpers.GetHashCode(this);
	}

	internal override bool Equals(TypeSymbol t2, TypeCompareKind comparison)
	{
		return (object)this == t2;
	}
}
