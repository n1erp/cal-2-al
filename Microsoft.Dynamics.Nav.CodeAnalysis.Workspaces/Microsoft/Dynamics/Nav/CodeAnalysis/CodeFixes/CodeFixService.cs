using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

internal class CodeFixService : ICodeFixService, IWorkspaceService
{
	private readonly Lazy<ImmutableArray<ICodeFixProvider>> lazyCodeFixProviders;

	private ImmutableDictionary<ICodeFixProvider, ImmutableArray<string>> fixerToFixableIdsMap = ImmutableDictionary<ICodeFixProvider, ImmutableArray<string>>.Empty;

	private ImmutableArray<ICodeFixProvider> CodeFixProviders => lazyCodeFixProviders.Value;

	public CodeFixService(IEnumerable<ICodeFixProvider> providers)
	{
		lazyCodeFixProviders = new Lazy<ImmutableArray<ICodeFixProvider>>(() => (providers != null) ? providers.ToImmutableArray() : ImmutableArray<ICodeFixProvider>.Empty);
	}

	public async Task<ImmutableArray<CodeFixCollection>> GetFixesAsync(Document document, TextSpan textSpan, CancellationToken cancellationToken)
	{
		if (document == null)
		{
			return ImmutableArray<CodeFixCollection>.Empty;
		}
		SemanticModel semanticModel = await document.GetSemanticModelAsync();
		PooledDictionary<TextSpan, List<Diagnostic>> aggregatedDiagnostics = null;
		try
		{
			foreach (Diagnostic diagnostic in semanticModel.GetDiagnostics())
			{
				if (!diagnostic.IsSuppressed && textSpan.IntersectsWith(diagnostic.Location.SourceSpan))
				{
					cancellationToken.ThrowIfCancellationRequested();
					aggregatedDiagnostics = aggregatedDiagnostics ?? PooledDictionary<TextSpan, List<Diagnostic>>.GetInstance();
					aggregatedDiagnostics.GetOrAdd(diagnostic.Location.SourceSpan, (TextSpan _) => new List<Diagnostic>()).Add(diagnostic);
				}
			}
			if (aggregatedDiagnostics == null)
			{
				return ImmutableArray<CodeFixCollection>.Empty;
			}
			ArrayBuilder<CodeFixCollection> result = ArrayBuilder<CodeFixCollection>.GetInstance();
			foreach (KeyValuePair<TextSpan, List<Diagnostic>> item in aggregatedDiagnostics)
			{
				TextSpan key = item.Key;
				IEnumerable<Diagnostic> diagnostics = item.Value.OrderByDescending((Diagnostic d) => d.Severity);
				await AppendFixesAsync(document, key, diagnostics, result, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			return result.ToImmutableAndFree();
		}
		finally
		{
			aggregatedDiagnostics?.Free();
		}
	}

	private async Task AppendFixesAsync(Document document, TextSpan span, IEnumerable<Diagnostic> diagnostics, ArrayBuilder<CodeFixCollection> result, CancellationToken cancellationToken)
	{
		foreach (ICodeFixProvider fixer in Enumerable.Distinct(CodeFixProviders))
		{
			cancellationToken.ThrowIfCancellationRequested();
			await AppendFixesAsync(document, span, diagnostics, result, fixer, (Diagnostic d) => GetFixableDiagnosticIds(fixer).Contains(d.Id), (ImmutableArray<Diagnostic> ds) => GetCodeFixesAsync(document, span, fixer, ds, cancellationToken), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task AppendFixesAsync(Document document, TextSpan span, IEnumerable<Diagnostic> diagnosticsWithSameSpan, ArrayBuilder<CodeFixCollection> result, ICodeFixProvider fixer, Func<Diagnostic, bool> hasFix, Func<ImmutableArray<Diagnostic>, Task<ImmutableArray<CodeFix>>> getFixes, CancellationToken cancellationToken)
	{
		ImmutableArray<Diagnostic> arg = diagnosticsWithSameSpan.Where(hasFix).ToImmutableArray();
		if (arg.Length > 0)
		{
			ImmutableArray<CodeFix> fixes = await getFixes(arg);
			if (!fixes.IsDefaultOrEmpty)
			{
				result.Add(new CodeFixCollection(fixer, span, fixes));
			}
		}
	}

	private async Task<ImmutableArray<CodeFix>> GetCodeFixesAsync(Document document, TextSpan span, ICodeFixProvider fixer, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
	{
		ArrayBuilder<CodeFix> fixes = ArrayBuilder<CodeFix>.GetInstance();
		CodeFixContext context = new CodeFixContext(document, span, diagnostics, (CodeAction action, ImmutableArray<Diagnostic> applicableDiagnostics) =>
		{
			fixes.Add(new CodeFix(document.Project, action, applicableDiagnostics));
		}, verifyArguments: false, cancellationToken);
		await (fixer.RegisterCodeFixesAsync(context) ?? Task.CompletedTask).ConfigureAwait(continueOnCapturedContext: false);
		return fixes.ToImmutableAndFree();
	}

	private ImmutableArray<string> GetFixableDiagnosticIds(ICodeFixProvider fixer)
	{
		try
		{
			return ImmutableInterlocked.GetOrAdd(ref fixerToFixableIdsMap, fixer, (ICodeFixProvider f) => GetAndTestFixableDiagnosticIds(f));
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception)
		{
			return ImmutableArray<string>.Empty;
		}
	}

	private static ImmutableArray<string> GetAndTestFixableDiagnosticIds(ICodeFixProvider codeFixProvider)
	{
		ImmutableArray<string> fixableDiagnosticIds = codeFixProvider.FixableDiagnosticIds;
		if (fixableDiagnosticIds.IsDefault)
		{
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, WorkspacesResources.ReturnedUninitializedImmutableArray, codeFixProvider.GetType().Name + ".FixableDiagnosticIds"));
		}
		return fixableDiagnosticIds;
	}
}
