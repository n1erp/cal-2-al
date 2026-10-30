using System.Diagnostics;

namespace Microsoft.CodeAnalysis;

internal abstract class WellKnownAttributeData
{
	public static readonly string StringMissingValue = "StringMissingValue";

	public WellKnownAttributeData()
	{
	}

	[Conditional("DEBUG")]
	protected void VerifySealed(bool expected = true)
	{
	}

	[Conditional("DEBUG")]
	internal void VerifyDataStored(bool expected = true)
	{
	}

	[Conditional("DEBUG")]
	protected void SetDataStored()
	{
	}

	[Conditional("DEBUG")]
	internal static void Seal(WellKnownAttributeData data)
	{
	}
}
