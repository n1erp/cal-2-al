using System;
using System.IO;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface IClientCallback
{
	bool IsCallbackAllowed { get; }

	void DialogMessage(string message, Guid automationId);

	void DialogHyperlink(string hyperlink, Guid automationId);

	bool DialogConfirm(string message, bool defaultValue, Guid automationId);

	void ProcessServerRequests();

	void DialogOpen(Guid handle, Guid automationId, string format, object[] parameters);

	void DialogUpdate(Guid dialogHandle, object[] parameters);

	void DialogClose(Guid dialogHandle);

	int DialogSelectionMenu(string[] options, int defaultSelection, string instruction, Guid automationId);

	bool DownloadFileAction(Stream stream, bool displayDialog, string title, string initialFolder, string typeFilter, ref string fileName, Guid automationId);

	FileBufferedStream UploadFileAction(bool displayDialog, string title, string initialFolder, string typeFilter, ref string fileName, Guid automationId);

	bool ExportDataAction(Stream stream, ref string fileName, string dialogTitle, bool showDialog);

	bool ImportDataAction(ref string fileName, string dialogTitle, bool showDialog);

	FormResult FormRunModal(NavForm form, NavFormRuntimeParameters parameters);

	void FormRun(NavForm form, NavFormRuntimeParameters parameters);

	void FormClose(NavForm form);

	void FormActivate(NavForm form, bool refresh);

	bool DataSetPageReady(DataSetRequest request);

	NavAutomationHandle CreateAutomationObject(Guid classId, string licenseKey, bool newServer);

	NavAutomationHandle CreateDotNetHandle(string assemblyFullName, string typeName, Guid formHandle, string varName, bool createInstance, params object[] arguments);

	NavAutomationHandle CreateDotNetTypeHandle(string assemblyFullName, string typeName);

	bool CanLoadDotNetType(string assemblyFullName, string typeName);

	NavAutomationHandle GetDotNetObject(Guid formHandle, int controlId);

	UserNamePasswordCredentials RequestCredentials(UserNamePasswordRequestOptions requestOptions);

	void DisposeAutomationObject(int handle, bool suppressDispose);

	object InvokeAutomationMethod(InvokeAutomationMethodRequest<object> request);

	NavAutomationHandle AssignDotNet(string assemblyFullName, string typeName, int oldTypeHandle, int otherHandle);

	NavAutomationHandle CreateDotNetEnumerationHandle(string assemblyFullName, string typeName, string fieldName);

	void ClearClientMetadataCache();

	void SendNotification(NotificationInfo notification);

	void SendSessionUpdateRequest(SessionSettingsInfo sessionSettingsInfo);

	void CompanyInformationChanged(CompanyInformationChanges companyInformationChanges);

	void WorkDateChanged(DateTime workDate);

	void VerifyCallbackAllowed(NavApplicationObjectBase applicationObject);

	Printer[] GetPrinters();
}
