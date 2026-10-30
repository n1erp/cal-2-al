using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

internal static class SyntaxTreeExtensions
{
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "semanticModelOpt")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "attributes")]
	internal static bool IsExpressionContext(this SyntaxTree syntaxTree, int position, SyntaxToken tokenOnLeftOfPosition, bool attributes, CancellationToken cancellationToken, SemanticModel semanticModelOpt = null)
	{
		SyntaxToken previousTokenIfTouchingWord = tokenOnLeftOfPosition.GetPreviousTokenIfTouchingWord(position);
		PropertyValueSyntax propertyValueSyntax = previousTokenIfTouchingWord.Parent?.FirstAncestorOrSelf<PropertySyntax>()?.Value;
		if (propertyValueSyntax == null)
		{
			if (previousTokenIfTouchingWord.Kind.IsOperatorKeyword() && !previousTokenIfTouchingWord.IsKind(SyntaxKind.EqualsToken))
			{
				return true;
			}
			if (previousTokenIfTouchingWord.IsKind(SyntaxKind.OpenParenToken) && previousTokenIfTouchingWord.ParentIsKind(SyntaxKind.ParenthesizedExpression))
			{
				return true;
			}
			if ((previousTokenIfTouchingWord.IsKind(SyntaxKind.OpenParenToken) || previousTokenIfTouchingWord.IsKind(SyntaxKind.OpenBracketToken) || previousTokenIfTouchingWord.IsKind(SyntaxKind.CommaToken)) && previousTokenIfTouchingWord.ParentIsKind(SyntaxKind.ArgumentList, SyntaxKind.BracketedArgumentList, SyntaxKind.ExitStatement))
			{
				return true;
			}
			if (syntaxTree.IsFieldSourceExpressionContext(position, cancellationToken))
			{
				return true;
			}
			if (syntaxTree.IsReportColumnSourceExpressionContext(position, cancellationToken))
			{
				return true;
			}
			if (IsQueryElementSourceExpressionContext(syntaxTree, position, cancellationToken))
			{
				return false;
			}
			if (tokenOnLeftOfPosition.Parent != null && (IsIfCondition(position, tokenOnLeftOfPosition) || IsWhileCondition(position, tokenOnLeftOfPosition) || IsRepeatCondition(position, tokenOnLeftOfPosition) || IsCaseCondition(position, tokenOnLeftOfPosition) || IsForLoopExpression(position, tokenOnLeftOfPosition) || IsWithExpression(position, tokenOnLeftOfPosition)))
			{
				return true;
			}
		}
		else if (propertyValueSyntax.Kind.IsPropertyContextSourceContext())
		{
			return true;
		}
		return false;
	}

	internal static bool IsIfCondition(int position, SyntaxToken tokenOnLeftOfPosition)
	{
		IfStatementSyntax ifStatementSyntax = tokenOnLeftOfPosition.Parent.FirstAncestorOrSelf<IfStatementSyntax>();
		if (ifStatementSyntax != null && ifStatementSyntax.IfKeywordToken.SpanEnd < position && position < (ifStatementSyntax.ThenKeywordToken.IsKind(SyntaxKind.ThenKeyword) ? ifStatementSyntax.ThenKeywordToken.SpanStart : ifStatementSyntax.EndPosition))
		{
			return true;
		}
		return false;
	}

	internal static bool IsWhileCondition(int position, SyntaxToken tokenOnLeftOfPosition)
	{
		WhileStatementSyntax whileStatementSyntax = tokenOnLeftOfPosition.Parent.FirstAncestorOrSelf<WhileStatementSyntax>();
		if (whileStatementSyntax != null && whileStatementSyntax.WhileKeywordToken.SpanEnd < position && position < (whileStatementSyntax.DoKeywordToken.IsKind(SyntaxKind.DoKeyword) ? whileStatementSyntax.DoKeywordToken.SpanStart : whileStatementSyntax.EndPosition))
		{
			return true;
		}
		return false;
	}

	internal static bool IsRepeatCondition(int position, SyntaxToken tokenOnLeftOfPosition)
	{
		RepeatStatementSyntax repeatStatementSyntax = tokenOnLeftOfPosition.Parent.FirstAncestorOrSelf<RepeatStatementSyntax>();
		if (repeatStatementSyntax != null && repeatStatementSyntax.UntilKeywordToken.SpanEnd < position && position < (repeatStatementSyntax.SemicolonToken.IsKind(SyntaxKind.SemicolonToken) ? repeatStatementSyntax.SemicolonToken.SpanStart : repeatStatementSyntax.EndPosition))
		{
			return true;
		}
		return false;
	}

	internal static bool IsCaseCondition(int position, SyntaxToken tokenOnLeftOfPosition)
	{
		CaseStatementSyntax caseStatementSyntax = tokenOnLeftOfPosition.Parent.FirstAncestorOrSelf<CaseStatementSyntax>();
		if (caseStatementSyntax != null && caseStatementSyntax.CaseKeywordToken.SpanEnd < position && position < (caseStatementSyntax.OfKeywordToken.IsKind(SyntaxKind.OfKeyword) ? caseStatementSyntax.OfKeywordToken.SpanStart : caseStatementSyntax.EndPosition))
		{
			return true;
		}
		return false;
	}

	internal static bool IsWithExpression(int position, SyntaxToken tokenOnLeftOfPosition)
	{
		WithStatementSyntax withStatementSyntax = tokenOnLeftOfPosition.Parent.FirstAncestorOrSelf<WithStatementSyntax>();
		if (withStatementSyntax != null && withStatementSyntax.WithKeywordToken.SpanEnd < position && position < (withStatementSyntax.DoKeywordToken.IsKind(SyntaxKind.DoKeyword) ? withStatementSyntax.DoKeywordToken.SpanStart : withStatementSyntax.EndPosition))
		{
			return true;
		}
		return false;
	}

	internal static bool IsForLoopExpression(int position, SyntaxToken tokenOnLeftOfPosition)
	{
		ForStatementSyntax forStatementSyntax = tokenOnLeftOfPosition.Parent.FirstAncestorOrSelf<ForStatementSyntax>();
		if (forStatementSyntax == null)
		{
			return false;
		}
		if (forStatementSyntax.ForKeywordToken.SpanEnd < position && position < (forStatementSyntax.AssignToken.IsKind(SyntaxKind.AssignToken) ? forStatementSyntax.AssignToken.SpanStart : forStatementSyntax.EndPosition))
		{
			return true;
		}
		if (forStatementSyntax.AssignToken.SpanEnd < position && position < ((!forStatementSyntax.OperatorKeywordToken.IsKind(SyntaxKind.None)) ? forStatementSyntax.OperatorKeywordToken.SpanStart : forStatementSyntax.EndPosition))
		{
			return true;
		}
		if (forStatementSyntax.OperatorKeywordToken.SpanEnd < position && position < (forStatementSyntax.DoKeywordToken.IsKind(SyntaxKind.DoKeyword) ? forStatementSyntax.DoKeywordToken.SpanStart : forStatementSyntax.EndPosition))
		{
			return true;
		}
		return false;
	}

	internal static bool IsAfterKeyword(this SyntaxTree syntaxTree, int position, SyntaxKind kind, CancellationToken cancellationToken)
	{
		return syntaxTree.GetTokenBefore(position, cancellationToken).IsKind(kind);
	}

	internal static SyntaxToken GetTokenBefore(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
	{
		return syntaxTree.FindTokenOnLeftOfPosition(position, cancellationToken).GetPreviousTokenIfTouchingWord(position);
	}

	internal static bool IsInNonUserCode(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
	{
		if (!syntaxTree.IsEntirelyWithinNonUserCodeComment(position, cancellationToken))
		{
			return syntaxTree.IsEntirelyWithinStringLiteral(position, cancellationToken);
		}
		return true;
	}

	internal static bool IsDefinitelyNotTypeContext(this SyntaxTree syntaxTree, int position, SyntaxToken leftOfPosition, CancellationToken cancellationToken)
	{
		if (!leftOfPosition.IsPropertyDeclarationContext() && !leftOfPosition.IsPropertyValueContext() && !syntaxTree.IsInNonUserCode(position, cancellationToken))
		{
			return syntaxTree.IsRightOfDotOrColonColon(position, cancellationToken);
		}
		return true;
	}

	internal static bool IsApplicationObjectContext(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
	{
		SyntaxToken syntaxToken = syntaxTree.FindTokenOnLeftOfPosition(position, cancellationToken);
		if (syntaxTree.IsDefinitelyNotTypeContext(position, syntaxToken, cancellationToken))
		{
			return false;
		}
		if (syntaxToken.ParentIsKind(SyntaxKind.SubtypedDataType))
		{
			return true;
		}
		if (syntaxToken.ParentIsKind(SyntaxKind.EnumDataType))
		{
			return true;
		}
		if (syntaxToken.ParentIsKind(SyntaxKind.IdentifierName))
		{
			if (!syntaxToken.Parent.ParentIsKind(SyntaxKind.ObjectReference) || !syntaxToken.Parent.Parent.ParentIsKind(SyntaxKind.SubtypedDataType))
			{
				return syntaxToken.Parent.ParentIsKind(SyntaxKind.EnumDataType);
			}
			return true;
		}
		return false;
	}

	internal static bool IsFieldSourceExpressionContext(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
	{
		PageFieldSyntax ancestor = syntaxTree.FindTokenOnLeftOfPosition(position, cancellationToken).GetAncestor<PageFieldSyntax>();
		if (ancestor == null)
		{
			return false;
		}
		if (ancestor.SemicolonToken.IsKindAndNotMissing(SyntaxKind.SemicolonToken) && ancestor.SemicolonToken.SpanEnd <= position && position < (ancestor.CloseParenthesisToken.IsKindAndNotMissing(SyntaxKind.CloseParenToken) ? ancestor.CloseParenthesisToken.SpanEnd : ancestor.SpanEnd))
		{
			return true;
		}
		return false;
	}

	internal static bool IsReportColumnSourceExpressionContext(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
	{
		ReportColumnSyntax ancestor = syntaxTree.FindTokenOnLeftOfPosition(position, cancellationToken).GetAncestor<ReportColumnSyntax>();
		if (ancestor == null)
		{
			return false;
		}
		if (ancestor.SemicolonToken.IsKindAndNotMissing(SyntaxKind.SemicolonToken) && ancestor.SemicolonToken.SpanEnd <= position && position < (ancestor.CloseParenthesisToken.IsKindAndNotMissing(SyntaxKind.CloseParenToken) ? ancestor.CloseParenthesisToken.SpanStart : ancestor.SpanEnd))
		{
			return true;
		}
		return false;
	}

	private static bool IsQueryElementSourceExpressionContext(SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
	{
		if (!IsQueryColumnSourceExpressionContext(syntaxTree, position, cancellationToken))
		{
			return IsQueryFilterSourceExpressionContext(syntaxTree, position, cancellationToken);
		}
		return true;
	}

	private static bool IsQueryColumnSourceExpressionContext(SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
	{
		QueryColumnSyntax ancestor = syntaxTree.FindTokenOnLeftOfPosition(position, cancellationToken).GetAncestor<QueryColumnSyntax>();
		if (ancestor == null)
		{
			return false;
		}
		if (ancestor.SemicolonToken.IsKindAndNotMissing(SyntaxKind.SemicolonToken) && ancestor.SemicolonToken.SpanEnd <= position && position < (ancestor.CloseParenthesisToken.IsKindAndNotMissing(SyntaxKind.CloseParenToken) ? ancestor.CloseParenthesisToken.SpanStart : ancestor.SpanEnd))
		{
			return true;
		}
		return false;
	}

	private static bool IsQueryFilterSourceExpressionContext(SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
	{
		QueryFilterSyntax ancestor = syntaxTree.FindTokenOnLeftOfPosition(position, cancellationToken).GetAncestor<QueryFilterSyntax>();
		if (ancestor == null)
		{
			return false;
		}
		if (ancestor.SemicolonToken.IsKindAndNotMissing(SyntaxKind.SemicolonToken) && ancestor.SemicolonToken.SpanEnd <= position && position < (ancestor.CloseParenthesisToken.IsKindAndNotMissing(SyntaxKind.CloseParenToken) ? ancestor.CloseParenthesisToken.SpanStart : ancestor.SpanEnd))
		{
			return true;
		}
		return false;
	}

	internal static bool IsXmlPortNodeSourceExpressionContext(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
	{
		XmlPortFieldNodeSyntax ancestor = syntaxTree.FindTokenOnLeftOfPosition(position, cancellationToken).GetAncestor<XmlPortFieldNodeSyntax>();
		if (ancestor == null)
		{
			return false;
		}
		if (ancestor.SemicolonToken.IsKindAndNotMissing(SyntaxKind.SemicolonToken) && ancestor.SemicolonToken.SpanEnd <= position && position < (ancestor.CloseParenthesisToken.IsKindAndNotMissing(SyntaxKind.CloseParenToken) ? ancestor.CloseParenthesisToken.SpanStart : ancestor.SpanEnd))
		{
			return true;
		}
		return false;
	}

	internal static bool IsTypeContext(this SyntaxTree syntaxTree, int position, SyntaxToken targetToken, CancellationToken cancellationToken)
	{
		if (syntaxTree.IsDefinitelyNotTypeContext(position, targetToken, cancellationToken))
		{
			return false;
		}
		if (targetToken.IsKind(SyntaxKind.ColonToken) && targetToken.ParentIsKind(SyntaxKind.Parameter, SyntaxKind.VariableDeclaration, SyntaxKind.ReturnValue))
		{
			return true;
		}
		if (targetToken.IsInsideGenericDataTypeArgumentList() || targetToken.IsInsideArrayDataTypeArgumentList() || targetToken.IsDotNetTypeSubtype())
		{
			return true;
		}
		if (targetToken.IsKind(SyntaxKind.SemicolonToken) && targetToken.ParentIsKind(SyntaxKind.Field) && targetToken.GetPreviousToken().IsKind(SyntaxKind.IdentifierToken))
		{
			return true;
		}
		return false;
	}

	internal static bool IsParameterTypeContext(this SyntaxTree syntaxTree, int position, SyntaxToken tokenOnLeftOfPosition, CancellationToken cancellationToken)
	{
		SyntaxToken previousTokenIfTouchingWord = tokenOnLeftOfPosition.GetPreviousTokenIfTouchingWord(position);
		if (previousTokenIfTouchingWord.IsKind(SyntaxKind.VarKeyword))
		{
			position = previousTokenIfTouchingWord.SpanStart;
			tokenOnLeftOfPosition = syntaxTree.FindTokenOnLeftOfPosition(position, cancellationToken);
		}
		return syntaxTree.IsParameterModifierContext(position, tokenOnLeftOfPosition, cancellationToken);
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "syntaxTree")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cancellationToken")]
	internal static bool IsParameterModifierContext(this SyntaxTree syntaxTree, int position, SyntaxToken tokenOnLeftOfPosition, CancellationToken cancellationToken, int? allowableIndex = null)
	{
		SyntaxToken token = tokenOnLeftOfPosition;
		token = token.GetPreviousTokenIfTouchingWord(position);
		if (token.IsKind(SyntaxKind.OpenParenToken) && token.Parent.IsInMethodParameterList())
		{
			if (allowableIndex.HasValue && allowableIndex.Value != 0)
			{
				return false;
			}
			return true;
		}
		if (token.IsKind(SyntaxKind.CommaToken) && token.Parent.IsInMethodParameterList())
		{
			return true;
		}
		if (token.IsKind(SyntaxKind.CloseBracketToken) && token.ParentIsKind(SyntaxKind.Parameter))
		{
			return true;
		}
		return false;
	}

	internal static bool IsEntirelyWithinNonUserCodeComment(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
	{
		if (!syntaxTree.IsEntirelyWithinTopLevelSingleLineComment(position, cancellationToken))
		{
			return syntaxTree.IsEntirelyWithinMultiLineComment(position, cancellationToken);
		}
		return true;
	}

	internal static bool IsEntirelyWithinStringLiteral(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
	{
		SyntaxToken token = syntaxTree.GetRoot(cancellationToken).FindToken(position, findInsideTrivia: true);
		if (token.IsKind(SyntaxKind.EndOfFileToken))
		{
			token = token.GetPreviousToken(includeZeroWidth: false, includeSkipped: true);
		}
		if (token.IsKind(SyntaxKind.StringLiteralToken))
		{
			TextSpan span = token.Span;
			if (position <= span.Start || position >= span.End)
			{
				return AtEndOfIncompleteStringOrCharLiteral(token, position, '\'');
			}
			return true;
		}
		return false;
	}

	internal static bool IsEntirelyWithinComment(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
	{
		if (!syntaxTree.IsEntirelyWithinTopLevelSingleLineComment(position, cancellationToken))
		{
			return syntaxTree.IsEntirelyWithinMultiLineComment(position, cancellationToken);
		}
		return true;
	}

	internal static bool IsEntirelyWithinMultiLineComment(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
	{
		SyntaxTrivia trivia = syntaxTree.FindTriviaAndAdjustForEndOfFile(position, cancellationToken);
		if (trivia.IsMultiLineComment())
		{
			TextSpan fullSpan = trivia.FullSpan;
			if (!trivia.IsCompleteMultiLineComment())
			{
				if (position > fullSpan.Start)
				{
					return position <= fullSpan.End;
				}
				return false;
			}
			if (position > fullSpan.Start)
			{
				return position < fullSpan.End;
			}
			return false;
		}
		return false;
	}

	internal static bool IsEntirelyWithinTopLevelSingleLineComment(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
	{
		SyntaxTrivia trivia = syntaxTree.FindTriviaAndAdjustForEndOfFile(position, cancellationToken);
		if (trivia.Kind == SyntaxKind.EndOfLineTrivia)
		{
			trivia = trivia.GetPreviousTrivia(syntaxTree, cancellationToken);
		}
		if (trivia.IsSingleLineComment())
		{
			TextSpan fullSpan = trivia.FullSpan;
			if (position > fullSpan.Start && position <= fullSpan.End)
			{
				return true;
			}
		}
		return false;
	}

	internal static bool IsRightOfDotOrColonColon(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
	{
		if (!syntaxTree.IsRightOfDot(position, cancellationToken))
		{
			return syntaxTree.IsRightOfColonColon(position, cancellationToken);
		}
		return true;
	}

	internal static bool IsRightOfDot(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
	{
		return syntaxTree.IsRightOf(position, (SyntaxToken token) => token.Kind == SyntaxKind.DotToken || token.HasDotDotAsFirstTrivia(position), cancellationToken);
	}

	internal static bool IsRightOfColonColon(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
	{
		return syntaxTree.IsRightOf(position, (SyntaxToken t) => t.Kind == SyntaxKind.ColonColonToken, cancellationToken);
	}

	internal static SyntaxTrivia FindTriviaAndAdjustForEndOfFile(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken, bool findInsideTrivia = false)
	{
		SyntaxNode root = syntaxTree.GetRoot(cancellationToken);
		CompilationUnitSyntax compilationUnitSyntax = root as CompilationUnitSyntax;
		DebugAssertHelper.Assert(compilationUnitSyntax != null);
		SyntaxTrivia result = root.FindTrivia(position, findInsideTrivia);
		if (position == root.FullWidth)
		{
			SyntaxToken endOfFileToken = compilationUnitSyntax.GetEndOfFileToken();
			if (endOfFileToken.HasLeadingTrivia)
			{
				result = endOfFileToken.LeadingTrivia.Last();
			}
			else
			{
				SyntaxToken previousToken = endOfFileToken.GetPreviousToken(includeZeroWidth: false, includeSkipped: true);
				if (previousToken.HasTrailingTrivia)
				{
					result = previousToken.TrailingTrivia.Last();
				}
			}
		}
		return result;
	}

	internal static SyntaxToken FindTokenOnRightOfPosition(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken, bool includeSkipped = true)
	{
		return syntaxTree.GetRoot(cancellationToken).FindTokenOnRightOfPosition(position, includeSkipped);
	}

	internal static SyntaxToken FindTokenOnLeftOfPosition(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken, bool includeSkipped = true)
	{
		return syntaxTree.GetRoot(cancellationToken).FindTokenOnLeftOfPosition(position, includeSkipped);
	}

	internal static ObjectSyntax GetDeclaringObject(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
	{
		return syntaxTree.GetTokenBefore(position, cancellationToken).GetAncestor<ObjectSyntax>();
	}

	internal static bool IsPageChangeAnchorContext(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
	{
		SyntaxToken tokenBefore = syntaxTree.GetTokenBefore(position, cancellationToken);
		if (!tokenBefore.IsKind(SyntaxKind.OpenParenToken))
		{
			return false;
		}
		return syntaxTree.FindTokenOnLeftOfPosition(tokenBefore.Position, cancellationToken).Kind.IsPageChangeKeyword();
	}

	internal static bool IsFieldModifyContext(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
	{
		SyntaxToken tokenBefore = syntaxTree.GetTokenBefore(position, cancellationToken);
		if (!tokenBefore.IsKind(SyntaxKind.OpenParenToken))
		{
			return false;
		}
		tokenBefore = syntaxTree.FindTokenOnLeftOfPosition(tokenBefore.Position, cancellationToken);
		return tokenBefore.IsKind(SyntaxKind.ModifyKeyword);
	}

	internal static bool IsReferenceMemberListContext(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
	{
		SyntaxToken tokenBefore = syntaxTree.GetTokenBefore(position, cancellationToken);
		if (tokenBefore.IsKind(SyntaxKind.SemicolonToken) && IsReferenceMemberListElement(tokenBefore))
		{
			return tokenBefore.GetPreviousToken().Kind == SyntaxKind.IdentifierToken;
		}
		if (tokenBefore.IsKind(SyntaxKind.CommaToken))
		{
			return IsReferenceMemberListElement(tokenBefore);
		}
		return false;
	}

	private static bool IsReferenceMemberListElement(SyntaxToken token)
	{
		SyntaxKind kind = token.Parent.Kind;
		if (kind == SyntaxKind.Key || kind == SyntaxKind.FieldGroup || kind == SyntaxKind.FieldGroupAddChange)
		{
			return true;
		}
		return token.Parent.Kind.IsMoveChange();
	}

	private static bool AtEndOfIncompleteStringOrCharLiteral(SyntaxToken token, int position, char lastChar)
	{
		if (!token.Kind.IsStringLiteral())
		{
			return false;
		}
		if (position == token.Span.End)
		{
			if (token.Span.Length != 1)
			{
				if (token.Span.Length > 1)
				{
					return token.ToString().Cast<char>().LastOrDefault() != lastChar;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private static bool IsRightOf(this SyntaxTree syntaxTree, int position, Func<SyntaxToken, bool> predicate, CancellationToken cancellationToken)
	{
		SyntaxToken token = syntaxTree.FindTokenOnLeftOfPosition(position, cancellationToken);
		token = token.GetPreviousTokenIfTouchingWord(position);
		if (token.Kind == SyntaxKind.None)
		{
			return false;
		}
		return predicate(token);
	}

	[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "allText")]
	[Conditional("DEBUG")]
	internal static void VerifySource(this SyntaxTree tree, IEnumerable<TextChangeRange> changes = null)
	{
		SyntaxNode root = tree.GetRoot();
		SourceText text = tree.GetText();
		TextSpan textSpan = new TextSpan(0, text.Length);
		SyntaxNode syntaxNode = null;
		if (changes != null)
		{
			TextSpan change = TextChangeRange.Collapse(changes).Span;
			if (change != textSpan)
			{
				syntaxNode = root.DescendantNodes((SyntaxNode n) => n.FullSpan.Contains(change)).LastOrDefault();
			}
		}
		if (syntaxNode == null)
		{
			syntaxNode = root;
		}
		TextSpan fullSpan = syntaxNode.FullSpan;
		TextSpan? textSpan2 = fullSpan.Intersection(textSpan);
		int num;
		if (!textSpan2.HasValue)
		{
			num = 0;
		}
		else
		{
			string s = text.ToString(textSpan2.Value);
			string s2 = syntaxNode.ToFullString();
			num = FindFirstDifference(s, s2);
		}
		if (num >= 0)
		{
			num += fullSpan.Start;
			string message;
			if (num < text.Length)
			{
				LinePosition linePosition = text.Lines.GetLinePosition(num);
				TextLine textLine = text.Lines[linePosition.Line];
				text.ToString();
				message = string.Format(CultureInfo.InvariantCulture, "Unexpected difference at offset {0}: Line {1}, Column {2} \"{3}\"", num, linePosition.Line + 1, linePosition.Character + 1, textLine.ToString());
			}
			else
			{
				message = "Unexpected difference past end of the file";
			}
			DebugAssertHelper.Assert(condition: false, message);
		}
	}

	private static int FindFirstDifference(string s1, string s2)
	{
		int length = s1.Length;
		int length2 = s2.Length;
		int num = Math.Min(length, length2);
		for (int i = 0; i < num; i++)
		{
			if (s1[i] != s2[i])
			{
				return i;
			}
		}
		if (length != length2)
		{
			return num + 1;
		}
		return -1;
	}

	public static async Task<SyntaxToken> GetTouchingTokenAsync(this SyntaxTree syntaxTree, int position, Predicate<SyntaxToken> predicate, CancellationToken cancellationToken, bool findInsideTrivia = false)
	{
		if (position >= syntaxTree.Length)
		{
			return default;
		}
		SyntaxToken syntaxToken = (await syntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).FindToken(position, findInsideTrivia);
		if ((syntaxToken.Span.Contains(position) || syntaxToken.Span.End == position) && predicate(syntaxToken))
		{
			return syntaxToken;
		}
		syntaxToken = syntaxToken.GetPreviousToken();
		if (syntaxToken.Span.End == position && predicate(syntaxToken))
		{
			return syntaxToken;
		}
		return default;
	}
}
