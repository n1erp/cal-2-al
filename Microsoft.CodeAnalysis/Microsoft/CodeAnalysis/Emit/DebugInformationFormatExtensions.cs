namespace Microsoft.CodeAnalysis.Emit;

internal static class DebugInformationFormatExtensions
{
	internal static bool IsValid(this DebugInformationFormat value)
	{
		if (value >= DebugInformationFormat.Pdb)
		{
			return value <= DebugInformationFormat.Embedded;
		}
		return false;
	}

	internal static bool IsPortable(this DebugInformationFormat value)
	{
		if (value != DebugInformationFormat.PortablePdb)
		{
			return value == DebugInformationFormat.Embedded;
		}
		return true;
	}
}
