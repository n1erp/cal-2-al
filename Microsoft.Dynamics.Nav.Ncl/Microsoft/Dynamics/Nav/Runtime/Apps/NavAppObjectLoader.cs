using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppObjectLoader
{
	private readonly NavSqlConnectionScope connectionScope;

	public NavAppObjectLoader(NavSqlConnectionScope connectionScope)
	{
		this.connectionScope = connectionScope;
	}

	public Result LoadObjectsForApp(PackageId packageId, IEnumerable<NavAppObjectMetadata> metadataEntries)
	{
		try
		{
			metadataEntries.ForEach((NavAppObjectMetadata m) =>
			{
				m.AppPackageId = packageId;
			});
			if (metadataEntries.Any())
			{
				NavAppObjectStorage navAppObjectStorage = new NavAppObjectStorage(NavGlobal.AppDatabase, connectionScope);
				navAppObjectStorage.AddOrUpdateAppRecords(metadataEntries);
			}
			return Result.Ok();
		}
		catch (NavCSideException ex)
		{
			return Result.Fail(Error.FromException((Exception)ex));
		}
	}

	public IEnumerable<NavAppObjectMetadata> GetLoadedObjectsForApp(PackageId packageId)
	{
		NavAppObjectStorage navAppObjectStorage = new NavAppObjectStorage(NavGlobal.AppDatabase, connectionScope);
		return navAppObjectStorage.GetAppRecords(packageId);
	}

	public void DeleteLoadedObjectsForApp(PackageId packageId)
	{
		NavAppObjectStorage navAppObjectStorage = new NavAppObjectStorage(NavGlobal.AppDatabase, connectionScope);
		navAppObjectStorage.DeleteAppRecords(packageId);
	}
}
