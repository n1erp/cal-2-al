using System.Linq;
using System.Management.Automation;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Model.Tools.Cmdlets;

[Cmdlet("Merge", "NAVApplicationObject", SupportsShouldProcess = true, HelpUri = "http://go.microsoft.com/fwlink/?LinkId=398884")]
public sealed class MergeNavApplicationObject : NavCmdlet
{
	[Parameter(Mandatory = true, Position = 1)]
	[Alias(new string[] { "Original" })]
	public string[] OriginalPath { get; set; }

	[Parameter(Mandatory = true, Position = 2)]
	[Alias(new string[] { "Modified" })]
	public string[] ModifiedPath { get; set; }

	[Parameter(Mandatory = true, Position = 3)]
	[Alias(new string[] { "Target" })]
	public string[] TargetPath { get; set; }

	[Parameter(Mandatory = true, Position = 4)]
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

	public MergeNavApplicationObject()
	{
		DateTimeProperty = DateTimePropertyAction.FromTarget;
		ModifiedProperty = ModifiedPropertyAction.FromTarget;
		VersionListProperty = VersionListPropertyAction.FromTarget;
		DocumentationConflict = DocumentationConflictAction.ModifiedFirst;
	}

	protected override void InternalProcessRecord()
	{
		MergeInfoCollection mergeInfoCollection = new ModelChangeTool(this, new ModelChangeOptions
		{
			DevBaseCompatibility = Legacy,
			InlineCodeConflicts = !Legacy,
			UseCodeCompression = false,
			OverwriteExistingFiles = Force,
			DateTimeProperty = DateTimeProperty,
			ModifiedProperty = ModifiedProperty,
			VersionListProperty = VersionListProperty,
			DocumentationConflict = DocumentationConflict,
			Strict = Strict,
			DisableCommentOut = DisableCommentOut
		}).MergeApplicationObject(ResolveToFileList(OriginalPath), ResolveToFileList(ModifiedPath), ResolveToFileList(TargetPath), ResolveToFileOrDirectory(ResultPath), NavCmdlet.GetPropertyName(() => OriginalPath), NavCmdlet.GetPropertyName(() => ModifiedPath), NavCmdlet.GetPropertyName(() => TargetPath));
		WriteObject(new PSObjectCollection(mergeInfoCollection.Select((MergeInfo i) => new PSObject(i)
		{
			Properties = { (PSPropertyInfo)new PSNoteProperty("PSPath", i.Result.FileName) }
		}), mergeInfoCollection.Summary, Host), PassThru);
	}
}
