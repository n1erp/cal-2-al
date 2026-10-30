namespace Microsoft.Dynamics.Nav.MetaModel;

public class ComputeChangesOptions
{
	public bool UseCodeCompression { get; set; }

	public bool DevBaseCompatibility { get; set; }

	public bool IgnoreDocumentationChanges { get; set; }

	public bool ExportToNewSyntax { get; set; }

	public ComputeChangesOptions()
	{
		UseCodeCompression = true;
		DevBaseCompatibility = false;
		IgnoreDocumentationChanges = false;
	}
}
