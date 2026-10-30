using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class ImageValuesRecommender : PropertyValuesRecommender
{
	private enum ImageTarget
	{
		Unknown,
		Field,
		Action,
		ActionGroup
	}

	protected internal override bool IsExclusive => true;

	internal ImageValuesRecommender(MemberSyntaxContext context)
		: base(context)
	{
		Next = new ObjectReferencePropertyValuesRecommender(context);
	}

	protected internal override async Task<IEnumerable<PropertyValueRecommendation>> RecommendPropertyValuesAsync(CancellationToken cancellationToken)
	{
		if (PropertyTypeInfo == null || PropertyValue == null)
		{
			return SpecializedCollections.EmptyEnumerable<PropertyValueRecommendation>();
		}
		if (PropertyTypeInfo.Kind != PropertyKind.Image)
		{
			return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		ImageTarget kind = ImageTarget.Unknown;
		SyntaxNode syntaxNode = FindDeclaringParent(Context.LeftToken.GetPreviousToken().Parent, ref kind);
		switch (kind)
		{
		case ImageTarget.Action:
			if (PageMemberSymbolHelpers.IsContainedInControlOfKind(Context.SemanticModel.GetDeclaredSymbol((PageActionSyntax)syntaxNode), ControlKind.CueGroup))
			{
				return CreateImagePropertyRecommendation(ImageResources.GetActionCueGroupImageResources());
			}
			return CreateImagePropertyRecommendation(ImageResources.GetActionImageResources());
		case ImageTarget.ActionGroup:
		{
			ActionSymbol declaredSymbol = Context.SemanticModel.GetDeclaredSymbol((PageActionGroupSyntax)syntaxNode);
			if (PageMemberSymbolHelpers.IsDirectlyContainedInGivenActionArea(declaredSymbol, ActionAreaKind.Sections))
			{
				return CreateImagePropertyRecommendation(ImageResources.GetRoleCenterActionGroupImageResources());
			}
			if (!PageMemberSymbolHelpers.IsContainedInGroupInGivenActionArea(declaredSymbol, ActionAreaKind.Sections))
			{
				return CreateImagePropertyRecommendation(ImageResources.GetActionImageResources());
			}
			break;
		}
		case ImageTarget.Field:
			if (PageMemberSymbolHelpers.IsContainedInControlOfKind(Context.SemanticModel.GetDeclaredSymbol((PageFieldSyntax)syntaxNode), ControlKind.CueGroup))
			{
				return CreateImagePropertyRecommendation(ImageResources.GetFieldCueGroupImageResources());
			}
			break;
		}
		return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private static SyntaxNode FindDeclaringParent(SyntaxNode objectSyntax, ref ImageTarget kind)
	{
		if (objectSyntax == null)
		{
			return null;
		}
		if (IsValidDeclaringType(objectSyntax, ref kind))
		{
			return objectSyntax;
		}
		return FindDeclaringParent(objectSyntax.Parent, ref kind);
	}

	private static bool IsValidDeclaringType(SyntaxNode syntaxNode, ref ImageTarget kind)
	{
		switch (syntaxNode.Kind)
		{
		case SyntaxKind.PageAction:
			kind = ImageTarget.Action;
			return true;
		case SyntaxKind.PageActionGroup:
			kind = ImageTarget.ActionGroup;
			return true;
		case SyntaxKind.PageField:
			kind = ImageTarget.Field;
			return true;
		default:
			return false;
		}
	}

	private static IEnumerable<PropertyValueRecommendation> CreateImagePropertyRecommendation(IDictionary<string, string> resource)
	{
		List<PropertyValueRecommendation> list = new List<PropertyValueRecommendation>();
		foreach (KeyValuePair<string, string> item in resource)
		{
			string key = item.Key;
			string text = ((item.Value != null) ? Uri.UnescapeDataString(item.Value) : string.Empty);
			DebugAssertHelper.Assert(!string.IsNullOrEmpty(text));
			list.Add(new PropertyValueRecommendation
			{
				DisplayText = key,
				DetailText = key + " Image",
				DescriptionValue = MarkdownHelper.CreateBase64Image(key, text),
				IsMarkdownDocs = true
			});
		}
		return list;
	}
}
