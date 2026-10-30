using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.ClientDesigner;
using Microsoft.Dynamics.Nav.Types.Data;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.Types.Office;

namespace Microsoft.Dynamics.Nav.Client;

public interface IService : IDisposable
{
	WaitingForServerCallToComplete OnWaitingForServerCallToComplete { get; set; }

	ServerCallCompleted OnServerCallCompleted { get; set; }

	IMetadataService MetadataService { get; }

	IClientCallbackHandler CallbackHandler { get; set; }

	bool IsOpened { get; }

	object Url { get; set; }

	bool IsSpn { get; }

	UserSettings Settings { get; set; }

	SystemIndicator SystemIndicator { get; }

	string CompanyDisplayName { get; }

	NavDataSet GetPage(PageRequestDefinition pageRequestDefinition, ref NavRecordState state);

	Image GetMediaThumbnail(Guid mediaHandle, int width, int height, ref bool allowCaching);

	ReadOnlyCollection<int> GetIds(string metadataType);

	NavDataSet GetRecord(string metadataType, int id);

	bool GetAggregates(NavRecordState state, ref NavDataSet aggregateData);

	bool GetAggregatesAsync(NavRecordState state, ref NavDataSet aggregateData);

	NavDataSet ModifyRecord(ref NavRecordState state, NavDataSet recDataSet);

	NavDataSet InsertRecord(ref NavRecordState state, NavDataSet recDataSet, bool belowXRec, bool isEmptyMultipleLine);

	byte[][] DeleteRecord(ref NavRecordState state, NavDataSet recDataSet, bool deleteSelectedRecords);

	NavDataSet RenameRecord(ref NavRecordState state, NavDataSet recDataSet);

	object InvokeCodeUnit(int objectId, string methodName, ref NavRecordState state, ref object[] args);

	object InvokeApplicationMethod(ApplicationCodeType objectType, int objectId, string methodName, ref NavDataSet dataSet, ref NavRecordState state, ref object[] args);

	string SendDataToDocument(NavRecordState recordState, OfficeDocumentType documentType);

	Uri GetServicesUrl(NavClientType clientType, NavObjectType objectType, int objectId, NavRecordState recordState, bool useFilter);

	void OpenConnection();

	void CloseConnection();

	void SetClientSettings(SessionSettings sessionSettings, bool saveOnly);

	string EvaluateFilter(string toEvaluate, ObjectType objectType, int objectId, int fieldId);

	string EvaluateQueryFilterByPosition(string toEvaluate, int objectId, int columnPosition);

	EvaluateBag Evaluate(string toEvaluate, NavType navType);

	DateTime GetWorkDate();

	DateTime SetWorkDate(DateTime workDate);

	NavDataSet GetAllowedCompanies();

	void OpenCompany(string companyName, bool overridePersonalization = false);

	void OpenForm(ref NavOpenFormArguments mainForm, ref NavOpenFormArguments[] subForms);

	void CloseForm(ref NavCloseFormArguments mainForm, ref NavCloseFormArguments[] subForms, bool force);

	NavDataSet ValidateField(ref NavRecordState state, NavDataSet recDataSet, int controlId);

	NavDataSet LookupField(ref NavRecordState state, NavDataSet recDataSet, int controlId, ref string lookupValue);

	NavDataSet DrilldownField(ref NavRecordState state, NavDataSet recDataSet, int controlId);

	NavDataSet AssistEditField(ref NavRecordState state, NavDataSet recDataSet, int controlId);

	NavDataSet ActionField(ref NavRecordState state, NavDataSet recDataSet, int controlId, int controlEventId);

	NavDataSet InvokeEventTrigger(ref NavRecordState state, NavDataSet recDataSet, int controlId, string eventName, ref object[] args);

	InvokeDesignerActionResponse InvokeDesignerAction(NavDesignerRequest designerAction);

	LaunchDebuggerResponse LaunchDebugger();

	void OpenLastErrorPage();

	void EnableVerboseTelemetry(bool enableFullALFunctionTracing, TimeSpan duration);

	bool VerboseTraceLevelEnabled();

	bool FullALFunctionTracingEnabled();

	SupportInformationResponse GetSupportInformation();

	void OpenSettings();

	NavDataRow GetChartRow(string chartPartId, bool includeBLOB);

	byte[] CustomizeChart(string name, byte[] chartDefinition);

	void RunQuery(Guid value, int queryId);

	void RunQuery(Guid value, int queryId, IList<NavFilter> filters);

	void RunQuery(Guid value, int queryId, IList<NavFilter> filters, int topNumberOfRows);

	void RunQuery(Guid value, IList<NavFilter> filters, MetaQuery metaQuery);

	NavDataSet RunQueryDataSet(Guid value, IList<NavFilter> filters, MetaQuery metaQuery);

	void RunQueryOnForm(Guid value, int queryId);

	void RunQueryOnForm(Guid value, int queryId, IList<NavFilter> filters, int requestedTopNumberOfRows);

	void RunReport(int reportId);

	void RunReport(int reportId, IList<string> textualFilters);

	void RunReport(int reportId, ref NavRecordState state, NavDataSet recordDataSet);

	int RecordCountApprox(int tableId);

	void RunXmlPort(int xmlPortId);

	void RunTable(int tableId, string personalizationId, NavRecordState state);

	int EvaluateRelation(NavRecordState state, NavDataSet dataSet, int fieldId);

	void RunClientRequest(ProcessClientRequest clientRequest);

	void OnSessionTerminating(EventArgs e);

	void SendClientException(Exception exception);

	void EndClientCall(object result);

	void EndClientCall(AutomationMethodResponse result);

	void EndClientCall(FileActionResponse result);

	void EndClientCall(FormResult result);

	void CancelDialog();

	StreamTransfer Download();

	bool DownloadControlAddIn(string assemblyFullName);

	void Upload(StreamTransfer stream);

	UploadTempFileResponse UploadTempFile(StreamTransfer stream);

	void HasExecutePermission(int objectId, ObjectType objectType);

	void Initialize();

	ICollection<AddInRegistration> GetAddInRegistrations();

	void CancelRequest();

	void AssociateExchangeToken(string token);
}
