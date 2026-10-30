using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public abstract class CommandLineParser
{
	private readonly CommonMessageProvider _messageProvider;

	internal readonly bool IsScriptCommandLineParser;

	private static readonly char[] s_searchPatternTrimChars = new char[8] { '\t', '\n', '\v', '\f', '\r', ' ', '\u0085', '\u00a0' };

	private static readonly char[] s_resourceSeparators = new char[1] { ',' };

	private static readonly char[] s_pathSeparators = new char[2] { ';', ',' };

	private static readonly char[] s_wildcards = new char[2] { '*', '?' };

	internal CommonMessageProvider MessageProvider => _messageProvider;

	protected abstract string RegularFileExtension { get; }

	protected abstract string ScriptFileExtension { get; }

	internal static string MismatchedVersionErrorText => CodeAnalysisResources.MismatchedVersion;

	internal CommandLineParser(CommonMessageProvider messageProvider, bool isScriptCommandLineParser)
	{
		_messageProvider = messageProvider;
		IsScriptCommandLineParser = isScriptCommandLineParser;
	}

	internal virtual TextReader CreateTextFileReader(string fullPath)
	{
		return new StreamReader(new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read), detectEncodingFromByteOrderMarks: true);
	}

	internal virtual IEnumerable<string> EnumerateFiles(string directory, string fileNamePattern, SearchOption searchOption)
	{
		return Directory.EnumerateFiles(directory, fileNamePattern, searchOption);
	}

	internal abstract CommandLineArguments CommonParse(IEnumerable<string> args, string baseDirectory, string sdkDirectoryOpt, string additionalReferenceDirectories);

	public CommandLineArguments Parse(IEnumerable<string> args, string baseDirectory, string sdkDirectory, string additionalReferenceDirectories)
	{
		return CommonParse(args, baseDirectory, sdkDirectory, additionalReferenceDirectories);
	}

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

	internal static bool TryParseOption(string arg, out string name, out string value)
	{
		if (!IsOption(arg))
		{
			name = null;
			value = null;
			return false;
		}
		int num = arg.IndexOf(':');
		if (arg.Length > 1 && arg[0] != '-')
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

	internal static void ParseAndNormalizeFile(string unquoted, string baseDirectory, out string outputFileName, out string outputDirectory, out string invalidPath)
	{
		outputFileName = null;
		outputDirectory = null;
		invalidPath = unquoted;
		string text = FileUtilities.ResolveRelativePath(unquoted, baseDirectory);
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

	protected ImmutableArray<KeyValuePair<string, string>> ParsePathMap(string pathMap, IList<Diagnostic> errors)
	{
		ArrayBuilder<KeyValuePair<string, string>> instance = ArrayBuilder<KeyValuePair<string, string>>.GetInstance();
		if (pathMap.IsEmpty())
		{
			return instance.ToImmutableAndFree();
		}
		string[] array = pathMap.Split(new char[1] { ',' });
		foreach (string text in array)
		{
			string[] array2 = text.Split(new char[1] { '=' });
			if (array2.Length != 2)
			{
				errors.Add(Diagnostic.Create(_messageProvider, _messageProvider.ERR_InvalidPathMap, text));
				continue;
			}
			string text2 = array2[0];
			string text3 = array2[1];
			if (text2.Length == 0 || text3.Length == 0)
			{
				errors.Add(Diagnostic.Create(_messageProvider, _messageProvider.ERR_InvalidPathMap, text));
			}
			else
			{
				text2 = PathUtilities.EnsureTrailingSeparator(text2);
				text3 = PathUtilities.EnsureTrailingSeparator(text3);
				instance.Add(new KeyValuePair<string, string>(text2, text3));
			}
		}
		return instance.ToImmutableAndFree();
	}

	internal void ParseOutputFile(string value, IList<Diagnostic> errors, string baseDirectory, out string outputFileName, out string outputDirectory)
	{
		outputFileName = null;
		outputDirectory = null;
		string invalidPath = null;
		ParseAndNormalizeFile(RemoveQuotesAndSlashes(value), baseDirectory, out outputFileName, out outputDirectory, out invalidPath);
		if (outputFileName == null || !MetadataHelpers.IsValidAssemblyOrModuleName(outputFileName))
		{
			errors.Add(Diagnostic.Create(_messageProvider, _messageProvider.FTL_InvalidInputFileName, invalidPath));
			outputFileName = null;
			outputDirectory = baseDirectory;
		}
	}

	internal string ParsePdbPath(string value, IList<Diagnostic> errors, string baseDirectory)
	{
		string outputFileName = null;
		string outputDirectory = null;
		string result = null;
		string invalidPath = null;
		ParseAndNormalizeFile(RemoveQuotesAndSlashes(value), baseDirectory, out outputFileName, out outputDirectory, out invalidPath);
		if (outputFileName == null || PathUtilities.ChangeExtension(outputFileName, null).Length == 0)
		{
			errors.Add(Diagnostic.Create(_messageProvider, _messageProvider.FTL_InvalidInputFileName, invalidPath));
		}
		else
		{
			result = Path.ChangeExtension(Path.Combine(outputDirectory, outputFileName), ".pdb");
		}
		return result;
	}

	internal string ParseGenericPathToFile(string unquoted, IList<Diagnostic> errors, string baseDirectory, bool generateDiagnostic = true)
	{
		string outputFileName = null;
		string outputDirectory = null;
		string result = null;
		string invalidPath = null;
		ParseAndNormalizeFile(unquoted, baseDirectory, out outputFileName, out outputDirectory, out invalidPath);
		if (string.IsNullOrWhiteSpace(outputFileName))
		{
			if (generateDiagnostic)
			{
				errors.Add(Diagnostic.Create(_messageProvider, _messageProvider.FTL_InvalidInputFileName, invalidPath));
			}
		}
		else
		{
			result = Path.Combine(outputDirectory, outputFileName);
		}
		return result;
	}

	internal void FlattenArgs(IEnumerable<string> rawArguments, IList<Diagnostic> diagnostics, List<string> processedArgs, List<string> scriptArgsOpt, string baseDirectory, List<string> responsePaths = null)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		Stack<string> stack = new Stack<string>(rawArguments.Reverse());
		while (stack.Count > 0)
		{
			string text = stack.Pop().TrimEnd(Array.Empty<char>());
			if (flag)
			{
				scriptArgsOpt.Add(text);
				continue;
			}
			if (scriptArgsOpt != null)
			{
				if (flag2)
				{
					flag = true;
					scriptArgsOpt.Add(text);
					continue;
				}
				if (!flag3 && text == "--")
				{
					flag3 = true;
					processedArgs.Add(text);
					continue;
				}
			}
			if (!flag3 && text.StartsWith("@", StringComparison.Ordinal))
			{
				string text2 = RemoveQuotesAndSlashes(text.Substring(1)).TrimEnd(null);
				string text3 = FileUtilities.ResolveRelativePath(text2, baseDirectory);
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
							diagnostics.Add(Diagnostic.Create(_messageProvider, _messageProvider.WRN_NoConfigNotOnCommandLine));
						}
					}
					responsePaths?.Add(FileUtilities.NormalizeAbsolutePath(PathUtilities.GetDirectoryName(text3)));
				}
				else
				{
					diagnostics.Add(Diagnostic.Create(_messageProvider, _messageProvider.FTL_InvalidInputFileName, text2));
				}
			}
			else
			{
				processedArgs.Add(text);
				flag2 |= flag3 || !IsOption(text);
			}
		}
	}

	internal static bool TryParseClientArgs(IEnumerable<string> args, out List<string> parsedArgs, out bool containsShared, out string keepAliveValue, out string sessionKey, out string errorMessage)
	{
		containsShared = false;
		keepAliveValue = null;
		errorMessage = null;
		parsedArgs = null;
		sessionKey = null;
		List<string> list = new List<string>();
		foreach (string arg in args)
		{
			if (IsClientArgsOption(arg, "/keepalive", out var hasValue, out var optionValue))
			{
				if (string.IsNullOrEmpty(optionValue))
				{
					errorMessage = CodeAnalysisResources.MissingKeepAlive;
					return false;
				}
				if (!int.TryParse(optionValue, out var result))
				{
					errorMessage = CodeAnalysisResources.KeepAliveIsNotAnInteger;
					return false;
				}
				if (result < -1)
				{
					errorMessage = CodeAnalysisResources.KeepAliveIsTooSmall;
					return false;
				}
				keepAliveValue = optionValue;
			}
			else if (IsClientArgsOption(arg, "/shared", out hasValue, out optionValue))
			{
				if (hasValue)
				{
					if (string.IsNullOrEmpty(optionValue))
					{
						errorMessage = CodeAnalysisResources.SharedArgumentMissing;
						return false;
					}
					sessionKey = optionValue;
				}
				containsShared = true;
			}
			else
			{
				list.Add(arg);
			}
		}
		if (keepAliveValue != null && !containsShared)
		{
			errorMessage = CodeAnalysisResources.KeepAliveWithoutShared;
			return false;
		}
		parsedArgs = list;
		return true;
	}

	internal static bool IsClientArgsOption(string arg, string optionName, out bool hasValue, out string optionValue)
	{
		hasValue = false;
		optionValue = null;
		if (!arg.StartsWith(optionName, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (arg.Length > optionName.Length && arg[optionName.Length] != ':' && arg[optionName.Length] != '=')
		{
			return false;
		}
		if (arg.Length > optionName.Length)
		{
			hasValue = true;
			optionValue = arg.Substring(optionName.Length + 1).Trim(new char[1] { '"' });
		}
		return true;
	}

	internal IEnumerable<string> ParseResponseFile(string fullPath, IList<Diagnostic> errors)
	{
		List<string> list = new List<string>();
		try
		{
			using TextReader textReader = CreateTextFileReader(fullPath);
			string item;
			while ((item = textReader.ReadLine()) != null)
			{
				list.Add(item);
			}
		}
		catch (Exception)
		{
			errors.Add(Diagnostic.Create(_messageProvider, _messageProvider.ERR_OpenResponseFile, fullPath));
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
		string[] array = ParseSeparatedStrings(resourceDescriptor, s_resourceSeparators).ToArray();
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
			filePath = RemoveQuotesAndSlashes(array[i]);
		}
		if (num >= 2)
		{
			resourceName = RemoveQuotesAndSlashes(array[i + 1]);
		}
		if (num >= 3)
		{
			accessibility = RemoveQuotesAndSlashes(array[i + 2]);
		}
		if (!string.IsNullOrWhiteSpace(filePath))
		{
			fileName = PathUtilities.GetFileName(filePath);
			fullPath = FileUtilities.ResolveRelativePath(filePath, baseDirectory);
			if (string.IsNullOrWhiteSpace(resourceName))
			{
				resourceName = fileName;
			}
		}
	}

	public static IEnumerable<string> SplitCommandLineIntoArguments(string commandLine, bool removeHashComments)
	{
		return CommandLineUtilities.SplitCommandLineIntoArguments(commandLine, removeHashComments);
	}

	internal static string RemoveQuotesAndSlashes(string arg)
	{
		if (arg == null)
		{
			return arg;
		}
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		int i = 0;
		while (i < arg.Length)
		{
			char c = arg[i];
			switch (c)
			{
			case '\\':
				ProcessSlashes(builder, arg, ref i);
				break;
			case '"':
				i++;
				break;
			default:
				builder.Append(c);
				i++;
				break;
			}
		}
		return instance.ToStringAndFree();
	}

	internal static void ProcessSlashes(StringBuilder builder, string arg, ref int i)
	{
		int num = 0;
		while (i < arg.Length && arg[i] == '\\')
		{
			num++;
			i++;
		}
		if (i < arg.Length && arg[i] == '"')
		{
			while (num >= 2)
			{
				builder.Append('\\');
				num -= 2;
			}
			if (num == 1)
			{
				builder.Append('"');
			}
			i++;
		}
		else
		{
			while (num > 0)
			{
				builder.Append('\\');
				num--;
			}
		}
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

	internal static IEnumerable<string> ParseSeparatedPaths(string str)
	{
		return ParseSeparatedStrings(str, s_pathSeparators, StringSplitOptions.RemoveEmptyEntries).Select(RemoveQuotesAndSlashes);
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
			return !inQuotes && Enumerable.Contains(separators, c);
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
			string text = FileUtilities.ResolveRelativePath(path, baseDirectory);
			if (text == null)
			{
				errors.Add(Diagnostic.Create(_messageProvider, _messageProvider.FTL_InvalidInputFileName, path));
			}
			else
			{
				yield return text;
			}
		}
	}

	private CommandLineSourceFile ToCommandLineSourceFile(string resolvedPath)
	{
		string extension = PathUtilities.GetExtension(resolvedPath);
		bool isScript = IsScriptCommandLineParser && !string.Equals(extension, RegularFileExtension, StringComparison.OrdinalIgnoreCase);
		return new CommandLineSourceFile(resolvedPath, isScript);
	}

	internal IEnumerable<CommandLineSourceFile> ParseFileArgument(string arg, string baseDirectory, IList<Diagnostic> errors)
	{
		string text = RemoveQuotesAndSlashes(arg);
		if (text.IndexOfAny(s_wildcards) != -1)
		{
			foreach (CommandLineSourceFile item in ExpandFileNamePattern(text, baseDirectory, SearchOption.TopDirectoryOnly, errors))
			{
				yield return item;
			}
			yield break;
		}
		string text2 = FileUtilities.ResolveRelativePath(text, baseDirectory);
		if (text2 == null)
		{
			errors.Add(Diagnostic.Create(MessageProvider, MessageProvider.FTL_InvalidInputFileName, text));
		}
		else
		{
			yield return ToCommandLineSourceFile(text2);
		}
	}

	internal IEnumerable<CommandLineSourceFile> ParseSeparatedFileArgument(string value, string baseDirectory, IList<Diagnostic> errors)
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

	internal IEnumerable<CommandLineSourceFile> ParseRecurseArgument(string arg, string baseDirectory, IList<Diagnostic> errors)
	{
		return ExpandFileNamePattern(arg, baseDirectory, SearchOption.AllDirectories, errors);
	}

	internal static Encoding TryParseEncodingName(string arg)
	{
		if (!string.IsNullOrWhiteSpace(arg) && long.TryParse(arg, NumberStyles.None, CultureInfo.InvariantCulture, out var result) && result > 0)
		{
			try
			{
				return Encoding.GetEncoding((int)result);
			}
			catch (Exception)
			{
				return null;
			}
		}
		return null;
	}

	internal static SourceHashAlgorithm TryParseHashAlgorithmName(string arg)
	{
		if (string.Equals("sha1", arg, StringComparison.OrdinalIgnoreCase))
		{
			return SourceHashAlgorithm.Sha1;
		}
		if (string.Equals("sha256", arg, StringComparison.OrdinalIgnoreCase))
		{
			return SourceHashAlgorithm.Sha256;
		}
		return SourceHashAlgorithm.None;
	}

	private IEnumerable<CommandLineSourceFile> ExpandFileNamePattern(string path, string baseDirectory, SearchOption searchOption, IList<Diagnostic> errors)
	{
		string directoryName = PathUtilities.GetDirectoryName(path);
		string pattern = PathUtilities.GetFileName(path);
		string resolvedDirectoryPath = ((directoryName.Length == 0) ? baseDirectory : FileUtilities.ResolveRelativePath(directoryName, baseDirectory));
		IEnumerator<string> enumerator = null;
		try
		{
			bool yielded = false;
			pattern = pattern.Trim(s_searchPatternTrimChars);
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
						goto IL_00fd;
					}
					catch
					{
						text = null;
						goto IL_00fd;
					}
					IL_00fd:
					if (text != null)
					{
						text = FileUtilities.ResolveRelativePath(text, baseDirectory);
					}
					if (text == null)
					{
						errors.Add(Diagnostic.Create(MessageProvider, MessageProvider.FTL_InvalidInputFileName, path));
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
				errors.Add(Diagnostic.Create(MessageProvider, MessageProvider.ERR_FileNotFound, path));
			}
		}
		finally
		{
			enumerator?.Dispose();
		}
	}

	internal abstract void GenerateErrorForNoFilesFoundInRecurse(string path, IList<Diagnostic> errors);

	internal ReportDiagnostic GetDiagnosticOptionsFromRulesetFile(string fullPath, out Dictionary<string, ReportDiagnostic> diagnosticOptions, IList<Diagnostic> diagnostics)
	{
		return RuleSet.GetDiagnosticOptionsFromRulesetFile(fullPath, out diagnosticOptions, diagnostics, _messageProvider);
	}

	internal static bool TryParseUInt64(string value, out ulong result)
	{
		result = 0uL;
		if (string.IsNullOrEmpty(value))
		{
			return false;
		}
		int fromBase = 10;
		if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
		{
			fromBase = 16;
		}
		else if (value.StartsWith("0", StringComparison.OrdinalIgnoreCase))
		{
			fromBase = 8;
		}
		try
		{
			result = Convert.ToUInt64(value, fromBase);
		}
		catch
		{
			return false;
		}
		return true;
	}

	internal static bool TryParseUInt16(string value, out ushort result)
	{
		result = 0;
		if (string.IsNullOrEmpty(value))
		{
			return false;
		}
		int fromBase = 10;
		if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
		{
			fromBase = 16;
		}
		else if (value.StartsWith("0", StringComparison.OrdinalIgnoreCase))
		{
			fromBase = 8;
		}
		try
		{
			result = Convert.ToUInt16(value, fromBase);
		}
		catch
		{
			return false;
		}
		return true;
	}

	internal static ImmutableDictionary<string, string> ParseFeatures(List<string> features)
	{
		ImmutableDictionary<string, string>.Builder builder = ImmutableDictionary.CreateBuilder<string, string>();
		CompilerOptionParseUtilities.ParseFeatures(builder, features);
		return builder.ToImmutable();
	}
}
