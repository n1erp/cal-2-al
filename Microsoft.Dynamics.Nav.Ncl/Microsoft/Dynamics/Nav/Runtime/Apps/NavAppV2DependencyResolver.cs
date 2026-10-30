using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class NavAppV2DependencyResolver
{
	public class NavAppDependencyCheckResult
	{
		public IEnumerable<NavAppRuntimeMetadata> SatisfiedDependencies { get; }

		public IEnumerable<NavAppRuntimeDependencyDefinition> UnsatisfiedDependencies { get; }

		public string UnsatisfiedDependenciesString
		{
			get
			{
				List<string> list = new List<string>();
				foreach (NavAppRuntimeDependencyDefinition unsatisfiedDependency in UnsatisfiedDependencies)
				{
					list.Add(string.Format(CultureInfo.InvariantCulture, "{0} by {1} {2}", unsatisfiedDependency.Name, unsatisfiedDependency.Publisher, unsatisfiedDependency.Version));
				}
				return Environment.NewLine + string.Join(Environment.NewLine, list);
			}
		}

		internal NavAppDependencyCheckResult(IEnumerable<NavAppRuntimeMetadata> satisfiedDependencies, IEnumerable<NavAppRuntimeDependencyDefinition> unsatisfiedDependencies)
		{
			SatisfiedDependencies = satisfiedDependencies;
			UnsatisfiedDependencies = unsatisfiedDependencies;
		}
	}

	public static IEnumerable<NavAppRuntimeMetadata> AppsDependentOnPackage(NavAppRuntimeMetadata appMetadata, IEnumerable<NavAppRuntimeMetadata> installed, IEnumerable<NavAppRuntimeMetadata> published)
	{
		if (appMetadata == null)
		{
			throw new ArgumentNullException("appMetadata");
		}
		if (installed == null)
		{
			throw new ArgumentNullException("installed");
		}
		if (published == null)
		{
			throw new ArgumentNullException("published");
		}
		List<NavAppRuntimeMetadata> results = new List<NavAppRuntimeMetadata>();
		IEnumerable<NavAppRuntimeMetadata> source = published.Where((NavAppRuntimeMetadata app) => app.Dependencies.Any((NavAppRuntimeDependencyDefinition d) => (WrappedComparable<Guid>)(object)d.AppId == (WrappedComparable<Guid>)(object)appMetadata.AppId && d.Version <= appMetadata.Version));
		IEnumerable<NavAppRuntimeMetadata> collection = source.Where((NavAppRuntimeMetadata m) => installed.Any((NavAppRuntimeMetadata i) => (WrappedComparable<Guid>)(object)i.AppId == (WrappedComparable<Guid>)(object)m.AppId));
		Queue<NavAppRuntimeMetadata> processQueue = new Queue<NavAppRuntimeMetadata>(collection);
		while (processQueue.Count > 0)
		{
			int insertPos = results.Count;
			NavAppRuntimeMetadata current = processQueue.Dequeue();
			source = published.Where((NavAppRuntimeMetadata app) => app.Dependencies.Any((NavAppRuntimeDependencyDefinition d) => (WrappedComparable<Guid>)(object)d.AppId == (WrappedComparable<Guid>)(object)current.AppId && d.Version <= current.Version));
			source.Where((NavAppRuntimeMetadata m) => installed.Any((NavAppRuntimeMetadata i) => (WrappedComparable<Guid>)(object)i.AppId == (WrappedComparable<Guid>)(object)m.AppId)).ToList().ForEach((NavAppRuntimeMetadata dependent) =>
			{
				int num = results.IndexOf(dependent);
				if (num != -1 && num < insertPos)
				{
					insertPos = num;
				}
				else
				{
					processQueue.Enqueue(dependent);
				}
			});
			results.Insert(insertPos, current);
		}
		return results.AsEnumerable().Distinct().Reverse();
	}

	public static IEnumerable<NavAppRuntimeMetadata> AppsDependentOnPackages(IEnumerable<NavAppRuntimeMetadata> apps, IEnumerable<NavAppRuntimeMetadata> installed, IEnumerable<NavAppRuntimeMetadata> published)
	{
		if (apps == null)
		{
			throw new ArgumentNullException("apps");
		}
		if (installed == null)
		{
			throw new ArgumentNullException("installed");
		}
		if (published == null)
		{
			throw new ArgumentNullException("published");
		}
		List<NavAppRuntimeMetadata> results = new List<NavAppRuntimeMetadata>();
		Queue<NavAppRuntimeMetadata> processQueue = new Queue<NavAppRuntimeMetadata>(apps);
		while (processQueue.Count > 0)
		{
			int insertPos = results.Count;
			NavAppRuntimeMetadata current = processQueue.Dequeue();
			IEnumerable<NavAppRuntimeMetadata> source = published.Where((NavAppRuntimeMetadata app) => app.Dependencies.Any((NavAppRuntimeDependencyDefinition d) => (WrappedComparable<Guid>)(object)d.AppId == (WrappedComparable<Guid>)(object)current.AppId && d.Version <= current.Version));
			source.Where((NavAppRuntimeMetadata m) => installed.Any((NavAppRuntimeMetadata i) => (WrappedComparable<Guid>)(object)i.AppId == (WrappedComparable<Guid>)(object)m.AppId)).ToList().ForEach((NavAppRuntimeMetadata dependent) =>
			{
				int num = results.IndexOf(dependent);
				if (num != -1 && num < insertPos)
				{
					insertPos = num;
				}
				else
				{
					processQueue.Enqueue(dependent);
				}
			});
			results.Insert(insertPos, current);
		}
		return results;
	}

	public static NavAppDependencyCheckResult PackagesAppDependsOn(NavAppRuntimeMetadata runtimeMetadata, IEnumerable<INavAppReference> installed, IEnumerable<UninstalledAppRecord> uninstalled, IEnumerable<NavAppRuntimeMetadata> published, IEnumerable<AppSyncTrackingRecord> synced, bool checkSyncState = true)
	{
		if (runtimeMetadata == null)
		{
			throw new ArgumentNullException("runtimeMetadata");
		}
		if (installed == null)
		{
			throw new ArgumentNullException("installed");
		}
		if (published == null)
		{
			throw new ArgumentNullException("published");
		}
		if (synced == null)
		{
			throw new ArgumentNullException("synced");
		}
		List<NavAppRuntimeMetadata> source = published.Where((NavAppRuntimeMetadata p) => installed.Any((INavAppReference r) => (WrappedComparable<Guid>)(object)r.AppId == (WrappedComparable<Guid>)(object)p.AppId && r.Version == p.Version)).ToList();
		List<NavAppRuntimeMetadata> satisfiedDependencies = new List<NavAppRuntimeMetadata>();
		List<NavAppRuntimeDependencyDefinition> list = new List<NavAppRuntimeDependencyDefinition>();
		Queue<NavAppRuntimeDependencyDefinition> processQueue = new Queue<NavAppRuntimeDependencyDefinition>(runtimeMetadata.Dependencies);
		while (processQueue.Count > 0)
		{
			int insertPos = satisfiedDependencies.Count;
			NavAppRuntimeDependencyDefinition current = processQueue.Dequeue();
			Func<NavAppRuntimeMetadata, NavAppRuntimeDependencyDefinition, bool> isAcceptableMatch = (NavAppRuntimeMetadata meta, NavAppRuntimeDependencyDefinition dep) =>
			{
				bool flag = !checkSyncState || synced.Any((AppSyncTrackingRecord app) => (WrappedComparable<Guid>)(object)app.AppId == (WrappedComparable<Guid>)(object)meta.AppId && app.BaselineVersion <= meta.Version && app.Version >= meta.Version);
				bool flag2 = !uninstalled.Any((UninstalledAppRecord app) => (WrappedComparable<Guid>)(object)app.AppId == (WrappedComparable<Guid>)(object)meta.AppId && app.HasData && app.Version != meta.Version);
				return (WrappedComparable<Guid>)(object)meta.AppId == (WrappedComparable<Guid>)(object)dep.AppId && meta.Version >= dep.Version && (meta.ExtensionType != NavExtensionType.ModernDev || (flag & flag2)) && (meta.ExtensionType == NavExtensionType.ModernDev || meta.CompatibilityId == dep.CompatibilityId);
			};
			NavAppRuntimeMetadata val = source.FirstOrDefault((NavAppRuntimeMetadata i) => isAcceptableMatch(i, current)) ?? published.OrderByDescending((NavAppRuntimeMetadata p) => p.Version).FirstOrDefault((NavAppRuntimeMetadata p) => isAcceptableMatch(p, current));
			if (val == null)
			{
				list.Add(current);
				continue;
			}
			val.Dependencies.ToList().ForEach((NavAppRuntimeDependencyDefinition p) =>
			{
				NavAppRuntimeMetadata item = satisfiedDependencies.FirstOrDefault((NavAppRuntimeMetadata m) => (WrappedComparable<Guid>)(object)m.AppId == (WrappedComparable<Guid>)(object)p.AppId && m.Version >= p.Version) ?? (from m in published
					where (WrappedComparable<Guid>)(object)m.AppId == (WrappedComparable<Guid>)(object)p.AppId
					orderby m.Version descending
					select m).First((NavAppRuntimeMetadata m) => m.Version >= p.Version);
				int num = satisfiedDependencies.IndexOf(item);
				if (num != -1 && num < insertPos)
				{
					insertPos = num;
				}
				else
				{
					processQueue.Enqueue(p);
				}
			});
			satisfiedDependencies.Insert(insertPos, val);
		}
		return new NavAppDependencyCheckResult(satisfiedDependencies.AsEnumerable().Reverse(), list);
	}
}
