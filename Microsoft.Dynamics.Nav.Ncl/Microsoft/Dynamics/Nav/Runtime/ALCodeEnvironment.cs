using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class ALCodeEnvironment
{
	private class ObjectTypeIdManager
	{
		private const int SequentialIdFlag = 16384;

		private readonly Dictionary<int, int> actualIdToSequentialId = new Dictionary<int, int>();

		private readonly List<int> sequentialIdToActualId = new List<int>();

		public int GetActualObjectId(int compressedObjectId)
		{
			if (compressedObjectId < 16384)
			{
				return compressedObjectId;
			}
			int num = compressedObjectId & -16385;
			lock (this)
			{
				if (num >= 0 && num < sequentialIdToActualId.Count)
				{
					return sequentialIdToActualId[num];
				}
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000010H", Category.Runtime, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Code environment cannot fetch the actual application object id for compressed ID 0x{0:X}, sequential ID = {1}, ID count = {2}", compressedObjectId, num, sequentialIdToActualId.Count);
				return 0;
			}
		}

		public int GetCompressedObjectId(int actualId)
		{
			if (actualId < 16384)
			{
				return actualId;
			}
			lock (this)
			{
				if (!actualIdToSequentialId.TryGetValue(actualId, out var value))
				{
					int count = sequentialIdToActualId.Count;
					if (count >= 16384)
					{
						throw new InvalidOperationException("An internal error has occurred. Cannot assign a scope id. The maximum number of sequential ids has been exhausted.");
					}
					value = count | 0x4000;
					sequentialIdToActualId.Add(actualId);
					actualIdToSequentialId.Add(actualId, value);
				}
				return value;
			}
		}
	}

	internal const uint DefaultRawScopeId = 0u;

	internal const uint HasBreakpointFlag = 1u;

	private const int CompressedObjectNumberPosition = 12;

	private const int CompressedObjectNumberBitSize = 15;

	private const int MethodNumberPosition = 1;

	private const int MethodNumberBitSize = 11;

	private const int TypeIdPosition = 27;

	private const int TypeIdBitSize = 5;

	private readonly ObjectTypeIdManager[] objectTypeIdManagers;

	private readonly LeastRecentlyUsedCache<AppQualifiedApplicationObjectId, ObjectSourceInfo> sourceInfoCache;

	private readonly Func<ApplicationObjectId, NCLMetaApplicationObject> metaApplicationObjectGetter;

	private readonly Func<int, NavAppGroup> navAppGroupGetter;

	private readonly LeastRecentlyUsedCache<AppQualifiedApplicationObjectId, ObjectAlSourceCodeInfo> sourceCodeInfoCache;

	private readonly LeastRecentlyUsedCache<AppQualifiedApplicationObjectId, SourceContextInfo> sourceContextInfoCache;

	public event EventHandler<EventArgs<ApplicationObjectId>> MetaObjectReplaced;

	public event EventHandler<EventArgs<ApplicationObjectId>> MetaObjectSourceChanged;

	public ALCodeEnvironment(Func<ApplicationObjectId, NCLMetaApplicationObject> metaApplicationObjectGetter, Func<int, NavAppGroup> navAppGroupGetter)
	{
		if (metaApplicationObjectGetter == null)
		{
			throw new ArgumentNullException("metaApplicationObjectGetter");
		}
		if (navAppGroupGetter == null)
		{
			throw new ArgumentNullException("navAppGroupGetter");
		}
		this.metaApplicationObjectGetter = metaApplicationObjectGetter;
		this.navAppGroupGetter = navAppGroupGetter;
		sourceInfoCache = new LeastRecentlyUsedCache<AppQualifiedApplicationObjectId, ObjectSourceInfo>(100, CreateObjectSourceInfo, (ObjectSourceInfo sourceInfo) => sourceInfo.AppQualifiedObjectId);
		sourceCodeInfoCache = new LeastRecentlyUsedCache<AppQualifiedApplicationObjectId, ObjectAlSourceCodeInfo>(100, CreateObjectAlSourceCodeInfo, (ObjectAlSourceCodeInfo sourceCodeInfo) => sourceCodeInfo.AppQualifiedObjectId);
		sourceContextInfoCache = new LeastRecentlyUsedCache<AppQualifiedApplicationObjectId, SourceContextInfo>(100, CreateSourceContextInfo, (SourceContextInfo sourceLineInfo) => sourceLineInfo.AppQualifiedObjectId);
		int count = Enum.GetValues(typeof(ObjectType)).Cast<int>().Max() + 1;
		objectTypeIdManagers = (from i in Enumerable.Range(0, count)
			select new ObjectTypeIdManager()).ToArray();
	}

	public static (string SourceCode, bool IsNavAppApplicationObject) GetSourceCodeContext(AppQualifiedApplicationObjectId appQualifiedObjectId, NavSession session, NavAppGroup appGroup)
	{
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000150 }))
		{
			SpecificNavAppObjectMetadataSummaryPair objectSummary = appGroup.GetObjectSummary(appQualifiedObjectId.UnqualifiedId);
			if (objectSummary != null)
			{
				if (!objectSummary.RuntimeMetadata.ShowMyCode && !objectSummary.RuntimeMetadata.IsDevExtension(session?.Tenant?.Id ?? string.Empty))
				{
					return (SourceCode: string.Empty, IsNavAppApplicationObject: true);
				}
				using NavRecord navRecord = new NavRecord(session, 2000000150);
				if (navRecord.ALGet(DataError.TrapError, NavGuid.CreateFromObject(((WrappedValue<Guid>)(object)objectSummary.Summary.AppPackageId).Value), NavInteger.Create((int)appQualifiedObjectId.ObjectType), NavInteger.Create(appQualifiedObjectId.ObjectNumber)))
				{
					int num = navRecord.GetFieldValue(8).ToInt32();
					if (num != 130000 && num != 70008)
					{
						throw new NavNCLMetadataVersionMismatchException(CultureInfo.CurrentCulture, appQualifiedObjectId.ObjectType, appQualifiedObjectId.ObjectNumber);
					}
					navRecord.CalcFields(DataError.ThrowError, new int[1] { 7 });
					NavBLOB navBLOB = (NavBLOB)navRecord.GetFieldValue(7);
					return (SourceCode: Encoding.UTF8.GetString(navBLOB.GetBytes()), IsNavAppApplicationObject: true);
				}
			}
			using NavRecord navRecord2 = new NavRecord(session, 2000000071);
			if (!navRecord2.ALGet(DataError.TrapError, NavInteger.Create((int)appQualifiedObjectId.ObjectType), NavInteger.Create(appQualifiedObjectId.ObjectNumber)))
			{
				throw new NavNCLMetadataObjectNotFoundException(CultureInfo.CurrentCulture, appQualifiedObjectId.ObjectType, appQualifiedObjectId.ObjectNumber, null);
			}
			int num2 = navRecord2.GetFieldValue(27).ToInt32();
			if (num2 != 130000 && num2 != 70008)
			{
				throw new NavNCLMetadataVersionMismatchException(CultureInfo.CurrentCulture, appQualifiedObjectId.ObjectType, appQualifiedObjectId.ObjectNumber);
			}
			navRecord2.CalcFields(DataError.ThrowError, new int[1] { 18 });
			NavBLOB navBLOB2 = (NavBLOB)navRecord2.GetFieldValue(18);
			return (SourceCode: Encoding.UTF8.GetString(navBLOB2.GetBytes()), IsNavAppApplicationObject: false);
		}
	}

	public int GetOrAssignScopeId(ApplicationObjectId objectId, Type navMethodScopeType, bool setBreakpointFlag)
	{
		return GetOrAssignScopeId(objectId, GetScopeIdProvider(navMethodScopeType), setBreakpointFlag);
	}

	public int GetOrAssignScopeId(NavMethodScope methodScope)
	{
		return GetOrAssignScopeId(methodScope, setBreakpointFlag: false);
	}

	public int GetOrAssignScopeId(NavMethodScope methodScope, bool setBreakpointFlag)
	{
		if (!setBreakpointFlag)
		{
			int scopeId = methodScope.ScopeId;
			if (scopeId != 0L)
			{
				return scopeId;
			}
		}
		return GetOrAssignScopeId(EffectiveObjectId(methodScope.ApplicationObject), methodScope, setBreakpointFlag);
	}

	public static void ClearBreakpointOnMethodScopeFlag(Type methodScopeType)
	{
		IRawScopeIdProvider scopeIdProvider = GetScopeIdProvider(methodScopeType);
		uint rawScopeId = scopeIdProvider.RawScopeId;
		if (rawScopeId != 0)
		{
			scopeIdProvider.RawScopeId = rawScopeId & 0xFFFFFFFEu;
		}
	}

	public static int CompressedObjectIdFromScopeId(int scopeId)
	{
		return Slice(scopeId, 12, 15);
	}

	public static ObjectType ObjectTypeFromScopeId(int scopeId)
	{
		return (ObjectType)Slice(scopeId, 27, 5);
	}

	public ApplicationObjectId ObjectIdFromScopeId(int scopeId)
	{
		ObjectType objectType = ObjectTypeFromScopeId(scopeId);
		int compressedObjectId = CompressedObjectIdFromScopeId(scopeId);
		int actualObjectId = objectTypeIdManagers[(int)objectType].GetActualObjectId(compressedObjectId);
		return new ApplicationObjectId(objectType, actualObjectId);
	}

	public ApplicationObjectId ObjectIdFromCompressedObjectIdToken(int token)
	{
		return ObjectIdFromScopeId(token);
	}

	public static int EncodedTypeAndObjectTokenFromScopeId(int scopeId)
	{
		return scopeId & -4096;
	}

	public static bool CheckHasBreakpointInScopeFlag(uint rawScopeId)
	{
		return (rawScopeId & 1) != 0;
	}

	public static bool CheckHasBreakpointInScopeFlag(Type navMethodScopeType)
	{
		return CheckHasBreakpointInScopeFlag(GetScopeIdProvider(navMethodScopeType).RawScopeId);
	}

	public static int GetScopeId(Type navMethodScopeType)
	{
		return RawScopeIdToScopeId(GetScopeIdProvider(navMethodScopeType).RawScopeId);
	}

	public static int RawScopeIdToScopeId(uint scopeId)
	{
		return (int)scopeId & -2;
	}

	public void OnObjectSourceUpdated(ApplicationObjectId objectId)
	{
		RemoveAllObjectsFromCachesWithMatchingUnqualifiedId(objectId);
		MetaObjectSourceChanged?.Invoke(this, new EventArgs<ApplicationObjectId>(objectId));
	}

	public void OnBaseObjectTypeReplaced(NCLMetaApplicationObject oldApplicationObjectType, NCLMetaApplicationObject newApplicationObjectType)
	{
		ApplicationObjectId applicationObjectId = new ApplicationObjectId(oldApplicationObjectType.ApplicationObjectType, oldApplicationObjectType.ObjectId);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000009M", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Replacing code environment base object -- Type:{0}; Id:{1}", applicationObjectId.ObjectType, applicationObjectId.ObjectNumber);
		RemoveAllObjectsFromCachesWithMatchingUnqualifiedId(applicationObjectId);
		TransferRawScopeIdsToNewType(oldApplicationObjectType, newApplicationObjectType);
		MetaObjectReplaced?.Invoke(this, new EventArgs<ApplicationObjectId>(applicationObjectId));
	}

	public void OnAppObjectTypeReplaced(NCLMetaApplicationObject oldMetaObject, NCLMetaApplicationObject newMetaObject, IEnumerable<int> affectedAppGroupIds)
	{
		ApplicationObjectId applicationObjectId = oldMetaObject.ApplicationObjectId;
		foreach (int affectedAppGroupId in affectedAppGroupIds)
		{
			AppQualifiedApplicationObjectId appQualifiedObjectId = new AppQualifiedApplicationObjectId(applicationObjectId.ObjectType, applicationObjectId.ObjectNumber, affectedAppGroupId);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000SV", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Clearing code environment cache for object -- Type:{0}; Id:{1}; Group:{2}", applicationObjectId.ObjectType, applicationObjectId.ObjectNumber, affectedAppGroupId);
			RemoveAllObjectsFromCachesWithMatchingAppQualifiedId(appQualifiedObjectId);
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000009N", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Replacing code environment extension object -- Type:{0}; Id:{1}; Metadata Group:{2}", applicationObjectId.ObjectType, applicationObjectId.ObjectNumber, oldMetaObject.MetadataAppGroup.GroupId);
		TransferRawScopeIdsToNewType(oldMetaObject, newMetaObject);
		MetaObjectReplaced?.Invoke(this, new EventArgs<ApplicationObjectId>(applicationObjectId));
	}

	public NCLMetaApplicationObject GetMetaApplicationObject(ApplicationObjectId applicationObjectId)
	{
		return metaApplicationObjectGetter(applicationObjectId);
	}

	public NavAppRuntimeMetadata GetExtensionMetadataForObject(ApplicationObjectId applicationObjectId)
	{
		AppQualifiedApplicationObjectId qualifiedObjectIdBasedOnCurrentTenantContext = GetQualifiedObjectIdBasedOnCurrentTenantContext(applicationObjectId);
		NavAppGroup navAppGroup = navAppGroupGetter(qualifiedObjectIdBasedOnCurrentTenantContext.AppGroupId) ?? NavAppGroup.BaseGroup;
		return navAppGroup.GetObjectSummary(applicationObjectId)?.RuntimeMetadata;
	}

	public static ApplicationObjectId EffectiveObjectId(NavApplicationObjectBase applicationObject)
	{
		return applicationObject.EffectiveObjectId;
	}

	public ObjectSourceInfo GetSourceInfo(ApplicationObjectId objectId)
	{
		AppQualifiedApplicationObjectId qualifiedObjectIdBasedOnCurrentTenantContext = GetQualifiedObjectIdBasedOnCurrentTenantContext(objectId);
		return sourceInfoCache.GetItem(qualifiedObjectIdBasedOnCurrentTenantContext);
	}

	public ObjectAlSourceCodeInfo GetSourceCodeInfo(ApplicationObjectId objectId)
	{
		AppQualifiedApplicationObjectId qualifiedObjectIdBasedOnCurrentTenantContext = GetQualifiedObjectIdBasedOnCurrentTenantContext(objectId);
		return sourceCodeInfoCache.GetItem(qualifiedObjectIdBasedOnCurrentTenantContext);
	}

	internal SourceContextInfo GetSourceContextInfo(NavMethodScope methodScope)
	{
		ApplicationObjectId applicationObjectId = GetApplicationObjectId(methodScope);
		return GetSourceContextInfo(applicationObjectId);
	}

	internal SourceContextInfo GetSourceContextInfo(ApplicationObjectId objectId)
	{
		AppQualifiedApplicationObjectId qualifiedObjectIdBasedOnCurrentTenantContext = GetQualifiedObjectIdBasedOnCurrentTenantContext(objectId);
		return sourceContextInfoCache.GetItem(qualifiedObjectIdBasedOnCurrentTenantContext);
	}

	internal string GetMethodName(NavMethodScope methodScope, SourceContextInfo infoParam = null)
	{
		SourceContextInfo orCreateSourceContextInfo = GetOrCreateSourceContextInfo(methodScope, infoParam);
		int orAssignScopeId = GetOrAssignScopeId(methodScope);
		if (orCreateSourceContextInfo.TryGetMethodName(orAssignScopeId, out var methodName))
		{
			return methodName;
		}
		return orCreateSourceContextInfo.AddOrUpdateMethodName(orAssignScopeId, methodScope.ScopeName);
	}

	internal Tuple<int, string> GetLineNumberAndStatement(NavMethodScope methodScope, SourceContextInfo infoParam = null)
	{
		SourceContextInfo orCreateSourceContextInfo = GetOrCreateSourceContextInfo(methodScope, infoParam);
		int orAssignScopeId = GetOrAssignScopeId(methodScope);
		if (orCreateSourceContextInfo.TryGetLineNumberAndStatement(orAssignScopeId, methodScope.StatementNumber, out var lineNumberAndStatement))
		{
			return lineNumberAndStatement;
		}
		SourceSpan currentStatementSourceSpan = MethodSourceInfo.GetCurrentStatementSourceSpan(methodScope);
		return orCreateSourceContextInfo.AddOrUpdateLineNumberAndStatement(orAssignScopeId, methodScope.StatementNumber, currentStatementSourceSpan);
	}

	internal ApplicationObjectId GetApplicationObjectId(NavMethodScope methodScope)
	{
		int orAssignScopeId = GetOrAssignScopeId(methodScope);
		return ObjectIdFromScopeId(orAssignScopeId);
	}

	private static AppQualifiedApplicationObjectId GetQualifiedObjectIdBasedOnCurrentTenantContext(ApplicationObjectId objectId)
	{
		int groupId = NavCurrentThread.ResolveAppGroup().GroupId;
		return new AppQualifiedApplicationObjectId(objectId.ObjectType, objectId.ObjectNumber, groupId);
	}

	private void RemoveAllObjectsFromCachesWithMatchingUnqualifiedId(ApplicationObjectId objectId)
	{
		RemoveAllObjectsFromCacheWithMatchingUnqualifiedId(sourceInfoCache, objectId);
		RemoveAllObjectsFromCacheWithMatchingUnqualifiedId(sourceCodeInfoCache, objectId);
		RemoveAllObjectsFromCacheWithMatchingUnqualifiedId(sourceContextInfoCache, objectId);
	}

	private void RemoveAllObjectsFromCachesWithMatchingAppQualifiedId(AppQualifiedApplicationObjectId appQualifiedObjectId)
	{
		sourceInfoCache.RemoveItem(appQualifiedObjectId);
		sourceCodeInfoCache.RemoveItem(appQualifiedObjectId);
		sourceContextInfoCache.RemoveItem(appQualifiedObjectId);
	}

	private static void RemoveAllObjectsFromCacheWithMatchingUnqualifiedId<TValue>(LeastRecentlyUsedCache<AppQualifiedApplicationObjectId, TValue> cache, ApplicationObjectId objectId)
	{
		List<AppQualifiedApplicationObjectId> list = cache.Keys.Where((AppQualifiedApplicationObjectId k) => k.UnqualifiedId == objectId).ToList();
		list.ForEach(cache.RemoveItem);
	}

	private static bool CheckIfScopeIdAlreadySet(IRawScopeIdProvider scopeIdProvider, bool setBreakpointFlag, out uint rawScopeId)
	{
		rawScopeId = scopeIdProvider.RawScopeId;
		if (rawScopeId == 0)
		{
			return false;
		}
		if (setBreakpointFlag && !CheckHasBreakpointInScopeFlag(rawScopeId))
		{
			rawScopeId |= 1u;
			scopeIdProvider.RawScopeId = rawScopeId;
		}
		return true;
	}

	private static IEnumerable<Type> GetAllNestedMethodScopeTypes(Type rootType)
	{
		Type[] nestedTypes = rootType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
		foreach (Type type in nestedTypes)
		{
			if (type.IsSubclassOf(typeof(NavApplicationObjectBase)))
			{
				foreach (Type allNestedMethodScopeType in GetAllNestedMethodScopeTypes(type))
				{
					yield return allNestedMethodScopeType;
				}
			}
			else if (type.IsSubclassOf(typeof(NavMethodScope)))
			{
				yield return type;
			}
		}
	}

	private static int Slice(int input, int fromIndex, int length)
	{
		input >>= fromIndex;
		int num = (1 << length) - 1;
		return input & num;
	}

	private static void TransferRawScopeIdsToNewType(NCLMetaApplicationObject oldApplicationObjectType, NCLMetaApplicationObject newApplicationObjectType)
	{
		if (!oldApplicationObjectType.ClrTypeLoaded || oldApplicationObjectType.ApplicationObjectClrType == null)
		{
			return;
		}
		newApplicationObjectType.MethodNumberCounter = oldApplicationObjectType.MethodNumberCounter;
		var list = (from methodScopeType in GetAllNestedMethodScopeTypes(oldApplicationObjectType.ApplicationObjectClrType)
			let scopeId = GetScopeIdProvider(methodScopeType).RawScopeId
			where scopeId != 0
			select new
			{
				methodScopeType = methodScopeType,
				rawScopeId = scopeId
			}).ToList();
		if (list.Count == 0)
		{
			return;
		}
		Dictionary<string, Type> dictionary = GetAllNestedMethodScopeTypes(newApplicationObjectType.ApplicationObjectClrType).ToDictionary((Type t) => t.Name);
		foreach (var item in list)
		{
			if (dictionary.TryGetValue(item.methodScopeType.Name, out var value))
			{
				GetScopeIdProvider(value).RawScopeId = item.rawScopeId;
			}
		}
	}

	private int GetOrAssignScopeId(ApplicationObjectId objectId, IRawScopeIdProvider scopeIdProvider, bool setBreakpointFlag)
	{
		if (CheckIfScopeIdAlreadySet(scopeIdProvider, setBreakpointFlag, out var rawScopeId))
		{
			return RawScopeIdToScopeId(rawScopeId);
		}
		ObjectTypeIdManager objectTypeIdManager = objectTypeIdManagers[(int)objectId.ObjectType];
		uint compressedObjectId = (uint)objectTypeIdManager.GetCompressedObjectId(objectId.ObjectNumber);
		NCLMetaApplicationObject nCLMetaApplicationObject = metaApplicationObjectGetter(objectId);
		lock (nCLMetaApplicationObject.MethodNumberCounter)
		{
			if (CheckIfScopeIdAlreadySet(scopeIdProvider, setBreakpointFlag, out rawScopeId))
			{
				return RawScopeIdToScopeId(rawScopeId);
			}
			uint objectType = (uint)objectId.ObjectType;
			uint num = (uint)nCLMetaApplicationObject.MethodNumberCounter.Next();
			if (num >= 2048)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "An internal error has occurred. Cannot assign a scope id. The maximum number of methods for {0} {1} has been exceeded.", objectId.ObjectType, objectId.ObjectNumber));
			}
			uint num2 = objectType << 27;
			uint num3 = compressedObjectId << 12;
			uint num4 = num << 1;
			uint num5 = (setBreakpointFlag ? 1u : 0u);
			rawScopeId = (scopeIdProvider.RawScopeId = num2 | num3 | num4 | num5);
			return RawScopeIdToScopeId(rawScopeId);
		}
	}

	private static IRawScopeIdProvider GetScopeIdProvider(Type methodScopeType)
	{
		return (IRawScopeIdProvider)FormatterServices.GetUninitializedObject(methodScopeType);
	}

	private ObjectSourceInfo CreateObjectSourceInfo(AppQualifiedApplicationObjectId appQualifiedObjectId)
	{
		NCLMetaApplicationObject metaApplicationObject = GetMetaApplicationObject(appQualifiedObjectId.UnqualifiedId);
		return new ObjectSourceInfo(appQualifiedObjectId, metaApplicationObject?.ApplicationObjectClrType);
	}

	private ObjectAlSourceCodeInfo CreateObjectAlSourceCodeInfo(AppQualifiedApplicationObjectId appQualifiedObjectId)
	{
		NavAppGroup appGroup = navAppGroupGetter(appQualifiedObjectId.AppGroupId) ?? NavAppGroup.BaseGroup;
		return new ObjectAlSourceCodeInfo(appQualifiedObjectId, appGroup);
	}

	private SourceContextInfo CreateSourceContextInfo(AppQualifiedApplicationObjectId appQualifiedObjectId)
	{
		ObjectAlSourceCodeInfo item = sourceCodeInfoCache.GetItem(appQualifiedObjectId);
		string[] sourceLines = item.SourceCode.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
		return new SourceContextInfo(appQualifiedObjectId, sourceLines, item.IsNavAppApplicationObjectSource);
	}

	private SourceContextInfo GetOrCreateSourceContextInfo(NavMethodScope methodScope, SourceContextInfo info)
	{
		if (info == null)
		{
			info = GetSourceContextInfo(methodScope);
		}
		return info;
	}
}
