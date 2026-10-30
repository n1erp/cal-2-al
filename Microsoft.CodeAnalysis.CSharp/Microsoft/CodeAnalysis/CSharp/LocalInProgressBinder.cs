using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class LocalInProgressBinder : Binder
{
	private readonly LocalSymbol _inProgress;

	internal override LocalSymbol LocalInProgress => _inProgress;

	internal LocalInProgressBinder(LocalSymbol inProgress, Binder next)
		: base(next)
	{
		_inProgress = inProgress;
	}
}
