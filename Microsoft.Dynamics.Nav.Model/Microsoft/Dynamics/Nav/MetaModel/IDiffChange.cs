namespace Microsoft.Dynamics.Nav.MetaModel;

internal interface IDiffChange
{
	DiffChangeType ChangeType { get; }

	int OriginalStart { get; }

	int OriginalLength { get; }

	int OriginalEnd { get; }

	int ModifiedStart { get; }

	int ModifiedLength { get; }

	int ModifiedEnd { get; }

	IDiffChange Add(IDiffChange diffChange);
}
