using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavDatabaseSharedSchemaExtension
{
	private ConcurrentDictionary<int, Lazy<CompanyId>> companyIds = new ConcurrentDictionary<int, Lazy<CompanyId>>();

	private NavDatabase database;

	public NavDatabaseSharedSchemaExtension(NavDatabase database)
	{
		this.database = database;
	}

	internal CompanyId? GetCompanyIdFromToken(int companyToken)
	{
		try
		{
			NavDatabase database = this.database;
			Lazy<CompanyId> orAdd = companyIds.GetOrAdd(companyToken, (int token) => new Lazy<CompanyId>(() => database.TenantCompanies.GetCompanyId(database.GetCompanyNameFromToken(token)), LazyThreadSafetyMode.ExecutionAndPublication));
			return orAdd.Value;
		}
		catch
		{
			companyIds.TryRemove(companyToken, out var _);
			throw;
		}
	}

	internal void ResetCompanyIdCache()
	{
		companyIds.Clear();
	}
}
