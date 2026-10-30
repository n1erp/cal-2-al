using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Dynamics.Nav.Model.Tools.Cmdlets;

[Cmdlet("Set", "NAVApplicationObjectProperty", SupportsShouldProcess = true, HelpUri = "http://go.microsoft.com/fwlink/?LinkId=398889")]
public sealed class SetNavApplicationObjectProperty : NavCmdlet
{
	private readonly HashSet<string> targetFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	[Parameter(Mandatory = true, Position = 1, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
	[Alias(new string[] { "PSPath", "Target" })]
	public string[] TargetPath { get; set; }

	[Parameter(ValueFromPipelineByPropertyName = true)]
	public string VersionListProperty { get; set; }

	[Parameter(ValueFromPipelineByPropertyName = true)]
	public SetModifiedPropertyAction? ModifiedProperty { get; set; }

	[Parameter(ValueFromPipelineByPropertyName = true)]
	public string DateTimeProperty { get; set; }

	[Parameter]
	public SwitchParameter PassThru { get; set; }

	protected override void InternalProcessRecord()
	{
		if (TargetPath != null)
		{
			string[] array = ResolveToFileList(TargetPath);
			foreach (string item in array)
			{
				targetFiles.Add(item);
			}
		}
	}

	protected override void InternalEndProcessing()
	{
		ApplicationObjectFileInfoCollection applicationObjectFileInfoCollection = new ApplicationObjectPropertyTool(this, new ApplicationObjectPropertyToolOptions
		{
			OverwriteExistingFiles = true,
			VersionListProperty = VersionListProperty,
			ModifiedProperty = ModifiedProperty,
			DateTimeProperty = DateTimeProperty
		}).SetApplicationObjectProperty(targetFiles.ToArray(), NavCmdlet.GetPropertyName(() => TargetPath));
		if (!PassThru)
		{
			return;
		}
		foreach (ApplicationObjectFileInfo item in applicationObjectFileInfoCollection)
		{
			PSObject pSObject = new PSObject(item);
			pSObject.Properties.Add(new PSNoteProperty("PSPath", item.FileName));
			WriteObject(pSObject);
		}
	}
}
