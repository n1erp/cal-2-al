using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal abstract class CommonCompiler
{
	private sealed class CompilerEmitStreamProvider : Compilation.EmitStreamProvider
	{
		private readonly CommonCompiler _compiler;

		private readonly string _filePath;

		private Stream _streamToDispose;

		public override Stream Stream => null;

		internal CompilerEmitStreamProvider(CommonCompiler compiler, string filePath)
		{
			_compiler = compiler;
			_filePath = filePath;
		}

		public void Close(DiagnosticBag diagnostics)
		{
			try
			{
				_streamToDispose?.Dispose();
			}
			catch (Exception ex)
			{
				CommonMessageProvider messageProvider = _compiler.MessageProvider;
				DiagnosticInfo info = new DiagnosticInfo(messageProvider, messageProvider.ERR_OutputWriteFailed, _filePath, ex.Message);
				diagnostics.Add(messageProvider.CreateDiagnostic(info));
			}
		}

		public override Stream CreateStream(DiagnosticBag diagnostics)
		{
			try
			{
				try
				{
					return OpenFileStream();
				}
				catch (IOException ex)
				{
					try
					{
						if (PathUtilities.IsUnixLikePlatform)
						{
							File.Delete(_filePath);
						}
						else if (ex.HResult == -2147024864)
						{
							string text = Path.Combine(Path.GetDirectoryName(_filePath), Guid.NewGuid().ToString() + "_" + Path.GetFileName(_filePath));
							File.Move(_filePath, text);
							File.SetAttributes(text, FileAttributes.Hidden);
							File.Delete(text);
						}
					}
					catch
					{
						ReportOpenFileDiagnostic(diagnostics, ex);
						return null;
					}
					return OpenFileStream();
				}
			}
			catch (Exception e)
			{
				ReportOpenFileDiagnostic(diagnostics, e);
				return null;
			}
		}

		private Stream OpenFileStream()
		{
			return _streamToDispose = _compiler.FileOpen(_filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
		}

		private void ReportOpenFileDiagnostic(DiagnosticBag diagnostics, Exception e)
		{
			CommonMessageProvider messageProvider = _compiler.MessageProvider;
			diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_CantOpenFileWrite, Location.None, _filePath, e.Message));
		}
	}

	internal sealed class ExistingReferencesResolver : MetadataReferenceResolver, IEquatable<ExistingReferencesResolver>
	{
		private readonly MetadataReferenceResolver _resolver;

		private readonly ImmutableArray<MetadataReference> _availableReferences;

		private readonly Lazy<HashSet<AssemblyIdentity>> _lazyAvailableReferences;

		public ExistingReferencesResolver(MetadataReferenceResolver resolver, ImmutableArray<MetadataReference> availableReferences)
		{
			_resolver = resolver;
			_availableReferences = availableReferences;
			_lazyAvailableReferences = new Lazy<HashSet<AssemblyIdentity>>(() => new HashSet<AssemblyIdentity>(from reference in _availableReferences
				let identity = TryGetIdentity(reference)
				where identity != null
				select identity));
		}

		public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string baseFilePath, MetadataReferenceProperties properties)
		{
			return _resolver.ResolveReference(reference, baseFilePath, properties).WhereAsArray((PortableExecutableReference r) => _lazyAvailableReferences.Value.Contains(TryGetIdentity(r)));
		}

		private static AssemblyIdentity TryGetIdentity(MetadataReference metadataReference)
		{
			if (!(metadataReference is PortableExecutableReference { Properties: { Kind: MetadataImageKind.Assembly } } portableExecutableReference))
			{
				return null;
			}
			try
			{
				return ((AssemblyMetadata)portableExecutableReference.GetMetadataNoCopy()).GetAssembly().Identity;
			}
			catch (Exception ex) when (ex is BadImageFormatException || ex is IOException)
			{
				return null;
			}
		}

		public override int GetHashCode()
		{
			return _resolver.GetHashCode();
		}

		public bool Equals(ExistingReferencesResolver other)
		{
			if (_resolver.Equals(other._resolver))
			{
				return _availableReferences.SequenceEqual(other._availableReferences);
			}
			return false;
		}

		public override bool Equals(object other)
		{
			return Equals(other as ExistingReferencesResolver);
		}
	}

	internal sealed class LoggingMetadataFileReferenceResolver : MetadataReferenceResolver, IEquatable<LoggingMetadataFileReferenceResolver>
	{
		private readonly TouchedFileLogger _loggerOpt;

		private readonly RelativePathResolver _pathResolver;

		private readonly Func<string, MetadataReferenceProperties, PortableExecutableReference> _provider;

		public LoggingMetadataFileReferenceResolver(RelativePathResolver pathResolver, Func<string, MetadataReferenceProperties, PortableExecutableReference> provider, TouchedFileLogger loggerOpt)
		{
			_pathResolver = pathResolver;
			_provider = provider;
			_loggerOpt = loggerOpt;
		}

		public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string baseFilePath, MetadataReferenceProperties properties)
		{
			string text = _pathResolver.ResolvePath(reference, baseFilePath);
			if (text != null)
			{
				_loggerOpt?.AddRead(text);
				return ImmutableArray.Create(_provider(text, properties));
			}
			return ImmutableArray<PortableExecutableReference>.Empty;
		}

		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}

		public bool Equals(LoggingMetadataFileReferenceResolver other)
		{
			throw new NotImplementedException();
		}

		public override bool Equals(object other)
		{
			return Equals(other as LoggingMetadataFileReferenceResolver);
		}
	}

	internal sealed class LoggingSourceFileResolver : SourceFileResolver
	{
		private readonly TouchedFileLogger _loggerOpt;

		public LoggingSourceFileResolver(ImmutableArray<string> searchPaths, string baseDirectory, ImmutableArray<KeyValuePair<string, string>> pathMap, TouchedFileLogger logger)
			: base(searchPaths, baseDirectory, pathMap)
		{
			_loggerOpt = logger;
		}

		protected override bool FileExists(string fullPath)
		{
			if (fullPath != null)
			{
				_loggerOpt?.AddRead(fullPath);
			}
			return base.FileExists(fullPath);
		}

		public LoggingSourceFileResolver WithBaseDirectory(string value)
		{
			if (!(BaseDirectory == value))
			{
				return new LoggingSourceFileResolver(SearchPaths, value, PathMap, _loggerOpt);
			}
			return this;
		}

		public LoggingSourceFileResolver WithSearchPaths(ImmutableArray<string> value)
		{
			if (!(SearchPaths == value))
			{
				return new LoggingSourceFileResolver(value, BaseDirectory, PathMap, _loggerOpt);
			}
			return this;
		}
	}

	internal sealed class LoggingStrongNameFileSystem : StrongNameFileSystem
	{
		private readonly TouchedFileLogger _loggerOpt;

		public LoggingStrongNameFileSystem(TouchedFileLogger logger)
		{
			_loggerOpt = logger;
		}

		internal override bool FileExists(string fullPath)
		{
			if (fullPath != null)
			{
				_loggerOpt?.AddRead(fullPath);
			}
			return base.FileExists(fullPath);
		}

		internal override byte[] ReadAllBytes(string fullPath)
		{
			_loggerOpt?.AddRead(fullPath);
			return base.ReadAllBytes(fullPath);
		}
	}

	internal sealed class LoggingXmlFileResolver : XmlFileResolver
	{
		private readonly TouchedFileLogger _loggerOpt;

		public LoggingXmlFileResolver(string baseDirectory, TouchedFileLogger logger)
			: base(baseDirectory)
		{
			_loggerOpt = logger;
		}

		protected override bool FileExists(string fullPath)
		{
			if (fullPath != null)
			{
				_loggerOpt?.AddRead(fullPath);
			}
			return base.FileExists(fullPath);
		}
	}

	internal const int Failed = 1;

	internal const int Succeeded = 0;

	private readonly string _clientDirectory;

	private readonly HashSet<Diagnostic> _reportedDiagnostics = new HashSet<Diagnostic>();

	private Func<string, FileMode, FileAccess, FileShare, Stream> _fileOpen;

	public CommonMessageProvider MessageProvider { get; }

	public CommandLineArguments Arguments { get; }

	public IAnalyzerAssemblyLoader AssemblyLoader { get; private set; }

	public abstract DiagnosticFormatter DiagnosticFormatter { get; }

	public Roslyn.Utilities.IReadOnlySet<string> EmbeddedSourcePaths { get; }

	internal abstract Type Type { get; }

	internal Func<string, FileMode, FileAccess, FileShare, Stream> FileOpen
	{
		get
		{
			return _fileOpen ?? ((Func<string, FileMode, FileAccess, FileShare, Stream>)((string path, FileMode mode, FileAccess access, FileShare share) => new FileStream(path, mode, access, share)));
		}
		set
		{
			_fileOpen = value;
		}
	}

	protected virtual CultureInfo Culture => Arguments.PreferredUILang ?? CultureInfo.CurrentUICulture;

	public abstract Compilation CreateCompilation(TextWriter consoleOutput, TouchedFileLogger touchedFilesLogger, ErrorLogger errorLoggerOpt);

	public abstract void PrintLogo(TextWriter consoleOutput);

	public abstract void PrintHelp(TextWriter consoleOutput);

	public abstract void PrintLangVersions(TextWriter consoleOutput);

	public virtual void PrintVersion(TextWriter consoleOutput)
	{
		consoleOutput.WriteLine(GetAssemblyFileVersion());
	}

	protected abstract bool TryGetCompilerDiagnosticCode(string diagnosticId, out uint code);

	protected abstract ImmutableArray<DiagnosticAnalyzer> ResolveAnalyzersFromArguments(List<DiagnosticInfo> diagnostics, CommonMessageProvider messageProvider);

	public CommonCompiler(CommandLineParser parser, string responseFile, string[] args, BuildPaths buildPaths, string additionalReferenceDirectories, IAnalyzerAssemblyLoader assemblyLoader)
	{
		IEnumerable<string> enumerable = args;
		_clientDirectory = buildPaths.ClientDirectory;
		if (!SuppressDefaultResponseFile(args) && File.Exists(responseFile))
		{
			enumerable = new string[1] { "@" + responseFile }.Concat(enumerable);
		}
		Arguments = parser.Parse(enumerable, buildPaths.WorkingDirectory, buildPaths.SdkDirectory, additionalReferenceDirectories);
		MessageProvider = parser.MessageProvider;
		AssemblyLoader = assemblyLoader;
		EmbeddedSourcePaths = GetEmbeddedSourcePaths(Arguments);
		if (Arguments.ParseOptions.Features.ContainsKey("debug-determinism"))
		{
			EmitDeterminismKey(Arguments, args, buildPaths.WorkingDirectory, parser);
		}
	}

	internal abstract bool SuppressDefaultResponseFile(IEnumerable<string> args);

	internal virtual string GetAssemblyFileVersion()
	{
		return GetAssemblyFileVersion(Type.GetTypeInfo().Assembly);
	}

	internal static string GetAssemblyFileVersion(Assembly assembly)
	{
		string text = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
		string text2 = ExtractShortCommitHash(assembly.GetCustomAttribute<CommitHashAttribute>()?.Hash);
		return text + " (" + text2 + ")";
	}

	internal static string ExtractShortCommitHash(string hash)
	{
		if (hash != null && hash.Length >= 8 && hash[0] != '<')
		{
			return hash.Substring(0, 8);
		}
		return hash;
	}

	internal abstract string GetToolName();

	internal Version GetAssemblyVersion()
	{
		return Type.GetTypeInfo().Assembly.GetName().Version;
	}

	internal string GetCultureName()
	{
		return Culture.Name;
	}

	internal virtual Func<string, MetadataReferenceProperties, PortableExecutableReference> GetMetadataProvider()
	{
		return (string path, MetadataReferenceProperties properties) => MetadataReference.CreateFromFile(path, properties);
	}

	internal virtual MetadataReferenceResolver GetCommandLineMetadataReferenceResolver(TouchedFileLogger loggerOpt)
	{
		return new LoggingMetadataFileReferenceResolver(new RelativePathResolver(Arguments.ReferencePaths, Arguments.BaseDirectory), GetMetadataProvider(), loggerOpt);
	}

	internal List<MetadataReference> ResolveMetadataReferences(List<DiagnosticInfo> diagnostics, TouchedFileLogger touchedFiles, out MetadataReferenceResolver referenceDirectiveResolver)
	{
		MetadataReferenceResolver commandLineMetadataReferenceResolver = GetCommandLineMetadataReferenceResolver(touchedFiles);
		List<MetadataReference> list = new List<MetadataReference>();
		Arguments.ResolveMetadataReferences(commandLineMetadataReferenceResolver, diagnostics, MessageProvider, list);
		if (Arguments.IsScriptRunner)
		{
			referenceDirectiveResolver = commandLineMetadataReferenceResolver;
		}
		else
		{
			referenceDirectiveResolver = new ExistingReferencesResolver(commandLineMetadataReferenceResolver, list.ToImmutableArray());
		}
		return list;
	}

	internal SourceText TryReadFileContent(CommandLineSourceFile file, IList<DiagnosticInfo> diagnostics)
	{
		string normalizedFilePath;
		return TryReadFileContent(file, diagnostics, out normalizedFilePath);
	}

	internal SourceText TryReadFileContent(CommandLineSourceFile file, IList<DiagnosticInfo> diagnostics, out string normalizedFilePath)
	{
		string path = file.Path;
		try
		{
			using FileStream fileStream = OpenFileForReadWithSmallBufferOptimization(path);
			normalizedFilePath = fileStream.Name;
			return EncodedStringText.Create(fileStream, Arguments.Encoding, Arguments.ChecksumAlgorithm, EmbeddedSourcePaths.Contains(file.Path));
		}
		catch (Exception e)
		{
			diagnostics.Add(ToFileReadDiagnostics(MessageProvider, e, path));
			normalizedFilePath = null;
			return null;
		}
	}

	private static FileStream OpenFileForReadWithSmallBufferOptimization(string filePath)
	{
		return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1, FileOptions.None);
	}

	internal EmbeddedText TryReadEmbeddedFileContent(string filePath, DiagnosticBag diagnostics)
	{
		try
		{
			using FileStream fileStream = OpenFileForReadWithSmallBufferOptimization(filePath);
			if (fileStream.Length < 81920 && EncodedStringText.TryGetBytesFromStream(fileStream, out var bytes))
			{
				return EmbeddedText.FromBytes(filePath, bytes, Arguments.ChecksumAlgorithm);
			}
			return EmbeddedText.FromStream(filePath, fileStream, Arguments.ChecksumAlgorithm);
		}
		catch (Exception e)
		{
			diagnostics.Add(MessageProvider.CreateDiagnostic(ToFileReadDiagnostics(MessageProvider, e, filePath)));
			return null;
		}
	}

	private ImmutableArray<EmbeddedText> AcquireEmbeddedTexts(Compilation compilation, DiagnosticBag diagnostics)
	{
		if (Arguments.EmbeddedFiles.IsEmpty)
		{
			return ImmutableArray<EmbeddedText>.Empty;
		}
		Dictionary<string, SyntaxTree> dictionary = new Dictionary<string, SyntaxTree>(Arguments.EmbeddedFiles.Length);
		OrderedSet<string> orderedSet = new OrderedSet<string>(Arguments.EmbeddedFiles.Select((CommandLineSourceFile e) => e.Path));
		foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
		{
			if (EmbeddedSourcePaths.Contains(syntaxTree.FilePath) && !dictionary.ContainsKey(syntaxTree.FilePath))
			{
				dictionary.Add(syntaxTree.FilePath, syntaxTree);
				ResolveEmbeddedFilesFromExternalSourceDirectives(syntaxTree, compilation.Options.SourceReferenceResolver, orderedSet, diagnostics);
			}
		}
		ImmutableArray<EmbeddedText>.Builder builder = ImmutableArray.CreateBuilder<EmbeddedText>(orderedSet.Count);
		foreach (string item2 in orderedSet)
		{
			EmbeddedText item = ((!dictionary.TryGetValue(item2, out var value)) ? TryReadEmbeddedFileContent(item2, diagnostics) : EmbeddedText.FromSource(item2, value.GetText()));
			builder.Add(item);
		}
		return builder.MoveToImmutable();
	}

	protected abstract void ResolveEmbeddedFilesFromExternalSourceDirectives(SyntaxTree tree, SourceReferenceResolver resolver, OrderedSet<string> embeddedFiles, DiagnosticBag diagnostics);

	private static Roslyn.Utilities.IReadOnlySet<string> GetEmbeddedSourcePaths(CommandLineArguments arguments)
	{
		if (arguments.EmbeddedFiles.IsEmpty)
		{
			return SpecializedCollections.EmptyReadOnlySet<string>();
		}
		HashSet<string> hashSet = new HashSet<string>(arguments.EmbeddedFiles.Select((CommandLineSourceFile f) => f.Path));
		hashSet.IntersectWith(arguments.SourceFiles.Select((CommandLineSourceFile f) => f.Path));
		return SpecializedCollections.StronglyTypedReadOnlySet(hashSet);
	}

	internal static DiagnosticInfo ToFileReadDiagnostics(CommonMessageProvider messageProvider, Exception e, string filePath)
	{
		if (e is FileNotFoundException || ((object)e).GetType().Name == "DirectoryNotFoundException")
		{
			return new DiagnosticInfo(messageProvider, messageProvider.ERR_FileNotFound, filePath);
		}
		if (e is InvalidDataException)
		{
			return new DiagnosticInfo(messageProvider, messageProvider.ERR_BinaryFile, filePath);
		}
		return new DiagnosticInfo(messageProvider, messageProvider.ERR_NoSourceFile, filePath, e.Message);
	}

	internal bool ReportErrors(IEnumerable<Diagnostic> diagnostics, TextWriter consoleOutput, ErrorLogger errorLoggerOpt)
	{
		bool result = false;
		foreach (Diagnostic diagnostic in diagnostics)
		{
			if (_reportedDiagnostics.Contains(diagnostic) || diagnostic.Severity == DiagnosticSeverity.Hidden)
			{
				continue;
			}
			errorLoggerOpt?.LogDiagnostic(diagnostic);
			if (!diagnostic.IsSuppressed)
			{
				if (diagnostic.Severity == DiagnosticSeverity.Error)
				{
					result = true;
				}
				PrintError(diagnostic, consoleOutput);
				_reportedDiagnostics.Add(diagnostic);
			}
		}
		return result;
	}

	private bool ReportErrors(DiagnosticBag diagnostics, TextWriter consoleOutput, ErrorLogger errorLoggerOpt)
	{
		return ReportErrors(diagnostics.ToReadOnly(), consoleOutput, errorLoggerOpt);
	}

	private static bool IsReportedError(Diagnostic diagnostic)
	{
		if (diagnostic.Severity == DiagnosticSeverity.Error)
		{
			return !diagnostic.IsSuppressed;
		}
		return false;
	}

	public bool ReportErrors(IEnumerable<DiagnosticInfo> diagnostics, TextWriter consoleOutput, ErrorLogger errorLoggerOpt)
	{
		return ReportErrors(diagnostics.Select((DiagnosticInfo info) => Diagnostic.Create(info)), consoleOutput, errorLoggerOpt);
	}

	protected virtual void PrintError(Diagnostic diagnostic, TextWriter consoleOutput)
	{
		consoleOutput.WriteLine(DiagnosticFormatter.Format(diagnostic, Culture));
	}

	public StreamErrorLogger GetErrorLogger(TextWriter consoleOutput, CancellationToken cancellationToken)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		Stream stream = OpenFile(Arguments.ErrorLogPath, instance, FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
		StreamErrorLogger streamErrorLogger = ((stream != null) ? new StreamErrorLogger(stream, GetToolName(), GetAssemblyFileVersion(), GetAssemblyVersion(), Culture) : null);
		ReportErrors(instance.ToReadOnlyAndFree(), consoleOutput, streamErrorLogger);
		return streamErrorLogger;
	}

	public virtual int Run(TextWriter consoleOutput, CancellationToken cancellationToken = default(CancellationToken))
	{
		CultureInfo currentUICulture = CultureInfo.CurrentUICulture;
		StreamErrorLogger streamErrorLogger = null;
		try
		{
			CultureInfo culture = Culture;
			if (culture != null)
			{
				CultureInfo.CurrentUICulture = culture;
			}
			if (Arguments.ErrorLogPath != null)
			{
				streamErrorLogger = GetErrorLogger(consoleOutput, cancellationToken);
				if (streamErrorLogger == null)
				{
					return 1;
				}
			}
			return RunCore(consoleOutput, streamErrorLogger, cancellationToken);
		}
		catch (OperationCanceledException)
		{
			int eRR_CompileCancelled = MessageProvider.ERR_CompileCancelled;
			if (eRR_CompileCancelled > 0)
			{
				DiagnosticInfo diagnosticInfo = new DiagnosticInfo(MessageProvider, eRR_CompileCancelled);
				ReportErrors(new DiagnosticInfo[1] { diagnosticInfo }, consoleOutput, streamErrorLogger);
			}
			return 1;
		}
		finally
		{
			CultureInfo.CurrentUICulture = currentUICulture;
			streamErrorLogger?.Dispose();
		}
	}

	private int RunCore(TextWriter consoleOutput, ErrorLogger errorLogger, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (Arguments.DisplayVersion)
		{
			PrintVersion(consoleOutput);
			return 0;
		}
		if (Arguments.DisplayLangVersions)
		{
			PrintLangVersions(consoleOutput);
			return 0;
		}
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
		TouchedFileLogger touchedFilesLogger = ((Arguments.TouchedFilesPath != null) ? new TouchedFileLogger() : null);
		Compilation compilation = CreateCompilation(consoleOutput, touchedFilesLogger, errorLogger);
		if (compilation == null)
		{
			return 1;
		}
		List<DiagnosticInfo> diagnostics = new List<DiagnosticInfo>();
		ImmutableArray<DiagnosticAnalyzer> analyzers = ResolveAnalyzersFromArguments(diagnostics, MessageProvider);
		ImmutableArray<AdditionalTextFile> additionalTextFiles = ResolveAdditionalFilesFromArguments(diagnostics, MessageProvider, touchedFilesLogger);
		if (ReportErrors(diagnostics, consoleOutput, errorLogger))
		{
			return 1;
		}
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		ImmutableArray<EmbeddedText> embeddedTexts = AcquireEmbeddedTexts(compilation, instance);
		if (ReportErrors(instance, consoleOutput, errorLogger))
		{
			return 1;
		}
		CompileAndEmit(touchedFilesLogger, ref compilation, analyzers, additionalTextFiles, embeddedTexts, instance, cancellationToken, out var analyzerCts, out var reportAnalyzer, out var analyzerDriver);
		analyzerCts?.Cancel();
		int result = (ReportErrors(instance, consoleOutput, errorLogger) ? 1 : 0);
		foreach (AdditionalTextFile item in additionalTextFiles)
		{
			if (ReportErrors(item.Diagnostics, consoleOutput, errorLogger))
			{
				result = 1;
			}
		}
		instance.Free();
		if (reportAnalyzer)
		{
			ReportAnalyzerExecutionTime(consoleOutput, analyzerDriver, Culture, compilation.Options.ConcurrentBuild);
		}
		return result;
	}

	private void CompileAndEmit(TouchedFileLogger touchedFilesLogger, ref Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, ImmutableArray<AdditionalTextFile> additionalTextFiles, ImmutableArray<EmbeddedText> embeddedTexts, DiagnosticBag diagnostics, CancellationToken cancellationToken, out CancellationTokenSource analyzerCts, out bool reportAnalyzer, out AnalyzerDriver analyzerDriver)
	{
		analyzerCts = null;
		reportAnalyzer = false;
		analyzerDriver = null;
		compilation.GetDiagnostics(CompilationStage.Parse, includeEarlierStages: false, diagnostics, cancellationToken);
		if (diagnostics.HasAnyErrors())
		{
			return;
		}
		DiagnosticBag diagnosticBag = null;
		if (!analyzers.IsEmpty)
		{
			analyzerCts = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken[1] { cancellationToken });
			diagnosticBag = new DiagnosticBag();
			AnalyzerOptions options = new AnalyzerOptions(ImmutableArray<AdditionalText>.CastUp(additionalTextFiles));
			analyzerDriver = AnalyzerDriver.CreateAndAttachToCompilation(compilation, analyzers, options, new AnalyzerManager(analyzers), diagnosticBag.Add, Arguments.ReportAnalyzer, out compilation, analyzerCts.Token);
			reportAnalyzer = Arguments.ReportAnalyzer && !analyzers.IsEmpty;
		}
		compilation.GetDiagnostics(CompilationStage.Declare, includeEarlierStages: false, diagnostics, cancellationToken);
		if (diagnostics.HasAnyErrors())
		{
			return;
		}
		cancellationToken.ThrowIfCancellationRequested();
		string outputFileName = GetOutputFileName(compilation, cancellationToken);
		string text = Path.Combine(Arguments.OutputDirectory, outputFileName);
		string text2 = Arguments.PdbPath ?? Path.ChangeExtension(text, ".pdb");
		string documentationPath = Arguments.DocumentationPath;
		NoThrowStreamDisposer noThrowStreamDisposer = null;
		try
		{
			EmitOptions emitOptions = Arguments.EmitOptions.WithOutputNameOverride(outputFileName).WithPdbFilePath(PathUtilities.NormalizePathPrefix(text2, Arguments.PathMap));
			if (Arguments.ParseOptions.Features.ContainsKey("pdb-path-determinism") && !string.IsNullOrEmpty(emitOptions.PdbFilePath))
			{
				emitOptions = emitOptions.WithPdbFilePath(Path.GetFileName(emitOptions.PdbFilePath));
			}
			if (Arguments.SourceLink != null)
			{
				Stream stream = OpenFile(Arguments.SourceLink, diagnostics, FileMode.Open, FileAccess.Read, FileShare.Read);
				if (stream != null)
				{
					noThrowStreamDisposer = new NoThrowStreamDisposer(stream, Arguments.SourceLink, diagnostics, MessageProvider);
				}
			}
			CommonPEModuleBuilder commonPEModuleBuilder = compilation.CheckOptionsAndCreateModuleBuilder(diagnostics, Arguments.ManifestResources, emitOptions, null, noThrowStreamDisposer?.Stream, embeddedTexts, null, cancellationToken);
			if (commonPEModuleBuilder != null)
			{
				bool flag;
				try
				{
					flag = compilation.CompileMethods(commonPEModuleBuilder, Arguments.EmitPdb, emitOptions.EmitMetadataOnly, emitOptions.EmitTestCoverageData, diagnostics, null, cancellationToken);
					if (flag)
					{
						NoThrowStreamDisposer noThrowStreamDisposer2 = null;
						if (documentationPath != null)
						{
							Stream stream2 = OpenFile(documentationPath, diagnostics, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
							if (stream2 == null)
							{
								return;
							}
							try
							{
								stream2.SetLength(0L);
							}
							catch (Exception e)
							{
								MessageProvider.ReportStreamWriteException(e, documentationPath, diagnostics);
								return;
							}
							noThrowStreamDisposer2 = new NoThrowStreamDisposer(stream2, documentationPath, diagnostics, MessageProvider);
						}
						using (noThrowStreamDisposer2)
						{
							using Stream win32ResourcesStream = GetWin32Resources(MessageProvider, Arguments, compilation, diagnostics);
							if (diagnostics.HasAnyErrors())
							{
								return;
							}
							flag = compilation.GenerateResourcesAndDocumentationComments(commonPEModuleBuilder, noThrowStreamDisposer2?.Stream, win32ResourcesStream, emitOptions.OutputNameOverride, diagnostics, cancellationToken);
						}
						if (noThrowStreamDisposer2 != null && noThrowStreamDisposer2.HasFailedToDispose)
						{
							return;
						}
						if (flag)
						{
							compilation.ReportUnusedImports(null, diagnostics, cancellationToken);
						}
					}
					compilation.CompleteTrees(null);
					if (analyzerDriver != null)
					{
						ImmutableArray<Diagnostic> result = analyzerDriver.GetDiagnosticsAsync(compilation).Result;
						diagnostics.AddRange(result);
						if (result.Any(IsReportedError))
						{
							flag = false;
						}
					}
				}
				finally
				{
					commonPEModuleBuilder.CompilationFinished();
				}
				if (flag)
				{
					bool num = Arguments.EmitPdb && emitOptions.DebugInformationFormat != DebugInformationFormat.Embedded;
					CompilerEmitStreamProvider compilerEmitStreamProvider = new CompilerEmitStreamProvider(this, text);
					CompilerEmitStreamProvider compilerEmitStreamProvider2 = (num ? new CompilerEmitStreamProvider(this, text2) : null);
					string outputRefFilePath = Arguments.OutputRefFilePath;
					CompilerEmitStreamProvider compilerEmitStreamProvider3 = ((outputRefFilePath != null) ? new CompilerEmitStreamProvider(this, outputRefFilePath) : null);
					RSAParameters? privateKeyOpt = null;
					StrongNameProvider strongNameProvider = compilation.Options.StrongNameProvider;
					if (strongNameProvider != null && strongNameProvider.Capability == SigningCapability.SignsPeBuilder && !compilation.Options.PublicSign)
					{
						privateKeyOpt = compilation.StrongNameKeys.PrivateKey;
					}
					flag = compilation.SerializeToPeStream(commonPEModuleBuilder, compilerEmitStreamProvider, compilerEmitStreamProvider3, compilerEmitStreamProvider2, null, diagnostics, emitOptions.EmitMetadataOnly, emitOptions.IncludePrivateMembers, emitOptions.EmitTestCoverageData, emitOptions.PdbFilePath, privateKeyOpt, cancellationToken);
					compilerEmitStreamProvider.Close(diagnostics);
					compilerEmitStreamProvider3?.Close(diagnostics);
					compilerEmitStreamProvider2?.Close(diagnostics);
					if (flag && touchedFilesLogger != null)
					{
						if (compilerEmitStreamProvider2 != null)
						{
							touchedFilesLogger.AddWritten(text2);
						}
						if (compilerEmitStreamProvider3 != null)
						{
							touchedFilesLogger.AddWritten(outputRefFilePath);
						}
						touchedFilesLogger.AddWritten(text);
					}
				}
			}
			if (diagnostics.HasAnyErrors())
			{
				return;
			}
		}
		finally
		{
			noThrowStreamDisposer?.Dispose();
		}
		if (noThrowStreamDisposer != null && noThrowStreamDisposer.HasFailedToDispose)
		{
			return;
		}
		cancellationToken.ThrowIfCancellationRequested();
		if (diagnosticBag != null)
		{
			diagnostics.AddRange(diagnosticBag);
			if (diagnosticBag.HasAnyErrors())
			{
				return;
			}
		}
		cancellationToken.ThrowIfCancellationRequested();
		WriteTouchedFiles(diagnostics, touchedFilesLogger, documentationPath);
	}

	private bool WriteTouchedFiles(DiagnosticBag diagnostics, TouchedFileLogger touchedFilesLogger, string finalXmlFilePath)
	{
		if (Arguments.TouchedFilesPath != null)
		{
			if (finalXmlFilePath != null)
			{
				touchedFilesLogger.AddWritten(finalXmlFilePath);
			}
			string text = Arguments.TouchedFilesPath + ".read";
			string text2 = Arguments.TouchedFilesPath + ".write";
			Stream stream = OpenFile(text, diagnostics, FileMode.OpenOrCreate);
			Stream stream2 = OpenFile(text2, diagnostics, FileMode.OpenOrCreate);
			if (stream == null || stream2 == null)
			{
				return false;
			}
			string filePath = null;
			try
			{
				filePath = text;
				using (StreamWriter s = new StreamWriter(stream))
				{
					touchedFilesLogger.WriteReadPaths(s);
				}
				filePath = text2;
				using StreamWriter s2 = new StreamWriter(stream2);
				touchedFilesLogger.WriteWrittenPaths(s2);
			}
			catch (Exception e)
			{
				MessageProvider.ReportStreamWriteException(e, filePath, diagnostics);
				return false;
			}
		}
		return true;
	}

	protected virtual ImmutableArray<AdditionalTextFile> ResolveAdditionalFilesFromArguments(List<DiagnosticInfo> diagnostics, CommonMessageProvider messageProvider, TouchedFileLogger touchedFilesLogger)
	{
		ImmutableArray<AdditionalTextFile>.Builder builder = ImmutableArray.CreateBuilder<AdditionalTextFile>();
		foreach (CommandLineSourceFile additionalFile in Arguments.AdditionalFiles)
		{
			builder.Add(new AdditionalTextFile(additionalFile, this));
		}
		return builder.ToImmutableArray();
	}

	private static void ReportAnalyzerExecutionTime(TextWriter consoleOutput, AnalyzerDriver analyzerDriver, CultureInfo culture, bool isConcurrentBuild)
	{
		if (analyzerDriver.AnalyzerExecutionTimes.IsEmpty)
		{
			return;
		}
		double num = analyzerDriver.AnalyzerExecutionTimes.Sum((KeyValuePair<DiagnosticAnalyzer, TimeSpan> kvp) => kvp.Value.TotalSeconds);
		Func<double, string> func = (double d) => d.ToString("##0.000", culture);
		consoleOutput.WriteLine();
		consoleOutput.WriteLine(string.Format(CodeAnalysisResources.AnalyzerTotalExecutionTime, func(num)));
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
		Func<int, string> func2 = (int i) => string.Format("{0,5}", (i < 1) ? "<1" : i.ToString());
		Func<string, string> func3 = (string s) => "   " + s;
		string text = $"{CodeAnalysisResources.AnalyzerExecutionTimeColumnHeader,8}";
		string text2 = string.Format("{0,5}", "%");
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
				text3 = func3("   " + item2.Key.ToString());
				consoleOutput.WriteLine(text + text2 + text3);
			}
			consoleOutput.WriteLine();
		}
	}

	protected virtual string GetOutputFileName(Compilation compilation, CancellationToken cancellationToken)
	{
		return Arguments.OutputFileName;
	}

	private Stream OpenFile(string filePath, DiagnosticBag diagnostics, FileMode mode = FileMode.Open, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.None)
	{
		try
		{
			return FileOpen(filePath, mode, access, share);
		}
		catch (Exception e)
		{
			MessageProvider.ReportStreamWriteException(e, filePath, diagnostics);
			return null;
		}
	}

	internal static Stream GetWin32ResourcesInternal(CommonMessageProvider messageProvider, CommandLineArguments arguments, Compilation compilation, out IEnumerable<DiagnosticInfo> errors)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		Stream win32Resources = GetWin32Resources(messageProvider, arguments, compilation, instance);
		errors = instance.ToReadOnlyAndFree().SelectAsArray((Diagnostic diag) => new DiagnosticInfo(messageProvider, diag.IsWarningAsError, diag.Code, (object[])diag.Arguments));
		return win32Resources;
	}

	private static Stream GetWin32Resources(CommonMessageProvider messageProvider, CommandLineArguments arguments, Compilation compilation, DiagnosticBag diagnostics)
	{
		if (arguments.Win32ResourceFile != null)
		{
			return OpenStream(messageProvider, arguments.Win32ResourceFile, arguments.BaseDirectory, messageProvider.ERR_CantOpenWin32Resource, diagnostics);
		}
		using (Stream manifestContents = OpenManifestStream(messageProvider, compilation.Options.OutputKind, arguments, diagnostics))
		{
			using Stream iconInIcoFormat = OpenStream(messageProvider, arguments.Win32Icon, arguments.BaseDirectory, messageProvider.ERR_CantOpenWin32Icon, diagnostics);
			try
			{
				return compilation.CreateDefaultWin32Resources(versionResource: true, arguments.NoWin32Manifest, manifestContents, iconInIcoFormat);
			}
			catch (Exception ex)
			{
				diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_ErrorBuildingWin32Resource, Location.None, ex.Message));
			}
		}
		return null;
	}

	private static Stream OpenManifestStream(CommonMessageProvider messageProvider, OutputKind outputKind, CommandLineArguments arguments, DiagnosticBag diagnostics)
	{
		if (!outputKind.IsNetModule())
		{
			return OpenStream(messageProvider, arguments.Win32Manifest, arguments.BaseDirectory, messageProvider.ERR_CantOpenWin32Manifest, diagnostics);
		}
		return null;
	}

	private static Stream OpenStream(CommonMessageProvider messageProvider, string path, string baseDirectory, int errorCode, DiagnosticBag diagnostics)
	{
		if (path == null)
		{
			return null;
		}
		string text = ResolveRelativePath(messageProvider, path, baseDirectory, diagnostics);
		if (text == null)
		{
			return null;
		}
		try
		{
			return new FileStream(text, FileMode.Open, FileAccess.Read);
		}
		catch (Exception ex)
		{
			diagnostics.Add(messageProvider.CreateDiagnostic(errorCode, Location.None, text, ex.Message));
		}
		return null;
	}

	private static string ResolveRelativePath(CommonMessageProvider messageProvider, string path, string baseDirectory, DiagnosticBag diagnostics)
	{
		string text = FileUtilities.ResolveRelativePath(path, baseDirectory);
		if (text == null)
		{
			diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.FTL_InvalidInputFileName, Location.None, path));
		}
		return text;
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

	private static void EmitDeterminismKey(CommandLineArguments args, string[] rawArgs, string baseDirectory, CommandLineParser parser)
	{
		string s = CreateDeterminismKey(args, rawArgs, baseDirectory, parser);
		using FileStream fileStream = File.Create(Path.Combine(args.OutputDirectory, args.OutputFileName + ".key"));
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		fileStream.Write(bytes, 0, bytes.Length);
	}

	private static string CreateDeterminismKey(CommandLineArguments args, string[] rawArgs, string baseDirectory, CommandLineParser parser)
	{
		List<Diagnostic> diagnostics = new List<Diagnostic>();
		List<string> list = new List<string>();
		parser.FlattenArgs(rawArgs, diagnostics, list, null, baseDirectory);
		StringBuilder stringBuilder = new StringBuilder();
		string text = ((!string.IsNullOrEmpty(args.OutputFileName)) ? Path.GetFileNameWithoutExtension(Path.GetFileName(args.OutputFileName)) : ("no-output-name-" + Guid.NewGuid()));
		stringBuilder.AppendLine(text ?? "");
		stringBuilder.AppendLine("Command Line:");
		foreach (string item in list)
		{
			stringBuilder.AppendLine("\t" + item);
		}
		stringBuilder.AppendLine("Source Files:");
		MD5 mD = MD5.Create();
		foreach (CommandLineSourceFile sourceFile in args.SourceFiles)
		{
			string fileName = Path.GetFileName(sourceFile.Path);
			string text2;
			try
			{
				byte[] buffer = File.ReadAllBytes(sourceFile.Path);
				text2 = BitConverter.ToString(mD.ComputeHash(buffer)).Replace("-", "");
			}
			catch (Exception ex)
			{
				text2 = "Could not compute " + ex.Message;
			}
			stringBuilder.AppendLine("\t" + fileName + " - " + text2);
		}
		return stringBuilder.ToString();
	}
}
