using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Exceptions.Apps;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class ApiWebServiceEntryHelper
{
	internal static ApiWebServiceEntry GetLatestEntryVersion(ObjectType objectType, int objectId, IComparer<string> versionComparer)
	{
		ApiWebServiceEntry result = null;
		string text = null;
		IEnumerable<ApiWebServiceEntry> apiWebServicesInstalled = WebServiceHelper.GetApiWebServicesInstalled(objectType, objectId);
		foreach (ApiWebServiceEntry item in apiWebServicesInstalled)
		{
			if (text == null || versionComparer.Compare(item.Version, text) > 0)
			{
				text = item.Version;
				result = item;
			}
		}
		return result;
	}

	internal static IEnumerable<ApiWebServiceEntry> GetEntriesFromNavApp(NavAppGroup group, HashSet<string> paths)
	{
		List<ApiWebServiceEntry> list = new List<ApiWebServiceEntry>();
		IEnumerable<NavAppObjectMetadataSummary> enumerable = from m in @group.OrderedAppMetadata.Select((NavAppRuntimeMetadata m) => m.ObjectMetadataSummaries).SelectMany((IReadOnlyList<NavAppObjectMetadataSummary> m) => m)
			where (m.ObjectType == ObjectType.Page || m.ObjectType == ObjectType.Query) && m.ObjectSubtype == "API"
			select m;
		foreach (NavAppObjectMetadataSummary item in enumerable)
		{
			IEnumerable<ApiWebServiceEntry> allVersions = GetAllVersions(item.ObjectType, item.ObjectId, item.AppPackageId, group, paths, throwOnEndpointDuplicate: true);
			list.AddRange(allVersions);
		}
		return list;
	}

	internal static IEnumerable<ApiWebServiceEntry> GetEntriesFromBaseApp(HashSet<string> paths)
	{
		List<ApiWebServiceEntry> services = new List<ApiWebServiceEntry>();
		((ITenantSessionHandler)NavEnvironment.Instance.Tenants.SystemTenant).RunTenantActionInSystemSession((Action<NavTenant, NavSession>)((NavTenant t, NavSession systemSession) =>
		{
			using NavRecord navRecord = new NavRecord(systemSession, 2000000071, SecurityFiltering.Ignored);
			navRecord.ALSetFilter(3, "%1 | %2", NavInteger.Create(8), NavInteger.Create(9));
			navRecord.ALSetRange(33, NavText.Create("API"));
			if (navRecord.ALFindSet(DataError.TrapError))
			{
				do
				{
					ObjectType objectType = (ObjectType)navRecord.GetFieldValue(3).ToInt32();
					int objectId = navRecord.GetFieldValue(6).ToInt32();
					services.AddRange(GetAllVersions(objectType, objectId, PackageId.op_Implicit(Guid.Empty), t.NavAppGroup, paths));
				}
				while (navRecord.NextRecord());
			}
		}), false, false, false, (LanguageSetting)null);
		return services;
	}

	private static IEnumerable<ApiWebServiceEntry> GetAllVersions(ObjectType objectType, int objectId, PackageId packageId, NavAppGroup group, HashSet<string> paths, bool throwOnEndpointDuplicate = false)
	{
		List<ApiWebServiceEntry> list = new List<ApiWebServiceEntry>();
		ReadOnlyCollection<string> readOnlyCollection = null;
		string text = string.Empty;
		string text2 = string.Empty;
		string value = string.Empty;
		string text3 = string.Empty;
		int tableId = 0;
		bool tableTemporary = false;
		string tableFilter = string.Empty;
		string text4 = string.Empty;
		try
		{
			switch (objectType)
			{
			case ObjectType.Page:
			{
				NCLMetaForm metaFormById = NavGlobal.NCLMetadata.GetMetaFormById(objectId, group.GroupId, requireCompiled: false);
				text = metaFormById.ApiPublisher;
				text2 = metaFormById.ApiGroup;
				readOnlyCollection = metaFormById.ApiVersionList;
				value = metaFormById.EntityName;
				text3 = metaFormById.EntitySetName;
				tableId = metaFormById.SourceTable;
				tableTemporary = metaFormById.SourceTableTemporary;
				tableFilter = PageDataProvider.GenerateSourceTableViewString(metaFormById.SourceTableView);
				text4 = metaFormById.ODataKeyFields;
				break;
			}
			case ObjectType.Query:
			{
				NCLMetaQuery metaQueryById = NavGlobal.NCLMetadata.GetMetaQueryById(objectId, group.GroupId, requireCompiled: false);
				MetaQueryDataItem metaQueryDataItem = metaQueryById.MetaQuery.DataItems.FirstOrDefault();
				text = metaQueryById.ApiPublisher;
				text2 = metaQueryById.ApiGroup;
				readOnlyCollection = metaQueryById.ApiVersionList;
				value = metaQueryById.EntityName;
				text3 = metaQueryById.EntitySetName;
				tableId = metaQueryDataItem?.TableNo ?? 0;
				break;
			}
			}
		}
		catch (NavNCLMetadataObjectNotFoundException)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000021Z", Category.Api, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Metadata for {0} {1} is not found (package {2}, application group {3}).", objectType, objectId, packageId, group.GroupId);
		}
		if (readOnlyCollection != null && !string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(text3))
		{
			Dictionary<int, string> primaryKeyFieldsDict = new Dictionary<int, string>();
			foreach (string item2 in readOnlyCollection)
			{
				string text5 = ((paths != null) ? WebServiceHelper.GetApiPath(text, text2, item2, text3) : null);
				if (text5 == null || paths.Add(text5))
				{
					bool flag = !string.IsNullOrEmpty(text4);
					string keyFields = (flag ? text4 : GetPrimaryKeyFields(tableId, group, primaryKeyFieldsDict));
					ApiWebServiceEntry item = new ApiWebServiceEntry(text, text2, item2, text3, tableId, tableTemporary, tableFilter, keyFields, flag, objectType, objectId, packageId);
					list.Add(item);
					continue;
				}
				if (throwOnEndpointDuplicate)
				{
					NavAppDuplicateApiEndpointException ex2 = new NavAppDuplicateApiEndpointException(string.Format(CultureInfo.CurrentCulture, Lang.DuplicateApiEndpoint, text5, group.GroupId, objectType, objectId, packageId));
					DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000021E", Category.Api, ex2);
					throw ex2;
				}
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000021F", Category.Api, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Multiple API web services with the path '{0}' have been found for the application group {1} ({2} {3}, package {4}).", text5, group.GroupId, objectType, objectId, packageId);
			}
		}
		return list;
	}

	internal static IEnumerable<ApiWebServiceEntry> GetEntriesFromNavApp(NavTenant tenant, HashSet<string> paths)
	{
		List<ApiWebServiceEntry> list = new List<ApiWebServiceEntry>();
		IReadOnlyList<NavAppRuntimeMetadata> orderedAppMetadata = tenant.NavAppGroup.OrderedAppMetadata;
		ImmutableHashSet<PackageId> packagesInstalled = orderedAppMetadata.Select((NavAppRuntimeMetadata m) => m.PackageId).ToImmutableHashSet();
		List<NavAppApiMetadata> source = NavEnvironment.Instance.NavAppMetadataRetriever.GetAllApiMetadataVisibleToTenant(tenant.Id).ToList();
		IEnumerable<NavAppApiMetadata> enumerable = source.Where((NavAppApiMetadata m) => packagesInstalled.Contains(m.PackageId));
		foreach (NavAppApiMetadata item in enumerable)
		{
			IEnumerable<ApiWebServiceEntry> allVersions = GetAllVersions(item.ObjectType, item.ObjectId, item.PackageId, tenant.NavAppGroup, paths);
			list.AddRange(allVersions);
		}
		return list;
	}

	private static string GetPrimaryKeyFields(int tableId, NavAppGroup group, Dictionary<int, string> primaryKeyFieldsDict)
	{
		if (tableId > 0)
		{
			if (primaryKeyFieldsDict.ContainsKey(tableId))
			{
				return primaryKeyFieldsDict[tableId];
			}
			IEnumerable<int> values = NavGlobal.NCLMetadata.GetMetaTableById(tableId, group.GroupId, requireCompiled: false).PrimaryKey.KeyFieldsList.Select((NCLMetaField f) => f.FieldNo);
			string text = string.Join(",", values);
			primaryKeyFieldsDict.Add(tableId, text);
			return text;
		}
		return string.Empty;
	}
}
