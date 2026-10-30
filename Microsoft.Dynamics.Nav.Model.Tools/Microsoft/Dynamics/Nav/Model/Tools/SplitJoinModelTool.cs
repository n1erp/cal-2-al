using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Model.IO.Txt;
using Microsoft.Dynamics.Nav.Model.Tools.Language;

namespace Microsoft.Dynamics.Nav.Model.Tools;

internal sealed class SplitJoinModelTool : ReadWriteModelTool
{
	internal SplitJoinModelTool(ILogger logger, ReadWriteModelToolOptions options)
		: base(logger, options)
	{
	}

	internal IEnumerable<FileInfo> SplitApplicationObjectFile(string[] source, string destination, string sourceName = null)
	{
		ApplicationObjectFileInfoCollection applicationObjectFileInfoCollection = ReadObjects(source, sourceName);
		return WriteApplicationObjects(applicationObjectFileInfoCollection.ObjectCollection, destination, OutputType.ApplicationObject).Files;
	}

	internal IEnumerable<FileInfo> SplitApplicationObjectFileNoParsing(string[] source, string destination)
	{
		ConcurrentBag<FileInfo> resultFiles = new ConcurrentBag<FileInfo>();
		Encoding encoding = new TxtExportOptions().Encoding;
		foreach (string file in source)
		{
			Dictionary<string, Stream> objectStreams = new Dictionary<string, Stream>();
			Logger.InvokeParallelProcessing(NonLocalizable.ReadFile, 0, (IProgressLogger progress) =>
			{
				using StreamReader streamReader = new StreamReader(file, encoding);
				StreamWriter streamWriter = null;
				long num = ((streamReader.BaseStream.Length == 0L) ? 1 : streamReader.BaseStream.Length);
				while (!streamReader.EndOfStream)
				{
					string text = streamReader.ReadLine();
					if (text.Length > 15)
					{
						string text2 = text.TrimStart();
						if (text2.Length > 15 && text2.Substring(0, 7) == "OBJECT ")
						{
							string[] array = text2.Split(' ', '\t');
							string key = array[1].Substring(0, 3).ToUpper(CultureInfo.InvariantCulture) + array[2];
							streamWriter?.Flush();
							objectStreams[key] = new MemoryStream();
							streamWriter = new StreamWriter(objectStreams[key], encoding);
							progress.SetProgress((int)(100 * streamReader.BaseStream.Position / num));
						}
					}
					streamWriter.WriteLine(text);
				}
				streamWriter.Flush();
			});
			Logger.InvokeParallelProcessing(NonLocalizable.WriteFiles, objectStreams.Count, (IProgressLogger progress) =>
			{
				Parallel.ForEach(objectStreams, ParallelOptions, (KeyValuePair<string, Stream> kvp) =>
				{
					string text = destination + Path.DirectorySeparatorChar + kvp.Key + ".TXT";
					if (progress.ShouldProcess(text))
					{
						if (!Options.OverwriteExistingFiles && File.Exists(text))
						{
							throw new ModelToolException(NonLocalizable.ErrorFileExists, text);
						}
						using FileStream fileStream = new FileStream(text, FileMode.Create, FileAccess.Write);
						kvp.Value.Position = 0L;
						kvp.Value.CopyTo(fileStream);
						resultFiles.Add(new FileInfo(fileStream.Name));
						progress.Increment();
					}
				});
			});
		}
		return resultFiles;
	}

	internal void JoinApplicationObjectFiles(string[] source, string destination, string sourceName)
	{
		ApplicationObjectFileInfoCollection source2 = ReadObjects(source, sourceName);
		source2 = ApplicationObjectFileInfoCollection.Create(source2.Where((ApplicationObjectFileInfo o) => o.ApplicationObject.IsValid), NonLocalizable.ReadFiles);
		if (Directory.Exists(destination))
		{
			Logger.WriteLine(NonLocalizable.ErrorFileNameRequired, MessageType.Error);
		}
		else
		{
			WriteApplicationObjects(source2.ObjectCollection, destination, OutputType.CompareResult);
		}
	}
}
