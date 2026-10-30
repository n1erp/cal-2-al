using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

public static class CSharpExtensions
{
	internal static bool IsCSharpKind(int rawKind)
	{
		return (uint)(rawKind - 2) > 8190u;
	}

	public static SyntaxKind Kind(this SyntaxToken token)
	{
		int rawKind = token.RawKind;
		if (!IsCSharpKind(rawKind))
		{
			return SyntaxKind.None;
		}
		return (SyntaxKind)rawKind;
	}

	public static SyntaxKind Kind(this SyntaxTrivia trivia)
	{
		int rawKind = trivia.RawKind;
		if (!IsCSharpKind(rawKind))
		{
			return SyntaxKind.None;
		}
		return (SyntaxKind)rawKind;
	}

	public static SyntaxKind Kind(this SyntaxNode node)
	{
		int rawKind = node.RawKind;
		if (!IsCSharpKind(rawKind))
		{
			return SyntaxKind.None;
		}
		return (SyntaxKind)rawKind;
	}

	public static SyntaxKind Kind(this SyntaxNodeOrToken nodeOrToken)
	{
		int rawKind = nodeOrToken.RawKind;
		if (!IsCSharpKind(rawKind))
		{
			return SyntaxKind.None;
		}
		return (SyntaxKind)rawKind;
	}

	public static bool IsKeyword(this SyntaxToken token)
	{
		return SyntaxFacts.IsKeywordKind(token.Kind());
	}

	public static bool IsContextualKeyword(this SyntaxToken token)
	{
		return SyntaxFacts.IsContextualKeyword(token.Kind());
	}

	public static bool IsReservedKeyword(this SyntaxToken token)
	{
		return SyntaxFacts.IsReservedKeyword(token.Kind());
	}

	public static bool IsVerbatimStringLiteral(this SyntaxToken token)
	{
		if (token.IsKind(SyntaxKind.StringLiteralToken) && token.Text.Length > 0)
		{
			return token.Text[0] == '@';
		}
		return false;
	}

	public static bool IsVerbatimIdentifier(this SyntaxToken token)
	{
		if (token.IsKind(SyntaxKind.IdentifierToken) && token.Text.Length > 0)
		{
			return token.Text[0] == '@';
		}
		return false;
	}

	public static VarianceKind VarianceKindFromToken(this SyntaxToken node)
	{
		return node.Kind() switch
		{
			SyntaxKind.OutKeyword => VarianceKind.Out, 
			SyntaxKind.InKeyword => VarianceKind.In, 
			_ => VarianceKind.None, 
		};
	}

	public static SyntaxTokenList Insert(this SyntaxTokenList list, int index, params SyntaxToken[] items)
	{
		if (index < 0 || index > list.Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		if (list.Count == 0)
		{
			return SyntaxFactory.TokenList(items);
		}
		SyntaxTokenListBuilder syntaxTokenListBuilder = new SyntaxTokenListBuilder(list.Count + items.Length);
		if (index > 0)
		{
			syntaxTokenListBuilder.Add(list, 0, index);
		}
		syntaxTokenListBuilder.Add(items);
		if (index < list.Count)
		{
			syntaxTokenListBuilder.Add(list, index, list.Count - index);
		}
		return syntaxTokenListBuilder.ToList();
	}

	public static SyntaxToken ReplaceTrivia(this SyntaxToken token, IEnumerable<SyntaxTrivia> trivia, Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia> computeReplacementTrivia)
	{
		return SyntaxReplacer.Replace(token, null, null, null, null, trivia, computeReplacementTrivia);
	}

	public static SyntaxToken ReplaceTrivia(this SyntaxToken token, SyntaxTrivia oldTrivia, SyntaxTrivia newTrivia)
	{
		return SyntaxReplacer.Replace(token, null, null, null, null, new SyntaxTrivia[1] { oldTrivia }, (SyntaxTrivia o, SyntaxTrivia r) => newTrivia);
	}

	internal static DirectiveStack ApplyDirectives(this SyntaxNode node, DirectiveStack stack)
	{
		return ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode)node.Green).ApplyDirectives(stack);
	}

	internal static DirectiveStack ApplyDirectives(this SyntaxToken token, DirectiveStack stack)
	{
		return ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode)token.Node).ApplyDirectives(stack);
	}

	internal static DirectiveStack ApplyDirectives(this SyntaxNodeOrToken nodeOrToken, DirectiveStack stack)
	{
		if (nodeOrToken.IsToken)
		{
			return nodeOrToken.AsToken().ApplyDirectives(stack);
		}
		if (nodeOrToken.IsNode)
		{
			return nodeOrToken.AsNode().ApplyDirectives(stack);
		}
		return stack;
	}

	internal static SeparatedSyntaxList<TOther> AsSeparatedList<TOther>(this SyntaxNodeOrTokenList list) where TOther : SyntaxNode
	{
		SeparatedSyntaxListBuilder<TOther> separatedSyntaxListBuilder = SeparatedSyntaxListBuilder<TOther>.Create();
		foreach (SyntaxNodeOrToken item in list)
		{
			SyntaxNode syntaxNode = item.AsNode();
			if (syntaxNode != null)
			{
				separatedSyntaxListBuilder.Add((TOther)syntaxNode);
			}
			else
			{
				separatedSyntaxListBuilder.AddSeparator(item.AsToken());
			}
		}
		return separatedSyntaxListBuilder.ToList();
	}

	internal static IList<Microsoft.CodeAnalysis.CSharp.Syntax.DirectiveTriviaSyntax> GetDirectives(this SyntaxNode node, Func<Microsoft.CodeAnalysis.CSharp.Syntax.DirectiveTriviaSyntax, bool> filter = null)
	{
		return ((CSharpSyntaxNode)node).GetDirectives(filter);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DirectiveTriviaSyntax GetFirstDirective(this SyntaxNode node, Func<Microsoft.CodeAnalysis.CSharp.Syntax.DirectiveTriviaSyntax, bool> predicate = null)
	{
		return ((CSharpSyntaxNode)node).GetFirstDirective(predicate);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.DirectiveTriviaSyntax GetLastDirective(this SyntaxNode node, Func<Microsoft.CodeAnalysis.CSharp.Syntax.DirectiveTriviaSyntax, bool> predicate = null)
	{
		return ((CSharpSyntaxNode)node).GetLastDirective(predicate);
	}

	public static Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax GetCompilationUnitRoot(this SyntaxTree tree, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)tree.GetRoot(cancellationToken);
	}

	internal static bool HasReferenceDirectives(this SyntaxTree tree)
	{
		if (tree is CSharpSyntaxTree cSharpSyntaxTree)
		{
			return cSharpSyntaxTree.HasReferenceDirectives;
		}
		return false;
	}

	internal static bool HasReferenceOrLoadDirectives(this SyntaxTree tree)
	{
		if (tree is CSharpSyntaxTree cSharpSyntaxTree)
		{
			return cSharpSyntaxTree.HasReferenceOrLoadDirectives;
		}
		return false;
	}

	internal static bool IsAnyPreprocessorSymbolDefined(this SyntaxTree tree, ImmutableArray<string> conditionalSymbols)
	{
		if (tree is CSharpSyntaxTree cSharpSyntaxTree)
		{
			return cSharpSyntaxTree.IsAnyPreprocessorSymbolDefined(conditionalSymbols);
		}
		return false;
	}

	internal static bool IsPreprocessorSymbolDefined(this SyntaxTree tree, string symbolName, int position)
	{
		if (tree is CSharpSyntaxTree cSharpSyntaxTree)
		{
			return cSharpSyntaxTree.IsPreprocessorSymbolDefined(symbolName, position);
		}
		return false;
	}

	internal static ReportDiagnostic GetPragmaDirectiveWarningState(this SyntaxTree tree, string id, int position)
	{
		return ((CSharpSyntaxTree)tree).GetPragmaDirectiveWarningState(id, position);
	}

	public static Conversion ClassifyConversion(this Compilation compilation, ITypeSymbol source, ITypeSymbol destination)
	{
		if (compilation is CSharpCompilation cSharpCompilation)
		{
			return cSharpCompilation.ClassifyConversion(source, destination);
		}
		return Conversion.NoConversion;
	}

	public static SymbolInfo GetSymbolInfo(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.OrderingSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetSymbolInfo(node, cancellationToken);
		}
		return SymbolInfo.None;
	}

	public static SymbolInfo GetSymbolInfo(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.SelectOrGroupClauseSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetSymbolInfo(node, cancellationToken);
		}
		return SymbolInfo.None;
	}

	public static SymbolInfo GetSymbolInfo(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetSymbolInfo(expression, cancellationToken);
		}
		return SymbolInfo.None;
	}

	public static SymbolInfo GetCollectionInitializerSymbolInfo(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetCollectionInitializerSymbolInfo(expression, cancellationToken);
		}
		return SymbolInfo.None;
	}

	public static SymbolInfo GetSymbolInfo(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorInitializerSyntax constructorInitializer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetSymbolInfo(constructorInitializer, cancellationToken);
		}
		return SymbolInfo.None;
	}

	public static SymbolInfo GetSymbolInfo(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax attributeSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetSymbolInfo(attributeSyntax, cancellationToken);
		}
		return SymbolInfo.None;
	}

	public static SymbolInfo GetSymbolInfo(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.CrefSyntax crefSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetSymbolInfo(crefSyntax, cancellationToken);
		}
		return SymbolInfo.None;
	}

	public static SymbolInfo GetSpeculativeSymbolInfo(this SemanticModel semanticModel, int position, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, SpeculativeBindingOption bindingOption)
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetSpeculativeSymbolInfo(position, expression, bindingOption);
		}
		return SymbolInfo.None;
	}

	public static SymbolInfo GetSpeculativeSymbolInfo(this SemanticModel semanticModel, int position, Microsoft.CodeAnalysis.CSharp.Syntax.CrefSyntax expression, SpeculativeBindingOption bindingOption)
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetSpeculativeSymbolInfo(position, expression, bindingOption);
		}
		return SymbolInfo.None;
	}

	public static SymbolInfo GetSpeculativeSymbolInfo(this SemanticModel semanticModel, int position, Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax attribute)
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetSpeculativeSymbolInfo(position, attribute);
		}
		return SymbolInfo.None;
	}

	public static SymbolInfo GetSpeculativeSymbolInfo(this SemanticModel semanticModel, int position, Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorInitializerSyntax constructorInitializer)
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetSpeculativeSymbolInfo(position, constructorInitializer);
		}
		return SymbolInfo.None;
	}

	public static TypeInfo GetTypeInfo(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorInitializerSyntax constructorInitializer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetTypeInfo(constructorInitializer, cancellationToken);
		}
		return CSharpTypeInfo.None;
	}

	public static TypeInfo GetTypeInfo(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.SelectOrGroupClauseSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetTypeInfo(node, cancellationToken);
		}
		return CSharpTypeInfo.None;
	}

	public static TypeInfo GetTypeInfo(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetTypeInfo(expression, cancellationToken);
		}
		return CSharpTypeInfo.None;
	}

	public static TypeInfo GetTypeInfo(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax attributeSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetTypeInfo(attributeSyntax, cancellationToken);
		}
		return CSharpTypeInfo.None;
	}

	public static TypeInfo GetSpeculativeTypeInfo(this SemanticModel semanticModel, int position, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, SpeculativeBindingOption bindingOption)
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetSpeculativeTypeInfo(position, expression, bindingOption);
		}
		return CSharpTypeInfo.None;
	}

	public static Conversion GetConversion(this SemanticModel semanticModel, SyntaxNode expression, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetConversion(expression, cancellationToken);
		}
		return Conversion.NoConversion;
	}

	public static Conversion GetConversion(this IConversionOperation conversionExpression)
	{
		if (conversionExpression.Language == "C#")
		{
			return (Conversion)(object)((BaseConversionExpression)conversionExpression).ConvertibleConversion;
		}
		throw new ArgumentException(string.Format(CSharpResources.IConversionExpressionIsNotCSharpConversion, "IConversionOperation"), "conversionExpression");
	}

	public static Conversion GetInConversion(this ICompoundAssignmentOperation compoundAssignment)
	{
		if (compoundAssignment == null)
		{
			throw new ArgumentNullException("compoundAssignment");
		}
		if (compoundAssignment.Language == "C#")
		{
			return (Conversion)(object)((BaseCompoundAssignmentExpression)compoundAssignment).InConversionConvertible;
		}
		throw new ArgumentException(string.Format(CSharpResources.ICompoundAssignmentOperationIsNotCSharpCompoundAssignment, "compoundAssignment"), "compoundAssignment");
	}

	public static Conversion GetOutConversion(this ICompoundAssignmentOperation compoundAssignment)
	{
		if (compoundAssignment == null)
		{
			throw new ArgumentNullException("compoundAssignment");
		}
		if (compoundAssignment.Language == "C#")
		{
			return (Conversion)(object)((BaseCompoundAssignmentExpression)compoundAssignment).OutConversionConvertible;
		}
		throw new ArgumentException(string.Format(CSharpResources.ICompoundAssignmentOperationIsNotCSharpCompoundAssignment, "compoundAssignment"), "compoundAssignment");
	}

	public static Conversion GetSpeculativeConversion(this SemanticModel semanticModel, int position, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, SpeculativeBindingOption bindingOption)
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetSpeculativeConversion(position, expression, bindingOption);
		}
		return Conversion.NoConversion;
	}

	public static ForEachStatementInfo GetForEachStatementInfo(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.ForEachStatementSyntax forEachStatement)
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetForEachStatementInfo(forEachStatement);
		}
		return default;
	}

	public static ForEachStatementInfo GetForEachStatementInfo(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.CommonForEachStatementSyntax forEachStatement)
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetForEachStatementInfo(forEachStatement);
		}
		return default;
	}

	public static DeconstructionInfo GetDeconstructionInfo(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax assignment)
	{
		if (!(semanticModel is CSharpSemanticModel cSharpSemanticModel))
		{
			return default;
		}
		return cSharpSemanticModel.GetDeconstructionInfo(assignment);
	}

	public static DeconstructionInfo GetDeconstructionInfo(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.ForEachVariableStatementSyntax @foreach)
	{
		if (!(semanticModel is CSharpSemanticModel cSharpSemanticModel))
		{
			return default;
		}
		return cSharpSemanticModel.GetDeconstructionInfo(@foreach);
	}

	public static AwaitExpressionInfo GetAwaitExpressionInfo(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.AwaitExpressionSyntax awaitExpression)
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetAwaitExpressionInfo(awaitExpression);
		}
		return default;
	}

	public static ImmutableArray<ISymbol> GetMemberGroup(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetMemberGroup(expression, cancellationToken);
		}
		return ImmutableArray.Create<ISymbol>();
	}

	public static ImmutableArray<ISymbol> GetMemberGroup(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax attribute, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetMemberGroup(attribute, cancellationToken);
		}
		return ImmutableArray.Create<ISymbol>();
	}

	public static ImmutableArray<ISymbol> GetMemberGroup(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorInitializerSyntax initializer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetMemberGroup(initializer, cancellationToken);
		}
		return ImmutableArray.Create<ISymbol>();
	}

	public static ImmutableArray<IPropertySymbol> GetIndexerGroup(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetIndexerGroup(expression, cancellationToken);
		}
		return ImmutableArray.Create<IPropertySymbol>();
	}

	public static Optional<object> GetConstantValue(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetConstantValue(expression, cancellationToken);
		}
		return default;
	}

	public static QueryClauseInfo GetQueryClauseInfo(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.QueryClauseSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.GetQueryClauseInfo(node, cancellationToken);
		}
		return default;
	}

	public static IAliasSymbol GetAliasInfo(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax nameSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetAliasInfo(nameSyntax, cancellationToken);
	}

	public static IAliasSymbol GetSpeculativeAliasInfo(this SemanticModel semanticModel, int position, Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax nameSyntax, SpeculativeBindingOption bindingOption)
	{
		return (semanticModel as CSharpSemanticModel)?.GetSpeculativeAliasInfo(position, nameSyntax, bindingOption);
	}

	public static ControlFlowAnalysis AnalyzeControlFlow(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax firstStatement, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax lastStatement)
	{
		return (semanticModel as CSharpSemanticModel)?.AnalyzeControlFlow(firstStatement, lastStatement);
	}

	public static ControlFlowAnalysis AnalyzeControlFlow(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement)
	{
		return (semanticModel as CSharpSemanticModel)?.AnalyzeControlFlow(statement);
	}

	public static DataFlowAnalysis AnalyzeDataFlow(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return (semanticModel as CSharpSemanticModel)?.AnalyzeDataFlow(expression);
	}

	public static DataFlowAnalysis AnalyzeDataFlow(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax firstStatement, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax lastStatement)
	{
		return (semanticModel as CSharpSemanticModel)?.AnalyzeDataFlow(firstStatement, lastStatement);
	}

	public static DataFlowAnalysis AnalyzeDataFlow(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement)
	{
		return (semanticModel as CSharpSemanticModel)?.AnalyzeDataFlow(statement);
	}

	public static bool TryGetSpeculativeSemanticModelForMethodBody(this SemanticModel semanticModel, int position, Microsoft.CodeAnalysis.CSharp.Syntax.BaseMethodDeclarationSyntax method, out SemanticModel speculativeModel)
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.TryGetSpeculativeSemanticModelForMethodBody(position, method, out speculativeModel);
		}
		speculativeModel = null;
		return false;
	}

	public static bool TryGetSpeculativeSemanticModelForMethodBody(this SemanticModel semanticModel, int position, Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax accessor, out SemanticModel speculativeModel)
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.TryGetSpeculativeSemanticModelForMethodBody(position, accessor, out speculativeModel);
		}
		speculativeModel = null;
		return false;
	}

	public static bool TryGetSpeculativeSemanticModel(this SemanticModel semanticModel, int position, Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax type, out SemanticModel speculativeModel, SpeculativeBindingOption bindingOption = SpeculativeBindingOption.BindAsExpression)
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.TryGetSpeculativeSemanticModel(position, type, out speculativeModel, bindingOption);
		}
		speculativeModel = null;
		return false;
	}

	public static bool TryGetSpeculativeSemanticModel(this SemanticModel semanticModel, int position, Microsoft.CodeAnalysis.CSharp.Syntax.CrefSyntax crefSyntax, out SemanticModel speculativeModel)
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.TryGetSpeculativeSemanticModel(position, crefSyntax, out speculativeModel);
		}
		speculativeModel = null;
		return false;
	}

	public static bool TryGetSpeculativeSemanticModel(this SemanticModel semanticModel, int position, Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement, out SemanticModel speculativeModel)
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.TryGetSpeculativeSemanticModel(position, statement, out speculativeModel);
		}
		speculativeModel = null;
		return false;
	}

	public static bool TryGetSpeculativeSemanticModel(this SemanticModel semanticModel, int position, Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax initializer, out SemanticModel speculativeModel)
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.TryGetSpeculativeSemanticModel(position, initializer, out speculativeModel);
		}
		speculativeModel = null;
		return false;
	}

	public static bool TryGetSpeculativeSemanticModel(this SemanticModel semanticModel, int position, Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax expressionBody, out SemanticModel speculativeModel)
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.TryGetSpeculativeSemanticModel(position, expressionBody, out speculativeModel);
		}
		speculativeModel = null;
		return false;
	}

	public static bool TryGetSpeculativeSemanticModel(this SemanticModel semanticModel, int position, Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorInitializerSyntax constructorInitializer, out SemanticModel speculativeModel)
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.TryGetSpeculativeSemanticModel(position, constructorInitializer, out speculativeModel);
		}
		speculativeModel = null;
		return false;
	}

	public static bool TryGetSpeculativeSemanticModel(this SemanticModel semanticModel, int position, Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax attribute, out SemanticModel speculativeModel)
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.TryGetSpeculativeSemanticModel(position, attribute, out speculativeModel);
		}
		speculativeModel = null;
		return false;
	}

	public static Conversion ClassifyConversion(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, ITypeSymbol destination, bool isExplicitInSource = false)
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.ClassifyConversion(expression, (TypeSymbol)destination, isExplicitInSource);
		}
		return Conversion.NoConversion;
	}

	public static Conversion ClassifyConversion(this SemanticModel semanticModel, int position, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, ITypeSymbol destination, bool isExplicitInSource = false)
	{
		if (semanticModel is CSharpSemanticModel cSharpSemanticModel)
		{
			return cSharpSemanticModel.ClassifyConversion(position, expression, (TypeSymbol)destination, isExplicitInSource);
		}
		return Conversion.NoConversion;
	}

	public static ISymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public static INamespaceSymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public static INamedTypeSymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.BaseTypeDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public static INamedTypeSymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.DelegateDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public static IFieldSymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.EnumMemberDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public static IMethodSymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.BaseMethodDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public static ISymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.BasePropertyDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public static IPropertySymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public static IPropertySymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.IndexerDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public static IEventSymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.EventDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public static IPropertySymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousObjectMemberDeclaratorSyntax declaratorSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(declaratorSyntax, cancellationToken);
	}

	public static INamedTypeSymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousObjectCreationExpressionSyntax declaratorSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(declaratorSyntax, cancellationToken);
	}

	public static INamedTypeSymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.TupleExpressionSyntax declaratorSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(declaratorSyntax, cancellationToken);
	}

	public static ISymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax declaratorSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(declaratorSyntax, cancellationToken);
	}

	public static IMethodSymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public static ISymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.SingleVariableDesignationSyntax designationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(designationSyntax, cancellationToken);
	}

	public static ISymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public static ISymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.TupleElementSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public static ILabelSymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.LabeledStatementSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public static ILabelSymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.SwitchLabelSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public static IAliasSymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public static IAliasSymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.ExternAliasDirectiveSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public static IParameterSymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public static ITypeParameterSymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterSyntax typeParameter, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(typeParameter, cancellationToken);
	}

	public static ILocalSymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.ForEachStatementSyntax forEachStatement, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(forEachStatement, cancellationToken);
	}

	public static ILocalSymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.CatchDeclarationSyntax catchDeclaration, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(catchDeclaration, cancellationToken);
	}

	public static IRangeVariableSymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.QueryClauseSyntax queryClause, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(queryClause, cancellationToken);
	}

	public static IRangeVariableSymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.JoinIntoClauseSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(node, cancellationToken);
	}

	public static IRangeVariableSymbol GetDeclaredSymbol(this SemanticModel semanticModel, Microsoft.CodeAnalysis.CSharp.Syntax.QueryContinuationSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (semanticModel as CSharpSemanticModel)?.GetDeclaredSymbol(node, cancellationToken);
	}
}
