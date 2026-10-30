using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class LazyObsoleteDiagnosticInfo : DiagnosticInfo
{
	private DiagnosticInfo _lazyActualObsoleteDiagnostic;

	private readonly Symbol _symbol;

	private readonly Symbol _containingSymbol;

	private readonly BinderFlags _binderFlags;

	internal LazyObsoleteDiagnosticInfo(Symbol symbol, Symbol containingSymbol, BinderFlags binderFlags)
		: base(Microsoft.CodeAnalysis.CSharp.MessageProvider.Instance, -1)
	{
		_symbol = symbol;
		_containingSymbol = containingSymbol;
		_binderFlags = binderFlags;
		_lazyActualObsoleteDiagnostic = null;
	}

	internal override DiagnosticInfo GetResolvedInfo()
	{
		if (_lazyActualObsoleteDiagnostic == null)
		{
			_symbol.ForceCompleteObsoleteAttribute();
			DiagnosticInfo diagnosticInfo = ((ObsoleteAttributeHelpers.GetObsoleteDiagnosticKind(_symbol, _containingSymbol, forceComplete: true) == ObsoleteDiagnosticKind.Diagnostic) ? ObsoleteAttributeHelpers.CreateObsoleteDiagnostic(_symbol, _binderFlags) : null);
			Interlocked.CompareExchange(ref _lazyActualObsoleteDiagnostic, diagnosticInfo ?? CSDiagnosticInfo.VoidDiagnosticInfo, null);
		}
		return _lazyActualObsoleteDiagnostic;
	}
}
