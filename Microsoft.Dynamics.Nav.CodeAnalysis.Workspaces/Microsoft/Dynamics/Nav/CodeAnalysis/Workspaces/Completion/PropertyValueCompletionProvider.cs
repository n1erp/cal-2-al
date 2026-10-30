using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class PropertyValueCompletionProvider : SymbolCompletionProvider
{
	private static readonly ImmutableArray<string> Tags = ImmutableArray.Create("PropertyValue");

	public override async Task ProvideCompletionsAsync(CompletionContext context, AbstractSyntaxContext memberSyntaxContext)
	{
		Document document = context.Document;
		CancellationToken cancellationToken = context.CancellationToken;
		using (Logger.LogBlock(FunctionId.Completion_PropertyValueProvider_ProvideCompletionsAsync, cancellationToken))
		{
			PropertyValuesRecommender propertyValuesRecommender = new BooleanValuesRecommender(memberSyntaxContext as MemberSyntaxContext);
			IEnumerable<PropertyValueRecommendation> enumerable = await RecommendPropertyValuesAsync(document, propertyValuesRecommender, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			context.IsExclusive = propertyValuesRecommender.IsExclusive;
			PropertyTypeInfo info = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<PropertySyntax>(memberSyntaxContext.TargetToken)?.Value?.GetPropertyTypeInfo();
			foreach (PropertyValueRecommendation item in enumerable)
			{
				context.AddItem(CreateItem(info, item));
			}
		}
	}

	protected virtual async Task<IEnumerable<PropertyValueRecommendation>> RecommendPropertyValuesAsync(Document document, PropertyValuesRecommender propertyValuesRecommender, CancellationToken cancellationToken)
	{
		if (!propertyValuesRecommender.Context.General.HasFlag(GeneralContexts.PropertyValue))
		{
			return ImmutableArray<PropertyValueRecommendation>.Empty;
		}
		return await propertyValuesRecommender.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private static CompletionItem CreateItem(PropertyTypeInfo info, PropertyValueRecommendation recommendation)
	{
		string displayText = recommendation.DisplayText;
		string detailText = recommendation.DetailText;
		string insertionText = GetInsertionText(info, recommendation);
		string descriptionValue = recommendation.DescriptionValue;
		return CommonCompletionItem.Create(glyph: Glyph.Property, descriptionText: descriptionValue, detailText: detailText, sortText: null, filterText: null, insertionText: insertionText, preselect: false, showsWarningIcon: false, shouldFormatOnCommit: false, isArgumentName: false, isSnippet: false, tags: Tags, displayText: displayText, span: default, isMarkdownDocs: recommendation.IsMarkdownDocs);
	}

	private static string GetInsertionText(PropertyTypeInfo info, PropertyValueRecommendation recommendation)
	{
		if (!ShouldQuotePropertyValue(info))
		{
			return recommendation.DisplayText;
		}
		return recommendation.DisplayText.QuoteIdentifierIfNeeded();
	}

	private static bool ShouldQuotePropertyValue(PropertyTypeInfo info)
	{
		if (string.Equals(info?.TypeName, NavTypeKind.Boolean.ToString(), StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		return true;
	}
}
