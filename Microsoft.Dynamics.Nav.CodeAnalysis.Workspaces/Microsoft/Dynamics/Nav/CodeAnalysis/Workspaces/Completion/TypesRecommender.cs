using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal sealed class TypesRecommender : TextRecommender
{
	private readonly Lazy<ImmutableArray<string>> parameterTypes = new Lazy<ImmutableArray<string>>(() => GetTypes(NavTypeKind._Parameter));

	private readonly Lazy<ImmutableArray<string>> variableTypes = new Lazy<ImmutableArray<string>>(() => GetTypes(NavTypeKind._Variable));

	private readonly Lazy<ImmutableArray<string>> returnValueTypes = new Lazy<ImmutableArray<string>>(() => GetTypes(NavTypeKind._ReturnValue));

	private readonly Lazy<ImmutableArray<string>> fieldTypes = new Lazy<ImmutableArray<string>>(() => GetTypes(NavTypeKind._Field));

	private readonly Lazy<ImmutableArray<string>> genericTypes = new Lazy<ImmutableArray<string>>(GetGenericTypes);

	protected internal override async Task<IEnumerable<CompletionItem>> RecommendTextAsync(CompletionContext context, MemberSyntaxContext syntaxContext, CancellationToken cancellationToken)
	{
		if (!syntaxContext.General.HasFlag(GeneralContexts.Type))
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		IEnumerable<string> result;
		switch (LookupSymbol(syntaxContext))
		{
		case SyntaxKind.Parameter:
			result = parameterTypes.Value;
			break;
		case SyntaxKind.VariableDeclaration:
		case SyntaxKind.Array:
			result = variableTypes.Value;
			break;
		case SyntaxKind.ReturnValue:
			result = returnValueTypes.Value;
			break;
		case SyntaxKind.GenericDataType:
			result = returnValueTypes.Value;
			result = result.Concat(genericTypes.Value);
			break;
		case SyntaxKind.Field:
			result = fieldTypes.Value;
			break;
		default:
			return ImmutableArray<CompletionItem>.Empty;
		}
		SyntaxToken leftToken = syntaxContext.LeftToken;
		if (leftToken.Kind == SyntaxKind.IdentifierToken)
		{
			return TextRecommender.ToCompletionItemArray(result, Glyph.Type, leftToken.Span);
		}
		return TextRecommender.ToCompletionItemArray(result, Glyph.Type);
	}

	private static SyntaxKind LookupSymbol(MemberSyntaxContext context)
	{
		SyntaxToken targetToken = context.TargetToken;
		if (targetToken.IsKind(SyntaxKind.ColonToken))
		{
			SyntaxNode parent = targetToken.Parent;
			if (parent != null)
			{
				SyntaxKind kind = parent.Kind;
				if ((uint)(kind - 240) <= 2u)
				{
					return parent.Kind;
				}
			}
		}
		else
		{
			if (targetToken.IsKind(SyntaxKind.SemicolonToken) && targetToken.ParentIsKind(SyntaxKind.Field))
			{
				return SyntaxKind.Field;
			}
			if (targetToken.IsInsideGenericDataTypeArgumentList())
			{
				return SyntaxKind.GenericDataType;
			}
			if (targetToken.IsInsideArrayDataTypeArgumentList())
			{
				return SyntaxKind.Array;
			}
			if (targetToken.IsDotNetTypeSubtype())
			{
				return SyntaxKind.DotNetTypeReference;
			}
		}
		return SyntaxKind.None;
	}

	private static ImmutableArray<string> GetNavTypeKinds(Predicate<NavTypeKind> function)
	{
		return (from NavTypeKind t in typeof(NavTypeKind).GetTypeInfo().GetEnumValues()
			where function(t)
			select t.ToString()).ToImmutableArray();
	}

	private static ImmutableArray<string> GetTypes(NavTypeKind kind)
	{
		return GetNavTypeKinds((NavTypeKind e) => (e & kind) != 0 && e != kind);
	}

	private static ImmutableArray<string> GetGenericTypes()
	{
		return GetNavTypeKinds((NavTypeKind e) => e.IsGenericType());
	}
}
