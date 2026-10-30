using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Translation;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CommandLine;

[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal abstract class CommonCompiler
{
	private delegate CommandLineArguments CommonParser();

	internal static class Misc
	{
		internal static string GetFileVersion(string path)
		{
			return FileVersionInfo.GetVersionInfo(path).FileVersion;
		}

		internal static void SetCurrentUICulture(CultureInfo cultureInfo)
		{
			Thread.CurrentThread.CurrentUICulture = cultureInfo;
		}
	}

	internal const int Failed = 1;

	internal const int Succeeded = 0;

	private CommandLineArguments arguments;

	private ProjectManifest projectManifest;

	public CommonMessageProvider MessageProvider { get; }

	public IAnalyzerAssemblyLoader AnalyzerLoader { get; }

	public abstract DiagnosticFormatter DiagnosticFormatter { get; }

	internal string ClientDirectory { get; }

	private CommonParser ParseDelegate { get; }

	internal IFileSystem FileSystem { get; }

	public CommandLineArguments Arguments
	{
		get
		{
			if (arguments == null)
			{
				arguments = ParseDelegate();
			}
			return arguments;
		}
	}

	protected internal virtual ProjectManifest ProjectManifest
	{
		get
		{
			return projectManifest ?? Arguments.ProjectManifest;
		}
		internal set
		{
			projectManifest = value;
		}
	}

	protected HashSet<Diagnostic> ReportedDiagnostics { get; } = new HashSet<Diagnostic>();

	protected virtual CultureInfo Culture => CultureInfo.CurrentUICulture;

	public abstract Compilation GetOrCreateCompilation(CompilerTextWriter consoleOutput, CompilationOptions compilationOptions, ErrorLogger errorLogger);

	public abstract void PrintLogo(CompilerTextWriter consoleOutput);

	public abstract void PrintHelp(CompilerTextWriter consoleOutput);

	internal abstract string GetToolName();

	protected abstract bool TryGetCompilerDiagnosticCode(string diagnosticId, out uint code);

	protected abstract ImmutableArray<DiagnosticAnalyzer> ResolveAnalyzersFromArguments(IList<DiagnosticInfo> diagnostics, CommonMessageProvider messageProvider);

	protected CommonCompiler(CommandLineParser parser, string responseFile, IEnumerable<string> args, string clientDirectory, string baseDirectory, string sdkDirectoryOpt, string additionalReferenceDirectories, IAnalyzerAssemblyLoader analyzerLoader, IFileSystem fileSystem = null)
	{
		CommonCompiler commonCompiler = this;
		IEnumerable<string> allArgs = args;
		ClientDirectory = clientDirectory;
		FileSystem = fileSystem ?? new FileSystem();
		DebugAssertHelper.Assert(responseFile == null || PathUtilities.IsAbsolute(responseFile));
		ParseDelegate = () =>
		{
			if (responseFile != null && !commonCompiler.SuppressDefaultResponseFile(args) && commonCompiler.FileSystem.Exists(responseFile))
			{
				allArgs = new string[1] { "@" + responseFile }.Concat(allArgs);
			}
			return parser.CommonParse(allArgs, baseDirectory, sdkDirectoryOpt, additionalReferenceDirectories);
		};
		MessageProvider = parser.MessageProvider;
		AnalyzerLoader = analyzerLoader;
	}

	internal abstract bool SuppressDefaultResponseFile(IEnumerable<string> args);

	internal string GetAssemblyFileVersion()
	{
		if (ClientDirectory != null)
		{
			string location = typeof(CommonCompiler).Assembly.Location;
			if (!string.IsNullOrEmpty(location))
			{
				return Misc.GetFileVersion(location);
			}
		}
		return string.Empty;
	}

	internal static Version GetAssemblyVersion()
	{
		return typeof(CommonCompiler).GetTypeInfo().Assembly.GetName().Version;
	}

	internal SourceText ReadFileContent(CommandLineSourceFile file, IList<DiagnosticInfo> diagnostics)
	{
		string normalizedFilePath;
		return ReadFileContent(file, diagnostics, out normalizedFilePath);
	}

	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	internal SourceText ReadFileContent(CommandLineSourceFile file, IList<DiagnosticInfo> diagnostics, out string normalizedFilePath)
	{
		string path = file.Path;
		try
		{
			using Stream stream = FileSystem.OpenFile(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1);
			FileStream fileStream = stream as FileStream;
			normalizedFilePath = ((fileStream != null) ? fileStream.Name : path);
			return EncodedStringText.Create(stream, Arguments.Encoding);
		}
		catch (Exception e)
		{
			diagnostics.Add(ToFileReadDiagnostics(MessageProvider, e, path));
			normalizedFilePath = null;
			return null;
		}
	}

	internal static DiagnosticInfo ToFileReadDiagnostics(CommonMessageProvider messageProvider, Exception e, string filePath)
	{
		if (e is FileNotFoundException || e.GetType().Name == "DirectoryNotFoundException")
		{
			return new DiagnosticInfo(messageProvider, 1001, filePath);
		}
		if (e is InvalidDataException)
		{
			return new DiagnosticInfo(messageProvider, 1013, filePath);
		}
		return new DiagnosticInfo(messageProvider, 1014, filePath, e.Message);
	}

	public bool ReportErrors(IEnumerable<Diagnostic> diagnostics, CompilerTextWriter consoleOutput, ErrorLogger errorLogger)
	{
		bool result = false;
		foreach (Diagnostic diagnostic in diagnostics)
		{
			if (ReportedDiagnostics.Contains(diagnostic) || diagnostic.Severity == DiagnosticSeverity.Hidden)
			{
				continue;
			}
			ErrorLogger.LogDiagnostic(diagnostic, Culture, errorLogger);
			if (!diagnostic.IsSuppressed)
			{
				consoleOutput.WriteLine(DiagnosticFormatter.Format(diagnostic, Culture));
				if (diagnostic.Severity == DiagnosticSeverity.Error)
				{
					result = true;
				}
				ReportedDiagnostics.Add(diagnostic);
			}
		}
		return result;
	}

	public bool ReportErrors(List<DiagnosticInfo> diagnostics, CompilerTextWriter consoleOutput, ErrorLogger errorLogger)
	{
		bool result = false;
		if (diagnostics != null && diagnostics.Any())
		{
			foreach (DiagnosticInfo diagnostic in diagnostics)
			{
				if (diagnostic.Severity != DiagnosticSeverity.Hidden)
				{
					PrintError(diagnostic, consoleOutput);
					ErrorLogger.LogDiagnostic(Diagnostic.Create(diagnostic), Culture, errorLogger);
					if (diagnostic.Severity == DiagnosticSeverity.Error)
					{
						result = true;
					}
				}
			}
		}
		return result;
	}

	protected virtual void PrintError(DiagnosticInfo diagnostic, CompilerTextWriter consoleOutput)
	{
		consoleOutput.WriteLine(diagnostic.ToString(Culture));
	}

	public ErrorLogger GetErrorLogger(CompilerTextWriter consoleOutput)
	{
		DebugAssertHelper.Assert(Arguments.ErrorLogPath != null);
		Stream stream = OpenFile(Arguments.ErrorLogPath, consoleOutput, FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
		if (stream == null)
		{
			return null;
		}
		return new ErrorLogger(stream, GetToolName(), GetAssemblyFileVersion(), GetAssemblyVersion());
	}

	public virtual int Run(CompilerTextWriter consoleOutput, CancellationToken cancellationToken = default(CancellationToken))
	{
		CultureInfo currentUICulture = CultureInfo.CurrentUICulture;
		ErrorLogger errorLogger = null;
		try
		{
			CultureInfo culture = Culture;
			if (culture != null)
			{
				Misc.SetCurrentUICulture(culture);
			}
			if (Arguments.ErrorLogPath != null)
			{
				errorLogger = GetErrorLogger(consoleOutput);
				if (errorLogger == null)
				{
					return 1;
				}
			}
			if (Arguments.LogLevel.HasValue)
			{
				GlobalLogger.SetLogLevel(Arguments.LogLevel.Value);
			}
			return RunCore(consoleOutput, errorLogger, cancellationToken);
		}
		catch (OperationCanceledException)
		{
			int num = 1015;
			if (num > 0)
			{
				DiagnosticInfo item = new DiagnosticInfo(MessageProvider, num);
				ReportErrors(new List<DiagnosticInfo> { item }, consoleOutput, errorLogger);
			}
			return 1;
		}
		finally
		{
			Misc.SetCurrentUICulture(currentUICulture);
			errorLogger?.Dispose();
		}
	}

	protected virtual void PrintCompilationStart(CompilerTextWriter consoleOutput)
	{
		string text = ((!string.IsNullOrEmpty(Arguments.CompilationName)) ? Arguments.CompilationName : (Arguments.ProjectManifest?.AppManifest.AppName ?? "Compilation"));
		int num = ((Arguments != null && !(Arguments.SourceFiles == null)) ? Arguments.SourceFiles.Length : 0);
		consoleOutput.WriteLine(string.Format(CultureInfo.CurrentUICulture, CodeAnalysisResources.CompilationStarted, text, num, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond));
		consoleOutput.WriteLine();
	}

	protected virtual void PrintCompilationEnd(CompilerTextWriter consoleOutput)
	{
		consoleOutput.WriteLine();
		consoleOutput.WriteLine(string.Format(CultureInfo.CurrentUICulture, CodeAnalysisResources.CompilationEnded, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond));
		consoleOutput.WriteLine();
	}

	protected virtual AnalyzerCompilationContext CreateAnalyzerContext(IList<DiagnosticInfo> diagnostics, ImmutableArray<AdditionalText> additionalTextFiles, Compilation compilation, CancellationToken cancellationToken)
	{
		return AnalyzerCompilationContext.CreateAnalyzerContext(ResolveAnalyzersFromArguments(diagnostics, MessageProvider), diagnostics, additionalTextFiles, compilation, Arguments.ReportAnalyzer, cancellationToken);
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	protected virtual int RunCore(CompilerTextWriter consoleOutput, ErrorLogger errorLogger, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (Arguments.DisplayLogo)
		{
			PrintLogo(consoleOutput);
		}
		if (Arguments.DisplayHelp)
		{
			PrintHelp(consoleOutput);
			return 0;
		}
		if (ReportErrors(Arguments.Errors, consoleOutput, errorLogger))
		{
			return 1;
		}
		try
		{
			PrintCompilationStart(consoleOutput);
			Compilation compilation = GetOrCreateCompilation(consoleOutput, Arguments.CompilationOptions, errorLogger);
			if (compilation == null)
			{
				return 1;
			}
			List<DiagnosticInfo> diagnostics = new List<DiagnosticInfo>();
			ImmutableArray<AdditionalTextFile> immutableArray = ResolveAdditionalFilesFromArguments(diagnostics, MessageProvider);
			if (ReportErrors(diagnostics, consoleOutput, errorLogger))
			{
				return 1;
			}
			cancellationToken.ThrowIfCancellationRequested();
			AnalyzerCompilationContext analyzerCompilationContext = null;
			try
			{
				analyzerCompilationContext = CreateAnalyzerContext(diagnostics, ImmutableArray<AdditionalText>.CastUp(immutableArray), compilation, cancellationToken);
				compilation = analyzerCompilationContext.Compilation;
				if (ReportErrors(diagnostics, consoleOutput, errorLogger))
				{
					return 1;
				}
				return RunCoreImpl(consoleOutput, errorLogger, compilation, analyzerCompilationContext.AnalyzerDriver, analyzerCompilationContext.AnalyzerExceptionDiagnostics, immutableArray, cancellationToken);
			}
			finally
			{
				if (analyzerCompilationContext != null)
				{
					CleanAnalyzerCompilation(consoleOutput, analyzerCompilationContext.AnalyzerTokenSource, analyzerCompilationContext.AnalyzerDriver, analyzerCompilationContext.ReportAnalyzerDiagnostics, compilation.Options.ConcurrentBuild);
				}
			}
		}
		finally
		{
			PrintCompilationEnd(consoleOutput);
		}
	}

	protected virtual int RunCoreImpl(CompilerTextWriter consoleOutput, ErrorLogger errorLogger, Compilation compilation, AnalyzerDriver analyzerDriver, IEnumerable<Diagnostic> analyzerExceptionDiagnostics, IEnumerable<AdditionalTextFile> additionalTextFiles, CancellationToken cancellationToken)
	{
		if (ReportErrors(compilation.GetParseDiagnostics(), consoleOutput, errorLogger))
		{
			return 1;
		}
		cancellationToken.ThrowIfCancellationRequested();
		if (ReportErrors(compilation.GetDeclarationDiagnostics(), consoleOutput, errorLogger))
		{
			return 1;
		}
		cancellationToken.ThrowIfCancellationRequested();
		if (ReportErrors(analyzerExceptionDiagnostics, consoleOutput, errorLogger))
		{
			return 1;
		}
		cancellationToken.ThrowIfCancellationRequested();
		if (PackageTheModule(compilation, consoleOutput, cancellationToken, errorLogger, analyzerDriver) == 1)
		{
			return 1;
		}
		OutputLanguageFile(compilation, consoleOutput, errorLogger);
		cancellationToken.ThrowIfCancellationRequested();
		bool flag = false;
		foreach (AdditionalTextFile additionalTextFile in additionalTextFiles)
		{
			if (ReportErrors(additionalTextFile.Diagnostics, consoleOutput, errorLogger))
			{
				flag = true;
			}
		}
		if (flag)
		{
			return 1;
		}
		cancellationToken.ThrowIfCancellationRequested();
		return 0;
	}

	protected void CleanAnalyzerCompilation(CompilerTextWriter consoleOutput, CancellationTokenSource analyzerCts, AnalyzerDriver analyzerDriver, bool reportAnalyzer, bool isConcurrentBuild)
	{
		analyzerCts?.Cancel();
		analyzerDriver?.ClearAnalyzerState();
		if (reportAnalyzer)
		{
			ReportAnalyzerExecutionTime(consoleOutput, analyzerDriver, Culture, isConcurrentBuild);
		}
	}

	private void OutputLanguageFile(Compilation compilation, CompilerTextWriter consoleOutput, ErrorLogger errorLogger)
	{
		CompilerFeatures compilerFeatures = compilation.Options.CompilerFeatures;
		if (!compilerFeatures.ShouldGenerateTranslationFile())
		{
			return;
		}
		string extension = ((!compilerFeatures.ShouldGenerateLcgTranslationFile()) ? ".xlf" : ".lcg");
		string appName = Arguments.ProjectManifest?.AppManifest?.AppName ?? string.Empty;
		try
		{
			Stream languageFileStream = GetLanguageFileStream(extension);
			LanguageFileOutputter.OutputLanguageFile(compilation, languageFileStream, appName);
			DisposeStream(languageFileStream);
		}
		catch (UnauthorizedAccessException)
		{
			string translationFileName = LanguageFileUtilities.GetTranslationFileName(appName, extension);
			List<DiagnosticInfo> diagnostics = new List<DiagnosticInfo>();
			if (compilation.Options.ReadonlyErrorsAsWarnings)
			{
				diagnostics.Add(ErrorCode.WRN_FileIsReadonly, Location.None, translationFileName);
			}
			else
			{
				diagnostics.Add(ErrorCode.ERR_FileIsReadonly, Location.None, translationFileName);
			}
			ReportErrors(diagnostics, consoleOutput, errorLogger);
		}
	}

	protected virtual Stream GetLanguageFileStream(string extension)
	{
		string path = Path.Combine(Arguments.ProjectPath ?? string.Empty, "Translations");
		string translationFileName = LanguageFileUtilities.GetTranslationFileName(Arguments.ProjectManifest?.AppManifest.AppName, extension);
		string text = Path.Combine(path, translationFileName);
		if (!FileSystem.DirectoryExistsForFile(text))
		{
			FileSystem.CreateDirectoryForFile(text);
		}
		return FileSystem.OpenFile(text, FileMode.Create, FileAccess.ReadWrite);
	}

	protected virtual PackageModuleOutputterBase CreatePackageModuleOutputter(NavAppPackageWriter packageWriter, Compilation compilation)
	{
		EmitOptions options = CreateEmitOptions(compilation.Options);
		return new PackageModuleOutputter(ProjectManifest.AppManifest, packageWriter, compilation, options, Arguments.ProjectPath);
	}

	protected virtual EmitOptions CreateEmitOptions(CompilationOptions options)
	{
		return new EmitOptions(130000, null, skipStmtHit: false, null, options.CodeGenerationOptions == CodeGenerationOption.Ast, options.ConcurrentEmit, !options.ShowMyCode);
	}

	protected int PackageTheModule(Compilation compilation, CompilerTextWriter consoleOutput, CancellationToken cancellationToken, ErrorLogger errorLogger, AnalyzerDriver analyzerDriver, Predicate<ISymbol> filterOpt = null, Predicate<ISymbol> radCompilationFilter = null)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		Stream stream = null;
		try
		{
			string text = CreateOutputPath(compilation, consoleOutput, cancellationToken);
			if (string.IsNullOrEmpty(text))
			{
				return 1;
			}
			(NavAppPackageWriter, Stream) tuple = CreateNavAppPackageWriterAndStream(text, consoleOutput);
			if (tuple.Item2 == null)
			{
				return 1;
			}
			stream = tuple.Item2;
			var (packageWriter, _) = tuple;
			using (PackageModuleOutputterBase packageModuleOutputterBase = CreatePackageModuleOutputter(packageWriter, compilation))
			{
				packageModuleOutputterBase.DiagnosticBag = instance;
				ModuleBuilder moduleBuilder = Compilation.CreateModuleBuilder(packageModuleOutputterBase.EmitOptions, instance, compilation.Options);
				if (moduleBuilder == null)
				{
					ReportErrors(instance.AsEnumerable(), consoleOutput, errorLogger);
					return 1;
				}
				if (!compilation.Compile(moduleBuilder, emittingPdb: false, instance, filterOpt, radCompilationFilter, cancellationToken))
				{
					ReportErrors(instance.AsEnumerable(), consoleOutput, errorLogger);
					return 1;
				}
				compilation.CompleteTrees(null);
				if (analyzerDriver != null && radCompilationFilter == null)
				{
					ImmutableArray<Diagnostic> result = analyzerDriver.GetDiagnosticsAsync(compilation).Result;
					if (ReportErrors(result, consoleOutput, errorLogger))
					{
						return 1;
					}
				}
				Compilation.WriteToOutput(packageModuleOutputterBase, moduleBuilder, compilation.CompiledModule, instance);
				if (ReportErrors(instance.AsEnumerable(), consoleOutput, errorLogger))
				{
					return 1;
				}
			}
			return 0;
		}
		finally
		{
			instance.Free();
			DisposeStream(stream);
		}
	}

	protected virtual string CreateOutputPath(Compilation compilation, CompilerTextWriter consoleOutput, CancellationToken cancellationToken)
	{
		string outputFileName = GetOutputFileName(compilation, cancellationToken);
		string text = Path.Combine(Arguments.OutputDirectory, outputFileName);
		string directoryName = Path.GetDirectoryName(text);
		if (directoryName != null)
		{
			try
			{
				Directory.CreateDirectory(directoryName);
			}
			catch (IOException ex)
			{
				consoleOutput.WriteLine(ex.Message);
				return null;
			}
		}
		return text;
	}

	protected internal virtual (NavAppPackageWriter Writer, Stream PackageStream) CreateNavAppPackageWriterAndStream(string outputFilePath, CompilerTextWriter consoleOutput)
	{
		Stream stream = OpenFile(outputFilePath, consoleOutput, FileMode.Create);
		if (stream == null)
		{
			return (Writer: null, PackageStream: null);
		}
		return (Writer: NavAppPackageWriter.Create(stream), PackageStream: stream);
	}

	protected virtual void DisposeStream(Stream stream)
	{
		stream?.Dispose();
	}

	protected virtual ImmutableArray<AdditionalTextFile> ResolveAdditionalFilesFromArguments(List<DiagnosticInfo> diagnostics, CommonMessageProvider messageProvider)
	{
		ImmutableArray<AdditionalTextFile>.Builder builder = ImmutableArray.CreateBuilder<AdditionalTextFile>();
		foreach (CommandLineSourceFile additionalFile in Arguments.AdditionalFiles)
		{
			builder.Add(new AdditionalTextFile(additionalFile, this));
		}
		return builder.ToImmutable();
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	private static void ReportAnalyzerExecutionTime(CompilerTextWriter consoleOutput, AnalyzerDriver analyzerDriver, CultureInfo culture, bool isConcurrentBuild)
	{
		DebugAssertHelper.Assert(analyzerDriver.AnalyzerExecutionTimes != null);
		if (analyzerDriver.AnalyzerExecutionTimes.IsEmpty)
		{
			return;
		}
		double num = analyzerDriver.AnalyzerExecutionTimes.Sum((KeyValuePair<DiagnosticAnalyzer, TimeSpan> kvp) => kvp.Value.TotalSeconds);
		Func<double, string> func = (double d) => d.ToString("##0.000", culture);
		consoleOutput.WriteLine();
		consoleOutput.WriteLine(CodeAnalysisResources.AnalyzerTotalExecutionTime, func(num));
		if (isConcurrentBuild)
		{
			consoleOutput.WriteLine(CodeAnalysisResources.MultithreadedAnalyzerExecutionNote);
		}
		IOrderedEnumerable<IGrouping<Assembly, KeyValuePair<DiagnosticAnalyzer, TimeSpan>>> orderedEnumerable = from kvp in analyzerDriver.AnalyzerExecutionTimes
			group kvp by kvp.Key.GetType().GetTypeInfo().Assembly into kvp
			orderby kvp.Sum((KeyValuePair<DiagnosticAnalyzer, TimeSpan> entry) => entry.Value.Ticks) descending
			select kvp;
		consoleOutput.WriteLine();
		func = (double d) => (!(d < 0.001)) ? string.Format(culture, "{0,8:##0.000}", d) : string.Format(culture, "{0,8:<0.000}", 0.001);
		Func<int, string> func2 = (int i) => string.Format(CultureInfo.CurrentCulture, "{0,5}", (i < 1) ? "<1" : i.ToString(CultureInfo.CurrentCulture));
		Func<string, string> func3 = (string s) => "   " + s;
		string text = string.Format(CultureInfo.CurrentUICulture, "{0,8}", CodeAnalysisResources.AnalyzerExecutionTimeColumnHeader);
		string text2 = string.Format(CultureInfo.CurrentUICulture, "{0,5}", "%");
		string text3 = func3(CodeAnalysisResources.AnalyzerNameColumnHeader);
		consoleOutput.WriteLine(text + text2 + text3);
		foreach (IGrouping<Assembly, KeyValuePair<DiagnosticAnalyzer, TimeSpan>> item in orderedEnumerable)
		{
			double num2 = item.Sum((KeyValuePair<DiagnosticAnalyzer, TimeSpan> kvp) => kvp.Value.TotalSeconds);
			int arg = (int)(num2 * 100.0 / num);
			text = func(num2);
			text2 = func2(arg);
			text3 = func3(item.Key.FullName);
			consoleOutput.WriteLine(text + text2 + text3);
			foreach (KeyValuePair<DiagnosticAnalyzer, TimeSpan> item2 in item.OrderByDescending((KeyValuePair<DiagnosticAnalyzer, TimeSpan> kvp) => kvp.Value))
			{
				num2 = item2.Value.TotalSeconds;
				arg = (int)(num2 * 100.0 / num);
				text = func(num2);
				text2 = func2(arg);
				text3 = func3("   " + item2.Key);
				consoleOutput.WriteLine(text + text2 + text3);
			}
			consoleOutput.WriteLine();
		}
	}

	protected virtual string GetOutputFileName(Compilation compilation, CancellationToken cancellationToken)
	{
		return Arguments.OutputFileName;
	}

	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	protected Stream OpenFile(string filePath, CompilerTextWriter consoleOutput, FileMode mode = FileMode.Open, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.None)
	{
		try
		{
			return FileSystem.OpenFile(filePath, mode, access, share);
		}
		catch (IOException ex)
		{
			if (consoleOutput != null)
			{
				DiagnosticInfo diagnosticInfo = new DiagnosticInfo(MessageProvider, 1028, filePath, ex.Message);
				consoleOutput.WriteLine(diagnosticInfo.ToString(Culture));
			}
			return null;
		}
		catch (Exception ex2)
		{
			if (consoleOutput != null)
			{
				DiagnosticInfo diagnosticInfo2 = new DiagnosticInfo(MessageProvider, 1012, filePath, ex2.Message);
				consoleOutput.WriteLine(diagnosticInfo2.ToString(Culture));
			}
			return null;
		}
	}

	internal static bool TryGetCompilerDiagnosticCode(string diagnosticId, string expectedPrefix, out uint code)
	{
		code = 0u;
		if (diagnosticId.StartsWith(expectedPrefix, StringComparison.Ordinal))
		{
			return uint.TryParse(diagnosticId.Substring(expectedPrefix.Length), out code);
		}
		return false;
	}
}
