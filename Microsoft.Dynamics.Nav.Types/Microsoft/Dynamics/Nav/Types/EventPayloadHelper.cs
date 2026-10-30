using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Types;

public static class EventPayloadHelper
{
	private const int maxEmissionsPerEvent = 3;

	public const int MessageThreshold = 25000;

	public static bool IsLargeMessage(string message)
	{
		if (message == null)
		{
			return false;
		}
		return message.Length > 25000;
	}

	public static IList<string> SplitIfLarge(string message)
	{
		if (IsLargeMessage(message))
		{
			return SplitLargeMessage(message);
		}
		return new List<string> { message };
	}

	public static IList<string> SplitLargeMessage(string message)
	{
		int num = Math.Min(3, (message.Length + 25000 - 1) / 25000);
		IList<string> list = new List<string>();
		int num2 = 0;
		for (int i = 0; i < num - 1; i++)
		{
			if (num2 >= message.Length)
			{
				break;
			}
			int num3 = Math.Min(message.Length - num2, 25000);
			list.Add(string.Format(CultureInfo.InvariantCulture, "(Message {0} of {1}) ", i + 1, num) + message.Substring(num2, num3) + "...");
			num2 += num3;
		}
		if (num == 3 && num2 + 25000 < message.Length)
		{
			list.Add(string.Format(CultureInfo.InvariantCulture, "(Message {0} of {1}) ", num, num) + TruncateIfLarge(message.Substring(num2)));
		}
		else
		{
			list.Add(string.Format(CultureInfo.InvariantCulture, "(Message {0} of {1}) ", num, num) + message.Substring(num2));
		}
		return list;
	}

	public static string TruncateIfLarge(string message)
	{
		if (!IsLargeMessage(message))
		{
			return message;
		}
		return message.Substring(0, 12500) + Environment.NewLine + "." + Environment.NewLine + "." + Environment.NewLine + "." + Environment.NewLine + message.Substring(message.Length - 12500);
	}
}
