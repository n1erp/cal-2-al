namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal enum PathKind
{
	Empty,
	Relative,
	RelativeToCurrentDirectory,
	RelativeToCurrentParent,
	RelativeToCurrentRoot,
	RelativeToDriveDirectory,
	Absolute
}
