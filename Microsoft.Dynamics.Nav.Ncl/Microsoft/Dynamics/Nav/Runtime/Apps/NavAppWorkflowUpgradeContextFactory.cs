using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NavAppWorkflowUpgradeContextFactory
{
	private class ContextAppMetadata
	{
		public NavAppRuntimeMetadata Target { get; private set; }

		public INavAppMetadataReference Existing { get; private set; }

		public bool IsInstall { get; private set; }

		public bool HasInstallData { get; private set; }

		public static ContextAppMetadata CreateForUpgrade(NavAppRuntimeMetadata target, INavAppMetadataReference existing)
		{
			return new ContextAppMetadata
			{
				Target = target,
				Existing = existing,
				IsInstall = false,
				HasInstallData = false
			};
		}

		public static ContextAppMetadata CreateForInstall(NavAppRuntimeMetadata target, bool hasInstallData)
		{
			return new ContextAppMetadata
			{
				Target = target,
				Existing = null,
				IsInstall = true,
				HasInstallData = hasInstallData
			};
		}
	}

	private readonly NavAppPackagePoolSelector poolSelector;

	private readonly Func<NavSqlConnectionScope, IEnumerable<INavAppMetadataReference>> tenantInstalledNavAppRetriever;

	private readonly Func<NavSqlConnectionScope, string, IEnumerable<UninstalledAppRecord>> tenantUninstalledAppRetriever;

	private readonly INavAppGroupResolver groupResolver;

	public NavAppWorkflowUpgradeContextFactory(NavAppPackagePoolSelector poolSelector, Func<NavSqlConnectionScope, IEnumerable<INavAppMetadataReference>> tenantInstalledAppRetriever, Func<NavSqlConnectionScope, string, IEnumerable<UninstalledAppRecord>> tenantUninstalledAppRetriever, INavAppGroupResolver groupResolver)
	{
		this.poolSelector = poolSelector;
		tenantInstalledNavAppRetriever = tenantInstalledAppRetriever;
		this.tenantUninstalledAppRetriever = tenantUninstalledAppRetriever;
		this.groupResolver = groupResolver;
	}

	public bool IsAppsUpgradeNeeded(NavTenant tenant, bool skipUninstalledV2Extensions)
	{
		using NavSqlConnectionScope tenantDatabaseScope = NavSqlConnectionScope.Create(tenant.Database, TransactionType.Browse);
		return GetOrderedAppsToUpgradeOrInstallForTenant(tenant.Id, tenantDatabaseScope, skipUninstalledV2Extensions).Count > 0;
	}

	public List<NavAppJointUpgradeContext> GetOrderedAppContextsForDataUpgrade(NavTenant tenant, Guid activityId, bool skipUninstalledExtensions)
	{
		IReadOnlyCollection<ContextAppMetadata> orderedAppsToUpgradeOrInstallForTenant;
		using (NavSqlConnectionScope tenantDatabaseScope = NavSqlConnectionScope.Create(tenant.Database, TransactionType.Browse))
		{
			orderedAppsToUpgradeOrInstallForTenant = GetOrderedAppsToUpgradeOrInstallForTenant(tenant.Id, tenantDatabaseScope, skipUninstalledExtensions);
		}
		List<PackageId> appPackageIds = tenant.NavAppGroup.OrderedAppMetadata.Select((NavAppRuntimeMetadata m) => m.PackageId).Concat(orderedAppsToUpgradeOrInstallForTenant.Select((ContextAppMetadata m) => m.Target.PackageId)).ToList();
		NavAppGroup upgradeAppGroup = groupResolver.GetOrCreateGroupForAppCombination(appPackageIds);
		string text = string.Join(" | ", upgradeAppGroup.OrderedAppMetadata.Select((NavAppRuntimeMetadata m) => m.TechnicalDisplayName));
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000094A", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "App group for data upgrade -- Group:{0}; Extensions:{1}", upgradeAppGroup.GroupId, text);
		return orderedAppsToUpgradeOrInstallForTenant.Select((ContextAppMetadata m) => (!m.IsInstall) ? NavAppJointUpgradeContext.CreateForUpgrade(NavAppUpgradeContext.Create(tenant, upgradeAppGroup, m.Target, (INavAppReference)(object)m.Existing, activityId.ToString())) : NavAppJointUpgradeContext.CreateForInstall(NavAppInstallationContext.CreateForInstall(tenant, upgradeAppGroup, m.Target, NavAppSyncOptions.Default, activityId.ToString(), m.HasInstallData))).ToList();
	}

	private IReadOnlyCollection<ContextAppMetadata> GetOrderedAppsToUpgradeOrInstallForTenant(string tenantId, NavSqlConnectionScope tenantDatabaseScope, bool skipUninstalledV2Extensions)
	{
		List<ContextAppMetadata> list = new List<ContextAppMetadata>();
		HashSet<AppId> hashSet = new HashSet<AppId>();
		List<INavAppMetadataReference> list2 = tenantInstalledNavAppRetriever(tenantDatabaseScope).ToList();
		List<UninstalledAppRecord> list3 = tenantUninstalledAppRetriever(tenantDatabaseScope, tenantId).ToList();
		List<NavAppRuntimeMetadata> list4 = poolSelector.GetLatestVersionOfPackages(tenantDatabaseScope, requireSynced: true, (IEnumerable<INavAppReference>)list2).Values.Where((NavAppRuntimeMetadata a) => a.ExtensionType == NavExtensionType.ModernDev).ToList();
		NavAppRuntimeMetadata[] array = NavAppOrderResolver.DetermineProperOrderForApps(list4).ToArray();
		NavAppRuntimeMetadata[] array2 = array;
		foreach (NavAppRuntimeMetadata target in array2)
		{
			INavAppMetadataReference val = list2.FirstOrDefault((INavAppMetadataReference app) => (WrappedComparable<Guid>)(object)((INavAppReference)app).AppId == (WrappedComparable<Guid>)(object)target.AppId);
			UninstalledAppRecord uninstalledAppRecord = list3.FirstOrDefault((UninstalledAppRecord app) => (WrappedComparable<Guid>)(object)app.AppId == (WrappedComparable<Guid>)(object)target.AppId);
			if (val != null && target.Version > ((INavAppReference)val).Version)
			{
				list.Add(ContextAppMetadata.CreateForUpgrade(target, val));
				hashSet.Add(target.AppId);
			}
			else if (!skipUninstalledV2Extensions && uninstalledAppRecord != null && target.Version > uninstalledAppRecord.Version)
			{
				list.Add(ContextAppMetadata.CreateForUpgrade(target, null));
				hashSet.Add(target.AppId);
			}
		}
		AddDependencies(list, hashSet, list4, list2, list3);
		return list.AsReadOnly();
	}

	private static void AddDependencies(List<ContextAppMetadata> orderedUpgrade, HashSet<AppId> upgradeAppIds, IEnumerable<NavAppRuntimeMetadata> appPool, List<INavAppMetadataReference> installedApps, List<UninstalledAppRecord> uninstalledApps)
	{
		Queue<ContextAppMetadata> queue = new Queue<ContextAppMetadata>(orderedUpgrade.ToList());
		List<Tuple<ContextAppMetadata, NavAppRuntimeDependencyDefinition>> list = new List<Tuple<ContextAppMetadata, NavAppRuntimeDependencyDefinition>>();
		while (queue.Any())
		{
			ContextAppMetadata current = queue.Dequeue();
			int num = orderedUpgrade.IndexOf(current);
			List<Tuple<NavAppRuntimeDependencyDefinition, NavAppRuntimeMetadata>> source = current.Target.Dependencies?.Select((NavAppRuntimeDependencyDefinition d) => Tuple.Create<NavAppRuntimeDependencyDefinition, NavAppRuntimeMetadata>(d, appPool.FirstOrDefault((NavAppRuntimeMetadata t) => (WrappedComparable<Guid>)(object)t.AppId == (WrappedComparable<Guid>)(object)d.AppId))).ToList() ?? new List<Tuple<NavAppRuntimeDependencyDefinition, NavAppRuntimeMetadata>>();
			List<Tuple<NavAppRuntimeDependencyDefinition, NavAppRuntimeMetadata>> source2 = source.Where((Tuple<NavAppRuntimeDependencyDefinition, NavAppRuntimeMetadata> d) => d.Item2 == null).ToList();
			if (source2.Any())
			{
				list.AddRange(source2.Select((Tuple<NavAppRuntimeDependencyDefinition, NavAppRuntimeMetadata> u) => Tuple.Create<ContextAppMetadata, NavAppRuntimeDependencyDefinition>(current, u.Item1)));
				continue;
			}
			IEnumerable<NavAppRuntimeMetadata> enumerable = NavAppOrderResolver.DetermineProperOrderForApps(source.Select((Tuple<NavAppRuntimeDependencyDefinition, NavAppRuntimeMetadata> d) => d.Item2));
			foreach (NavAppRuntimeMetadata dependency in enumerable)
			{
				INavAppMetadataReference val = installedApps.FirstOrDefault((INavAppMetadataReference app) => (WrappedComparable<Guid>)(object)((INavAppReference)app).AppId == (WrappedComparable<Guid>)(object)dependency.AppId);
				if (val == null && upgradeAppIds.Add(dependency.AppId))
				{
					UninstalledAppRecord uninstalledAppRecord = uninstalledApps.FirstOrDefault((UninstalledAppRecord app) => (WrappedComparable<Guid>)(object)app.AppId == (WrappedComparable<Guid>)(object)dependency.AppId);
					ContextAppMetadata item;
					if (uninstalledAppRecord == null)
					{
						item = ContextAppMetadata.CreateForInstall(dependency, hasInstallData: false);
					}
					else
					{
						item = ((uninstalledAppRecord.Version < dependency.Version) ? ContextAppMetadata.CreateForUpgrade(dependency, null) : ContextAppMetadata.CreateForInstall(dependency, uninstalledAppRecord.HasData));
					}
					orderedUpgrade.Insert(num++, item);
					queue.Enqueue(item);
				}
			}
		}
		if (list.Any())
		{
			throw new NavUpgradeProcessExecutionException(string.Join(Environment.NewLine, list.Select((Tuple<ContextAppMetadata, NavAppRuntimeDependencyDefinition> d) => Lang.NavAppJointUpgrade_DependencyNotResolved.FormatWithResolvedCulture(d.Item1.Target.TechnicalDisplayName, NavAppReferenceExtensions.TechnicalDisplayName((INavAppReference)(object)d.Item2)))));
		}
	}
}
