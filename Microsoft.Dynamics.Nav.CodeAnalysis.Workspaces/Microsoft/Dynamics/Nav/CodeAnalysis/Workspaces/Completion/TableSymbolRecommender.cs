using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class TableSymbolRecommender : ContextAwareSymbolRecommender
{
	internal TableSymbolRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (IsNotTableContext())
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return Context.SemanticModel.Compilation.GetApplicationObjectTypeSymbolsAcrossModules(SymbolKind.Table);
	}

	private bool IsNotTableContext()
	{
		if (!Context.Report.HasFlag(ReportContexts.DataItemSource) && !Context.XmlPort.HasFlag(XmlPortContexts.TableNodeSource))
		{
			return !Context.Query.HasFlag(QueryContexts.DataItemSource);
		}
		return false;
	}
}
