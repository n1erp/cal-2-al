namespace Microsoft.Dynamics.Nav.MetaModel;

internal class MergeOptions
{
	public bool AdjacentChangesConflict { get; set; }

	public MergeOptions()
	{
		AdjacentChangesConflict = true;
	}
}
