using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

public static class SyntaxTokenExtensions
{
	internal static readonly HashSet<SyntaxKind> PropertyDeclaringNodes = new HashSet<SyntaxKind>
	{
		SyntaxKind.ProfileObject,
		SyntaxKind.EnumType,
		SyntaxKind.EnumValue,
		SyntaxKind.EnumExtensionType,
		SyntaxKind.DotNetPackage,
		SyntaxKind.DotNetAssembly,
		SyntaxKind.DotNetTypeDeclaration,
		SyntaxKind.CodeunitObject,
		SyntaxKind.TableObject,
		SyntaxKind.Field,
		SyntaxKind.FieldModification,
		SyntaxKind.Key,
		SyntaxKind.FieldGroupAddChange,
		SyntaxKind.FieldGroup,
		SyntaxKind.PageObject,
		SyntaxKind.PageField,
		SyntaxKind.PageLabel,
		SyntaxKind.PageGroup,
		SyntaxKind.PagePart,
		SyntaxKind.PageSystemPart,
		SyntaxKind.PageChartPart,
		SyntaxKind.PageUserControl,
		SyntaxKind.PageArea,
		SyntaxKind.PageView,
		SyntaxKind.PageActionGroup,
		SyntaxKind.PageActionArea,
		SyntaxKind.PageAction,
		SyntaxKind.PageActionSeparator,
		SyntaxKind.PageExtensionObject,
		SyntaxKind.PageCustomizationObject,
		SyntaxKind.TableExtensionObject,
		SyntaxKind.ControlModifyChange,
		SyntaxKind.ViewModifyChange,
		SyntaxKind.ActionModifyChange,
		SyntaxKind.QueryObject,
		SyntaxKind.QueryDataItem,
		SyntaxKind.QueryColumn,
		SyntaxKind.QueryFilter,
		SyntaxKind.ReportObject,
		SyntaxKind.ReportDataItem,
		SyntaxKind.ReportColumn,
		SyntaxKind.RequestPage,
		SyntaxKind.XmlPortObject,
		SyntaxKind.XmlPortTableElement,
		SyntaxKind.XmlPortFieldElement,
		SyntaxKind.XmlPortFieldAttribute,
		SyntaxKind.XmlPortTextElement,
		SyntaxKind.XmlPortTextAttribute,
		SyntaxKind.ControlAddInObject
	};

	public static bool IsKind(this SyntaxToken token, SyntaxKind kind)
	{
		return token.Kind == kind;
	}

	public static bool IsKind(this SyntaxToken token, SyntaxKind kind1, SyntaxKind kind2)
	{
		if (token.Kind != kind1)
		{
			return token.Kind == kind2;
		}
		return true;
	}

	public static bool IsKind(this SyntaxToken token, SyntaxKind kind1, SyntaxKind kind2, SyntaxKind kind3)
	{
		if (token.Kind != kind1 && token.Kind != kind2)
		{
			return token.Kind == kind3;
		}
		return true;
	}

	public static bool IsKind(this SyntaxToken token, SyntaxKind kind1, SyntaxKind kind2, SyntaxKind kind3, SyntaxKind kind4)
	{
		if (token.Kind != kind1 && token.Kind != kind2 && token.Kind != kind3)
		{
			return token.Kind == kind4;
		}
		return true;
	}

	public static bool IsKind(this SyntaxTrivia trivia, SyntaxKind kind)
	{
		return trivia.Kind == kind;
	}

	public static bool IntersectsWith(this SyntaxToken token, int position)
	{
		return token.Span.IntersectsWith(position);
	}

	public static SyntaxToken GetPreviousTokenIfTouchingWord(this SyntaxToken token, int position)
	{
		if (!token.IntersectsWith(position) || !token.IsWord())
		{
			return token;
		}
		return token.GetPreviousToken(includeZeroWidth: false, includeSkipped: true);
	}

	public static bool IsWord(this SyntaxToken token)
	{
		if (token.IsKind(SyntaxKind.IdentifierToken))
		{
			return true;
		}
		if (!token.Kind.IsKeyword())
		{
			return token.Kind.IsPropertyKeyword();
		}
		return true;
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	internal static bool IsBeginningOfStatementContext(this SyntaxToken token)
	{
		if (token.Kind == SyntaxKind.BeginKeyword && token.ParentIsKind(SyntaxKind.Block))
		{
			return true;
		}
		if (token.Kind == SyntaxKind.SemicolonToken)
		{
			StatementSyntax ancestor = token.GetAncestor<StatementSyntax>();
			if (ancestor != null && !ancestor.ParentIsKind(SyntaxKind.MethodDeclaration) && ancestor.GetLastToken(includeZeroWidth: true) == token && token.GetNextToken().Parent.IsAnyParentKind(SyntaxKind.Block))
			{
				return true;
			}
		}
		if (token.Kind == SyntaxKind.ThenKeyword && token.ParentIsKind(SyntaxKind.IfStatement))
		{
			return true;
		}
		if (token.Kind == SyntaxKind.RepeatKeyword && token.ParentIsKind(SyntaxKind.RepeatStatement))
		{
			return true;
		}
		if (token.Kind == SyntaxKind.DoKeyword && token.ParentIsKind(SyntaxKind.WhileStatement, SyntaxKind.ForStatement, SyntaxKind.WithStatement))
		{
			return true;
		}
		if (token.Kind == SyntaxKind.EndKeyword && token.ParentIsKind(SyntaxKind.Block))
		{
			if (token.Parent.Parent is StatementSyntax)
			{
				if (!token.ParentIsKind(SyntaxKind.RepeatStatement))
				{
					return true;
				}
			}
			else if (token.ParentIsKind(SyntaxKind.ElseKeyword) || token.ParentIsKind(SyntaxKind.ElseTableRelationExpression) || token.ParentIsKind(SyntaxKind.CaseStatement) || token.ParentIsKind(SyntaxKind.CaseElse))
			{
				return true;
			}
		}
		if (token.Kind == SyntaxKind.ColonToken && token.ParentIsKind(SyntaxKind.CaseLine))
		{
			return true;
		}
		if (token.Kind == SyntaxKind.UntilKeyword && token.ParentIsKind(SyntaxKind.RepeatStatement))
		{
			return true;
		}
		if (token.Kind == SyntaxKind.CloseParenToken)
		{
			SyntaxNode parent = token.Parent;
			if (parent.IsKind(SyntaxKind.ForStatement) || parent.IsKind(SyntaxKind.ForEachStatement) || parent.IsKind(SyntaxKind.WhileStatement) || parent.IsKind(SyntaxKind.IfStatement))
			{
				return true;
			}
		}
		if (token.Kind == SyntaxKind.ElseKeyword)
		{
			return true;
		}
		return false;
	}

	internal static bool IsAnyParentKind(this SyntaxNode node, SyntaxKind kind)
	{
		while (node != null)
		{
			if (node.Kind == kind)
			{
				return true;
			}
			node = node.Parent;
		}
		return false;
	}

	internal static bool IsMemberNamingContext(this SyntaxToken token)
	{
		SyntaxToken previousTokenIfTouchingWord = token.GetPreviousTokenIfTouchingWord(token.Position);
		SyntaxNode syntaxNode = null;
		if (token.IsKind(SyntaxKind.OpenParenToken))
		{
			syntaxNode = token.Parent;
		}
		else if (previousTokenIfTouchingWord.IsKind(SyntaxKind.OpenParenToken))
		{
			syntaxNode = token.Parent.Parent;
		}
		return syntaxNode?.Kind.CanGetNameRecommended() ?? false;
	}

	internal static bool IsPropertyDeclarationContext(this SyntaxToken token)
	{
		if (token.IsKind(SyntaxKind.SemicolonToken) && token.ParentIsKind(SyntaxKind.Property))
		{
			return true;
		}
		if (token.ParentIsKind(SyntaxKind.PropertyName))
		{
			SyntaxToken previousTokenIfTouchingWord = token.GetPreviousTokenIfTouchingWord(token.Position);
			if (!previousTokenIfTouchingWord.IsKind(SyntaxKind.SemicolonToken) && previousTokenIfTouchingWord.GetAncestor<TableFilterExpressionSyntax>() != null)
			{
				return false;
			}
			return true;
		}
		if (token.IsKind(SyntaxKind.OpenBraceToken))
		{
			return token.IsBraceTokenStartingAPropertyDeclaration();
		}
		return false;
	}

	internal static bool IsPropertyValueContext(this SyntaxToken token)
	{
		if (token.IsKind(SyntaxKind.EqualsToken) && token.ParentIsKind(SyntaxKind.Property))
		{
			return true;
		}
		PropertySyntax ancestor = token.GetAncestor<PropertySyntax>();
		if (ancestor == null)
		{
			return false;
		}
		return ancestor.EqualsToken.Position < token.Position;
	}

	internal static T GetAncestor<T>(this SyntaxToken token, Func<T, bool> predicate = null) where T : SyntaxNode
	{
		SyntaxNode parent = token.Parent;
		if (parent == null)
		{
			return null;
		}
		return parent.FirstAncestorOrSelf(predicate);
	}

	internal static IEnumerable<T> GetAncestors<T>(this SyntaxToken token) where T : SyntaxNode
	{
		return token.Parent?.AncestorsAndSelf().OfType<T>() ?? SpecializedCollections.EmptyEnumerable<T>();
	}

	internal static bool ParentIsKind(this SyntaxToken token, SyntaxKind kind)
	{
		if (token.Parent != null)
		{
			return token.Parent.IsKind(kind);
		}
		return false;
	}

	internal static bool ParentIsKind(this SyntaxToken token, SyntaxKind kind1, SyntaxKind kind2)
	{
		if (!token.ParentIsKind(kind1))
		{
			return token.ParentIsKind(kind2);
		}
		return true;
	}

	public static bool ParentIsKind(this SyntaxToken token, params SyntaxKind[] kinds)
	{
		if (token.Parent != null)
		{
			return kinds.Any((SyntaxKind k) => token.Parent.Kind == k);
		}
		return false;
	}

	internal static bool IsArgumentExpression(this SyntaxToken token, int position)
	{
		SyntaxToken previousTokenIfTouchingWord = token.GetPreviousTokenIfTouchingWord(position);
		if (!previousTokenIfTouchingWord.IsKind(SyntaxKind.OpenParenToken))
		{
			return previousTokenIfTouchingWord.IsKind(SyntaxKind.CommaToken);
		}
		return true;
	}

	public static bool IsInsideArrayDataTypeArgumentList(this SyntaxToken token)
	{
		if (token.ParentIsKind(SyntaxKind.Array))
		{
			return token.IsKind(SyntaxKind.OfKeyword);
		}
		return false;
	}

	public static bool IsInsideGenericDataTypeArgumentList(this SyntaxToken token)
	{
		if (token.ParentIsKind(SyntaxKind.GenericDataType))
		{
			return token.IsKind(SyntaxKind.OpenBracketToken, SyntaxKind.CommaToken);
		}
		return false;
	}

	public static bool IsDotNetTypeSubtype(this SyntaxToken token)
	{
		if (token.ParentIsKind(SyntaxKind.SubtypedDataType))
		{
			return token.IsKind(SyntaxKind.DotNetKeyword);
		}
		return false;
	}

	public static bool IsBraceTokenStartingAPropertyDeclaration(this SyntaxToken token)
	{
		if (token.Kind != SyntaxKind.OpenBraceToken || token.Parent == null)
		{
			return false;
		}
		return PropertyDeclaringNodes.Contains(token.Parent.Kind);
	}

	public static bool HasDotDotAsFirstTrivia(this SyntaxToken token, int? position = null)
	{
		SyntaxTrivia? syntaxTrivia = token.TrailingTrivia.FirstOrDefault();
		InternalSyntaxToken internalSyntaxToken = syntaxTrivia?.UnderlyingNode?.GetFirstToken();
		if (internalSyntaxToken != null && internalSyntaxToken.Kind == SyntaxKind.DotDotToken)
		{
			if (syntaxTrivia?.SpanStart != position - 1)
			{
				return !position.HasValue;
			}
			return true;
		}
		return false;
	}
}
