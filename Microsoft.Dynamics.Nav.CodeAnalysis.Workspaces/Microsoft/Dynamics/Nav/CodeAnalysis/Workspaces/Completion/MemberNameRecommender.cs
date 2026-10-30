using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal sealed class MemberNameRecommender : TextRecommender
{
	private const string NameAndSourceSeparator = ";";

	protected internal override async Task<IEnumerable<CompletionItem>> RecommendTextAsync(CompletionContext context, MemberSyntaxContext syntaxContext, CancellationToken cancellationToken)
	{
		if (!syntaxContext.General.HasFlag(GeneralContexts.MemberName))
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		SyntaxNode syntaxNode = GoToParentIfIdentifier(syntaxContext.LeftToken.Parent);
		bool hasToHaveClsCompliantName = syntaxNode.Kind.NameMustBeClsCompliant();
		IEnumerable<ISymbol> first = syntaxContext.SemanticModel.LookupSymbols(syntaxContext.LeftToken.SpanStart, LookupOptions.Default, null, null, cancellationToken);
		first = first.Concat(GetTableSymbolsIfExpected(syntaxContext, syntaxNode));
		first = (syntaxContext.General.HasFlag(GeneralContexts.AnyExpression) ? first.Where(SemanticFacts.IsValidForExpression) : first);
		TextSpan spanForReplacement = GetSpanForReplacement(syntaxNode, syntaxContext.LeftToken, out var isSourceMissing);
		first = from s in first
			where !string.IsNullOrEmpty(GetName(s))
			where s.IsKind(SymbolKind.Field, SymbolKind.Table, SymbolKind.GlobalVariable, SymbolKind.LocalVariable) || (s.IsKind(SymbolKind.Method) && ((MethodSymbol)s).MethodKind == MethodKind.Method)
			select s;
		return first.Select((ISymbol s) => CreateItem(s, spanForReplacement, hasToHaveClsCompliantName, isSourceMissing));
	}

	private static IEnumerable<ISymbol> GetTableSymbolsIfExpected(MemberSyntaxContext context, SyntaxNode parentOfInterest)
	{
		SyntaxKind kind = parentOfInterest.Kind;
		if (kind == SyntaxKind.ReportDataItem || kind == SyntaxKind.XmlPortTableElement || kind == SyntaxKind.QueryDataItem)
		{
			return context.SemanticModel.Compilation.GetApplicationObjectTypeSymbolsAcrossModules(SymbolKind.Table);
		}
		return Enumerable.Empty<ISymbol>();
	}

	private static string GetName(ISymbol s)
	{
		return s.Kind switch
		{
			SymbolKind.ReportDataItem => ((ReportDataItemSymbol)s).RelatedTable?.Name, 
			SymbolKind.XmlPortNode => ((SourceXmlPortNodeSymbol)s).Type?.Name, 
			SymbolKind.QueryDataItem => ((SourceQueryDataItemSymbol)s).RelatedTable?.Name, 
			_ => s.Name, 
		};
	}

	private static SyntaxNode GoToParentIfIdentifier(SyntaxNode node)
	{
		if (node.IsKind(SyntaxKind.IdentifierName))
		{
			return node.Parent;
		}
		return node;
	}

	private static CompletionItem CreateItem(ISymbol symbol, TextSpan span, bool nameMustBeClsCompliant, bool isSourceMissing)
	{
		string name = GetName(symbol);
		string value = (nameMustBeClsCompliant ? FixNameIfNotClsCompliant(name) : name);
		value = value.QuoteIdentifierIfNeeded();
		string text = (isSourceMissing ? (value + ";" + name.QuoteIdentifierIfNeeded()) : value);
		Glyph? glyph = symbol.GetGlyph();
		string insertionText = text;
		return CommonCompletionItem.Create(name, span, glyph, null, null, null, null, insertionText);
	}

	private static string FixNameIfNotClsCompliant(string name)
	{
		if (name.IsValidClsIdentifier())
		{
			return name;
		}
		return Microsoft.Dynamics.Nav.CodeAnalysis.Utilities.StringExtensions.GetValidClsIdentifierByUsingUnderscores(name);
	}

	private static TextSpan GetSpanForReplacement(SyntaxNode parentOfInterest, SyntaxToken leftToken, out bool isSourceMissing)
	{
		SyntaxToken openParenthesisToken;
		SyntaxToken semicolonToken;
		SyntaxNode syntaxNode;
		SyntaxToken closeParenthesisToken;
		switch (parentOfInterest.Kind)
		{
		case SyntaxKind.PageField:
		{
			PageFieldSyntax pageFieldSyntax = (PageFieldSyntax)parentOfInterest;
			openParenthesisToken = pageFieldSyntax.OpenParenthesisToken;
			semicolonToken = pageFieldSyntax.SemicolonToken;
			syntaxNode = pageFieldSyntax.Expression;
			closeParenthesisToken = pageFieldSyntax.CloseParenthesisToken;
			break;
		}
		case SyntaxKind.XmlPortTableElement:
		{
			XmlPortTableElementSyntax xmlPortTableElementSyntax = (XmlPortTableElementSyntax)parentOfInterest;
			openParenthesisToken = xmlPortTableElementSyntax.OpenParenthesisToken;
			semicolonToken = xmlPortTableElementSyntax.SemicolonToken;
			syntaxNode = xmlPortTableElementSyntax.SourceTable;
			closeParenthesisToken = xmlPortTableElementSyntax.CloseParenthesisToken;
			break;
		}
		case SyntaxKind.ReportDataItem:
		{
			ReportDataItemSyntax reportDataItemSyntax = (ReportDataItemSyntax)parentOfInterest;
			openParenthesisToken = reportDataItemSyntax.OpenParenthesisToken;
			semicolonToken = reportDataItemSyntax.SemicolonToken;
			syntaxNode = reportDataItemSyntax.DataItemTable;
			closeParenthesisToken = reportDataItemSyntax.CloseParenthesisToken;
			break;
		}
		case SyntaxKind.ReportColumn:
		{
			ReportColumnSyntax reportColumnSyntax = (ReportColumnSyntax)parentOfInterest;
			openParenthesisToken = reportColumnSyntax.OpenParenthesisToken;
			semicolonToken = reportColumnSyntax.SemicolonToken;
			syntaxNode = reportColumnSyntax.SourceExpression;
			closeParenthesisToken = reportColumnSyntax.CloseParenthesisToken;
			break;
		}
		case SyntaxKind.QueryDataItem:
		{
			QueryDataItemSyntax queryDataItemSyntax = (QueryDataItemSyntax)parentOfInterest;
			openParenthesisToken = queryDataItemSyntax.OpenParenthesisToken;
			semicolonToken = queryDataItemSyntax.SemicolonToken;
			syntaxNode = queryDataItemSyntax.DataItemTable;
			closeParenthesisToken = queryDataItemSyntax.CloseParenthesisToken;
			break;
		}
		case SyntaxKind.QueryColumn:
		{
			QueryColumnSyntax queryColumnSyntax = (QueryColumnSyntax)parentOfInterest;
			openParenthesisToken = queryColumnSyntax.OpenParenthesisToken;
			semicolonToken = queryColumnSyntax.SemicolonToken;
			syntaxNode = queryColumnSyntax.RelatedField;
			closeParenthesisToken = queryColumnSyntax.CloseParenthesisToken;
			break;
		}
		case SyntaxKind.QueryFilter:
		{
			QueryFilterSyntax queryFilterSyntax = (QueryFilterSyntax)parentOfInterest;
			openParenthesisToken = queryFilterSyntax.OpenParenthesisToken;
			semicolonToken = queryFilterSyntax.SemicolonToken;
			syntaxNode = queryFilterSyntax.RelatedField;
			closeParenthesisToken = queryFilterSyntax.CloseParenthesisToken;
			break;
		}
		default:
			isSourceMissing = false;
			return default;
		}
		if (closeParenthesisToken.IsMissing || closeParenthesisToken.IsKind(SyntaxKind.None))
		{
			if (semicolonToken.IsMissing || semicolonToken.IsKind(SyntaxKind.None))
			{
				isSourceMissing = true;
				return GetSpanFromPosToPos(openParenthesisToken.SpanEnd, leftToken.SpanEnd);
			}
			isSourceMissing = true;
			return GetSpanFromPosToPos(openParenthesisToken.SpanEnd, semicolonToken.SpanEnd);
		}
		if (semicolonToken.IsMissing || semicolonToken.IsKind(SyntaxKind.None))
		{
			isSourceMissing = true;
			return GetSpanFromPosToPos(openParenthesisToken.SpanEnd, leftToken.SpanEnd);
		}
		isSourceMissing = syntaxNode == null || syntaxNode.IsMissing || syntaxNode.IsKind(SyntaxKind.None);
		return GetSpanFromPosToPos(openParenthesisToken.SpanEnd, isSourceMissing ? semicolonToken.SpanEnd : semicolonToken.SpanStart);
	}

	private static TextSpan GetSpanFromPosToPos(int start, int end)
	{
		return new TextSpan(start, end - start);
	}
}
