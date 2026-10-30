using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.DiagnosticRules;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CommandLine;

internal class CommandLineParser : CommonCommandLineParser
{
	[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
	public static readonly CommandLineParser Default = new CommandLineParser();

	protected override string RegularFileExtension => ".al";

	internal CommandLineParser()
		: base(Microsoft.Dynamics.Nav.CodeAnalysis.MessageProvider.Instance)
	{
	}

	internal sealed override CommandLineArguments CommonParse(IEnumerable<string> args, string baseDirectory, string sdkDirectoryOpt, string additionalReferenceDirectories)
	{
		return Parse(args, baseDirectory, additionalReferenceDirectories);
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "additionalReferenceDirectories", Justification = "By design")]
	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Needs to be refactored !!")]
	[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode", Justification = "Needs to be refactored !!")]
	[SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals", Justification = "Needs to be refactored !!")]
	public virtual CommandLineArguments Parse(IEnumerable<string> args, string baseDirectory, string additionalReferenceDirectories = null)
	{
		List<Diagnostic> list = new List<Diagnostic>();
		List<string> list2 = new List<string>();
		FlattenArgs(args, list, list2, null, baseDirectory);
		bool displayLogo = true;
		bool displayHelp = false;
		bool flag = true;
		string outputDirectory = baseDirectory;
		string text = null;
		string outputFileName = null;
		string errorLogPath = null;
		LogLevel? logLevel = null;
		bool printFullPaths = false;
		string text2 = null;
		SymbolReferenceSpecification symbolReferenceSpecification = null;
		List<CommandLineSourceFile> list3 = new List<CommandLineSourceFile>();
		List<CommandLineSourceFile> list4 = new List<CommandLineSourceFile>();
		ImmutableArray<string>.Builder builder = ImmutableArray.CreateBuilder<string>();
		List<CommandLineAnalyzerReference> list5 = new List<CommandLineAnalyzerReference>();
		Dictionary<string, ReportDiagnostic> dictionary = new Dictionary<string, ReportDiagnostic>();
		Dictionary<string, ReportDiagnostic> dictionary2 = new Dictionary<string, ReportDiagnostic>();
		Dictionary<string, ReportDiagnostic> dictionary3 = new Dictionary<string, ReportDiagnostic>();
		int warningLevel = 4;
		ReportDiagnostic reportDiagnostic = ReportDiagnostic.Default;
		CompilationTarget? compilationTarget = null;
		CompilerFeatures compilerFeatures = CompilerFeatures.None;
		string text3 = null;
		bool flag2 = false;
		bool flag3 = false;
		CompilationGenerationOptions compilationGenerationOptions = CompilationGenerationOptions.ReportLayout;
		CodeGenerationOption codeGenerationOptions = CodeGenerationOption.Text;
		bool concurrentEmit = false;
		NavAppSupportedLocales navAppSupportedLocales = null;
		string text4 = null;
		foreach (string item in list2)
		{
			if (CommonCommandLineParser.TryParseOption(item, out var name, out var value) && name == "ruleset")
			{
				if (string.IsNullOrEmpty(value))
				{
					AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, ":<file>", CommonCommandLineParser.RemoveAllQuotes(item));
				}
				else
				{
					text3 = ParseGenericPathToFile(value, list, baseDirectory);
					RuleSet ruleSet = RuleSetResolver.LoadFromFile(text3, list);
					dictionary = new Dictionary<string, ReportDiagnostic>(ruleSet.SpecificDiagnosticOptions);
					reportDiagnostic = ruleSet.GeneralDiagnosticOption;
				}
			}
		}
		foreach (string item2 in list2)
		{
			DebugAssertHelper.Assert(!item2.StartsWith("@", StringComparison.Ordinal));
			if (!CommonCommandLineParser.TryParseOption(item2, out var name2, out var value2))
			{
				list3.AddRange(ParseFileArgument(item2, baseDirectory, list));
				continue;
			}
			switch (name2.ToLowerInvariant())
			{
			case "?":
			case "help":
				displayHelp = true;
				continue;
			case "a":
			case "analyzer":
				list5.AddRange(ParseAnalyzers(item2, value2, list));
				continue;
			case "features":
			{
				CompilerFeaturesExtensions.TryGetFromRawString(value2, out var compilerFeatures2);
				compilerFeatures |= compilerFeatures2;
				continue;
			}
			case "project":
			{
				text2 = CommonCommandLineParser.RemoveAllQuotes(value2);
				if (string.IsNullOrEmpty(text2))
				{
					AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, ":<directory>", CommonCommandLineParser.RemoveAllQuotes(item2));
					continue;
				}
				text2 = PathUtilities.ResolveRelativePath(text2, null, baseDirectory);
				if (string.IsNullOrEmpty(text2) || !Directory.Exists(text2))
				{
					AddDiagnostic(list, ErrorCode.ERR_DirectoryNotFound, text2);
					continue;
				}
				string[] files = Directory.GetFiles(text2, "*" + RegularFileExtension, SearchOption.AllDirectories);
				foreach (string path in files)
				{
					list3.Add(new CommandLineSourceFile(path));
				}
				continue;
			}
			case "app":
			{
				string text5 = CommonCommandLineParser.RemoveAllQuotes(value2);
				if (string.IsNullOrEmpty(text5) || !Version.TryParse(text5, out var result))
				{
					AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, ":<major.minor.build.revision>", CommonCommandLineParser.RemoveAllQuotes(item2));
				}
				else
				{
					symbolReferenceSpecification = SymbolReferenceSpecification.ApplicationReference(result);
				}
				continue;
			}
			case "readonlyerrorsaswarnings":
				flag2 = true;
				continue;
			case "generatecode":
			case "generatecode+":
				compilationGenerationOptions |= CompilationGenerationOptions.Code;
				continue;
			case "generatecode-":
				compilationGenerationOptions &= ~CompilationGenerationOptions.Code;
				continue;
			case "generatereportlayout":
			case "generatereportlayout+":
				compilationGenerationOptions |= CompilationGenerationOptions.ReportLayout;
				continue;
			case "generatereportlayout-":
				compilationGenerationOptions &= ~CompilationGenerationOptions.ReportLayout;
				continue;
			case "generatecodeasbinary":
				codeGenerationOptions = CodeGenerationOption.Ast;
				continue;
			case "parallelemit":
				concurrentEmit = true;
				continue;
			case "noconfig":
				continue;
			}
			switch (name2)
			{
			case "out":
				if (string.IsNullOrWhiteSpace(value2))
				{
					AddDiagnostic(list, ErrorCode.ERR_NoFileSpec, item2);
				}
				else
				{
					ParseOutputFile(value2, list, baseDirectory, out outputFileName, out outputDirectory);
				}
				continue;
			case "t":
			case "target":
			{
				if (value2 == null)
				{
					break;
				}
				if (string.IsNullOrEmpty(value2) || !Enum.TryParse<CompilationTarget>(value2, ignoreCase: true, out var result2))
				{
					AddDiagnostic(list, ErrorCode.FTL_InvalidTarget);
				}
				else
				{
					compilationTarget = result2;
				}
				continue;
			}
			case "fullpaths":
				if (value2 != null)
				{
					break;
				}
				printFullPaths = true;
				continue;
			case "nologo":
				displayLogo = false;
				continue;
			case "recurse":
				if (value2 == null)
				{
					break;
				}
				if (string.IsNullOrEmpty(value2))
				{
					AddDiagnostic(list, ErrorCode.ERR_NoFileSpec, item2);
				}
				else
				{
					list3.AddRange(ParseRecurseArgument(value2, baseDirectory, list));
				}
				continue;
			case "p":
			case "parallel":
			case "p+":
			case "parallel+":
				if (value2 != null)
				{
					break;
				}
				flag = true;
				continue;
			case "p-":
			case "parallel-":
				if (value2 != null)
				{
					break;
				}
				flag = false;
				continue;
			case "warnaserror":
			case "warnaserror+":
				if (value2 == null)
				{
					reportDiagnostic = ReportDiagnostic.Error;
					dictionary3.Clear();
					foreach (string key in dictionary.Keys)
					{
						if (dictionary[key] == ReportDiagnostic.Warn)
						{
							dictionary3[key] = ReportDiagnostic.Error;
						}
					}
				}
				else if (string.IsNullOrEmpty(value2))
				{
					AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsNumber, name2);
				}
				else
				{
					AddWarnings(dictionary3, ReportDiagnostic.Error, ParseWarnings(value2));
				}
				continue;
			case "warnaserror-":
				if (value2 == null)
				{
					reportDiagnostic = ReportDiagnostic.Default;
					dictionary3.Clear();
					continue;
				}
				if (string.IsNullOrEmpty(value2))
				{
					AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsNumber, name2);
					continue;
				}
				foreach (string item3 in ParseWarnings(value2))
				{
					if (dictionary.TryGetValue(item3, out var value4))
					{
						dictionary3[item3] = value4;
					}
					else
					{
						dictionary3[item3] = ReportDiagnostic.Default;
					}
				}
				continue;
			case "nowarn":
				if (value2 == null)
				{
					AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsNumber, name2);
				}
				else if (string.IsNullOrEmpty(value2))
				{
					AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsNumber, name2);
				}
				else
				{
					AddWarnings(dictionary2, ReportDiagnostic.Suppress, ParseWarnings(value2));
				}
				continue;
			case "errorlog":
			{
				string text6 = CommonCommandLineParser.RemoveAllQuotes(value2);
				if (string.IsNullOrEmpty(text6))
				{
					AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, ":<file>", CommonCommandLineParser.RemoveAllQuotes(item2));
				}
				else
				{
					errorLogPath = ParseGenericPathToFile(text6, list, baseDirectory);
				}
				continue;
			}
			case "loglevel":
			{
				string value3 = CommonCommandLineParser.RemoveAllQuotes(value2);
				LogLevel result3;
				if (string.IsNullOrEmpty(value3))
				{
					AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, ":<LogLevel>", CommonCommandLineParser.RemoveAllQuotes(item2));
				}
				else if (Enum.TryParse<LogLevel>(value3, ignoreCase: true, out result3))
				{
					logLevel = result3;
				}
				else
				{
					AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, ":<LogLevel>", CommonCommandLineParser.RemoveAllQuotes(item2));
				}
				continue;
			}
			case "additionalfile":
				if (string.IsNullOrEmpty(value2))
				{
					AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, ":<file1, file2, file3, ...>", name2);
				}
				else
				{
					list4.AddRange(ParseAdditionalFileArgument(value2, baseDirectory, list));
				}
				continue;
			case "packagecachepath":
				text = CommonCommandLineParser.RemoveAllQuotes(value2);
				if (string.IsNullOrEmpty(text))
				{
					AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, ":<directory>", CommonCommandLineParser.RemoveAllQuotes(item2));
					continue;
				}
				text = PathUtilities.ResolveRelativePath(text, null, baseDirectory);
				if (string.IsNullOrEmpty(text) || !Directory.Exists(text))
				{
					AddDiagnostic(list, ErrorCode.ERR_DirectoryNotFound, text);
				}
				continue;
			case "assemblyprobingpaths":
				if (string.IsNullOrEmpty(value2))
				{
					AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, ":<directory1, directory2, directory3, ...>", name2);
				}
				else
				{
					builder.AddRange(ParseAssemblyProbingPathsArgument(value2, baseDirectory, list));
				}
				continue;
			case "ruleset":
				continue;
			}
			AddDiagnostic(list, ErrorCode.ERR_BadSwitch, item2);
		}
		foreach (KeyValuePair<string, ReportDiagnostic> item4 in dictionary3)
		{
			dictionary[item4.Key] = item4.Value;
		}
		foreach (KeyValuePair<string, ReportDiagnostic> item5 in dictionary2)
		{
			dictionary[item5.Key] = item5.Value;
		}
		ProjectManifest projectManifest = ProjectLoader.LoadFromFolder(text2, list);
		if (projectManifest != null)
		{
			compilerFeatures |= projectManifest.AppManifest.CompilerFeatures;
			flag2 |= projectManifest.AppManifest.ReadonlyErrorsAsWarnings;
			if (!compilationTarget.HasValue)
			{
				compilationTarget = projectManifest.AppManifest.Target;
			}
			if (symbolReferenceSpecification != null)
			{
				projectManifest.AppManifest.Application = symbolReferenceSpecification.Version;
			}
			flag3 = projectManifest.AppManifest.ShowMyCode;
			navAppSupportedLocales = projectManifest.AppManifest.AppSupportedLocales;
			text4 = projectManifest.AppManifest.ContextSensitiveHelpUrl;
		}
		bool concurrentBuild = flag;
		ReportDiagnostic generalDiagnosticOption = reportDiagnostic;
		IdSpaces idSpaces = projectManifest?.AppManifest.IdSpaces;
		CompilationTarget target = compilationTarget ?? CompilationTarget.Extension;
		bool readonlyErrorsAsWarnings = flag2;
		bool flag4 = flag3;
		CompilerFeatures compilerFeatures3 = compilerFeatures;
		bool showMyCode = flag4;
		CompilationGenerationOptions generateOptions = compilationGenerationOptions;
		NavAppSupportedLocales supportedLocales = navAppSupportedLocales;
		string contextSensitiveHelpUrl = text4;
		CompilationOptions compilationOptions = new CompilationOptions(concurrentBuild, warningLevel, generalDiagnosticOption, reportSuppressedDiagnostics: false, idSpaces, target, allowExtensionAndTargetInSameModule: false, readonlyErrorsAsWarnings, compilerFeatures3, showMyCode, null, generateOptions, supportedLocales, codeGenerationOptions, concurrentEmit, contextSensitiveHelpUrl);
		if (outputFileName == null && projectManifest?.AppManifest != null)
		{
			outputFileName = Path.Combine(text2, projectManifest.AppManifest.DefaultPackageName() + ".app");
		}
		if (outputFileName == null && projectManifest == null)
		{
			AddDiagnostic(list, ErrorCode.ERR_OutputFileNotSpecified);
		}
		return new CommandLineArguments
		{
			BaseDirectory = baseDirectory,
			Errors = list.ToImmutableArray(),
			CompilationName = string.Empty,
			ProjectManifest = projectManifest,
			OutputFileName = outputFileName,
			OutputDirectory = outputDirectory,
			PackageCacheDirectory = text,
			ErrorLogPath = errorLogPath,
			SourceFiles = list3.ToImmutableArray(),
			AnalyzerReferences = list5.ToImmutableArray(),
			AdditionalFiles = list4.ToImmutableArray(),
			DisplayLogo = displayLogo,
			DisplayHelp = displayHelp,
			CompilationOptions = compilationOptions,
			PrintFullPaths = printFullPaths,
			ProjectPath = text2,
			AssemblyProbingPaths = builder.ToImmutable(),
			RuleSetPath = text3,
			LogLevel = logLevel
		};
	}

	private IEnumerable<CommandLineAnalyzerReference> ParseAnalyzers(string arg, string value, List<Diagnostic> diagnostics)
	{
		if (value == null)
		{
			AddDiagnostic(diagnostics, ErrorCode.ERR_SwitchNeedsString, MessageID.IDS_Text.Localize(), arg);
			yield break;
		}
		if (value.Length == 0)
		{
			AddDiagnostic(diagnostics, ErrorCode.ERR_NoFileSpec, arg);
			yield break;
		}
		IEnumerable<string> enumerable = from path in CommonCommandLineParser.ParseSeparatedPaths(value)
			where !string.IsNullOrWhiteSpace(path)
			select path;
		foreach (string item in enumerable)
		{
			yield return new CommandLineAnalyzerReference(item);
		}
	}

	private static IEnumerable<string> ParseWarnings(string value)
	{
		value = value.UnquoteIdentifier();
		string[] array = value.Split(new char[3] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (ushort.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) && ErrorFacts.IsWarning((ErrorCode)result))
			{
				yield return Microsoft.Dynamics.Nav.CodeAnalysis.MessageProvider.Instance.GetIdForErrorCode(result);
			}
			else
			{
				yield return text;
			}
		}
	}

	private static void AddWarnings(Dictionary<string, ReportDiagnostic> d, ReportDiagnostic kind, IEnumerable<string> items)
	{
		foreach (string item in items)
		{
			if (d.TryGetValue(item, out var value))
			{
				if (value != ReportDiagnostic.Suppress)
				{
					d[item] = kind;
				}
			}
			else
			{
				d.Add(item, kind);
			}
		}
	}

	private static void AddDiagnostic(IList<Diagnostic> diagnostics, ErrorCode errorCode)
	{
		diagnostics.Add(Diagnostic.Create(Microsoft.Dynamics.Nav.CodeAnalysis.MessageProvider.Instance, (int)errorCode));
	}

	private static void AddDiagnostic(IList<Diagnostic> diagnostics, ErrorCode errorCode, params object[] arguments)
	{
		diagnostics.Add(Diagnostic.Create(Microsoft.Dynamics.Nav.CodeAnalysis.MessageProvider.Instance, (int)errorCode, arguments));
	}

	private static void AddDiagnostic(IList<Diagnostic> diagnostics, Dictionary<string, ReportDiagnostic> warningOptions, ErrorCode errorCode, params object[] arguments)
	{
		warningOptions.TryGetValue(Microsoft.Dynamics.Nav.CodeAnalysis.MessageProvider.Instance.GetIdForErrorCode((int)errorCode), out var value);
		if (value != ReportDiagnostic.Suppress)
		{
			AddDiagnostic(diagnostics, errorCode, arguments);
		}
	}
}
