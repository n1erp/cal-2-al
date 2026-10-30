using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Translation;

internal class NewlineIterator
{
	private readonly List<int> PositionsOfNewlines;

	private int CurrentNewline;

	public int LastSearchedPosition { get; private set; }

	public int CurrentNewlinePosition
	{
		get
		{
			if (CurrentNewline >= PositionsOfNewlines.Count)
			{
				DebugAssertHelper.Fail("This should always be handled before we get to here");
				return 0;
			}
			return PositionsOfNewlines[CurrentNewline];
		}
	}

	public NewlineIterator(string file)
	{
		PositionsOfNewlines = new List<int> { 0 };
		for (int i = 0; i < file.Length; i++)
		{
			if (file[i] == '\n')
			{
				PositionsOfNewlines.Add(i);
			}
		}
		CurrentNewline = 0;
	}

	public int CalculateNewlinesUpUntil(int position)
	{
		LastSearchedPosition = position;
		while (CurrentNewline < PositionsOfNewlines.Count && CurrentNewlinePosition <= position)
		{
			CurrentNewline++;
		}
		CurrentNewline--;
		return CurrentNewline;
	}
}
