using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.Dynamics.Nav.Model.Tools.Language;

namespace Microsoft.Dynamics.Nav.Model.Tools.Cmdlets;

[Cmdlet("Test", "NAVApplicationObjectLanguage", HelpUri = "http://go.microsoft.com/fwlink/?LinkID=400544")]
public sealed class TestNavApplicationObjectLanguage : NavCmdlet
{
	private readonly HashSet<string> sourceFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	[Parameter(Mandatory = true, Position = 1, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
	[Alias(new string[] { "PSPath" })]
	public string[] Source { get; set; }

	[Parameter(Mandatory = true, Position = 2)]
	public string[] LanguageId { get; set; }

	[Parameter]
	public SwitchParameter PassThru { get; set; }

	protected override void InternalProcessRecord()
	{
		if (Source != null)
		{
			string[] array = ResolveToFileList(Source);
			foreach (string item in array)
			{
				sourceFiles.Add(item);
			}
		}
	}

	protected override void InternalEndProcessing()
	{
		LanguageTool languageTool = new LanguageTool(this, new LanguageToolOptions());
		SortedSet<int> languageSet = null;
		if (LanguageId != null)
		{
			languageSet = LanguageTool.ParseLanguageIds(LanguageId);
		}
		IEnumerable<ApplicationObjectLanguageInfo> enumerable = languageTool.TestLanguage(sourceFiles.ToArray(), languageSet, NavCmdlet.GetPropertyName(() => Source));
		if (enumerable.Any())
		{
			string text = string.Format(CultureInfo.InvariantCulture, NonLocalizable.MissingTranslationsFound, enumerable.SelectMany((ApplicationObjectLanguageInfo l) => l.TranslateLines).Count());
			if (!PassThru)
			{
				throw new ModelToolException(text);
			}
			WriteObject(enumerable, enumerateCollection: true);
			WriteWarning(text);
		}
	}
}
