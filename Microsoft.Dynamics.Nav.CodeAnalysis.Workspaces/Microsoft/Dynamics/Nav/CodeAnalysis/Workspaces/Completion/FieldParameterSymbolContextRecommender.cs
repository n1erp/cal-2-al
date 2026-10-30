using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class FieldParameterSymbolContextRecommender : ContextAwareSymbolRecommender
{
	protected internal override bool IsExclusive => true;

	internal FieldParameterSymbolContextRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (!Context.IsArgumentExpression())
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		InvocationExpressionSyntax node = Context.LeftToken.Parent.FirstAncestorOrSelf<InvocationExpressionSyntax>();
		if (node == null || node.Expression == null)
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		string memberName = string.Empty;
		string lhs = string.Empty;
		node.Expression.SplitMemberAccess(out lhs, out memberName);
		Tuple<ITypeSymbol, ImmutableArray<ISymbol>> tuple = await Context.GetSymbolInfoAtPositionAsync(node.SpanStart, cancellationToken);
		if (tuple == null)
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		IContainerSymbol containerSymbol = tuple.Item1;
		if (containerSymbol == null)
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		BuiltInMethodTypeSymbol builtInMethodTypeSymbol = null;
		bool flag = false;
		if (tuple.Item2.Length == 1 && tuple.Item2[0].Kind == SymbolKind.Method)
		{
			MethodSymbol methodSymbol = (MethodSymbol)tuple.Item2[0];
			if (methodSymbol.MethodKind == MethodKind.BuiltInMethod)
			{
				builtInMethodTypeSymbol = (BuiltInMethodTypeSymbol)methodSymbol;
				containerSymbol = GetBuiltinMethodOwnerSymbol(builtInMethodTypeSymbol, cancellationToken);
				flag = true;
			}
		}
		else
		{
			builtInMethodTypeSymbol = Context.SemanticModel.LookupSymbols(node.SpanStart, LookupOptions.Default, containerSymbol, null, cancellationToken).FirstOrDefault((ISymbol s) => s.Kind == SymbolKind.Method && string.Compare(s.Name, memberName, StringComparison.OrdinalIgnoreCase) == 0) as BuiltInMethodTypeSymbol;
		}
		if (builtInMethodTypeSymbol == null || builtInMethodTypeSymbol.ParameterTypes.Length == 0)
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		int argumentOrdinal = GetArgumentOrdinal(node);
		if (argumentOrdinal > builtInMethodTypeSymbol.ParameterTypes.Length - 1)
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		BuiltInParameterSymbol builtInParameterSymbol = builtInMethodTypeSymbol.Parameters[argumentOrdinal] as BuiltInParameterSymbol;
		if (builtInParameterSymbol == null || !builtInParameterSymbol.IsMemberReference || !builtInParameterSymbol.IsSameVar)
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		IEnumerable<ISymbol> enumerable = RecommendationHelper.GetMembers(Context, containerSymbol, flag, cancellationToken);
		if (!flag)
		{
			enumerable = enumerable.Concat(tuple.Item2);
		}
		return enumerable;
	}

	private int GetArgumentOrdinal(InvocationExpressionSyntax invocation)
	{
		int num = 0;
		using (IEnumerator<SyntaxToken> enumerator = invocation.ArgumentList.Arguments.GetSeparators().GetEnumerator())
		{
			while (enumerator.MoveNext() && enumerator.Current.Span.Start <= Context.Position)
			{
				num++;
			}
		}
		return num;
	}

	private IContainerSymbol GetBuiltinMethodOwnerSymbol(BuiltInMethodTypeSymbol methodSymbol, CancellationToken cancellationToken)
	{
		if (DeclaringObject.Kind == SymbolKind.Codeunit)
		{
			ISymbol enclosingSymbol = Context.SemanticModel.GetEnclosingSymbol(Context.Position, cancellationToken);
			if (enclosingSymbol == null || enclosingSymbol.Kind != SymbolKind.Method)
			{
				return null;
			}
			TableTypeSymbol relatedTable = ((CodeunitTypeSymbol)DeclaringObject).RelatedTable;
			if (relatedTable != null)
			{
				return relatedTable;
			}
		}
		foreach (WithStatementSyntax ancestor in Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxNodeExtensions.GetAncestors<WithStatementSyntax>(Context.LeftToken.Parent))
		{
			CodeExpressionSyntax withId = ancestor.WithId;
			TypeSymbol typeSymbol = Context.SemanticModel.GetSymbolInfo(withId, cancellationToken).GetBestOrAllSymbols().FirstOrDefault()?.GetTypeSymbol() as TypeSymbol;
			TypeSymbol typeSymbol2 = typeSymbol?.BaseType;
			while (typeSymbol2 != null)
			{
				if (typeSymbol2 == methodSymbol.ContainingType)
				{
					return typeSymbol;
				}
				typeSymbol2 = typeSymbol2?.BaseType;
			}
		}
		return (from SynthesizedGlobalVariableSymbol s in from m in DeclaringObject.GetMembers()
				where m.Kind == SymbolKind.GlobalVariable && ((VariableSymbol)m).VariableKind == VariableKind.Synthesized && ((SynthesizedGlobalVariableSymbol)m).Type.Kind == SymbolKind.Record
				select m
			select s.Type).FirstOrDefault();
	}
}
