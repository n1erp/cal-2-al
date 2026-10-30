using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Client;

public interface IWritableMetadataCache
{
	void CacheMetadataAndDependencies(PageDependencyResult dependencyResult, string personalizationId, DataSourceType dataSourceType);

	void CacheMasterNavigation(MasterNavigation masterNavigation);

	void CheckMetadataToken(long token);
}
