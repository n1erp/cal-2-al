using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

public class NavAppLanguageFileLines
{
	private readonly List<NavAppLanguageFileError> duplicateLines = new List<NavAppLanguageFileError>();

	private readonly List<NavAppLanguageFileError> invalidLines = new List<NavAppLanguageFileError>();

	private readonly HashSet<string> knownLanguageKeys = new HashSet<string>();

	private readonly List<NavAppLanguageFileLine> validLines = new List<NavAppLanguageFileLine>();

	public IEnumerable<NavAppLanguageFileError> InvalidLines => invalidLines;

	public IEnumerable<NavAppLanguageFileError> DuplicateLines => duplicateLines;

	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	public void AnalyzeAndAddLines(string fileName, IEnumerable<string> rawLines)
	{
		string[] array = rawLines.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			int lineNumber = i + 1;
			string rawLine = array[i];
			try
			{
				NavAppLanguageFileLine navAppLanguageFileLine = NavAppLanguageFileLine.Create(rawLine);
				if (navAppLanguageFileLine != null)
				{
					if (knownLanguageKeys.Contains(navAppLanguageFileLine.LanguageKey))
					{
						duplicateLines.Add(new NavAppLanguageFileError(fileName, lineNumber, rawLine));
						continue;
					}
					validLines.Add(navAppLanguageFileLine);
					knownLanguageKeys.Add(navAppLanguageFileLine.LanguageKey);
				}
			}
			catch (Exception)
			{
				invalidLines.Add(new NavAppLanguageFileError(fileName, lineNumber, rawLine));
			}
		}
	}
}
