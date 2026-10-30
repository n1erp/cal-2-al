using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class SourceDocumentationCommentUtils
{
	internal static string GetAndCacheDocumentationComment(Symbol symbol, bool expandIncludes, ref string lazyXmlText)
	{
		if (lazyXmlText == null)
		{
			string documentationCommentXml = DocumentationCommentCompiler.GetDocumentationCommentXml(symbol, expandIncludes, default);
			Interlocked.CompareExchange(ref lazyXmlText, documentationCommentXml, null);
		}
		return lazyXmlText;
	}

	internal static ImmutableArray<DocumentationCommentTriviaSyntax> GetDocumentationCommentTriviaFromSyntaxNode(CSharpSyntaxNode syntaxNode, DiagnosticBag diagnostics)
	{
		if ((int)syntaxNode.SyntaxTree.Options.DocumentationMode < 1)
		{
			return ImmutableArray<DocumentationCommentTriviaSyntax>.Empty;
		}
		if (syntaxNode.Kind() == SyntaxKind.VariableDeclarator)
		{
			CSharpSyntaxNode cSharpSyntaxNode;
			for (cSharpSyntaxNode = syntaxNode; cSharpSyntaxNode != null; cSharpSyntaxNode = cSharpSyntaxNode.Parent)
			{
				SyntaxKind syntaxKind = cSharpSyntaxNode.Kind();
				if (syntaxKind == SyntaxKind.FieldDeclaration || syntaxKind == SyntaxKind.EventFieldDeclaration)
				{
					break;
				}
			}
			if (cSharpSyntaxNode != null)
			{
				syntaxNode = cSharpSyntaxNode;
			}
		}
		ArrayBuilder<DocumentationCommentTriviaSyntax> arrayBuilder = null;
		bool flag = false;
		foreach (SyntaxTrivia item in syntaxNode.GetLeadingTrivia().Reverse())
		{
			switch (item.Kind())
			{
			case SyntaxKind.SingleLineDocumentationCommentTrivia:
			case SyntaxKind.MultiLineDocumentationCommentTrivia:
				if (flag)
				{
					SyntaxTree syntaxTree = item.SyntaxTree;
					if (syntaxTree.ReportDocumentationCommentDiagnostics())
					{
						int position = item.Position;
						diagnostics.Add(ErrorCode.WRN_UnprocessedXMLComment, new SourceLocation(syntaxTree, new TextSpan(position, 1)));
					}
				}
				else
				{
					if (arrayBuilder == null)
					{
						arrayBuilder = ArrayBuilder<DocumentationCommentTriviaSyntax>.GetInstance();
					}
					arrayBuilder.Add((DocumentationCommentTriviaSyntax)item.GetStructure());
				}
				break;
			default:
				if (arrayBuilder != null)
				{
					flag = true;
				}
				break;
			case SyntaxKind.EndOfLineTrivia:
			case SyntaxKind.WhitespaceTrivia:
				break;
			}
		}
		if (arrayBuilder == null)
		{
			return ImmutableArray<DocumentationCommentTriviaSyntax>.Empty;
		}
		arrayBuilder.ReverseContents();
		return arrayBuilder.ToImmutableAndFree();
	}
}
