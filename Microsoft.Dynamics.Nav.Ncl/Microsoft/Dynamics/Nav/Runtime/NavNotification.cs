using System;
using System.Globalization;
using System.Reflection;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Extensions;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavNotification : NavValue
{
	private const int MaxGlobalNotificationActionCount = 2;

	private const int MaxLocalNotificationActionCount = 3;

	public string ALMessage
	{
		get
		{
			return NotificationInfo.Message;
		}
		set
		{
			NotificationInfo.Message = value;
		}
	}

	public NotificationScope ALScope
	{
		get
		{
			return NotificationInfo.Scope;
		}
		set
		{
			NotificationInfo.Scope = value;
			VerifyActionCountIsNotExeeded();
		}
	}

	public Guid ALId
	{
		get
		{
			return NotificationInfo.Id;
		}
		set
		{
			NotificationInfo.Id = value;
		}
	}

	internal NotificationInfo NotificationInfo { get; private set; } = new NotificationInfo();

	public override NavNclType NclType => NavNclType.NavNotification;

	public override int GetBytesSize => 0;

	public static NavNotification Default => new NavNotification();

	public override bool IsZeroOrEmpty => false;

	public void ALAddAction(string actionCaption, int codeunitId, string functionName)
	{
		VerifyCanAddAction();
		VerifyNotificationActionHandler(codeunitId, functionName);
		NotificationInfo.AddAction(actionCaption, codeunitId, functionName);
	}

	public void ALSetData(string keyName, string keyValue)
	{
		NotificationInfo.Data[keyName ?? string.Empty] = keyValue;
	}

	public NavText ALGetData(string keyName)
	{
		return new NavText(int.MaxValue, NotificationInfo.Data.ContainsKey(keyName ?? string.Empty) ? NotificationInfo.Data[keyName ?? string.Empty] : string.Empty);
	}

	public bool ALHasData(string keyName)
	{
		return NotificationInfo.Data.ContainsKey(keyName);
	}

	public bool ALSend(DataError errorLevel)
	{
		NavSession session = NavCurrentThread.Session;
		session.Diagnostics.SendTraceTag("0000092", Category.Runtime, Verbosity.Verbose, "Sending notification. Message: {0}, Scope: {1}, Actions: {2}", DiagnosticParameter.CustomerContent(ALMessage), DiagnosticParameter.SystemMetadata(ALScope), DiagnosticParameter.SystemMetadata(NotificationInfo.Actions?.Count ?? 0));
		try
		{
			VerifyNotificationBeforeSend();
			if (NotificationInfo.Id == Guid.Empty)
			{
				NotificationInfo.Id = Guid.NewGuid();
			}
			if (session.TestExecution.RedirectNotificationOperationToTestHandler(this, NavHandlerType.SendNotification))
			{
				return true;
			}
			if (ALScope == NotificationScope.LocalScope)
			{
				SendLocalNotification();
				return true;
			}
			SendGlobalNotification();
		}
		catch (NavBaseException exception)
		{
			session.Diagnostics.SendExceptionTag("000009L", Category.Runtime, exception);
			if (errorLevel == DataError.TrapError)
			{
				return false;
			}
			throw;
		}
		return true;
	}

	public bool ALRecall(DataError errorLevel)
	{
		NavSession session = NavCurrentThread.Session;
		session.Diagnostics.SendTraceTag("00000B5", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Recalling notification. Id: {0}", ALId);
		try
		{
			if (NotificationInfo.Id == Guid.Empty)
			{
				throw NavNotificationCannotRecallUnknownException.Create();
			}
			if (session.TestExecution.RedirectNotificationOperationToTestHandler(this, NavHandlerType.RecallNotification))
			{
				return true;
			}
			if (ALScope == NotificationScope.LocalScope)
			{
				RecallLocalNotification();
				return true;
			}
			RecallGlobalNotification();
		}
		catch (NavBaseException exception)
		{
			session.Diagnostics.SendExceptionTag("00000B6", Category.Runtime, exception);
			if (errorLevel == DataError.TrapError)
			{
				return false;
			}
			throw;
		}
		return true;
	}

	public void ALAssign(NavNotification other)
	{
		Clone(other);
	}

	public void ALAssign(NavValue value)
	{
		if (value is NavNotification other)
		{
			ALAssign(other);
			return;
		}
		NotificationInfo = new NotificationInfo();
		NotificationInfo.Message = value.ToString();
	}

	public NavNotification()
	{
	}

	public NavNotification(string message)
	{
		NotificationInfo.Message = message;
	}

	public NavNotification(NavNotification other)
		: this()
	{
		Clone(other);
	}

	public NavNotification(NotificationInfo clrNotification)
		: this()
	{
		if (clrNotification != null)
		{
			NotificationInfo = clrNotification;
		}
	}

	public static NavNotification CreateFromObject(object clrNotification)
	{
		if (clrNotification is NotificationInfo clrNotification2)
		{
			return new NavNotification(clrNotification2);
		}
		throw new ArgumentException(Lang.ParameterNullOrEmpty, "clrNotification");
	}

	public NavNotification Clone()
	{
		return new NavNotification(this);
	}

	public override int GetHashCode()
	{
		return NotificationInfo?.GetHashCode() ?? 0;
	}

	public void Clear()
	{
		NotificationInfo = new NotificationInfo();
	}

	private void SendLocalNotification()
	{
		NavSession session = NavCurrentThread.Session;
		session.Diagnostics.SendTraceTag("0000093", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Sending local notification.");
		if (session.ClientConnectionType == ConnectionType.Nas || session.ClientConnectionType == ConnectionType.Background)
		{
			NavServerEventSource.Log.WriteALInfoMessageEvent(NavEnvironment.ServiceInstanceName, NavEnvironment.CurrentTenantId, NavDialog.ReplaceBackSlashWithCRLF(ALMessage));
			return;
		}
		IClientCallback clientCallbackOrNull = session.ClientCallbackOrNull;
		if (clientCallbackOrNull != null)
		{
			GetActivePageData(session);
			session.Diagnostics.SendTraceTag("000010U", Category.Runtime, Verbosity.Verbose, "Sending notification to the client with id: {0}.", DiagnosticParameter.Eupi(NotificationInfo.Id));
			clientCallbackOrNull.SendNotification(NotificationInfo.Clone());
			ClearActivePageData();
		}
		else
		{
			NavServerEventSource.Log.WriteALInfoMessageEvent(NavEnvironment.ServiceInstanceName, NavEnvironment.CurrentTenantId, NavDialog.ReplaceBackSlashWithCRLF(ALMessage));
		}
	}

	private void SendGlobalNotification()
	{
		NavSession session = NavCurrentThread.Session;
		session.Diagnostics.SendTraceTag("0000096", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Sending global notification.");
		throw new NavNotSupportedException(string.Format(CultureInfo.InvariantCulture, "GlobalScope is currently not supported. Notification with message '{0}' cannot be sent.", NotificationInfo.Message));
	}

	private void RecallLocalNotification()
	{
		NavSession session = NavCurrentThread.Session;
		session.Diagnostics.SendTraceTag("00000B7", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Recalling local notification.");
		if (session.ClientConnectionType != ConnectionType.Nas && session.ClientConnectionType != ConnectionType.Background)
		{
			IClientCallback clientCallbackOrNull = session.ClientCallbackOrNull;
			if (clientCallbackOrNull != null)
			{
				GetActivePageData(session);
				session.Diagnostics.SendTraceTag("00000B8", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Sending recall notification to the client.");
				clientCallbackOrNull.SendNotification(new NotificationInfo
				{
					Id = NotificationInfo.Id,
					SentFromServerFormHandleId = NotificationInfo.SentFromServerFormHandleId
				});
				ClearActivePageData();
			}
		}
	}

	private void RecallGlobalNotification()
	{
		NavSession session = NavCurrentThread.Session;
		session.Diagnostics.SendTraceTag("00000B9", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Recalling global notification.");
		throw new NavNotSupportedException(string.Format(CultureInfo.InvariantCulture, "GlobalScope is currently not supported. Notification with ID '{0}' cannot be recalled.", NotificationInfo.Id));
	}

	private void VerifyNotificationBeforeSend()
	{
		VerifyActionCountIsNotExeeded();
		if (string.IsNullOrEmpty(NotificationInfo.Message))
		{
			throw NavNotificationException.Create(Lang.NotificationMessageEmpty);
		}
	}

	private void VerifyCanAddAction()
	{
		VerifyActionCountIsNotExeeded(NotificationInfo.Actions.Count + 1);
	}

	private void VerifyActionCountIsNotExeeded()
	{
		VerifyActionCountIsNotExeeded(NotificationInfo.Actions.Count);
	}

	private void VerifyActionCountIsNotExeeded(int actionCount)
	{
		if (NotificationInfo.Scope == NotificationScope.GlobalScope && actionCount > 2)
		{
			throw NavNotificationActionCountExceededException.Create(NotificationInfo.Scope);
		}
		if (NotificationInfo.Scope == NotificationScope.LocalScope && actionCount > 3)
		{
			throw NavNotificationActionCountExceededException.Create(NotificationInfo.Scope);
		}
	}

	private void VerifyNotificationActionHandler(int codeunitId, string functionName)
	{
		using NavCodeunitHandle navCodeunitHandle = new NavCodeunitHandle(NavCurrentThread.Session, codeunitId);
		try
		{
			MethodInfo methodInfo = NavApplicationMethod.FindMethod(navCodeunitHandle.Target, functionName, new object[1] { NotificationInfo }, out var _);
			if (methodInfo == null)
			{
				throw NavNotificationActionHandlerNotFoundException.Create(codeunitId, functionName);
			}
		}
		catch (NavNCLMetadataObjectNotFoundException innerException)
		{
			throw NavNotificationActionHandlerNotFoundException.Create(codeunitId, functionName, innerException);
		}
	}

	private void GetActivePageData(NavSession session)
	{
		NavMethodScope navMethodScope;
		for (navMethodScope = session.CurrentMethodScope; navMethodScope != null; navMethodScope = navMethodScope.ParentScope)
		{
			NavApplicationObjectBase applicationObject = navMethodScope.ApplicationObject;
			if ((applicationObject != null && applicationObject.ObjectId.ObjectType == ObjectType.Page) || navMethodScope.ApplicationObject is NavFormExtension)
			{
				break;
			}
		}
		if (navMethodScope?.ApplicationObject is NavFormExtension { ParentObject: not null } navFormExtension)
		{
			NotificationInfo.SentFromServerFormHandleId = navFormExtension.ParentObject.Handle;
		}
		else if (navMethodScope == null || navMethodScope.ApplicationObject?.ObjectId.ObjectType != ObjectType.Page)
		{
			session.Diagnostics.SendTraceTag("00000AH", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Could not detect active page.");
		}
		else if (!(navMethodScope.ApplicationObject is NavForm navForm))
		{
			session.Diagnostics.SendTraceTag("00000AI", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "The found methodscope did not contain an ApplicationObject of type NavForm. {0}", navMethodScope.ApplicationObject);
		}
		else
		{
			NotificationInfo.SentFromServerFormHandleId = navForm.Handle;
		}
	}

	private void ClearActivePageData()
	{
		NotificationInfo.SentFromServerFormHandleId = Guid.Empty;
	}

	private void Clone(NavNotification other)
	{
		NotificationInfo = other.NotificationInfo.Clone();
	}
}
