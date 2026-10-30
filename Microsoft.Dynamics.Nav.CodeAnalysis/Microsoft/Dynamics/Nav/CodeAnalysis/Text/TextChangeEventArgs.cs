using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Text;

public class TextChangeEventArgs : EventArgs
{
	public SourceText OldText { get; private set; }

	public SourceText NewText { get; private set; }

	public IReadOnlyList<TextChangeRange> Changes { get; private set; }

	public TextChangeEventArgs(SourceText oldText, SourceText newText, IEnumerable<TextChangeRange> changes)
	{
		if (changes == null || changes.IsEmpty())
		{
			throw new ArgumentException(CodeAnalysisResources.ArgumentCannotBeEmpty, "changes");
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
