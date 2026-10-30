using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal sealed class TriggersRecommender : TextRecommender
{
	protected internal override async Task<IEnumerable<CompletionItem>> RecommendTextAsync(CompletionContext context, MemberSyntaxContext syntaxContext, CancellationToken cancellationToken)
	{
		if (!syntaxContext.General.HasFlag(GeneralContexts.Trigger))
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		SyntaxNode parent = syntaxContext.TargetToken.Parent.Parent;
		ISymbolWithTriggers symbol = syntaxContext.SemanticModel.GetDeclaredSymbol(parent) as ISymbolWithTriggers;
		ImmutableHashSet<string> existingTriggers = ImmutableHashSet<string>.Empty;
		if (symbol != null)
		{
			existingTriggers = symbol.Triggers.Select((IMethodSymbol t) => t.Name).ToImmutableHashSet();
			if (symbol.IsKind(SymbolKind.Control))
			{
				ControlSymbol control = (ControlSymbol)symbol;
				if (control.RelatedControlAddInSymbol != null)
				{
					return from x in control.GetMembers().OfType<SynthesizedMethodSymbol>()
						where IsControlAddInEvent(control.RelatedControlAddInSymbol, x.Name) && !existingTriggers.Contains(x.Name)
						select CompletionUtilities.CreateTriggerCompletionItem(x.Name, x.ToDisplayString(SymbolDisplayFormat.MethodCompletionFormat));
				}
			}
		}
		IEnumerable<TriggerTypeInfo> source = from t in parent.Kind.GetDeclaredTriggerTypeInfos()
			where !existingTriggers.Contains(t.Name) && IsTriggerAllowedOnSymbol(symbol, t)
			select symbol.GetTriggerTypeInfo(t.Name);
		if (syntaxContext.LeftToken.GetNextToken().Kind == SyntaxKind.OpenParenToken)
		{
			return source.Select((TriggerTypeInfo t) => CompletionUtilities.CreateTriggerCompletionItem(t.Name, t.Name));
		}
		return source.Select((TriggerTypeInfo t) => CompletionUtilities.CreateTriggerCompletionItem(t.Name, t.GetSignatureString(appendKeyword: false)));
	}

	private static bool IsControlAddInEvent(ControlAddInTypeSymbol controlAddIn, string memberName)
	{
		return Enumerable.Count(controlAddIn.GetMembers(memberName), (Symbol x) => x.Kind == SymbolKind.Event) == 1;
	}

	private static bool IsTriggerAllowedOnSymbol(ISymbolWithTriggers symbol, TriggerTypeInfo triggerInfo)
	{
		if (symbol?.GetTriggerTypeInfo(triggerInfo.Name) == null)
		{
			return false;
		}
		if (!symbol.IsKind(SymbolKind.Codeunit))
		{
			return true;
		}
		switch (((Symbol)symbol).GetEnumPropertyValue<CodeunitSubtypeKind>(PropertyKind.Subtype))
		{
		case CodeunitSubtypeKind.Install:
			if (!triggerInfo.IsCodeunitInstallTriggerTypeInfo())
			{
				return triggerInfo.IsCodeunitOnRunTriggerTypeInfo();
			}
			return true;
		case CodeunitSubtypeKind.Upgrade:
			if (!triggerInfo.IsCodeunitUpgradeTriggerTypeInfo())
			{
				return triggerInfo.IsCodeunitOnRunTriggerTypeInfo();
			}
			return true;
		case CodeunitSubtypeKind.TestRunner:
			if (!triggerInfo.IsCodeunitTestRunnerTriggerTypeInfo())
			{
				return triggerInfo.IsCodeunitOnRunTriggerTypeInfo();
			}
			return true;
		default:
			return triggerInfo.IsCodeunitOnRunTriggerTypeInfo();
		}
	}
}
