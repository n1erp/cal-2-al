namespace Microsoft.Dynamics.Nav.Runtime;

public static class SignatureComparison
{
	public static bool AreStringsIdentical(string correctValue, string userOrAttackerProvidedValue)
	{
		if (string.IsNullOrEmpty(correctValue) || correctValue.Length != userOrAttackerProvidedValue.Length)
		{
			return false;
		}
		bool result = true;
		for (int i = 0; i < correctValue.Length; i++)
		{
			if (correctValue[i] != userOrAttackerProvidedValue[i])
			{
				result = false;
			}
		}
		return result;
	}
}
