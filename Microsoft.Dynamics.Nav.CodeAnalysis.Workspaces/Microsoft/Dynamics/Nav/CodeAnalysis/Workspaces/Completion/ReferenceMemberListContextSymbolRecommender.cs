using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class ReferenceMemberListContextSymbolRecommender : ContextAwareSymbolRecommender
{
	internal ReferenceMemberListContextSymbolRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (!Context.General.HasFlag(GeneralContexts.ReferenceMemberList))
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (DeclaringObject == null)
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		SyntaxToken tokenBefore = Context.SyntaxTree.GetTokenBefore(Context.Position, cancellationToken);
		if (tokenBefore.Parent.Kind.IsMoveChange())
		{
			GetMembersAndGroupsForPageLayoutChange(tokenBefore, cancellationToken, out var members, out var _, out var _);
			return members;
		}
		if (tokenBefore.ParentIsKind(SyntaxKind.Key))
		{
			return RecommendationHelper.RecommendFieldSymbolsForKey(Context, DeclaringObject, cancellationToken);
		}
		if (tokenBefore.ParentIsKind(SyntaxKind.FieldGroup) || tokenBefore.ParentIsKind(SyntaxKind.FieldGroupAddChange))
		{
			return RecommendationHelper.RecommendFieldSymbolsForFieldGroup(Context, DeclaringObject, cancellationToken);
		}
		if (tokenBefore.ParentIsKind(SyntaxKind.SortingExpression))
		{
			return RecommendSymbolsForSortingListContext();
		}
		return SpecializedCollections.EmptyEnumerable<ISymbol>();
	}

	private IEnumerable<ISymbol> RecommendSymbolsForSortingListContext()
	{
		TableTypeSymbol tableTypeSymbol = null;
		PropertySyntax ancestor = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<PropertySyntax>(Context.TargetToken);
		if (ancestor != null)
		{
			PropertyTypeInfo propertyTypeInfo = ancestor.Value?.GetPropertyTypeInfo();
			if (propertyTypeInfo != null && (propertyTypeInfo.Kind == PropertyKind.RunPageView || propertyTypeInfo.Kind == PropertyKind.SubPageView))
			{
				tableTypeSymbol = GetRelatedTableSymbol(ancestor.Value);
			}
			else if (propertyTypeInfo != null)
			{
				tableTypeSymbol = DeclaringObject?.GetRelatedTableSymbol();
			}
		}
		if (tableTypeSymbol == null)
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		ImmutableArray<KeySymbol> keys = tableTypeSymbol.GetKeys();
		if (keys.Length > 0)
		{
			ArrayBuilder<FieldSymbol> instance = ArrayBuilder<FieldSymbol>.GetInstance();
			PooledNameComparisonHashSet instance2 = PooledNameComparisonHashSet.GetInstance();
			try
			{
				foreach (KeySymbol item in keys)
				{
					foreach (FieldSymbol field in item.Fields)
					{
						if (!string.IsNullOrEmpty(field.Name) && !instance2.Contains(field.Name))
						{
							instance.Add(field);
							instance2.Add(field.Name);
						}
					}
				}
				return instance.ToImmutable();
			}
			finally
			{
				instance.Free();
				instance2.Free();
			}
		}
		return SpecializedCollections.EmptyEnumerable<ISymbol>();
	}
}
