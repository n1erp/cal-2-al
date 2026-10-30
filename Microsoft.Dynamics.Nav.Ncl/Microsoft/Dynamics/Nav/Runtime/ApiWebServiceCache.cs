using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class ApiWebServiceCache
{
	private class ModelKey
	{
		public string TenantId { get; private set; }

		public int GroupId { get; private set; }

		public int SetupVersion { get; private set; }

		public static ModelKey Create(NavTenant tenant)
		{
			return new ModelKey(tenant.Id, tenant.NavAppGroup.GroupId, tenant.Database.WebServiceSetupMonitor.SetupVersion);
		}

		internal ModelKey(string tenantId, int groupId, int setupVersion)
		{
			TenantId = tenantId;
			GroupId = groupId;
			SetupVersion = setupVersion;
		}

		public static bool operator ==(ModelKey key1, ModelKey key2)
		{
			if ((object)key1 == null || (object)key2 == null)
			{
				if ((object)key1 == null)
				{
					return (object)key2 == null;
				}
				return false;
			}
			if (key1.GroupId == key2.GroupId)
			{
				return key1.TenantId == key2.TenantId;
			}
			return false;
		}

		public static bool operator !=(ModelKey key1, ModelKey key2)
		{
			return !(key1 == key2);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ModelKey modelKey))
			{
				return false;
			}
			return this == modelKey;
		}

		public override int GetHashCode()
		{
			return (GroupId.GetHashCode() * 397) ^ ((TenantId != null) ? TenantId.GetHashCode() : 0);
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture, "{{ TenantId={0}, GroupId={1} }}", TenantId, GroupId);
		}
	}

	private static readonly object modelLock = new object();

	private static readonly ConcurrentDictionary<ModelKey, List<ApiWebServiceEntry>> models = new ConcurrentDictionary<ModelKey, List<ApiWebServiceEntry>>();

	internal static bool IsInitialized(NavTenant tenant)
	{
		ModelKey key = ModelKey.Create(tenant);
		return models.ContainsKey(key);
	}

	internal static List<ApiWebServiceEntry> GetInstalledServices(NavTenant tenant)
	{
		ModelKey modelKey = ModelKey.Create(tenant);
		List<ApiWebServiceEntry> value = null;
		lock (modelLock)
		{
			if (models.ContainsKey(modelKey) && IsCurrentSetupVersion(modelKey) && !models.TryGetValue(modelKey, out value))
			{
				value = null;
			}
		}
		return value;
	}

	internal static bool SetInstalledServices(NavTenant tenant, List<ApiWebServiceEntry> installedServices)
	{
		ModelKey key = ModelKey.Create(tenant);
		lock (modelLock)
		{
			if (models.ContainsKey(key) && !models.TryRemove(key, out var _))
			{
				return false;
			}
			return models.TryAdd(key, installedServices);
		}
	}

	private static bool IsCurrentSetupVersion(ModelKey modelKey)
	{
		ModelKey modelKey2 = models.Keys.FirstOrDefault((ModelKey p) => p == modelKey);
		if (modelKey2 != null && modelKey2.SetupVersion != modelKey.SetupVersion)
		{
			using (DiagnosticsResolver.GetMostSpecificInstance()?.CreateTraceScope("000024H", DiagnosticParameter.SystemMetadata("API - ApiWebServiceCache.Get"), MonitorType.None, Category.Api))
			{
				models.TryRemove(modelKey, out var _);
			}
			return false;
		}
		return true;
	}

	public static void Invalidate()
	{
		lock (modelLock)
		{
			using (DiagnosticsResolver.GetMostSpecificInstance()?.CreateTraceScope("000024I", DiagnosticParameter.SystemMetadata("API - ApiWebServiceCache.Invalidate"), MonitorType.None, Category.Api))
			{
				models.Clear();
			}
		}
	}

	public static void Invalidate(NavAppGroup group)
	{
		lock (modelLock)
		{
			IEnumerable<ModelKey> modelKeys = from k in models.Keys
				where k.GroupId == @group.GroupId
				select new ModelKey(k.TenantId, k.GroupId, k.SetupVersion);
			Invalidate(modelKeys, group);
		}
	}

	private static void Invalidate(IEnumerable<ModelKey> modelKeys, object context)
	{
		using (DiagnosticsResolver.GetMostSpecificInstance()?.CreateTraceScope("000024J", DiagnosticParameter.SystemMetadata("API - ApiWebServiceCache.Invalidate"), MonitorType.None, Category.Api))
		{
			try
			{
				foreach (ModelKey modelKey in modelKeys)
				{
					models.TryRemove(modelKey, out var _);
				}
			}
			catch (Exception exception)
			{
				DiagnosticsResolver.GetMostSpecificInstance()?.SendExceptionTag("000024G", Category.Api, exception, Verbosity.Error, DiagnosticParameter.Oii(context));
				models.Clear();
				throw;
			}
		}
	}
}
