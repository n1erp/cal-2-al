using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal sealed class ApplicationObjectIdRecommender : TextRecommender
{
	protected internal override async Task<IEnumerable<CompletionItem>> RecommendTextAsync(CompletionContext context, MemberSyntaxContext syntaxContext, CancellationToken cancellationToken)
	{
		SyntaxToken leftToken = syntaxContext.LeftToken;
		IdSpaces idSpaces = syntaxContext.SemanticModel.Compilation.Options.IdSpaces;
		int position = context.Position;
		if (!ShouldSuggestApplicationObjectId(leftToken, idSpaces, position))
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		SyntaxToken syntaxToken = leftToken;
		if (leftToken.Kind == SyntaxKind.Int32LiteralToken)
		{
			syntaxToken = leftToken.GetPreviousToken();
		}
		if (!syntaxToken.Kind.IsObjectKeyword() || !(syntaxToken.Parent?.Kind.IsApplicationObject() ?? false))
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		Project project = context.Document.Project;
		SyntaxKind objectSyntaxKindFromKeywordSyntaxKind = GetObjectSyntaxKindFromKeywordSyntaxKind(syntaxToken.Kind);
		string text = GetNextFreeObjectIdInApplicationObjectRange(project, idSpaces, objectSyntaxKindFromKeywordSyntaxKind).ToString(CultureInfo.InvariantCulture);
		if (leftToken.Kind == SyntaxKind.Int32LiteralToken)
		{
			return new CompletionItem[1] { CreateItem(text, leftToken.Span) };
		}
		SyntaxToken nextToken = leftToken.GetNextToken();
		if (nextToken.Kind == SyntaxKind.Int32LiteralToken && nextToken.Span.IntersectsWith(position))
		{
			return new CompletionItem[1] { CreateItem(text, nextToken.Span) };
		}
		TextSpan span = new TextSpan(position, 0);
		if (nextToken.SpanStart == position)
		{
			text += " ";
		}
		else if (leftToken.Span.IntersectsWith(position))
		{
			text = " " + text;
		}
		return new CompletionItem[1] { CreateItem(text, span) };
	}

	private static int GetNextFreeObjectIdInApplicationObjectRange(Project project, IdSpaces idSpaces, SyntaxKind applicationObjectKind)
	{
		HashSet<int> setOfUsedIds = GetSetOfUsedIds(project, applicationObjectKind);
		foreach (IdSpace applicationObjectIdRange in idSpaces.ApplicationObjectIdRanges)
		{
			for (int i = applicationObjectIdRange.ApplicationObjectIdRangeStart; i <= applicationObjectIdRange.ApplicationObjectIdRangeEnd; i++)
			{
				if (!setOfUsedIds.Contains(i))
				{
					return i;
				}
			}
		}
		IdSpace idSpace = idSpaces.ApplicationObjectIdRanges.LastOrDefault();
		if ((object)idSpace == null)
		{
			return 0;
		}
		return idSpace.ApplicationObjectIdRangeEnd + 1;
	}

	private static HashSet<int> GetSetOfUsedIds(Project project, SyntaxKind applicationObjectKind)
	{
		HashSet<int> hashSet = new HashSet<int>();
		foreach (Document document in project.Documents)
		{
			IEnumerable<int> enumerable = (document.GetSyntaxTreeAsync().Result.GetRoot().ChildNodes()?.OfType<ApplicationObjectSyntax>())?.Where((ApplicationObjectSyntax x) => x.Kind == applicationObjectKind && x.ObjectId.Value.Value != null).Select((ApplicationObjectSyntax x) => (int)x.ObjectId.Value.Value);
			if (enumerable != null)
			{
				hashSet.AddRange(enumerable);
			}
		}
		return hashSet;
	}

	private static bool ShouldSuggestApplicationObjectId(SyntaxToken targetToken, IdSpaces idSpaces, int position)
	{
		SyntaxToken nextToken = targetToken.GetNextToken();
		SyntaxToken previousToken = targetToken.GetPreviousToken();
		if (nextToken.IsKind(SyntaxKind.Int32LiteralToken))
		{
			return !idSpaces.IsWithinSpaces((int)nextToken.Value);
		}
		if (GetObjectSyntaxKindFromKeywordSyntaxKind(previousToken.Kind) != SyntaxKind.None && targetToken.Kind == SyntaxKind.Int32LiteralToken)
		{
			if (!idSpaces.IsWithinSpaces((int)targetToken.Value))
			{
				return targetToken.Span.IntersectsWith(position);
			}
			return false;
		}
		if (previousToken.Kind == SyntaxKind.None || previousToken.Kind == SyntaxKind.CloseBraceToken)
		{
			return targetToken.Parent?.Kind.IsApplicationObjectWithId() ?? false;
		}
		return false;
	}

	private static SyntaxKind GetObjectSyntaxKindFromKeywordSyntaxKind(SyntaxKind syntaxKeyword)
	{
		return syntaxKeyword switch
		{
			SyntaxKind.CodeunitKeyword => SyntaxKind.CodeunitObject, 
			SyntaxKind.EnumKeyword => SyntaxKind.EnumType, 
			SyntaxKind.EnumExtensionKeyword => SyntaxKind.EnumExtensionType, 
			SyntaxKind.PageKeyword => SyntaxKind.PageObject, 
			SyntaxKind.PageExtensionKeyword => SyntaxKind.PageExtensionObject, 
			SyntaxKind.QueryKeyword => SyntaxKind.QueryObject, 
			SyntaxKind.ReportKeyword => SyntaxKind.ReportObject, 
			SyntaxKind.TableKeyword => SyntaxKind.TableObject, 
			SyntaxKind.TableExtensionKeyword => SyntaxKind.TableExtensionObject, 
			SyntaxKind.XmlPortKeyword => SyntaxKind.XmlPortObject, 
			_ => SyntaxKind.None, 
		};
	}

	private static CompletionItem CreateItem(string displayText, TextSpan span = default(TextSpan))
	{
		Glyph? glyph = Glyph.None;
		return CommonCompletionItem.Create(displayText, span, glyph, null, null, null, displayText, displayText);
	}
}
