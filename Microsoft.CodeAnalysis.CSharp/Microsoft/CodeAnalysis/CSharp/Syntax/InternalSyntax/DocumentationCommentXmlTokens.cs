using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal static class DocumentationCommentXmlTokens
{
	private static readonly SyntaxToken s_seeToken = Identifier("see");

	private static readonly SyntaxToken s_codeToken = Identifier("code");

	private static readonly SyntaxToken s_listToken = Identifier("list");

	private static readonly SyntaxToken s_paramToken = Identifier("param");

	private static readonly SyntaxToken s_valueToken = Identifier("value");

	private static readonly SyntaxToken s_exampleToken = Identifier("example");

	private static readonly SyntaxToken s_includeToken = Identifier("include");

	private static readonly SyntaxToken s_remarksToken = Identifier("remarks");

	private static readonly SyntaxToken s_seealsoToken = Identifier("seealso");

	private static readonly SyntaxToken s_summaryToken = Identifier("summary");

	private static readonly SyntaxToken s_exceptionToken = Identifier("exception");

	private static readonly SyntaxToken s_typeparamToken = Identifier("typeparam");

	private static readonly SyntaxToken s_permissionToken = Identifier("permission");

	private static readonly SyntaxToken s_typeparamrefToken = Identifier("typeparamref");

	private static readonly SyntaxToken s_crefToken = IdentifierWithLeadingSpace("cref");

	private static readonly SyntaxToken s_fileToken = IdentifierWithLeadingSpace("file");

	private static readonly SyntaxToken s_nameToken = IdentifierWithLeadingSpace("name");

	private static readonly SyntaxToken s_pathToken = IdentifierWithLeadingSpace("path");

	private static readonly SyntaxToken s_typeToken = IdentifierWithLeadingSpace("type");

	private static SyntaxToken Identifier(string text)
	{
		return SyntaxFactory.Identifier(SyntaxKind.None, null, text, text, null);
	}

	private static SyntaxToken IdentifierWithLeadingSpace(string text)
	{
		return SyntaxFactory.Identifier(SyntaxKind.None, SyntaxFactory.Space, text, text, null);
	}

	private static bool IsSingleSpaceTrivia(SyntaxListBuilder syntax)
	{
		if (syntax.Count == 1)
		{
			return SyntaxFactory.Space.IsEquivalentTo(syntax[0]);
		}
		return false;
	}

	public static SyntaxToken LookupToken(string text, SyntaxListBuilder leading)
	{
		if (leading == null)
		{
			return LookupXmlElementTag(text);
		}
		if (IsSingleSpaceTrivia(leading))
		{
			return LookupXmlAttribute(text);
		}
		return null;
	}

	private static SyntaxToken LookupXmlElementTag(string text)
	{
		switch (text.Length)
		{
		case 3:
			if (text == "see")
			{
				return s_seeToken;
			}
			break;
		case 4:
			if (!(text == "code"))
			{
				if (!(text == "list"))
				{
					break;
				}
				return s_listToken;
			}
			return s_codeToken;
		case 5:
			if (!(text == "param"))
			{
				if (!(text == "value"))
				{
					break;
				}
				return s_valueToken;
			}
			return s_paramToken;
		case 7:
			switch (text)
			{
			case "example":
				return s_exampleToken;
			case "include":
				return s_includeToken;
			case "remarks":
				return s_remarksToken;
			case "seealso":
				return s_seealsoToken;
			case "summary":
				return s_summaryToken;
			}
			break;
		case 9:
			if (!(text == "exception"))
			{
				if (!(text == "typeparam"))
				{
					break;
				}
				return s_typeparamToken;
			}
			return s_exceptionToken;
		case 10:
			if (text == "permission")
			{
				return s_permissionToken;
			}
			break;
		case 12:
			if (text == "typeparam")
			{
				return s_typeparamrefToken;
			}
			break;
		}
		return null;
	}

	private static SyntaxToken LookupXmlAttribute(string text)
	{
		if (text.Length != 4)
		{
			return null;
		}
		return text switch
		{
			"cref" => s_crefToken, 
			"file" => s_fileToken, 
			"name" => s_nameToken, 
			"path" => s_pathToken, 
			"type" => s_typeToken, 
			_ => null, 
		};
	}
}
