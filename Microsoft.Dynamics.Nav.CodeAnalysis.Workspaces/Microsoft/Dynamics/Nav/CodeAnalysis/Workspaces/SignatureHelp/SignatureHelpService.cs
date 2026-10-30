using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.SignatureHelp;

internal class SignatureHelpService : ISignatureHelpService, ILanguageService
{
	public async Task<SignatureHelpResult<SymbolSignature>> ProvideSymbolicHelpAsync(Document document, int position, CancellationToken cancellationToken)
	{
		SyntaxTree tree = await document.GetSyntaxTreeAsync(cancellationToken);
		await tree.GetRootAsync(cancellationToken);
		for (SyntaxNode parent = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTreeExtensions.FindTokenOnLeftOfPosition(tree, position, cancellationToken).Parent; parent != null; parent = parent.Parent)
		{
			if (parent.IsKind(SyntaxKind.InvocationExpression))
			{
				InvocationExpressionSyntax invocation = (InvocationExpressionSyntax)parent;
				TextSpan? textSpan = invocation.ArgumentList?.Span;
				if ((!textSpan.HasValue || textSpan.GetValueOrDefault().Start != position) && (textSpan?.Contains(position) ?? false))
				{
					ArgumentListSyntax argumentList = invocation.ArgumentList;
					return BuildInvocationSignatures(argumentList, await document.GetSemanticModelAsync(cancellationToken), invocation.Expression, position, cancellationToken);
				}
			}
			if (parent.IsKind(SyntaxKind.MemberAttribute))
			{
				MemberAttributeSyntax memberAttributeSyntax = (MemberAttributeSyntax)parent;
				TextSpan? textSpan2 = memberAttributeSyntax.ArgumentList?.Span;
				if ((!textSpan2.HasValue || textSpan2.GetValueOrDefault().Start != position) && (textSpan2?.Contains(position) ?? false))
				{
					MemberAttributeSyntax attribute = memberAttributeSyntax;
					return BuildAttributeSignatures(attribute, await document.GetSemanticModelAsync(cancellationToken), position);
				}
			}
		}
		return null;
	}

	private static SignatureHelpResult<SymbolSignature> BuildInvocationSignatures(ArgumentListSyntax argumentList, SemanticModel semanticModel, ExpressionSyntax receiver, int position, CancellationToken cancellationToken)
	{
		ArrayBuilder<SymbolSignature> instance = ArrayBuilder<SymbolSignature>.GetInstance();
		foreach (IMethodSymbol methodOverload in GetMethodOverloads(semanticModel, receiver, cancellationToken))
		{
			instance.Add(new SymbolSignature(methodOverload, methodOverload.Parameters));
		}
		int num = 0;
		using (IEnumerator<SyntaxToken> enumerator2 = argumentList.Arguments.GetSeparators().GetEnumerator())
		{
			while (enumerator2.MoveNext() && enumerator2.Current.Span.Start < position)
			{
				num++;
			}
		}
		return new SignatureHelpResult<SymbolSignature>(instance.ToArrayAndFree(), 0, num);
	}

	private static IEnumerable<IMethodSymbol> GetMethodOverloads(SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
	{
		ISymbol symbol = null;
		SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(node, cancellationToken);
		if (symbolInfo.Symbol != null)
		{
			symbol = symbolInfo.Symbol;
		}
		else if (!symbolInfo.CandidateSymbols.IsEmpty)
		{
			symbol = symbolInfo.CandidateSymbols.First();
		}
		if (symbol == null || symbol.ContainingType == null)
		{
			return new IMethodSymbol[0];
		}
		IContainerSymbol symbolThroughWhichMethodIsAccessed = GetSymbolThroughWhichMethodIsAccessed(symbol, semanticModel, node, cancellationToken);
		int position = node.Position;
		string name = symbol.Name;
		return semanticModel.LookupSymbols(position, LookupOptions.AllMethods, symbolThroughWhichMethodIsAccessed, name, cancellationToken).OfType<IMethodSymbol>();
	}

	private static IContainerSymbol GetSymbolThroughWhichMethodIsAccessed(ISymbol target, SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
	{
		if (node.Kind == SyntaxKind.MemberAccessExpression)
		{
			MemberAccessExpressionSyntax memberAccessExpressionSyntax = (MemberAccessExpressionSyntax)node;
			IContainerSymbol result;
			if ((result = semanticModel.GetSymbolInfo(memberAccessExpressionSyntax.Expression, cancellationToken).Symbol?.GetTypeSymbol()) != null)
			{
				return result;
			}
		}
		return target.ContainingType;
	}

	private static SignatureHelpResult<SymbolSignature> BuildAttributeSignatures(MemberAttributeSyntax attribute, SemanticModel semanticModel, int position)
	{
		AttributeSymbol attributeSymbol = semanticModel.GetDeclaredSymbol(attribute.ArgumentList.Parent) as AttributeSymbol;
		if (attributeSymbol?.AttributeInfo == null)
		{
			return null;
		}
		ArrayBuilder<SymbolSignature> instance = ArrayBuilder<SymbolSignature>.GetInstance();
		SymbolSignature item = new SymbolSignature(attributeSymbol, attributeSymbol.SignatureArguments);
		instance.Add(item);
		int num = 0;
		using (IEnumerator<SyntaxToken> enumerator = attribute.ArgumentList.Arguments.GetSeparators().GetEnumerator())
		{
			while (enumerator.MoveNext() && enumerator.Current.Span.Start <= position)
			{
				num++;
			}
		}
		return new SignatureHelpResult<SymbolSignature>(instance.ToArrayAndFree(), 0, num);
	}

	public async Task<SignatureHelpResult<SyntaxSignature>> ProvideSyntacticHelpAsync(Document document, int position, CancellationToken cancellationToken)
	{
		SyntaxToken token = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTreeExtensions.FindTokenOnLeftOfPosition(await document.GetSyntaxTreeAsync(cancellationToken), position, cancellationToken);
		token = token.GetPreviousTokenIfTouchingWord(position);
		if (token.Parent == null)
		{
			return null;
		}
		SyntaxNode parent = token.Parent;
		SignatureHelpResult<SyntaxSignature> signatureHelpResult = ProvideSyntacticalHelp(parent, position);
		if (signatureHelpResult == null && parent.Parent != null)
		{
			signatureHelpResult = ProvideSyntacticalHelp(parent.Parent, position);
			if (signatureHelpResult == null && parent.Parent.IsKind(SyntaxKind.LiteralExpression) && parent.Parent.Parent != null)
			{
				signatureHelpResult = ProvideSyntacticalHelp(parent.Parent.Parent, position);
			}
		}
		return signatureHelpResult;
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	private static SignatureHelpResult<SyntaxSignature> ProvideSyntacticalHelp(SyntaxNode node, int position)
	{
		SyntaxNodeSlotDefinition currentDefinition = null;
		InternalSyntaxNode currentNode = null;
		ImmutableArray<SyntaxNodeSlotDefinition> slots = node.Node.Definition.Slots.Value;
		int slotIndex = 0;
		int currentPosition = node.SpanStart;
		Func<bool> func = () =>
		{
			if (slotIndex >= slots.Length)
			{
				return false;
			}
			if (currentNode != null && slotIndex > 0)
			{
				currentPosition += currentNode.FullWidth - currentNode.GetLeadingTriviaWidth();
			}
			currentDefinition = slots[slotIndex];
			currentNode = node.Node.GetSlot(slotIndex);
			int num4 = slotIndex + 1;
			slotIndex = num4;
			return true;
		};
		SyntaxKind? syntaxKind = null;
		while (func())
		{
			if (currentNode == null || currentNode.IsMissing)
			{
				return null;
			}
			if (currentNode.Kind.IsKeyword())
			{
				syntaxKind = currentNode.Kind;
				break;
			}
		}
		if (!syntaxKind.HasValue)
		{
			return null;
		}
		func();
		InternalSyntaxNode internalSyntaxNode = currentNode;
		if (internalSyntaxNode == null || !internalSyntaxNode.IsMissing)
		{
			InternalSyntaxNode internalSyntaxNode2 = currentNode;
			if (internalSyntaxNode2 != null && internalSyntaxNode2.Kind == SyntaxKind.OpenParenToken)
			{
				ArrayBuilder<SyntaxKind> instance = ArrayBuilder<SyntaxKind>.GetInstance();
				ArrayBuilder<SyntaxNodeSlotDefinition> instance2 = ArrayBuilder<SyntaxNodeSlotDefinition>.GetInstance();
				bool flag = false;
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				while (func())
				{
					if (currentDefinition.Kinds.Length > 0 && currentDefinition.Kinds[0] == SyntaxKind.CloseParenToken)
					{
						InternalSyntaxNode internalSyntaxNode3 = currentNode;
						if (internalSyntaxNode3 == null || internalSyntaxNode3.IsMissing || currentPosition >= position)
						{
							break;
						}
						return null;
					}
					if (flag)
					{
						DebugAssertHelper.Assert(currentDefinition.Kinds.Length == 1);
						SyntaxKind item = currentDefinition.Kinds.SingleOrDefault();
						instance.Add(item);
						InternalSyntaxNode internalSyntaxNode4 = currentNode;
						if (internalSyntaxNode4 != null && !internalSyntaxNode4.IsMissing)
						{
							num2++;
							if (currentPosition < position)
							{
								num3++;
							}
						}
					}
					else
					{
						instance2.Add(currentDefinition);
						InternalSyntaxNode internalSyntaxNode5 = currentNode;
						if (internalSyntaxNode5 != null && !internalSyntaxNode5.IsMissing)
						{
							num++;
						}
					}
					flag = !flag;
				}
				if (num2 > 0 && num2 == num)
				{
					num++;
				}
				return BuildSyntaxHelp(new SyntaxSignature(node.Node.Definition, syntaxKind.Value, instance2.ToArrayAndFree(), instance.ToArrayAndFree()), num3, num);
			}
		}
		return null;
	}

	private static SignatureHelpResult<SyntaxSignature> BuildSyntaxHelp(SyntaxSignature fullSignature, int activeParameter, int parametersSpecified)
	{
		IEnumerable<SyntaxSignature> enumerable = BuildSignatureCombinations(fullSignature);
		int num = 0;
		using (IEnumerator<SyntaxSignature> enumerator = enumerable.GetEnumerator())
		{
			while (enumerator.MoveNext() && enumerator.Current.Parameters.Count < parametersSpecified)
			{
				num++;
			}
		}
		return new SignatureHelpResult<SyntaxSignature>(enumerable, num, activeParameter);
	}

	private static IEnumerable<SyntaxSignature> BuildSignatureCombinations(SyntaxSignature fullSignature)
	{
		ArrayBuilder<SyntaxSignature> instance = ArrayBuilder<SyntaxSignature>.GetInstance();
		instance.Add(fullSignature);
		for (int i = 0; i < instance.Count; i++)
		{
			SyntaxSignature syntaxSignature = instance[i];
			for (int j = 0; j < syntaxSignature.Parameters.Count; j++)
			{
				if (!syntaxSignature.Parameters[j].IsOptional)
				{
					continue;
				}
				ArrayBuilder<SyntaxNodeSlotDefinition> instance2 = ArrayBuilder<SyntaxNodeSlotDefinition>.GetInstance();
				ArrayBuilder<SyntaxKind> instance3 = ArrayBuilder<SyntaxKind>.GetInstance();
				for (int k = 0; k < syntaxSignature.Parameters.Count; k++)
				{
					if (k != j)
					{
						if (instance2.Count > 0)
						{
							instance3.Add(syntaxSignature.Separators[k - 1]);
						}
						instance2.Add(syntaxSignature.Parameters[k]);
					}
				}
				SyntaxNodeSlotDefinition[] newParameters = instance2.ToArrayAndFree();
				SyntaxKind[] separators = instance3.ToArrayAndFree();
				if (!instance.Any((SyntaxSignature x) => x.Parameters.SequenceEqual(newParameters)))
				{
					instance.Add(new SyntaxSignature(fullSignature.Definition, fullSignature.KeywordKind, newParameters, separators));
				}
			}
		}
		return instance.ToArrayAndFree().Reverse();
	}
}
