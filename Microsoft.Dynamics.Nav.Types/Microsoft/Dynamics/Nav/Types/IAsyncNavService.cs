using System;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.Dynamics.Nav.Types.Data;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.Types.Office;

namespace Microsoft.Dynamics.Nav.Types;

[ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IClientContract), Name = "INavService", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public interface IAsyncNavService : INavService
{
	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginOpenConnection(ConnectionRequest connectionRequest, AsyncCallback callback, object state);

	UserSettings EndOpenConnection(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginOpenCompany(string companyName, bool overridePersonalization, AsyncCallback callback, object state);

	OpenCompanyResponse EndOpenCompany(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginGetAllowedCompanies(AsyncCallback callback, object state);

	DataSetResponse EndGetAllowedCompanies(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginGetWorkDate(AsyncCallback callback, object state);

	DateTime EndGetWorkDate(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginSetWorkDate(DateTime workDate, AsyncCallback callback, object state);

	DateTime EndSetWorkDate(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginOpenForm(OpenFormRequest form, AsyncCallback callback, object state);

	FormOpenResponse EndOpenForm(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginCloseForm(CloseFormRequest form, AsyncCallback callback, object state);

	FormOpenCloseResponse EndCloseForm(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginValidateField(FieldRequest fieldRequest, NavRecordState dataSetState, AsyncCallback callback, object state);

	CodeUnitResponse EndValidateField(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginGetPage(PageRequestDefinition pageRequestDefinition, NavRecordState state, AsyncCallback callback, object asyncState);

	DataSetResponse EndGetPage(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginInvokeCodeUnit(ApplicationMethodRequest applicationMethodRequest, NavRecordState state, AsyncCallback callback, object asyncState);

	CodeUnitResponse EndInvokeCodeUnit(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginInvokeApplicationMethod(ApplicationMethodRequest applicationMethodRequest, NavRecordState state, AsyncCallback callback, object asyncState);

	CodeUnitResponse EndInvokeApplicationMethod(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginEvaluateFilter(string toEvaluate, ObjectType objectType, int objectId, int fieldId, AsyncCallback callback, object asyncState);

	string EndEvaluateFilter(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginEvaluateQueryFilterByPosition(string toEvaluate, int objectId, int columnPosition, AsyncCallback callback, object asyncState);

	string EndEvaluateQueryFilterByPosition(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginActionField(FieldRequest fieldRequest, NavRecordState dataSetState, AsyncCallback callback, object asyncState);

	CodeUnitResponse EndActionField(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginInvokeEventTrigger(FieldRequest fieldRequest, NavRecordState dataSetState, string eventName, AsyncCallback callback, object asyncState);

	CodeUnitResponse EndInvokeEventTrigger(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginGetNavigationFrame(bool applyPersonalization, AsyncCallback callback, object asyncState);

	MasterNavigation EndGetNavigationFrame(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginGetSystemPart(SystemPartTypes partType, AsyncCallback callback, object asyncState);

	MasterPage EndGetSystemPart(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginGetTableMetadata(int tableId, AsyncCallback callback, object asyncState);

	MetaTable EndGetTableMetadata(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginGetQueryMetadata(int queryId, AsyncCallback callback, object asyncState);

	MetaQuery EndGetQueryMetadata(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginGetQueryTableMetadata(int queryId, AsyncCallback callback, object asyncState);

	MetaTable EndGetQueryTableMetadata(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginGetReportRDLMetadata(Guid reportHandle, ref bool allowCaching, ref bool enableExternalImages, ref bool enableHyperlinks, ref bool enableExternalAssemblies, ref bool enableAppDomainIsolation, AsyncCallback callback, object asyncState);

	byte[] EndGetReportRDLMetadata(ref bool allowCaching, ref bool enableExternalImages, ref bool enableHyperlinks, ref bool enableExternalAssemblies, ref bool enableAppDomainIsolation, IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginGetMediaContent(Guid mediaHandle, ref bool allowCaching, ref string mimeType, AsyncCallback callback, object asyncState);

	byte[] EndGetMediaContent(ref bool allowCaching, ref string mimeType, IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginGetScaledMedia(Guid mediaHandle, int width, int height, ref bool allowCaching, ref string mimeType, AsyncCallback callback, object asyncState);

	byte[] EndGetScaledMedia(ref bool allowCaching, ref string mimeType, IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginGetFilterPageForRecord(Guid formHandle, AsyncCallback callback, object asyncState);

	MasterPage EndGetFilterPageForRecord(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginGetRequestPageForReport(Guid formHandle, AsyncCallback callback, object asyncState);

	MasterPage EndGetRequestPageForReport(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginGetRequestPageForXmlPort(Guid formHandle, AsyncCallback callback, object asyncState);

	MasterPage EndGetRequestPageForXmlPort(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginGetMasterPage(int pageId, DataSourceType dataSourceType, string personalizationId, bool applyPersonalization, AsyncCallback callback, object asyncState);

	MasterPage EndGetMasterPage(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginGetMasterPageWithoutConfiguration(int pageId, AsyncCallback callback, object asyncState);

	MasterPage EndGetMasterPageWithoutConfiguration(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginGetMasterPageTemplate(PageType pageType, AsyncCallback callback, object asyncState);

	MasterPage EndGetMasterPageTemplate(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginGetTableMasterPage(int tableId, string personalizationId, AsyncCallback callback, object asyncState);

	MasterPage EndGetTableMasterPage(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginGetHomepage(string personalizationId, bool applyPersonalization, AsyncCallback callback, object asyncState);

	MasterPage EndGetHomepage(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginLaunchDebugger(AsyncCallback callback, object asyncState);

	LaunchDebuggerResponse EndLaunchDebugger(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginOpenLastErrorPage(AsyncCallback callback, object asyncState);

	void EndOpenLastErrorPage(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginEnableVerboseTelemetry(bool enableFullALFunctionTracing, TimeSpan duration, AsyncCallback callback, object asyncState);

	void EndEnableVerboseTelemetry(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginVerboseTraceLevelEnabled(AsyncCallback callback, object asyncState);

	bool EndVerboseTraceLevelEnabled(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginFullALFunctionTracingEnabled(AsyncCallback callback, object asyncState);

	bool EndFullALFunctionTracingEnabled(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginGetSupportInformation(AsyncCallback callback, object asyncState);

	SupportInformationResponse EndGetSupportInformation(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginOpenSettings(AsyncCallback callback, object asyncState);

	ServerResponse EndOpenSettings(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginSaveConfiguration(MasterPage page, string personalizationId, AsyncCallback callback, object asyncState);

	void EndSaveConfiguration(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginSavePageDataPersonalization(string personalizationId, int objectId, ObjectType objectType, string dataIdentifier, string value, AsyncCallback callback, object asyncState);

	void EndSavePageDataPersonalization(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginRunQuery(ApplicationMethodRequest applicationMethodRequest, AsyncCallback callback, object asyncState);

	void EndRunQuery(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginRunQueryOnForm(ApplicationMethodRequest applicationMethodRequest, AsyncCallback callback, object asyncState);

	ServerResponse EndRunQueryOnForm(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginSendDataToDocument(NavRecordState recordState, OfficeDocumentType documentType, AsyncCallback callback, object asyncState);

	SendDataToDocumentResponse EndSendDataToDocument(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginGetServicesUrl(NavClientType clientType, NavObjectType objectType, int objectId, NavRecordState recordState, bool useFilter, AsyncCallback callback, object asyncState);

	ServicesUrlResponse EndGetServicesUrl(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginRunQueryDataSet(ApplicationMethodRequest applicationMethodRequest, AsyncCallback callback, object asyncState);

	NavDataSet EndRunQueryDataSet(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginRunReport(ApplicationMethodRequest applicationMethodRequest, AsyncCallback callback, object asyncState);

	RunReportResponse EndRunReport(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginLookupField(FieldRequest fieldRequest, NavRecordState dataSetState, AsyncCallback callback, object asyncState);

	CodeUnitResponse EndLookupField(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginDrillDownField(FieldRequest fieldRequest, NavRecordState dataSetState, AsyncCallback callback, object asyncState);

	CodeUnitResponse EndDrillDownField(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginAssistEdit(FieldRequest fieldRequest, NavRecordState dataSetState, AsyncCallback callback, object asyncState);

	CodeUnitResponse EndAssistEdit(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginEvaluate(string toEvaluate, NavType navType, AsyncCallback callback, object asyncState);

	EvaluateBag EndEvaluate(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginRunXmlPort(ApplicationMethodRequest applicationMethodRequest, AsyncCallback callback, object asyncState);

	ServerResponse EndRunXmlPort(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginRunTable(RunTableRequest runTableRequest, AsyncCallback callback, object asyncState);

	ServerResponse EndRunTable(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginEvaluateRelation(FieldRequest fieldRequest, NavRecordState state, AsyncCallback callback, object asyncState);

	EvaluateRelationResponse EndEvaluateRelation(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginSendClientException(ServerFault fault, string faultReason, AsyncCallback callback, object asyncState);

	void EndSendClientException(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginEndClientCall(object value, AsyncCallback callback, object asyncState);

	void EndEndClientCall(IAsyncResult result);

	[OperationContract(Name = "EndClientFileActionCall", AsyncPattern = true)]
	IAsyncResult BeginEndClientFileActionCall(FileActionResponse fileActionResponse, AsyncCallback callback, object asyncState);

	void EndEndClientFileActionCall(IAsyncResult result);

	[OperationContract(Name = "EndClientFormRunModalCall", AsyncPattern = true)]
	IAsyncResult BeginEndClientFormRunModalCall(FormResult formResult, AsyncCallback callback, object asyncState);

	void EndEndClientFormRunModalCall(IAsyncResult result);

	[OperationContract(Name = "EndClientAutomationMethodCall", AsyncPattern = true)]
	IAsyncResult BeginEndClientAutomationMethodCall(AutomationMethodResponse automationMethodResponse, AsyncCallback callback, object asyncState);

	void EndEndClientAutomationMethodCall(IAsyncResult result);

	[OperationContract(Name = "CancelDialog", AsyncPattern = true)]
	IAsyncResult BeginCancelDialog(AsyncCallback callback, object asyncState);

	void EndCancelDialog(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginSetClientSettings(SessionSettings settings, bool saveOnly, AsyncCallback callback, object asyncState);

	void EndSetClientSettings(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginSystemInfo(AsyncCallback callback, object asyncState);

	NSSystemInfoData EndSystemInfo(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginGetAddInRegistrations(AsyncCallback callback, object asyncState);

	ICollection<AddInRegistration> EndGetAddInRegistrations(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginDeleteRecord(NavRecordState dataSetState, RecordRequest recordRequest, bool deleteSelectedRecords, AsyncCallback callback, object asyncState);

	RecordDeletedResponse EndDeleteRecord(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginRenameRecord(NavRecordState dataSetState, RecordRequest recordRequest, AsyncCallback callback, object asyncState);

	DataSetResponse EndRenameRecord(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginModifyRecord(NavRecordState dataSetState, RecordRequest recordRequest, AsyncCallback callback, object asyncState);

	DataSetResponse EndModifyRecord(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginInsertRecord(NavRecordState dataSetState, RecordRequest recordRequest, bool belowXRecord, bool isEmptyMultipleLine, AsyncCallback callback, object asyncState);

	DataSetResponse EndInsertRecord(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginGetAggregates(NavRecordState dataSetState, ref NavDataSet dataSet, AsyncCallback callback, object asyncState);

	bool EndGetAggregates(ref NavDataSet dataSet, IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginRecordCountApprox(int tableId, AsyncCallback callback, object asyncState);

	int EndRecordCountApprox(IAsyncResult result);

	[OperationContract(Name = "GetMetadataForPageAndAllItsDependencies", AsyncPattern = true)]
	IAsyncResult BeginGetMetadataForPageAndAllItsDependencies(int pageId, DataSourceType dataSourceType, string personalizationId, bool applyPersonalization, ICollection<int> tablesToSkip, string viewName, AsyncCallback callback, object asyncState);

	PageDependencyResult EndGetMetadataForPageAndAllItsDependencies(IAsyncResult result);

	[OperationContract(Name = "Download", AsyncPattern = true)]
	IAsyncResult BeginDownload(AsyncCallback callback, object asyncState);

	StreamTransfer EndDownload(IAsyncResult result);

	[OperationContract(Name = "DownloadControlAddIn", AsyncPattern = true)]
	IAsyncResult BeginDownloadControlAddIn(string assemblyFullName, AsyncCallback callback, object asyncState);

	bool EndDownloadControlAddIn(IAsyncResult result);

	[OperationContract(Name = "Upload", IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginUpload(StreamTransfer stream, AsyncCallback callback, object asyncState);

	void EndUpload(IAsyncResult result);

	[OperationContract(Name = "UploadTempFile", IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginUploadTempFile(StreamTransfer stream, AsyncCallback callback, object asyncState);

	UploadTempFileResponse EndUploadTempFile(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginHasExecutePermission(int objectId, ObjectType objectType, AsyncCallback callback, object asyncState);

	void EndHasExecutePermission(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginCustomizeChart(NavDataSet dataSet, AsyncCallback callback, object asyncState);

	DataSetResponse EndCustomizeChart(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginCancelRequest(AsyncCallback callback, object asyncState);

	void EndCancelRequest(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginAssociateExchangeToken(string token, AsyncCallback callback, object asyncState);

	void EndAssociateExchangeToken(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginInvokeDesignerAction(DesignerRequest designerRequest, AsyncCallback callback, object asyncState);

	InvokeDesignerActionResponse EndInvokeDesignerAction(IAsyncResult result);

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	IAsyncResult BeginGetProductOverview(ProductOverviewResourceType productOverviewResourceType, AsyncCallback callback, object asyncState);

	MasterNavigation EndGetProductOverview(IAsyncResult result);
}
