using System;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class ALFunctionTracingExecutionListener : IExecutionListener
{
	private string[] objectTypeNames;

	private ALCodeEnvironment codeEnvironment;

	private ALCodeEnvironment CodeEnvironment => codeEnvironment ?? (codeEnvironment = NavGlobal.NCLMetadata.CodeEnvironment);

	private static Guid ClientSessionId => TypesThreadLocalStorage.Current?.ClientSessionId ?? Guid.Empty;

	private static Guid ClientActivityId => TypesThreadLocalStorage.Current?.ClientActivityId ?? Guid.Empty;

	private static ApplicationObjectId GetApplicationObjectId(NavMethodScope methodScope)
	{
		return ALCodeEnvironment.EffectiveObjectId(methodScope.ApplicationObject);
	}

	void IExecutionListener.ProcessExit(NavMethodScope methodScope)
	{
		if (CanLogEvent(methodScope))
		{
			ApplicationObjectId applicationObjectId = GetApplicationObjectId(methodScope);
			NavAppRuntimeMetadata extensionMetadataForObject = CodeEnvironment.GetExtensionMetadataForObject(applicationObjectId);
			NavServerEventSource.Log.WriteALFunctionStopEvent(methodScope.Session.Tenant.Id, methodScope.Session.Id, ObjectTypeToString(applicationObjectId.ObjectType), applicationObjectId.ObjectNumber, (extensionMetadataForObject != null) ? ((object)extensionMetadataForObject.AppId).ToString() : null, ((extensionMetadataForObject != null) ? extensionMetadataForObject.TechnicalDisplayName : null) ?? "", GetMethodName(methodScope), NavEnvironment.ServerInstanceName, VersionCompatibility.AssemblyFileVersionName, methodScope.Session.Tenant.TenantSettings.AadTenantId ?? "", ClientSessionId, ClientActivityId, methodScope.Session.UniqueSessionId, Guid.Empty, methodScope.ExecutionMilliseconds);
		}
	}

	void IExecutionListener.ProcessStatementHit(NavMethodScope methodScope)
	{
		if (CanLogEvent(methodScope))
		{
			ApplicationObjectId? applicationObjectId = null;
			NavAppRuntimeMetadata val = null;
			if (!methodScope.IsALFunctionStartEventLogged)
			{
				applicationObjectId = GetApplicationObjectId(methodScope);
				val = CodeEnvironment.GetExtensionMetadataForObject(applicationObjectId.Value);
				NavServerEventSource.Log.WriteALFunctionStartEvent(methodScope.Session.Tenant.Id, methodScope.Session.Id, ObjectTypeToString(applicationObjectId.Value.ObjectType), applicationObjectId.Value.ObjectNumber, (val != null) ? ((object)val.AppId).ToString() : null, ((val != null) ? val.TechnicalDisplayName : null) ?? "", methodScope.ScopeName, NavEnvironment.ServerInstanceName, VersionCompatibility.AssemblyFileVersionName, methodScope.Session.Tenant.TenantSettings.AadTenantId ?? "", ClientSessionId, ClientActivityId, methodScope.Session.UniqueSessionId, Guid.Empty);
				methodScope.IsALFunctionStartEventLogged = true;
			}
			if (ServerUserSettings.Instance.EnableFullALFunctionTracing.Value)
			{
				GetSourceContextInfoAndMethodName(methodScope, out var sourceContextInfo, out var _);
				Tuple<int, string> lineNumberAndStatement = CodeEnvironment.GetLineNumberAndStatement(methodScope, sourceContextInfo);
				applicationObjectId = applicationObjectId ?? GetApplicationObjectId(methodScope);
				val = val ?? CodeEnvironment.GetExtensionMetadataForObject(applicationObjectId.Value);
				NavServerEventSource.Log.WriteALFunctionStatementEvent(methodScope.Session.Tenant.Id, methodScope.Session.Id, ObjectTypeToString(applicationObjectId.Value.ObjectType), applicationObjectId.Value.ObjectNumber, (val != null) ? ((object)val.AppId).ToString() : null, ((val != null) ? val.TechnicalDisplayName : null) ?? "", methodScope.ScopeName, lineNumberAndStatement.Item1, lineNumberAndStatement.Item2, NavEnvironment.ServerInstanceName, VersionCompatibility.AssemblyFileVersionName, methodScope.Session.Tenant.TenantSettings.AadTenantId ?? "", ClientSessionId, ClientActivityId, methodScope.Session.UniqueSessionId, Guid.Empty);
			}
		}
	}

	void IExecutionListener.ProcessException(NavMethodScope methodScope, Exception exception)
	{
		if (CanLogEvent(methodScope))
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("AL");
			stringBuilder.Append("ErrorCode:");
			stringBuilder.AppendLine(ALSystemErrorHandling.ALGetLastErrorCode);
			stringBuilder.Append("Connection type:");
			stringBuilder.AppendLine(NavCurrentThread.Session.ClientConnectionType.ToString());
			stringBuilder.AppendLine("Message:");
			stringBuilder.AppendLine(NavCurrentThread.Session.GetLastErrorTextWithoutPrivateInformation);
			stringBuilder.AppendLine("StackTrace:");
			stringBuilder.AppendLine(ALSystemErrorHandling.ALGetLastErrorCallStack.Replace("\\", Environment.NewLine));
			stringBuilder.AppendLine(".NET:");
			ExceptionHandler.FormatExceptionClean(stringBuilder, exception);
			ApplicationObjectId applicationObjectId = GetApplicationObjectId(methodScope);
			NavAppRuntimeMetadata extensionMetadataForObject = CodeEnvironment.GetExtensionMetadataForObject(applicationObjectId);
			string objectExtensionInfo = ((extensionMetadataForObject != null) ? extensionMetadataForObject.TechnicalDisplayName : null) ?? "";
			string methodName = GetMethodName(methodScope);
			string failureMessage = stringBuilder.ToString();
			NavServerEventSource.Log.WriteALFunctionFailedEventNoPersonalData(methodScope.Session.Tenant.Id, methodScope.Session.Id, ObjectTypeToString(applicationObjectId.ObjectType), applicationObjectId.ObjectNumber, (extensionMetadataForObject != null) ? ((object)extensionMetadataForObject.AppId).ToString() : null, objectExtensionInfo, methodName, failureMessage, NavEnvironment.ServerInstanceName, VersionCompatibility.AssemblyFileVersionName, methodScope.Session.Tenant.TenantSettings.AadTenantId ?? "", ClientSessionId, ClientActivityId, methodScope.Session.UniqueSessionId, Guid.Empty);
			NavServerEventSource.Log.WriteALFunctionFailedEvent(methodScope.Session.Tenant.Id, methodScope.Session.Id, methodScope.Session.UserDisplayName, ObjectTypeToString(applicationObjectId.ObjectType), applicationObjectId.ObjectNumber, (extensionMetadataForObject != null) ? ((object)extensionMetadataForObject.AppId).ToString() : null, objectExtensionInfo, methodName, failureMessage, NavEnvironment.ServerInstanceName, VersionCompatibility.AssemblyFileVersionName, methodScope.Session.Tenant.TenantSettings.AadTenantId ?? "", methodScope.Session.User.AuthenticationObjectId ?? "", ClientSessionId, ClientActivityId, methodScope.Session.UniqueSessionId, Guid.Empty);
		}
	}

	bool IExecutionListener.CanCoexistWithOtherListener(IExecutionListener otherListener)
	{
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool CanLogEvent(NavMethodScope methodScope)
	{
		if (methodScope.Session.Diagnostics.FullALFunctionTracingEnabled || ((methodScope.IsTopLevelCall || ServerUserSettings.Instance.EnableFullALFunctionTracing.Value) && NavServerEventSource.Log.IsEnabledALFunctionCallTracing()))
		{
			return methodScope.ApplicationObject != null;
		}
		return false;
	}

	private void GetSourceContextInfoAndMethodName(NavMethodScope methodScope, out SourceContextInfo sourceContextInfo, out string methodName)
	{
		sourceContextInfo = CodeEnvironment.GetSourceContextInfo(methodScope);
		methodName = CodeEnvironment.GetMethodName(methodScope, sourceContextInfo);
	}

	private static string GetMethodName(NavMethodScope methodScope)
	{
		return methodScope.ScopeName;
	}

	private string ObjectTypeToString(ObjectType objectType)
	{
		if (objectTypeNames == null)
		{
			objectTypeNames = Enum.GetNames(typeof(ObjectType));
		}
		return objectTypeNames[(int)objectType];
	}
}
