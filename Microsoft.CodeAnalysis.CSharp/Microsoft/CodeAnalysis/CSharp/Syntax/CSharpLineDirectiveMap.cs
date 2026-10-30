using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

internal class CSharpLineDirectiveMap : LineDirectiveMap<DirectiveTriviaSyntax>
{
	public CSharpLineDirectiveMap(SyntaxTree syntaxTree)
		: base(syntaxTree)
	{
	}

	protected override bool ShouldAddDirective(DirectiveTriviaSyntax directive)
	{
		if (directive.IsActive)
		{
			return directive.Kind() == SyntaxKind.LineDirectiveTrivia;
		}
		return false;
	}

	protected override LineMappingEntry GetEntry(DirectiveTriviaSyntax directiveNode, SourceText sourceText, LineMappingEntry previous)
	{
		LineDirectiveTriviaSyntax lineDirectiveTriviaSyntax = (LineDirectiveTriviaSyntax)directiveNode;
		int num = sourceText.Lines.IndexOf(lineDirectiveTriviaSyntax.SpanStart) + 1;
		int num2 = num;
		int mappedLine = previous.MappedLine + num - previous.UnmappedLine;
		string mappedPathOpt = previous.MappedPathOpt;
		PositionState state = PositionState.Unmapped;
		SyntaxToken line = lineDirectiveTriviaSyntax.Line;
		if (!line.IsMissing)
		{
			switch (line.Kind())
			{
			case SyntaxKind.HiddenKeyword:
				state = PositionState.Hidden;
				break;
			case SyntaxKind.DefaultKeyword:
				mappedLine = num2;
				mappedPathOpt = null;
				state = PositionState.Unmapped;
				break;
			case SyntaxKind.NumericLiteralToken:
				if (!line.ContainsDiagnostics)
				{
					object value = line.Value;
					if (value is int)
					{
						mappedLine = (int)value - 1;
					}
					if (lineDirectiveTriviaSyntax.File.Kind() == SyntaxKind.StringLiteralToken)
					{
						mappedPathOpt = (string)lineDirectiveTriviaSyntax.File.Value;
					}
					state = PositionState.Remapped;
				}
				break;
			}
		}
		return new LineMappingEntry(num2, mappedLine, mappedPathOpt, state);
	}

	protected override LineMappingEntry InitializeFirstEntry()
	{
		return new LineMappingEntry(0, 0, (string)null, PositionState.Unmapped);
	}

	public override LineVisibility GetLineVisibility(SourceText sourceText, int position)
	{
		LinePosition linePosition = sourceText.Lines.GetLinePosition(position);
		if (Entries.Length == 1)
		{
			return LineVisibility.Visible;
		}
		int num = FindEntryIndex(linePosition.Line);
		LineMappingEntry lineMappingEntry = Entries[num];
		switch (lineMappingEntry.State)
		{
		case PositionState.Unmapped:
			if (num == 0)
			{
				return LineVisibility.BeforeFirstLineDirective;
			}
			return LineVisibility.Visible;
		case PositionState.Remapped:
			return LineVisibility.Visible;
		case PositionState.Hidden:
			return LineVisibility.Hidden;
		default:
			throw ExceptionUtilities.UnexpectedValue(lineMappingEntry.State);
		}
	}

	internal override FileLinePositionSpan TranslateSpanAndVisibility(SourceText sourceText, string treeFilePath, TextSpan span, out bool isHiddenPosition)
	{
		TextLineCollection lines = sourceText.Lines;
		LinePosition linePosition = lines.GetLinePosition(span.Start);
		LinePosition linePosition2 = lines.GetLinePosition(span.End);
		if (Entries.Length == 1)
		{
			isHiddenPosition = false;
			return new FileLinePositionSpan(treeFilePath, linePosition, linePosition2);
		}
		LineMappingEntry entry = FindEntry(linePosition.Line);
		isHiddenPosition = (uint)entry.State == 5u;
		return TranslateSpan(entry, treeFilePath, linePosition, linePosition2);
	}
}
