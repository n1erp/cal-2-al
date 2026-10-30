using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Syntax;

internal abstract class AbstractWarningStateMap
{
	protected struct WarningStateMapEntry : IComparable<WarningStateMapEntry>
	{
		public readonly int Position;

		public readonly ReportDiagnostic GeneralWarningOption;

		public readonly ImmutableDictionary<string, ReportDiagnostic> SpecificWarningOption;

		public WarningStateMapEntry(int position)
		{
			Position = position;
			GeneralWarningOption = ReportDiagnostic.Default;
			SpecificWarningOption = ImmutableDictionary.Create<string, ReportDiagnostic>();
		}

		public WarningStateMapEntry(int position, ReportDiagnostic general, ImmutableDictionary<string, ReportDiagnostic> specific)
		{
			Position = position;
			GeneralWarningOption = general;
			SpecificWarningOption = specific ?? ImmutableDictionary.Create<string, ReportDiagnostic>();
		}

		public int CompareTo(WarningStateMapEntry other)
		{
			return Position - other.Position;
		}
	}

	private readonly WarningStateMapEntry[] _warningStateMapEntries;

	protected AbstractWarningStateMap(SyntaxTree syntaxTree)
	{
		_warningStateMapEntries = CreateWarningStateMapEntries(syntaxTree);
	}

	protected abstract WarningStateMapEntry[] CreateWarningStateMapEntries(SyntaxTree syntaxTree);

	public ReportDiagnostic GetWarningState(string id, int position)
	{
		WarningStateMapEntry entryAtOrBeforePosition = GetEntryAtOrBeforePosition(position);
		if (entryAtOrBeforePosition.SpecificWarningOption.TryGetValue(id, out var value))
		{
			return value;
		}
		return entryAtOrBeforePosition.GeneralWarningOption;
	}

	private WarningStateMapEntry GetEntryAtOrBeforePosition(int position)
	{
		int num = Array.BinarySearch(_warningStateMapEntries, new WarningStateMapEntry(position));
		return _warningStateMapEntries[(num >= 0) ? num : (~num - 1)];
	}
}
