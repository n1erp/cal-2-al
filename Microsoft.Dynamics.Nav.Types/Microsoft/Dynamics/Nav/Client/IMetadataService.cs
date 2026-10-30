using System;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Client;

public interface IMetadataService
{
	long MetadataToken { get; }

	MetaTable GetTableMetadata(int tableId);

	MetaQuery GetQueryMetadata(int queryId);

	MetaTable GetQueryTableMetadata(int queryId);

	MasterPage GetMasterPage(int pageId, DataSourceType dataSourceType, string personalizationId, bool applyPersonalization);

	MasterPage GetMasterPageWithoutConfiguration(int pageId, DataSourceType dataSourceType);

	MasterPage GetMasterPage(int pageId, DataSourceType dataSourceType, string personalizationId, bool applyPersonalization, bool doGetDependencies, string viewName);

	MasterPage GetMasterPage(int pageId, DataSourceType dataSourceType);

	MasterPage GetMasterPageTemplate(PageType pageType);

	MasterPage GetTableMasterPage(int tableId, string personalizationId);

	MasterPage GetSystemPart(SystemPartTypes partType);

	ILocalReportHandle GetReportHandle(int reportId, Guid reportHandle, bool useCache);

	MasterPage GetRequestPageForReport(int reportId, Guid formHandle);

	MasterPage GetRequestPageForXmlPort(int xmlPortId, Guid formHandle);

	MasterPage GetFilterPage(Guid formHandle);

	MasterPage GetHomepage(string personalizationId, bool applyPersonalization);

	MasterNavigation GetNavigationFrame();

	MasterNavigation GetNavigationFrame(bool applyPersonalization, bool getNavigationFrameFromServer);

	void SaveConfiguration(MasterPage page, string personalizationId);

	void SavePageDataPersonalization(SavePageDataPersonalizationRequest savePersonalizationRequest, DataSourceType dataSourceType);

	MasterNavigation GetProductOverview(ProductOverviewResourceType productOverviewResourceType, bool getFromServer);
}
