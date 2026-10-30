using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class TypeContextSymbolRecommender : ContextAwareSymbolRecommender
{
	internal TypeContextSymbolRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (!Context.General.HasFlag(GeneralContexts.ApplicationObject))
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		SymbolKind symbolKind = LookupSymbol(cancellationToken);
		if (symbolKind == SymbolKind.Undefined)
		{
			return Context.SemanticModel.LookupSymbols(Context.LeftToken.SpanStart, LookupOptions.Default, null, null, cancellationToken);
		}
		return Context.SemanticModel.Compilation.GetObjectSymbolsByKindAcrossModules(symbolKind);
	}

	private SymbolKind LookupSymbol(CancellationToken cancellationToken)
	{
		SyntaxTree syntaxTree = Context.SyntaxTree;
		int position = Context.Position;
		string subTypeName = GetSubTypeName(Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTreeExtensions.FindTokenOnLeftOfPosition(syntaxTree, position, cancellationToken));
		if (subTypeName == null)
		{
			return SymbolKind.Undefined;
		}
		return NavTypeExtensions.GetNavTypeKind(subTypeName, throwIfUnknown: false).ToSubTypeSymbolKind();
	}

	private static string GetSubTypeName(SyntaxToken tokenOnLeftOfPosition)
	{
		SubtypedDataTypeSyntax ancestor = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<SubtypedDataTypeSyntax>(tokenOnLeftOfPosition);
		if (ancestor != null)
		{
			return ancestor.TypeName.ValueText;
		}
		return Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<EnumDataTypeSyntax>(tokenOnLeftOfPosition)?.TypeName.ValueText;
	}
}
