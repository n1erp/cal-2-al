using System;
using System.ServiceModel;
using Microsoft.Dynamics.Nav.Types.Channels;

namespace Microsoft.Dynamics.Nav.Types;

[ServiceContract(SessionMode = SessionMode.Required, Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public interface IClientContract
{
	[OperationContract(IsOneWay = true)]
	void Confirm(string confirmMessage, bool defaultValue, Guid automationId, ServerResponse response);

	[OperationContract(IsOneWay = true)]
	void ProcessServerRequests(ServerResponse response);

	[OperationContract(IsOneWay = true)]
	void SelectionMenu(string[] options, int defaultSelection, string instruction, Guid automationId, ServerResponse response);

	[OperationContract(IsOneWay = true)]
	void FormRunModal(FormRunModalRequest formRequest);

	[OperationContract(IsOneWay = true)]
	void FileActionDialog(FileActionRequest fileActionRequest);

	[OperationContract]
	void OpenDialog(DialogOpenRequest dialogOpenRequest, string format, object[] parameters);

	[OperationContract]
	void UpdateDialog(DialogRequest dialogRequest, object[] parameters);

	[OperationContract]
	void CloseDialog(DialogRequest dialogRequest);

	[OperationContract(IsOneWay = true)]
	void FormClose(FormCloseRequest formRequest);

	[OperationContract(IsOneWay = true)]
	void FormActivate(FormActivateRequest formActivateRequest);

	[OperationContract(IsOneWay = true)]
	void OnSessionTerminating();

	[OperationContract(IsOneWay = true)]
	void CreateAutomationObject(Guid classId, string licenseKey, bool newServer);

	[OperationContract(IsOneWay = true)]
	void CreateDotNetHandle(string assemblyFullName, string typeName, Guid formHandle, string varName, bool createInstance, params NavAutomationArgument[] arguments);

	[OperationContract(IsOneWay = true)]
	void CreateDotNetTypeHandle(string assemblyFullName, string typeName);

	[OperationContract(IsOneWay = true)]
	void CanLoadDotNetType(string assemblyFullName, string typeName);

	[OperationContract(IsOneWay = true)]
	void GetDotNetObject(Guid formHandle, int controlId);

	[OperationContract(IsOneWay = true)]
	void DisposeAutomationObject(int handle, bool suppressDispose);

	[OperationContract(IsOneWay = true)]
	void AssignDotNet(string assemblyFullName, string typeName, int oldTypeHandle, int otherHandle);

	[OperationContract(IsOneWay = true)]
	void ClearClientMetadataCache();

	[OperationContract(IsOneWay = true)]
	void CreateDotNetEnumerationHandle(string assemblyFullName, string typeName, string fieldName);

	[OperationContract(IsOneWay = true)]
	void InvokeAutomationMethod(InvokeAutomationMethodRequest<NavAutomationArgument> request);

	[OperationContract(IsOneWay = true)]
	[ChunkingBehavior(ChunkInput = true)]
	void DataSetPageReady(DataSetRequest request);

	[OperationContract]
	void RequestCredentials(UserNamePasswordRequestOptions requestOptions);

	[OperationContract(IsOneWay = true)]
	void GetPrinters();
}
