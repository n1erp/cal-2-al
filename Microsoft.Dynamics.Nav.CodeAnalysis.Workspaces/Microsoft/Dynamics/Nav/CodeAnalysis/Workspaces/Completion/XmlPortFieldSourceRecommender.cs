using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class XmlPortFieldSourceRecommender : ContextAwareSymbolRecommender
{
	public XmlPortFieldSourceRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (!Context.XmlPort.HasFlag(XmlPortContexts.FieldNodeSource))
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return from s in Context.SemanticModel.LookupSymbols(Context.LeftToken.SpanStart, LookupOptions.Default, null, null, cancellationToken)
			where IsTableNode(s) || s.IsKind(SymbolKind.Field)
			select s;
	}

	private static bool IsTableNode(ISymbol symbol)
	{
		if (symbol.IsKind(SymbolKind.XmlPortNode))
		{
			return ((IXmlPortNodeSymbol)symbol).SourceTypeKind == XmlPortSourceTypeKind.Table;
		}
		return false;
	}
}
