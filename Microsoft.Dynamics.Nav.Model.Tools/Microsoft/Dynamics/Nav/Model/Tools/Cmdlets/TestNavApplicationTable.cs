using System.Management.Automation;

namespace Microsoft.Dynamics.Nav.Model.Tools.Cmdlets;

[Cmdlet("Test", "NAVApplicationTable", SupportsShouldProcess = true, HelpUri = "https://go.microsoft.com/fwlink/?linkid=866202")]
public sealed class TestNavApplicationTable : NavCmdlet
{
	[Parameter(Mandatory = true, Position = 1)]
	public string OriginalPath { get; set; }

	[Parameter(Mandatory = true, Position = 2)]
	public string ModifiedPath { get; set; }

	protected override void InternalProcessRecord()
	{
		string[] original = ResolveToFileList(OriginalPath, allDirectories: true);
		string[] modified = ResolveToFileList(ModifiedPath, allDirectories: true);
		SchemaGatingTool schemaGatingTool = new SchemaGatingTool(this);
		if (!schemaGatingTool.ValidateApplicationTableChange(original, modified))
		{
			foreach (string disallowedChange in schemaGatingTool.DisallowedChanges)
			{
				WriteWarning(disallowedChange);
			}
			throw new ModelToolException("Changes in application table(s) are not allowed.");
		}
		WriteObject("Changes in application table(s) are allowed.");
	}
}
