using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Dynamics.Nav.Model.Tools.Cmdlets;

[Cmdlet("Join", "NAVApplicationObjectLanguageFile", SupportsShouldProcess = true, HelpUri = "http://go.microsoft.com/fwlink/?LinkID=400546")]
public sealed class JoinNavApplicationObjectLanguageFile : NavCmdlet
{
	private readonly HashSet<string> sourceFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	[Parameter(Mandatory = true, Position = 1, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
	[Alias(new string[] { "PSPath" })]
	public string[] Source { get; set; }

	[Parameter(Mandatory = false, Position = 2)]
	public string Destination { get; set; }

	[Parameter]
	public SwitchParameter Force { get; set; }

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
		LanguageTool languageTool = new LanguageTool(this, new LanguageToolOptions
		{
			OverwriteExistingFiles = Force
		});
		string destination = null;
		if (Destination != null)
		{
			destination = ResolveToFileOrDirectory(Destination);
		}
		IEnumerable<LanguageFileInfo> enumerable = languageTool.JoinApplicationObjectLanguageFile(sourceFiles.ToArray(), destination);
		if (!PassThru)
		{
			return;
		}
		List<PSObject> list = new List<PSObject>();
		foreach (LanguageFileInfo item in enumerable)
		{
			PSObject pSObject = new PSObject(item);
			pSObject.Properties.Add(new PSNoteProperty("PSPath", item.FileName));
			list.Add(pSObject);
		}
		WriteObject(list, enumerateCollection: true);
	}
}
