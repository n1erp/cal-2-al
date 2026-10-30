using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Apps;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NavAppMountHelper
{
	private class MountMetadataEntry
	{
		public INavAppMetadataReference TenantTarget { get; set; }

		public NavAppRuntimeMetadata BestMatch { get; set; }

		public bool Valid { get; set; }
	}

	private class UpdatePackageIdEntry
	{
		public INavAppMetadataReference TenantTarget { get; set; }

		public NavAppRuntimeMetadata UpdateTarget { get; set; }
	}

	public static IEnumerable<NavAppRuntimeMetadata> GetAppsToMarkAsPublished(IList<NavAppRuntimeMetadata> allAppDBPublished, IList<INavAppMetadataReference> allTenantDBPublished, IList<NavAppRuntimeMetadata> allInstalled, string tenantId)
	{
		List<NavAppRuntimeMetadata> list = new List<NavAppRuntimeMetadata>();
		foreach (NavAppRuntimeMetadata installed in allInstalled)
		{
			NavAppRuntimeMetadata val = allAppDBPublished.FirstOrDefault((NavAppRuntimeMetadata p) => (WrappedComparable<Guid>)(object)p.AppId == (WrappedComparable<Guid>)(object)installed.AppId && p.Version == installed.Version);
			if (val != null && val.Scope == NavAppScope.Tenant)
			{
				list.Add(val);
			}
		}
		foreach (INavAppMetadataReference tenantDbPublished in allTenantDBPublished)
		{
			NavAppRuntimeMetadata val2 = allAppDBPublished.FirstOrDefault((NavAppRuntimeMetadata p) => (WrappedComparable<Guid>)(object)p.AppId == (WrappedComparable<Guid>)(object)((INavAppReference)tenantDbPublished).AppId && p.Version == ((INavAppReference)tenantDbPublished).Version);
			if (val2 != null && val2.Scope == NavAppScope.Tenant && !list.Contains(val2))
			{
				list.Add(val2);
			}
		}
		List<string> list2 = list.Select((NavAppRuntimeMetadata e) => string.Format(CultureInfo.InvariantCulture, "  AppId:{0}; PackageId:{1}; Name:{2}; Publisher:{3}; Version:{4}; Scope:{5}", e.AppId, e.PackageId, e.Name, e.Publisher, e.Version, e.Scope)).ToList();
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001S5", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Tenant scoped apps to mark published -- Tenant:" + tenantId + "; Count: " + list2.Count + Environment.NewLine + string.Join(Environment.NewLine, list2));
		return list;
	}

	public static IEnumerable<NavAppRuntimeMetadata> GetAppsToMarkAsInstalled(IEnumerable<NavAppRuntimeMetadata> appDbPublished, IEnumerable<INavAppMetadataReference> tenantDbPublished, IEnumerable<INavAppMetadataReference> tenantDbInstalled, IEnumerable<AppSyncTrackingRecord> tenantDbSynced, string tenantId)
	{
		List<MountMetadataEntry> list = tenantDbInstalled.Select((INavAppMetadataReference t) => new MountMetadataEntry
		{
			TenantTarget = t
		}).ToList();
		foreach (MountMetadataEntry entry in list)
		{
			List<NavAppRuntimeMetadata> source = (from p in appDbPublished
				where (WrappedComparable<Guid>)(object)p.AppId == (WrappedComparable<Guid>)(object)((INavAppReference)entry.TenantTarget).AppId && tenantDbSynced.Any((AppSyncTrackingRecord s) => (WrappedComparable<Guid>)(object)s.AppId == (WrappedComparable<Guid>)(object)p.AppId)
				orderby p.Version descending
				select p).ToList();
			List<NavAppRuntimeMetadata> source2 = source.Where((NavAppRuntimeMetadata c) => c.Scope == NavAppScope.Global || tenantDbPublished.Any((INavAppMetadataReference tp) => (WrappedComparable<Guid>)(object)((INavAppReference)tp).AppId == (WrappedComparable<Guid>)(object)c.AppId && ((INavAppReference)tp).Version == c.Version)).ToList();
			entry.BestMatch = source2.FirstOrDefault((NavAppRuntimeMetadata c) => c.Version == ((INavAppReference)entry.TenantTarget).Version) ?? source.FirstOrDefault((NavAppRuntimeMetadata c) => c.Version == ((INavAppReference)entry.TenantTarget).Version) ?? source2.FirstOrDefault((NavAppRuntimeMetadata m) => m.Version > ((INavAppReference)entry.TenantTarget).Version && entry.TenantTarget.ExtensionType == NavExtensionType.ModernDev) ?? source.FirstOrDefault((NavAppRuntimeMetadata m) => m.Version > ((INavAppReference)entry.TenantTarget).Version && entry.TenantTarget.ExtensionType == NavExtensionType.ModernDev);
			MountMetadataEntry mountMetadataEntry = entry;
			NavAppRuntimeMetadata bestMatch = entry.BestMatch;
			int valid;
			if (!(((bestMatch != null) ? bestMatch.Version : null) > ((INavAppReference)entry.TenantTarget).Version))
			{
				NavAppRuntimeMetadata bestMatch2 = entry.BestMatch;
				valid = ((((bestMatch2 != null) ? bestMatch2.Version : null) == ((INavAppReference)entry.TenantTarget).Version && CheckMatchVersionAndContentHash(entry.BestMatch, entry.TenantTarget)) ? 1 : 0);
			}
			else
			{
				valid = 1;
			}
			mountMetadataEntry.Valid = (byte)valid != 0;
		}
		List<string> list2 = list.Select((MountMetadataEntry e) =>
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] array = new object[4]
			{
				((object)e.TenantTarget.PackageId)?.ToString() + " - " + ((INavAppReference)e.TenantTarget).Name + " - " + ((INavAppReference)e.TenantTarget).Publisher + " - " + ((INavAppReference)e.TenantTarget).Version,
				(e.BestMatch == null) ? "NONE" : (((object)e.BestMatch.PackageId)?.ToString() + " - " + e.BestMatch.Name + " - " + e.BestMatch.Publisher + " - " + e.BestMatch.Version),
				null,
				null
			};
			NavAppRuntimeMetadata bestMatch3 = e.BestMatch;
			array[2] = ((bestMatch3 != null) ? new NavAppScope?(bestMatch3.Scope) : ((NavAppScope?)null));
			array[3] = e.Valid;
			return string.Format(invariantCulture, "  Old:[{0}]; New:[{1}]; Scope:{2}; Valid:{3}", array);
		}).ToList();
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001S6", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Tenant installed app mapping -- Tenant:" + tenantId + "; Count: " + list2.Count + Environment.NewLine + string.Join(Environment.NewLine, list2));
		IEnumerable<INavAppMetadataReference> source3 = from e in list
			where !e.Valid
			select e.TenantTarget;
		if (source3.Any())
		{
			string arg = string.Join(Environment.NewLine, source3.Select((INavAppMetadataReference app) => string.Format(CultureInfo.CurrentCulture, Lang.MissingApp, ((INavAppReference)app).Name, ((INavAppReference)app).Publisher, ((INavAppReference)app).Version)));
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000009E", Category.Extensions, new NavAppException(string.Format(CultureInfo.CurrentCulture, Lang.TenantMountFailedDueToMissingApp, tenantId, arg)));
		}
		return list.Select((MountMetadataEntry e) => e.BestMatch);
	}

	public static IEnumerable<NavAppRuntimeMetadata> GetAppsToUpdatePackageIdFor(IEnumerable<NavAppRuntimeMetadata> markingAsInstalled, IEnumerable<INavAppMetadataReference> tenantDbInstalled, string tenantId)
	{
		List<UpdatePackageIdEntry> list = new List<UpdatePackageIdEntry>();
		foreach (NavAppRuntimeMetadata marking in markingAsInstalled)
		{
			INavAppMetadataReference val = tenantDbInstalled.FirstOrDefault((INavAppMetadataReference t) => (WrappedComparable<Guid>)(object)((INavAppReference)t).AppId == (WrappedComparable<Guid>)(object)marking.AppId && ((INavAppReference)t).Version == marking.Version);
			if (val != null && CheckMatchVersionAndContentHash(marking, val) && ((WrappedComparable<Guid>)(object)val.PackageId != (WrappedComparable<Guid>)(object)marking.PackageId || val.ContentHash != marking.ContentHash))
			{
				list.Add(new UpdatePackageIdEntry
				{
					TenantTarget = val,
					UpdateTarget = marking
				});
			}
		}
		List<string> list2 = list.Select((UpdatePackageIdEntry e) => string.Format(CultureInfo.InvariantCulture, "  AppId:[{0}]; Name:[{1}]; Pulisher:[{2}]; Version:[{3}]; OldPackageId:[{4}]; OldHash:[{5}]; NewPackageId:[{6}]; NewHash:[{7}]", ((INavAppReference)e.TenantTarget).AppId, ((INavAppReference)e.TenantTarget).Name, ((INavAppReference)e.TenantTarget).Publisher, ((INavAppReference)e.TenantTarget).Version, e.TenantTarget.PackageId, string.IsNullOrEmpty(e.TenantTarget.ContentHash) ? string.Empty : Convert.ToBase64String(Encoding.UTF8.GetBytes(e.TenantTarget.ContentHash)), e.UpdateTarget.PackageId, string.IsNullOrEmpty(e.UpdateTarget.ContentHash) ? string.Empty : Convert.ToBase64String(Encoding.UTF8.GetBytes(e.UpdateTarget.ContentHash)))).ToList();
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000027N", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Tenant installed app package ID remapping -- Tenant:" + tenantId + "; Count: " + list2.Count + Environment.NewLine + string.Join(Environment.NewLine, list2));
		return list.Select((UpdatePackageIdEntry e) => e.UpdateTarget);
	}

	public static bool CheckMatchVersionAndContentHash(NavAppRuntimeMetadata publishedMetadata, INavAppMetadataReference tenantApp)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (publishedMetadata.Version == ((INavAppReference)tenantApp).Version)
		{
			if (!((WrappedComparable<Guid>)(object)publishedMetadata.PackageId == (WrappedComparable<Guid>)(object)tenantApp.PackageId))
			{
				return NavEnvironment.Instance.NavAppContentHashComparer.CompareContentHashesForAppFromTenant(publishedMetadata, publishedMetadata.ContentHash, tenantApp.ContentHash, tenantApp.HashAlgorithm);
			}
			return true;
		}
		return false;
	}
}
