using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

internal class NavAppTranslationFileChecker : NavAppChecker
{
	private readonly NavAppLanguageFileLines languageFileLines;

	protected override NavAppFileTypes FileTypes => NavAppFileTypes.TranslationFile;

	public NavAppTranslationFileChecker(IFileSystem fileSystem)
		: base(new string[1] { ".txt" }, fileSystem)
	{
		languageFileLines = new NavAppLanguageFileLines();
	}

	protected override IEnumerable<NavAppSourceFile> FilterSourceFiles(IEnumerable<NavAppSourceFile> sources, ICollection<NavAppError> errors)
	{
		return sources.Where((NavAppSourceFile s) => !s.FileTypes.HasFlag(NavAppFileTypes.NavObjectSourceFile) && HasAnyChanceOfBeingALanguageFile(s.FileLocation, FileSystem));
	}

	protected override NavAppCheckerResult InternalVerify(NavAppCheckerResult result)
	{
		foreach (NavAppSourceFile filteredSource in FilteredSources)
		{
			byte[] bytes = FileSystem.ReadBytes(filteredSource.FileLocation);
			string[] rawLines = Encoding.UTF8.GetString(bytes).Split(Environment.NewLine.ToCharArray());
			languageFileLines.AnalyzeAndAddLines(filteredSource.FileLocation, rawLines);
		}
		foreach (NavAppLanguageFileError duplicate in languageFileLines.DuplicateLines)
		{
			FilteredSources.First((NavAppSourceFile f) => f.FileLocation.Equals(duplicate.FileName, StringComparison.OrdinalIgnoreCase)).ErrorCategories[Category].Add(string.Format(CultureInfo.CurrentCulture, NavAppCheckerStrings.DuplicateTranslationLineError, Path.GetFileName(duplicate.FileName), duplicate.LineNumber, duplicate.RawLine));
		}
		foreach (NavAppLanguageFileError invalid in languageFileLines.InvalidLines)
		{
			FilteredSources.First((NavAppSourceFile f) => f.FileLocation.Equals(invalid.FileName, StringComparison.OrdinalIgnoreCase)).ErrorCategories[Category].Add(string.Format(CultureInfo.CurrentCulture, NavAppCheckerStrings.InvalidTranslationLineError, Path.GetFileName(invalid.FileName), invalid.LineNumber, invalid.RawLine));
		}
		return result;
	}

	private static bool HasAnyChanceOfBeingALanguageFile(string path, IFileSystem fileSystem)
	{
		byte[] bytes = fileSystem.ReadBytes(path, Encoding.UTF8.GetPreamble().Length + 2);
		int offset = FileEncodingUtilities.PreambleOffset(bytes, Encoding.UTF8);
		return LanguageFormatConverter.IsTextFormat(bytes, offset);
	}
}
