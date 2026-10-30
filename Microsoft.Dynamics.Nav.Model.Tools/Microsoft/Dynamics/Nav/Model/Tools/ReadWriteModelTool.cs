using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.IO.Txt;
using Microsoft.Dynamics.Nav.Model.Tools.IO.Nea;
using Microsoft.Dynamics.Nav.Model.Tools.Language;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.Tools;

internal abstract class ReadWriteModelTool : ModelToolBase
{
	private readonly ReadWriteModelToolOptions options;

	private readonly ParallelOptions parallelOptions;

	public const string ApplicationObjectExt = "TXT";

	public const string DeltaObjectExt = "DELTA";

	public const string ConflictObjectExt = "CONFLICT";

	public const string TranslateExt = "TXT";

	protected ReadWriteModelToolOptions Options => options;

	protected ParallelOptions ParallelOptions => parallelOptions;

	internal ReadWriteModelTool(ILogger logger, ReadWriteModelToolOptions options = null)
		: base(logger)
	{
		this.options = options ?? ReadWriteModelToolOptions.Default;
		parallelOptions = new ParallelOptions
		{
			MaxDegreeOfParallelism = ((!this.options.RunInParallel) ? 1 : 16)
		};
	}

	protected ApplicationObjectFileInfoCollection ReadObjects(string[] files, string sourceName)
	{
		ConcurrentQueue<ApplicationObjectFileInfo> applicationObjectInfos = new ConcurrentQueue<ApplicationObjectFileInfo>();
		if (files.Length == 1)
		{
			Logger.InvokeParallelProcessing(NonLocalizable.ReadFile, 0, (IProgressLogger progress) =>
			{
				ReadSingleFile(files[0], sourceName, (long position, long total) =>
				{
					progress.SetProgress((int)(100 * position / total));
				}).ForEach((ApplicationObjectFileInfo i) =>
				{
					applicationObjectInfos.Enqueue(i);
				});
			});
		}
		else
		{
			Logger.InvokeParallelProcessing(NonLocalizable.ReadFiles, files.Length, (IProgressLogger progress) =>
			{
				Parallel.ForEach(files, parallelOptions, (string file) =>
				{
					ReadSingleFile(file, sourceName).ForEach((ApplicationObjectFileInfo i) =>
					{
						applicationObjectInfos.Enqueue(i);
					});
					progress.Increment();
				});
			});
		}
		return ApplicationObjectFileInfoCollection.Create(applicationObjectInfos, sourceName);
	}

	protected ApplicationObjectFileInfoCollection ReadSingleFile(string file, string sourceName, Action<long, long> progressAction = null)
	{
		List<ApplicationObjectFileInfo> objectFiles = new List<ApplicationObjectFileInfo>();
		using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
		{
			bool encoded = NeaStreamReader.IsSupported(fileStream);
			Stream stream = (encoded ? ((Stream)new NeaStreamReader(fileStream, leaveOpen: true)) : ((Stream)fileStream));
			TxtImportOptions txtImportOptions = options.GetTxtImportOptions();
			txtImportOptions.ProgressAction = progressAction;
			txtImportOptions.WarningAction = (int lineNo, int linePos, ElementType objectType, int objectId, string message) =>
			{
				Logger.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} ({1},{2}) {3}", file, lineNo, linePos, message), MessageType.Warning);
			};
			TxtImporter txtImporter = new TxtImporter(TxtFileModelInfo.Instance, txtImportOptions);
			try
			{
				List<ApplicationObject> list = txtImporter.ImportFromStream(stream);
				if (encoded)
				{
					list.ForEach((ApplicationObject o) =>
					{
						o.ShouldEncode = encoded;
					});
				}
				list.ForEach((ApplicationObject o) =>
				{
					objectFiles.Add(new ApplicationObjectFileInfo(file, o));
				});
			}
			catch (Exception e)
			{
				Logger.WriteLine(FormatException(file, e), MessageType.Error);
			}
		}
		return ApplicationObjectFileInfoCollection.Create(objectFiles, sourceName);
	}

	protected ApplicationObjectFileInfoCollection WriteApplicationObjects(IEnumerable<ApplicationObject> objects, string fileSpec, OutputType outputType)
	{
		ConcurrentBag<ApplicationObjectFileInfo> objectFiles = new ConcurrentBag<ApplicationObjectFileInfo>();
		if (objects.Any())
		{
			if (Directory.Exists(fileSpec))
			{
				Logger.InvokeParallelProcessing(NonLocalizable.WriteFile, objects.Count(), (IProgressLogger progress) =>
				{
					Parallel.ForEach(objects, ParallelOptions, (ApplicationObject o) =>
					{
						string text = Path.Combine(fileSpec, CalcSaveFileName(o, outputType));
						if (progress.ShouldProcess(text))
						{
							WriteApplicationObject(o, text, progress);
						}
						progress.Increment();
						objectFiles.Add(new ApplicationObjectFileInfo(text, o));
					});
				});
			}
			else
			{
				Logger.InvokeParallelProcessing(NonLocalizable.WriteFiles, objects.Count(), (IProgressLogger progress) =>
				{
					if (progress.ShouldProcess(fileSpec))
					{
						CheckFileNotExistsOrOverwrite(fileSpec, progress);
						IOrderedEnumerable<ApplicationObject> orderedEnumerable = objects.OrderBy((ApplicationObject o) => o.Id);
						using FileStream fileStream = File.Open(fileSpec, FileMode.Create, FileAccess.Write);
						Options.GetApplicationObjectExporter().ExportObjects(orderedEnumerable, orderedEnumerable.Any((ApplicationObject obj) => obj.ShouldEncode) ? ((Stream)new NeaStreamWriter(fileStream, leaveOpen: true)) : ((Stream)fileStream), progress.Increment);
						orderedEnumerable.ForEach((ApplicationObject o) =>
						{
							objectFiles.Add(new ApplicationObjectFileInfo(fileSpec, o));
						});
					}
				});
			}
		}
		return ApplicationObjectFileInfoCollection.Create(objectFiles, NonLocalizable.WriteFiles);
	}

	protected void CheckFileNotExistsOrOverwrite(string fileName, IProgressLogger progress)
	{
		if (File.Exists(fileName) && !options.OverwriteExistingFiles && !progress.ShouldContinue(fileName))
		{
			throw new ModelToolException(NonLocalizable.ErrorFileExists, fileName);
		}
	}

	protected ApplicationObjectFileInfo WriteApplicationObject(ApplicationObject appObj, string fileName, IProgressLogger progress)
	{
		CheckFileNotExistsOrOverwrite(fileName, progress);
		using (FileStream fileStream = File.Open(fileName, FileMode.Create, FileAccess.Write))
		{
			Options.GetApplicationObjectExporter().ExportObject(appObj, appObj.ShouldEncode ? ((Stream)new NeaStreamWriter(fileStream, leaveOpen: true)) : ((Stream)fileStream));
		}
		return new ApplicationObjectFileInfo(fileName, appObj);
	}

	protected static string CalcSaveFileName(ApplicationObject ao, OutputType outputType)
	{
		bool flag = ao.ElementTypeInfo.ElementType == ElementType.Modification;
		string path;
		if (flag)
		{
			IElementKeySegment elementKeySegment = ElementKeyHelper.DisplayProvider.GetElementKeySegments(ao.RootElement.GetStringProperty(PropertyType.Target)).Single();
			path = elementKeySegment.ElementType.ToString().Substring(0, 3).ToUpper(CultureInfo.InvariantCulture) + elementKeySegment.Id;
		}
		else
		{
			path = ao.ElementTypeInfo.Name.Substring(0, 3).ToUpper(CultureInfo.InvariantCulture) + ao.Id.ObjectNumber.ToString(CultureInfo.InvariantCulture);
		}
		return Path.ChangeExtension(path, outputType switch
		{
			OutputType.ApplicationObject => "TXT", 
			OutputType.CompareResult => "DELTA", 
			OutputType.MergeResult => flag ? "CONFLICT" : "TXT", 
			_ => throw new NotSupportedException(), 
		});
	}

	protected static string CalcSaveFileName(ElementType objectType, int objectId, string extension, string postFix = null)
	{
		return objectType.ToString().Substring(0, 3).ToUpper(CultureInfo.InvariantCulture) + objectId + (postFix ?? string.Empty) + "." + extension;
	}

	private static string FormatException(string fileName, Exception e)
	{
		if (e is TxtImportException ex)
		{
			return string.Format(CultureInfo.InvariantCulture, "{3}({0},{1}) {2}", ex.LineNo, ex.LinePos, ex.Message, Path.GetFileName(fileName));
		}
		return e.Message;
	}

	protected static Encoding GetEncoding(FileEncoding enc)
	{
		return enc switch
		{
			FileEncoding.ASCII => Encoding.ASCII, 
			FileEncoding.BigEndianUnicode => Encoding.BigEndianUnicode, 
			FileEncoding.Default => Encoding.Default, 
			FileEncoding.UTF32 => Encoding.UTF32, 
			FileEncoding.UTF7 => Encoding.UTF7, 
			FileEncoding.UTF8 => Encoding.UTF8, 
			FileEncoding.Unicode => Encoding.Unicode, 
			_ => OemEncoding.Instance, 
		};
	}
}
