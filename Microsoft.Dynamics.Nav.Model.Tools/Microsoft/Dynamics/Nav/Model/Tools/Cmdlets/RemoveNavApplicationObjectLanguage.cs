using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Dynamics.Nav.Model.Tools.Cmdlets;

[Cmdlet("Remove", "NAVApplicationObjectLanguage", SupportsShouldProcess = true, HelpUri = "http://go.microsoft.com/fwlink/?LinkID=400543")]
public sealed class RemoveNavApplicationObjectLanguage : NavCmdlet
{
	private readonly HashSet<string> sourceFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	internal const string DefaultDevelopmentLanguageThreeLetterName = "ENU";

	[Parameter(Mandatory = true, Position = 1, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
	[Alias(new string[] { "PSPath" })]
	public string[] Source { get; set; }

	[Parameter(Mandatory = true, Position = 2)]
	public string Destination { get; set; }

	[Parameter(Mandatory = false, Position = 3)]
	public string[] LanguageId { get; set; }

	[Parameter(Mandatory = false)]
	[PSDefaultValue(Value = "ENU")]
	public string DevelopmentLanguageId { get; set; }

	[Parameter(Mandatory = false)]
	public SwitchParameter RemoveRedundant { get; set; }

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
			OverwriteExistingFiles = Force
		});
		SortedSet<int> languageSet = null;
		if (LanguageId != null)
		{
			languageSet = LanguageTool.ParseLanguageIds(LanguageId);
		}
		IEnumerable<ApplicationObjectFileInfo> enumerable = languageTool.RemoveLanguage(sourceFiles.ToArray(), languageSet, (Destination != null) ? ResolveToFileOrDirectory(Destination) : null, LanguageTool.ParseLCID(DevelopmentLanguageId ?? "ENU"), RemoveRedundant, NavCmdlet.GetPropertyName(() => Source));
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
