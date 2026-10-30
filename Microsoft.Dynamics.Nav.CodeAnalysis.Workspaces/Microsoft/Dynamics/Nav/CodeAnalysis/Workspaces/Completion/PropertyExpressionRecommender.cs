using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal abstract class PropertyExpressionRecommender<T, U> : ContextAwareSymbolRecommender where T : PropertyValueSyntax where U : PropertyExpressionSyntax
{
	private TableTypeSymbol referencedTable;

	protected T DeclaringSyntax { get; }

	protected U PropertyExpressionSyntax { get; }

	protected virtual bool HasContext
	{
		get
		{
			if (PropertyExpressionSyntax != null || Context.TargetToken.IsKind(SyntaxKind.CommaToken))
			{
				return DeclaringSyntax != null;
			}
			return false;
		}
	}

	protected TableTypeSymbol ReferencedTable
	{
		get
		{
			if (referencedTable != null)
			{
				return referencedTable;
			}
			referencedTable = SetReferencedTable();
			return referencedTable;
		}
	}

	protected PropertyExpressionRecommender(MemberSyntaxContext context)
		: base(context)
	{
		PropertyExpressionSyntax = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<U>(Context.TargetToken);
		DeclaringSyntax = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<T>(Context.TargetToken);
	}

	protected virtual TableTypeSymbol GetSourceTable()
	{
		return DeclaringObject?.GetRelatedTableSymbol();
	}

	protected abstract TableTypeSymbol SetReferencedTable();

	protected TableTypeSymbol GetReferencedTable(SyntaxNode memberAccess)
	{
		if (memberAccess == null || memberAccess.IsMissing)
		{
			return null;
		}
		memberAccess.SplitMemberAccess(out var lhs, out var _);
		ImmutableArray<ISymbol> objectSymbolsByNameAcrossModules = Context.SemanticModel.Compilation.GetObjectSymbolsByNameAcrossModules(SymbolKind.Table, lhs);
		if (objectSymbolsByNameAcrossModules.Length <= 0)
		{
			return null;
		}
		return objectSymbolsByNameAcrossModules[0] as TableTypeSymbol;
	}

	protected async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken, IEnumerable<PropertyExpressionSyntax> conditions, bool shouldUseLhsFieldsFromSource = true)
	{
		if (!HasContext || !Context.General.HasFlag(GeneralContexts.AnyComplexPropertyExpression))
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		SyntaxToken targetToken = Context.TargetToken;
		if ((targetToken.IsKind(SyntaxKind.OpenParenToken, SyntaxKind.CommaToken) && targetToken.ParentIsKind(SyntaxKind.TableFilterExpression)) || (targetToken.IsKind(SyntaxKind.CommaToken) && targetToken.ParentIsKind(SyntaxKind.TableFilterPropertyValue)))
		{
			HashSet<string> existingFields = new HashSet<string>();
			GetUsedFieldsInSyntax(existingFields, conditions);
			return from f in RecommendationHelper.GetFields(Context, shouldUseLhsFieldsFromSource ? GetSourceTable() : ReferencedTable, cancellationToken)
				where !existingFields.Contains(f.Name.ToUpperInvariant())
				select f;
		}
		if (targetToken.IsKind(SyntaxKind.OpenParenToken) && targetToken.ParentIsKind(SyntaxKind.SimpleFieldExpression, SyntaxKind.FieldUpperLimitExpression, SyntaxKind.FieldUpperLimitExpression))
		{
			return RecommendationHelper.GetFields(Context, (!shouldUseLhsFieldsFromSource) ? GetSourceTable() : ReferencedTable, cancellationToken);
		}
		if (targetToken.IsKind(SyntaxKind.OpenParenToken) && targetToken.ParentIsKind(SyntaxKind.ConstExpression))
		{
			return GetPossibleFieldValues(((ConstExpressionSyntax)targetToken.Parent).LeftHandSide, shouldUseLhsFieldsFromSource);
		}
		FilterExpressionSyntax ancestor = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<FilterExpressionSyntax>(targetToken);
		if (ancestor != null && (targetToken.IsKind(SyntaxKind.OpenParenToken, SyntaxKind.AndFilterKeyword, SyntaxKind.OrFilterKeyword, SyntaxKind.DotDotToken) || targetToken.IsKind(SyntaxKind.IdentifierToken) || targetToken.Kind.IsLiteral()))
		{
			return GetPossibleFieldValues(ancestor.LeftHandSide, shouldUseLhsFieldsFromSource);
		}
		return SpecializedCollections.EmptyEnumerable<ISymbol>();
	}

	protected IEnumerable<ISymbol> GetPossibleFieldValues(IdentifierNameSyntax lhs, bool shouldUseLhsFieldsFromSource)
	{
		FieldSymbol fieldSymbol = (shouldUseLhsFieldsFromSource ? GetSourceTable() : ReferencedTable)?.GetCodeMembers(lhs.Unquoted()).FirstOrDefault((Symbol m) => m.Kind == SymbolKind.Field) as FieldSymbol;
		if (fieldSymbol != null)
		{
			switch (fieldSymbol.Type.Kind)
			{
			case SymbolKind.Enum:
			{
				EnumTypeSymbol enumTypeSymbol = (EnumTypeSymbol)fieldSymbol.Type;
				SemanticModel semanticModel = Context.SemanticModel;
				int position = Context.Position;
				IContainerSymbol container = enumTypeSymbol;
				return semanticModel.LookupSymbols(position, enumTypeSymbol.GetLookupOptions() | LookupOptions.MustBeOption, container);
			}
			case SymbolKind.OptionType:
				return ((OptionTypeSymbol)fieldSymbol.Type).GetCodeMembers();
			}
		}
		return SpecializedCollections.EmptyEnumerable<ISymbol>();
	}

	protected static void GetUsedFieldsInSyntax(HashSet<string> set, IEnumerable<PropertyExpressionSyntax> conditions)
	{
		foreach (PropertyExpressionSyntax condition in conditions)
		{
			IdentifierNameSyntax identifierNameSyntax = null;
			switch (condition.Kind)
			{
			case SyntaxKind.ConstExpression:
				identifierNameSyntax = ((ConstExpressionSyntax)condition).LeftHandSide;
				break;
			case SyntaxKind.FilterExpression:
				identifierNameSyntax = ((FilterExpressionSyntax)condition).LeftHandSide;
				break;
			case SyntaxKind.SimpleFieldExpression:
				identifierNameSyntax = ((SimpleFieldExpressionSyntax)condition).LeftHandSide;
				break;
			case SyntaxKind.FieldFilterExpression:
				identifierNameSyntax = ((FieldFilterExpressionSyntax)condition).LeftHandSide;
				break;
			case SyntaxKind.FieldUpperLimitFilterExpression:
				identifierNameSyntax = ((FieldUpperLimitFilterExpressionSyntax)condition).LeftHandSide;
				break;
			case SyntaxKind.FieldUpperLimitExpression:
				identifierNameSyntax = ((FieldUpperLimitExpressionSyntax)condition).LeftHandSide;
				break;
			}
			if (identifierNameSyntax == null)
			{
				continue;
			}
			string valueText = identifierNameSyntax.Identifier.ValueText;
			if (!string.IsNullOrEmpty(valueText))
			{
				string item = valueText.ToUpperInvariant();
				if (!set.Contains(item))
				{
					set.Add(item);
				}
			}
		}
	}
}
