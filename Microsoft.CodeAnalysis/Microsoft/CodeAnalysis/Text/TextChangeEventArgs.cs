using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Text;

public class TextChangeEventArgs : EventArgs
{
	public SourceText OldText { get; }

	public SourceText NewText { get; }

	public IReadOnlyList<TextChangeRange> Changes { get; }

	public TextChangeEventArgs(SourceText oldText, SourceText newText, IEnumerable<TextChangeRange> changes)
	{
		if (changes == null)
		{
			throw new ArgumentException("changes");
		}
		OldText = oldText;
		NewText = newText;
		Changes = changes.ToImmutableArray();
	}

	public TextChangeEventArgs(SourceText oldText, SourceText newText, params TextChangeRange[] changes)
		: this(oldText, newText, (IEnumerable<TextChangeRange>)changes)
	{
	}
}
