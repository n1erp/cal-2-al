using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.Tools.Language;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.Tools;

internal class LanguageTool : ReadWriteModelTool
{
	private readonly LanguageToolOptions options;

	internal LanguageTool(ILogger logger, LanguageToolOptions options)
		: base(logger, options)
	{
		this.options = options;
	}

	internal static SortedSet<int> ParseLanguageIds(string[] language)
	{
		return new SortedSet<int>(language.Select(ParseLCID));
	}

	internal IEnumerable<LanguageFileInfo> ExportLanguage(string[] source, SortedSet<int> languageSet, int developmentLcid, string destination, string sourceName)
	{
		ApplicationObjectFileInfoCollection objects = ReadObjects(source, sourceName);
		ConcurrentBag<ApplicationObjectLanguageInfo> exportedLanguages = new ConcurrentBag<ApplicationObjectLanguageInfo>();
		Logger.InvokeParallelProcessing(NonLocalizable.ExtractingLanguages, objects.Count(), (IProgressLogger progress) =>
		{
			Parallel.ForEach(objects, ParallelOptions, (ApplicationObjectFileInfo o) =>
			{
				ExportLanguageVisitor exportLanguageVisitor = new ExportLanguageVisitor(o.ApplicationObject.RootElement, languageSet, developmentLcid);
				exportLanguageVisitor.Visit();
				foreach (ApplicationObjectLanguageInfo language in exportLanguageVisitor.GetLanguages())
				{
					exportedLanguages.Add(language);
				}
				progress.Increment();
			});
		});
		List<ApplicationObjectLanguageInfo> list = exportedLanguages.ToList();
		list.Sort(ApplicationObjectLanguageInfo.Comparer);
		if (destination == null)
		{
			return new LanguageFileInfo[1]
			{
				new LanguageFileInfo(string.Empty, list)
			};
		}
		return WriteLanguageFiles(destination, list);
	}

	internal IEnumerable<ApplicationObjectFileInfo> ImportLanguage(string[] source, string[] languageFiles, SortedSet<int> languageSet, string destination, string sourceName = null)
	{
		ApplicationObjectFileInfoCollection objects = ReadObjects(source, sourceName);
		List<string> lines = ReadLanguageFiles(languageFiles);
		Logger.InvokeParallelProcessing(NonLocalizable.ApplyingLanguage, lines.Count, (IProgressLogger progress) =>
		{
			ImportLanguage(lines, objects, languageSet, (ElementType objType, int objId) =>
			{
				Logger.WriteLine(string.Format(CultureInfo.InvariantCulture, NonLocalizable.UnknownLanguageTargetObject, objType, objId), MessageType.Warning);
			}, (string line) =>
			{
				Logger.WriteLine(string.Format(CultureInfo.InvariantCulture, NonLocalizable.UnknownLanguageTarget, line), MessageType.Warning);
			}, progress.Increment);
		});
		return WriteApplicationObjects(objects.Select((ApplicationObjectFileInfo o) => o.ApplicationObject), destination, OutputType.ApplicationObject);
	}

	internal IEnumerable<ApplicationObjectFileInfo> RemoveLanguage(string[] source, SortedSet<int> languageSet, string destination, int defaultLanguage, bool reverseMakeCap, string sourceName = null)
	{
		ApplicationObjectFileInfoCollection objects = ReadObjects(source, sourceName);
		Logger.InvokeParallelProcessing(NonLocalizable.RemovingLanguages, objects.Count(), (IProgressLogger progress) =>
		{
			Parallel.ForEach(objects, ParallelOptions, (ApplicationObjectFileInfo o) =>
			{
				new RemoveLanguageVisitor(o.ApplicationObject.RootElement, languageSet, defaultLanguage, reverseMakeCap).Visit();
				progress.Increment();
			});
		});
		return WriteApplicationObjects(objects.Select((ApplicationObjectFileInfo o) => o.ApplicationObject), destination, OutputType.ApplicationObject);
	}

	internal IEnumerable<ApplicationObjectLanguageInfo> TestLanguage(string[] source, SortedSet<int> languageSet, string sourceName = null)
	{
		ApplicationObjectFileInfoCollection objects = ReadObjects(source, sourceName);
		ConcurrentBag<ApplicationObjectLanguageInfo> missingLanguages = new ConcurrentBag<ApplicationObjectLanguageInfo>();
		Logger.InvokeParallelProcessing(NonLocalizable.TestingLanguages, objects.Count(), (IProgressLogger progress) =>
		{
			Parallel.ForEach(objects, ParallelOptions, (ApplicationObjectFileInfo o) =>
			{
				TestLanguageVisitor testLanguageVisitor = new TestLanguageVisitor(o.ApplicationObject.RootElement, languageSet);
				testLanguageVisitor.Visit();
				foreach (ApplicationObjectLanguageInfo language in testLanguageVisitor.GetLanguages())
				{
					missingLanguages.Add(language);
				}
				progress.Increment();
			});
		});
		return missingLanguages;
	}

	internal IEnumerable<LanguageFileInfo> SplitApplicationObjectLanguageFile(string[] source, string destination)
	{
		return ReWriteApplicationObjectLanguageFile(source, destination);
	}

	internal IEnumerable<LanguageFileInfo> JoinApplicationObjectLanguageFile(string[] source, string destination)
	{
		if (destination != null && Directory.Exists(destination))
		{
			throw new ModelToolException(NonLocalizable.ErrorFileNameRequired, destination);
		}
		return ReWriteApplicationObjectLanguageFile(source, destination);
	}

	internal IEnumerable<LanguageFileInfo> ReWriteApplicationObjectLanguageFile(string[] source, string destination)
	{
		List<string> lines = ReadLanguageFiles(source);
		List<ApplicationObjectLanguageInfo> languageFiles = null;
		Logger.InvokeParallelProcessing(NonLocalizable.ProcessLanguage, lines.Count(), (IProgressLogger progress) =>
		{
			languageFiles = LinesToLanguageInfo(lines, progress.Increment);
		});
		if (destination == null)
		{
			return new LanguageFileInfo[1]
			{
				new LanguageFileInfo(string.Empty, languageFiles)
			};
		}
		return WriteLanguageFiles(destination, languageFiles);
	}

	internal static List<ApplicationObjectLanguageInfo> LinesToLanguageInfo(IEnumerable<string> lines, Action progress = null)
	{
		List<ApplicationObjectLanguageInfo> list = new List<ApplicationObjectLanguageInfo>();
		foreach (string line in lines)
		{
			LanguageKeyHelper.SplitLanguageStringIntoKeyValue(line, out var key, out var value);
			if (LanguageKeyHelper.GetInfoFromLanguageKey(key, out var objectType, out var id, out var lcid))
			{
				ApplicationObjectLanguageInfo applicationObjectLanguageInfo = list.SingleOrDefault((ApplicationObjectLanguageInfo i) => i.Id == id && i.ObjectType == objectType && i.LCID == lcid);
				if (applicationObjectLanguageInfo == null)
				{
					applicationObjectLanguageInfo = new ApplicationObjectLanguageInfo(objectType, id, lcid);
					list.Add(applicationObjectLanguageInfo);
				}
				applicationObjectLanguageInfo.AddLine(key, value);
			}
			progress?.Invoke();
		}
		return list;
	}

	internal static void ImportLanguage(IEnumerable<string> lines, IEnumerable<ApplicationObjectFileInfo> objects, SortedSet<int> languageSet, Action<ElementType, int> objectNotFound = null, Action<string> elementNotFound = null, Action progress = null)
	{
		string text = string.Empty;
		ApplicationObjectFileInfo applicationObjectFileInfo = null;
		ApplicationObjectFileInfo[] source = (objects as ApplicationObjectFileInfo[]) ?? objects.ToArray();
		foreach (string line in lines)
		{
			LanguageKeyHelper.SplitLanguageStringIntoKeyValue(line, out var key, out var value);
			string[] array = LanguageKeyHelper.SplitKeyIntoSegments(key);
			if (array[0] != text)
			{
				LanguageKeyHelper.GetObjectTypeAndIdFromKey(array[0], out var objType, out var objId);
				text = array[0];
				applicationObjectFileInfo = source.SingleOrDefault((ApplicationObjectFileInfo o) => o.ObjectType == objType && o.Id == objId);
				if (applicationObjectFileInfo == null)
				{
					objectNotFound?.Invoke(objType, objId);
				}
			}
			if (applicationObjectFileInfo != null && LanguageKeyHelper.ValidateKeySegments(array, out var propertyType, out var lcid, out var _) && IsLanguageInSet(languageSet, lcid))
			{
				IElement element = LanguageKeyHelper.FindElementFromLanguageKey(applicationObjectFileInfo.ApplicationObject.RootElement, array);
				if (element != null)
				{
					AddOrUpdateLanguageOnProperty(element, propertyType, WindowsLanguageHelper.FindCulture(lcid), value);
				}
				else
				{
					elementNotFound?.Invoke(line);
				}
			}
			progress?.Invoke();
		}
	}

	internal static void AddOrUpdateLanguageOnProperty(IElement element, PropertyType propType, CultureInfo culture, string value)
	{
		element.SetStringProperty(propType, ModelTextHelper.AddOrUpdateLanguage(element.GetStringProperty(propType), culture, value));
	}

	private List<string> ReadLanguageFiles(ICollection<string> languageFiles)
	{
		List<string> lines = new List<string>();
		Logger.InvokeParallelProcessing(NonLocalizable.ReadFiles, languageFiles.Count, (IProgressLogger progress) =>
		{
			Parallel.ForEach(languageFiles, ParallelOptions, (string langFile) =>
			{
				List<string> list = new List<string>();
				using (StreamReader streamReader = new StreamReader(new BufferedStream(new FileStream(langFile, FileMode.Open, FileAccess.Read)), ReadWriteModelTool.GetEncoding(options.Encoding)))
				{
					while (!streamReader.EndOfStream)
					{
						list.Add(streamReader.ReadLine());
					}
				}
				lock (lines)
				{
					lines.AddRange(list);
				}
				progress.Increment();
			});
		});
		lines.Sort();
		return lines;
	}

	private IEnumerable<LanguageFileInfo> WriteLanguageFiles(string destination, IReadOnlyCollection<ApplicationObjectLanguageInfo> sortedLanguages)
	{
		ConcurrentBag<LanguageFileInfo> result = new ConcurrentBag<LanguageFileInfo>();
		if (Directory.Exists(destination))
		{
			Logger.InvokeParallelProcessing(NonLocalizable.WriteFiles, sortedLanguages.Count, (IProgressLogger progress) =>
			{
				Parallel.ForEach(sortedLanguages, ParallelOptions, (ApplicationObjectLanguageInfo lang) =>
				{
					string text = Path.Combine(destination, ReadWriteModelTool.CalcSaveFileName(lang.ObjectType, lang.Id, "TXT", "-" + ((lang.LCID == 1) ? "Comment" : WindowsLanguageHelper.AbbreviatedName(WindowsLanguageHelper.FindCulture(lang.LCID)))));
					if (progress.ShouldProcess(text))
					{
						CheckFileNotExistsOrOverwrite(text, progress);
						using (StreamWriter streamWriter = new StreamWriter(new FileStream(text, FileMode.Create, FileAccess.Write), ReadWriteModelTool.GetEncoding(options.Encoding)))
						{
							foreach (TranslateLine translateLine in lang.TranslateLines)
							{
								streamWriter.WriteLine(translateLine.Line);
							}
							result.Add(new LanguageFileInfo(text, lang));
						}
						progress.Increment();
					}
				});
			});
		}
		else
		{
			Logger.InvokeParallelProcessing(NonLocalizable.WriteFile, sortedLanguages.Count, (IProgressLogger progress) =>
			{
				if (progress.ShouldProcess(destination))
				{
					CheckFileNotExistsOrOverwrite(destination, progress);
					using StreamWriter streamWriter = new StreamWriter(new FileStream(destination, FileMode.Create, FileAccess.Write), ReadWriteModelTool.GetEncoding(options.Encoding));
					foreach (ApplicationObjectLanguageInfo sortedLanguage in sortedLanguages)
					{
						foreach (TranslateLine translateLine2 in sortedLanguage.TranslateLines)
						{
							streamWriter.WriteLine(translateLine2.Line);
						}
						progress.Increment();
					}
					result.Add(new LanguageFileInfo(destination, sortedLanguages));
				}
			});
		}
		return result;
	}

	internal static bool IsLanguageInSet(SortedSet<int> languageSet, int lcid)
	{
		return languageSet?.Contains(lcid) ?? true;
	}

	internal static int ParseLCID(string value)
	{
		if (value == "@@@" || string.Compare(value, "Comment", StringComparison.OrdinalIgnoreCase) == 0)
		{
			return 1;
		}
		CultureInfo cultureInfo;
		if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
		{
			if (result == 1)
			{
				return 1;
			}
			cultureInfo = WindowsLanguageHelper.FindCulture(result);
		}
		else
		{
			cultureInfo = WindowsLanguageHelper.FindCulture(value);
		}
		if (cultureInfo == null)
		{
			throw new ModelToolException(string.Format(CultureInfo.InvariantCulture, NonLocalizable.InvalidLanguageId, value));
		}
		return cultureInfo.LCID;
	}
}
