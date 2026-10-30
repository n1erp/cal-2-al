using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal struct NextAction<TArgument>(int index, SyntaxNode node, SyntaxToken lastToken, IActionHolder<TArgument> actionCache)
{
	private readonly int _index = index;

	private readonly SyntaxNode _node = node;

	private readonly SyntaxToken _lastToken = lastToken;

	private readonly IActionHolder<TArgument> _actionCache = actionCache;

	public void Invoke(List<TArgument> arguments)
	{
		_actionCache.Continuation(_index, arguments, _node, _lastToken, _actionCache);
	}
}
