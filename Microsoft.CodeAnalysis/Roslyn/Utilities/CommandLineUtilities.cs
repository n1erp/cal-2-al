using System.Collections.Generic;
using System.Text;

namespace Roslyn.Utilities;

internal static class CommandLineUtilities
{
	public static IEnumerable<string> SplitCommandLineIntoArguments(string commandLine, bool removeHashComments)
	{
		char? illegalChar;
		return SplitCommandLineIntoArguments(commandLine, removeHashComments, out illegalChar);
	}

	public static IEnumerable<string> SplitCommandLineIntoArguments(string commandLine, bool removeHashComments, out char? illegalChar)
	{
		StringBuilder stringBuilder = new StringBuilder(commandLine.Length);
		List<string> list = new List<string>();
		int i = 0;
		illegalChar = null;
		while (i < commandLine.Length)
		{
			for (; i < commandLine.Length && char.IsWhiteSpace(commandLine[i]); i++)
			{
			}
			if (i == commandLine.Length || ((commandLine[i] == '#') & removeHashComments))
			{
				break;
			}
			int num = 0;
			stringBuilder.Length = 0;
			while (i < commandLine.Length && (!char.IsWhiteSpace(commandLine[i]) || num % 2 != 0))
			{
				char c = commandLine[i];
				if (c != '"')
				{
					if (c == '\\')
					{
						int num2 = 0;
						do
						{
							stringBuilder.Append(commandLine[i]);
							i++;
							num2++;
						}
						while (i < commandLine.Length && commandLine[i] == '\\');
						if (i < commandLine.Length && commandLine[i] == '"')
						{
							if (num2 % 2 == 0)
							{
								num++;
							}
							stringBuilder.Append('"');
							i++;
						}
						continue;
					}
					if ((c >= '\u0001' && c <= '\u001f') || c == '|')
					{
						if (!illegalChar.HasValue)
						{
							illegalChar = c;
						}
					}
					else
					{
						stringBuilder.Append(c);
					}
					i++;
				}
				else
				{
					stringBuilder.Append(c);
					num++;
					i++;
				}
			}
			if (num == 2 && stringBuilder[0] == '"' && stringBuilder[stringBuilder.Length - 1] == '"')
			{
				stringBuilder.Remove(0, 1);
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			if (stringBuilder.Length > 0)
			{
				list.Add(stringBuilder.ToString());
			}
		}
		return list;
	}
}
