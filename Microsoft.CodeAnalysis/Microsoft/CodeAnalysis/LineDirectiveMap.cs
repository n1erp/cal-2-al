using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis;

internal abstract class LineDirectiveMap<TDirective> where TDirective : SyntaxNode
{
	public enum PositionState : byte
	{
		Unknown,
		Unmapped,
		Remapped,
		RemappedAfterUnknown,
		RemappedAfterHidden,
		Hidden
	}

	protected struct LineMappingEntry : IComparable<LineMappingEntry>
	{
		public readonly int UnmappedLine;

		public readonly int MappedLine;

		public readonly string MappedPathOpt;

		public readonly PositionState State;

		public LineMappingEntry(int unmappedLine)
		{
			UnmappedLine = unmappedLine;
			MappedLine = unmappedLine;
			MappedPathOpt = null;
			State = PositionState.Unmapped;
		}

		public LineMappingEntry(int unmappedLine, int mappedLine, string mappedPathOpt, PositionState state)
		{
			UnmappedLine = unmappedLine;
			MappedLine = mappedLine;
			MappedPathOpt = mappedPathOpt;
			State = state;
		}

		public int CompareTo(LineMappingEntry other)
		{
			int unmappedLine = UnmappedLine;
			return unmappedLine.CompareTo(other.UnmappedLine);
		}
	}

	protected readonly LineMappingEntry[] Entries;

	protected abstract bool ShouldAddDirective(TDirective directive);

	protected abstract LineMappingEntry GetEntry(TDirective directive, SourceText sourceText, LineMappingEntry previous);

	protected abstract LineMappingEntry InitializeFirstEntry();

	protected LineDirectiveMap(SyntaxTree syntaxTree)
	{
		IList<TDirective> directives = ((SyntaxNodeOrToken)syntaxTree.GetRoot()).GetDirectives<TDirective>(ShouldAddDirective);
		Entries = CreateEntryMap(syntaxTree, directives);
	}

	public FileLinePositionSpan TranslateSpan(SourceText sourceText, string treeFilePath, TextSpan span)
	{
		LinePosition linePosition = sourceText.Lines.GetLinePosition(span.Start);
		LinePosition linePosition2 = sourceText.Lines.GetLinePosition(span.End);
		LineMappingEntry entry = FindEntry(linePosition.Line);
		return TranslateSpan(entry, treeFilePath, linePosition, linePosition2);
	}

	protected FileLinePositionSpan TranslateSpan(LineMappingEntry entry, string treeFilePath, LinePosition unmappedStartPos, LinePosition unmappedEndPos)
	{
		string path = entry.MappedPathOpt ?? treeFilePath;
		int num = unmappedStartPos.Line - entry.UnmappedLine + entry.MappedLine;
		int num2 = unmappedEndPos.Line - entry.UnmappedLine + entry.MappedLine;
		return new FileLinePositionSpan(path, new LinePositionSpan((num == -1) ? new LinePosition(unmappedStartPos.Character) : new LinePosition(num, unmappedStartPos.Character), (num2 == -1) ? new LinePosition(unmappedEndPos.Character) : new LinePosition(num2, unmappedEndPos.Character)), entry.MappedPathOpt != null);
	}

	public abstract LineVisibility GetLineVisibility(SourceText sourceText, int position);

	internal abstract FileLinePositionSpan TranslateSpanAndVisibility(SourceText sourceText, string treeFilePath, TextSpan span, out bool isHiddenPosition);

	public bool HasAnyHiddenRegions()
	{
		return Entries.Any((LineMappingEntry e) => e.State == PositionState.Hidden);
	}

	protected LineMappingEntry FindEntry(int lineNumber)
	{
		int num = FindEntryIndex(lineNumber);
		return Entries[num];
	}

	protected int FindEntryIndex(int lineNumber)
	{
		int num = Array.BinarySearch(Entries, new LineMappingEntry(lineNumber));
		if (num < 0)
		{
			return ~num - 1;
		}
		return num;
	}

	private LineMappingEntry[] CreateEntryMap(SyntaxTree tree, IList<TDirective> directives)
	{
		LineMappingEntry[] array = new LineMappingEntry[directives.Count + 1];
		LineMappingEntry lineMappingEntry = InitializeFirstEntry();
		int num = 0;
		array[num] = lineMappingEntry;
		if (directives.Count > 0)
		{
			SourceText text = tree.GetText();
			foreach (TDirective directive in directives)
			{
				lineMappingEntry = GetEntry(directive, text, lineMappingEntry);
				num++;
				array[num] = lineMappingEntry;
			}
		}
		return array;
	}
}
