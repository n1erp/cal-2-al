using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeRefactoring;

internal class CodeRefactoringService : ICodeRefactoringService
{
	private readonly Lazy<ImmutableArray<ICodeRefactoringProvider>> lazyCodeRefactoringProviders;

	private ImmutableArray<ICodeRefactoringProvider> CodeRefactoringProviders => lazyCodeRefactoringProviders.Value;

	public CodeRefactoringService(IEnumerable<ICodeRefactoringProvider> providers)
	{
		lazyCodeRefactoringProviders = new Lazy<ImmutableArray<ICodeRefactoringProvider>>(() => (providers != null) ? providers.ToImmutableArray() : ImmutableArray<ICodeRefactoringProvider>.Empty);
	}

	public async Task<ImmutableArray<CodeRefactoring>> GetRefactoringsAsync(Document document, TextSpan state, CancellationToken cancellationToken)
	{
		List<Task<CodeRefactoring>> list = new List<Task<CodeRefactoring>>();
		foreach (ICodeRefactoringProvider provider in CodeRefactoringProviders)
		{
			list.Add(Task.Run(() => GetRefactoringFromProviderAsync(document, state, provider, cancellationToken), cancellationToken));
		}
		return (await Task.WhenAll(list).ConfigureAwait(continueOnCapturedContext: false)).WhereNotNull().ToImmutableArray();
	}

	private async Task<CodeRefactoring> GetRefactoringFromProviderAsync(Document document, TextSpan state, ICodeRefactoringProvider provider, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		ArrayBuilder<CodeAction> actions = ArrayBuilder<CodeAction>.GetInstance();
		try
		{
			CodeRefactoringContext context = new CodeRefactoringContext(document, state, (CodeAction a) =>
			{
				lock (actions)
				{
					actions.Add(a);
				}
			}, cancellationToken);
			await (provider.ComputeRefactoringsAsync(context) ?? Task.CompletedTask).ConfigureAwait(continueOnCapturedContext: false);
			return (actions.Count > 0) ? new CodeRefactoring(provider, actions.ToImmutable()) : null;
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception)
		{
			throw;
		}
		finally
		{
			actions.Free();
		}
	}
}
