using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

public class MarkdownHelper
{
	public static string CreateSignature(string signature, string docs = null)
	{
		if (string.IsNullOrEmpty(signature) && string.IsNullOrEmpty(docs))
		{
			return null;
		}
		return FormattableString.Invariant($"\r\n```al\r\n{signature}\r\n```\r\n\r\n{docs}");
	}

	public static string CreateBase64Image(string imageName, string base64imageValue)
	{
		if (string.IsNullOrEmpty(base64imageValue))
		{
			return null;
		}
		string text = "data:image/png;base64,";
		if (!base64imageValue.StartsWith(text, StringComparison.Ordinal))
		{
			base64imageValue = text + base64imageValue;
		}
		return FormattableString.Invariant($"![{imageName}_Image]({base64imageValue})");
	}

	public static string CreateDocumentationWithHelpLink(string documentation, string link)
	{
		if (string.IsNullOrEmpty(link))
		{
			return documentation;
		}
		return FormattableString.Invariant($"\r\n{documentation}\r\n\r\n[Get help]({link})");
	}
}
