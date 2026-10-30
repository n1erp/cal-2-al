using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference.DotNet;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.IO.Txt;
using Microsoft.Dynamics.Nav.Txt2AlConverter;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.DotNet;

namespace Txt2Al;

internal static class Program
{
	private const int NumberOfConcurrentThreads = 4;

	private const string XliffFileExtension = ".xlf";

	private const string LcgFileExtension = ".lcg";

	public static void Main(string[] args)
	{
		AlExporterCommandLineOptions alExporterCommandLineOptions = new AlExporterCommandLineOptions();
		if (!Parser.Default.ParseArgumentsStrict(args, alExporterCommandLineOptions))
		{
			Console.WriteLine(alExporterCommandLineOptions.GetUsage());
			Environment.Exit(-1);
		}
		alExporterCommandLineOptions.Sanitize();
		try
		{
			ConvertFilesFromDirectory(alExporterCommandLineOptions);
			Environment.Exit(0);
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine(ex.Message);
			Environment.Exit(-1);
		}
	}

	private static void ConvertFilesFromDirectory(AlExporterCommandLineOptions options)
	{
		options.Sanitize();
		long ticks = DateTime.Now.Ticks;
		ConcurrentQueue<Exception> errors = new ConcurrentQueue<Exception>();
		IEnumerable<string> txtFiles = Directory.EnumerateFiles(options.SourceDirectory, "*.txt", SearchOption.AllDirectories).Concat(Directory.EnumerateFiles(options.SourceDirectory, "*.delta", SearchOption.AllDirectories));
		try
		{
			ExportManifestFile(options);
			AlExportOptions exporterOptions = new AlExportOptions(options.ExtensionStartId, 4, options.FallbackTypeNameResolution, GetDotNetInjector(options), options.AddLegacyTranslationInformation, options.TranslationFileFormat, options.GetParsedRuntimeVersion(), dataClassificationDefauling: false, options.TableDataOnly);
			IDotNetDeclarationExtractor dotNetExtractor = new DotNetDeclarationExtractor(options.DotNetTypePrefix);
			ITranslationExtractor translationExtractor = new TranslationExtractor(exporterOptions);
			IEnumDeclarationExtractor enumDeclarationExtractor = new EnumDeclarationExtractor();
			ParallelQuery<Tuple<string, ApplicationObject>> source = ParallelExportQuery(txtFiles, options, errors);
			FileNameExporter fileNameExporter = new FileNameExporter(exporterOptions, options.ExtensionObjectFileNamePattern, options.ObjectFileNamePattern, options.SourceDirectory, options.TargetDirectory);
			source.ForAll((Tuple<string, ApplicationObject> fileNameAndApplicationObject) =>
			{
				ExportApplicationObject(fileNameAndApplicationObject, options, exporterOptions, dotNetExtractor, translationExtractor, enumDeclarationExtractor, errors, fileNameExporter);
			});
			ExportDotNet(exporterOptions, options, dotNetExtractor);
			ExportEnum(exporterOptions, options, enumDeclarationExtractor);
			ExportTranslationFiles(exporterOptions, options, translationExtractor);
			FormatFiles(options);
		}
		catch (AggregateException ex)
		{
			foreach (Exception innerException in ex.InnerExceptions)
			{
				if (innerException != null)
				{
					errors.Enqueue(innerException);
				}
			}
		}
		catch (Exception item)
		{
			errors.Enqueue(item);
			throw;
		}
		finally
		{
			int count = errors.Count;
			OutputErrorsIfAny(errors, options);
			Console.WriteLine("Total Time: {0:g}", TimeSpan.FromTicks(DateTime.Now.Ticks - ticks));
			Console.WriteLine("Errors: {0}", count);
		}
	}

	private static void FormatFiles(AlExporterCommandLineOptions options)
	{
		if (options.Format)
		{
			Formatter.FormatFilesInFolder(options.TargetDirectory).GetAwaiter().GetResult();
		}
	}

	private static IDotNetAddInsInjector GetDotNetInjector(AlExporterCommandLineOptions options)
	{
		if (options.InjectDotNetAddIns)
		{
			return new DefaultDotNetAddInsInjector();
		}
		if (!string.IsNullOrEmpty(options.DotNetAddInsPackage))
		{
			return new DeltaDotNetAddInsInjector(File.ReadAllText(options.DotNetAddInsPackage));
		}
		return NullDotNetAddInsInjector.Instance;
	}

	private static ParallelQuery<Tuple<string, ApplicationObject>> ParallelExportQuery(IEnumerable<string> txtFiles, AlExporterCommandLineOptions options, ConcurrentQueue<Exception> errors)
	{
		TxtFileModelInfo modelInfo = new TxtFileModelInfo();
		return txtFiles.AsParallel().WithDegreeOfParallelism((!options.MultiThreaded) ? 1 : 4).SelectMany((string fileName) => ExtractApplicationObjectsFromFile(options, modelInfo, fileName, errors));
	}

	private static void ExportTranslationFiles(AlExportOptions exporterOptions, AlExporterCommandLineOptions options, ITranslationExtractor translationExtractor)
	{
		string fileExt = GetTranslationFileExtension(exporterOptions);
		translationExtractor.FinalizeExtraction((CultureInfo culture, Stream stream) =>
		{
			string path = Path.Combine(options.TargetDirectory, "translation." + culture?.ToString() + fileExt);
			stream.Position = 0L;
			using StreamReader streamReader = new StreamReader(stream, Encoding.Unicode);
			string contents = streamReader.ReadToEnd();
			File.WriteAllText(path, contents);
		});
	}

	private static string GetTranslationFileExtension(AlExportOptions options)
	{
		return options.TranslationFileFormat switch
		{
			TranslationFileFormat.Lcg => ".lcg", 
			TranslationFileFormat.Xliff => ".xlf", 
			_ => throw new ArgumentOutOfRangeException(options.TranslationFileFormat.ToString()), 
		};
	}

	private static void ExportDotNet(AlExportOptions exporterOptions, AlExporterCommandLineOptions commandLineOptions, IDotNetDeclarationExtractor dotNetExtractor)
	{
		string text = ExportToString((Stream ms) =>
		{
			new AlExporter(exporterOptions, dotNetExtractor).ExportDotNet(dotNetExtractor, ms);
		});
		if (text.Length > 0)
		{
			File.WriteAllText(Path.Combine(commandLineOptions.TargetDirectory, "dotnet.al"), text);
		}
	}

	private static void ExportEnum(AlExportOptions exporterOptions, AlExporterCommandLineOptions commandLineOptions, IEnumDeclarationExtractor enumDeclarationExtractor)
	{
		string text = ExportToString((Stream ms) =>
		{
			new AlExporter(exporterOptions, null, null, enumDeclarationExtractor).ExportEnum(enumDeclarationExtractor, ms);
		});
		if (text.Length > 0)
		{
			File.WriteAllText(Path.Combine(commandLineOptions.TargetDirectory, "enums.al"), text);
		}
	}

	private static void ExportApplicationObject(Tuple<string, ApplicationObject> fileAndApplicationObject, AlExporterCommandLineOptions cmdLineOptions, AlExportOptions exporterOptions, IDotNetDeclarationExtractor dotNetExtractor, ITranslationExtractor translationExtractor, IEnumDeclarationExtractor enumExtractor, ConcurrentQueue<Exception> errorsStorage, FileNameExporter fileNameExporter)
	{
		string item = fileAndApplicationObject.Item1;
		ApplicationObject item2 = fileAndApplicationObject.Item2;
		try
		{
			string text = ExportObjectToString(item2, dotNetExtractor, translationExtractor, enumExtractor, fileNameExporter, exporterOptions);
			if (text.Length > 0)
			{
				SaveNewSourceToFile(GetPathToNewAlFile(item, item2, fileNameExporter, cmdLineOptions.Rename), text);
			}
			string text2 = ExportRdlcToString(new List<ApplicationObject> { item2 }, fileNameExporter);
			if (text2.Length > 0)
			{
				SaveRdlcToFile(item, item2, text2, fileNameExporter);
			}
			using MemoryStream memoryStream = ExportWordLayoutToStream(new List<ApplicationObject> { item2 }, fileNameExporter);
			if (memoryStream.Length > 0)
			{
				SaveWordLayoutToFile(item, item2, memoryStream, fileNameExporter);
			}
		}
		catch (TxtImportException ex)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Error importing file: " + item);
			stringBuilder.AppendLine(((Exception)(object)ex).Message);
			stringBuilder.AppendLine(ex.Line);
			stringBuilder.AppendLine(ex.LineNo.ToString());
			stringBuilder.Append(ex.LinePos.ToString());
			Exception item3 = new Exception(stringBuilder.ToString(), (Exception)(object)ex);
			errorsStorage.Enqueue(item3);
		}
		catch (AlExporterException ex2)
		{
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.AppendLine("Error exporting file: " + item);
			stringBuilder2.Append(ex2.Message);
			Exception item4 = new Exception(stringBuilder2.ToString(), ex2);
			errorsStorage.Enqueue(item4);
		}
		catch (Exception ex3)
		{
			StringBuilder stringBuilder3 = new StringBuilder();
			stringBuilder3.AppendLine("Error converting file: " + item);
			stringBuilder3.Append(ex3.Message);
			Exception item5 = new Exception(stringBuilder3.ToString(), ex3);
			errorsStorage.Enqueue(item5);
		}
	}

	private static void ExportManifestFile(AlExporterCommandLineOptions options)
	{
		string text = Directory.GetFiles(options.SourceDirectory, AlExporter.GetV1ExtensionManifestFileName()).FirstOrDefault();
		string text2 = Path.Combine(options.TargetDirectory, AlExporter.GetV2ExtensionManifestFileName());
		if (!string.IsNullOrWhiteSpace(text))
		{
			WriteProgressMessage(text2);
			string contents = AlExporter.ConvertAppManifest(text);
			File.WriteAllText(text2, contents);
		}
	}

	private static void SaveNewSourceToFile(string newFullFilePath, string newSourceCode)
	{
		WriteProgressMessage(newFullFilePath);
		File.WriteAllText(newFullFilePath, newSourceCode);
	}

	private static void WriteProgressMessage(string filePath)
	{
		if (File.Exists(filePath))
		{
			Console.WriteLine("Overwriting: " + filePath);
		}
		else
		{
			Console.WriteLine("Writing: " + filePath);
		}
	}

	private static string GetPathToNewAlFile(string txtFilename, ApplicationObject ao, FileNameExporter fileNameExporter, bool rename)
	{
		string newFileName = ((!rename || ao == null) ? (Path.GetFileNameWithoutExtension(txtFilename) + BaseFileNameExporter.AlFileExtension) : fileNameExporter.GetAlFileName(ao));
		return fileNameExporter.GetPathToResourceFile(txtFilename, newFileName);
	}

	private static void SaveRdlcToFile(string txtFilename, ApplicationObject ao, string newSourceCode, FileNameExporter fileNameExporter)
	{
		string pathToResourceFile = fileNameExporter.GetPathToResourceFile(txtFilename, fileNameExporter.GetRdlcLayoutFileName(ao.RootElement));
		WriteProgressMessage(pathToResourceFile);
		File.WriteAllText(pathToResourceFile, newSourceCode);
	}

	private static void SaveWordLayoutToFile(string txtFilename, ApplicationObject ao, MemoryStream wordLayoutStream, FileNameExporter fileNameExporter)
	{
		string pathToResourceFile = fileNameExporter.GetPathToResourceFile(txtFilename, fileNameExporter.GetWordLayoutFileName(ao.RootElement));
		WriteProgressMessage(pathToResourceFile);
		using FileStream destination = new FileStream(pathToResourceFile, FileMode.Create, FileAccess.Write);
		wordLayoutStream.CopyTo(destination);
	}

	private static string ExportObjectToString(ApplicationObject ao, IDotNetDeclarationExtractor dotNetExtractor, ITranslationExtractor translationExtractor, IEnumDeclarationExtractor enumExtractor, FileNameExporter fileNameExporter, AlExportOptions options = null)
	{
		return new AlExporter(options, dotNetExtractor, translationExtractor, enumExtractor, fileNameExporter).ExportObjectToString(ao);
	}

	private static string ExportRdlcToString(List<ApplicationObject> ao, FileNameExporter fileNameExporter)
	{
		return ExportToString((Stream ms) =>
		{
			new AlExporter(AlExportOptions.Default, null, null, null, fileNameExporter).ExportRdlc(ao, ms);
		});
	}

	private static string ExportToString(Action<Stream> exportToStream)
	{
		using MemoryStream memoryStream = new MemoryStream();
		exportToStream(memoryStream);
		memoryStream.Position = 0L;
		using StreamReader streamReader = new StreamReader(memoryStream, Encoding.Unicode);
		return streamReader.ReadToEnd();
	}

	private static MemoryStream ExportWordLayoutToStream(List<ApplicationObject> ao, FileNameExporter fileNameExporter)
	{
		return new AlExporter(AlExportOptions.Default, null, null, null, fileNameExporter).ExportWordLayout(ao);
	}

	private static IEnumerable<Tuple<string, ApplicationObject>> ExtractApplicationObjectsFromFile(AlExporterCommandLineOptions options, TxtFileModelInfo modelInfo, string filename, ConcurrentQueue<Exception> errorsStorage)
	{
		using FileStream fs = File.OpenRead(filename);
		List<ApplicationObject> list = new List<ApplicationObject>();
		TxtImporter txtImporter = new TxtImporter(modelInfo);
		try
		{
			list = txtImporter.ImportFromStream(fs);
		}
		catch (TxtImportException ex)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Error extracting application object from file: " + filename);
			stringBuilder.AppendLine("Message: " + ((Exception)(object)ex).Message);
			stringBuilder.AppendLine("Line content: " + ex.Line);
			stringBuilder.AppendLine("Line number: " + ex.LineNo);
			stringBuilder.Append("Line position: " + ex.LinePos);
			Exception item = new Exception(stringBuilder.ToString());
			errorsStorage.Enqueue(item);
		}
		catch (Exception ex2)
		{
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.AppendLine("Error extracting application object from file: " + filename);
			stringBuilder2.Append("Message: " + ex2.Message);
			Exception item2 = new Exception(stringBuilder2.ToString());
			errorsStorage.Enqueue(item2);
		}
		foreach (ApplicationObject item3 in list)
		{
			if (options.Type.ShouldExportElementType(item3.ElementTypeInfo.ElementType))
			{
				yield return new Tuple<string, ApplicationObject>(filename, item3);
			}
		}
	}

	private static void WriteWarning(string message)
	{
		Console.Error.WriteLine();
		Console.BackgroundColor = ConsoleColor.Black;
		Console.ForegroundColor = ConsoleColor.DarkYellow;
		Console.Error.WriteLine("----WARNING----");
		Console.Error.WriteLine("Warning: " + message);
		Console.Error.WriteLine();
		Console.ResetColor();
	}

	private static void OutputErrorsIfAny(ConcurrentQueue<Exception> errors, AlExporterCommandLineOptions options)
	{
		Console.Error.WriteLine();
		Console.BackgroundColor = ConsoleColor.Black;
		Console.ForegroundColor = ConsoleColor.Red;
		Exception result;
		while (errors.TryDequeue(out result) && result != null)
		{
			Console.Error.WriteLine("----ERROR----");
			Console.Error.WriteLine(result.Message);
			if (options.Stacktrace)
			{
				Console.Error.WriteLine(result.StackTrace ?? result.InnerException?.StackTrace);
			}
			Console.Error.WriteLine();
		}
		Console.ResetColor();
	}
}
