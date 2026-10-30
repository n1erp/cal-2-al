using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CommandLine;

internal abstract class CommonCommandLineParser
{
	private static readonly char[] SearchPatternTrimChars = new char[8] { '\t', '\n', '\v', '\f', '\r', ' ', '\u0085', '\u00a0' };

	private static readonly char[] ResourceSeparators = new char[1] { ',' };

	private static readonly char[] PathSeparators = new char[2] { ';', ',' };

	private static readonly char[] Wildcards = new char[2] { '*', '?' };

	internal CommonMessageProvider MessageProvider { get; }

	protected abstract string RegularFileExtension { get; }

	internal CommonCommandLineParser(CommonMessageProvider messageProvider)
	{
		DebugAssertHelper.Assert(messageProvider != null);
		MessageProvider = messageProvider;
	}

	internal virtual TextReader CreateTextFileReader(string fullPath)
	{
		return new StreamReader(new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read), detectEncodingFromByteOrderMarks: true);
	}

	internal virtual IEnumerable<string> EnumerateFiles(string directory, string fileNamePattern, SearchOption searchOption)
	{
		DebugAssertHelper.Assert(PathUtilities.IsAbsolute(directory));
		return Directory.EnumerateFiles(directory, fileNamePattern, searchOption);
	}

	internal abstract CommandLineArguments CommonParse(IEnumerable<string> args, string baseDirectory, string sdkDirectoryOpt, string additionalReferenceDirectories);

	private static bool IsOption(string arg)
	{
		if (!string.IsNullOrEmpty(arg))
		{
			if (arg[0] != '/')
			{
				return arg[0] == '-';
			}
			return true;
		}
		return false;
	}

	[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
	internal static bool TryParseOption(string arg, out string name, out string value)
	{
		if (!IsOption(arg))
		{
			name = null;
			value = null;
			return false;
		}
		int num = arg.IndexOf(':');
		if (arg.Length > 1)
		{
			int num2 = arg.IndexOf('/', 1);
			if (num2 > 0 && (num < 0 || num2 < num))
			{
				name = null;
				value = null;
				return false;
			}
		}
		if (num >= 0)
		{
			name = arg.Substring(1, num - 1);
			value = arg.Substring(num + 1);
		}
		else
		{
			name = arg.Substring(1);
			value = null;
		}
		name = name.ToLowerInvariant();
		return true;
	}

	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	internal static void ParseAndNormalizeFile(string unquoted, string baseDirectory, out string outputFileName, out string outputDirectory, out string invalidPath)
	{
		outputFileName = null;
		outputDirectory = null;
		invalidPath = unquoted;
		string text = PathUtilities.ResolveRelativePath(unquoted, baseDirectory);
		if (text != null)
		{
			try
			{
				text = (invalidPath = Path.GetFullPath(text));
				outputFileName = Path.GetFileName(text);
				outputDirectory = Path.GetDirectoryName(text);
			}
			catch (Exception)
			{
				text = null;
			}
			if (outputFileName != null)
			{
				outputFileName = RemoveTrailingSpacesAndDots(outputFileName);
			}
		}
		if (text == null || !MetadataHelpers.IsValidMetadataIdentifier(outputDirectory) || !MetadataHelpers.IsValidMetadataIdentifier(outputFileName))
		{
			outputFileName = null;
		}
	}

	internal static string RemoveTrailingSpacesAndDots(string path)
	{
		if (path == null)
		{
			return path;
		}
		int length = path.Length;
		for (int num = length - 1; num >= 0; num--)
		{
			char c = path[num];
			if (!char.IsWhiteSpace(c) && c != '.')
			{
				if (num != length - 1)
				{
					return path.Substring(0, num + 1);
				}
				return path;
			}
		}
		return string.Empty;
	}

	internal void ParseOutputFile(string value, IList<Diagnostic> errors, string baseDirectory, out string outputFileName, out string outputDirectory)
	{
		ParseAndNormalizeFile(RemoveAllQuotes(value), baseDirectory, out outputFileName, out outputDirectory, out var invalidPath);
		if (outputFileName == null || !MetadataHelpers.IsValidAssemblyOrModuleName(outputFileName))
		{
			errors.Add(Diagnostic.Create(MessageProvider, 1100, invalidPath));
			outputFileName = null;
			outputDirectory = baseDirectory;
		}
	}

	internal string ParseGenericPathToFile(string unquoted, IList<Diagnostic> errors, string baseDirectory, bool generateDiagnostic = true)
	{
		string result = null;
		ParseAndNormalizeFile(unquoted, baseDirectory, out var outputFileName, out var outputDirectory, out var invalidPath);
		if (string.IsNullOrWhiteSpace(outputFileName))
		{
			if (generateDiagnostic)
			{
				errors.Add(Diagnostic.Create(MessageProvider, 1100, invalidPath));
			}
		}
		else
		{
			result = Path.Combine(outputDirectory, outputFileName);
		}
		return result;
	}

	internal void FlattenArgs(IEnumerable<string> rawArguments, IList<Diagnostic> diagnostics, List<string> processedArgs, List<string> scriptArgs, string baseDirectory, List<string> responsePaths = null)
	{
		bool flag = false;
		Stack<string> stack = new Stack<string>(rawArguments.Reverse());
		while (stack.Count > 0)
		{
			string text = stack.Pop().TrimEnd();
			if (flag)
			{
				scriptArgs.Add(text);
			}
			else if (text.StartsWith("@", StringComparison.Ordinal))
			{
				string text2 = RemoveAllQuotes(text.Substring(1)).TrimEnd(null);
				string text3 = PathUtilities.ResolveRelativePath(text2, baseDirectory);
				if (text3 != null)
				{
					foreach (string item in ParseResponseFile(text3, diagnostics).Reverse())
					{
						if (!string.Equals(item, "/noconfig", StringComparison.OrdinalIgnoreCase) && !string.Equals(item, "-noconfig", StringComparison.OrdinalIgnoreCase))
						{
							stack.Push(item);
						}
						else
						{
							diagnostics.Add(Diagnostic.Create(MessageProvider, 1000));
						}
					}
					responsePaths?.Add(PathUtilities.NormalizeAbsolutePath(text3.GetDirectoryName()));
				}
				else
				{
					diagnostics.Add(Diagnostic.Create(MessageProvider, 1100, text2));
				}
			}
			else if (text == "--" && scriptArgs != null)
			{
				flag = true;
			}
			else
			{
				processedArgs.Add(text);
			}
		}
	}

	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	internal IEnumerable<string> ParseResponseFile(string fullPath, IList<Diagnostic> errors)
	{
		List<string> list = new List<string>();
		try
		{
			DebugAssertHelper.Assert(PathUtilities.IsAbsolute(fullPath));
			using TextReader textReader = CreateTextFileReader(fullPath);
			string item;
			while ((item = textReader.ReadLine()) != null)
			{
				list.Add(item);
			}
		}
		catch (Exception)
		{
			errors.Add(Diagnostic.Create(MessageProvider, 1002, fullPath));
			return SpecializedCollections.EmptyEnumerable<string>();
		}
		return ParseResponseLines(list);
	}

	internal static IEnumerable<string> ParseResponseLines(IEnumerable<string> lines)
	{
		List<string> list = new List<string>();
		foreach (string line in lines)
		{
			list.AddRange(SplitCommandLineIntoArguments(line, removeHashComments: true));
		}
		return list;
	}

	internal static void ParseResourceDescription(string resourceDescriptor, string baseDirectory, bool skipLeadingSeparators, out string filePath, out string fullPath, out string fileName, out string resourceName, out string accessibility)
	{
		filePath = null;
		fullPath = null;
		fileName = null;
		resourceName = null;
		accessibility = null;
		string[] array = ParseSeparatedStrings(resourceDescriptor, ResourceSeparators).ToArray();
		int i = 0;
		int num = array.Length;
		if (skipLeadingSeparators)
		{
			for (; i < num && string.IsNullOrEmpty(array[i]); i++)
			{
			}
			num -= i;
		}
		if (num >= 1)
		{
			filePath = RemoveAllQuotes(array[i]);
		}
		if (num >= 2)
		{
			resourceName = RemoveAllQuotes(array[i + 1]);
		}
		if (num >= 3)
		{
			accessibility = RemoveAllQuotes(array[i + 2]);
		}
		if (!string.IsNullOrWhiteSpace(filePath))
		{
			fileName = PathUtilities.GetFileName(filePath);
			fullPath = PathUtilities.ResolveRelativePath(filePath, baseDirectory);
			if (string.IsNullOrWhiteSpace(resourceName))
			{
				resourceName = fileName;
			}
		}
	}

	internal static string RemoveAllQuotes(string arg)
	{
		return arg?.Replace("\"", string.Empty);
	}

	public static IEnumerable<string> SplitCommandLineIntoArguments(string commandLine, bool removeHashComments)
	{
		bool inQuotes = false;
		int backslashCount = 0;
		return from arg in (from arg in Split(commandLine, (char c) =>
				{
					switch (c)
					{
					case '\\':
						backslashCount++;
						break;
					case '"':
						if ((backslashCount & 1) != 1)
						{
							inQuotes = !inQuotes;
						}
						backslashCount = 0;
						break;
					default:
						backslashCount = 0;
						break;
					}
					return !inQuotes && IsCommandLineDelimiter(c);
				})
				select arg.Trim()).TakeWhile((string arg) => !removeHashComments || !arg.StartsWith("#", StringComparison.Ordinal))
			select UnquoteAndUnescape(NormalizeBackslashes(arg)) into arg
			where !string.IsNullOrEmpty(arg)
			select arg;
	}

	private static string UnquoteAndUnescape(string v)
	{
		if (v.IndexOf('"') < 0 && v.IndexOf('\\') < 0)
		{
			return v;
		}
		bool afterSingleBackslash = false;
		string[] array = Split(v, (char c) =>
		{
			if (!afterSingleBackslash && c == '"')
			{
				return true;
			}
			afterSingleBackslash = !afterSingleBackslash & (c == '\\');
			return false;
		}).ToArray();
		for (int num = 0; num < array.Length; num++)
		{
			if (array[num].IndexOf('\\') >= 0)
			{
				array[num] = array[num].Replace("\\\"", "\"");
				array[num] = array[num].Replace("\\\\", "\\");
			}
		}
		return string.Join(string.Empty, array);
	}

	private static bool IsCommandLineDelimiter(char c)
	{
		if (c != ' ' && c != '\t' && c != '\r')
		{
			return c == '\n';
		}
		return true;
	}

	private static IEnumerable<string> Split(string str, Func<char, bool> splitHere)
	{
		if (str == null)
		{
			yield break;
		}
		int num = 0;
		for (int c = 0; c < str.Length; c++)
		{
			if (splitHere(str[c]))
			{
				yield return str.Substring(num, c - num);
				num = c + 1;
			}
		}
		yield return str.Substring(num);
	}

	private static string NormalizeBackslashes(string input)
	{
		int num = input.IndexOf('\\');
		if (num < 0)
		{
			return input;
		}
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		builder.Append(input, 0, num);
		int num2 = 0;
		do
		{
			char c = input[num];
			switch (c)
			{
			case '\\':
				num2++;
				continue;
			case '"':
				AddBackslashes(builder, num2);
				break;
			default:
				AddBackslashes(builder, num2 * 2);
				break;
			}
			builder.Append(c);
			num2 = 0;
		}
		while (++num < input.Length);
		AddBackslashes(builder, num2 * 2);
		return instance.ToStringAndFree();
	}

	private static void AddBackslashes(StringBuilder builder, int count)
	{
		builder.Append('\\', count);
	}

	internal static IEnumerable<string> ParseSeparatedPaths(string str)
	{
		return ParseSeparatedStrings(str, PathSeparators, StringSplitOptions.RemoveEmptyEntries).Select(RemoveAllQuotes);
	}

	internal static IEnumerable<string> ParseSeparatedStrings(string str, char[] separators, StringSplitOptions options = StringSplitOptions.None)
	{
		bool inQuotes = false;
		IEnumerable<string> enumerable = Split(str, (char c) =>
		{
			if (c == '"')
			{
				inQuotes = !inQuotes;
			}
			return !inQuotes && separators.Contains(c);
		});
		if (options != StringSplitOptions.RemoveEmptyEntries)
		{
			return enumerable;
		}
		return enumerable.Where((string s) => s.Length > 0);
	}

	internal IEnumerable<string> ResolveRelativePaths(IEnumerable<string> paths, string baseDirectory, IList<Diagnostic> errors)
	{
		foreach (string path in paths)
		{
			string text = PathUtilities.ResolveRelativePath(path, baseDirectory);
			if (text == null)
			{
				errors.Add(Diagnostic.Create(MessageProvider, 1100, path));
			}
			else
			{
				yield return text;
			}
		}
	}

	internal IEnumerable<CommandLineSourceFile> ParseFileArgument(string arg, string baseDirectory, IList<Diagnostic> errors)
	{
		DebugAssertHelper.Assert(!arg.StartsWith("-", StringComparison.Ordinal) && !arg.StartsWith("@", StringComparison.Ordinal));
		string text = RemoveAllQuotes(arg);
		if (text.IndexOfAny(Wildcards) != -1)
		{
			foreach (CommandLineSourceFile item in ExpandFileNamePattern(text, baseDirectory, SearchOption.TopDirectoryOnly, errors))
			{
				yield return item;
			}
			yield break;
		}
		string text2 = PathUtilities.ResolveRelativePath(text, baseDirectory);
		if (text2 == null)
		{
			errors.Add(Diagnostic.Create(MessageProvider, 1100, text));
		}
		else
		{
			yield return ToCommandLineSourceFile(text2);
		}
	}

	internal IEnumerable<CommandLineSourceFile> ParseAdditionalFileArgument(string value, string baseDirectory, IList<Diagnostic> errors)
	{
		foreach (string item in from path in ParseSeparatedPaths(value)
			where !string.IsNullOrWhiteSpace(path)
			select path)
		{
			foreach (CommandLineSourceFile item2 in ParseFileArgument(item, baseDirectory, errors))
			{
				yield return item2;
			}
		}
	}

	internal IEnumerable<string> ParseAssemblyProbingPathsArgument(string value, string baseDirectory, List<Diagnostic> diagnostics)
	{
		foreach (string item in from path in ParseSeparatedPaths(value)
			where !string.IsNullOrWhiteSpace(path)
			select path)
		{
			foreach (CommandLineSourceFile item2 in ParseFileArgument(item, baseDirectory, diagnostics))
			{
				if (!Directory.Exists(item2.Path))
				{
					diagnostics.Add(Diagnostic.Create(MessageProvider, 1018, item2.Path));
				}
				yield return item2.Path;
			}
		}
	}

	internal IEnumerable<CommandLineSourceFile> ParseRecurseArgument(string arg, string baseDirectory, IList<Diagnostic> errors)
	{
		return ExpandFileNamePattern(arg, baseDirectory, SearchOption.AllDirectories, errors);
	}

	private IEnumerable<CommandLineSourceFile> ExpandFileNamePattern(string path, string baseDirectory, SearchOption searchOption, IList<Diagnostic> errors)
	{
		string directoryName = path.GetDirectoryName();
		string pattern = PathUtilities.GetFileName(path);
		string resolvedDirectoryPath = ((directoryName.Length == 0) ? baseDirectory : PathUtilities.ResolveRelativePath(directoryName, baseDirectory));
		IEnumerator<string> enumerator = null;
		try
		{
			bool yielded = false;
			pattern = pattern.Trim(SearchPatternTrimChars);
			if (!string.Equals(pattern, ".", StringComparison.Ordinal))
			{
				while (true)
				{
					string text;
					try
					{
						if (enumerator == null)
						{
							enumerator = EnumerateFiles(resolvedDirectoryPath, pattern, searchOption).GetEnumerator();
						}
						if (!enumerator.MoveNext())
						{
							break;
						}
						text = enumerator.Current;
						goto IL_00fa;
					}
					catch
					{
						text = null;
						goto IL_00fa;
					}
					IL_00fa:
					if (text != null)
					{
						text = PathUtilities.ResolveRelativePath(text, baseDirectory);
					}
					if (text == null)
					{
						errors.Add(Diagnostic.Create(MessageProvider, 1100, path));
						break;
					}
					yielded = true;
					yield return ToCommandLineSourceFile(text);
				}
			}
			if (!yielded)
			{
				if (searchOption == SearchOption.AllDirectories)
				{
					GenerateErrorForNoFilesFoundInRecurse(path, errors);
					yield break;
				}
				errors.Add(Diagnostic.Create(MessageProvider, 1001, path));
			}
		}
		finally
		{
			enumerator?.Dispose();
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "path")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "errors")]
	internal void GenerateErrorForNoFilesFoundInRecurse(string path, IList<Diagnostic> errors)
	{
	}

	private static CommandLineSourceFile ToCommandLineSourceFile(string resolvedPath)
	{
		return new CommandLineSourceFile(resolvedPath);
	}
}
