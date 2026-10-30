using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal struct NextOperation<TResult>(int index, SyntaxToken token1, SyntaxToken token2, IOperationHolder<TResult> operationCache)
{
	private readonly int _index = index;

	private readonly SyntaxToken _token1 = token1;

	private readonly SyntaxToken _token2 = token2;

	private readonly IOperationHolder<TResult> _operationCache = operationCache;

	public TResult Invoke()
	{
		return _operationCache.Continuation(_index, _token1, _token2, _operationCache);
	}
}
