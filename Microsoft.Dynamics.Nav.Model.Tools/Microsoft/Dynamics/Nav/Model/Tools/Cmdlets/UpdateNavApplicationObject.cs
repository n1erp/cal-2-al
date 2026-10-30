using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Model.Tools.Cmdlets;

[Cmdlet("Update", "NAVApplicationObject", SupportsShouldProcess = true, HelpUri = "http://go.microsoft.com/fwlink/?LinkId=398886")]
public sealed class UpdateNavApplicationObject : NavCmdlet
{
	private readonly HashSet<string> deltaFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	[Parameter(Mandatory = true, Position = 1)]
	[Alias(new string[] { "Target" })]
	public string[] TargetPath { get; set; }

	[Parameter(Mandatory = true, Position = 2, ValueFromPipelineByPropertyName = true)]
	[Alias(new string[] { "PSPath", "Delta" })]
	[AllowNull]
	[AllowEmptyString]
	public string[] DeltaPath { get; set; }

	[Parameter(Mandatory = true, Position = 3)]
	[Alias(new string[] { "Result" })]
	public string ResultPath { get; set; }

	[Parameter]
	public SwitchParameter PassThru { get; set; }

	[Parameter]
	[Alias(new string[] { "DevBaseCompatibility" })]
	public SwitchParameter Legacy { get; set; }

	[Parameter]
	public DateTimePropertyAction DateTimeProperty { get; set; }

	[Parameter]
	public ModifiedPropertyAction ModifiedProperty { get; set; }

	[Parameter]
	public VersionListPropertyAction VersionListProperty { get; set; }

	[Parameter]
	public DocumentationConflictAction DocumentationConflict { get; set; }

	[Parameter]
	public SwitchParameter Strict { get; set; }

	[Parameter]
	public SwitchParameter Force { get; set; }

	[Parameter]
	public SwitchParameter DisableCommentOut { get; set; }

	public UpdateNavApplicationObject()
	{
		DateTimeProperty = DateTimePropertyAction.FromTarget;
		ModifiedProperty = ModifiedPropertyAction.FromTarget;
		VersionListProperty = VersionListPropertyAction.FromTarget;
		DocumentationConflict = DocumentationConflictAction.ModifiedFirst;
	}

	protected override void InternalProcessRecord()
	{
		if (DeltaPath != null)
		{
			string[] array = ResolveToFileList(DeltaPath);
			foreach (string item in array)
			{
				deltaFiles.Add(item);
			}
		}
	}

	protected override void InternalEndProcessing()
	{
		UpdateInfoCollection updateInfoCollection = new ModelChangeTool(this, new ModelChangeOptions
		{
			DevBaseCompatibility = Legacy,
			InlineCodeConflicts = !Legacy,
			OverwriteExistingFiles = Force,
			DateTimeProperty = DateTimeProperty,
			ModifiedProperty = ModifiedProperty,
			VersionListProperty = VersionListProperty,
			DocumentationConflict = DocumentationConflict,
			Strict = Strict,
			DisableCommentOut = DisableCommentOut
		}).UpdateApplicationObject(ResolveToFileList(TargetPath), deltaFiles.ToArray(), ResolveToFileOrDirectory(ResultPath), NavCmdlet.GetPropertyName(() => TargetPath), NavCmdlet.GetPropertyName(() => DeltaPath));
		WriteObject(new PSObjectCollection(updateInfoCollection.Select((UpdateInfo i) => new PSObject(i)
		{
			Properties = { (PSPropertyInfo)new PSNoteProperty("PSPath", i.Result.FileName) }
		}), updateInfoCollection.Summary, Host), PassThru);
	}
}
