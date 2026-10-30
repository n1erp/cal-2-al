using System;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.Dynamics.Nav.Types.Channels;
using Microsoft.Dynamics.Nav.Types.Data;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.Types.Office;

namespace Microsoft.Dynamics.Nav.Types;

[ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IClientContract), Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public interface INavService
{
	[OperationContract(IsInitiating = true)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	UserSettings OpenConnection(ConnectionRequest connectionRequest);

	[OperationContract(IsInitiating = true)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	bool Reconnect(string previousSessionId, string tenantId);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void KeepAlive();

	[OperationContract(IsInitiating = false)]
	[ServiceKnownType(typeof(OpenCompanyResponse))]
	[ServiceKnownType(typeof(CodeUnitResponse))]
	[ServiceKnownType(typeof(LaunchDebuggerResponse))]
	[ServiceKnownType(typeof(SupportInformationResponse))]
	[ServiceKnownType(typeof(DataSetResponse))]
	[ServiceKnownType(typeof(DateTime))]
	[ServiceKnownType(typeof(EvaluateBag))]
	[ServiceKnownType(typeof(EvaluateRelationResponse))]
	[ServiceKnownType(typeof(ServerFault))]
	[ServiceKnownType(typeof(FormOpenResponse))]
	[ServiceKnownType(typeof(FormOpenCloseResponse))]
	[ServiceKnownType(typeof(MasterNavigation))]
	[ServiceKnownType(typeof(MasterPage))]
	[ServiceKnownType(typeof(MetaTable))]
	[ServiceKnownType(typeof(MetaQuery))]
	[ServiceKnownType(typeof(NSSystemInfoData))]
	[ServiceKnownType(typeof(PageDependencyResult))]
	[ServiceKnownType(typeof(SendDataToDocumentResponse))]
	[ServiceKnownType(typeof(UploadTempFileResponse))]
	[ServiceKnownType(typeof(ServicesUrlResponse))]
	[ServiceKnownType(typeof(RunReportResponse))]
	[ServiceKnownType(typeof(RecordDeletedResponse))]
	[ServiceKnownType(typeof(ServerResponse))]
	[ServiceKnownType(typeof(InvokeDesignerActionResponse))]
	[ServiceKnownType(typeof(NavDataSet))]
	[ServiceKnownType(typeof(byte[]))]
	[ServiceKnownType(typeof(string))]
	[ServiceKnownType(typeof(int))]
	[ServiceKnownType(typeof(bool))]
	[ServiceKnownType(typeof(ICollection<AddInRegistration>))]
	[ChunkingBehavior(ChunkOutput = true)]
	bool TryGetReturnValue(int messageId, out object returnValue);

	[OperationContract(IsTerminating = true, IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse CloseConnection();

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	DateTime GetWorkDate();

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	DateTime SetWorkDate(DateTime workDate);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	OpenCompanyResponse OpenCompany(string companyName, bool overridePersonalization = false);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	DataSetResponse GetAllowedCompanies();

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void SetClientSettings(SessionSettings settings, bool saveOnly);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	NSSystemInfoData SystemInfo();

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	FormOpenResponse OpenForm(OpenFormRequest form);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	FormOpenCloseResponse CloseForm(CloseFormRequest form);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	CodeUnitResponse LookupField(FieldRequest fieldRequest, NavRecordState dataSetState);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	CodeUnitResponse DrillDownField(FieldRequest fieldRequest, NavRecordState dataSetState);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	CodeUnitResponse AssistEdit(FieldRequest fieldRequest, NavRecordState dataSetState);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkOutput = true)]
	CodeUnitResponse ActionField(FieldRequest fieldRequest, NavRecordState dataSetState);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	CodeUnitResponse InvokeEventTrigger(FieldRequest fieldRequest, NavRecordState dataSetState, string eventName);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	InvokeDesignerActionResponse InvokeDesignerAction(DesignerRequest designerRequest);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkOutput = true)]
	MasterNavigation GetNavigationFrame(bool applyPersonalization);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkOutput = true)]
	MasterNavigation GetProductOverview(ProductOverviewResourceType productOverviewResourceType);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkOutput = true)]
	MasterPage GetSystemPart(SystemPartTypes partType);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkOutput = true)]
	MetaTable GetTableMetadata(int tableId);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkOutput = true)]
	MetaQuery GetQueryMetadata(int queryId);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkOutput = true)]
	MetaTable GetQueryTableMetadata(int queryId);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	byte[] GetReportRDLMetadata(Guid reportHandle, ref bool allowCaching, ref bool enableExternalImages, ref bool enableHyperlinks, ref bool enableExternalAssemblies, ref bool enableAppDomainIsolation);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkOutput = true)]
	byte[] GetMediaContent(Guid mediaHandle, ref bool allowCaching, ref string mimeType);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkOutput = true)]
	byte[] GetScaledMedia(Guid mediaHandle, int width, int height, ref bool allowCaching, ref string mimeType);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkOutput = true)]
	MasterPage GetRequestPageForReport(Guid formHandle);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkOutput = true)]
	MasterPage GetFilterPageForRecord(Guid formHandle);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkOutput = true)]
	MasterPage GetRequestPageForXmlPort(Guid formHandle);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkOutput = true)]
	MasterPage GetMasterPage(int pageId, DataSourceType dataSourceType, string personalizationId, bool applyPersonalization);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkOutput = true)]
	MasterPage GetMasterPageWithoutConfiguration(int pageId);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkOutput = true)]
	MasterPage GetMasterPageTemplate(PageType pageType);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkOutput = true)]
	MasterPage GetTableMasterPage(int tableId, string personalizationId);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkOutput = true)]
	MasterPage GetHomepage(string personalizationId, bool applyPersonalization);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	LaunchDebuggerResponse LaunchDebugger();

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void OpenLastErrorPage();

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void EnableVerboseTelemetry(bool enableFullALFunctionTracing, TimeSpan duration);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	bool VerboseTraceLevelEnabled();

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	bool FullALFunctionTracingEnabled();

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkOutput = true)]
	SupportInformationResponse GetSupportInformation();

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse OpenSettings();

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkInput = true)]
	void SaveConfiguration(MasterPage page, string personalizationId);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void SavePageDataPersonalization(string personalizationId, int objectId, ObjectType objectType, string dataIdentifier, string value);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void RunQuery(ApplicationMethodRequest applicationMethodRequest);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse RunQueryOnForm(ApplicationMethodRequest applicationMethodRequest);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkOutput = true)]
	NavDataSet RunQueryDataSet(ApplicationMethodRequest applicationMethodRequest);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	SendDataToDocumentResponse SendDataToDocument(NavRecordState recordState, OfficeDocumentType documentType);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServicesUrlResponse GetServicesUrl(NavClientType clientType, NavObjectType objectType, int objectId, NavRecordState recordState, bool useFilter);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	RunReportResponse RunReport(ApplicationMethodRequest applicationMethodRequest);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	RecordDeletedResponse DeleteRecord(NavRecordState dataSetState, RecordRequest recordRequest, bool deleteSelectedRecords);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	DataSetResponse RenameRecord(NavRecordState dataSetState, RecordRequest recordRequest);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkInput = true)]
	DataSetResponse ModifyRecord(NavRecordState dataSetState, RecordRequest recordRequest);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkInput = true)]
	DataSetResponse InsertRecord(NavRecordState dataSetState, RecordRequest recordRequest, bool belowXRecord, bool isEmptyMultipleLine);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	bool GetAggregates(NavRecordState dataSetState, ref NavDataSet dataSet);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	int RecordCountApprox(int tableId);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	CodeUnitResponse ValidateField(FieldRequest fieldRequest, NavRecordState dataSetState);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkOutput = true)]
	DataSetResponse GetPage(PageRequestDefinition pageRequestDefinition, NavRecordState state);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	CodeUnitResponse InvokeCodeUnit(ApplicationMethodRequest applicationMethodRequest, NavRecordState state);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkOutput = true)]
	[ServiceKnownType(typeof(NotificationInfo))]
	CodeUnitResponse InvokeApplicationMethod(ApplicationMethodRequest applicationMethodRequest, NavRecordState state);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	string EvaluateFilter(string toEvaluate, ObjectType objectType, int objectId, int fieldId);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	string EvaluateQueryFilterByPosition(string toEvaluate, int objectId, int columnPosition);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	EvaluateBag Evaluate(string toEvaluate, NavType navType);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse RunXmlPort(ApplicationMethodRequest applicationMethodRequest);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ServerResponse RunTable(RunTableRequest runTableRequest);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	EvaluateRelationResponse EvaluateRelation(FieldRequest fieldRequest, NavRecordState state);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void SendClientException(ServerFault fault, string faultReason);

	[OperationContract(IsInitiating = false)]
	[ServiceKnownType(typeof(UserNamePasswordResponse))]
	[ServiceKnownType(typeof(Printer[]))]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void EndClientCall(object value);

	[OperationContract(Name = "EndClientFileActionCall")]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void EndClientFileActionCall(FileActionResponse fileActionResponse);

	[OperationContract(Name = "EndClientFormRunModalCall")]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void EndClientFormRunModalCall(FormResult formResult);

	[OperationContract(Name = "EndClientAutomationMethodCall")]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void EndClientAutomationMethodCall(AutomationMethodResponse automationMethodResponse);

	[OperationContract(Name = "CancelDialog")]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void CancelDialog();

	[OperationContract(Name = "GetMetadataForPageAndAllItsDependencies")]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkOutput = true)]
	PageDependencyResult GetMetadataForPageAndAllItsDependencies(int pageId, DataSourceType dataSourceType, string personalizationId, bool applyPersonalization, ICollection<int> tablesToSkip, string viewName);

	[OperationContract(Name = "Download")]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkOutput = true)]
	StreamTransfer Download();

	[OperationContract(Name = "DownloadControlAddIn")]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	bool DownloadControlAddIn(string assemblyFullName);

	[OperationContract(Name = "Upload", IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkInput = true)]
	void Upload(StreamTransfer stream);

	[OperationContract(Name = "UploadTempFile", IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkInput = true)]
	UploadTempFileResponse UploadTempFile(StreamTransfer stream);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void HasExecutePermission(int objectId, ObjectType objectType);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	ICollection<AddInRegistration> GetAddInRegistrations();

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	DataSetResponse CustomizeChart(NavDataSet dataSet);

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void CancelRequest();

	[OperationContract(IsInitiating = false)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	void AssociateExchangeToken(string token);
}
