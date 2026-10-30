using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.IO.Txt;
using Microsoft.Dynamics.Nav.Model.Tools.Language;

namespace Microsoft.Dynamics.Nav.Model.Tools;

public sealed class WhitelistValidator
{
	private const string DefaultConfigFileName = "SchemaGatingTools.Whitelist.config";

	private static AppObjectChangesFilter changesFilter;

	public static IReadOnlyCollection<string> DisallowedChanges
	{
		get
		{
			if (changesFilter == null)
			{
				throw new InvalidOperationException(NonLocalizable.WhitelistValidatorIsNotInitialized);
			}
			return changesFilter.GetDisallowedChanges();
		}
	}

	private WhitelistValidator()
	{
	}

	public static void Initialize(string settingsFilePath, Func<string, IReadOnlyCollection<IElement>, bool> notAllowedCallback = null)
	{
		CheckNotNull(settingsFilePath, "settingsFilePath");
		if (!File.Exists(settingsFilePath))
		{
			throw new FileNotFoundException(settingsFilePath);
		}
		InternalInit(File.ReadAllLines(settingsFilePath), notAllowedCallback);
	}

	public static void Initialize(Func<string, IReadOnlyCollection<IElement>, bool> notAllowedCallback = null)
	{
		List<string> list = new List<string>();
		using (StreamReader streamReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("SchemaGatingTools.Whitelist.config")))
		{
			string item;
			while ((item = streamReader.ReadLine()) != null)
			{
				list.Add(item);
			}
		}
		InternalInit(list.ToArray(), notAllowedCallback);
	}

	private static void InternalInit(string[] settingsBody, Func<string, IReadOnlyCollection<IElement>, bool> notAllowedCallback = null)
	{
		if (notAllowedCallback == null)
		{
			notAllowedCallback = DefaultCallback;
		}
		changesFilter = new AppObjectChangesFilter(settingsBody, notAllowedCallback);
	}

	public static void Clear()
	{
		changesFilter = null;
	}

	public static bool IsAllowed(string beforeFilePath, string afterFilePath)
	{
		if (changesFilter == null)
		{
			throw new InvalidOperationException(NonLocalizable.WhitelistValidatorIsNotInitialized);
		}
		bool flag = true;
		CheckNotNull(beforeFilePath, "beforeFilePath");
		CheckNotNull(afterFilePath, "afterFilePath");
		TxtImporter importer = new TxtImporter(TxtFileModelInfo.Instance);
		List<ApplicationObject> source = LoadAppObjects(beforeFilePath, importer);
		List<ApplicationObject> source2 = LoadAppObjects(afterFilePath, importer);
		List<ApplicationObject> list = source.Where((ApplicationObject ao) => ao.Id.ObjectType == ElementType.Table).ToList();
		List<ApplicationObject> list2 = source2.Where((ApplicationObject ao) => ao.Id.ObjectType == ElementType.Table).ToList();
		var source3 = (from b in list
			join a in list2 on b.Id equals a.Id
			select new
			{
				Before = b,
				After = a
			}).ToArray();
		foreach (ApplicationObject beforeTable in list)
		{
			var anon = source3.FirstOrDefault(p => p.Before.Id.Equals(beforeTable.Id));
			ApplicationObject applicationObject;
			if (anon != null)
			{
				applicationObject = anon.After;
				list2.Remove(applicationObject);
			}
			else
			{
				applicationObject = new ApplicationObject(null);
			}
			bool flag2 = IsAllowed(beforeTable, applicationObject);
			flag = flag && flag2;
		}
		foreach (ApplicationObject item in list2)
		{
			bool flag3 = IsAllowed(new ApplicationObject(null), item);
			flag = flag && flag3;
		}
		return flag;
	}

	internal static bool IsAllowed(ApplicationObject before, ApplicationObject after)
	{
		return changesFilter.ValidateModification(before, after);
	}

	internal static void CheckNotNull(object value, string name)
	{
		if (value == null)
		{
			throw new ArgumentNullException(name);
		}
	}

	private static bool DefaultCallback(string changeDescription, IReadOnlyCollection<IElement> changeStack)
	{
		return false;
	}

	private static List<ApplicationObject> LoadAppObjects(string filePath, TxtImporter importer)
	{
		if (!File.Exists(filePath))
		{
			return new List<ApplicationObject>();
		}
		using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
		return importer.ImportFromStream(stream);
	}
}
