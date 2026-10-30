using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Dynamics.Nav.Model.Tools.Cmdlets;

[Cmdlet("Get", "NAVApplicationObjectProperty", HelpUri = "http://go.microsoft.com/fwlink/?LinkId=398888")]
public sealed class GetNavApplicationObjectProperty : NavCmdlet
{
	private readonly HashSet<string> sourceFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	[Parameter(Mandatory = true, Position = 1, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
	[Alias(new string[] { "PSPath" })]
	public string[] Source { get; set; }

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
		foreach (ApplicationObjectFileInfo item in new ApplicationObjectPropertyTool(this, new ApplicationObjectPropertyToolOptions()).GetApplicationObjectProperty(sourceFiles.ToArray(), NavCmdlet.GetPropertyName(() => Source)))
		{
			PSObject pSObject = new PSObject(item);
			pSObject.Properties.Add(new PSNoteProperty("PSPath", item.FileName));
			WriteObject(pSObject);
		}
	}
}
