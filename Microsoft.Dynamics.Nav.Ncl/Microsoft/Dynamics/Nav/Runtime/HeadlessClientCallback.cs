using System;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class HeadlessClientCallback : IClientCallback
{
	private readonly string serviceName;

	private readonly NavSession session;

	protected NavSession Session => session;

	public virtual bool IsCallbackAllowed => false;

	public HeadlessClientCallback(string serviceName, NavSession session)
	{
		this.serviceName = serviceName;
		this.session = session;
	}

	public virtual void VerifyCallbackAllowed(NavApplicationObjectBase applicationObject)
	{
		throw new NavNCLCallbackNotAllowedException(string.Format(CultureInfo.InvariantCulture, "{0} attempted to issue a client callback ({1} - {2}({3})). Client callbacks are not supported on {0}.", serviceName, applicationObject.ObjectId.ObjectType, applicationObject.ObjectName, applicationObject.ObjectId.ObjectNumber));
	}

	public virtual void DialogMessage(string message, Guid automationId)
	{
		DialogMessageRequest item = new DialogMessageRequest(message)
		{
			AutomationId = automationId
		};
		NavCurrentThread.Session.RequestsToClient.Enqueue(item);
	}

	public virtual void DialogHyperlink(string hyperlink, Guid automationId)
	{
	}

	public virtual bool DialogConfirm(string message, bool defaultValue, Guid automationId)
	{
		throw GenerateCallbackException(message, "{0} attempted to issue a client callback to show a confirmation dialog box: {1} ({2} {4} {3}). Client callbacks are not supported on {0}.");
	}

	public virtual void ProcessServerRequests()
	{
	}

	public virtual void DialogOpen(Guid handle, Guid automationId, string format, object[] parameters)
	{
	}

	public virtual void DialogUpdate(Guid dialogHandle, object[] parameters)
	{
	}

	public virtual void DialogClose(Guid dialogHandle)
	{
	}

	public virtual int DialogSelectionMenu(string[] options, int defaultSelection, string instruction, Guid automationId)
	{
		throw GenerateCallbackException(instruction, "{0} attempted to issue a client callback to show a selection menu: {1} ({2} {4} {3}). Client callbacks are not supported on {0}.");
	}

	public virtual bool DownloadFileAction(Stream stream, bool displayDialog, string title, string initialFolder, string typeFilter, ref string fileName, Guid automationId)
	{
		throw GenerateCallbackException(fileName, "{0} attempted to issue a client callback to download a file: {1} ({2} {4} {3}). Client callbacks are not supported on {0}.");
	}

	public virtual FileBufferedStream UploadFileAction(bool displayDialog, string title, string initialFolder, string typeFilter, ref string fileName, Guid automationId)
	{
		throw GenerateCallbackException(fileName, "{0} attempted to issue a client callback to upload a file: {1} ({2} {4} {3}). Client callbacks are not supported on {0}.");
	}

	public virtual bool ExportDataAction(Stream stream, ref string fileName, string dialogTitle, bool showDialog)
	{
		throw GenerateCallbackException(fileName, "{0} attempted to issue a client callback to download a file: {1} ({2} {4} {3}). Client callbacks are not supported on {0}.");
	}

	public virtual bool ImportDataAction(ref string fileName, string dialogTitle, bool showDialog)
	{
		throw GenerateCallbackException(fileName, "{0} attempted to issue a client callback to upload a file: {1} ({2} {4} {3}). Client callbacks are not supported on {0}.");
	}

	public virtual FormResult FormRunModal(NavForm form, NavFormRuntimeParameters parameters)
	{
		throw GenerateFormOperationException(form, "{0} attempted to issue a client callback to run page {1} {2} as modal. Client callbacks are not supported on {0}.");
	}

	public virtual void FormRun(NavForm form, NavFormRuntimeParameters parameters)
	{
		throw GenerateFormOperationException(form, "{0} attempted to issue a client callback to run page {1} {2}. Client callbacks are not supported on {0}.");
	}

	public virtual void FormClose(NavForm form)
	{
		throw GenerateFormOperationException(form, "{0} attempted to issue a client callback to close page {1} {2}. Client callbacks are not supported on {0}.");
	}

	public virtual void FormActivate(NavForm form, bool refresh)
	{
		throw GenerateFormOperationException(form, "{0} attempted to issue a client callback to activate page {1} {2}. Client callbacks are not supported on {0}.");
	}

	public virtual bool DataSetPageReady(DataSetRequest request)
	{
		throw GenerateCallbackException("{0} attempted to issue a client callback to run: {1} {3} {2}. Client callbacks are not supported on {0}.");
	}

	public virtual NavAutomationHandle CreateAutomationObject(Guid classId, string licenseKey, bool newServer)
	{
		throw GenerateCallbackException(classId.ToString(), "{0} attempted to issue a client callback to create an Automation object: {1} ({2} {4} {3}). Client callbacks are not supported on {0}.");
	}

	public virtual NavAutomationHandle CreateDotNetHandle(string assemblyFullName, string typeName, Guid formHandle, string varName, bool createInstance, params object[] arguments)
	{
		throw GenerateCallbackException(typeName, "{0} attempted to issue a client callback to create a DotNet object: {1} ({2} {4} {3}). Client callbacks are not supported on {0}.");
	}

	public virtual NavAutomationHandle CreateDotNetTypeHandle(string assemblyFullName, string typeName)
	{
		throw GenerateCallbackException(typeName, "{0} attempted to issue a client callback to create a DotNet object: {1} ({2} {4} {3}). Client callbacks are not supported on {0}.");
	}

	public virtual bool CanLoadDotNetType(string assemblyFullName, string typeName)
	{
		return false;
	}

	public virtual NavAutomationHandle GetDotNetObject(Guid formHandle, int controlId)
	{
		throw GenerateCallbackException("{0} attempted to issue a client callback to get a DotNet object {1} {3} {2}. Client callbacks are not supported on {0}.");
	}

	public UserNamePasswordCredentials RequestCredentials(UserNamePasswordRequestOptions requestOptions)
	{
		throw new NotImplementedException();
	}

	public virtual void DisposeAutomationObject(int handle, bool suppressDispose)
	{
		throw GenerateCallbackException("{0} attempted to issue a client callback to dispose of an Automation object: {1} {3} {2}. Client callbacks are not supported on {0}.");
	}

	public virtual object InvokeAutomationMethod(InvokeAutomationMethodRequest<object> request)
	{
		throw GenerateCallbackException("{0} attempted to issue a client callback on an Automation object to invoke method: {1} {3} {2}. Client callbacks are not supported on {0}.");
	}

	public virtual NavAutomationHandle AssignDotNet(string assemblyFullName, string typeName, int oldTypeHandle, int otherHandle)
	{
		throw GenerateCallbackException(typeName, "{0} attempted to issue a client callback to assign a DotNet object: {1} ({2} {4} {3}). Client callbacks are not supported on {0}.");
	}

	public virtual void ClearClientMetadataCache()
	{
	}

	public virtual NavAutomationHandle CreateDotNetEnumerationHandle(string assemblyFullName, string typeName, string fieldName)
	{
		throw GenerateCallbackException(typeName, "{0} attempted to issue a client callback to create a DotNet object: {1} ({2} {4} {3}). Client callbacks are not supported on {0}.");
	}

	public virtual void SendNotification(NotificationInfo notification)
	{
	}

	public virtual void SendSessionUpdateRequest(SessionSettingsInfo sessionSettingsInfo)
	{
	}

	public void CompanyInformationChanged(CompanyInformationChanges companyInformationChanges)
	{
	}

	public void WorkDateChanged(DateTime workDate)
	{
	}

	private NavNCLCallbackNotAllowedException GenerateFormOperationException(NavForm form, string formatString)
	{
		return new NavNCLCallbackNotAllowedException(string.Format(CultureInfo.CurrentCulture, formatString, serviceName, form.ObjectId.ObjectNumber, form.ObjectName));
	}

	private NavNCLCallbackNotAllowedException GenerateCallbackException(string formatString)
	{
		NavApplicationObjectBase applicationObject = session.CurrentMethodScope.ApplicationObject;
		if (applicationObject == null)
		{
			return new NavNCLCallbackNotAllowedException(string.Format(CultureInfo.CurrentCulture, formatString, serviceName, string.Empty, string.Empty, string.Empty));
		}
		return new NavNCLCallbackNotAllowedException(string.Format(CultureInfo.CurrentCulture, formatString, serviceName, applicationObject.ObjectId.ObjectType, applicationObject.ObjectName, applicationObject.ObjectId.ObjectNumber));
	}

	private NavNCLCallbackNotAllowedException GenerateCallbackException(string contextInformation, string formatString)
	{
		NavApplicationObjectBase applicationObject = session.CurrentMethodScope.ApplicationObject;
		if (applicationObject == null)
		{
			return new NavNCLCallbackNotAllowedException(string.Format(CultureInfo.CurrentCulture, formatString, serviceName, contextInformation, string.Empty, string.Empty, string.Empty));
		}
		return new NavNCLCallbackNotAllowedException(string.Format(CultureInfo.CurrentCulture, formatString, serviceName, contextInformation, applicationObject.ObjectId.ObjectType, applicationObject.ObjectName, applicationObject.ObjectId.ObjectNumber));
	}

	public virtual Printer[] GetPrinters()
	{
		return PrinterHelper.GetPrinters();
	}
}
