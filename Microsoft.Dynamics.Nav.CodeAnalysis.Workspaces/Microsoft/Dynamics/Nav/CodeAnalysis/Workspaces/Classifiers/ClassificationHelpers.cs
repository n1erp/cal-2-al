using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Classification;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Classifiers;

internal static class ClassificationHelpers
{
	public static string GetClassification(SyntaxToken token)
	{
		if (token.Kind.IsKeyword())
		{
			return "keyword";
		}
		if (token.Kind.IsTokenIdentifier())
		{
			return "identifier";
		}
		if (token.Kind.IsNumericLiteral())
		{
			return "number";
		}
		if (token.Kind.IsStringLiteral())
		{
			return "string";
		}
		if (token.Kind.IsDateTimeLiteral())
		{
			return "date time";
		}
		if (token.Kind.IsMemberStart())
		{
			return "member start name";
		}
		if (token.Kind == SyntaxKind.DecimalLiteralToken)
		{
			return "decimal";
		}
		if (token.Kind.IsApplicationObject())
		{
			return "application object name";
		}
		return null;
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "text")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "textSpan")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "result")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cancellationToken")]
	internal static void AddLexicalClassifications(SourceText text, TextSpan textSpan, List<ClassifiedSpan> result, CancellationToken cancellationToken)
	{
	}
}
