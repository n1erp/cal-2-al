namespace Microsoft.Dynamics.Nav.MetaModel;

internal interface IMergeChange
{
	MergeChangeType ChangeType { get; }

	IDiffChange[] ModifiedChanges { get; }

	IDiffChange[] TargetChanges { get; }

	IDiffChange ModifiedChange { get; }

	IDiffChange TargetChange { get; }
}
