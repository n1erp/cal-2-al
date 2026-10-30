using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class IsolatedScopeBinder : SimpleBinder<Symbol>
{
	internal delegate BoundExpression ReceiverFactory(ContainerSymbol owningSymbol);

	private readonly ReceiverFactory receiverFactory;

	internal ContainerSymbol Container => (ContainerSymbol)ContainingMember;

	internal IsolatedScopeBinder(ContainerSymbol symbol, ReceiverFactory receiverFactory, Binder next, SyntaxNode declarationSyntax)
		: base((Symbol)symbol, next, declarationSyntax)
	{
		this.receiverFactory = receiverFactory;
	}

	protected override void AddLookupSymbolsInfoInSingleBinder(LookupSymbolsInfo info, LookupOptions options, Binder originalBinder)
	{
		TypeSymbol typeSymbol = Container as TypeSymbol;
		if (typeSymbol != null)
		{
			Binder.AddMemberLookupSymbolsInfo(info, typeSymbol, options, originalBinder);
			return;
		}
		foreach (Symbol candidateMember in Binder.GetCandidateMembers(Container, null))
		{
			if (originalBinder.CanAddLookupSymbolInfo(candidateMember, options, null))
			{
				info.AddSymbol(candidateMember, candidateMember.Name, 0);
			}
		}
	}

	internal override void LookupSymbolsInSingleBinder(LookupResult result, string name, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		BoundExpression receiver = receiverFactory?.Invoke(Container);
		Binder.LookupMembersInternal(result, Container, name, options, originalBinder, diagnose, ref useSiteDiagnostics, receiver);
	}
}
