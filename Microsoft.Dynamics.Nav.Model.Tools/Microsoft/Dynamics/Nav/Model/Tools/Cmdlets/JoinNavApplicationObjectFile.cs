using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Dynamics.Nav.Model.Tools.Cmdlets;

[Cmdlet("Join", "NAVApplicationObjectFile", SupportsShouldProcess = true, HelpUri = "http://go.microsoft.com/fwlink/?LinkId=398883")]
public sealed class JoinNavApplicationObjectFile : NavCmdlet
{
	private readonly HashSet<string> sourceFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	[Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true, ValueFromPipeline = true)]
	[Alias(new string[] { "Result", "PSPath" })]
	public string[] Source { get; set; }

	[Parameter(Mandatory = true, Position = 2)]
	public string Destination { get; set; }

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
		SplitJoinModelTool splitJoinModelTool = new SplitJoinModelTool(this, new ReadWriteModelToolOptions
		{
			OverwriteExistingFiles = Force
		});
		string text = ResolveToFileOrDirectory(Destination);
		splitJoinModelTool.JoinApplicationObjectFiles(sourceFiles.ToArray(), text, NavCmdlet.GetPropertyName(() => Source));
		FileInfo fileInfo = new FileInfo(text);
		PSObject pSObject = new PSObject(fileInfo);
		pSObject.Properties.Add(new PSNoteProperty("PSPath", fileInfo.FullName));
		WriteObject(pSObject);
	}
}
