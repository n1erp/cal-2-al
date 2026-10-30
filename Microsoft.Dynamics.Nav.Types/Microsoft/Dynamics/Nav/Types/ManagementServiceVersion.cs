using System.Collections;

namespace Microsoft.Dynamics.Nav.Types;

internal static class ManagementServiceVersion
{
	private static int versionNumber;

	public static int VersionNumber
	{
		get
		{
			if (versionNumber == 0)
			{
				int[] array = new int[1];
				BitArray bitArray = new BitArray(array);
				bitArray.Set(0, value: true);
				bitArray.Set(1, value: true);
				bitArray.Set(2, value: true);
				bitArray.Set(3, value: true);
				bitArray.Set(4, value: true);
				bitArray.Set(5, value: true);
				bitArray.Set(6, value: true);
				bitArray.CopyTo(array, 0);
				versionNumber = array[0];
			}
			return versionNumber;
		}
	}
}
