using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Apps;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.Apps.MetadataDeltas;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Apps;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class CachingDatabaseNavAppMetadataRetriever : INavAppMetadataRetriever
{
	private class ExtractedPackageInfo
	{
		public uint MetadataVersion { get; set; }

		public NavAppManifest Manifest { get; set; }
	}

	private class VariableAppData
	{
		public NavAppMetadataRecord RawMetadataRecord { get; set; }

		public NavAppPublishProperties PublishProperties { get; set; }
	}

	private class FixedAppData
	{
		public NavAppRuntimeMetadata RuntimeMetadata { get; set; }

		public NavAppManifest Manifest { get; set; }
	}

	private readonly Lazy<INavFilesystemContentCache> blobCache;

	private readonly object metadataLock = new object();

	private readonly int metadataLockTimeout = (int)ServerUserSettings.Instance.NavAppMetadataRetrieverTimeout.Value.TotalMilliseconds;

	private bool initialPopulationDone;

	private readonly Dictionary<PackageId, FixedAppData> allFixedEntries = new Dictionary<PackageId, FixedAppData>();

	private Dictionary<PackageId, VariableAppData> allVariableEntries = new Dictionary<PackageId, VariableAppData>();

	private readonly Dictionary<PackageId, List<NavAppApiMetadata>> allApiEntries = new Dictionary<PackageId, List<NavAppApiMetadata>>();

	private readonly Dictionary<string, HashSet<PackageId>> tenantPublishReferences = new Dictionary<string, HashSet<PackageId>>();

	private int nextRuntimeAppId = 1;

	private readonly ISet<PackageId> stalePackages = new HashSet<PackageId>();

	private int dbObjectBlobRequests;

	private TimeSpan dbObjectBlobElapsed = TimeSpan.Zero;

	public CachingDatabaseNavAppMetadataRetriever(Func<INavFilesystemContentCache> getBlobCache)
	{
		blobCache = new Lazy<INavFilesystemContentCache>(getBlobCache);
	}

	public IEnumerable<NavAppRuntimeMetadata> GetAllMetadata()
	{
		using (new MonitorLock(metadataLock, "metadataLock", metadataLockTimeout))
		{
			EnsureInitialCachePopulationHasRun();
			if (stalePackages.Count > 0)
			{
				RefreshCacheToReflectPackageModifications();
			}
			IEnumerable<NavAppRuntimeMetadata> source = allFixedEntries.Values.Select((FixedAppData fe) => fe.RuntimeMetadata);
			return source.Where((NavAppRuntimeMetadata rm) => allVariableEntries.ContainsKey(rm.PackageId)).ToList();
		}
	}

	private HashSet<PackageId> GetTenantPublishReferences(string tenantId)
	{
		tenantId = NavTenant.ConvertTenantIdToExpectedCase(tenantId);
		if (tenantId == null)
		{
			return new HashSet<PackageId>();
		}
		using (new MonitorLock(metadataLock, "metadataLock", metadataLockTimeout))
		{
			if (tenantPublishReferences.TryGetValue(tenantId, out var value))
			{
				return value;
			}
		}
		HashSet<PackageId> hashSet = new HashSet<PackageId>(from r in DatabaseNavAppMetadataBulkRetriever.GetPublishReferences(tenantId, null)
			select r.Item2);
		using (new MonitorLock(metadataLock, "metadataLock", metadataLockTimeout))
		{
			if (!tenantPublishReferences.TryGetValue(tenantId, out var value2))
			{
				value2 = hashSet;
				tenantPublishReferences.Add(tenantId, value2);
			}
			return value2;
		}
	}

	public IEnumerable<NavAppRuntimeMetadata> GetAllMetadataVisibleToTenant(string tenantId)
	{
		tenantId = NavTenant.ConvertTenantIdToExpectedCase(tenantId);
		HashSet<PackageId> packageIds = GetTenantPublishReferences(tenantId);
		using (new MonitorLock(metadataLock, "metadataLock", metadataLockTimeout))
		{
			IEnumerable<NavAppRuntimeMetadata> source = from m in GetAllMetadata()
				where m.Scope == NavAppScope.Global || (m.Scope == NavAppScope.Tenant && packageIds.Contains(m.PackageId))
				select m;
			return source.ToList();
		}
	}

	public IEnumerable<NavAppApiMetadata> GetAllApiMetadata()
	{
		using (new MonitorLock(metadataLock, "metadataLock", metadataLockTimeout))
		{
			EnsureInitialCachePopulationHasRun();
			if (stalePackages.Count > 0)
			{
				RefreshCacheToReflectPackageModifications();
			}
			IEnumerable<NavAppApiMetadata> source = allApiEntries.Where((KeyValuePair<PackageId, List<NavAppApiMetadata>> e) => allVariableEntries.ContainsKey(e.Key)).SelectMany((KeyValuePair<PackageId, List<NavAppApiMetadata>> e) => e.Value);
			return source.ToList();
		}
	}

	public IEnumerable<NavAppApiMetadata> GetAllApiMetadataVisibleToTenant(string tenantId)
	{
		tenantId = NavTenant.ConvertTenantIdToExpectedCase(tenantId);
		HashSet<PackageId> packageIds = GetTenantPublishReferences(tenantId);
		using (new MonitorLock(metadataLock, "metadataLock", metadataLockTimeout))
		{
			IEnumerable<NavAppApiMetadata> source = from m in GetAllApiMetadata()
				where m.Scope == NavAppScope.Global || (m.Scope == NavAppScope.Tenant && packageIds.Contains(m.PackageId))
				select m;
			return source.ToList();
		}
	}

	public IEnumerable<NavAppApiMetadata> GetApiMetadata(PackageId packageId)
	{
		using (new MonitorLock(metadataLock, "metadataLock", metadataLockTimeout))
		{
			EnsureInitialCachePopulationHasRun();
			EnsureNonBlobPopulationIsDoneForApp(packageId);
			if (allVariableEntries.TryGetValue(packageId, out var _) && allApiEntries.TryGetValue(packageId, out var value2))
			{
				return value2;
			}
		}
		return null;
	}

	public IEnumerable<string> GetTenantIdsWithAppPublished(PackageId packageId)
	{
		return (from r in DatabaseNavAppMetadataBulkRetriever.GetPublishReferences(null, ((WrappedValue<Guid>)(object)packageId).Value)
			select r.Item1).ToList();
	}

	public NavAppRuntimeMetadata GetMetadata(PackageId packageId)
	{
		using (new MonitorLock(metadataLock, "metadataLock", metadataLockTimeout))
		{
			EnsureInitialCachePopulationHasRun();
			EnsureNonBlobPopulationIsDoneForApp(packageId);
			if (allVariableEntries.TryGetValue(packageId, out var _) && allFixedEntries.TryGetValue(packageId, out var value2))
			{
				return value2.RuntimeMetadata;
			}
		}
		return null;
	}

	public NavAppObjectMetadataBlob GetObjectMetadataBlob(ApplicationObjectId objectId, PackageId packageId, NavAppObjectMetadataBlobType blobType)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		using (new MonitorLock(metadataLock, "metadataLock", metadataLockTimeout))
		{
			EnsureInitialCachePopulationHasRun();
			EnsureNonBlobPopulationIsDoneForApp(packageId);
			if (allVariableEntries.TryGetValue(PackageId.op_Implicit(((WrappedValue<Guid>)(object)packageId).Value), out var value))
			{
				NavAppMetadataRecord rawMetadataRecord = value.RawMetadataRecord;
				NavAppObjectMetadataRecord navAppObjectMetadataRecord = rawMetadataRecord.ObjectMetadata.SingleOrDefault((NavAppObjectMetadataRecord r) => r.ObjectType == objectId.ObjectType && r.ObjectId == objectId.ObjectNumber);
				if (navAppObjectMetadataRecord != null)
				{
					EnsureObjectMetadataBlobsAreCached(navAppObjectMetadataRecord);
					NCLMetaApplicationObject.VerifyMetadataVersion(navAppObjectMetadataRecord.ObjectId, navAppObjectMetadataRecord.ObjectType, navAppObjectMetadataRecord.MetadataVersion);
					NavFilesystemContentCacheKey cacheKey = (((int)blobType == 0) ? new NavFilesystemContentCacheKey(navAppObjectMetadataRecord.MetadataHash, "xml") : new NavFilesystemContentCacheKey(navAppObjectMetadataRecord.UserCodeHash, "cs"));
					byte[] content = blobCache.Value.GetContent(cacheKey);
					string content2 = ((content != null) ? Encoding.UTF8.GetString(content) : string.Empty);
					string path = blobCache.Value.GetPath(cacheKey);
					return new NavAppObjectMetadataBlob(content2, path, navAppObjectMetadataRecord.SqlTimestamp);
				}
			}
		}
		return null;
	}

	public NavAppPublishProperties GetPublishProperties(NavAppRuntimeMetadata metadata)
	{
		using (new MonitorLock(metadataLock, "metadataLock", metadataLockTimeout))
		{
			EnsureInitialCachePopulationHasRun();
			EnsureNonBlobPopulationIsDoneForApp(metadata.PackageId);
			if (allVariableEntries.TryGetValue(metadata.PackageId, out var value))
			{
				return value.PublishProperties;
			}
			return null;
		}
	}

	public NavAppResource GetResource(NavAppResourceId id)
	{
		using (new MonitorLock(metadataLock, "metadataLock", metadataLockTimeout))
		{
			EnsureInitialCachePopulationHasRun();
			EnsureNonBlobPopulationIsDoneForApp(id.PackageId);
			if (allVariableEntries.TryGetValue(id.PackageId, out var value))
			{
				NavAppResourceRecord navAppResourceRecord = value.RawMetadataRecord.Resources.FirstOrDefault((NavAppResourceRecord res) => res.Name == id.ResourceName.Value && res.Type == id.ResourceType);
				if (navAppResourceRecord != null)
				{
					EnsureResourceBlobIsCached(navAppResourceRecord);
					NavFilesystemContentCacheKey cacheKey = new NavFilesystemContentCacheKey(navAppResourceRecord.Hash, "res");
					byte[] content = blobCache.Value.GetContent(cacheKey);
					if (content != null)
					{
						return new NavAppResource(id, content, navAppResourceRecord.Hash);
					}
				}
			}
			return NavAppResource.None;
		}
	}

	public Stream GetPackageBlob(PackageId packageId)
	{
		using (new MonitorLock(metadataLock, "metadataLock", metadataLockTimeout))
		{
			EnsureInitialCachePopulationHasRun();
			EnsureNonBlobPopulationIsDoneForApp(packageId);
			if (allVariableEntries.TryGetValue(packageId, out var value))
			{
				NavFilesystemContentCacheKey cacheKey = new NavFilesystemContentCacheKey(value.RawMetadataRecord.PackageHash, "app");
				byte[] content = blobCache.Value.GetContent(cacheKey);
				return new MemoryStream(content);
			}
		}
		return null;
	}

	public NavAppManifest GetManifest(PackageId packageId)
	{
		using (new MonitorLock(metadataLock, "metadataLock", metadataLockTimeout))
		{
			EnsureInitialCachePopulationHasRun();
			EnsureNonBlobPopulationIsDoneForApp(packageId);
			if (allFixedEntries.TryGetValue(PackageId.op_Implicit(((WrappedValue<Guid>)(object)packageId).Value), out var value))
			{
				return value.Manifest;
			}
		}
		return null;
	}

	public NavAppScope? GetPackageScopeIfItHasBeenSeenBefore(PackageId packageId)
	{
		using (new MonitorLock(metadataLock, "metadataLock", metadataLockTimeout))
		{
			EnsureInitialCachePopulationHasRun();
			EnsureNonBlobPopulationIsDoneForApp(packageId);
			if (allFixedEntries.TryGetValue(PackageId.op_Implicit(((WrappedValue<Guid>)(object)packageId).Value), out var value))
			{
				return value.RuntimeMetadata.Scope;
			}
		}
		return null;
	}

	public void NotifyAboutMetadataChange(PackageId packageId)
	{
		using (new MonitorLock(metadataLock, "metadataLock", metadataLockTimeout))
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000097", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Extension metadata change detected (publish, repair, unpublish) -- PackageId:{0}", ((WrappedValue<Guid>)(object)packageId).Value);
			if (initialPopulationDone)
			{
				allVariableEntries.Remove(packageId);
				stalePackages.Add(packageId);
			}
		}
	}

	public void NotifyAboutPublishReferenceChange()
	{
		using (new MonitorLock(metadataLock, "metadataLock", metadataLockTimeout))
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001S1", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Extension publish reference change detected (publish/unpublish)");
			tenantPublishReferences.Clear();
		}
	}

	public void NotifyAboutPublishReferenceChange(string tenantId)
	{
		if (tenantId == null)
		{
			throw new ArgumentNullException("tenantId");
		}
		tenantId = NavTenant.ConvertTenantIdToExpectedCase(tenantId);
		using (new MonitorLock(metadataLock, "metadataLock", metadataLockTimeout))
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001S2", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Extension publish reference change detected (publish/unpublish) -- Tenant:{0}", tenantId);
			tenantPublishReferences.Remove(tenantId);
		}
	}

	public NavAppObjectMetadataTimestampRecord<NavAppObjectMetadataRuntimeDeltas> GetObjectExtensionDeltas(ApplicationObjectId objectId, NavAppRuntimeMetadata runtimeAppMetadata)
	{
		NavAppObjectMetadataBlob objectMetadataBlob = GetObjectMetadataBlob(objectId, runtimeAppMetadata.PackageId, (NavAppObjectMetadataBlobType)0);
		if (objectMetadataBlob != null)
		{
			try
			{
				NavAppObjectMetadataRuntimeDeltas val = NavAppObjectMetadataRuntimeDeltas.FromXml(XDocument.Parse(objectMetadataBlob.Content));
				((NavAppObjectMetadataDeltaCreator<Delta>)(object)val).AppMetadata = runtimeAppMetadata;
				return new NavAppObjectMetadataTimestampRecord<NavAppObjectMetadataRuntimeDeltas>(val, objectMetadataBlob.SqlTimestamp);
			}
			catch (XmlException innerException)
			{
				throw NavAppObjectMetadataException.Create(runtimeAppMetadata.TechnicalDisplayName, objectId, innerException);
			}
		}
		return null;
	}

	private void EnsureInitialCachePopulationHasRun()
	{
		if (initialPopulationDone)
		{
			return;
		}
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("0000098", DiagnosticParameter.SystemMetadata("Initial extension metadata cache population"), MonitorType.SqlReadCount, Category.Extensions, createNewActivity: false))
		{
			List<NavAppMetadataRecord> list = DatabaseNavAppMetadataBulkRetriever.GetMetadataForAllApps().ToList();
			HashSet<Sha256Hash> distinctHashesForApps = GetDistinctHashesForApps(list);
			blobCache.Value.PurgeUnreferencedContent(distinctHashesForApps);
			Dictionary<PackageId, VariableAppData> dictionary = new Dictionary<PackageId, VariableAppData>();
			Dictionary<PackageId, NavAppMetadataRecord> packageIdsToMetadataRecords = list.ToDictionary((NavAppMetadataRecord metadataRecord) => metadataRecord.PackageId, (NavAppMetadataRecord metadataRecord) => metadataRecord);
			EnsureNonBlobPopulationIsDoneForApps(packageIdsToMetadataRecords, dictionary);
			allVariableEntries = new Dictionary<PackageId, VariableAppData>(dictionary);
		}
		initialPopulationDone = true;
		stalePackages.Clear();
	}

	private static HashSet<Sha256Hash> GetDistinctHashesForApps(IEnumerable<NavAppMetadataRecord> allPublished)
	{
		HashSet<Sha256Hash> hashSet = new HashSet<Sha256Hash>();
		foreach (NavAppMetadataRecord item in allPublished)
		{
			hashSet.Add(item.PackageHash);
			foreach (Sha256Hash item2 in from om in item.ObjectMetadata
				select om.MetadataHash into h
				where h != null
				select h)
			{
				hashSet.Add(item2);
			}
			foreach (Sha256Hash item3 in from om in item.ObjectMetadata
				select om.UserCodeHash into h
				where h != null
				select h)
			{
				hashSet.Add(item3);
			}
			foreach (Sha256Hash item4 in from r in item.Resources
				select r.Hash into h
				where h != null
				select h)
			{
				hashSet.Add(item4);
			}
		}
		return hashSet;
	}

	private void EnsureNonBlobPopulationIsDoneForApp(PackageId packageId)
	{
		if (!allVariableEntries.ContainsKey(packageId))
		{
			NavAppMetadataRecord metadataForApp = DatabaseNavAppMetadataBulkRetriever.GetMetadataForApp(packageId);
			if (metadataForApp != null)
			{
				Dictionary<PackageId, NavAppMetadataRecord> packageIdsToMetadataRecords = new Dictionary<PackageId, NavAppMetadataRecord> { [packageId] = metadataForApp };
				EnsureNonBlobPopulationIsDoneForApps(packageIdsToMetadataRecords, allVariableEntries);
			}
		}
	}

	private void EnsureNonBlobPopulationIsDoneForApps(IDictionary<PackageId, NavAppMetadataRecord> packageIdsToMetadataRecords, IDictionary<PackageId, VariableAppData> variableEntries)
	{
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Expected Obj, but got Unknown
		packageIdsToMetadataRecords = packageIdsToMetadataRecords.Where((KeyValuePair<PackageId, NavAppMetadataRecord> kvp) => !variableEntries.ContainsKey(kvp.Key)).ToDictionary((KeyValuePair<PackageId, NavAppMetadataRecord> kvp) => kvp.Key, (KeyValuePair<PackageId, NavAppMetadataRecord> kvp) => kvp.Value);
		if (!packageIdsToMetadataRecords.Any())
		{
			return;
		}
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("00001P3", DiagnosticParameter.SystemMetadata("Non-blob metadata cache population -- PackageIds:" + string.Join(",", packageIdsToMetadataRecords.Keys)), MonitorType.SqlReadCount, Category.Extensions, createNewActivity: false))
		{
			Dictionary<PackageId, NavFilesystemContentCacheKey> dictionary = new Dictionary<PackageId, NavFilesystemContentCacheKey>(packageIdsToMetadataRecords.Count);
			foreach (KeyValuePair<PackageId, NavAppMetadataRecord> packageIdsToMetadataRecord in packageIdsToMetadataRecords)
			{
				PackageId key = packageIdsToMetadataRecord.Key;
				NavAppMetadataRecord value = packageIdsToMetadataRecord.Value;
				NavFilesystemContentCacheKey navFilesystemContentCacheKey = new NavFilesystemContentCacheKey(value.PackageHash, "app");
				if (!blobCache.Value.HasContent(navFilesystemContentCacheKey))
				{
					dictionary.Add(key, navFilesystemContentCacheKey);
				}
			}
			IDictionary<PackageId, byte[]> packageBlobs = DatabaseNavAppMetadataBulkRetriever.GetPackageBlobs(dictionary.Keys.ToList());
			foreach (KeyValuePair<PackageId, NavAppMetadataRecord> packageIdsToMetadataRecord2 in packageIdsToMetadataRecords)
			{
				PackageId key2 = packageIdsToMetadataRecord2.Key;
				NavAppMetadataRecord metadataRecord = packageIdsToMetadataRecord2.Value;
				VariableAppData value2 = new VariableAppData
				{
					RawMetadataRecord = metadataRecord,
					PublishProperties = new NavAppPublishProperties(metadataRecord.PackageType, metadataRecord.TenantId)
				};
				if (dictionary.TryGetValue(key2, out var value3))
				{
					byte[] content = packageBlobs[key2];
					blobCache.Value.PutContent(value3, content);
				}
				variableEntries.Add(metadataRecord.PackageId, value2);
				if (!allFixedEntries.ContainsKey(metadataRecord.PackageId))
				{
					ExtractedPackageInfo extractedPackageInfo = ExtractNecessaryInfoFromPackage(metadataRecord.PackageHash);
					FixedAppData fixedAppData = new FixedAppData
					{
						RuntimeMetadata = ToRuntimeMetadata(metadataRecord, extractedPackageInfo.MetadataVersion),
						Manifest = extractedPackageInfo.Manifest
					};
					allFixedEntries.Add(metadataRecord.PackageId, fixedAppData);
					NavAppRuntimeMetadata runtimeMetadata = fixedAppData.RuntimeMetadata;
					IEnumerable<NavAppObjectMetadataSummary> source = runtimeMetadata.ObjectMetadataSummaries.Where((NavAppObjectMetadataSummary ms) => (ms.ObjectType == ObjectType.Page || ms.ObjectType == ObjectType.Query) && ms.ObjectSubtype == "API");
					if (source.Any())
					{
						List<NavAppApiMetadata> value4 = source.Select((Func<NavAppObjectMetadataSummary, NavAppApiMetadata>)((NavAppObjectMetadataSummary ms) =>
						{
							//IL_0022: Unknown result type (might be due to invalid IL or missing references)
							//IL_0028: Expected Obj, but got Unknown
							return new NavAppApiMetadata(metadataRecord.PackageId, metadataRecord.Scope, ms.ObjectType, ms.ObjectId);
						})).ToList();
						allApiEntries.Add(metadataRecord.PackageId, value4);
					}
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000008Z", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Caching extension runtime metadata -- PackageId:{0}; AppId:{1}; Name:{2}; Publisher:{3}; Version:{4}; Scope:{5}", ((WrappedValue<Guid>)(object)runtimeMetadata.PackageId).Value, ((WrappedValue<Guid>)(object)runtimeMetadata.AppId).Value, runtimeMetadata.Name, runtimeMetadata.Publisher, runtimeMetadata.Version, runtimeMetadata.Scope);
				}
				SetRecordBuffersToNullForBlobsAlreadyInCache(metadataRecord);
			}
		}
	}

	private void SetRecordBuffersToNullForBlobsAlreadyInCache(NavAppMetadataRecord metadataRecord)
	{
		NavAppObjectMetadataRecord[] objectMetadata = metadataRecord.ObjectMetadata;
		foreach (NavAppObjectMetadataRecord navAppObjectMetadataRecord in objectMetadata)
		{
			NavFilesystemContentCacheKey navFilesystemContentCacheKey = ((navAppObjectMetadataRecord.MetadataHash != null) ? new NavFilesystemContentCacheKey(navAppObjectMetadataRecord.MetadataHash, "xml") : null);
			NavFilesystemContentCacheKey navFilesystemContentCacheKey2 = ((navAppObjectMetadataRecord.UserCodeHash != null) ? new NavFilesystemContentCacheKey(navAppObjectMetadataRecord.UserCodeHash, "cs") : null);
			bool flag = navFilesystemContentCacheKey == null || blobCache.Value.HasContent(navFilesystemContentCacheKey);
			bool flag2 = navFilesystemContentCacheKey2 == null || blobCache.Value.HasContent(navFilesystemContentCacheKey2);
			if (flag & flag2)
			{
				navAppObjectMetadataRecord.RecordBuffer = null;
			}
		}
		NavAppResourceRecord[] resources = metadataRecord.Resources;
		foreach (NavAppResourceRecord navAppResourceRecord in resources)
		{
			NavFilesystemContentCacheKey cacheKey = new NavFilesystemContentCacheKey(navAppResourceRecord.Hash, "res");
			if (navAppResourceRecord.Hash == null || blobCache.Value.HasContent(cacheKey))
			{
				navAppResourceRecord.RecordBuffer = null;
			}
		}
	}

	private void RefreshCacheToReflectPackageModifications()
	{
		if (!initialPopulationDone)
		{
			return;
		}
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("0000099", DiagnosticParameter.SystemMetadata("Extension metadata cache refresh"), MonitorType.SqlReadCount, Category.Extensions, createNewActivity: false))
		{
			List<NavAppMetadataRecord> list = DatabaseNavAppMetadataBulkRetriever.GetMetadataForApps(stalePackages).ToList();
			IEnumerable<PackageId> enumerable = stalePackages.Except(list.Select((NavAppMetadataRecord m) => m.PackageId));
			foreach (PackageId item in enumerable)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000009H", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Extension cache entry removal -- PackageId:{0}", item);
				allVariableEntries.Remove(item);
			}
			Dictionary<PackageId, NavAppMetadataRecord> dictionary = new Dictionary<PackageId, NavAppMetadataRecord>(list.Count);
			foreach (NavAppMetadataRecord item2 in list)
			{
				if (!allVariableEntries.ContainsKey(item2.PackageId))
				{
					dictionary.Add(item2.PackageId, item2);
				}
			}
			EnsureNonBlobPopulationIsDoneForApps(dictionary, allVariableEntries);
			stalePackages.Clear();
		}
	}

	private ExtractedPackageInfo ExtractNecessaryInfoFromPackage(Sha256Hash packageHash)
	{
		NavFilesystemContentCacheKey cacheKey = new NavFilesystemContentCacheKey(packageHash, "app");
		byte[] content = blobCache.Value.GetContent(cacheKey);
		using MemoryStream stream = new MemoryStream(content);
		using NavAppPackageReader navAppPackageReader = NavAppPackageReader.Create(stream);
		return new ExtractedPackageInfo
		{
			MetadataVersion = navAppPackageReader.MetadataVersion,
			Manifest = navAppPackageReader.ReadNavAppManifest()
		};
	}

	private void EnsureObjectMetadataBlobsAreCached(NavAppObjectMetadataRecord objectMetadataRecord)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		NavFilesystemContentCacheKey navFilesystemContentCacheKey = ((objectMetadataRecord.MetadataHash != null) ? new NavFilesystemContentCacheKey(objectMetadataRecord.MetadataHash, "xml") : null);
		NavFilesystemContentCacheKey navFilesystemContentCacheKey2 = ((objectMetadataRecord.UserCodeHash != null) ? new NavFilesystemContentCacheKey(objectMetadataRecord.UserCodeHash, "cs") : null);
		bool flag = navFilesystemContentCacheKey != null && !blobCache.Value.HasContent(navFilesystemContentCacheKey);
		bool flag2 = navFilesystemContentCacheKey2 != null && !blobCache.Value.HasContent(navFilesystemContentCacheKey2);
		NavAppObjectMetadataBlobFields val = (NavAppObjectMetadataBlobFields)0;
		val = (NavAppObjectMetadataBlobFields)(val | (flag ? 1 : 0));
		val = (NavAppObjectMetadataBlobFields)(val | (flag2 ? 2 : 0));
		if ((int)val != 0)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			Tuple<byte[], byte[]> objectMetadataBlobBytes = DatabaseNavAppMetadataBulkRetriever.GetObjectMetadataBlobBytes(objectMetadataRecord, val);
			stopwatch.Stop();
			dbObjectBlobRequests++;
			dbObjectBlobElapsed += stopwatch.Elapsed;
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000733", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "DB object metadata blob stats -- TotalRequests:{0}; Curr:{1}; Total:{2}; Avg:{3}", dbObjectBlobRequests, stopwatch.Elapsed.ToStringWithMilliseconds(), dbObjectBlobElapsed.ToStringWithMilliseconds(), TimeSpan.FromTicks(dbObjectBlobElapsed.Ticks / dbObjectBlobRequests).ToStringWithMilliseconds());
			if (flag)
			{
				blobCache.Value.PutContent(navFilesystemContentCacheKey, objectMetadataBlobBytes.Item1);
			}
			if (flag2)
			{
				blobCache.Value.PutContent(navFilesystemContentCacheKey2, objectMetadataBlobBytes.Item2);
			}
		}
		objectMetadataRecord.RecordBuffer = null;
	}

	private void EnsureResourceBlobIsCached(NavAppResourceRecord resourceRecord)
	{
		NavFilesystemContentCacheKey cacheKey = new NavFilesystemContentCacheKey(resourceRecord.Hash, "res");
		if (!blobCache.Value.HasContent(cacheKey))
		{
			byte[] resourceBlob = DatabaseNavAppMetadataBulkRetriever.GetResourceBlob(resourceRecord);
			blobCache.Value.PutContent(cacheKey, resourceBlob);
			resourceRecord.RecordBuffer = null;
		}
	}

	private NavAppRuntimeMetadata ToRuntimeMetadata(NavAppMetadataRecord rec, uint packageMetadataVersion)
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected Obj, but got Unknown
		IList<NavAppObjectMetadataSummary> list = rec.ObjectMetadata.Select(ToObjectMetadataSummary).ToList();
		NavExtensionType navExtensionType = ((packageMetadataVersion == 1) ? NavExtensionType.CSide : NavExtensionType.ModernDev);
		NavAppRuntimeMetadata result = new NavAppRuntimeMetadata(rec.PackageId, rec.AppId, rec.Name, rec.Publisher, VersionHelper.Create(rec.VersionMajor, rec.VersionMinor, rec.VersionBuild, rec.VersionRevision), VersionHelper.Create(rec.CompatibilityMajor, rec.CompatibilityMinor, rec.CompatibilityBuild, rec.CompatibilityRevision), nextRuntimeAppId, (IEnumerable<NavAppObjectMetadataSummary>)list, rec.Dependencies.Select(ToDependencyDefinition), rec.ContentHash, navExtensionType, rec.ShowMyCode, rec.Scope, rec.PackageHash, !string.IsNullOrEmpty(rec.TenantId));
		nextRuntimeAppId++;
		return result;
	}

	private static NavAppRuntimeDependencyDefinition ToDependencyDefinition(NavAppDependencyRecord rec)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected Obj, but got Unknown
		return new NavAppRuntimeDependencyDefinition(rec.Id, rec.Name, rec.Publisher, VersionHelper.Create(rec.VersionMajor, rec.VersionMinor, rec.VersionBuild, rec.VersionRevision), VersionHelper.Create(rec.CompatibilityMajor, rec.CompatibilityMinor, rec.CompatibilityBuild, rec.CompatibilityRevision));
	}

	private static NavAppObjectMetadataSummary ToObjectMetadataSummary(NavAppObjectMetadataRecord rec)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected Obj, but got Unknown
		ObjectType objectType = rec.ObjectType.ToBaseObjectType();
		int num = (rec.ObjectType.IsExtensionType() ? int.Parse(rec.ObjectSubtype, CultureInfo.InvariantCulture) : rec.ObjectId);
		return new NavAppObjectMetadataSummary(rec.AppPackageId, rec.ObjectType, rec.ObjectId, rec.Format, rec.ObjectSubtype, rec.ObjectName, rec.MetadataHash, rec.UserCodeHash, rec.ObjectFlags, objectType, num);
	}
}
