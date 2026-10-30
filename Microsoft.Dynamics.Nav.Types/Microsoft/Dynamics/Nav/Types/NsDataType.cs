namespace Microsoft.Dynamics.Nav.Types;

public static class NsDataType
{
	public static bool AreEqual(object value1, object value2)
	{
		if (value1 == null)
		{
			return value1 == value2;
		}
		if (value2 == null)
		{
			return value1 == value2;
		}
		if (value1.GetType().FullName != value2.GetType().FullName)
		{
			return false;
		}
		if (value1 is byte[] array)
		{
			byte[] array2 = value2 as byte[];
			if (array.Length != array2.Length)
			{
				return false;
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != array2[i])
				{
					return false;
				}
			}
			return true;
		}
		return value1.Equals(value2);
	}
}
