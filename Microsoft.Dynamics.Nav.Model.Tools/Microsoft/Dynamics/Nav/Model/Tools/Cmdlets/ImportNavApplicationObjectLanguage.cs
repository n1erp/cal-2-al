using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Dynamics.Nav.Model.Tools.Cmdlets;

[Cmdlet("Import", "NAVApplicationObjectLanguage", SupportsShouldProcess = true, HelpUri = "http://go.microsoft.com/fwlink/?LinkID=400542")]
public sealed class ImportNavApplicationObjectLanguage : NavCmdlet
{
	private readonly HashSet<string> sourceFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	[Parameter(Mandatory = true, Position = 1, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
	public string[] Source { get; set; }

	[Parameter(Mandatory = true, Position = 2, ValueFromPipeline = true)]
	[Alias(new string[] { "PSPath", "Language" })]
	public string[] LanguagePath { get; set; }

	[Parameter(Mandatory = true, Position = 3)]
	public string Destination { get; set; }

	[Parameter(Mandatory = false, Position = 4)]
	public string[] LanguageId { get; set; }

	[Parameter(Mandatory = false)]
	public FileEncoding Encoding { get; set; }

	[Parameter]
	public SwitchParameter PassThru { get; set; }

	[Parameter]
	public SwitchParameter Force { get; set; }

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
		LanguageTool languageTool = new LanguageTool(this, new LanguageToolOptions
		{
			OverwriteExistingFiles = Force,
			Encoding = Encoding
		});
		SortedSet<int> languageSet = null;
		if (LanguageId != null)
		{
			languageSet = LanguageTool.ParseLanguageIds(LanguageId);
		}
		IEnumerable<ApplicationObjectFileInfo> enumerable = languageTool.ImportLanguage(sourceFiles.ToArray(), ResolveToFileList(LanguagePath), languageSet, (Destination != null) ? ResolveToFileOrDirectory(Destination) : null, NavCmdlet.GetPropertyName(() => Source));
		if (!PassThru)
		{
			return;
		}
		foreach (ApplicationObjectFileInfo item in enumerable)
		{
			PSObject pSObject = new PSObject(item);
			pSObject.Properties.Add(new PSNoteProperty("PSPath", item.FileName));
			WriteObject(pSObject);
		}
	}
}
