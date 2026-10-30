using System;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

internal class SyntaxNormalizer : CSharpSyntaxRewriter
{
	private readonly TextSpan _consideredSpan;

	private readonly int _initialDepth;

	private readonly string _indentWhitespace;

	private readonly bool _useElasticTrivia;

	private readonly SyntaxTrivia _eolTrivia;

	private bool _isInStructuredTrivia;

	private SyntaxToken _previousToken;

	private bool _afterLineBreak;

	private bool _afterIndentation;

	private ArrayBuilder<SyntaxTrivia> _indentations;

	private static SyntaxTrivia s_trimmedDocCommentExterior = SyntaxFactory.DocumentationCommentExterior("///");

	private SyntaxNormalizer(TextSpan consideredSpan, int initialDepth, string indentWhitespace, string eolWhitespace, bool useElasticTrivia)
		: base(visitIntoStructuredTrivia: true)
	{
		_consideredSpan = consideredSpan;
		_initialDepth = initialDepth;
		_indentWhitespace = indentWhitespace;
		_useElasticTrivia = useElasticTrivia;
		_eolTrivia = (useElasticTrivia ? SyntaxFactory.ElasticEndOfLine(eolWhitespace) : SyntaxFactory.EndOfLine(eolWhitespace));
		_afterLineBreak = true;
	}

	internal static TNode Normalize<TNode>(TNode node, string indentWhitespace, string eolWhitespace, bool useElasticTrivia = false) where TNode : SyntaxNode
	{
		SyntaxNormalizer syntaxNormalizer = new SyntaxNormalizer(node.FullSpan, GetDeclarationDepth(node), indentWhitespace, eolWhitespace, useElasticTrivia);
		TNode result = (TNode)syntaxNormalizer.Visit(node);
		syntaxNormalizer.Free();
		return result;
	}

	internal static SyntaxToken Normalize(SyntaxToken token, string indentWhitespace, string eolWhitespace, bool useElasticTrivia = false)
	{
		SyntaxNormalizer syntaxNormalizer = new SyntaxNormalizer(token.FullSpan, GetDeclarationDepth(token), indentWhitespace, eolWhitespace, useElasticTrivia);
		SyntaxToken result = syntaxNormalizer.VisitToken(token);
		syntaxNormalizer.Free();
		return result;
	}

	internal static SyntaxTriviaList Normalize(SyntaxTriviaList trivia, string indentWhitespace, string eolWhitespace, bool useElasticTrivia = false)
	{
		SyntaxNormalizer syntaxNormalizer = new SyntaxNormalizer(trivia.FullSpan, GetDeclarationDepth(trivia.Token), indentWhitespace, eolWhitespace, useElasticTrivia);
		SyntaxTriviaList result = syntaxNormalizer.RewriteTrivia(trivia, GetDeclarationDepth(trivia.ElementAt(0).Token), isTrailing: false, indentAfterLineBreak: false, mustHaveSeparator: false, 0);
		syntaxNormalizer.Free();
		return result;
	}

	private void Free()
	{
		if (_indentations != null)
		{
			_indentations.Free();
		}
	}

	public override SyntaxToken VisitToken(SyntaxToken token)
	{
		if (token.Kind() == SyntaxKind.None || (token.IsMissing && token.FullWidth == 0))
		{
			return token;
		}
		try
		{
			SyntaxToken syntaxToken = token;
			int declarationDepth = GetDeclarationDepth(token);
			syntaxToken = syntaxToken.WithLeadingTrivia(RewriteTrivia(token.LeadingTrivia, declarationDepth, isTrailing: false, NeedsIndentAfterLineBreak(token), mustHaveSeparator: false, 0));
			SyntaxToken nextRelevantToken = GetNextRelevantToken(token);
			_afterLineBreak = IsLineBreak(token);
			_afterIndentation = false;
			int lineBreaksAfter = LineBreaksAfter(token, nextRelevantToken);
			bool mustHaveSeparator = NeedsSeparator(token, nextRelevantToken);
			return syntaxToken.WithTrailingTrivia(RewriteTrivia(token.TrailingTrivia, declarationDepth, isTrailing: true, indentAfterLineBreak: false, mustHaveSeparator, lineBreaksAfter));
		}
		finally
		{
			_previousToken = token;
		}
	}

	private SyntaxToken GetNextRelevantToken(SyntaxToken token)
	{
		SyntaxToken nextToken = token.GetNextToken((SyntaxToken t) => SyntaxToken.NonZeroWidth(t) || t.Kind() == SyntaxKind.EndOfDirectiveToken, (SyntaxTrivia t) => t.Kind() == SyntaxKind.SkippedTokensTrivia);
		if (_consideredSpan.Contains(nextToken.FullSpan))
		{
			return nextToken;
		}
		return default;
	}

	private SyntaxTrivia GetIndentation(int count)
	{
		count = Math.Max(count - _initialDepth, 0);
		int capacity = count + 1;
		if (_indentations == null)
		{
			_indentations = ArrayBuilder<SyntaxTrivia>.GetInstance(capacity);
		}
		else
		{
			_indentations.EnsureCapacity(capacity);
		}
		for (int i = _indentations.Count; i <= count; i++)
		{
			string text = ((i == 0) ? "" : (_indentations[i - 1].ToString() + _indentWhitespace));
			_indentations.Add(_useElasticTrivia ? SyntaxFactory.ElasticWhitespace(text) : SyntaxFactory.Whitespace(text));
		}
		return _indentations[count];
	}

	private static bool NeedsIndentAfterLineBreak(SyntaxToken token)
	{
		return !token.IsKind(SyntaxKind.EndOfFileToken);
	}

	private int LineBreaksAfter(SyntaxToken currentToken, SyntaxToken nextToken)
	{
		if (currentToken.IsKind(SyntaxKind.EndOfDirectiveToken))
		{
			return 1;
		}
		if (nextToken.Kind() == SyntaxKind.None)
		{
			return 0;
		}
		if (_isInStructuredTrivia)
		{
			return 0;
		}
		switch (currentToken.Kind())
		{
		case SyntaxKind.None:
			return 0;
		case SyntaxKind.OpenBraceToken:
			return LineBreaksAfterOpenBrace(currentToken, nextToken);
		case SyntaxKind.FinallyKeyword:
			return 1;
		case SyntaxKind.CloseBraceToken:
			return LineBreaksAfterCloseBrace(currentToken, nextToken);
		case SyntaxKind.CloseParenToken:
			if ((!(currentToken.Parent is StatementSyntax) || nextToken.Parent == currentToken.Parent) && nextToken.Kind() != SyntaxKind.OpenBraceToken && nextToken.Kind() != SyntaxKind.WhereKeyword)
			{
				return 0;
			}
			return 1;
		case SyntaxKind.CloseBracketToken:
			if (currentToken.Parent is AttributeListSyntax && !(currentToken.Parent.Parent is ParameterSyntax))
			{
				return 1;
			}
			break;
		case SyntaxKind.SemicolonToken:
			return LineBreaksAfterSemicolon(currentToken, nextToken);
		case SyntaxKind.CommaToken:
			if (!(currentToken.Parent is EnumDeclarationSyntax))
			{
				return 0;
			}
			return 1;
		case SyntaxKind.ElseKeyword:
			if (nextToken.Kind() == SyntaxKind.IfKeyword)
			{
				return 0;
			}
			return 1;
		case SyntaxKind.ColonToken:
			if (currentToken.Parent is LabeledStatementSyntax || currentToken.Parent is SwitchLabelSyntax)
			{
				return 1;
			}
			break;
		}
		if ((nextToken.IsKind(SyntaxKind.FromKeyword) && nextToken.Parent.IsKind(SyntaxKind.FromClause)) || (nextToken.IsKind(SyntaxKind.LetKeyword) && nextToken.Parent.IsKind(SyntaxKind.LetClause)) || (nextToken.IsKind(SyntaxKind.WhereKeyword) && nextToken.Parent.IsKind(SyntaxKind.WhereClause)) || (nextToken.IsKind(SyntaxKind.JoinKeyword) && nextToken.Parent.IsKind(SyntaxKind.JoinClause)) || (nextToken.IsKind(SyntaxKind.JoinKeyword) && nextToken.Parent.Kind() == SyntaxKind.JoinIntoClause) || (nextToken.Kind() == SyntaxKind.OrderByKeyword && nextToken.Parent.Kind() == SyntaxKind.OrderByClause) || (nextToken.Kind() == SyntaxKind.SelectKeyword && nextToken.Parent.Kind() == SyntaxKind.SelectClause) || (nextToken.Kind() == SyntaxKind.GroupKeyword && nextToken.Parent.Kind() == SyntaxKind.GroupClause))
		{
			return 1;
		}
		switch (nextToken.Kind())
		{
		case SyntaxKind.OpenBraceToken:
		case SyntaxKind.CloseBraceToken:
			if (!nextToken.Parent.IsKind(SyntaxKind.Interpolation) && !(nextToken.Parent is InitializerExpressionSyntax))
			{
				return 1;
			}
			return 0;
		case SyntaxKind.ElseKeyword:
		case SyntaxKind.FinallyKeyword:
			return 1;
		case SyntaxKind.OpenBracketToken:
			if (!(nextToken.Parent is AttributeListSyntax) || nextToken.Parent.Parent is ParameterSyntax)
			{
				return 0;
			}
			return 1;
		case SyntaxKind.WhereKeyword:
			if (!(currentToken.Parent is TypeParameterListSyntax))
			{
				return 0;
			}
			return 1;
		default:
			return 0;
		}
	}

	private static int LineBreaksAfterOpenBrace(SyntaxToken currentToken, SyntaxToken nextToken)
	{
		if (currentToken.Parent is InitializerExpressionSyntax || currentToken.Parent.IsKind(SyntaxKind.Interpolation))
		{
			return 0;
		}
		return 1;
	}

	private static int LineBreaksAfterCloseBrace(SyntaxToken currentToken, SyntaxToken nextToken)
	{
		if (currentToken.Parent is InitializerExpressionSyntax || currentToken.Parent.IsKind(SyntaxKind.Interpolation))
		{
			return 0;
		}
		SyntaxKind syntaxKind = nextToken.Kind();
		switch (syntaxKind)
		{
		case SyntaxKind.CloseBraceToken:
		case SyntaxKind.ElseKeyword:
		case SyntaxKind.CatchKeyword:
		case SyntaxKind.FinallyKeyword:
		case SyntaxKind.EndOfFileToken:
			return 1;
		default:
			if (syntaxKind == SyntaxKind.WhileKeyword && nextToken.Parent.IsKind(SyntaxKind.DoStatement))
			{
				return 1;
			}
			return 2;
		}
	}

	private static int LineBreaksAfterSemicolon(SyntaxToken currentToken, SyntaxToken nextToken)
	{
		if (currentToken.Parent.Kind() == SyntaxKind.ForStatement)
		{
			return 0;
		}
		if (nextToken.Kind() == SyntaxKind.CloseBraceToken)
		{
			return 1;
		}
		if (currentToken.Parent.Kind() == SyntaxKind.UsingDirective)
		{
			if (nextToken.Parent.Kind() != SyntaxKind.UsingDirective)
			{
				return 2;
			}
			return 1;
		}
		if (currentToken.Parent.Kind() == SyntaxKind.ExternAliasDirective)
		{
			if (nextToken.Parent.Kind() != SyntaxKind.ExternAliasDirective)
			{
				return 2;
			}
			return 1;
		}
		return 1;
	}

	private static bool NeedsSeparator(SyntaxToken token, SyntaxToken next)
	{
		if (token.Parent == null || next.Parent == null)
		{
			return false;
		}
		if (IsXmlTextToken(token.Kind()) || IsXmlTextToken(next.Kind()))
		{
			return false;
		}
		if (next.Kind() == SyntaxKind.EndOfDirectiveToken)
		{
			if (IsKeyword(token.Kind()))
			{
				return next.LeadingWidth > 0;
			}
			return false;
		}
		if ((token.Parent is AssignmentExpressionSyntax && AssignmentTokenNeedsSeparator(token.Kind())) || (next.Parent is AssignmentExpressionSyntax && AssignmentTokenNeedsSeparator(next.Kind())) || (token.Parent is BinaryExpressionSyntax && BinaryTokenNeedsSeparator(token.Kind())) || (next.Parent is BinaryExpressionSyntax && BinaryTokenNeedsSeparator(next.Kind())))
		{
			return true;
		}
		if (token.IsKind(SyntaxKind.GreaterThanToken) && token.Parent.IsKind(SyntaxKind.TypeArgumentList) && !SyntaxFacts.IsPunctuation(next.Kind()))
		{
			return true;
		}
		if (token.IsKind(SyntaxKind.CommaToken) && !next.IsKind(SyntaxKind.CommaToken) && !token.Parent.IsKind(SyntaxKind.EnumDeclaration))
		{
			return true;
		}
		if (token.Kind() == SyntaxKind.SemicolonToken && next.Kind() != SyntaxKind.SemicolonToken && next.Kind() != SyntaxKind.CloseParenToken)
		{
			return true;
		}
		if (token.IsKind(SyntaxKind.QuestionToken) && (token.Parent.IsKind(SyntaxKind.ConditionalExpression) || token.Parent is TypeSyntax))
		{
			return true;
		}
		if (token.IsKind(SyntaxKind.ColonToken))
		{
			return !token.Parent.IsKind(SyntaxKind.InterpolationFormatClause);
		}
		if (next.IsKind(SyntaxKind.ColonToken) && (next.Parent.IsKind(SyntaxKind.BaseList) || next.Parent.IsKind(SyntaxKind.TypeParameterConstraintClause)))
		{
			return true;
		}
		if (token.IsKind(SyntaxKind.CloseBracketToken) && IsWord(next.Kind()))
		{
			return true;
		}
		if ((next.IsKind(SyntaxKind.QuestionToken) || next.IsKind(SyntaxKind.ColonToken)) && next.Parent.IsKind(SyntaxKind.ConditionalExpression))
		{
			return true;
		}
		if (token.IsKind(SyntaxKind.EqualsToken) || next.IsKind(SyntaxKind.EqualsToken))
		{
			return true;
		}
		if (token.IsKind(SyntaxKind.EqualsGreaterThanToken) || next.IsKind(SyntaxKind.EqualsGreaterThanToken))
		{
			return true;
		}
		if (SyntaxFacts.IsLiteral(token.Kind()) && SyntaxFacts.IsLiteral(next.Kind()))
		{
			return true;
		}
		if (IsKeyword(token.Kind()) && !next.IsKind(SyntaxKind.ColonToken) && !next.IsKind(SyntaxKind.DotToken) && !next.IsKind(SyntaxKind.QuestionToken) && !next.IsKind(SyntaxKind.SemicolonToken) && !next.IsKind(SyntaxKind.OpenBracketToken) && (!next.IsKind(SyntaxKind.OpenParenToken) || KeywordNeedsSeparatorBeforeOpenParen(token.Kind())) && !next.IsKind(SyntaxKind.CloseParenToken) && !next.IsKind(SyntaxKind.CloseBraceToken) && !next.IsKind(SyntaxKind.ColonColonToken) && !next.IsKind(SyntaxKind.GreaterThanToken) && !next.IsKind(SyntaxKind.CommaToken))
		{
			return true;
		}
		if (IsWord(token.Kind()) && IsWord(next.Kind()))
		{
			return true;
		}
		if (token.Width > 1 && next.Width > 1)
		{
			char c = token.Text.Last();
			char c2 = next.Text.First();
			if (c == c2 && TokenCharacterCanBeDoubled(c))
			{
				return true;
			}
		}
		return false;
	}

	private static bool KeywordNeedsSeparatorBeforeOpenParen(SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.TypeOfKeyword:
		case SyntaxKind.SizeOfKeyword:
		case SyntaxKind.DefaultKeyword:
		case SyntaxKind.NewKeyword:
		case SyntaxKind.ArgListKeyword:
		case SyntaxKind.ThisKeyword:
		case SyntaxKind.BaseKeyword:
		case SyntaxKind.CheckedKeyword:
		case SyntaxKind.UncheckedKeyword:
			return false;
		default:
			return true;
		}
	}

	private static bool IsXmlTextToken(SyntaxKind kind)
	{
		if (kind - 8513 <= SyntaxKind.List)
		{
			return true;
		}
		return false;
	}

	private static bool BinaryTokenNeedsSeparator(SyntaxKind kind)
	{
		if (kind == SyntaxKind.DotToken || kind == SyntaxKind.MinusGreaterThanToken)
		{
			return false;
		}
		return SyntaxFacts.GetBinaryExpression(kind) != SyntaxKind.None;
	}

	private static bool AssignmentTokenNeedsSeparator(SyntaxKind kind)
	{
		return SyntaxFacts.GetAssignmentExpression(kind) != SyntaxKind.None;
	}

	private SyntaxTriviaList RewriteTrivia(SyntaxTriviaList triviaList, int depth, bool isTrailing, bool indentAfterLineBreak, bool mustHaveSeparator, int lineBreaksAfter)
	{
		ArrayBuilder<SyntaxTrivia> instance = ArrayBuilder<SyntaxTrivia>.GetInstance(triviaList.Count);
		try
		{
			foreach (SyntaxTrivia item2 in triviaList)
			{
				if (item2.IsKind(SyntaxKind.WhitespaceTrivia) || item2.IsKind(SyntaxKind.EndOfLineTrivia) || item2.FullWidth == 0)
				{
					continue;
				}
				bool flag = (instance.Count > 0 && NeedsSeparatorBetween(instance.Last())) || ((instance.Count == 0) & isTrailing);
				if ((NeedsLineBreakBefore(item2, isTrailing) || (instance.Count > 0 && NeedsLineBreakBetween(instance.Last(), item2, isTrailing))) && !_afterLineBreak)
				{
					instance.Add(GetEndOfLine());
					_afterLineBreak = true;
					_afterIndentation = false;
				}
				if (_afterLineBreak)
				{
					if (!_afterIndentation && NeedsIndentAfterLineBreak(item2))
					{
						instance.Add(GetIndentation(GetDeclarationDepth(item2)));
						_afterIndentation = true;
					}
				}
				else if (flag)
				{
					instance.Add(GetSpace());
					_afterLineBreak = false;
					_afterIndentation = false;
				}
				if (item2.HasStructure)
				{
					SyntaxTrivia item = VisitStructuredTrivia(item2);
					instance.Add(item);
				}
				else if (item2.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia))
				{
					instance.Add(s_trimmedDocCommentExterior);
				}
				else
				{
					instance.Add(item2);
				}
				if (NeedsLineBreakAfter(item2, isTrailing) && (instance.Count == 0 || !EndsInLineBreak(instance.Last())))
				{
					instance.Add(GetEndOfLine());
					_afterLineBreak = true;
					_afterIndentation = false;
				}
			}
			if (lineBreaksAfter > 0)
			{
				if (instance.Count > 0 && EndsInLineBreak(instance.Last()))
				{
					lineBreaksAfter--;
				}
				for (int i = 0; i < lineBreaksAfter; i++)
				{
					instance.Add(GetEndOfLine());
					_afterLineBreak = true;
					_afterIndentation = false;
				}
			}
			else if (indentAfterLineBreak && _afterLineBreak && !_afterIndentation)
			{
				instance.Add(GetIndentation(depth));
				_afterIndentation = true;
			}
			else if (mustHaveSeparator)
			{
				instance.Add(GetSpace());
				_afterLineBreak = false;
				_afterIndentation = false;
			}
			if (instance.Count == 0)
			{
				return default;
			}
			if (instance.Count == 1)
			{
				return SyntaxFactory.TriviaList(instance.First());
			}
			return SyntaxFactory.TriviaList(instance);
		}
		finally
		{
			instance.Free();
		}
	}

	private SyntaxTrivia GetSpace()
	{
		if (!_useElasticTrivia)
		{
			return SyntaxFactory.Space;
		}
		return SyntaxFactory.ElasticSpace;
	}

	private SyntaxTrivia GetEndOfLine()
	{
		return _eolTrivia;
	}

	private SyntaxTrivia VisitStructuredTrivia(SyntaxTrivia trivia)
	{
		bool isInStructuredTrivia = _isInStructuredTrivia;
		_isInStructuredTrivia = true;
		SyntaxToken previousToken = _previousToken;
		_previousToken = default;
		SyntaxTrivia result = VisitTrivia(trivia);
		_isInStructuredTrivia = isInStructuredTrivia;
		_previousToken = previousToken;
		return result;
	}

	private static bool NeedsSeparatorBetween(SyntaxTrivia trivia)
	{
		SyntaxKind syntaxKind = trivia.Kind();
		if (syntaxKind == SyntaxKind.None || syntaxKind == SyntaxKind.WhitespaceTrivia || syntaxKind == SyntaxKind.DocumentationCommentExteriorTrivia)
		{
			return false;
		}
		return !SyntaxFacts.IsPreprocessorDirective(trivia.Kind());
	}

	private static bool NeedsLineBreakBetween(SyntaxTrivia trivia, SyntaxTrivia next, bool isTrailingTrivia)
	{
		if (!NeedsLineBreakAfter(trivia, isTrailingTrivia))
		{
			return NeedsLineBreakBefore(next, isTrailingTrivia);
		}
		return true;
	}

	private static bool NeedsLineBreakBefore(SyntaxTrivia trivia, bool isTrailingTrivia)
	{
		SyntaxKind syntaxKind = trivia.Kind();
		if (syntaxKind == SyntaxKind.DocumentationCommentExteriorTrivia)
		{
			return !isTrailingTrivia;
		}
		return SyntaxFacts.IsPreprocessorDirective(syntaxKind);
	}

	private static bool NeedsLineBreakAfter(SyntaxTrivia trivia, bool isTrailingTrivia)
	{
		SyntaxKind syntaxKind = trivia.Kind();
		return syntaxKind switch
		{
			SyntaxKind.SingleLineCommentTrivia => true, 
			SyntaxKind.MultiLineCommentTrivia => !isTrailingTrivia, 
			_ => SyntaxFacts.IsPreprocessorDirective(syntaxKind), 
		};
	}

	private static bool NeedsIndentAfterLineBreak(SyntaxTrivia trivia)
	{
		SyntaxKind syntaxKind = trivia.Kind();
		if (syntaxKind - 8541 <= (SyntaxKind)4)
		{
			return true;
		}
		return false;
	}

	private static bool IsLineBreak(SyntaxToken token)
	{
		return token.Kind() == SyntaxKind.XmlTextLiteralNewLineToken;
	}

	private static bool EndsInLineBreak(SyntaxTrivia trivia)
	{
		if (trivia.Kind() == SyntaxKind.EndOfLineTrivia)
		{
			return true;
		}
		if (trivia.Kind() == SyntaxKind.PreprocessingMessageTrivia || trivia.Kind() == SyntaxKind.DisabledTextTrivia)
		{
			string text = trivia.ToFullString();
			if (text.Length > 0)
			{
				return SyntaxFacts.IsNewLine(text.Last());
			}
			return false;
		}
		if (trivia.HasStructure)
		{
			SyntaxNode structure = trivia.GetStructure();
			SyntaxTriviaList trailingTrivia = structure.GetTrailingTrivia();
			if (trailingTrivia.Count > 0)
			{
				return EndsInLineBreak(trailingTrivia.Last());
			}
			return IsLineBreak(structure.GetLastToken());
		}
		return false;
	}

	private static bool IsWord(SyntaxKind kind)
	{
		if (kind != SyntaxKind.IdentifierToken)
		{
			return IsKeyword(kind);
		}
		return true;
	}

	private static bool IsKeyword(SyntaxKind kind)
	{
		if (!SyntaxFacts.IsKeywordKind(kind))
		{
			return SyntaxFacts.IsPreprocessorKeyword(kind);
		}
		return true;
	}

	private static bool TokenCharacterCanBeDoubled(char c)
	{
		switch (c)
		{
		case '"':
		case '+':
		case '-':
		case ':':
		case '<':
		case '=':
		case '?':
			return true;
		default:
			return false;
		}
	}

	private static int GetDeclarationDepth(SyntaxToken token)
	{
		return GetDeclarationDepth(token.Parent);
	}

	private static int GetDeclarationDepth(SyntaxTrivia trivia)
	{
		if (SyntaxFacts.IsPreprocessorDirective(trivia.Kind()))
		{
			return 0;
		}
		return GetDeclarationDepth(trivia.Token);
	}

	private static int GetDeclarationDepth(SyntaxNode node)
	{
		if (node != null)
		{
			if (node.IsStructuredTrivia)
			{
				return GetDeclarationDepth(((StructuredTriviaSyntax)node).ParentTrivia);
			}
			if (node.Parent != null)
			{
				if (node.Parent.IsKind(SyntaxKind.CompilationUnit))
				{
					return 0;
				}
				int declarationDepth = GetDeclarationDepth(node.Parent);
				if (node.Parent.IsKind(SyntaxKind.GlobalStatement))
				{
					return declarationDepth;
				}
				if (node.IsKind(SyntaxKind.IfStatement) && node.Parent.IsKind(SyntaxKind.ElseClause))
				{
					return declarationDepth;
				}
				if (node.Parent is BlockSyntax || (node is StatementSyntax && !(node is BlockSyntax)))
				{
					return declarationDepth + 1;
				}
				if (node is MemberDeclarationSyntax || node is AccessorDeclarationSyntax || node is TypeParameterConstraintClauseSyntax || node is SwitchSectionSyntax || node is UsingDirectiveSyntax || node is ExternAliasDirectiveSyntax || node is QueryExpressionSyntax || node is QueryContinuationSyntax)
				{
					return declarationDepth + 1;
				}
				return declarationDepth;
			}
		}
		return 0;
	}
}
