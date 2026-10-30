namespace Roslyn.Utilities;

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
