using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavDataUpgradeMetadata : INavDataUpgradeMetadata
{
	public const int CompanyInitializationMethodCodeunitId = 2;

	private readonly object syncObject = new object();

	private int codeunitMetadataChangeBumperToken;

	private ITenantSessionHandler sessionHandler;

	private INavUpgradeCodeunitObjectFactory navUpgradeCodeunitObjectFactory;

	private IList<NCLMetaCodeunit> upgradeCodeunits;

	public IList<NCLMetaCodeunit> UpgradeCodeunits
	{
		get
		{
			lock (syncObject)
			{
				if (upgradeCodeunits == null || NavGlobal.NCLMetadata.BaseCodeunitMetadataChangeBumperToken != codeunitMetadataChangeBumperToken)
				{
					sessionHandler.RunTenantActionInSystemSession((NavTenant tenant, NavSession session) =>
					{
						upgradeCodeunits = GetAllUpgradeCodeunits(session);
						codeunitMetadataChangeBumperToken = NavGlobal.NCLMetadata.BaseCodeunitMetadataChangeBumperToken;
					}, throwExceptions: true);
				}
			}
			return upgradeCodeunits;
		}
	}

	public IList<Tuple<int, string>> UpgradePerCompanyMethods => GetUpgradeCodeunitMethodsSubSet((NavUpgradeCodeunit codeunit) => codeunit.UpgradePerCompanyMethods);

	public IList<Tuple<int, string>> UpgradePerDatabaseMethods => GetUpgradeCodeunitMethodsSubSet((NavUpgradeCodeunit codeunit) => codeunit.UpgradePerDatabaseMethods);

	public IList<Tuple<int, string>> CheckPreconditionMethods => GetUpgradeCodeunitMethodsSubSet((NavUpgradeCodeunit codeunit) => codeunit.PreconditionCheckMethods);

	public Tuple<int, string> CompanyInitializationMethod => Tuple.Create(2, "OnRun");

	public NavDataUpgradeMetadata(NavDatabase ownerDatabase)
	{
		if (ownerDatabase == null)
		{
			throw new ArgumentNullException("ownerDatabase");
		}
		if (ownerDatabase.DatabaseType != NavDatabaseType.Application)
		{
			throw new ArgumentException("ownerDatabase");
		}
		if (ownerDatabase.Tenant == null)
		{
			throw new ArgumentNullException("ownerDatabase.Tenant");
		}
		sessionHandler = ownerDatabase.Tenant;
		navUpgradeCodeunitObjectFactory = new NavUpgradeCodeunitObjectFactory();
	}

	internal NavDataUpgradeMetadata(ITenantSessionHandler sessionHandler, INavUpgradeCodeunitObjectFactory upgradeCodeunitObjectFactory, int codeunitMetadataChangeBumperToken = 0)
	{
		this.sessionHandler = sessionHandler;
		navUpgradeCodeunitObjectFactory = upgradeCodeunitObjectFactory;
		this.codeunitMetadataChangeBumperToken = codeunitMetadataChangeBumperToken;
	}

	public NCLMetaCodeunit GetUpgradeCodeunit(int id)
	{
		return UpgradeCodeunits.FirstOrDefault((NCLMetaCodeunit next) => next.CodeunitId == id);
	}

	public NCLMetaCodeunit GetCompanyInitializationCodeunit(int id)
	{
		return NavGlobal.NCLMetadata.GetMetaCodeunitById(id);
	}

	internal IList<Tuple<int, string>> GetUpgradeCodeunitMethodsSubSet(Func<NavUpgradeCodeunit, IEnumerable<string>> methodFilterMethod)
	{
		IList<Tuple<int, string>> foundMethods = new List<Tuple<int, string>>();
		sessionHandler.RunTenantActionInSystemSession((NavTenant tenant, NavSession session) =>
		{
			foreach (NCLMetaCodeunit upgradeCodeunit in UpgradeCodeunits)
			{
				using NavUpgradeCodeunit navUpgradeCodeunit = navUpgradeCodeunitObjectFactory.CreateCodeunitObject(session, upgradeCodeunit);
				foreach (string item in methodFilterMethod(navUpgradeCodeunit))
				{
					foundMethods.Add(new Tuple<int, string>(navUpgradeCodeunit.ObjectId.ObjectNumber, item));
				}
			}
		});
		return foundMethods;
	}

	internal virtual IList<NCLMetaCodeunit> GetAllUpgradeCodeunits(NavSession navSession)
	{
		List<NCLMetaCodeunit> list = new List<NCLMetaCodeunit>();
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000000058);
		using (NavRecord navRecord = new NavRecord(navSession, 2000000058))
		{
			navRecord.ALSetFilter(1, "%1", NavInteger.Create(5));
			navRecord.ALSetFilter(30, "%1", NavText.CreateTruncated(metaTableById.GetFieldByNo(30).FieldDefinedLength, CodeunitSubType.Upgrade.ToString()));
			if (navRecord.ALFindSet(DataError.TrapError))
			{
				do
				{
					list.Add(NavGlobal.NCLMetadata.GetMetaCodeunitById(navRecord.GetFieldValue(3).ToInt32()));
				}
				while (navRecord.ALNext() > 0);
			}
		}
		return list;
	}
}
