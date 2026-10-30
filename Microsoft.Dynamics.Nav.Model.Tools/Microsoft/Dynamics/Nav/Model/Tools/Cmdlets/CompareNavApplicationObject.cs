using System.Linq;
using System.Management.Automation;

namespace Microsoft.Dynamics.Nav.Model.Tools.Cmdlets;

[Cmdlet("Compare", "NAVApplicationObject", SupportsShouldProcess = true, HelpUri = "http://go.microsoft.com/fwlink/?LinkId=398882")]
public sealed class CompareNavApplicationObject : NavCmdlet
{
	[Parameter(Mandatory = true, Position = 1)]
	[Alias(new string[] { "Original" })]
	public string[] OriginalPath { get; set; }

	[Parameter(Mandatory = true, Position = 2)]
	[Alias(new string[] { "Modified" })]
	public string[] ModifiedPath { get; set; }

	[Parameter(Mandatory = true, Position = 3)]
	[Alias(new string[] { "Delta" })]
	public string DeltaPath { get; set; }

	[Parameter]
	public SwitchParameter PassThru { get; set; }

	[Parameter]
	[Alias(new string[] { "DevBaseCompatibility" })]
	public SwitchParameter Legacy { get; set; }

	[Parameter]
	public SwitchParameter NoCodeCompression { get; set; }

	[Parameter]
	public SwitchParameter IgnoreDocumentation { get; set; }

	[Parameter]
	public SwitchParameter Force { get; set; }

	[Parameter]
	public SwitchParameter ExportToNewSyntax { get; set; }

	protected override void InternalProcessRecord()
	{
		CompareInfoCollection compareInfoCollection = new ModelChangeTool(this, new ModelChangeOptions
		{
			DevBaseCompatibility = Legacy,
			UseCodeCompression = !NoCodeCompression,
			OverwriteExistingFiles = Force,
			IgnoreDocumentation = IgnoreDocumentation,
			ExportToNewSyntax = ExportToNewSyntax
		}).CompareApplicationObject(ResolveToFileList(OriginalPath), ResolveToFileList(ModifiedPath), ResolveToFileOrDirectory(DeltaPath), NavCmdlet.GetPropertyName(() => OriginalPath), NavCmdlet.GetPropertyName(() => ModifiedPath));
		WriteObject(new PSObjectCollection(compareInfoCollection.Select((CompareInfo i) => new PSObject(i)
		{
			Properties = { (PSPropertyInfo)new PSNoteProperty("PSPath", i.Delta.FileName) }
		}), compareInfoCollection.Summary, Host), PassThru);
	}
}
