using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Model.Tools;

public class LanguageFileInfo
{
	public string FileName { get; private set; }

	public IReadOnlyCollection<ApplicationObjectLanguageInfo> Languages { get; private set; }

	public IEnumerable<TranslateLine> TranslateLines => Languages.SelectMany((ApplicationObjectLanguageInfo l) => l.TranslateLines);

	internal LanguageFileInfo(string fileName, IEnumerable<ApplicationObjectLanguageInfo> languages)
	{
		Languages = new ReadOnlyCollection<ApplicationObjectLanguageInfo>(languages.ToList());
		FileName = fileName;
	}

	internal LanguageFileInfo(string fileName, ApplicationObjectLanguageInfo language)
	{
		Languages = new ReadOnlyCollection<ApplicationObjectLanguageInfo>(new ApplicationObjectLanguageInfo[1] { language }.ToList());
		FileName = fileName;
	}

	public override string ToString()
	{
		return FileName;
	}
}
