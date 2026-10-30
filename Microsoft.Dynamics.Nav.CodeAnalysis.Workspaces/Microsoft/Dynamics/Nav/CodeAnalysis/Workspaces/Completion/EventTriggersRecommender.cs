using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal sealed class EventTriggersRecommender : TextRecommender
{
	protected internal override async Task<IEnumerable<CompletionItem>> RecommendTextAsync(CompletionContext context, MemberSyntaxContext syntaxContext, CancellationToken cancellationToken)
	{
		if (!syntaxContext.General.HasFlag(GeneralContexts.EventTrigger))
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (!(syntaxContext.TargetToken.Parent is EventTriggerDeclarationSyntax { Publisher: not null } eventTriggerDeclarationSyntax))
		{
			return ImmutableArray<CompletionItem>.Empty;
		}
		ObjectSyntax containingObjectSyntax = eventTriggerDeclarationSyntax.GetContainingObjectSyntax();
		if (containingObjectSyntax == null)
		{
			return ImmutableArray<CompletionItem>.Empty;
		}
		IContainerSymbol declaredSymbol = syntaxContext.SemanticModel.GetDeclaredSymbol(containingObjectSyntax);
		string publisherName = eventTriggerDeclarationSyntax.Publisher.GetIdentifierOrLiteralValue();
		DotNetTypeSymbol dotNetTypeSymbol = (from VariableSymbol v in from m in declaredSymbol.GetMembers(publisherName)
				where m.Kind == SymbolKind.GlobalVariable
				select m
			where v.Type.Kind == SymbolKind.DotNet && v.Type.NavTypeKind == NavTypeKind.DotNet
			select v.Type).Cast<DotNetTypeSymbol>().FirstOrDefault();
		if (dotNetTypeSymbol == null)
		{
			return ImmutableArray<CompletionItem>.Empty;
		}
		IEnumerable<SourceDotNetEventTriggerSymbol> existingTriggersForPublisher = from SourceDotNetEventTriggerSymbol trigger in from s in declaredSymbol.GetMembers()
				where s.Kind == SymbolKind.Method && ((MethodSymbol)s).MethodKind == MethodKind.EventTrigger
				select s
			where string.Equals(trigger.PublisherName, publisherName, StringComparison.OrdinalIgnoreCase)
			select trigger;
		return from x in dotNetTypeSymbol.Events
			where !existingTriggersForPublisher.Any((SourceDotNetEventTriggerSymbol existingTrigger) => string.Equals(existingTrigger.EventName, x.Name, StringComparison.OrdinalIgnoreCase))
			select CompletionUtilities.CreateTriggerCompletionItem(x.Name, x.ToDisplayString(SymbolDisplayFormat.MethodCompletionFormat));
	}
}
