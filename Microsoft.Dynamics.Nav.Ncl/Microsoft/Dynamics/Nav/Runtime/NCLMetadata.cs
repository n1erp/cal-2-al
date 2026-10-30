using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Apps;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLMetadata
{
	private class EagerLoadFreshObjectResult
	{
		public EagerLoadFreshObjectOutcome Outcome { get; }

		public NCLMetaApplicationObject MetaApplicationObject { get; }

		public Exception Exception { get; }

		private EagerLoadFreshObjectResult(EagerLoadFreshObjectOutcome outcome, NCLMetaApplicationObject metaObject, Exception ex)
		{
			Outcome = outcome;
			MetaApplicationObject = metaObject;
			Exception = ex;
		}

		public static EagerLoadFreshObjectResult CreateSuccess(NCLMetaApplicationObject metaObject)
		{
			return new EagerLoadFreshObjectResult(EagerLoadFreshObjectOutcome.Success, metaObject, null);
		}

		public static EagerLoadFreshObjectResult CreateNotFound()
		{
			return new EagerLoadFreshObjectResult(EagerLoadFreshObjectOutcome.NotFound, null, null);
		}

		public static EagerLoadFreshObjectResult CreateFatalError(Exception ex)
		{
			return new EagerLoadFreshObjectResult(EagerLoadFreshObjectOutcome.FatalError, null, ex);
		}
	}

	private enum EagerLoadFreshObjectOutcome
	{
		Success,
		NotFound,
		FatalError
	}

	private class SharedAppObjectEntryLookup
	{
		private readonly Dictionary<int, SharedAppObjectEntry> entriesByMetadataAppGroup = new Dictionary<int, SharedAppObjectEntry>();

		public IEnumerable<SharedAppObjectEntry> Entries => entriesByMetadataAppGroup.Values;

		public SharedAppObjectEntry GetOrCreateEntry(int metadataGroupId, NCLMetaApplicationObject oldAppObject)
		{
			if (!entriesByMetadataAppGroup.TryGetValue(metadataGroupId, out var value))
			{
				value = new SharedAppObjectEntry(metadataGroupId);
				value.OldSharedObject = oldAppObject;
				entriesByMetadataAppGroup.Add(metadataGroupId, value);
			}
			return value;
		}
	}

	private class SharedAppObjectEntry
	{
		public int MetadataAppGroup { get; }

		public NCLMetaApplicationObject OldSharedObject { get; set; }

		public NCLMetaApplicationObject NewSharedObject { get; set; }

		public HashSet<int> AppGroupIdsPointingToThis { get; set; }

		public SharedAppObjectEntry(int metadataAppGroup)
		{
			MetadataAppGroup = metadataAppGroup;
			AppGroupIdsPointingToThis = new HashSet<int>();
		}
	}

	public const int UseCurrentTenantGroupId = -1;

	internal static readonly SortedList<int, string> SystemObjectAndNames = SystemObjects.CreateSystemObjectAndNamesList();

	private readonly Dictionary<int, NCLMetadataCacheEntry>[] metadataCacheEntries;

	private readonly Dictionary<int, NCLMetadataExtensionCacheEntry>[] metadataExtensionCacheEntries;

	private readonly HashSet<int> initializedAppGroups = new HashSet<int>();

	private InitializeAppGroupProfiler profiler = new InitializeAppGroupProfiler();

	private readonly object appObjectInitializationOrChangeSyncRoot = new object();

	private NavDatabase ownerDatabase;

	private readonly ALCodeEnvironment codeEnvironment;

	private int baseCodeunitMetadataChangeBumperToken;

	private int baseTableMetadataChangeBumperToken;

	private int baseMetadataChangeBumperToken;

	private int baseAllObjectIdsSnapshotBumperToken;

	private int baseTableSnapshotBumperToken;

	private Dictionary<int, ReadOnlyCollection<NCLMetaTable>> allMetaTablesSnapshots;

	private Dictionary<int, SortedList<ObjectType, SortedList<int, AllObjectSnapshotEntry>>> allObjectIdsSnapshots;

	private readonly object allMetaTablesSnapshotSyncRoot = new object();

	private readonly object allObjectIdsSnapshotSyncRoot = new object();

	private readonly Lazy<NavAppObjectChangeResolver> appObjectChangeResolver;

	private readonly INCLMetaApplicationObjectLoader metadataLoader;

	public NavDatabase OwnerDatabase => ownerDatabase;

	internal SymbolAggregator SymbolAggregator { get; set; }

	internal ALCodeEnvironment CodeEnvironment => codeEnvironment;

	internal int BaseCodeunitMetadataChangeBumperToken => baseCodeunitMetadataChangeBumperToken;

	internal int BaseTableMetadataChangeBumperToken => baseTableMetadataChangeBumperToken;

	private IEnumerable<Dictionary<int, NCLMetadataCacheEntry>> ValidMetadataCacheEntries => metadataCacheEntries.Where((Dictionary<int, NCLMetadataCacheEntry> e) => e != null);

	public NCLMetadata(NavDatabase ownerDatabase, INCLObjectXmlMetadataLoader xmlLoader, INCLCodeLoader coadLoader, INavAppClrTypeRetriever appClrTypeRetriever)
	{
		metadataLoader = NCLMetaApplicationObjectLoader.Create(this, xmlLoader, coadLoader, appClrTypeRetriever);
		appObjectChangeResolver = new Lazy<NavAppObjectChangeResolver>(() => new NavAppObjectChangeResolver(NavGlobal.NavAppGroupResolver));
		this.ownerDatabase = ownerDatabase;
		codeEnvironment = new ALCodeEnvironment((ApplicationObjectId id) =>
		{
			try
			{
				if (id.ObjectType.IsExtensionType())
				{
					return GetExtensionApplicationObject(id);
				}
				return GetMetaApplicationObject(id.ObjectType, id.ObjectNumber);
			}
			catch (NavNCLMetadataCSharpErrorException)
			{
				return (NCLMetaApplicationObject)null;
			}
		}, NavEnvironment.Instance.NavAppGroupResolver.GetExistingGroupById);
		int[] array = (int[])Enum.GetValues(typeof(ObjectType));
		int num = array.Max();
		metadataCacheEntries = new Dictionary<int, NCLMetadataCacheEntry>[num + 1];
		metadataExtensionCacheEntries = new Dictionary<int, NCLMetadataExtensionCacheEntry>[num + 1];
		for (int num2 = 0; num2 < array.Length; num2++)
		{
			metadataCacheEntries[array[num2]] = new Dictionary<int, NCLMetadataCacheEntry>(10);
			metadataExtensionCacheEntries[array[num2]] = new Dictionary<int, NCLMetadataExtensionCacheEntry>(10);
		}
		this.ownerDatabase.MetadataChanged += Database_ObjectMetadataChanged;
		this.ownerDatabase.NavAppObjectMetadataChanged += Database_NavAppObjectMetadataChanged;
		this.ownerDatabase.NavAppChanged += Database_NavAppChanged;
		this.ownerDatabase.NavAppPublishReferenceChanged += Database_NavAppPublishReferenceChanged;
		if (ServerUserSettings.Instance.EnableSymbolLoadingAtServerStartup.Value)
		{
			SymbolAggregator = new SymbolAggregator();
		}
	}

	internal void IncrementTableMetadataChangeBumperToken()
	{
		Interlocked.Increment(ref baseTableMetadataChangeBumperToken);
	}

	internal bool TryGetMetaTableById(int tableId, out NCLMetaTable metaTable, int appGroupId = -1, bool requireCompiled = true, bool onlySearchCache = false)
	{
		if (NavRecord.IsSystemTable(tableId))
		{
			appGroupId = 0;
		}
		metaTable = null;
		if (TryGetMetaApplicationObject(ObjectType.Table, tableId, out var metaApplicationObject, appGroupId, requireCompiled, onlySearchCache))
		{
			metaTable = metaApplicationObject as NCLMetaTable;
		}
		return metaTable != null;
	}

	public NCLMetaTable GetMetaTableById(int tableId, int appGroupId = -1, bool requireCompiled = true, bool onlySearchCache = false)
	{
		if (NavRecord.IsSystemTable(tableId))
		{
			appGroupId = 0;
		}
		return (NCLMetaTable)GetMetaApplicationObject(ObjectType.Table, tableId, appGroupId, requireCompiled, onlySearchCache);
	}

	public NCLMetaCodeunit GetMetaCodeunitById(int codeunitId, int appGroupId = -1, bool requireCompiled = true)
	{
		return (NCLMetaCodeunit)GetMetaApplicationObject(ObjectType.CodeUnit, codeunitId, appGroupId, requireCompiled);
	}

	public NCLMetaEnum GetMetaEnumById(int enumId, int appGroupId = -1, bool requireCompiled = true)
	{
		return (NCLMetaEnum)GetMetaApplicationObject(ObjectType.Enum, enumId, appGroupId, requireCompiled);
	}

	internal bool TryGetMetaFormById(int formId, out NCLMetaForm metaForm, int appGroupId = -1, bool requireCompiled = true, bool onlySearchCache = false)
	{
		metaForm = null;
		if (TryGetMetaApplicationObject(ObjectType.Page, formId, out var metaApplicationObject, appGroupId, requireCompiled, onlySearchCache))
		{
			metaForm = metaApplicationObject as NCLMetaForm;
		}
		return metaForm != null;
	}

	public NCLMetaForm GetMetaFormById(int formId, int appGroupId = -1, bool requireCompiled = true)
	{
		return (NCLMetaForm)GetMetaApplicationObject(ObjectType.Page, formId, appGroupId, requireCompiled);
	}

	public NCLMetaReport GetMetaReportById(int reportId, int appGroupId = -1, bool requireCompiled = true)
	{
		try
		{
			return (NCLMetaReport)GetMetaApplicationObject(ObjectType.Report, reportId, appGroupId, requireCompiled);
		}
		catch (NavNCLMetadataCSharpErrorException ex) when ("ReportVersionObsolete".Equals(ex.Errors, StringComparison.OrdinalIgnoreCase))
		{
			throw new NavReportVersionObsoleteException(reportId);
		}
	}

	public NCLMetaXmlPort GetMetaXmlPortById(int xmlPortId, int appGroupId = -1, bool requireCompiled = true)
	{
		return (NCLMetaXmlPort)GetMetaApplicationObject(ObjectType.XmlPort, xmlPortId, appGroupId, requireCompiled);
	}

	public NCLMetaQuery GetMetaQueryById(int queryId, int appGroupId = -1, bool requireCompiled = true)
	{
		return (NCLMetaQuery)GetMetaApplicationObject(ObjectType.Query, queryId, appGroupId, requireCompiled);
	}

	public NCLMetaTable GetMetaQueryTableById(int queryId, int appGroupId = -1, bool requireCompiled = true)
	{
		NCLMetaQuery metaQueryById = GetMetaQueryById(queryId, appGroupId, requireCompiled);
		return NCLMetaTable.CreateFromQuery(metaQueryById, metaQueryById.MetaQuery);
	}

	public virtual string GetTableNameByTableId(int tableId, bool onlySearchCache = false)
	{
		try
		{
			return GetMetaTableById(tableId, -1, requireCompiled: false, onlySearchCache)?.TableName;
		}
		catch (NavBaseException)
		{
			return tableId.ToString(CultureInfo.CurrentCulture);
		}
	}

	internal string GetObjectName(ApplicationObjectId objectId)
	{
		SortedList<ObjectType, SortedList<int, AllObjectSnapshotEntry>> snapshotOfAllObjects = GetSnapshotOfAllObjects();
		if (snapshotOfAllObjects.ContainsKey(objectId.ObjectType) && snapshotOfAllObjects[objectId.ObjectType].ContainsKey(objectId.ObjectNumber))
		{
			return snapshotOfAllObjects[objectId.ObjectType][objectId.ObjectNumber].Name;
		}
		return string.Empty;
	}

	public int GetObjectId(ObjectType objectType, string objectName)
	{
		SortedList<ObjectType, SortedList<int, AllObjectSnapshotEntry>> snapshotOfAllObjects = GetSnapshotOfAllObjects();
		if (snapshotOfAllObjects.ContainsKey(objectType))
		{
			return snapshotOfAllObjects[objectType].SingleOrDefault((KeyValuePair<int, AllObjectSnapshotEntry> o) => o.Value.Name.Equals(objectName)).Key;
		}
		return 0;
	}

	protected Dictionary<int, NCLMetadataCacheEntry> GetEntryDictionary(ObjectType applicationObjectType)
	{
		return GetObjectTypeDictionary(metadataCacheEntries, applicationObjectType);
	}

	protected Dictionary<int, NCLMetadataExtensionCacheEntry> GetExtensionEntryDictionary(ObjectType applicationObjectType)
	{
		return GetObjectTypeDictionary(metadataExtensionCacheEntries, applicationObjectType);
	}

	private static T GetObjectTypeDictionary<T>(T[] dictionaries, ObjectType applicationObjectType) where T : class, IDictionary
	{
		T val = ((applicationObjectType < ObjectType.TableData || (int)applicationObjectType >= dictionaries.Length) ? null : dictionaries[(int)applicationObjectType]);
		if (val == null)
		{
			throw new NavNCLArgumentException();
		}
		return val;
	}

	private NCLMetadataExtensionCacheEntry GetMetadataExtensionCacheEntry(ApplicationObjectId appObjectId, Func<ApplicationObjectId, NCLMetadataExtensionCacheEntry> entryCreator = null)
	{
		Dictionary<int, NCLMetadataExtensionCacheEntry> extensionEntryDictionary = GetExtensionEntryDictionary(appObjectId.ObjectType);
		NCLMetadataExtensionCacheEntry value;
		lock (extensionEntryDictionary)
		{
			if (!extensionEntryDictionary.TryGetValue(appObjectId.ObjectNumber, out value) && entryCreator != null)
			{
				NCLMetadataExtensionCacheEntry nCLMetadataExtensionCacheEntry = entryCreator(appObjectId);
				if (nCLMetadataExtensionCacheEntry != null)
				{
					extensionEntryDictionary.Add(appObjectId.ObjectNumber, nCLMetadataExtensionCacheEntry);
					value = nCLMetadataExtensionCacheEntry;
					return value;
				}
			}
		}
		return value;
	}

	private NCLMetadataCacheEntry GetMetadataCacheEntry(ApplicationObjectId appObjectId, Func<ApplicationObjectId, NCLMetadataCacheEntry> entryCreator = null, bool forceCreateNewOnStaleBaseObject = false)
	{
		Dictionary<int, NCLMetadataCacheEntry> entryDictionary = GetEntryDictionary(appObjectId.ObjectType);
		NCLMetadataCacheEntry value;
		lock (entryDictionary)
		{
			if (!entryDictionary.TryGetValue(appObjectId.ObjectNumber, out value) && entryCreator != null)
			{
				NCLMetadataCacheEntry nCLMetadataCacheEntry = entryCreator(appObjectId);
				if (nCLMetadataCacheEntry != null && !entryDictionary.TryGetValue(appObjectId.ObjectNumber, out value))
				{
					entryDictionary.Add(appObjectId.ObjectNumber, nCLMetadataCacheEntry);
					value = nCLMetadataCacheEntry;
				}
			}
			if (forceCreateNewOnStaleBaseObject && entryCreator != null && value != null && value.BaseObject != null && value.BaseObject.Stale)
			{
				NCLMetadataCacheEntry nCLMetadataCacheEntry2 = entryCreator(appObjectId);
				if (nCLMetadataCacheEntry2 != null)
				{
					entryDictionary[appObjectId.ObjectNumber] = nCLMetadataCacheEntry2;
					value = nCLMetadataCacheEntry2;
				}
			}
		}
		return value;
	}

	private IReadOnlyList<NCLMetadataCacheEntry> GetCacheSnapshot()
	{
		int num = ValidMetadataCacheEntries.Sum((Dictionary<int, NCLMetadataCacheEntry> d) => d.Count);
		List<NCLMetadataCacheEntry> list = new List<NCLMetadataCacheEntry>(num + 10);
		foreach (Dictionary<int, NCLMetadataCacheEntry> validMetadataCacheEntry in ValidMetadataCacheEntries)
		{
			lock (validMetadataCacheEntry)
			{
				list.AddRange(validMetadataCacheEntry.Values);
			}
		}
		return list;
	}

	public IEnumerable<ApplicationObjectId> SetObjectsStale(IEnumerable<ApplicationObjectId> changedObjectIds)
	{
		lock (appObjectInitializationOrChangeSyncRoot)
		{
			ApplicationObjectId[] first = changedObjectIds.Where((ApplicationObjectId obj) => obj.ObjectType != ObjectType.Query).ToArray();
			ApplicationObjectId[] second = changedObjectIds.Where((ApplicationObjectId obj) => obj.ObjectType == ObjectType.Query).ToArray();
			List<ApplicationObjectId> list = new List<ApplicationObjectId>(first.Concat(second));
			foreach (ApplicationObjectId item in list)
			{
				SetObjectStaleInternal(item);
			}
			return list.Concat(TraverseGraphCheckingForOtherStaleObjectsUntilAllAreMarked());
		}
	}

	public void ReloadTrackedNullAppObjectsForChanges(IEnumerable<NavAppObjectChange> appObjectChanges)
	{
		lock (appObjectInitializationOrChangeSyncRoot)
		{
			NavAppObjectChange[] first = appObjectChanges.Where((NavAppObjectChange obj) => obj.ObjectId.ObjectType != ObjectType.Query).ToArray();
			NavAppObjectChange[] second = appObjectChanges.Where((NavAppObjectChange obj) => obj.ObjectId.ObjectType == ObjectType.Query).ToArray();
			foreach (IGrouping<ApplicationObjectId, NavAppObjectChange> item in from c in first.Concat(second)
				group c by c.ObjectId)
			{
				ApplicationObjectId key = item.Key;
				ReloadTrackedNullAppObjects(key, item.Select((NavAppObjectChange g) => g.AppGroup.GroupId));
			}
		}
	}

	private IReadOnlyCollection<ApplicationObjectId> TraverseGraphCheckingForOtherStaleObjectsUntilAllAreMarked()
	{
		List<ApplicationObjectId> list = new List<ApplicationObjectId>();
		bool flag = true;
		while (flag)
		{
			IReadOnlyList<NCLMetadataCacheEntry> cacheSnapshot = GetCacheSnapshot();
			flag = false;
			foreach (NCLMetadataCacheEntry item in cacheSnapshot)
			{
				NCLMetaApplicationObject baseObject = item.BaseObject;
				if (baseObject != null)
				{
					lock (baseObject)
					{
						if (!baseObject.Stale && baseObject.CheckIsStale())
						{
							SetObjectStaleInternal(baseObject.ApplicationObjectId);
							list.Add(baseObject.ApplicationObjectId);
							flag = true;
						}
					}
				}
				IEnumerable<NavAppObjectEntry> appObjects = item.AppObjects;
				if (appObjects == null)
				{
					continue;
				}
				SharedAppObjectEntryLookup sharedAppObjectEntryLookup = new SharedAppObjectEntryLookup();
				foreach (NavAppObjectEntry item2 in appObjects)
				{
					NCLMetaApplicationObject groupObject = item2.GroupObject;
					if (groupObject != null)
					{
						SharedAppObjectEntry orCreateEntry = sharedAppObjectEntryLookup.GetOrCreateEntry(groupObject.MetadataAppGroup.GroupId, groupObject);
						orCreateEntry.AppGroupIdsPointingToThis.Add(item2.GroupId);
					}
				}
				foreach (SharedAppObjectEntry entry in sharedAppObjectEntryLookup.Entries)
				{
					NCLMetaApplicationObject oldSharedObject = entry.OldSharedObject;
					if (oldSharedObject != null && !oldSharedObject.Stale && oldSharedObject.CheckIsStale())
					{
						SetAppObjectStaleInternal(oldSharedObject.ApplicationObjectId, entry.AppGroupIdsPointingToThis);
						list.Add(oldSharedObject.ApplicationObjectId);
						flag = true;
					}
				}
			}
		}
		return list.AsReadOnly();
	}

	internal bool TryGetMetaApplicationObject(ObjectType objectType, int objectId, out NCLMetaApplicationObject metaApplicationObject, int appGroupId = -1, bool requireCompiled = true, bool onlySearchCache = false)
	{
		ApplicationObjectId appObjectId = new ApplicationObjectId(objectType, objectId);
		return TryGetMetaApplicationObject(appObjectId, out metaApplicationObject, appGroupId, requireCompiled, onlySearchCache);
	}

	internal bool TryGetMetaApplicationObject(ApplicationObjectId appObjectId, out NCLMetaApplicationObject metaApplicationObject, int appGroupId = -1, bool requireCompiled = true, bool onlySearchCache = false)
	{
		metaApplicationObject = null;
		try
		{
			metaApplicationObject = GetMetaApplicationObject(appObjectId, appGroupId, requireCompiled, onlySearchCache);
		}
		catch (NavMetadataNotFoundException)
		{
		}
		catch (NavNCLMetadataObjectNotFoundException)
		{
		}
		catch (NavNCLArgumentException)
		{
		}
		return metaApplicationObject != null;
	}

	public NCLMetaApplicationObject GetMetaApplicationObject(ObjectType objectType, int objectId, int appGroupId = -1, bool requireCompiled = true, bool onlySearchCache = false)
	{
		ApplicationObjectId appObjectId = new ApplicationObjectId(objectType, objectId);
		return GetMetaApplicationObject(appObjectId, appGroupId, requireCompiled, onlySearchCache);
	}

	public IEnumerable<NavAppGroup> GetAppGroupsThatOverrideBaseObject(ObjectType objectType, int objectId)
	{
		Dictionary<int, NCLMetadataCacheEntry> entryDictionary = GetEntryDictionary(objectType);
		lock (appObjectInitializationOrChangeSyncRoot)
		{
			lock (entryDictionary)
			{
				if (entryDictionary.TryGetValue(objectId, out var value) && value.BaseObject != null && value.AppObjects != null)
				{
					ImmutableHashSet<int> groupIds = (from o in value.AppObjects
						where o.GroupObject != null
						select o.GroupId).ToImmutableHashSet();
					return NavEnvironment.Instance.NavAppGroupResolver.GetExistingGroupsMatchingIds(groupIds).ToList();
				}
			}
		}
		return Enumerable.Empty<NavAppGroup>();
	}

	public IEnumerable<NCLMetaApplicationObject> GetExtensionApplicationObjects(ApplicationObjectId baseObjectId, int appGroupId = -1)
	{
		NavAppGroup navAppGroup = ((appGroupId == -1) ? NavCurrentThread.ResolveAppGroup() : NavEnvironment.Instance.NavAppGroupResolver.GetExistingGroupById(appGroupId));
		List<NCLMetaApplicationObject> list = new List<NCLMetaApplicationObject>();
		IEnumerable<Tuple<int, ApplicationObjectId>> enumerable = navAppGroup.OrderedAppMetadata.SelectMany((NavAppRuntimeMetadata m) => from s in m.ObjectMetadataSummaries
			where s.ObjectType.IsExtensionType() && s.TargetObjectType == baseObjectId.ObjectType && s.TargetObjectId == baseObjectId.ObjectNumber
			select Tuple.Create(m.RuntimeAppId, new ApplicationObjectId(s.ObjectType, s.ObjectId)));
		if (!enumerable.Any())
		{
			return Enumerable.Empty<NCLMetaApplicationObject>();
		}
		foreach (Tuple<int, ApplicationObjectId> item in enumerable)
		{
			NCLMetaApplicationObject extensionApplicationObject = GetExtensionApplicationObject(item.Item1, item.Item2);
			list.Add(extensionApplicationObject);
		}
		return list;
	}

	internal bool TryGetExtensionApplicationObjects(out IEnumerable<NCLMetaApplicationObject> extensionObjects, ApplicationObjectId baseObjectId, int appGroupId = -1)
	{
		IEnumerable<NCLMetaApplicationObject> enumerable = null;
		try
		{
			enumerable = GetExtensionApplicationObjects(baseObjectId, appGroupId);
		}
		catch (NavMetadataNotFoundException)
		{
		}
		catch (NavNCLMetadataObjectNotFoundException)
		{
		}
		catch (NavNCLArgumentException)
		{
		}
		extensionObjects = ((enumerable != null && enumerable.Any()) ? enumerable : null);
		return extensionObjects != null;
	}

	public NCLMetaApplicationObject GetExtensionApplicationObject(int runtimeAppId, ApplicationObjectId appObjectId)
	{
		NCLMetaApplicationObject extensionObjectInternal = GetExtensionObjectInternal(appObjectId, runtimeAppId);
		NavCurrentThread.Session?.SignalNavAppUsage(extensionObjectInternal?.OwningApp);
		return extensionObjectInternal;
	}

	public NCLMetaApplicationObject GetExtensionApplicationObject(ApplicationObjectId appObjectId, int appGroupId = -1)
	{
		NavAppRuntimeMetadata val = FindOwningAppForExtensionObject(appObjectId, appGroupId);
		if (val != null)
		{
			return GetExtensionObjectInternal(appObjectId, val.RuntimeAppId);
		}
		throw new NavNCLMetadataObjectNotFoundException(CultureInfo.CurrentCulture, appObjectId.ObjectType, appObjectId.ObjectNumber, null);
	}

	private static NavAppRuntimeMetadata FindOwningAppForExtensionObject(ApplicationObjectId appObjectId, int appGroupId)
	{
		NavAppGroup navAppGroup = ((appGroupId == -1) ? NavCurrentThread.ResolveAppGroup() : NavEnvironment.Instance.NavAppGroupResolver.GetExistingGroupById(appGroupId));
		return navAppGroup.OrderedAppMetadata.FirstOrDefault((NavAppRuntimeMetadata m) => m.ObjectMetadataSummaries.Any((NavAppObjectMetadataSummary oms) => oms.ObjectType == appObjectId.ObjectType && oms.ObjectId == appObjectId.ObjectNumber));
	}

	private NCLApplicationObjectExtension GetExtensionObjectInternal(ApplicationObjectId appObjectId, int runtimeAppId)
	{
		Dictionary<int, NCLMetadataExtensionCacheEntry> extensionEntryDictionary = GetExtensionEntryDictionary(appObjectId.ObjectType);
		lock (extensionEntryDictionary)
		{
			if (extensionEntryDictionary.TryGetValue(appObjectId.ObjectNumber, out var value))
			{
				NCLApplicationObjectExtension nCLApplicationObjectExtension = value.SpecificRuntimeAppObject(runtimeAppId);
				if (nCLApplicationObjectExtension != null)
				{
					return nCLApplicationObjectExtension;
				}
			}
		}
		throw new NavNCLMetadataObjectNotFoundException(CultureInfo.CurrentCulture, appObjectId.ObjectType, appObjectId.ObjectNumber, null);
	}

	public NCLMetaApplicationObject GetMetaApplicationObject(ApplicationObjectId appObjectId, int appGroupId = -1, bool requireCompiled = true, bool onlySearchCache = false)
	{
		NCLMetaApplicationObject nCLMetaApplicationObject = (appObjectId.ObjectType.IsExtensionType() ? GetExtensionApplicationObject(appObjectId, appGroupId) : GetMetaApplicationObjectInternal(appObjectId, requireCompiled, onlySearchCache, appGroupId));
		NavCurrentThread.Session?.SignalNavAppUsage(nCLMetaApplicationObject?.OwningApp);
		return nCLMetaApplicationObject;
	}

	private NCLMetaApplicationObject GetExistingMetaApplicationObjectFromApp(ApplicationObjectId appObjectId, int appGroupId)
	{
		if (appGroupId == 0 || appGroupId < 0)
		{
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001LO", Category.Extensions, new ArgumentException("This operation requires that a non-base app group ID be specified."));
		}
		Dictionary<int, NCLMetadataCacheEntry> entryDictionary = GetEntryDictionary(appObjectId.ObjectType);
		lock (entryDictionary)
		{
			if (!entryDictionary.TryGetValue(appObjectId.ObjectNumber, out var value))
			{
				string message = string.Format(CultureInfo.InvariantCulture, "No metadata cache entry was found -- Type:{0}; Id:{1}; Group:{2}", appObjectId.ObjectType, appObjectId.ObjectNumber, appGroupId);
				throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001LP", Category.Extensions, new InvalidOperationException(message));
			}
			NCLMetaApplicationObject nCLMetaApplicationObject = value.SpecificGroupObject(appGroupId);
			if (nCLMetaApplicationObject == null)
			{
				string message2 = string.Format(CultureInfo.InvariantCulture, "No cache entry group object was found -- Type:{0}; Id:{1}; Group:{2}", appObjectId.ObjectType, appObjectId.ObjectNumber, appGroupId);
				throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001LQ", Category.Extensions, new InvalidOperationException(message2));
			}
			return nCLMetaApplicationObject;
		}
	}

	private NCLMetaApplicationObject GetMetaApplicationObjectInternal(ApplicationObjectId appObjectId, bool requireCompiled = true, bool onlySearchCache = false, int appGroupId = -1, bool initializing = false)
	{
		Dictionary<int, NCLMetadataCacheEntry> entryDictionary = GetEntryDictionary(appObjectId.ObjectType);
		while (true)
		{
			NCLMetadataCacheEntry value = null;
			NCLMetaApplicationObject nCLMetaApplicationObject = null;
			NCLMetaApplicationObject nCLMetaApplicationObject2 = null;
			bool lockTaken = false;
			try
			{
				lock (entryDictionary)
				{
					if (!entryDictionary.TryGetValue(appObjectId.ObjectNumber, out value) || value.ShouldTryLazyLoadingBase)
					{
						if (onlySearchCache)
						{
							return null;
						}
						if (value != null)
						{
							nCLMetaApplicationObject2 = value.BaseObject;
							if (nCLMetaApplicationObject2 != null)
							{
								nCLMetaApplicationObject2.RemovedFromCache = true;
							}
						}
						nCLMetaApplicationObject = CreateEmptyMetaApplicationObject(appObjectId, NavAppGroup.BaseGroup);
						if (value != null && value.BaseObject != null)
						{
							entryDictionary[appObjectId.ObjectNumber] = NCLMetadataCacheEntry.CreateWithExistingAppEntries(nCLMetaApplicationObject, value);
						}
						else
						{
							entryDictionary[appObjectId.ObjectNumber] = NCLMetadataCacheEntry.CreateWithBase(nCLMetaApplicationObject);
						}
						Monitor.Enter(nCLMetaApplicationObject, ref lockTaken);
					}
					else
					{
						nCLMetaApplicationObject = value.SpecificGroupOrBaseObject(appGroupId);
						if (nCLMetaApplicationObject == null)
						{
							throw new NavNCLMetadataObjectNotFoundException(CultureInfo.CurrentCulture, appObjectId.ObjectType, appObjectId.ObjectNumber, null);
						}
						if (nCLMetaApplicationObject.Stale)
						{
							NCLMetaApplicationObject nCLMetaApplicationObject3 = CreateEmptyMetaApplicationObject(appObjectId, nCLMetaApplicationObject.MetadataAppGroup);
							value.ReplaceSingleGroupObject(appGroupId, nCLMetaApplicationObject3);
							nCLMetaApplicationObject = nCLMetaApplicationObject3;
						}
					}
				}
				bool flag = !nCLMetaApplicationObject.MetadataLoaded;
				bool flag2 = !nCLMetaApplicationObject.ClrTypeLoaded & requireCompiled;
				if (!flag && !flag2)
				{
					return nCLMetaApplicationObject;
				}
				if (!lockTaken)
				{
					Monitor.Enter(nCLMetaApplicationObject, ref lockTaken);
				}
				if (nCLMetaApplicationObject.RemovedFromCache)
				{
					continue;
				}
				flag = !nCLMetaApplicationObject.MetadataLoaded;
				flag2 = !nCLMetaApplicationObject.ClrTypeLoaded & requireCompiled;
				if (!flag && !flag2)
				{
					return nCLMetaApplicationObject;
				}
				try
				{
					if (flag)
					{
						nCLMetaApplicationObject.Populate();
					}
					if (flag2)
					{
						nCLMetaApplicationObject.CompileAndLoadClrObject();
					}
				}
				catch (Exception ex)
				{
					int groupId = NavCurrentThread.ResolveAppGroup().GroupId;
					if (initializing && appObjectId.ObjectType == ObjectType.Enum && ex is NavNCLMetadataObjectNotFoundException)
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00007YQ", Category.Extensions, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Warning during metadata ignored load -- Type:{0}; Id:{1}; MetadataGroup:{2}; MetadataLoaded:{3}; CLRLoaded:{4}; ThreadGroup:{5}", nCLMetaApplicationObject.ApplicationObjectType, nCLMetaApplicationObject.ObjectId, nCLMetaApplicationObject.MetadataAppGroup.GroupId, nCLMetaApplicationObject.MetadataLoaded, nCLMetaApplicationObject.ClrTypeLoaded, groupId);
						return null;
					}
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00007YQ", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, "Fatal error during metadata load -- Type:{0}; Id:{1}; MetadataGroup:{2}; MetadataLoaded:{3}; CLRLoaded:{4}; ThreadGroup:{5}", nCLMetaApplicationObject.ApplicationObjectType, nCLMetaApplicationObject.ObjectId, nCLMetaApplicationObject.MetadataAppGroup.GroupId, nCLMetaApplicationObject.MetadataLoaded, nCLMetaApplicationObject.ClrTypeLoaded, groupId);
					DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00007YR", Category.Extensions, ex);
					nCLMetaApplicationObject.Stale = true;
					nCLMetaApplicationObject.RemovedFromCache = true;
					Monitor.Exit(nCLMetaApplicationObject);
					lockTaken = false;
					throw;
				}
				if (nCLMetaApplicationObject2 != null)
				{
					codeEnvironment.OnBaseObjectTypeReplaced(nCLMetaApplicationObject2, nCLMetaApplicationObject);
				}
				return nCLMetaApplicationObject;
			}
			finally
			{
				if (lockTaken)
				{
					Monitor.Exit(nCLMetaApplicationObject);
				}
			}
		}
	}

	private NCLMetaApplicationObject CreateEmptyMetaApplicationObject(ApplicationObjectId appObjectId, NavAppGroup appGroup)
	{
		int appDependencyOrderForObject = appGroup.GetAppDependencyOrderForObject(appObjectId.ObjectType, appObjectId.ObjectNumber);
		return appObjectId.ObjectType switch
		{
			ObjectType.Table => (NCLMetaApplicationObject)NCLMetaTable.CreateEmptyNCLMetaTable(metadataLoader, appObjectId.ObjectNumber, appGroup, appDependencyOrderForObject), 
			ObjectType.CodeUnit => NCLMetaCodeunit.CreateEmptyNCLMetaCodeunit(metadataLoader, appObjectId.ObjectNumber, appGroup, appDependencyOrderForObject), 
			ObjectType.XmlPort => NCLMetaXmlPort.CreateEmptyNCLMetaXmlPort(metadataLoader, appObjectId.ObjectNumber, appGroup, appDependencyOrderForObject), 
			ObjectType.Page => NCLMetaForm.CreateEmptyNCLMetaForm(metadataLoader, appObjectId.ObjectNumber, appGroup, appDependencyOrderForObject), 
			ObjectType.Report => NCLMetaReport.CreateEmptyNCLMetaReport(metadataLoader, appObjectId.ObjectNumber, appGroup, appDependencyOrderForObject), 
			ObjectType.Query => NCLMetaQuery.CreateEmptyNCLMetaQuery(metadataLoader, appObjectId.ObjectNumber, appGroup, appDependencyOrderForObject), 
			ObjectType.Enum => NCLMetaEnum.CreateEmptyNCLMetaEnum(metadataLoader, appObjectId.ObjectNumber, appGroup, appDependencyOrderForObject), 
			_ => throw new InvalidOperationException("Cannot create empty object for type '" + appObjectId.ObjectType.ToString() + "'"), 
		};
	}

	private NCLApplicationObjectExtension CreateEmptyMetaApplicationExtensionObject(ApplicationObjectId appObjectId, NavAppRuntimeMetadata runtimeAppMetadata)
	{
		return appObjectId.ObjectType switch
		{
			ObjectType.TableExtension => (NCLApplicationObjectExtension)NCLTableExtension.CreateEmptyNCLTableExtension(metadataLoader, appObjectId.ObjectNumber, runtimeAppMetadata), 
			ObjectType.PageExtension => NCLPageExtension.CreateEmptyNCLPageExtension(metadataLoader, appObjectId.ObjectNumber, runtimeAppMetadata), 
			ObjectType.EnumExtension => NCLEnumExtension.CreateEmptyNCLEnumExtension(metadataLoader, appObjectId.ObjectNumber, runtimeAppMetadata), 
			_ => throw new InvalidOperationException("Cannot create empty extension object for type '" + appObjectId.ObjectType.ToString() + "'"), 
		};
	}

	private void Database_ObjectMetadataChanged(object sender, NavTrackingEventArgs eventArgs)
	{
		if (sender == null)
		{
			throw new ArgumentNullException("sender");
		}
		if (eventArgs == null)
		{
			throw new ArgumentNullException("eventArgs");
		}
		IEnumerable<ApplicationObjectId> changedObjectIds = eventArgs.TrackingObjects.Select((NavTrackingObject trackingObject) => new ApplicationObjectId(trackingObject.ObjectType, trackingObject.ObjectId));
		NavGlobal.EventSubscriptionMetadata.RebindEventSubscriptionsIfNeeded(() => SetObjectsStale(changedObjectIds));
		if (SymbolAggregator != null)
		{
			SymbolAggregator.OnMetadataChanged(eventArgs);
		}
		if (changedObjectIds.Any((ApplicationObjectId x) => x.ObjectType == ObjectType.Query))
		{
			MetadataProvider.UpdateQueryCategories(eventArgs);
		}
		NavGlobal.NavAppRoutesUpdater?.ConfigureRoutes(changedObjectIds);
	}

	private void Database_NavAppObjectMetadataChanged(object sender, NavTrackingEventArgs eventArgs)
	{
		IEnumerable<PackageId> enumerable = eventArgs.TrackingObjects.Select((NavTrackingObject to) => to.AppPackageId).Distinct();
		foreach (PackageId item in enumerable)
		{
			NavEnvironment.Instance.NavAppMetadataRetriever.NotifyAboutMetadataChange(item);
		}
		List<NavTrackingObject> trackingObjects = eventArgs.TrackingObjects.Where((NavTrackingObject to) => to.ChangeOperation == ObjectTrackingChangeOperation.Insert || to.ChangeOperation == ObjectTrackingChangeOperation.Update).ToList();
		IList<NavAppObjectChange> changesWithGroupContext = appObjectChangeResolver.Value.ResolveChangesFromTrackingObjects(trackingObjects).ToList();
		ReloadTrackedNullAppObjectsForChanges(changesWithGroupContext);
		NavGlobal.EventSubscriptionMetadata.RebindEventSubscriptionsIfNeeded(() => changesWithGroupContext.Select((NavAppObjectChange o) => o.ObjectId).ToList());
	}

	private void Database_NavAppChanged(object sender, NavTrackingEventArgs eventArgs)
	{
		IEnumerable<PackageId> enumerable = eventArgs.TrackingObjects.Select((NavTrackingObject to) => to.AppPackageId).Distinct();
		foreach (PackageId item in enumerable)
		{
			NavAppRuntimeMetadata metadata = NavEnvironment.Instance.NavAppMetadataRetriever.GetMetadata(item);
			if (metadata != null)
			{
				NavGlobal.SystemTenant.SymbolReferenceCache?.InvalidateModuleDefinition(metadata.Name, metadata.Publisher);
			}
			else
			{
				NavGlobal.SystemTenant.SymbolReferenceCache?.InvalidateModuleDefinition(item);
			}
			NavEnvironment.Instance.NavAppMetadataRetriever.NotifyAboutMetadataChange(item);
		}
	}

	private void Database_NavAppPublishReferenceChanged(object sender, NavTrackingEventArgs eventArgs)
	{
		IEnumerable<string> enumerable = eventArgs.TrackingObjects.Select((NavTrackingObject to) => to.TenantId).Distinct();
		foreach (string item in enumerable)
		{
			NavEnvironment.Instance.NavAppMetadataRetriever.NotifyAboutPublishReferenceChange(item);
		}
	}

	private void SetObjectStaleInternal(ApplicationObjectId objectId)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000009O", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "SetObjectStaleInternal -- Type:{0}; Id:{1}", objectId.ObjectType, objectId.ObjectNumber);
		SharedAppObjectEntryLookup sharedAppObjectEntryLookup = new SharedAppObjectEntryLookup();
		Tuple<NCLMetaApplicationObject, NCLMetaApplicationObject> tuple = new Tuple<NCLMetaApplicationObject, NCLMetaApplicationObject>(null, null);
		NCLMetadataCacheEntry metadataCacheEntry = GetMetadataCacheEntry(objectId);
		if (metadataCacheEntry != null)
		{
			IEnumerable<NavAppObjectEntry> appObjects = metadataCacheEntry.AppObjects;
			if (appObjects != null && appObjects.Any())
			{
				EagerLoadFreshObjectResult eagerLoadFreshObjectResult = TryToEagerLoadFreshBaseObject(objectId);
				NCLMetaApplicationObject nCLMetaApplicationObject = eagerLoadFreshObjectResult.MetaApplicationObject ?? metadataCacheEntry.BaseObject;
				NCLMetadataCacheEntry nCLMetadataCacheEntry = ((nCLMetaApplicationObject != null) ? NCLMetadataCacheEntry.CreateWithBase(nCLMetaApplicationObject) : NCLMetadataCacheEntry.CreateWithNoBase(objectId));
				tuple = Tuple.Create(metadataCacheEntry.BaseObject, eagerLoadFreshObjectResult.MetaApplicationObject);
				foreach (NavAppObjectEntry item in appObjects)
				{
					NCLMetaApplicationObject groupObject = item.GroupObject;
					SharedAppObjectEntry orCreateEntry = sharedAppObjectEntryLookup.GetOrCreateEntry(item.MetadataGroupId, groupObject);
					orCreateEntry.AppGroupIdsPointingToThis.Add(item.GroupId);
				}
				foreach (SharedAppObjectEntry entry in sharedAppObjectEntryLookup.Entries)
				{
					NavAppGroup existingGroupById = NavGlobal.NavAppGroupResolver.GetExistingGroupById(entry.MetadataAppGroup);
					bool flag = existingGroupById.OrderedAppMetadata.All((NavAppRuntimeMetadata app) => NavEnvironment.Instance.NavAppMetadataRetriever.GetMetadata(app.PackageId) != null);
					EagerLoadFreshObjectResult eagerLoadFreshObjectResult2 = EagerLoadFreshObjectResult.CreateNotFound();
					if ((eagerLoadFreshObjectResult.Outcome == EagerLoadFreshObjectOutcome.Success) & flag)
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000028O", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Modified base object reload - attempt reload case -- Type:{0}; Id:{1}; MetadataGroup:{2}; BaseOutcome:{3}; Pointers:{4}", objectId.ObjectType, objectId.ObjectNumber, entry.MetadataAppGroup, eagerLoadFreshObjectResult.Outcome, entry.AppGroupIdsPointingToThis.Count);
						eagerLoadFreshObjectResult2 = TryToEagerLoadFreshAppObject(nCLMetaApplicationObject, objectId, entry.MetadataAppGroup);
						entry.NewSharedObject = eagerLoadFreshObjectResult2.MetaApplicationObject;
					}
					else
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000293", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "App object reload skipped because the base reload failed or at least one required extension is not published -- Type:{0}; Id:{1}; MetadataGroup:{2}; BaseOutcome:{3}; Pointers:{4}", objectId.ObjectType, objectId.ObjectNumber, entry.MetadataAppGroup, eagerLoadFreshObjectResult.Outcome, entry.AppGroupIdsPointingToThis.Count);
					}
					if (eagerLoadFreshObjectResult2.Outcome == EagerLoadFreshObjectOutcome.Success || eagerLoadFreshObjectResult2.Outcome == EagerLoadFreshObjectOutcome.NotFound)
					{
						foreach (int appGroupIdsPointingToThi in entry.AppGroupIdsPointingToThis)
						{
							DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000028P", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Refreshing cache entry -- Type:{0}; Id:{1}; Group:{2}; MetadataGroup:{3}; OldWasNull:{4}; NewIsNull:{5}; Outcome:{6}", objectId.ObjectType, objectId.ObjectNumber, appGroupIdsPointingToThi, entry.MetadataAppGroup, metadataCacheEntry.SpecificGroupObject(appGroupIdsPointingToThi) == null, eagerLoadFreshObjectResult2.MetaApplicationObject == null, eagerLoadFreshObjectResult2.Outcome);
							nCLMetadataCacheEntry.AddNavAppObject(appGroupIdsPointingToThi, entry.MetadataAppGroup, eagerLoadFreshObjectResult2.MetaApplicationObject);
						}
					}
					else
					{
						if (eagerLoadFreshObjectResult2.Outcome != EagerLoadFreshObjectOutcome.FatalError)
						{
							continue;
						}
						foreach (int appGroupIdsPointingToThi2 in entry.AppGroupIdsPointingToThis)
						{
							NavAppGroup existingGroupById2 = NavGlobal.NavAppGroupResolver.GetExistingGroupById(appGroupIdsPointingToThi2);
							HandleFatalAppObjectInitializationForMultiTenantEnvironment(eagerLoadFreshObjectResult2.Exception, existingGroupById2);
						}
					}
				}
				Dictionary<int, NCLMetadataCacheEntry> entryDictionary = GetEntryDictionary(objectId.ObjectType);
				lock (entryDictionary)
				{
					entryDictionary[objectId.ObjectNumber] = nCLMetadataCacheEntry;
					metadataCacheEntry.MarkEverythingAsStale();
				}
			}
			else if (metadataCacheEntry.BaseObject != null)
			{
				metadataCacheEntry.BaseObject.Stale = true;
			}
		}
		if (tuple.Item1 != null && tuple.Item2 != null)
		{
			codeEnvironment.OnBaseObjectTypeReplaced(tuple.Item1, tuple.Item2);
		}
		foreach (SharedAppObjectEntry entry2 in sharedAppObjectEntryLookup.Entries)
		{
			NCLMetaApplicationObject oldSharedObject = entry2.OldSharedObject;
			NCLMetaApplicationObject newSharedObject = entry2.NewSharedObject;
			if (oldSharedObject != null && newSharedObject != null)
			{
				codeEnvironment.OnAppObjectTypeReplaced(oldSharedObject, newSharedObject, entry2.AppGroupIdsPointingToThis);
			}
		}
		codeEnvironment.OnObjectSourceUpdated(objectId);
		Interlocked.Increment(ref baseMetadataChangeBumperToken);
		if (objectId.ObjectType == ObjectType.Table)
		{
			NavGlobal.SystemTenant.SyncMetadataResolver.ClearCache();
			Interlocked.Increment(ref baseTableMetadataChangeBumperToken);
		}
		if (objectId.ObjectType == ObjectType.CodeUnit)
		{
			Interlocked.Increment(ref baseCodeunitMetadataChangeBumperToken);
		}
	}

	private void SetAppObjectStaleInternal(ApplicationObjectId objectId, IEnumerable<int> affectedAppGroupIds)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000009P", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "SetAppObjectStaleInternal -- Type:{0}; Id:{1}", objectId.ObjectType, objectId.ObjectNumber);
		EnsureAppObjectsTrackedByCacheAreLoaded(objectId, affectedAppGroupIds, markStaleAndOverwriteIfPresent: true);
		if (objectId.ObjectType == ObjectType.Table)
		{
			NavGlobal.SystemTenant.SyncMetadataResolver.ClearCache();
		}
	}

	private void ReloadTrackedNullAppObjects(ApplicationObjectId objectId, IEnumerable<int> affectedAppGroupIds)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000LR", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "ReloadNullAppObjectsTrackedByCacheStructure -- Type:{0}; Id:{1}", objectId.ObjectType, objectId.ObjectNumber);
		HashSet<ApplicationObjectId> hashSet = new HashSet<ApplicationObjectId>();
		if (objectId.ObjectType.IsExtensionType())
		{
			foreach (int affectedAppGroupId in affectedAppGroupIds)
			{
				NavAppGroup existingGroupById = NavEnvironment.Instance.NavAppGroupResolver.GetExistingGroupById(affectedAppGroupId);
				NavAppObjectMetadataSummary summary = existingGroupById.GetObjectSummary(objectId).Summary;
				ApplicationObjectId item = new ApplicationObjectId(summary.TargetObjectType, summary.TargetObjectId);
				hashSet.Add(item);
			}
		}
		else
		{
			hashSet.Add(objectId);
		}
		foreach (ApplicationObjectId item2 in hashSet)
		{
			EnsureAppObjectsTrackedByCacheAreLoaded(item2, affectedAppGroupIds, markStaleAndOverwriteIfPresent: false);
		}
	}

	private void EnsureAppObjectsTrackedByCacheAreLoaded(ApplicationObjectId objectId, IEnumerable<int> affectedAppGroupIds, bool markStaleAndOverwriteIfPresent)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000LS", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "EnsureAppObjectsTrackedByCacheAreLoaded -- Type:{0}; Id:{1}; MarkStaleAndOverwrite:{2}", objectId.ObjectType, objectId.ObjectNumber, markStaleAndOverwriteIfPresent);
		List<int> list = affectedAppGroupIds.ToList();
		SharedAppObjectEntryLookup sharedAppObjectEntryLookup = new SharedAppObjectEntryLookup();
		Tuple<NCLMetaApplicationObject, NCLMetaApplicationObject> tuple = new Tuple<NCLMetaApplicationObject, NCLMetaApplicationObject>(null, null);
		NCLMetadataCacheEntry metadataCacheEntry = GetMetadataCacheEntry(objectId);
		if (metadataCacheEntry != null)
		{
			IEnumerable<NavAppObjectEntry> appObjects = metadataCacheEntry.AppObjects;
			if (appObjects != null)
			{
				foreach (NavAppObjectEntry item in appObjects)
				{
					if (list.Contains(item.GroupId))
					{
						NCLMetaApplicationObject groupObject = item.GroupObject;
						SharedAppObjectEntry orCreateEntry = sharedAppObjectEntryLookup.GetOrCreateEntry(item.MetadataGroupId, groupObject);
						orCreateEntry.AppGroupIdsPointingToThis.Add(item.GroupId);
					}
				}
				foreach (SharedAppObjectEntry entry in sharedAppObjectEntryLookup.Entries)
				{
					if (!markStaleAndOverwriteIfPresent && entry.OldSharedObject != null)
					{
						continue;
					}
					NavAppGroup existingGroupById = NavGlobal.NavAppGroupResolver.GetExistingGroupById(entry.MetadataAppGroup);
					bool flag = existingGroupById.OrderedAppMetadata.All((NavAppRuntimeMetadata app) => NavEnvironment.Instance.NavAppMetadataRetriever.GetMetadata(app.PackageId) != null);
					EagerLoadFreshObjectResult eagerLoadFreshObjectResult = EagerLoadFreshObjectResult.CreateNotFound();
					if (flag)
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000028S", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "App object reload - attempt reload case -- Type:{0}; Id:{1}; MetadataGroup:{2}; Pointers:{3}", objectId.ObjectType, objectId.ObjectNumber, entry.MetadataAppGroup, entry.AppGroupIdsPointingToThis.Count);
						eagerLoadFreshObjectResult = TryToEagerLoadFreshAppObject(metadataCacheEntry.BaseObject, objectId, entry.MetadataAppGroup);
						entry.NewSharedObject = eagerLoadFreshObjectResult.MetaApplicationObject;
					}
					else
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000294", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "App object reload skipped because at least one required extension is not published -- Type:{0}; Id:{1}; MetadataGroup:{2}; Pointers:{3}", objectId.ObjectType, objectId.ObjectNumber, entry.MetadataAppGroup, entry.AppGroupIdsPointingToThis.Count);
					}
					if (eagerLoadFreshObjectResult.Outcome == EagerLoadFreshObjectOutcome.Success || eagerLoadFreshObjectResult.Outcome == EagerLoadFreshObjectOutcome.NotFound)
					{
						foreach (int appGroupIdsPointingToThi in entry.AppGroupIdsPointingToThis)
						{
							DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000028T", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Replacing cache entry -- Type:{0}; Id:{1}; Group:{2}; MetadataGroup:{3}; OldWasNull:{4}; NewIsNull:{5}; Outcome:{6}", objectId.ObjectType, objectId.ObjectNumber, appGroupIdsPointingToThi, entry.MetadataAppGroup, metadataCacheEntry.SpecificGroupObject(appGroupIdsPointingToThi) == null, eagerLoadFreshObjectResult.MetaApplicationObject == null, eagerLoadFreshObjectResult.Outcome);
							metadataCacheEntry.ReplaceSingleGroupObject(appGroupIdsPointingToThi, entry.NewSharedObject);
						}
						if (entry.OldSharedObject != null)
						{
							entry.OldSharedObject.Stale = true;
						}
						tuple = Tuple.Create(entry.OldSharedObject, eagerLoadFreshObjectResult.MetaApplicationObject);
					}
					else
					{
						if (eagerLoadFreshObjectResult.Outcome != EagerLoadFreshObjectOutcome.FatalError)
						{
							continue;
						}
						foreach (int appGroupIdsPointingToThi2 in entry.AppGroupIdsPointingToThis)
						{
							NavAppGroup existingGroupById2 = NavGlobal.NavAppGroupResolver.GetExistingGroupById(appGroupIdsPointingToThi2);
							HandleFatalAppObjectInitializationForMultiTenantEnvironment(eagerLoadFreshObjectResult.Exception, existingGroupById2);
						}
					}
				}
			}
		}
		if (tuple.Item1 != null && tuple.Item2 != null)
		{
			codeEnvironment.OnAppObjectTypeReplaced(tuple.Item1, tuple.Item2, list);
		}
		codeEnvironment.OnObjectSourceUpdated(objectId);
	}

	private EagerLoadFreshObjectResult TryToEagerLoadFreshBaseObject(ApplicationObjectId objectId)
	{
		try
		{
			NCLMetaApplicationObject nCLMetaApplicationObject = CreateEmptyMetaApplicationObject(objectId, NavAppGroup.BaseGroup);
			nCLMetaApplicationObject.Populate();
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000009Q", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Eager loading fresh base object :: success -- Type:{0}; Id:{1}", objectId.ObjectType, objectId.ObjectNumber);
			return EagerLoadFreshObjectResult.CreateSuccess(nCLMetaApplicationObject);
		}
		catch (NavMetadataNotFoundException)
		{
		}
		catch (NavNCLMetadataObjectNotFoundException)
		{
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000009R", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Eager loading base object :: not found -- Type:{0}; Id:{1}", objectId.ObjectType, objectId.ObjectNumber);
		return EagerLoadFreshObjectResult.CreateNotFound();
	}

	private EagerLoadFreshObjectResult TryToEagerLoadFreshAppObject(NCLMetaApplicationObject baseObject, ApplicationObjectId objectId, int appGroupId)
	{
		NavAppGroup existingGroupById = NavEnvironment.Instance.NavAppGroupResolver.GetExistingGroupById(appGroupId);
		if (baseObject != null && !baseObject.Stale && existingGroupById.GetObjectOwner(objectId.ObjectType, objectId.ObjectNumber) != null)
		{
			NavAppRuntimeMetadata objectOwner = existingGroupById.GetObjectOwner(objectId.ObjectType, objectId.ObjectNumber);
			string message = string.Format(CultureInfo.InvariantCulture, "The application object of type '{0}' with the ID '{1}' is defined in the app '{2}', but already exists in the base application.", objectId.ObjectType, objectId.ObjectNumber, objectOwner.TechnicalDisplayName);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000009S", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Eager loading fresh extension object :: conflict -- Type:{0}; Id:{1}; Group:{2}", objectId.ObjectType, objectId.ObjectNumber, existingGroupById.GroupId);
			Exception ex = new NavAppException(message);
			return EagerLoadFreshObjectResult.CreateFatalError(ex);
		}
		try
		{
			NCLMetaApplicationObject nCLMetaApplicationObject = CreateEmptyMetaApplicationObject(objectId, existingGroupById);
			NavAppGroupObjectMetadataSummary navAppGroupObjectMetadataSummary = existingGroupById.GroupObjectMetadataSummaries.First((NavAppGroupObjectMetadataSummary gs) => gs.ObjectType == objectId.ObjectType && gs.ObjectId == objectId.ObjectNumber);
			if (baseObject != null || navAppGroupObjectMetadataSummary.MetadataAppGroup.OrderedAppMetadata.Count > 2)
			{
				using (new NavAppGroupCurrentThreadOverrideScope(existingGroupById))
				{
					nCLMetaApplicationObject.Populate();
				}
			}
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000009T", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Eager loading fresh extension object :: success -- Type:{0}; Id:{1}; Group:{2}", objectId.ObjectType, objectId.ObjectNumber, existingGroupById.GroupId);
			return EagerLoadFreshObjectResult.CreateSuccess(nCLMetaApplicationObject);
		}
		catch (NavMetadataNotFoundException ex2)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000009U", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Eager loading fresh extension object :: not found -- Type:{0}; Id:{1}; Group:{2}", objectId.ObjectType, objectId.ObjectNumber, existingGroupById.GroupId, ex2.Message);
			EmitEagerLoadGracefullyHandledExceptionInfo(ex2);
			return EagerLoadFreshObjectResult.CreateNotFound();
		}
		catch (NavNCLMetadataObjectNotFoundException ex3)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000009V", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Eager loading fresh extension object :: not found -- Type:{0}; Id:{1}; Group:{2}; Error:{3}", objectId.ObjectType, objectId.ObjectNumber, existingGroupById.GroupId, ex3.Message);
			EmitEagerLoadGracefullyHandledExceptionInfo(ex3);
			return EagerLoadFreshObjectResult.CreateNotFound();
		}
		catch (NavNotSupportedException ex4)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001QX", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Eager loading fresh extension object :: not supported -- Type:{0}; Id:{1}; Group:{2}; Error:{3}", objectId.ObjectType, objectId.ObjectNumber, existingGroupById.GroupId, ex4.Message);
			EmitEagerLoadGracefullyHandledExceptionInfo(ex4);
			return EagerLoadFreshObjectResult.CreateNotFound();
		}
		catch (Exception ex5)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000028L", Category.Runtime, ex5, Verbosity.Error, DiagnosticParameter.SystemMetadata(string.Format(CultureInfo.InvariantCulture, "Eager loading fresh extension object :: error -- Type:{0}; Id:{1}; Group:{2}", objectId.ObjectType, objectId.ObjectNumber, existingGroupById.GroupId)));
			if (NavEnvironment.Instance.Tenants.IsSingleTenancyMode)
			{
				throw;
			}
			return EagerLoadFreshObjectResult.CreateFatalError(ex5);
		}
	}

	private static void EmitEagerLoadGracefullyHandledExceptionInfo(Exception ex)
	{
		for (Exception ex2 = ex; ex2 != null; ex2 = ex2.InnerException)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000028Y", Category.Runtime, ex2);
		}
	}

	private void HandleFatalAppObjectInitializationForMultiTenantEnvironment(Exception ex, NavAppGroup group)
	{
		if (group == null)
		{
			return;
		}
		List<NavTenant> list = new List<NavTenant>();
		try
		{
			list = (from t in NavEnvironment.Instance.Tenants.AllTenants()
				where t.NavAppGroup == @group
				select t).ToList();
		}
		catch (Exception exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000028J", Category.Extensions, exception);
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000007B", Category.Extensions, ex, Verbosity.Error, DiagnosticParameter.SystemMetadata(string.Format(CultureInfo.InvariantCulture, "Fatal metadata initialization issue is invalidating an app group -- Group:{0}; TenantsToDismount:{1}", group.GroupId, list.Count)));
		foreach (NavTenant item in list)
		{
			try
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000007C", Category.Extensions, Verbosity.Error, "Dismounting tenant '{0}' because of an app metadata conflict.", DiagnosticParameter.Oii(item.Id));
				NavTenantSettings tenantSettings = item.TenantSettings;
				NavEnvironment.Instance.Tenants.RemoveTenant(item.Id);
				NavEnvironment.Instance.Tenants.AddFailedTenant(tenantSettings);
			}
			catch (Exception exception2)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("0000220", Category.Extensions, exception2);
			}
		}
		try
		{
			NavGlobal.EventSubscriptionMetadata.RemoveForNavAppGroup(group);
			foreach (Dictionary<int, NCLMetadataCacheEntry> validMetadataCacheEntry in ValidMetadataCacheEntries)
			{
				lock (validMetadataCacheEntry)
				{
					foreach (NCLMetadataCacheEntry value in validMetadataCacheEntry.Values)
					{
						value.RemoveGroupObjectIfPresent(group.GroupId);
					}
				}
			}
			initializedAppGroups.Remove(group.GroupId);
			NavGlobal.NavAppRoutesUpdater?.InvalidateCache(group);
		}
		catch (Exception exception3)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("0000221", Category.Extensions, exception3);
		}
	}

	private static void CheckGroupForV1ExtensionAndRaiseFatalErrorIfFound(NavAppGroup group)
	{
		List<string> list = (from m in @group.OrderedAppMetadata
			where m.ExtensionType == NavExtensionType.CSide
			select m.TechnicalDisplayName).ToList();
		if (list.Any())
		{
			string message = "An unsupported attempt was made to use the following V1 extensions:" + Environment.NewLine + string.Join(Environment.NewLine, list);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001WC", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, message);
			throw new InvalidOperationException(message);
		}
	}

	internal ReadOnlyCollection<NCLMetaTable> GetSnapshotOfAllNonVirtualMetaTables(int appGroupId = -1)
	{
		int num = NCLMetadataCacheEntry.ResolveGroupId(appGroupId);
		Dictionary<int, ReadOnlyCollection<NCLMetaTable>> dictionary = allMetaTablesSnapshots;
		if (dictionary != null && baseTableSnapshotBumperToken == baseTableMetadataChangeBumperToken && dictionary.TryGetValue(num, out var value))
		{
			return value;
		}
		lock (allMetaTablesSnapshotSyncRoot)
		{
			int num2 = baseTableMetadataChangeBumperToken;
			dictionary = allMetaTablesSnapshots;
			if (dictionary != null && baseTableSnapshotBumperToken == baseTableMetadataChangeBumperToken && dictionary.TryGetValue(num, out value))
			{
				return value;
			}
			if (dictionary == null || baseTableSnapshotBumperToken != baseTableMetadataChangeBumperToken)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000PB", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Refreshing all table snapshots -- First:{0}; CurrentToken:{1}; SnapshotToken:{2}", dictionary == null, baseTableMetadataChangeBumperToken, baseTableSnapshotBumperToken);
				allMetaTablesSnapshots = null;
				baseTableSnapshotBumperToken = num2;
				dictionary = new Dictionary<int, ReadOnlyCollection<NCLMetaTable>>();
			}
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000PC", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Building all table snapshot for group -- Group:{0}", num);
			value = BuildAllNonVirtualMetaTableSnapshotListFromDatabase(num);
			dictionary.Add(num, value);
			allMetaTablesSnapshots = dictionary;
			return value;
		}
	}

	private ReadOnlyCollection<NCLMetaTable> BuildAllNonVirtualMetaTableSnapshotListFromDatabase(int targetAppGroupId)
	{
		List<NCLMetaTable> list = new List<NCLMetaTable>(2048);
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000000071, -1, requireCompiled: false);
		NCLMetaField fieldByNo = metaTableById.GetFieldByNo(6);
		NCLMetaField fieldByNo2 = metaTableById.GetFieldByNo(3);
		FilterExpression item = FilterExpression.Equal(NavOption.Create(fieldByNo2.FieldOptionMetadata, 1), fieldByNo2.ExpressionContext);
		PositionedFindProviderRequest request = new PositionedFindProviderRequest(0, metaTableById, DataLockState.None, new FiltersAndMarks(new FilterFieldDictionary(new Tuple<INavFieldMetadata, FilterExpression>(fieldByNo2, item))), null, SecurityFiltering.Ignored, FieldList.Empty, metaTableById.PrimaryKey.DefinedSortingFields, FindType.All, null, includeCurrent: false);
		using (NavMetadataAccessContext.GetAccessToRemoved(NavCurrentThread.Session, Guid.NewGuid().ToString("B")))
		{
			foreach (ReadOnlyRecordBuffer item2 in NavSqlIsamQueryCommand.FindAll(NavGlobal.AppDatabase, request, () => true))
			{
				int value = ((NavInteger)item2[fieldByNo.SqlColumnIndex]).Value;
				if (!SystemTables.ApplicationDatabaseTables.Contains(value) && !NavRecord.IsTableVirtual(value))
				{
					list.Add(GetMetaTableById(value, targetAppGroupId, requireCompiled: false));
				}
			}
			NavAppGroup existingGroupById = NavGlobal.NavAppGroupResolver.GetExistingGroupById(targetAppGroupId);
			if (existingGroupById != null)
			{
				List<NavAppObjectMetadataSummary> list2 = (from o in existingGroupById.GetDistinctNewObjectsAddedByApps()
					where o.ObjectType == ObjectType.Table
					select o).ToList();
				foreach (NavAppObjectMetadataSummary item3 in list2)
				{
					list.Add(GetMetaTableById(item3.ObjectId, targetAppGroupId, requireCompiled: false));
				}
			}
		}
		return list.AsReadOnly();
	}

	internal IEnumerable<NCLMetaTable> GetSnapshotOfAllNonVirtualMetaTablesExceptApplicationTables(int navAppGroup = -1)
	{
		return from table in GetSnapshotOfAllNonVirtualMetaTables(navAppGroup)
			where table.IsTenantTable
			select table;
	}

	internal SortedList<ObjectType, SortedList<int, AllObjectSnapshotEntry>> GetSnapshotOfAllObjects(int appGroupId = -1, bool elevateSessionPermissions = true)
	{
		int num = NCLMetadataCacheEntry.ResolveGroupId(appGroupId);
		Dictionary<int, SortedList<ObjectType, SortedList<int, AllObjectSnapshotEntry>>> dictionary = allObjectIdsSnapshots;
		if (dictionary != null && baseMetadataChangeBumperToken == baseAllObjectIdsSnapshotBumperToken && dictionary.TryGetValue(num, out var value))
		{
			return value;
		}
		lock (allObjectIdsSnapshotSyncRoot)
		{
			int num2 = baseMetadataChangeBumperToken;
			dictionary = allObjectIdsSnapshots;
			if (dictionary != null && baseMetadataChangeBumperToken == baseAllObjectIdsSnapshotBumperToken && dictionary.TryGetValue(num, out value))
			{
				return value;
			}
			SortedList<ObjectType, SortedList<int, AllObjectSnapshotEntry>> value2 = null;
			if (dictionary != null && baseMetadataChangeBumperToken == baseAllObjectIdsSnapshotBumperToken)
			{
				dictionary.TryGetValue(0, out value2);
			}
			if (dictionary == null || baseMetadataChangeBumperToken != baseAllObjectIdsSnapshotBumperToken)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000PD", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Refreshing all object snapshots -- First:{0}; CurrentToken:{1}; SnapshotToken:{2}", dictionary == null, baseMetadataChangeBumperToken, baseAllObjectIdsSnapshotBumperToken);
				allObjectIdsSnapshots = null;
				baseAllObjectIdsSnapshotBumperToken = num2;
				dictionary = new Dictionary<int, SortedList<ObjectType, SortedList<int, AllObjectSnapshotEntry>>>();
				value2 = BuildNewSnapshotListForBaseObjectsFromDatabase(elevateSessionPermissions);
				dictionary.Add(0, value2);
			}
			if (num == 0)
			{
				value = value2;
			}
			else
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000PE", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Building all object snapshot for group -- Group:{0}", num);
				value = BuildNewSnapshotListForAppGroup(value2, num);
				dictionary.Add(num, value);
			}
			allObjectIdsSnapshots = dictionary;
			return value;
		}
	}

	private SortedList<ObjectType, SortedList<int, AllObjectSnapshotEntry>> BuildNewSnapshotListForBaseObjectsFromDatabase(bool elevateSessionPermissions)
	{
		NCLMetaQueryDefinition queryDefinition = CreateObjectIdsWithNameQuery();
		SortedList<ObjectType, SortedList<int, AllObjectSnapshotEntry>> groupList = new SortedList<ObjectType, SortedList<int, AllObjectSnapshotEntry>>();
		ObjectType currentObjectType = (ObjectType)(-1);
		SortedList<int, AllObjectSnapshotEntry> currentObjectList = new SortedList<int, AllObjectSnapshotEntry>();
		Action action = () =>
		{
			foreach (ReadOnlyRecordBuffer item in NavSqlQueryCommand.QueryWithSingleScope(queryDefinition, NavGlobal.AppDatabase, 0, 0, 0, null, null, includeCurrent: false, FindType.All, null, SecurityFiltering.Ignored, addExplicitJoinHints: false))
			{
				ObjectType objectType = (ObjectType)item[0].ToInt32();
				int key = item[1].ToInt32();
				string name = item[2].ToString();
				if (objectType != currentObjectType)
				{
					if (currentObjectList.Count > 0)
					{
						groupList.Add(currentObjectType, currentObjectList);
						currentObjectList = new SortedList<int, AllObjectSnapshotEntry>();
					}
					currentObjectType = objectType;
				}
				AllObjectSnapshotEntry value2 = new AllObjectSnapshotEntry(name, null);
				currentObjectList.Add(key, value2);
			}
			if (currentObjectList.Count > 0)
			{
				groupList.Add(currentObjectType, currentObjectList);
			}
		};
		if (elevateSessionPermissions)
		{
			using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000001 }))
			{
				action();
			}
		}
		else
		{
			action();
		}
		SortedList<int, AllObjectSnapshotEntry> value = new SortedList<int, AllObjectSnapshotEntry>(SystemObjectAndNames.ToDictionary((KeyValuePair<int, string> o) => o.Key, (KeyValuePair<int, string> o) => new AllObjectSnapshotEntry(o.Value, null)));
		groupList.Add(ObjectType.System, value);
		return groupList;
	}

	private static SortedList<ObjectType, SortedList<int, AllObjectSnapshotEntry>> BuildNewSnapshotListForAppGroup(SortedList<ObjectType, SortedList<int, AllObjectSnapshotEntry>> allObjectsForBase, int appGroupId)
	{
		SortedList<ObjectType, SortedList<int, AllObjectSnapshotEntry>> allObjectsForAppGroup = new SortedList<ObjectType, SortedList<int, AllObjectSnapshotEntry>>();
		allObjectsForBase.ForEach((KeyValuePair<ObjectType, SortedList<int, AllObjectSnapshotEntry>> k) =>
		{
			allObjectsForAppGroup.Add(k.Key, new SortedList<int, AllObjectSnapshotEntry>(k.Value.ToDictionary((KeyValuePair<int, AllObjectSnapshotEntry> v) => v.Key, (KeyValuePair<int, AllObjectSnapshotEntry> v) => v.Value)));
		});
		ObjectType[] array = new ObjectType[11]
		{
			ObjectType.Table,
			ObjectType.Report,
			ObjectType.CodeUnit,
			ObjectType.XmlPort,
			ObjectType.MenuSuite,
			ObjectType.Page,
			ObjectType.Query,
			ObjectType.PageExtension,
			ObjectType.TableExtension,
			ObjectType.Enum,
			ObjectType.EnumExtension
		};
		ObjectType[] array2 = array;
		foreach (ObjectType objectType in array2)
		{
			if (!allObjectsForAppGroup.TryGetValue(objectType, out var value))
			{
				value = new SortedList<int, AllObjectSnapshotEntry>();
			}
			AddNewAppObjectsToSnapshotList(value, objectType, appGroupId);
			if (value.Count > 0 && !allObjectsForAppGroup.ContainsKey(objectType))
			{
				allObjectsForAppGroup.Add(objectType, value);
			}
		}
		return allObjectsForAppGroup;
	}

	private static void AddNewAppObjectsToSnapshotList(SortedList<int, AllObjectSnapshotEntry> list, ObjectType type, int appGroupId)
	{
		NavAppGroup existingGroupById = NavGlobal.NavAppGroupResolver.GetExistingGroupById(appGroupId);
		if (existingGroupById != null)
		{
			IEnumerable<NavAppObjectMetadataSummary> newObjectsToAdd = from o in existingGroupById.GetDistinctNewObjectsAddedByApps()
				where o.ObjectType == type
				select o;
			AddNewAppObjectsToShapshotListFromCollection(list, newObjectsToAdd);
			IEnumerable<NavAppObjectMetadataSummary> newObjectsToAdd2 = from o in existingGroupById.GetExtensionObjects()
				select o.Summary into o
				where o.ObjectType == type
				select o;
			AddNewAppObjectsToShapshotListFromCollection(list, newObjectsToAdd2);
		}
	}

	private static void AddNewAppObjectsToShapshotListFromCollection(SortedList<int, AllObjectSnapshotEntry> list, IEnumerable<NavAppObjectMetadataSummary> newObjectsToAdd)
	{
		foreach (NavAppObjectMetadataSummary item in newObjectsToAdd)
		{
			if (!list.ContainsKey(item.ObjectId))
			{
				AllObjectSnapshotEntry value = new AllObjectSnapshotEntry(item.ObjectName, ((WrappedValue<Guid>)(object)item.AppPackageId).Value);
				list.Add(item.ObjectId, value);
			}
		}
	}

	private NCLMetaQueryDefinition CreateObjectIdsWithNameQuery()
	{
		NCLMetaTable objectMetadataTable = GetMetaTableById(2000000071);
		NCLMetaField objectTypeField = objectMetadataTable.GetFieldByNo(3);
		NCLMetaField objectIdField = objectMetadataTable.GetFieldByNo(6);
		NCLMetaTable metaTableById = GetMetaTableById(2000000001);
		NCLMetaField objectNameField = metaTableById.GetFieldByNo(4);
		FilterExpression filters = FilterExpression.Or(FilterExpression.NotEqual(NavOption.Create(objectTypeField.FieldOptionMetadata, 7), objectTypeField.ExpressionContext), FilterExpression.NotEqual(NavInteger.Create(2100000000), objectIdField.ExpressionContext));
		Func<NCLMetaQueryDataItem, FiltersAndMarks> filtersCreator = (NCLMetaQueryDataItem dataItemBeingCreated) => new FiltersAndMarks(new FilterFieldDictionary(new Tuple<INavFieldMetadata, FilterExpression>(FilterFieldDictionary.MultipleFields, filters)));
		return NCLMetaQueryDefinition.Create(0, (NCLMetaQueryDefinition queryBeingCreated) => NCLMetaQueryDataItemCollection.Create(queryBeingCreated, objectMetadataTable, DataLockState.None, "ObjectMetadata", (NCLMetaQueryDataItem dataItemBeingCreated) => new FieldList(NCLMetaQueryColumn.Create(dataItemBeingCreated, objectTypeField, filterOnly: false), NCLMetaQueryColumn.Create(dataItemBeingCreated, objectIdField, filterOnly: false)), filtersCreator).LeftOuterJoin(queryBeingCreated, 2000000001, DataLockState.None, "Object", (NCLMetaQueryDataItem dataItemBeingCreated) => new FieldList(NCLMetaQueryColumn.Create(dataItemBeingCreated, objectNameField, filterOnly: false)), null, (NCLMetaQueryDataItem dataItemBeingCreated) => new ReadOnlyCollection<NCLMetaDataItemLink>(new NCLMetaDataItemLink[2]
		{
			NCLMetaDataItemLink.CreateFieldEqualsField(dataItemBeingCreated, "ObjectMetadata", 3, 1, NCLMetaFilterModes.None),
			NCLMetaDataItemLink.CreateFieldEqualsField(dataItemBeingCreated, "ObjectMetadata", 6, 3, NCLMetaFilterModes.None)
		})), null);
	}

	public void InitializeAppGroup(NavAppGroup appGroup)
	{
		if (appGroup == NavAppGroup.BaseGroup)
		{
			return;
		}
		CheckGroupForV1ExtensionAndRaiseFatalErrorIfFound(appGroup);
		NavSession session = NavCurrentThread.Session;
		bool initializingAppGroup = NavCurrentThread.InitializingAppGroup;
		try
		{
			lock (appObjectInitializationOrChangeSyncRoot)
			{
				if (initializedAppGroups.Contains(appGroup.GroupId))
				{
					return;
				}
				IReadOnlyList<SpecificNavAppObjectMetadataSummaryPair> extensionObjects = appGroup.GetExtensionObjects();
				IReadOnlyList<NavAppGroupObjectMetadataSummary> readOnlyList = appGroup.GroupObjectMetadataSummaries.Where((NavAppGroupObjectMetadataSummary s) => s.ObjectType != ObjectType.MenuSuite && !s.OriginatesFromBase).ToList();
				IReadOnlyList<NavAppGroupObjectMetadataSummary> readOnlyList2 = appGroup.GroupObjectMetadataSummaries.Where((NavAppGroupObjectMetadataSummary s) => s.ObjectType != ObjectType.MenuSuite && s.OriginatesFromBase).ToList();
				IEnumerable<NavAppGroup> enumerable = from g in (from s in readOnlyList.Concat(readOnlyList2)
						select s.MetadataAppGroup).Distinct()
					where g != appGroup
					select g;
				foreach (NavAppGroup item in enumerable)
				{
					if (!initializedAppGroups.Contains(item.GroupId))
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000SW", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Initializing sub group -- ParentGroup:{0}; SubGroup:{1}", appGroup.GroupId, item.GroupId);
						InitializeAppGroup(item);
					}
				}
				NavCurrentThread.InitializingAppGroup = true;
				NavCurrentThread.Session = null;
				SortedList<ObjectType, SortedList<int, AllObjectSnapshotEntry>> snapshotOfAllObjects = GetSnapshotOfAllObjects(0, elevateSessionPermissions: false);
				InitializeAppGroupProfilerSession initializeAppGroupProfilerSession = new InitializeAppGroupProfilerSession();
				Stopwatch stopwatch = Stopwatch.StartNew();
				PerformAppGroupConflictChecks(appGroup, snapshotOfAllObjects, readOnlyList);
				using (new NavAppGroupCurrentThreadOverrideScope(appGroup))
				{
					string content = string.Format(CultureInfo.InvariantCulture, "Initializing NCL extension group -- Group:{0}", appGroup.GroupId);
					using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("000007D", DiagnosticParameter.SystemMetadata(content), MonitorType.SqlReadCount, Category.Extensions, createNewActivity: false))
					{
						if (readOnlyList.Any() || readOnlyList2.Any())
						{
							InitializeExtensionObjects(initializeAppGroupProfilerSession, extensionObjects);
							InitializeNewAppObjectsExceptQueries(appGroup, initializeAppGroupProfilerSession, readOnlyList);
							InitializeModifiedBaseObjects(appGroup, initializeAppGroupProfilerSession, readOnlyList2);
							InitializeNewAppQueries(appGroup, initializeAppGroupProfilerSession, readOnlyList);
						}
						initializedAppGroups.Add(appGroup.GroupId);
					}
					initializeAppGroupProfilerSession.RecordTotalTime(stopwatch.Elapsed);
					profiler.ProcessCompletedSession(initializeAppGroupProfilerSession);
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000734", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, InitializeAppGroupProfiler.GenerateTelemetryForSession(appGroup, initializeAppGroupProfilerSession));
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000020Z", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, profiler.GenerateOverallTelemetry());
				}
			}
		}
		catch (Exception exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000009W", Category.Extensions, exception);
			throw;
		}
		finally
		{
			NavCurrentThread.InitializingAppGroup = initializingAppGroup;
			NavCurrentThread.Session = session;
		}
	}

	private static void PerformAppGroupConflictChecks(NavAppGroup appGroup, SortedList<ObjectType, SortedList<int, AllObjectSnapshotEntry>> allBaseEntries, IReadOnlyList<NavAppGroupObjectMetadataSummary> newObjectsFromApps)
	{
		EnsureAppObjectsDoNotUseIdsOfBaseObjects(allBaseEntries, newObjectsFromApps);
		EnsureAllObjectNamesWillBeUnique(allBaseEntries, newObjectsFromApps);
		EnsureAllExtensionObjectIdsWillBeUniqueWithinTypes(appGroup);
	}

	private void InitializeExtensionObjects(InitializeAppGroupProfilerSession profilerSession, IReadOnlyList<SpecificNavAppObjectMetadataSummaryPair> extensionSummariesWeCareAbout)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000QM", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Loading extension objects");
		foreach (SpecificNavAppObjectMetadataSummaryPair item in extensionSummariesWeCareAbout)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			bool constructedNew = InitializeAppGroupExtension(item.RuntimeMetadata, item.Summary);
			profilerSession.RecordObjectTiming(item.Summary.ObjectType, constructedNew, stopwatch.Elapsed);
		}
	}

	private void InitializeNewAppObjectsExceptQueries(NavAppGroup appGroup, InitializeAppGroupProfilerSession profilerSession, IReadOnlyList<NavAppGroupObjectMetadataSummary> newObjectsFromApps)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000QN", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Loading new objects (no queries)");
		foreach (NavAppGroupObjectMetadataSummary item in newObjectsFromApps.Where((NavAppGroupObjectMetadataSummary s) => s.ObjectType != ObjectType.Query))
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			bool constructedNew = InitializeAppGroupNewObject(appGroup, item);
			profilerSession.RecordObjectTiming(item.ObjectType, constructedNew, stopwatch.Elapsed);
		}
	}

	private void InitializeModifiedBaseObjects(NavAppGroup appGroup, InitializeAppGroupProfilerSession profilerSession, IReadOnlyList<NavAppGroupObjectMetadataSummary> baseObjectsModifiedBySomething)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000QO", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Loading modified objects");
		foreach (NavAppGroupObjectMetadataSummary item in baseObjectsModifiedBySomething)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			bool constructedNew = InitializeModifiedBaseObject(appGroup, item);
			profilerSession.RecordObjectTiming(item.ObjectType, constructedNew, stopwatch.Elapsed);
		}
	}

	private void InitializeNewAppQueries(NavAppGroup appGroup, InitializeAppGroupProfilerSession profilerSession, IReadOnlyList<NavAppGroupObjectMetadataSummary> newObjectsFromApps)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000QP", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Loading queries");
		foreach (NavAppGroupObjectMetadataSummary item in newObjectsFromApps.Where((NavAppGroupObjectMetadataSummary s) => s.ObjectType == ObjectType.Query))
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			bool constructedNew = InitializeAppGroupNewObject(appGroup, item);
			profilerSession.RecordObjectTiming(item.ObjectType, constructedNew, stopwatch.Elapsed);
		}
	}

	private bool InitializeModifiedBaseObject(NavAppGroup appGroup, NavAppGroupObjectMetadataSummary modifiedSummary)
	{
		ApplicationObjectId appObjectId = new ApplicationObjectId(modifiedSummary.ObjectType, modifiedSummary.ObjectId);
		NCLMetaApplicationObject baseObj = GetMetaApplicationObjectInternal(appObjectId, requireCompiled: true, onlySearchCache: false, NavAppGroup.BaseGroup.GroupId, initializing: true);
		if (baseObj == null && appObjectId.ObjectType == ObjectType.Enum)
		{
			return false;
		}
		NCLMetadataCacheEntry metadataCacheEntry = GetMetadataCacheEntry(appObjectId, (ApplicationObjectId id) => NCLMetadataCacheEntry.CreateWithBase(baseObj), forceCreateNewOnStaleBaseObject: true);
		if (metadataCacheEntry.SpecificGroupObject(appGroup.GroupId) == null)
		{
			if (appGroup.GroupId == modifiedSummary.MetadataAppGroup.GroupId)
			{
				NCLMetaApplicationObject nCLMetaApplicationObject = CreateEmptyMetaApplicationObject(appObjectId, appGroup);
				nCLMetaApplicationObject.ValidateMetadata();
				metadataCacheEntry.AddNavAppObject(appGroup.GroupId, modifiedSummary.MetadataAppGroup.GroupId, nCLMetaApplicationObject);
				return true;
			}
			NCLMetaApplicationObject existingMetaApplicationObjectFromApp = GetExistingMetaApplicationObjectFromApp(appObjectId, modifiedSummary.MetadataAppGroup.GroupId);
			metadataCacheEntry.AddNavAppObject(appGroup.GroupId, modifiedSummary.MetadataAppGroup.GroupId, existingMetaApplicationObjectFromApp);
			return false;
		}
		return false;
	}

	private bool InitializeAppGroupExtension(NavAppRuntimeMetadata runtimeMetadata, NavAppObjectMetadataSummary extensionSummary)
	{
		ApplicationObjectId appObjectId = new ApplicationObjectId(extensionSummary.ObjectType, extensionSummary.ObjectId);
		NCLMetadataExtensionCacheEntry metadataExtensionCacheEntry = GetMetadataExtensionCacheEntry(appObjectId, (ApplicationObjectId createId) => new NCLMetadataExtensionCacheEntry(createId));
		if (metadataExtensionCacheEntry.SpecificRuntimeAppObject(runtimeMetadata.RuntimeAppId) == null)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000QS", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Initializing extension object -- Type:{0}; Id:{1}; TargetType:{2}; TargetId:{3}; Name:{4}; Publisher:{5}; Version:{6}", extensionSummary.ObjectType, extensionSummary.ObjectId, extensionSummary.TargetObjectType, extensionSummary.TargetObjectId, runtimeMetadata.Name, runtimeMetadata.Publisher, runtimeMetadata.Version);
			NCLApplicationObjectExtension nCLApplicationObjectExtension = CreateEmptyMetaApplicationExtensionObject(appObjectId, runtimeMetadata);
			nCLApplicationObjectExtension.Populate();
			metadataExtensionCacheEntry.AddNavAppExtensionObject(runtimeMetadata.RuntimeAppId, nCLApplicationObjectExtension);
			return true;
		}
		return false;
	}

	private bool InitializeAppGroupNewObject(NavAppGroup appGroup, NavAppGroupObjectMetadataSummary newObjectSummary)
	{
		ApplicationObjectId appObjectId = new ApplicationObjectId(newObjectSummary.ObjectType, newObjectSummary.ObjectId);
		NCLMetadataCacheEntry metadataCacheEntry = GetMetadataCacheEntry(appObjectId, NCLMetadataCacheEntry.CreateWithNoBase, forceCreateNewOnStaleBaseObject: true);
		if (metadataCacheEntry.BaseObject != null && !metadataCacheEntry.BaseObject.Stale)
		{
			ThrowExceptionForDuplicateAppAndBaseObjectById(newObjectSummary);
		}
		if (appGroup.GroupId == newObjectSummary.MetadataAppGroup.GroupId)
		{
			NCLMetaApplicationObject nCLMetaApplicationObject = CreateEmptyMetaApplicationObject(appObjectId, appGroup);
			if (newObjectSummary.MetadataAppGroup.OrderedAppMetadata.Count > 1)
			{
				nCLMetaApplicationObject.ValidateMetadata();
			}
			metadataCacheEntry.AddNavAppObject(appGroup.GroupId, newObjectSummary.MetadataAppGroup.GroupId, nCLMetaApplicationObject);
			return true;
		}
		NCLMetaApplicationObject existingMetaApplicationObjectFromApp = GetExistingMetaApplicationObjectFromApp(appObjectId, newObjectSummary.MetadataAppGroup.GroupId);
		metadataCacheEntry.AddNavAppObject(appGroup.GroupId, newObjectSummary.MetadataAppGroup.GroupId, existingMetaApplicationObjectFromApp);
		return false;
	}

	private static void ThrowExceptionForDuplicateAppAndBaseObjectById(NavAppGroupObjectMetadataSummary newObject)
	{
		NavAppRuntimeMetadata fullObjectOwner = newObject.FullObjectOwner;
		string message = string.Format(CultureInfo.InvariantCulture, "The application object of type '{0}' with the ID '{1}' is defined in the app '{2}', but already exists in the base application.", newObject.ObjectType, newObject.ObjectId, fullObjectOwner.TechnicalDisplayName);
		throw new NavAppException(message);
	}

	private static void EnsureAppObjectsDoNotUseIdsOfBaseObjects(SortedList<ObjectType, SortedList<int, AllObjectSnapshotEntry>> baseEntries, IEnumerable<NavAppGroupObjectMetadataSummary> newAppObjects)
	{
		foreach (IGrouping<ObjectType, NavAppGroupObjectMetadataSummary> item in from o in newAppObjects
			group o by o.ObjectType)
		{
			ObjectType key = item.Key;
			if (!baseEntries.TryGetValue(key, out var value))
			{
				continue;
			}
			foreach (NavAppGroupObjectMetadataSummary item2 in item)
			{
				if (value.ContainsKey(item2.ObjectId))
				{
					ThrowExceptionForDuplicateAppAndBaseObjectById(item2);
				}
			}
		}
	}

	private static void EnsureAllObjectNamesWillBeUnique(SortedList<ObjectType, SortedList<int, AllObjectSnapshotEntry>> baseEntries, IEnumerable<NavAppGroupObjectMetadataSummary> newAppObjects)
	{
		foreach (IGrouping<ObjectType, NavAppGroupObjectMetadataSummary> item in from o in newAppObjects
			group o by o.ObjectType)
		{
			ObjectType key = item.Key;
			if (!baseEntries.TryGetValue(key, out var value))
			{
				continue;
			}
			HashSet<string> hashSet = new HashSet<string>(value.Values.Select((AllObjectSnapshotEntry e) => e.Name));
			foreach (NavAppGroupObjectMetadataSummary item2 in item)
			{
				if (hashSet.Contains(item2.ObjectName))
				{
					string message = string.Format(CultureInfo.InvariantCulture, "The object name '{0}' can only appear once for the type '{1}'.", item2.ObjectName, key);
					throw NavAppObjectMetadataException.Create(message);
				}
				hashSet.Add(item2.ObjectName);
			}
		}
	}

	private static void EnsureAllExtensionObjectIdsWillBeUniqueWithinTypes(NavAppGroup appGroup)
	{
		ILookup<ApplicationObjectId, NavAppObjectMetadataSummary> source = (from metadata in appGroup.OrderedAppMetadata
			from result in metadata.ObjectMetadataSummaries
			select result).ToLookup((NavAppObjectMetadataSummary s) => new ApplicationObjectId(s.ObjectType, s.ObjectId));
		using IEnumerator<IGrouping<ApplicationObjectId, NavAppObjectMetadataSummary>> enumerator = source.Where((IGrouping<ApplicationObjectId, NavAppObjectMetadataSummary> item) => item.Count() > 1).GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return;
		}
		IGrouping<ApplicationObjectId, NavAppObjectMetadataSummary> current = enumerator.Current;
		NavAppObjectMetadataSummary summary = current.First();
		string arg = string.Join("; ", from m in appGroup.OrderedAppMetadata
			where m.ObjectMetadataSummaries.Any((NavAppObjectMetadataSummary s) => s.ObjectType == summary.ObjectType && s.ObjectId == summary.ObjectId)
			select m.TechnicalDisplayName);
		string message = string.Format(CultureInfo.InvariantCulture, "The application object of type '{0}' with the ID '{1}' is defined in multiple apps. The apps are: {2}.", summary.ObjectType, summary.ObjectId, arg);
		throw new NavAppException(message);
	}
}
