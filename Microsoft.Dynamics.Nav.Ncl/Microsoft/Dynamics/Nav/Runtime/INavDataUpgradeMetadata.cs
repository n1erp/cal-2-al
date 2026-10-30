using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface INavDataUpgradeMetadata
{
	IList<NCLMetaCodeunit> UpgradeCodeunits { get; }

	IList<Tuple<int, string>> UpgradePerCompanyMethods { get; }

	IList<Tuple<int, string>> UpgradePerDatabaseMethods { get; }

	IList<Tuple<int, string>> CheckPreconditionMethods { get; }

	Tuple<int, string> CompanyInitializationMethod { get; }

	NCLMetaCodeunit GetUpgradeCodeunit(int id);

	NCLMetaCodeunit GetCompanyInitializationCodeunit(int id);
}
