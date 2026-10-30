using System;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeRefactoring;

internal class CodeRefactoring
{
	public ICodeRefactoringProvider Provider { get; }

	public ImmutableArray<CodeAction> Actions { get; }

	public CodeRefactoring(ICodeRefactoringProvider provider, ImmutableArray<CodeAction> actions)
	{
		Provider = provider;
		Actions = actions.NullToEmpty();
		if (Actions.Length == 0)
		{
			throw new ArgumentException(WorkspacesResources.ActionsCanNotBeEmpty, "actions");
		}
	}
}
