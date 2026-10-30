using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal sealed class AttributeNameRecommender : TextRecommender
{
	protected internal override async Task<IEnumerable<CompletionItem>> RecommendTextAsync(CompletionContext context, MemberSyntaxContext syntaxContext, CancellationToken cancellationToken)
	{
		if (!syntaxContext.General.HasFlag(GeneralContexts.AttributeName))
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return AttributeDefinitions.Attributes.Values.Select((AttributeTypeInfo t) => CreateItem(t.Name, t.Name));
	}

	private static CompletionItem CreateItem(string displayText, string insertionText)
	{
		string attributeDocumentation = DocumentationFacts.GetAttributeDocumentation(displayText);
		return CommonCompletionItem.Create(displayText, default, Glyph.Builtin, attributeDocumentation, null, null, null, insertionText, preselect: false, showsWarningIcon: false, shouldFormatOnCommit: false, isArgumentName: false, isSnippet: false, isMarkdownDocs: true);
	}
}
