using System.Collections.Immutable;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Syntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

internal class CSharpPragmaWarningStateMap : AbstractWarningStateMap
{
	public CSharpPragmaWarningStateMap(SyntaxTree syntaxTree)
		: base(syntaxTree)
	{
	}

	protected override WarningStateMapEntry[] CreateWarningStateMapEntries(SyntaxTree syntaxTree)
	{
		ArrayBuilder<PragmaWarningDirectiveTriviaSyntax> instance = ArrayBuilder<PragmaWarningDirectiveTriviaSyntax>.GetInstance();
		GetAllPragmaWarningDirectives(syntaxTree, instance);
		return CreatePragmaWarningStateEntries(instance.ToImmutableAndFree());
	}

	private static void GetAllPragmaWarningDirectives(SyntaxTree syntaxTree, ArrayBuilder<PragmaWarningDirectiveTriviaSyntax> directiveList)
	{
		foreach (DirectiveTriviaSyntax directive in syntaxTree.GetRoot().GetDirectives())
		{
			if (directive.Kind() == SyntaxKind.PragmaWarningDirectiveTrivia)
			{
				PragmaWarningDirectiveTriviaSyntax pragmaWarningDirectiveTriviaSyntax = (PragmaWarningDirectiveTriviaSyntax)directive;
				if (!pragmaWarningDirectiveTriviaSyntax.DisableOrRestoreKeyword.IsMissing && !pragmaWarningDirectiveTriviaSyntax.WarningKeyword.IsMissing && pragmaWarningDirectiveTriviaSyntax.IsActive)
				{
					directiveList.Add(pragmaWarningDirectiveTriviaSyntax);
				}
			}
		}
	}

	private static WarningStateMapEntry[] CreatePragmaWarningStateEntries(ImmutableArray<PragmaWarningDirectiveTriviaSyntax> directiveList)
	{
		WarningStateMapEntry[] array = new WarningStateMapEntry[directiveList.Length + 1];
		WarningStateMapEntry warningStateMapEntry = new WarningStateMapEntry(0, ReportDiagnostic.Default, null);
		int num = 0;
		array[num] = warningStateMapEntry;
		ReportDiagnostic general = ReportDiagnostic.Default;
		ImmutableDictionary<string, ReportDiagnostic> immutableDictionary = ImmutableDictionary.Create<string, ReportDiagnostic>();
		while (num < directiveList.Length)
		{
			PragmaWarningDirectiveTriviaSyntax pragmaWarningDirectiveTriviaSyntax = directiveList[num];
			ReportDiagnostic reportDiagnostic = ((pragmaWarningDirectiveTriviaSyntax.DisableOrRestoreKeyword.Kind() == SyntaxKind.DisableKeyword) ? ReportDiagnostic.Suppress : ReportDiagnostic.Default);
			if (pragmaWarningDirectiveTriviaSyntax.ErrorCodes.Count == 0)
			{
				general = reportDiagnostic;
				immutableDictionary = ImmutableDictionary.Create<string, ReportDiagnostic>();
			}
			else
			{
				for (int i = 0; i < pragmaWarningDirectiveTriviaSyntax.ErrorCodes.Count; i++)
				{
					ExpressionSyntax expressionSyntax = pragmaWarningDirectiveTriviaSyntax.ErrorCodes[i];
					if (!expressionSyntax.IsMissing && !expressionSyntax.ContainsDiagnostics)
					{
						string text = string.Empty;
						if (expressionSyntax.Kind() == SyntaxKind.NumericLiteralExpression)
						{
							SyntaxToken token = ((LiteralExpressionSyntax)expressionSyntax).Token;
							text = MessageProvider.Instance.GetIdForErrorCode((int)token.Value);
						}
						else if (expressionSyntax.Kind() == SyntaxKind.IdentifierName)
						{
							text = ((IdentifierNameSyntax)expressionSyntax).Identifier.ValueText;
						}
						if (!string.IsNullOrWhiteSpace(text))
						{
							immutableDictionary = immutableDictionary.SetItem(text, reportDiagnostic);
						}
					}
				}
			}
			warningStateMapEntry = new WarningStateMapEntry(pragmaWarningDirectiveTriviaSyntax.Location.SourceSpan.End, general, immutableDictionary);
			num++;
			array[num] = warningStateMapEntry;
		}
		return array;
	}
}
