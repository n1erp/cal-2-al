using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal abstract class AbstractRecommendationService : IRecommendationService, ILanguageService
{
	private sealed class ShouldIncludeSymbolContext
	{
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		private readonly AbstractSyntaxContext context;

		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		private readonly CancellationToken cancellationToken;

		internal ShouldIncludeSymbolContext(AbstractSyntaxContext context, CancellationToken cancellationToken)
		{
			this.context = context;
			this.cancellationToken = cancellationToken;
		}

		internal bool ShouldIncludeSymbol(ISymbol symbol)
		{
			return true;
		}
	}

	protected abstract Task<IEnumerable<ISymbol>> GetRecommendedSymbolsAtPositionWorkerAsync(AbstractSyntaxContext context, OptionSet options, CancellationToken cancellationToken);

	public async Task<IEnumerable<ISymbol>> GetRecommendedSymbolsAtPositionAsync(AbstractSyntaxContext context, OptionSet options, CancellationToken cancellationToken)
	{
		IEnumerable<ISymbol> source = await GetRecommendedSymbolsAtPositionWorkerAsync(context, options, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		ShouldIncludeSymbolContext shouldIncludeSymbolContext = new ShouldIncludeSymbolContext(context, cancellationToken);
		return source.Where(shouldIncludeSymbolContext.ShouldIncludeSymbol);
	}
}
