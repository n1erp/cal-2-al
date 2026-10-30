using System.Collections.Generic;
using System.IO;
using System.Management.Automation;

namespace Microsoft.Dynamics.Nav.Model.Tools.Cmdlets;

[Cmdlet("Split", "NAVApplicationObjectFile", SupportsShouldProcess = true, HelpUri = "http://go.microsoft.com/fwlink/?LinkId=398885")]
public sealed class SplitNavApplicationObjectFile : NavCmdlet
{
	[Parameter(Mandatory = true, Position = 1, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
	[Alias(new string[] { "PSPath" })]
	public string Source { get; set; }

	[Parameter(Mandatory = false, Position = 2)]
	public string Destination { get; set; }

	[Parameter]
	public SwitchParameter PassThru { get; set; }

	[Parameter]
	public SwitchParameter PreserveFormatting { get; set; }

	[Parameter]
	public SwitchParameter Force { get; set; }

	protected override void InternalProcessRecord()
	{
		SplitJoinModelTool splitJoinModelTool = new SplitJoinModelTool(this, new ReadWriteModelToolOptions
		{
			OverwriteExistingFiles = Force
		});
		string text = ((Destination == null) ? SessionState.Path.CurrentFileSystemLocation.Path : ResolveToFileOrDirectory(Destination));
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		IEnumerable<FileInfo> enumerable = ((!PreserveFormatting) ? splitJoinModelTool.SplitApplicationObjectFile(ResolveToFileList(Source), text, NavCmdlet.GetPropertyName(() => Source)) : splitJoinModelTool.SplitApplicationObjectFileNoParsing(ResolveToFileList(Source), text));
		if (!PassThru)
		{
			return;
		}
		foreach (FileInfo item in enumerable)
		{
			PSObject pSObject = new PSObject(item);
			pSObject.Properties.Add(new PSNoteProperty("PSPath", item.FullName));
			WriteObject(pSObject);
		}
	}
}
