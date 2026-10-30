using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using Microsoft.Dynamics.Nav.Client;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.ClientDesigner;
using Microsoft.Dynamics.Nav.Types.Data;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.Types.Office;

namespace Microsoft.Dynamics.Nav.Runtime;

internal sealed class TestServiceConnection : ServiceConnectionBase, IService, IDisposable, IMetadataService
{
	private readonly NavSession session;

	private IAsyncNavService navService;

	private IClientCallbackHandler clientCallbackHandler;

	public WaitingForServerCallToComplete OnWaitingForServerCallToComplete { get; set; }

	public ServerCallCompleted OnServerCallCompleted { get; set; }

	public IMetadataService MetadataService => this;

	private IAsyncNavService Server
	{
		get
		{
			if (navService == null)
			{
				navService = new AsyncNavServiceWrapper(InterfaceDataSerializerFactory.CreateWrapperInstance(session.CreateNavTestService()), makeAsyncCalls: false);
			}
			return navService;
		}
	}

	public override long MetadataToken => NavGlobal.MetadataProvider.VersionNumber;

	public IClientCallbackHandler CallbackHandler
	{
		get
		{
			return clientCallbackHandler;
		}
		set
		{
			clientCallbackHandler = value;
		}
	}

	public bool IsOpened
	{
		get
		{
			throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "IsOpened");
		}
	}

	public object Url
	{
		get
		{
			throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "get_Url");
		}
		set
		{
			throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "set_Url");
		}
	}

	public bool IsSpn
	{
		get
		{
			throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "IsSpn");
		}
	}

	public UserSettings Settings
	{
		get
		{
			UserSettings personalization = session.Personalization;
			personalization.Timeout = TimeSpan.MaxValue;
			personalization.OrphanGracePeriod = TimeSpan.FromMinutes(10.0);
			personalization.MaxUploadFileSize = 5;
			personalization.TimeZoneInfo = session.ClientTimeZone;
			personalization.CompanyName = session.CompanyName;
			return personalization;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public SystemIndicator SystemIndicator => new SystemIndicator(string.Empty, SystemIndicatorStyle.Standard);

	public string CompanyDisplayName => string.Empty;

	internal TestServiceConnection(NavSession session)
	{
		this.session = session;
	}

	public ReadOnlyCollection<int> GetIds(string metadataType)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "GetIds");
	}

	public NavDataSet GetRecord(string metadataType, int id)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "GetRecord");
	}

	public object InvokeCodeUnit(int objectId, string methodName, ref NavRecordState state, ref object[] args)
	{
		if (objectId == 1 && methodName == "DefaultRoleCenter")
		{
			return 9006;
		}
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "InvokeCodeUnit");
	}

	public Uri GetServicesUrl(NavClientType clientType, NavObjectType objectType, int objectId, NavRecordState recordState, bool useFilter)
	{
		throw new NotImplementedException();
	}

	public void OpenConnection()
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "OpenConnection");
	}

	public void CloseConnection()
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "CloseConnection");
	}

	public void SetClientSettings(SessionSettings sessionSettings, bool saveOnly)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "SetClientSettings");
	}

	public DateTime GetWorkDate()
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "GetWorkDate");
	}

	public DateTime SetWorkDate(DateTime workDate)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "SetWorkDate");
	}

	public NavDataSet GetAllowedCompanies()
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "GetAllowedCompanies");
	}

	public void OpenCompany(string companyName, bool overridePersonalization)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "OpenCompany");
	}

	public NavDataSet InvokeEventTrigger(ref NavRecordState state, NavDataSet recDataSet, int controlId, string eventName, ref object[] args)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "ControlAddIn");
	}

	public InvokeDesignerActionResponse InvokeDesignerAction(NavDesignerRequest designerAction)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "InvokeDesignerAction");
	}

	public MetaTable GetTableMetadata(int tableId)
	{
		return InterfaceDataSerializerFactory.Serializer.Serialize(NavGlobal.MetadataProvider.GetTableMetadata(tableId));
	}

	public MetaQuery GetQueryMetadata(int queryId)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "GetQueryMetadata");
	}

	public MetaTable GetQueryTableMetadata(int queryId)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "GetQueryTableMetadata");
	}

	public MasterPage GetMasterPage(int pageId, DataSourceType dataSourceType, string personalizationId, bool applyPersonalization)
	{
		return InterfaceDataSerializerFactory.Serializer.Serialize(NavGlobal.MetadataProvider.GetMasterPage(pageId, dataSourceType, personalizationId, applyPersonalization));
	}

	public MasterPage GetMasterPageWithoutConfiguration(int pageId, DataSourceType dataSourceType)
	{
		return InterfaceDataSerializerFactory.Serializer.Serialize(NavGlobal.MetadataProvider.GetMasterPageWithoutConfiguration(pageId));
	}

	public MasterPage GetMasterPage(int pageId, DataSourceType dataSourceType, string personalizationId, bool applyPersonalization, bool doGetDependencies)
	{
		return GetMasterPage(pageId, dataSourceType, personalizationId, applyPersonalization);
	}

	public MasterPage GetMasterPage(int pageId, DataSourceType dataSourceType, string personalizationId, bool applyPersonalization, bool doGetDependencies, string viewName)
	{
		return GetMasterPage(pageId, dataSourceType, personalizationId, applyPersonalization, doGetDependencies);
	}

	public MasterPage GetMasterPage(int pageId, DataSourceType dataSourceType)
	{
		return GetMasterPage(pageId, dataSourceType, string.Empty, applyPersonalization: false);
	}

	public MasterPage GetMasterPageTemplate(PageType pageType)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "GetMasterPageTemplate");
	}

	public MasterPage GetTableMasterPage(int tableId, string personalizationId)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "GetTableMasterPage");
	}

	public MasterPage GetSystemPart(SystemPartTypes partType)
	{
		return InterfaceDataSerializerFactory.Serializer.Serialize(NavGlobal.MetadataProvider.GetSystemPart(partType));
	}

	public ILocalReportHandle GetReportHandle(int reportId, Guid reportHandle, bool useCache)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "GetReportHandle");
	}

	public MasterPage GetRequestPageForReport(int reportId, Guid formHandle)
	{
		return Server.GetRequestPageForReport(formHandle);
	}

	public MasterPage GetFilterPage(Guid formHandle)
	{
		return Server.GetFilterPageForRecord(formHandle);
	}

	public MasterPage GetRequestPageForXmlPort(int xmlPortId, Guid formHandle)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "GetRequestPageForXmlPort");
	}

	public MasterPage GetHomepage(string personalizationId, bool applyPersonalization)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "GetHomepage");
	}

	public LaunchDebuggerResponse LaunchDebugger()
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "LaunchDebugger");
	}

	public void OpenLastErrorPage()
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "OpenLastErrorPage");
	}

	public SupportInformationResponse GetSupportInformation()
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "GetSupportInformation");
	}

	public void OpenSettings()
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "OpenSettings");
	}

	public MasterNavigation GetNavigationFrame()
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "GetNavigationFrame");
	}

	public MasterNavigation GetNavigationFrame(bool applyPersonalization, bool getNavigationFrameFromServer)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "GetNavigationFrame");
	}

	public MasterNavigation GetProductOverview(ProductOverviewResourceType productOverviewResourceType, bool getFromServer)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "GetProductOverview");
	}

	public NavDataRow GetChartRow(string chartPartId, bool includeBLOB)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "GetChartRow");
	}

	public byte[] CustomizeChart(string name, byte[] chartDefinition)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "CustomizeChart");
	}

	public void RunQuery(Guid value, int queryId)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "RunQuery");
	}

	public void RunQuery(Guid value, int queryId, IList<NavFilter> filters)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "RunQuery");
	}

	public void RunQuery(Guid value, int queryId, IList<NavFilter> filters, int topNumberOfRows)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "RunQuery");
	}

	public NavDataSet RunQueryDataSet(Guid value, IList<NavFilter> filters, MetaQuery metaQuery)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "RunQuery");
	}

	public void RunQueryOnForm(Guid value, int queryId)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "RunQueryOnForm");
	}

	public void RunQueryOnForm(Guid value, int queryId, IList<NavFilter> filters, int requestedTopNumberOfRows)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "RunQueryOnForm");
	}

	public string SendDataToDocument(NavRecordState recordState, OfficeDocumentType documentType)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "SendDataToDocument");
	}

	public void RunQuery(Guid value, IList<NavFilter> filters, MetaQuery metaQuery)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "RunQuery");
	}

	public void RunReport(int reportId)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "RunReport");
	}

	public void RunReport(int reportId, IList<string> textualFilters)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "RunReport");
	}

	public void RunReport(int reportId, ref NavRecordState state, NavDataSet recordDataSet)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "RunReport");
	}

	public void SaveConfiguration(MasterPage page, string personalizationId)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "SaveConfiguration");
	}

	public void SavePageDataPersonalization(SavePageDataPersonalizationRequest request, DataSourceType dataSourceType)
	{
		throw new NotImplementedException();
	}

	public void RunXmlPort(int xmlPortId)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "RunXmlPort");
	}

	public void RunTable(int tableId, string personalizationId, NavRecordState state)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "RunTable");
	}

	public void OnSessionTerminating(EventArgs e)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "OnSessionTerminating");
	}

	public void SendClientException(Exception exception)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "SendClientException");
	}

	public void CancelDialog()
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "CancelDialog");
	}

	public StreamTransfer Download()
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "Download");
	}

	public bool DownloadControlAddIn(string assemblyFullName)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "DownloadControlAddIn");
	}

	public void Upload(StreamTransfer stream)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "Upload");
	}

	public UploadTempFileResponse UploadTempFile(StreamTransfer stream)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "UploadTempFile");
	}

	public void HasExecutePermission(int objectId, ObjectType objectType)
	{
		throw NavTestMethodNotSupportedInPageTestException.Create(CultureInfo.CurrentCulture, "HasExecutePermission");
	}

	public ICollection<AddInRegistration> GetAddInRegistrations()
	{
		return null;
	}

	public void CancelRequest()
	{
		throw new NotImplementedException();
	}

	public void AssociateExchangeToken(string token)
	{
		throw new NotImplementedException();
	}

	public void Initialize()
	{
	}

	public void Dispose()
	{
	}

	public override T CallServer<T>(BeginCallServerMethod beginCallServerMethod, EndCallServerMethod<T> endCallServerMethod)
	{
		return endCallServerMethod(Server, beginCallServerMethod(Server));
	}

	public override T CallServer<T>(BeginCallServerMethod beginCallServerMethod, EndCallServerMethod<T> endCallServerMethod, ServerCallOptions options)
	{
		return endCallServerMethod(Server, beginCallServerMethod(Server));
	}

	public override void RunClientRequest(ProcessClientRequest clientRequest)
	{
		clientRequest();
	}

	public void EnableVerboseTelemetry(bool enableFullALFunctionTracing, TimeSpan duration)
	{
		throw new NotImplementedException();
	}

	public bool VerboseTraceLevelEnabled()
	{
		throw new NotImplementedException();
	}

	public bool FullALFunctionTracingEnabled()
	{
		throw new NotImplementedException();
	}

	NavDataSet IService.GetPage(PageRequestDefinition pageRequestDefinition, ref NavRecordState state)
	{
		return GetPage(pageRequestDefinition, ref state);
	}

	Image IService.GetMediaThumbnail(Guid mediaHandle, int width, int height, ref bool allowCaching)
	{
		return GetMediaThumbnail(mediaHandle, width, height, ref allowCaching);
	}

	bool IService.GetAggregates(NavRecordState state, ref NavDataSet aggregateData)
	{
		return GetAggregates(state, ref aggregateData);
	}

	bool IService.GetAggregatesAsync(NavRecordState state, ref NavDataSet aggregateData)
	{
		return GetAggregatesAsync(state, ref aggregateData);
	}

	NavDataSet IService.ModifyRecord(ref NavRecordState state, NavDataSet recDataSet)
	{
		return ModifyRecord(ref state, recDataSet);
	}

	NavDataSet IService.InsertRecord(ref NavRecordState state, NavDataSet recDataSet, bool belowXRec, bool isEmptyMultipleLine)
	{
		return InsertRecord(ref state, recDataSet, belowXRec, isEmptyMultipleLine);
	}

	byte[][] IService.DeleteRecord(ref NavRecordState state, NavDataSet recDataSet, bool deleteSelectedRecords)
	{
		return DeleteRecord(ref state, recDataSet, deleteSelectedRecords);
	}

	NavDataSet IService.RenameRecord(ref NavRecordState state, NavDataSet recDataSet)
	{
		return RenameRecord(ref state, recDataSet);
	}

	object IService.InvokeApplicationMethod(ApplicationCodeType objectType, int objectId, string methodName, ref NavDataSet dataSet, ref NavRecordState state, ref object[] args)
	{
		return InvokeApplicationMethod(objectType, objectId, methodName, ref dataSet, ref state, ref args);
	}

	string IService.EvaluateFilter(string toEvaluate, ObjectType objectType, int objectId, int fieldId)
	{
		return EvaluateFilter(toEvaluate, objectType, objectId, fieldId);
	}

	string IService.EvaluateQueryFilterByPosition(string toEvaluate, int objectId, int columnPosition)
	{
		return EvaluateQueryFilterByPosition(toEvaluate, objectId, columnPosition);
	}

	EvaluateBag IService.Evaluate(string toEvaluate, NavType navType)
	{
		return Evaluate(toEvaluate, navType);
	}

	void IService.OpenForm(ref NavOpenFormArguments mainForm, ref NavOpenFormArguments[] subForms)
	{
		OpenForm(ref mainForm, ref subForms);
	}

	void IService.CloseForm(ref NavCloseFormArguments mainForm, ref NavCloseFormArguments[] subForms, bool force)
	{
		CloseForm(ref mainForm, ref subForms, force);
	}

	NavDataSet IService.ValidateField(ref NavRecordState state, NavDataSet recDataSet, int controlId)
	{
		return ValidateField(ref state, recDataSet, controlId);
	}

	NavDataSet IService.LookupField(ref NavRecordState state, NavDataSet recDataSet, int controlId, ref string lookupValue)
	{
		return LookupField(ref state, recDataSet, controlId, ref lookupValue);
	}

	NavDataSet IService.DrilldownField(ref NavRecordState state, NavDataSet recDataSet, int controlId)
	{
		return DrilldownField(ref state, recDataSet, controlId);
	}

	NavDataSet IService.AssistEditField(ref NavRecordState state, NavDataSet recDataSet, int controlId)
	{
		return AssistEditField(ref state, recDataSet, controlId);
	}

	NavDataSet IService.ActionField(ref NavRecordState state, NavDataSet recDataSet, int controlId, int controlEventId)
	{
		return ActionField(ref state, recDataSet, controlId, controlEventId);
	}

	int IService.RecordCountApprox(int tableId)
	{
		return RecordCountApprox(tableId);
	}

	int IService.EvaluateRelation(NavRecordState state, NavDataSet dataSet, int fieldId)
	{
		return EvaluateRelation(state, dataSet, fieldId);
	}

	void IService.EndClientCall(object result)
	{
		EndClientCall(result);
	}

	void IService.EndClientCall(AutomationMethodResponse result)
	{
		EndClientCall(result);
	}

	void IService.EndClientCall(FileActionResponse result)
	{
		EndClientCall(result);
	}

	void IService.EndClientCall(FormResult result)
	{
		EndClientCall(result);
	}
}
