using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using Microsoft.Dynamics.Nav.Types.Data;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.Types.Office;

namespace Microsoft.Dynamics.Nav.Types;

public class AsyncNavServiceWrapper : IAsyncNavService, INavService, IContextChannel, IChannel, ICommunicationObject, IExtensibleObject<IContextChannel>
{
	private class GetReportRDLMetadataExtraState
	{
		internal bool AllowCaching;

		internal bool EnableExternalImages;

		internal bool EnableHyperlinks;

		internal bool EnableExternalAssemblies;

		internal bool EnableAppDomainIsolation;

		public GetReportRDLMetadataExtraState(ref bool allowCaching, ref bool enableExternalImages, ref bool enableHyperlinks, ref bool enableExternalAssemblies, ref bool enableAppDomainIsolation)
		{
			AllowCaching = allowCaching;
			EnableExternalImages = enableExternalImages;
			EnableHyperlinks = enableHyperlinks;
			EnableExternalAssemblies = enableExternalAssemblies;
			EnableAppDomainIsolation = enableAppDomainIsolation;
		}
	}

	private class BeginGetMediaExtraState
	{
		internal bool AllowCaching;

		internal string MimeType;

		public BeginGetMediaExtraState(ref bool allowCaching, ref string mimeType)
		{
			AllowCaching = allowCaching;
			MimeType = mimeType;
		}
	}

	private class GetAggregatesExtraState
	{
		internal NavDataSet DataSet;

		public GetAggregatesExtraState(ref NavDataSet dataSet)
		{
			DataSet = dataSet;
		}
	}

	private class AsyncResult : IAsyncResult
	{
		public IAsyncResult InnerAsyncResult { get; private set; }

		public object ExtraState { get; private set; }

		public Func<object> Func { get; private set; }

		public Action Action { get; private set; }

		public bool IsCompleted => InnerAsyncResult.IsCompleted;

		public WaitHandle AsyncWaitHandle => InnerAsyncResult.AsyncWaitHandle;

		public object AsyncState => InnerAsyncResult.AsyncState;

		public bool CompletedSynchronously => InnerAsyncResult.CompletedSynchronously;

		public AsyncResult(IAsyncResult innerAsyncResult, Func<object> func, object extraState = null)
		{
			InnerAsyncResult = innerAsyncResult;
			Func = func;
			ExtraState = extraState;
		}

		public AsyncResult(IAsyncResult innerAsyncResult, Action action, object extraState = null)
		{
			InnerAsyncResult = innerAsyncResult;
			Action = action;
			ExtraState = extraState;
		}
	}

	private readonly IContextChannel wrappedContextChannel;

	private readonly INavService service;

	private readonly bool makeAsyncCalls;

	public CommunicationState State
	{
		get
		{
			if (wrappedContextChannel != null)
			{
				return wrappedContextChannel.State;
			}
			throw new NotSupportedException();
		}
	}

	public IExtensionCollection<IContextChannel> Extensions
	{
		get
		{
			if (wrappedContextChannel != null)
			{
				return wrappedContextChannel.Extensions;
			}
			throw new NotSupportedException();
		}
	}

	public bool AllowOutputBatching
	{
		get
		{
			if (wrappedContextChannel != null)
			{
				return wrappedContextChannel.AllowOutputBatching;
			}
			throw new NotSupportedException();
		}
		set
		{
			if (wrappedContextChannel != null)
			{
				wrappedContextChannel.AllowOutputBatching = value;
				return;
			}
			throw new NotSupportedException();
		}
	}

	public IInputSession InputSession
	{
		get
		{
			if (wrappedContextChannel != null)
			{
				return wrappedContextChannel.InputSession;
			}
			throw new NotSupportedException();
		}
	}

	public EndpointAddress LocalAddress
	{
		get
		{
			if (wrappedContextChannel != null)
			{
				return wrappedContextChannel.LocalAddress;
			}
			throw new NotSupportedException();
		}
	}

	public TimeSpan OperationTimeout
	{
		get
		{
			if (wrappedContextChannel != null)
			{
				return wrappedContextChannel.OperationTimeout;
			}
			throw new NotSupportedException();
		}
		set
		{
			if (wrappedContextChannel != null)
			{
				wrappedContextChannel.OperationTimeout = value;
				return;
			}
			throw new NotSupportedException();
		}
	}

	public IOutputSession OutputSession
	{
		get
		{
			if (wrappedContextChannel != null)
			{
				return wrappedContextChannel.OutputSession;
			}
			throw new NotSupportedException();
		}
	}

	public EndpointAddress RemoteAddress
	{
		get
		{
			if (wrappedContextChannel != null)
			{
				return wrappedContextChannel.RemoteAddress;
			}
			throw new NotSupportedException();
		}
	}

	public string SessionId
	{
		get
		{
			if (wrappedContextChannel != null)
			{
				return wrappedContextChannel.SessionId;
			}
			throw new NotSupportedException();
		}
	}

	public event EventHandler Closed
	{
		add
		{
			if (wrappedContextChannel != null)
			{
				wrappedContextChannel.Closed += value;
				return;
			}
			throw new NotSupportedException();
		}
		remove
		{
			if (wrappedContextChannel != null)
			{
				wrappedContextChannel.Closed -= value;
				return;
			}
			throw new NotSupportedException();
		}
	}

	public event EventHandler Closing
	{
		add
		{
			if (wrappedContextChannel != null)
			{
				wrappedContextChannel.Closing += value;
				return;
			}
			throw new NotSupportedException();
		}
		remove
		{
			if (wrappedContextChannel != null)
			{
				wrappedContextChannel.Closing -= value;
				return;
			}
			throw new NotSupportedException();
		}
	}

	public event EventHandler Faulted
	{
		add
		{
			if (wrappedContextChannel != null)
			{
				wrappedContextChannel.Faulted += value;
				return;
			}
			throw new NotSupportedException();
		}
		remove
		{
			if (wrappedContextChannel != null)
			{
				wrappedContextChannel.Faulted -= value;
				return;
			}
			throw new NotSupportedException();
		}
	}

	public event EventHandler Opened
	{
		add
		{
			if (wrappedContextChannel != null)
			{
				wrappedContextChannel.Opened += value;
				return;
			}
			throw new NotSupportedException();
		}
		remove
		{
			if (wrappedContextChannel != null)
			{
				wrappedContextChannel.Opened -= value;
				return;
			}
			throw new NotSupportedException();
		}
	}

	public event EventHandler Opening
	{
		add
		{
			if (wrappedContextChannel != null)
			{
				wrappedContextChannel.Opening += value;
				return;
			}
			throw new NotSupportedException();
		}
		remove
		{
			if (wrappedContextChannel != null)
			{
				wrappedContextChannel.Opening -= value;
				return;
			}
			throw new NotSupportedException();
		}
	}

	public AsyncNavServiceWrapper(INavService service)
		: this(service, makeAsyncCalls: true)
	{
	}

	public AsyncNavServiceWrapper(INavService service, bool makeAsyncCalls)
	{
		if (service == null)
		{
			throw new ArgumentNullException("service");
		}
		this.service = service;
		wrappedContextChannel = service as IContextChannel;
		this.makeAsyncCalls = makeAsyncCalls;
	}

	public UserSettings OpenConnection(ConnectionRequest connectionRequest)
	{
		return service.OpenConnection(connectionRequest);
	}

	public bool Reconnect(string previousSessionId, string tenantId)
	{
		return service.Reconnect(previousSessionId, tenantId);
	}

	public void KeepAlive()
	{
		service.KeepAlive();
	}

	public bool TryGetReturnValue(int messageId, out object returnValue)
	{
		return service.TryGetReturnValue(messageId, out returnValue);
	}

	public ServerResponse CloseConnection()
	{
		return service.CloseConnection();
	}

	public DateTime GetWorkDate()
	{
		return service.GetWorkDate();
	}

	public DateTime SetWorkDate(DateTime workDate)
	{
		return service.SetWorkDate(workDate);
	}

	public OpenCompanyResponse OpenCompany(string companyName, bool overridePersonalization)
	{
		return service.OpenCompany(companyName, overridePersonalization);
	}

	public DataSetResponse GetAllowedCompanies()
	{
		return service.GetAllowedCompanies();
	}

	public void SetClientSettings(SessionSettings settings, bool saveOnly)
	{
		service.SetClientSettings(settings, saveOnly);
	}

	public NSSystemInfoData SystemInfo()
	{
		return service.SystemInfo();
	}

	public FormOpenResponse OpenForm(OpenFormRequest form)
	{
		return service.OpenForm(form);
	}

	public FormOpenCloseResponse CloseForm(CloseFormRequest form)
	{
		return service.CloseForm(form);
	}

	public CodeUnitResponse LookupField(FieldRequest fieldRequest, NavRecordState dataSetState)
	{
		return service.LookupField(fieldRequest, dataSetState);
	}

	public CodeUnitResponse DrillDownField(FieldRequest fieldRequest, NavRecordState dataSetState)
	{
		return service.DrillDownField(fieldRequest, dataSetState);
	}

	public CodeUnitResponse AssistEdit(FieldRequest fieldRequest, NavRecordState dataSetState)
	{
		return service.AssistEdit(fieldRequest, dataSetState);
	}

	public CodeUnitResponse ActionField(FieldRequest fieldRequest, NavRecordState dataSetState)
	{
		return service.ActionField(fieldRequest, dataSetState);
	}

	public CodeUnitResponse InvokeEventTrigger(FieldRequest fieldRequest, NavRecordState dataSetState, string eventName)
	{
		return service.InvokeEventTrigger(fieldRequest, dataSetState, eventName);
	}

	public MasterNavigation GetNavigationFrame(bool applyPersonalization)
	{
		return service.GetNavigationFrame(applyPersonalization);
	}

	public MasterNavigation GetProductOverview(ProductOverviewResourceType productOverviewResourceType)
	{
		return service.GetProductOverview(productOverviewResourceType);
	}

	public MasterPage GetSystemPart(SystemPartTypes partType)
	{
		return service.GetSystemPart(partType);
	}

	public MetaTable GetTableMetadata(int tableId)
	{
		return service.GetTableMetadata(tableId);
	}

	public MetaQuery GetQueryMetadata(int queryId)
	{
		return service.GetQueryMetadata(queryId);
	}

	public MetaTable GetQueryTableMetadata(int queryId)
	{
		return service.GetQueryTableMetadata(queryId);
	}

	public byte[] GetReportRDLMetadata(Guid reportHandle, ref bool allowCaching, ref bool enableExternalImages, ref bool enableHyperlinks, ref bool enableExternalAssemblies, ref bool enableAppDomainIsolation)
	{
		return service.GetReportRDLMetadata(reportHandle, ref allowCaching, ref enableExternalImages, ref enableHyperlinks, ref enableExternalAssemblies, ref enableAppDomainIsolation);
	}

	public byte[] GetMediaContent(Guid mediaHandle, ref bool allowCaching, ref string mimeType)
	{
		return service.GetMediaContent(mediaHandle, ref allowCaching, ref mimeType);
	}

	public byte[] GetScaledMedia(Guid mediaHandle, int width, int height, ref bool allowCaching, ref string mimeType)
	{
		return service.GetScaledMedia(mediaHandle, width, height, ref allowCaching, ref mimeType);
	}

	public MasterPage GetFilterPageForRecord(Guid formHandle)
	{
		return service.GetFilterPageForRecord(formHandle);
	}

	public MasterPage GetRequestPageForReport(Guid formHandle)
	{
		return service.GetRequestPageForReport(formHandle);
	}

	public MasterPage GetRequestPageForXmlPort(Guid formHandle)
	{
		return service.GetRequestPageForXmlPort(formHandle);
	}

	public MasterPage GetMasterPage(int pageId, DataSourceType dataSourceType, string personalizationId, bool applyPersonalization)
	{
		return service.GetMasterPage(pageId, dataSourceType, personalizationId, applyPersonalization);
	}

	public MasterPage GetMasterPageWithoutConfiguration(int pageId)
	{
		return service.GetMasterPageWithoutConfiguration(pageId);
	}

	public MasterPage GetMasterPageTemplate(PageType pageType)
	{
		return service.GetMasterPageTemplate(pageType);
	}

	public MasterPage GetTableMasterPage(int tableId, string personalizationId)
	{
		return service.GetTableMasterPage(tableId, personalizationId);
	}

	public MasterPage GetHomepage(string personalizationId, bool applyPersonalization)
	{
		return service.GetHomepage(personalizationId, applyPersonalization);
	}

	public LaunchDebuggerResponse LaunchDebugger()
	{
		return service.LaunchDebugger();
	}

	public void OpenLastErrorPage()
	{
		service.OpenLastErrorPage();
	}

	public void EnableVerboseTelemetry(bool enableFullALFunctionTracing, TimeSpan duration)
	{
		service.EnableVerboseTelemetry(enableFullALFunctionTracing, duration);
	}

	public bool VerboseTraceLevelEnabled()
	{
		return service.VerboseTraceLevelEnabled();
	}

	public bool FullALFunctionTracingEnabled()
	{
		return service.FullALFunctionTracingEnabled();
	}

	public SupportInformationResponse GetSupportInformation()
	{
		return service.GetSupportInformation();
	}

	public ServerResponse OpenSettings()
	{
		return service.OpenSettings();
	}

	public void SaveConfiguration(MasterPage page, string personalizationId)
	{
		service.SaveConfiguration(page, personalizationId);
	}

	public void SavePageDataPersonalization(string personalizationId, int objectId, ObjectType objectType, string dataIdentifier, string value)
	{
		service.SavePageDataPersonalization(personalizationId, objectId, objectType, dataIdentifier, value);
	}

	public void RunQuery(ApplicationMethodRequest applicationMethodRequest)
	{
		service.RunQuery(applicationMethodRequest);
	}

	public ServerResponse RunQueryOnForm(ApplicationMethodRequest applicationMethodRequest)
	{
		return service.RunQueryOnForm(applicationMethodRequest);
	}

	public NavDataSet RunQueryDataSet(ApplicationMethodRequest applicationMethodRequest)
	{
		return service.RunQueryDataSet(applicationMethodRequest);
	}

	public SendDataToDocumentResponse SendDataToDocument(NavRecordState recordState, OfficeDocumentType documentType)
	{
		return service.SendDataToDocument(recordState, documentType);
	}

	public ServicesUrlResponse GetServicesUrl(NavClientType clientType, NavObjectType objectType, int objectId, NavRecordState recordState, bool useFilter)
	{
		return service.GetServicesUrl(clientType, objectType, objectId, recordState, useFilter);
	}

	public RunReportResponse RunReport(ApplicationMethodRequest applicationMethodRequest)
	{
		return service.RunReport(applicationMethodRequest);
	}

	public RecordDeletedResponse DeleteRecord(NavRecordState dataSetState, RecordRequest recordRequest, bool deleteSelectedRecords)
	{
		return service.DeleteRecord(dataSetState, recordRequest, deleteSelectedRecords);
	}

	public DataSetResponse RenameRecord(NavRecordState dataSetState, RecordRequest recordRequest)
	{
		return service.RenameRecord(dataSetState, recordRequest);
	}

	public DataSetResponse ModifyRecord(NavRecordState dataSetState, RecordRequest recordRequest)
	{
		return service.ModifyRecord(dataSetState, recordRequest);
	}

	public DataSetResponse InsertRecord(NavRecordState dataSetState, RecordRequest recordRequest, bool belowXRecord, bool isEmptyMultipleLine)
	{
		return service.InsertRecord(dataSetState, recordRequest, belowXRecord, isEmptyMultipleLine);
	}

	public bool GetAggregates(NavRecordState dataSetState, ref NavDataSet dataSet)
	{
		return service.GetAggregates(dataSetState, ref dataSet);
	}

	public int RecordCountApprox(int tableId)
	{
		return service.RecordCountApprox(tableId);
	}

	public CodeUnitResponse ValidateField(FieldRequest fieldRequest, NavRecordState dataSetState)
	{
		return service.ValidateField(fieldRequest, dataSetState);
	}

	public DataSetResponse GetPage(PageRequestDefinition pageRequestDefinition, NavRecordState state)
	{
		return service.GetPage(pageRequestDefinition, state);
	}

	public CodeUnitResponse InvokeCodeUnit(ApplicationMethodRequest applicationMethodRequest, NavRecordState state)
	{
		return service.InvokeCodeUnit(applicationMethodRequest, state);
	}

	public CodeUnitResponse InvokeApplicationMethod(ApplicationMethodRequest applicationMethodRequest, NavRecordState state)
	{
		return service.InvokeApplicationMethod(applicationMethodRequest, state);
	}

	public string EvaluateFilter(string toEvaluate, ObjectType objectType, int objectId, int fieldId)
	{
		return service.EvaluateFilter(toEvaluate, objectType, objectId, fieldId);
	}

	public string EvaluateQueryFilterByPosition(string toEvaluate, int objectId, int columnPosition)
	{
		return service.EvaluateQueryFilterByPosition(toEvaluate, objectId, columnPosition);
	}

	public EvaluateBag Evaluate(string toEvaluate, NavType navType)
	{
		return service.Evaluate(toEvaluate, navType);
	}

	public ServerResponse RunXmlPort(ApplicationMethodRequest applicationMethodRequest)
	{
		return service.RunXmlPort(applicationMethodRequest);
	}

	public ServerResponse RunTable(RunTableRequest runTableRequest)
	{
		return service.RunTable(runTableRequest);
	}

	public EvaluateRelationResponse EvaluateRelation(FieldRequest fieldRequest, NavRecordState state)
	{
		return service.EvaluateRelation(fieldRequest, state);
	}

	public void SendClientException(ServerFault fault, string faultReason)
	{
		service.SendClientException(fault, faultReason);
	}

	public void EndClientCall(object value)
	{
		service.EndClientCall(value);
	}

	public void EndClientFileActionCall(FileActionResponse fileActionResponse)
	{
		service.EndClientFileActionCall(fileActionResponse);
	}

	public void EndClientFormRunModalCall(FormResult formResult)
	{
		service.EndClientFormRunModalCall(formResult);
	}

	public void EndClientAutomationMethodCall(AutomationMethodResponse automationMethodResponse)
	{
		service.EndClientAutomationMethodCall(automationMethodResponse);
	}

	public void CancelDialog()
	{
		service.CancelDialog();
	}

	public PageDependencyResult GetMetadataForPageAndAllItsDependencies(int pageId, DataSourceType dataSourceType, string personalizationId, bool applyPersonalization, ICollection<int> tablesToSkip, string viewName)
	{
		return service.GetMetadataForPageAndAllItsDependencies(pageId, dataSourceType, personalizationId, applyPersonalization, tablesToSkip, viewName);
	}

	public StreamTransfer Download()
	{
		return service.Download();
	}

	public bool DownloadControlAddIn(string assemblyFullName)
	{
		return service.DownloadControlAddIn(assemblyFullName);
	}

	public void Upload(StreamTransfer stream)
	{
		service.Upload(stream);
	}

	public UploadTempFileResponse UploadTempFile(StreamTransfer stream)
	{
		return service.UploadTempFile(stream);
	}

	public void HasExecutePermission(int objectId, ObjectType objectType)
	{
		service.HasExecutePermission(objectId, objectType);
	}

	public ICollection<AddInRegistration> GetAddInRegistrations()
	{
		return service.GetAddInRegistrations();
	}

	public DataSetResponse CustomizeChart(NavDataSet dataSet)
	{
		return service.CustomizeChart(dataSet);
	}

	public IAsyncResult BeginOpenConnection(ConnectionRequest connectionRequest, AsyncCallback callback, object state)
	{
		return Invoke(() => OpenConnection(connectionRequest), callback, state);
	}

	public UserSettings EndOpenConnection(IAsyncResult result)
	{
		return (UserSettings)EndInvoke(result);
	}

	public IAsyncResult BeginOpenCompany(string companyName, bool overridePersonalization, AsyncCallback callback, object state)
	{
		return Invoke(() => OpenCompany(companyName, overridePersonalization), callback, state);
	}

	public OpenCompanyResponse EndOpenCompany(IAsyncResult result)
	{
		return (OpenCompanyResponse)EndInvoke(result);
	}

	public IAsyncResult BeginGetAllowedCompanies(AsyncCallback callback, object state)
	{
		return Invoke(() => GetAllowedCompanies(), callback, state);
	}

	public DataSetResponse EndGetAllowedCompanies(IAsyncResult result)
	{
		return (DataSetResponse)EndInvoke(result);
	}

	public IAsyncResult BeginGetWorkDate(AsyncCallback callback, object state)
	{
		return Invoke(() => GetWorkDate(), callback, state);
	}

	public DateTime EndGetWorkDate(IAsyncResult result)
	{
		return (DateTime)EndInvoke(result);
	}

	public IAsyncResult BeginSetWorkDate(DateTime workDate, AsyncCallback callback, object state)
	{
		return Invoke(() => SetWorkDate(workDate), callback, state);
	}

	public DateTime EndSetWorkDate(IAsyncResult result)
	{
		return (DateTime)EndInvoke(result);
	}

	public IAsyncResult BeginOpenForm(OpenFormRequest form, AsyncCallback callback, object state)
	{
		return Invoke(() => OpenForm(form), callback, state);
	}

	public FormOpenResponse EndOpenForm(IAsyncResult result)
	{
		return (FormOpenResponse)EndInvoke(result);
	}

	public IAsyncResult BeginCloseForm(CloseFormRequest form, AsyncCallback callback, object state)
	{
		return Invoke(() => CloseForm(form), callback, state);
	}

	public FormOpenCloseResponse EndCloseForm(IAsyncResult result)
	{
		return (FormOpenCloseResponse)EndInvoke(result);
	}

	public IAsyncResult BeginValidateField(FieldRequest fieldRequest, NavRecordState dataSetState, AsyncCallback callback, object state)
	{
		return Invoke(() => ValidateField(fieldRequest, dataSetState), callback, state);
	}

	public CodeUnitResponse EndValidateField(IAsyncResult result)
	{
		return (CodeUnitResponse)EndInvoke(result);
	}

	public IAsyncResult BeginGetPage(PageRequestDefinition pageRequestDefinition, NavRecordState state, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => GetPage(pageRequestDefinition, state), callback, state);
	}

	public DataSetResponse EndGetPage(IAsyncResult result)
	{
		return (DataSetResponse)EndInvoke(result);
	}

	public IAsyncResult BeginInvokeCodeUnit(ApplicationMethodRequest applicationMethodRequest, NavRecordState state, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => InvokeCodeUnit(applicationMethodRequest, state), callback, state);
	}

	public CodeUnitResponse EndInvokeCodeUnit(IAsyncResult result)
	{
		return (CodeUnitResponse)EndInvoke(result);
	}

	public IAsyncResult BeginInvokeApplicationMethod(ApplicationMethodRequest applicationMethodRequest, NavRecordState state, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => InvokeApplicationMethod(applicationMethodRequest, state), callback, state);
	}

	public CodeUnitResponse EndInvokeApplicationMethod(IAsyncResult result)
	{
		return (CodeUnitResponse)EndInvoke(result);
	}

	public IAsyncResult BeginEvaluateFilter(string toEvaluate, ObjectType objectType, int objectId, int fieldId, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => EvaluateFilter(toEvaluate, objectType, objectId, fieldId), callback, asyncState);
	}

	public string EndEvaluateFilter(IAsyncResult result)
	{
		return (string)EndInvoke(result);
	}

	public IAsyncResult BeginEvaluateQueryFilterByPosition(string toEvaluate, int objectId, int columnPosition, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => EvaluateQueryFilterByPosition(toEvaluate, objectId, columnPosition), callback, asyncState);
	}

	public string EndEvaluateQueryFilterByPosition(IAsyncResult result)
	{
		return (string)EndInvoke(result);
	}

	public IAsyncResult BeginActionField(FieldRequest fieldRequest, NavRecordState dataSetState, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => ActionField(fieldRequest, dataSetState), callback, asyncState);
	}

	public CodeUnitResponse EndActionField(IAsyncResult result)
	{
		return (CodeUnitResponse)EndInvoke(result);
	}

	public IAsyncResult BeginInvokeEventTrigger(FieldRequest fieldRequest, NavRecordState dataSetState, string eventName, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => InvokeEventTrigger(fieldRequest, dataSetState, eventName), callback, asyncState);
	}

	public CodeUnitResponse EndInvokeEventTrigger(IAsyncResult result)
	{
		return (CodeUnitResponse)EndInvoke(result);
	}

	public IAsyncResult BeginGetNavigationFrame(bool applyPersonalization, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => GetNavigationFrame(applyPersonalization), callback, asyncState);
	}

	public MasterNavigation EndGetNavigationFrame(IAsyncResult result)
	{
		return (MasterNavigation)EndInvoke(result);
	}

	public IAsyncResult BeginGetProductOverview(ProductOverviewResourceType productOverviewResourceType, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => GetProductOverview(productOverviewResourceType), callback, asyncState);
	}

	public MasterNavigation EndGetProductOverview(IAsyncResult result)
	{
		return (MasterNavigation)EndInvoke(result);
	}

	public IAsyncResult BeginGetSystemPart(SystemPartTypes partType, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => GetSystemPart(partType), callback, asyncState);
	}

	public MasterPage EndGetSystemPart(IAsyncResult result)
	{
		return (MasterPage)EndInvoke(result);
	}

	public IAsyncResult BeginGetTableMetadata(int tableId, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => GetTableMetadata(tableId), callback, asyncState);
	}

	public MetaTable EndGetTableMetadata(IAsyncResult result)
	{
		return (MetaTable)EndInvoke(result);
	}

	public IAsyncResult BeginGetQueryMetadata(int queryId, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => GetQueryMetadata(queryId), callback, asyncState);
	}

	public MetaQuery EndGetQueryMetadata(IAsyncResult result)
	{
		return (MetaQuery)EndInvoke(result);
	}

	public IAsyncResult BeginGetQueryTableMetadata(int queryId, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => GetQueryTableMetadata(queryId), callback, asyncState);
	}

	public MetaTable EndGetQueryTableMetadata(IAsyncResult result)
	{
		return (MetaTable)EndInvoke(result);
	}

	public IAsyncResult BeginGetReportRDLMetadata(Guid reportHandle, ref bool allowCaching, ref bool enableExternalImages, ref bool enableHyperlinks, ref bool enableExternalAssemblies, ref bool enableAppDomainIsolation, AsyncCallback callback, object asyncState)
	{
		GetReportRDLMetadataExtraState extraState = new GetReportRDLMetadataExtraState(ref allowCaching, ref enableExternalImages, ref enableHyperlinks, ref enableExternalAssemblies, ref enableAppDomainIsolation);
		return Invoke(() => GetReportRDLMetadata(reportHandle, ref extraState.AllowCaching, ref extraState.EnableExternalImages, ref extraState.EnableHyperlinks, ref extraState.EnableExternalAssemblies, ref extraState.EnableAppDomainIsolation), callback, asyncState, extraState);
	}

	public byte[] EndGetReportRDLMetadata(ref bool allowCaching, ref bool enableExternalImages, ref bool enableHyperlinks, ref bool enableExternalAssemblies, ref bool enableAppDomainIsolation, IAsyncResult result)
	{
		GetReportRDLMetadataExtraState getReportRDLMetadataExtraState = (GetReportRDLMetadataExtraState)((AsyncResult)result).ExtraState;
		allowCaching = getReportRDLMetadataExtraState.AllowCaching;
		enableExternalImages = getReportRDLMetadataExtraState.EnableExternalImages;
		enableHyperlinks = getReportRDLMetadataExtraState.EnableHyperlinks;
		enableExternalAssemblies = getReportRDLMetadataExtraState.EnableExternalAssemblies;
		enableAppDomainIsolation = getReportRDLMetadataExtraState.EnableAppDomainIsolation;
		return (byte[])EndInvoke(result);
	}

	public IAsyncResult BeginGetMediaContent(Guid mediaHandle, ref bool allowCaching, ref string mimeType, AsyncCallback callback, object asyncState)
	{
		BeginGetMediaExtraState extraState = new BeginGetMediaExtraState(ref allowCaching, ref mimeType);
		return Invoke(() => GetMediaContent(mediaHandle, ref extraState.AllowCaching, ref extraState.MimeType), callback, asyncState, extraState);
	}

	public byte[] EndGetMediaContent(ref bool allowCaching, ref string mimeType, IAsyncResult result)
	{
		BeginGetMediaExtraState beginGetMediaExtraState = (BeginGetMediaExtraState)((AsyncResult)result).ExtraState;
		allowCaching = beginGetMediaExtraState.AllowCaching;
		mimeType = beginGetMediaExtraState.MimeType;
		return (byte[])EndInvoke(result);
	}

	public IAsyncResult BeginGetScaledMedia(Guid mediaHandle, int width, int height, ref bool allowCaching, ref string mimeType, AsyncCallback callback, object asyncState)
	{
		BeginGetMediaExtraState extraState = new BeginGetMediaExtraState(ref allowCaching, ref mimeType);
		return Invoke(() => GetScaledMedia(mediaHandle, width, height, ref extraState.AllowCaching, ref extraState.MimeType), callback, asyncState, extraState);
	}

	public byte[] EndGetScaledMedia(ref bool allowCaching, ref string mimeType, IAsyncResult result)
	{
		BeginGetMediaExtraState beginGetMediaExtraState = (BeginGetMediaExtraState)((AsyncResult)result).ExtraState;
		allowCaching = beginGetMediaExtraState.AllowCaching;
		mimeType = beginGetMediaExtraState.MimeType;
		return (byte[])EndInvoke(result);
	}

	public IAsyncResult BeginGetFilterPageForRecord(Guid formHandle, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => GetFilterPageForRecord(formHandle), callback, asyncState);
	}

	public MasterPage EndGetFilterPageForRecord(IAsyncResult result)
	{
		return (MasterPage)EndInvoke(result);
	}

	public IAsyncResult BeginGetRequestPageForReport(Guid formHandle, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => GetRequestPageForReport(formHandle), callback, asyncState);
	}

	public MasterPage EndGetRequestPageForReport(IAsyncResult result)
	{
		return (MasterPage)EndInvoke(result);
	}

	public IAsyncResult BeginGetRequestPageForXmlPort(Guid formHandle, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => GetRequestPageForXmlPort(formHandle), callback, asyncState);
	}

	public MasterPage EndGetRequestPageForXmlPort(IAsyncResult result)
	{
		return (MasterPage)EndInvoke(result);
	}

	public IAsyncResult BeginGetMasterPage(int pageId, DataSourceType dataSourceType, string personalizationId, bool applyPersonalization, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => GetMasterPage(pageId, dataSourceType, personalizationId, applyPersonalization), callback, asyncState);
	}

	public MasterPage EndGetMasterPage(IAsyncResult result)
	{
		return (MasterPage)EndInvoke(result);
	}

	public IAsyncResult BeginGetMasterPageWithoutConfiguration(int pageId, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => GetMasterPageWithoutConfiguration(pageId), callback, asyncState);
	}

	public MasterPage EndGetMasterPageWithoutConfiguration(IAsyncResult result)
	{
		return (MasterPage)EndInvoke(result);
	}

	public IAsyncResult BeginGetMasterPageTemplate(PageType pageType, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => GetMasterPageTemplate(pageType), callback, asyncState);
	}

	public MasterPage EndGetMasterPageTemplate(IAsyncResult result)
	{
		return (MasterPage)EndInvoke(result);
	}

	public IAsyncResult BeginGetTableMasterPage(int tableId, string personalizationId, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => GetTableMasterPage(tableId, personalizationId), callback, asyncState);
	}

	public MasterPage EndGetTableMasterPage(IAsyncResult result)
	{
		return (MasterPage)EndInvoke(result);
	}

	public IAsyncResult BeginGetHomepage(string personalizationId, bool applyPersonalization, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => GetHomepage(personalizationId, applyPersonalization), callback, asyncState);
	}

	public MasterPage EndGetHomepage(IAsyncResult result)
	{
		return (MasterPage)EndInvoke(result);
	}

	public IAsyncResult BeginLaunchDebugger(AsyncCallback callback, object asyncState)
	{
		return Invoke(() => LaunchDebugger(), callback, asyncState);
	}

	public LaunchDebuggerResponse EndLaunchDebugger(IAsyncResult result)
	{
		return (LaunchDebuggerResponse)EndInvoke(result);
	}

	public IAsyncResult BeginOpenLastErrorPage(AsyncCallback callback, object asyncState)
	{
		return Invoke(() =>
		{
			OpenLastErrorPage();
		}, callback, asyncState);
	}

	public void EndOpenLastErrorPage(IAsyncResult result)
	{
		EndInvoke(result);
	}

	public IAsyncResult BeginEnableVerboseTelemetry(bool enableFullALFunctionTracing, TimeSpan duration, AsyncCallback callback, object asyncState)
	{
		return Invoke(() =>
		{
			EnableVerboseTelemetry(enableFullALFunctionTracing, duration);
		}, callback, asyncState);
	}

	public void EndEnableVerboseTelemetry(IAsyncResult result)
	{
		EndInvoke(result);
	}

	public IAsyncResult BeginVerboseTraceLevelEnabled(AsyncCallback callback, object asyncState)
	{
		return Invoke(() => VerboseTraceLevelEnabled(), callback, asyncState);
	}

	public bool EndVerboseTraceLevelEnabled(IAsyncResult result)
	{
		return (bool)EndInvoke(result);
	}

	public IAsyncResult BeginFullALFunctionTracingEnabled(AsyncCallback callback, object asyncState)
	{
		return Invoke(() => FullALFunctionTracingEnabled(), callback, asyncState);
	}

	public bool EndFullALFunctionTracingEnabled(IAsyncResult result)
	{
		return (bool)EndInvoke(result);
	}

	public IAsyncResult BeginGetSupportInformation(AsyncCallback callback, object asyncState)
	{
		return Invoke(() => GetSupportInformation(), callback, asyncState);
	}

	public SupportInformationResponse EndGetSupportInformation(IAsyncResult result)
	{
		return (SupportInformationResponse)EndInvoke(result);
	}

	public IAsyncResult BeginOpenSettings(AsyncCallback callback, object asyncState)
	{
		return Invoke(() => OpenSettings(), callback, asyncState);
	}

	public ServerResponse EndOpenSettings(IAsyncResult result)
	{
		return (ServerResponse)EndInvoke(result);
	}

	public IAsyncResult BeginSaveConfiguration(MasterPage page, string personalizationId, AsyncCallback callback, object asyncState)
	{
		return Invoke(() =>
		{
			SaveConfiguration(page, personalizationId);
		}, callback, asyncState);
	}

	public void EndSaveConfiguration(IAsyncResult result)
	{
		EndInvoke(result);
	}

	public IAsyncResult BeginSavePageDataPersonalization(string personalizationId, int objectId, ObjectType objectType, string dataIdentifier, string value, AsyncCallback callback, object asyncState)
	{
		return Invoke(() =>
		{
			SavePageDataPersonalization(personalizationId, objectId, objectType, dataIdentifier, value);
		}, callback, asyncState);
	}

	public void EndSavePageDataPersonalization(IAsyncResult result)
	{
		EndInvoke(result);
	}

	public IAsyncResult BeginRunQuery(ApplicationMethodRequest applicationMethodRequest, AsyncCallback callback, object asyncState)
	{
		return Invoke(() =>
		{
			RunQuery(applicationMethodRequest);
		}, callback, asyncState);
	}

	public void EndRunQuery(IAsyncResult result)
	{
		EndInvoke(result);
	}

	public IAsyncResult BeginRunQueryOnForm(ApplicationMethodRequest applicationMethodRequest, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => RunQueryOnForm(applicationMethodRequest), callback, asyncState);
	}

	public ServerResponse EndRunQueryOnForm(IAsyncResult result)
	{
		return (ServerResponse)EndInvoke(result);
	}

	public IAsyncResult BeginRunQueryDataSet(ApplicationMethodRequest applicationMethodRequest, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => RunQueryDataSet(applicationMethodRequest), callback, asyncState);
	}

	public NavDataSet EndRunQueryDataSet(IAsyncResult result)
	{
		return (NavDataSet)EndInvoke(result);
	}

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	public IAsyncResult BeginSendDataToDocument(NavRecordState recordState, OfficeDocumentType documentType, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => SendDataToDocument(recordState, documentType), callback, asyncState);
	}

	public SendDataToDocumentResponse EndSendDataToDocument(IAsyncResult result)
	{
		return (SendDataToDocumentResponse)EndInvoke(result);
	}

	[OperationContract(IsInitiating = false, AsyncPattern = true)]
	public IAsyncResult BeginGetServicesUrl(NavClientType clientType, NavObjectType objectType, int objectId, NavRecordState recordState, bool useFilter, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => GetServicesUrl(clientType, objectType, objectId, recordState, useFilter), callback, asyncState);
	}

	public ServicesUrlResponse EndGetServicesUrl(IAsyncResult result)
	{
		return (ServicesUrlResponse)EndInvoke(result);
	}

	public IAsyncResult BeginRunReport(ApplicationMethodRequest applicationMethodRequest, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => RunReport(applicationMethodRequest), callback, asyncState);
	}

	public RunReportResponse EndRunReport(IAsyncResult result)
	{
		return (RunReportResponse)EndInvoke(result);
	}

	public IAsyncResult BeginLookupField(FieldRequest fieldRequest, NavRecordState dataSetState, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => LookupField(fieldRequest, dataSetState), callback, asyncState);
	}

	public CodeUnitResponse EndLookupField(IAsyncResult result)
	{
		return (CodeUnitResponse)EndInvoke(result);
	}

	public IAsyncResult BeginDrillDownField(FieldRequest fieldRequest, NavRecordState dataSetState, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => DrillDownField(fieldRequest, dataSetState), callback, asyncState);
	}

	public CodeUnitResponse EndDrillDownField(IAsyncResult result)
	{
		return (CodeUnitResponse)EndInvoke(result);
	}

	public IAsyncResult BeginAssistEdit(FieldRequest fieldRequest, NavRecordState dataSetState, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => AssistEdit(fieldRequest, dataSetState), callback, asyncState);
	}

	public CodeUnitResponse EndAssistEdit(IAsyncResult result)
	{
		return (CodeUnitResponse)EndInvoke(result);
	}

	public IAsyncResult BeginEvaluate(string toEvaluate, NavType navType, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => Evaluate(toEvaluate, navType), callback, asyncState);
	}

	public EvaluateBag EndEvaluate(IAsyncResult result)
	{
		return (EvaluateBag)EndInvoke(result);
	}

	public IAsyncResult BeginRunXmlPort(ApplicationMethodRequest applicationMethodRequest, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => RunXmlPort(applicationMethodRequest), callback, asyncState);
	}

	public ServerResponse EndRunXmlPort(IAsyncResult result)
	{
		return (ServerResponse)EndInvoke(result);
	}

	public IAsyncResult BeginRunTable(RunTableRequest runTableRequest, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => RunTable(runTableRequest), callback, asyncState);
	}

	public ServerResponse EndRunTable(IAsyncResult result)
	{
		return (ServerResponse)EndInvoke(result);
	}

	public IAsyncResult BeginEvaluateRelation(FieldRequest fieldRequest, NavRecordState state, AsyncCallback callback, object asyncState)
	{
		return new CompletedAsyncResult(EvaluateRelation(fieldRequest, state), asyncState);
	}

	public EvaluateRelationResponse EndEvaluateRelation(IAsyncResult result)
	{
		return (EvaluateRelationResponse)EndInvoke(result);
	}

	public IAsyncResult BeginSendClientException(ServerFault fault, string faultReason, AsyncCallback callback, object asyncState)
	{
		return Invoke(() =>
		{
			SendClientException(fault, faultReason);
		}, callback, asyncState);
	}

	public void EndSendClientException(IAsyncResult result)
	{
		EndInvoke(result);
	}

	public IAsyncResult BeginEndClientCall(object value, AsyncCallback callback, object asyncState)
	{
		return Invoke(() =>
		{
			EndClientCall(value);
		}, callback, asyncState);
	}

	public void EndEndClientCall(IAsyncResult result)
	{
		EndInvoke(result);
	}

	public IAsyncResult BeginEndClientFileActionCall(FileActionResponse fileActionResponse, AsyncCallback callback, object asyncState)
	{
		return Invoke(() =>
		{
			EndClientFileActionCall(fileActionResponse);
		}, callback, asyncState);
	}

	public void EndEndClientFileActionCall(IAsyncResult result)
	{
		EndInvoke(result);
	}

	public IAsyncResult BeginEndClientFormRunModalCall(FormResult formResult, AsyncCallback callback, object asyncState)
	{
		return Invoke(() =>
		{
			EndClientFormRunModalCall(formResult);
		}, callback, asyncState);
	}

	public void EndEndClientFormRunModalCall(IAsyncResult result)
	{
		EndInvoke(result);
	}

	public IAsyncResult BeginEndClientAutomationMethodCall(AutomationMethodResponse automationMethodResponse, AsyncCallback callback, object asyncState)
	{
		return Invoke(() =>
		{
			EndClientAutomationMethodCall(automationMethodResponse);
		}, callback, asyncState);
	}

	public void EndEndClientAutomationMethodCall(IAsyncResult result)
	{
		EndInvoke(result);
	}

	public IAsyncResult BeginCancelDialog(AsyncCallback callback, object asyncState)
	{
		return Invoke(() =>
		{
			CancelDialog();
		}, callback, asyncState);
	}

	public void EndCancelDialog(IAsyncResult result)
	{
		EndInvoke(result);
	}

	public IAsyncResult BeginSetClientSettings(SessionSettings settings, bool saveOnly, AsyncCallback callback, object asyncState)
	{
		return Invoke(() =>
		{
			SetClientSettings(settings, saveOnly);
		}, callback, asyncState);
	}

	public void EndSetClientSettings(IAsyncResult result)
	{
		EndInvoke(result);
	}

	public IAsyncResult BeginSystemInfo(AsyncCallback callback, object asyncState)
	{
		return Invoke(() => SystemInfo(), callback, asyncState);
	}

	public NSSystemInfoData EndSystemInfo(IAsyncResult result)
	{
		return (NSSystemInfoData)EndInvoke(result);
	}

	public IAsyncResult BeginGetAddInRegistrations(AsyncCallback callback, object asyncState)
	{
		return Invoke(() => GetAddInRegistrations(), callback, asyncState);
	}

	public ICollection<AddInRegistration> EndGetAddInRegistrations(IAsyncResult result)
	{
		return (ICollection<AddInRegistration>)EndInvoke(result);
	}

	public IAsyncResult BeginDeleteRecord(NavRecordState dataSetState, RecordRequest recordRequest, bool deleteSelectedRecords, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => DeleteRecord(dataSetState, recordRequest, deleteSelectedRecords), callback, asyncState);
	}

	public RecordDeletedResponse EndDeleteRecord(IAsyncResult result)
	{
		return (RecordDeletedResponse)EndInvoke(result);
	}

	public IAsyncResult BeginRenameRecord(NavRecordState dataSetState, RecordRequest recordRequest, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => RenameRecord(dataSetState, recordRequest), callback, asyncState);
	}

	public DataSetResponse EndRenameRecord(IAsyncResult result)
	{
		return (DataSetResponse)EndInvoke(result);
	}

	public IAsyncResult BeginModifyRecord(NavRecordState dataSetState, RecordRequest recordRequest, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => ModifyRecord(dataSetState, recordRequest), callback, asyncState);
	}

	public DataSetResponse EndModifyRecord(IAsyncResult result)
	{
		return (DataSetResponse)EndInvoke(result);
	}

	public IAsyncResult BeginInsertRecord(NavRecordState dataSetState, RecordRequest recordRequest, bool belowXRecord, bool isEmptyMultipleLine, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => InsertRecord(dataSetState, recordRequest, belowXRecord, isEmptyMultipleLine), callback, asyncState);
	}

	public DataSetResponse EndInsertRecord(IAsyncResult result)
	{
		return (DataSetResponse)EndInvoke(result);
	}

	public IAsyncResult BeginGetAggregates(NavRecordState dataSetState, ref NavDataSet dataSet, AsyncCallback callback, object asyncState)
	{
		GetAggregatesExtraState extraState = new GetAggregatesExtraState(ref dataSet);
		return Invoke(() => GetAggregates(dataSetState, ref extraState.DataSet), callback, asyncState, extraState);
	}

	public bool EndGetAggregates(ref NavDataSet dataSet, IAsyncResult result)
	{
		GetAggregatesExtraState getAggregatesExtraState = (GetAggregatesExtraState)((AsyncResult)result).ExtraState;
		dataSet = getAggregatesExtraState.DataSet;
		return (bool)EndInvoke(result);
	}

	public IAsyncResult BeginRecordCountApprox(int tableId, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => RecordCountApprox(tableId), callback, asyncState);
	}

	public int EndRecordCountApprox(IAsyncResult result)
	{
		return (int)EndInvoke(result);
	}

	public IAsyncResult BeginGetMetadataForPageAndAllItsDependencies(int pageId, DataSourceType dataSourceType, string personalizationId, bool applyPersonalization, ICollection<int> tablesToSkip, string viewName, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => GetMetadataForPageAndAllItsDependencies(pageId, dataSourceType, personalizationId, applyPersonalization, tablesToSkip, viewName), callback, asyncState);
	}

	public PageDependencyResult EndGetMetadataForPageAndAllItsDependencies(IAsyncResult result)
	{
		return (PageDependencyResult)EndInvoke(result);
	}

	public IAsyncResult BeginDownload(AsyncCallback callback, object asyncState)
	{
		return Invoke(() => Download(), callback, asyncState);
	}

	public StreamTransfer EndDownload(IAsyncResult result)
	{
		return (StreamTransfer)EndInvoke(result);
	}

	public IAsyncResult BeginDownloadControlAddIn(string assemblyFullName, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => DownloadControlAddIn(assemblyFullName), callback, asyncState);
	}

	public bool EndDownloadControlAddIn(IAsyncResult result)
	{
		return (bool)EndInvoke(result);
	}

	public IAsyncResult BeginUploadTempFile(StreamTransfer stream, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => UploadTempFile(stream), callback, asyncState);
	}

	public UploadTempFileResponse EndUploadTempFile(IAsyncResult result)
	{
		return (UploadTempFileResponse)EndInvoke(result);
	}

	public IAsyncResult BeginUpload(StreamTransfer stream, AsyncCallback callback, object asyncState)
	{
		return Invoke(() =>
		{
			Upload(stream);
		}, callback, asyncState);
	}

	public void EndUpload(IAsyncResult result)
	{
		EndInvoke(result);
	}

	public IAsyncResult BeginHasExecutePermission(int objectId, ObjectType objectType, AsyncCallback callback, object asyncState)
	{
		return Invoke(() =>
		{
			HasExecutePermission(objectId, objectType);
		}, callback, asyncState);
	}

	public void EndHasExecutePermission(IAsyncResult result)
	{
		EndInvoke(result);
	}

	public IAsyncResult BeginCustomizeChart(NavDataSet dataSet, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => CustomizeChart(dataSet), callback, asyncState);
	}

	public DataSetResponse EndCustomizeChart(IAsyncResult result)
	{
		return (DataSetResponse)EndInvoke(result);
	}

	public IAsyncResult BeginCancelRequest(AsyncCallback callback, object asyncState)
	{
		return Invoke(() =>
		{
			CancelRequest();
		}, callback, asyncState);
	}

	public void CancelRequest()
	{
		service.CancelRequest();
	}

	public void EndCancelRequest(IAsyncResult result)
	{
		EndInvoke(result);
	}

	public IAsyncResult BeginAssociateExchangeToken(string token, AsyncCallback callback, object asyncState)
	{
		return Invoke(() =>
		{
			AssociateExchangeToken(token);
		}, callback, asyncState);
	}

	public void AssociateExchangeToken(string token)
	{
		service.AssociateExchangeToken(token);
	}

	public void EndAssociateExchangeToken(IAsyncResult result)
	{
		EndInvoke(result);
	}

	public IAsyncResult BeginInvokeDesignerAction(DesignerRequest designerRequest, AsyncCallback callback, object asyncState)
	{
		return Invoke(() => InvokeDesignerAction(designerRequest), callback, asyncState);
	}

	public InvokeDesignerActionResponse InvokeDesignerAction(DesignerRequest designerRequest)
	{
		return service.InvokeDesignerAction(designerRequest);
	}

	public InvokeDesignerActionResponse EndInvokeDesignerAction(IAsyncResult result)
	{
		return (InvokeDesignerActionResponse)EndInvoke(result);
	}

	private IAsyncResult Invoke(Action action, AsyncCallback callback, object asyncState, object extraState = null)
	{
		if (makeAsyncCalls)
		{
			return new AsyncResult(action.BeginInvoke(callback, asyncState), action, extraState);
		}
		action();
		return new CompletedAsyncResult(null, asyncState);
	}

	private IAsyncResult Invoke(Func<object> action, AsyncCallback callback, object asyncState, object extraState = null)
	{
		if (makeAsyncCalls)
		{
			return new AsyncResult(action.BeginInvoke(callback, asyncState), action, extraState);
		}
		return new CompletedAsyncResult(action(), asyncState);
	}

	private static object EndInvoke(IAsyncResult asyncResult)
	{
		if (asyncResult is CompletedAsyncResult completedAsyncResult)
		{
			return completedAsyncResult.Result;
		}
		AsyncResult asyncResult2 = (AsyncResult)asyncResult;
		if (asyncResult2.Func != null)
		{
			return asyncResult2.Func.EndInvoke(asyncResult2.InnerAsyncResult);
		}
		asyncResult2.Action.EndInvoke(asyncResult2.InnerAsyncResult);
		return null;
	}

	public void Abort()
	{
		if (wrappedContextChannel != null)
		{
			wrappedContextChannel.Abort();
			return;
		}
		throw new NotSupportedException();
	}

	public void Close()
	{
		if (wrappedContextChannel != null)
		{
			wrappedContextChannel.Close();
			return;
		}
		throw new NotSupportedException();
	}

	public void Close(TimeSpan timeout)
	{
		if (wrappedContextChannel != null)
		{
			wrappedContextChannel.Close(timeout);
			return;
		}
		throw new NotSupportedException();
	}

	public IAsyncResult BeginClose(AsyncCallback callback, object state)
	{
		if (wrappedContextChannel != null)
		{
			return wrappedContextChannel.BeginClose(callback, state);
		}
		throw new NotSupportedException();
	}

	public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
	{
		if (wrappedContextChannel != null)
		{
			return wrappedContextChannel.BeginClose(timeout, callback, state);
		}
		throw new NotSupportedException();
	}

	public void EndClose(IAsyncResult result)
	{
		if (wrappedContextChannel != null)
		{
			wrappedContextChannel.EndClose(result);
			return;
		}
		throw new NotSupportedException();
	}

	public void Open()
	{
		if (wrappedContextChannel != null)
		{
			wrappedContextChannel.Open();
			return;
		}
		throw new NotSupportedException();
	}

	public void Open(TimeSpan timeout)
	{
		if (wrappedContextChannel != null)
		{
			wrappedContextChannel.Open(timeout);
			return;
		}
		throw new NotSupportedException();
	}

	public IAsyncResult BeginOpen(AsyncCallback callback, object state)
	{
		if (wrappedContextChannel != null)
		{
			return wrappedContextChannel.BeginOpen(callback, state);
		}
		throw new NotSupportedException();
	}

	public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
	{
		if (wrappedContextChannel != null)
		{
			return wrappedContextChannel.BeginOpen(timeout, callback, state);
		}
		throw new NotSupportedException();
	}

	public void EndOpen(IAsyncResult result)
	{
		if (wrappedContextChannel != null)
		{
			wrappedContextChannel.EndOpen(result);
			return;
		}
		throw new NotSupportedException();
	}

	public T GetProperty<T>() where T : class
	{
		if (wrappedContextChannel != null)
		{
			return wrappedContextChannel.GetProperty<T>();
		}
		throw new NotSupportedException();
	}
}
