namespace Microsoft.Dynamics.Nav.Model.Compiler;

public static class TextUtilities
{
	public static string QuoteIdentifierIfNeeded(string value)
	{
		if (!NeedsIdentifierQuotes(value))
		{
			return value;
		}
		return "\"" + value.Replace("\"", "\"\"") + "\"";
	}

	public static bool NeedsIdentifierQuotes(string value)
	{
		if (value[0] != '_' && (value[0] < 'a' || value[0] > 'z') && (value[0] < 'A' || value[0] > 'Z'))
		{
			return true;
		}
		for (int num = value.Length - 1; num > 0; num--)
		{
			if (value[num] != '_' && (value[num] < 'a' || value[num] > 'z') && (value[num] < 'A' || value[num] > 'Z') && !char.IsDigit(value[num]))
			{
				return true;
			}
		}
		return false;
	}
}
