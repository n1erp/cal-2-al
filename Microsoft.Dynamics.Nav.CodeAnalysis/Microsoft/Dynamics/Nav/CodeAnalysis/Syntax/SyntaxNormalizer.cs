using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

public class SyntaxNormalizer : SyntaxRewriter, IDisposable
{
	private readonly TextSpan consideredSpan;

	private readonly int initialDepth;

	private readonly string indentWhitespace;

	private readonly bool useElasticTrivia;

	[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
	private SyntaxToken previousToken;

	private bool afterLineBreak;

	private bool afterIndentation;

	private ArrayBuilder<SyntaxTrivia> indentations;

	internal static TNode Format<TNode>(TNode node, string indentWhitespace, bool useElasticTrivia = false) where TNode : SyntaxNode
	{
		using SyntaxNormalizer syntaxNormalizer = new SyntaxNormalizer(node.FullSpan, GetDeclarationDepth(node), indentWhitespace, useElasticTrivia);
		return (TNode)syntaxNormalizer.Visit(node);
	}

	private SyntaxNormalizer(TextSpan consideredSpan, int initialDepth, string indentWhitespace, bool useElasticTrivia)
		: base(visitIntoStructuredTrivia: true)
	{
		this.consideredSpan = consideredSpan;
		this.initialDepth = initialDepth;
		this.indentWhitespace = indentWhitespace;
		this.useElasticTrivia = useElasticTrivia;
		afterLineBreak = true;
	}

	private static int GetDeclarationDepth(SyntaxTrivia trivia)
	{
		return GetDeclarationDepth(trivia.Token);
	}

	private static int GetDeclarationDepth(SyntaxToken token)
	{
		return GetDeclarationDepth(token.Parent);
	}

	private static int GetDeclarationDepth(SyntaxNode node)
	{
		if (node != null && node.Parent != null)
		{
			if (node.Parent.IsKind(SyntaxKind.CompilationUnit))
			{
				return 0;
			}
			int declarationDepth = GetDeclarationDepth(node.Parent);
			if (node.Parent is BlockSyntax || (node is StatementSyntax && !(node is BlockSyntax)))
			{
				return declarationDepth + 1;
			}
			if (node.Parent is ApplicationObjectSyntax)
			{
				return declarationDepth + 1;
			}
			switch (node.Parent.Kind)
			{
			case SyntaxKind.FieldList:
			case SyntaxKind.Field:
			case SyntaxKind.FieldExtensionList:
			case SyntaxKind.KeyList:
			case SyntaxKind.Key:
			case SyntaxKind.FieldGroupList:
			case SyntaxKind.PageLayout:
			case SyntaxKind.PageActionList:
			case SyntaxKind.GroupActionList:
			case SyntaxKind.PageArea:
			case SyntaxKind.PageGroup:
			case SyntaxKind.PageField:
			case SyntaxKind.PageLabel:
			case SyntaxKind.PagePart:
			case SyntaxKind.PageSystemPart:
			case SyntaxKind.PageChartPart:
			case SyntaxKind.PageAction:
			case SyntaxKind.PageActionGroup:
			case SyntaxKind.PageActionArea:
			case SyntaxKind.PageExtensionActionList:
			case SyntaxKind.ActionAddChange:
			case SyntaxKind.ActionModifyChange:
			case SyntaxKind.PageExtensionLayout:
			case SyntaxKind.ControlAddChange:
			case SyntaxKind.PageExtensionViewList:
			case SyntaxKind.ViewAddChange:
			case SyntaxKind.ViewModifyChange:
			case SyntaxKind.PageViewList:
			case SyntaxKind.PageView:
				return declarationDepth + 1;
			default:
				if (node.Parent is ActionChangeBaseSyntax)
				{
					return declarationDepth + 1;
				}
				return declarationDepth;
			}
		}
		return 0;
	}

	public override SyntaxToken VisitToken(SyntaxToken token)
	{
		if (token.Kind == SyntaxKind.None || (token.IsMissing && token.FullWidth == 0))
		{
			return token;
		}
		try
		{
			SyntaxToken syntaxToken = token;
			int declarationDepth = GetDeclarationDepth(token);
			syntaxToken = syntaxToken.WithLeadingTrivia(RewriteTrivia(token.LeadingTrivia, declarationDepth, isTrailing: false, NeedsIndentAfterLineBreak(token), mustHaveSeparator: false, 0));
			SyntaxToken nextRelevantToken = GetNextRelevantToken(token);
			afterLineBreak = false;
			afterIndentation = false;
			int lineBreaksAfter = LineBreaksAfter(token, nextRelevantToken);
			bool mustHaveSeparator = NeedsSeparator(token, nextRelevantToken);
			return syntaxToken.WithTrailingTrivia(RewriteTrivia(token.TrailingTrivia, declarationDepth, isTrailing: true, indentAfterLineBreak: false, mustHaveSeparator, lineBreaksAfter));
		}
		finally
		{
			previousToken = token;
		}
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	private SyntaxTriviaList RewriteTrivia(SyntaxTriviaList triviaList, int depth, bool isTrailing, bool indentAfterLineBreak, bool mustHaveSeparator, int lineBreaksAfter)
	{
		ArrayBuilder<SyntaxTrivia> instance = ArrayBuilder<SyntaxTrivia>.GetInstance(triviaList.Count);
		try
		{
			foreach (SyntaxTrivia item in triviaList)
			{
				if (item.IsKind(SyntaxKind.WhiteSpaceTrivia) || item.IsKind(SyntaxKind.EndOfLineTrivia) || item.FullWidth == 0)
				{
					continue;
				}
				bool flag = (instance.Count > 0 && NeedsSeparatorBetween(instance.Last())) || ((instance.Count == 0) & isTrailing);
				if (instance.Count > 0 && NeedsLineBreakBetween(instance.Last(), item, isTrailing) && !afterLineBreak)
				{
					instance.Add(GetCarriageReturnLineFeed());
					afterLineBreak = true;
					afterIndentation = false;
				}
				if (afterLineBreak)
				{
					if (!afterIndentation && NeedsIndentAfterLineBreak(item))
					{
						instance.Add(GetIndentation(GetDeclarationDepth(item)));
						afterIndentation = true;
					}
				}
				else if (flag)
				{
					instance.Add(GetSpace());
					afterLineBreak = false;
					afterIndentation = false;
				}
				instance.Add(item);
				if (NeedsLineBreakAfter(item, isTrailing) && (instance.Count == 0 || !EndsInLineBreak(instance.Last())))
				{
					instance.Add(GetCarriageReturnLineFeed());
					afterLineBreak = true;
					afterIndentation = false;
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
					instance.Add(GetCarriageReturnLineFeed());
					afterLineBreak = true;
					afterIndentation = false;
				}
			}
			else if (indentAfterLineBreak && afterLineBreak && !afterIndentation)
			{
				instance.Add(GetIndentation(depth));
				afterIndentation = true;
			}
			else if (mustHaveSeparator)
			{
				instance.Add(GetSpace());
				afterLineBreak = false;
				afterIndentation = false;
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

	private SyntaxToken GetNextRelevantToken(SyntaxToken token)
	{
		SyntaxToken nextToken = token.GetNextToken((SyntaxToken t) => SyntaxToken.NonZeroWidth(t));
		if (consideredSpan.Contains(nextToken.FullSpan))
		{
			return nextToken;
		}
		return default;
	}

	private SyntaxTrivia GetSpace()
	{
		if (!useElasticTrivia)
		{
			return SyntaxFactory.Space;
		}
		return SyntaxFactory.ElasticSpace;
	}

	private SyntaxTrivia GetCarriageReturnLineFeed()
	{
		if (!useElasticTrivia)
		{
			return SyntaxFactory.CarriageReturnLinefeed;
		}
		return SyntaxFactory.ElasticCarriageReturnLinefeed;
	}

	private static bool NeedsSeparatorBetween(SyntaxTrivia trivia)
	{
		SyntaxKind kind = trivia.Kind;
		if (kind == SyntaxKind.None || kind == SyntaxKind.WhiteSpaceTrivia)
		{
			return false;
		}
		return true;
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "next")]
	private static bool NeedsLineBreakBetween(SyntaxTrivia trivia, SyntaxTrivia next, bool isTrailingTrivia)
	{
		return NeedsLineBreakAfter(trivia, isTrailingTrivia);
	}

	private static bool NeedsLineBreakAfter(SyntaxTrivia trivia, bool isTrailingTrivia)
	{
		return trivia.Kind switch
		{
			SyntaxKind.LineCommentTrivia => true, 
			SyntaxKind.CommentTrivia => !isTrailingTrivia, 
			_ => false, 
		};
	}

	private static bool NeedsIndentAfterLineBreak(SyntaxToken token)
	{
		return !token.IsKind(SyntaxKind.EndOfFileToken);
	}

	private static bool NeedsIndentAfterLineBreak(SyntaxTrivia trivia)
	{
		SyntaxKind kind = trivia.Kind;
		if ((uint)(kind - 166) <= 1u)
		{
			return true;
		}
		return false;
	}

	private static bool NeedsSeparator(SyntaxToken token, SyntaxToken next)
	{
		if (token.Parent == null || next.Parent == null)
		{
			return false;
		}
		if (token.Parent is BinaryExpressionSyntax || next.Parent is BinaryExpressionSyntax)
		{
			return true;
		}
		if ((token.Parent.IsKind(SyntaxKind.Property) && token.IsKind(SyntaxKind.EqualsToken)) || (token.Parent.IsKind(SyntaxKind.PropertyName) && next.IsKind(SyntaxKind.EqualsToken)))
		{
			return true;
		}
		if (token.IsKind(SyntaxKind.CommaToken))
		{
			return true;
		}
		if (token.IsKind(SyntaxKind.ColonToken))
		{
			return true;
		}
		if (token.Kind.IsKeyword() && !next.IsKind(SyntaxKind.ColonToken) && !next.IsKind(SyntaxKind.DotToken) && !next.IsKind(SyntaxKind.SemicolonToken) && !next.IsKind(SyntaxKind.OpenBracketToken) && !next.IsKind(SyntaxKind.CloseParenToken) && !next.IsKind(SyntaxKind.CloseBraceToken) && !next.IsKind(SyntaxKind.ColonColonToken) && !next.IsKind(SyntaxKind.GreaterThanToken) && !next.IsKind(SyntaxKind.CommaToken) && !next.IsKind(SyntaxKind.OpenParenToken))
		{
			return true;
		}
		if (IsWordOrLiteral(token.Kind) && IsWordOrLiteral(next.Kind))
		{
			return true;
		}
		return false;
	}

	private static bool IsWord(SyntaxKind kind)
	{
		if (kind != SyntaxKind.IdentifierToken && !kind.IsKeyword())
		{
			return kind.IsPropertyKeyword();
		}
		return true;
	}

	private static bool IsWordOrLiteral(SyntaxKind kind)
	{
		if (!IsWord(kind))
		{
			return kind.IsLiteral();
		}
		return true;
	}

	private static int LineBreaksAfter(SyntaxToken currentToken, SyntaxToken nextToken)
	{
		if (nextToken.Kind == SyntaxKind.None)
		{
			return 0;
		}
		switch (currentToken.Kind)
		{
		case SyntaxKind.None:
			return 0;
		case SyntaxKind.OpenBraceToken:
			return 1;
		case SyntaxKind.BeginKeyword:
			return 1;
		case SyntaxKind.SemicolonToken:
			return LineBreaksAfterSemicolon(currentToken, nextToken);
		case SyntaxKind.ElseKeyword:
			if (nextToken.Kind == SyntaxKind.IfKeyword)
			{
				return 0;
			}
			return 1;
		case SyntaxKind.CloseParenToken:
			if (currentToken.Parent.Kind.IsMoveChange())
			{
				return 1;
			}
			break;
		case SyntaxKind.CloseBraceToken:
			return 1;
		case SyntaxKind.CloseBracketToken:
			if (currentToken.ParentIsKind(SyntaxKind.MemberAttribute))
			{
				return 1;
			}
			break;
		}
		switch (nextToken.Kind)
		{
		case SyntaxKind.OpenBraceToken:
		case SyntaxKind.CloseBraceToken:
			return 1;
		case SyntaxKind.ElseKeyword:
			return 1;
		default:
			return 0;
		}
	}

	private SyntaxTrivia GetIndentation(int count)
	{
		count = Math.Max(count - initialDepth, 0);
		int capacity = count + 1;
		if (indentations == null)
		{
			indentations = ArrayBuilder<SyntaxTrivia>.GetInstance(capacity);
		}
		else
		{
			indentations.EnsureCapacity(capacity);
		}
		for (int i = indentations.Count; i <= count; i++)
		{
			string text = ((i == 0) ? string.Empty : (indentations[i - 1].ToString() + indentWhitespace));
			indentations.Add(SyntaxFactory.WhiteSpace(text, useElasticTrivia));
		}
		return indentations[count];
	}

	private static bool EndsInLineBreak(SyntaxTrivia trivia)
	{
		if (trivia.Kind == SyntaxKind.EndOfLineTrivia)
		{
			return true;
		}
		return false;
	}

	private static int LineBreaksAfterSemicolon(SyntaxToken currentToken, SyntaxToken nextToken)
	{
		switch (currentToken.Parent.Kind)
		{
		case SyntaxKind.Field:
		case SyntaxKind.Key:
		case SyntaxKind.FieldGroup:
		case SyntaxKind.PageGroup:
		case SyntaxKind.PageField:
		case SyntaxKind.PageLabel:
		case SyntaxKind.PagePart:
		case SyntaxKind.PageSystemPart:
		case SyntaxKind.PageChartPart:
		case SyntaxKind.PageAction:
		case SyntaxKind.PageActionGroup:
		case SyntaxKind.ActionMoveChange:
		case SyntaxKind.ControlMoveChange:
		case SyntaxKind.ViewMoveChange:
			return 0;
		default:
			if (currentToken.Parent.IsKind(SyntaxKind.Property) && !nextToken.Parent.IsKind(SyntaxKind.PropertyName) && !nextToken.IsKind(SyntaxKind.CloseBraceToken))
			{
				return 2;
			}
			return 1;
		}
	}

	public void Dispose()
	{
		indentations?.Free();
	}
}
