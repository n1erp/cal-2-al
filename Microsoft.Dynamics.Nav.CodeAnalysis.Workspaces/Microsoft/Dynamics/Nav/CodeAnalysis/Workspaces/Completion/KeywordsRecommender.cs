using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal sealed class KeywordsRecommender : TextRecommender
{
	protected internal override async Task<IEnumerable<CompletionItem>> RecommendTextAsync(CompletionContext context, MemberSyntaxContext syntaxContext, CancellationToken cancellationToken)
	{
		if (ExcludedInThisContext(syntaxContext))
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		PooledHashSet<KeywordRecommendation> result = PooledHashSet<KeywordRecommendation>.GetInstance();
		try
		{
			bool flag = syntaxContext.LeftToken.IsWord() && syntaxContext.LeftToken.IntersectsWith(syntaxContext.Position);
			SyntaxToken syntaxToken = syntaxContext.TargetToken;
			SyntaxNode parent = syntaxContext.TargetToken.Parent;
			if (flag)
			{
				TextChange textChange = new TextChange(TextSpan.FromBounds(syntaxContext.LeftToken.Position, syntaxContext.Position), string.Empty);
				syntaxToken = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTreeExtensions.FindTokenOnLeftOfPosition(syntaxContext.SyntaxTree.WithChangedText(syntaxContext.SyntaxTree.GetText().WithChanges(textChange)), syntaxContext.LeftToken.Position, cancellationToken);
				parent = syntaxToken.Parent;
			}
			SyntaxNode syntaxNode = GetFirstNodeWithNonTerminalToken(parent, syntaxToken, out var slot);
			if (syntaxNode == null)
			{
				syntaxNode = syntaxContext.SyntaxTree.GetRoot();
				slot = 0;
			}
			result = FindKeywordsFromAllowedStartNodes(syntaxToken, syntaxNode, flag, slot, result);
			if (result.Count > 0)
			{
				return CreateSimpleCompletionList(result.ToImmutableArrayOrEmpty().ToArray());
			}
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			result.Free();
		}
	}

	private static bool ExcludedInThisContext(MemberSyntaxContext syntaxContext)
	{
		if (!syntaxContext.General.HasFlag(GeneralContexts.AttributeName) && !syntaxContext.General.HasFlag(GeneralContexts.AttributeArgumentList))
		{
			if (syntaxContext.IsArgumentExpression())
			{
				SyntaxNode parent = syntaxContext.TargetToken.Parent;
				if (parent != null && parent.Kind == SyntaxKind.ArgumentList)
				{
					goto IL_0061;
				}
			}
			return false;
		}
		goto IL_0061;
		IL_0061:
		return true;
	}

	private PooledHashSet<KeywordRecommendation> FindKeywordsFromAllowedStartNodes(SyntaxToken syntaxToken, SyntaxNode startingNode, bool startedTyping, int slot, PooledHashSet<KeywordRecommendation> result)
	{
		if (startingNode.Node.Definition.Slots.Value[slot].IsList)
		{
			bool flag = IsTokenTheLastTokenInTheSlot(startingNode, slot, syntaxToken);
			FindDistinctApplicableKeywords(startingNode, slot, result, startedTyping, !flag);
			while ((slot == startingNode.SlotCount - 1 && startingNode.Node.Definition.Slots.Value[slot].IsList) & flag)
			{
				startingNode = startingNode.Parent;
				slot = startingNode.Node.FindSlotIndexContainingOffset(syntaxToken.Position - startingNode.Position);
				flag = IsTokenTheLastTokenInTheSlot(startingNode, slot, syntaxToken);
				if (startingNode.Node.Definition.Slots.Value[slot].IsList)
				{
					FindDistinctApplicableKeywords(startingNode, slot, result, startedTyping, !flag);
				}
				else
				{
					FindDistinctApplicableKeywords(startingNode, slot + 1, result, startedTyping, tokenIsBeforeTheSlot: true);
				}
			}
		}
		else
		{
			FindDistinctApplicableKeywords(startingNode, slot + 1, result, startedTyping, tokenIsBeforeTheSlot: true);
		}
		return result;
	}

	private static bool IsTokenTheLastTokenInTheSlot(SyntaxNode node, int slot, SyntaxToken token)
	{
		return node.GetNodeSlot(slot)?.GetLastToken().Position == token.Position;
	}

	private void FindDistinctApplicableKeywords(SyntaxNode startingNode, int startingSlot, PooledHashSet<KeywordRecommendation> result, bool startedTyping, bool tokenIsBeforeTheSlot)
	{
		List<SyntaxNodeDefinition> visitedDefinitions = new List<SyntaxNodeDefinition>();
		KeywordRecommendation[] values = FindApplicableKeywords(startingNode, startingNode, startingNode.Node.Definition, startingSlot, startedTyping, tokenIsBeforeTheSlot, visitedDefinitions);
		result.AddAll(values);
	}

	private KeywordRecommendation[] FindApplicableKeywords(SyntaxNode startingNode, SyntaxNode syntaxNode, SyntaxNodeDefinition syntaxNodeDefinition, int startingSlot, bool startedTyping, bool tokenIsBeforeTheSlot, List<SyntaxNodeDefinition> visitedDefinitions)
	{
		if (syntaxNodeDefinition == null || visitedDefinitions.Count((SyntaxNodeDefinition n) => n == syntaxNodeDefinition) > 1)
		{
			return new KeywordRecommendation[0];
		}
		visitedDefinitions.Add(syntaxNodeDefinition);
		ImmutableArray<SyntaxNodeSlotDefinition> value = syntaxNodeDefinition.Slots.Value;
		int num = startingSlot;
		ArrayBuilder<KeywordRecommendation> instance = ArrayBuilder<KeywordRecommendation>.GetInstance();
		try
		{
			for (; num < value.Length; num++)
			{
				SyntaxNodeSlotDefinition syntaxNodeSlotDefinition = value[num];
				InternalSyntaxNode slotSyntaxNode = syntaxNode?.Node?.GetSlot(num);
				if (!((syntaxNodeSlotDefinition.IsList || IsNodeNullOrInvalid(slotSyntaxNode)) | startedTyping))
				{
					break;
				}
				if (syntaxNodeDefinition is PropertySyntaxDefinition && syntaxNodeSlotDefinition.Node is PropertyValueSyntaxDefinition)
				{
					FindApplicableKeywordsForPropertyValue(startingNode, syntaxNode, visitedDefinitions, instance);
				}
				else if (syntaxNodeSlotDefinition.Kinds.Any())
				{
					FindApplicableKeywordsForTokenAndNodeKinds(startingNode, visitedDefinitions, syntaxNodeSlotDefinition, instance);
				}
				else if (syntaxNodeSlotDefinition.Node != null)
				{
					KeywordRecommendation[] items = FindApplicableKeywords(startingNode, null, syntaxNodeSlotDefinition.Node, 0, startedTyping: false, tokenIsBeforeTheSlot: true, visitedDefinitions);
					instance.AddRange(items);
				}
				if (!CanContinueToNextSlot(tokenIsBeforeTheSlot, slotSyntaxNode, syntaxNodeSlotDefinition))
				{
					break;
				}
				tokenIsBeforeTheSlot = true;
			}
			return instance.ToArray();
		}
		finally
		{
			instance.Free();
		}
	}

	private static bool CanContinueToNextSlot(bool tokenIsBeforeTheSlot, InternalSyntaxNode slotSyntaxNode, SyntaxNodeSlotDefinition slot)
	{
		if (!IsNodeNotNullButInvalid(slotSyntaxNode) && ((!slot.IsOptional && !slot.IsList) || (slotSyntaxNode != null && !slotSyntaxNode.IsMissing)))
		{
			if (slot.IsList)
			{
				return !tokenIsBeforeTheSlot;
			}
			return false;
		}
		return true;
	}

	private void FindApplicableKeywordsForTokenAndNodeKinds(SyntaxNode startingNode, List<SyntaxNodeDefinition> visitedDefinitions, SyntaxNodeSlotDefinition slot, ArrayBuilder<KeywordRecommendation> result)
	{
		foreach (SyntaxKind kind in slot.Kinds)
		{
			if ((kind.IsKeyword() || kind.IsPropertyKeyword()) && SyntaxFacts.IsSyntaxAllowed(startingNode, kind))
			{
				result.Add(new KeywordRecommendation(kind, visitedDefinitions.Last()));
			}
			else if (!kind.IsAnyToken())
			{
				KeywordRecommendation[] items = FindApplicableKeywords(startingNode, null, SyntaxDefinition.SyntaxKindToDefintion(kind), 0, startedTyping: false, tokenIsBeforeTheSlot: true, visitedDefinitions);
				result.AddRange(items);
			}
		}
	}

	private void FindApplicableKeywordsForPropertyValue(SyntaxNode startingNode, SyntaxNode syntaxNode, List<SyntaxNodeDefinition> visitedDefinitions, ArrayBuilder<KeywordRecommendation> result)
	{
		SyntaxNodeDefinition syntaxNodeDefinition = (syntaxNode as PropertySyntax)?.GetPropertySyntaxDefinition();
		if (syntaxNodeDefinition != null)
		{
			KeywordRecommendation[] items = FindApplicableKeywords(startingNode, null, syntaxNodeDefinition, 0, startedTyping: false, tokenIsBeforeTheSlot: true, visitedDefinitions);
			result.AddRange(items);
		}
	}

	private static bool IsNodeNullOrInvalid(InternalSyntaxNode slotSyntaxNode)
	{
		if (slotSyntaxNode != null && !slotSyntaxNode.ContainsDiagnostics)
		{
			return slotSyntaxNode.IsMissing;
		}
		return true;
	}

	private static bool IsNodeNotNullButInvalid(InternalSyntaxNode slotSyntaxNode)
	{
		if (slotSyntaxNode != null && slotSyntaxNode.ContainsDiagnostics)
		{
			return !slotSyntaxNode.IsMissing;
		}
		return false;
	}

	private static SyntaxNode GetFirstNodeWithNonTerminalToken(SyntaxNode node, SyntaxToken token, out int slot)
	{
		slot = -1;
		if (node != null)
		{
			slot = node.Node.FindSlotIndexContainingOffset(token.Position - node.Position);
			if (slot == node.Node.SlotCount - 1 && !node.Node.Definition.Slots.Value[slot].IsList)
			{
				return GetFirstNodeWithNonTerminalToken(node.Parent, token, out slot);
			}
			if ((token.Kind == SyntaxKind.SemicolonToken || token.Kind == SyntaxKind.CloseParenToken) && node.Parent.Kind == SyntaxKind.ControlAddInObject)
			{
				return GetFirstNodeWithNonTerminalToken(node.Parent, token, out slot);
			}
			return node;
		}
		return null;
	}

	private static ImmutableArray<CompletionItem> CreateSimpleCompletionList(params KeywordRecommendation[] items)
	{
		PooledHashSet<KeywordRecommendation> instance = PooledHashSet<KeywordRecommendation>.GetInstance();
		for (int i = 0; i < items.Length; i++)
		{
			instance.Add(items[i]);
		}
		ImmutableArray<CompletionItem> result = instance.Select((KeywordRecommendation keywordRecommendation) =>
		{
			string text = keywordRecommendation.SyntaxKind.GetText();
			return TextRecommender.CreateItem(descriptionText: keywordRecommendation.SyntaxNodeDefinition.GetDocumentation(), displayText: text, glyph: TextRecommender.CategorizeKeyword(text));
		}).ToImmutableArrayOrEmpty();
		instance.Free();
		return result;
	}
}
