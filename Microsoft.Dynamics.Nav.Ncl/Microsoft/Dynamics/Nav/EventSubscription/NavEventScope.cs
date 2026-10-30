using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.EventSubscription;

public class NavEventScope
{
	private delegate object[] PrepareParametersCallBack(NavScope callerScope, NavEventSubscription subscriber);

	private NavEventSubscription[] registeredSubscriptions;

	private bool isStale;

	private long subscriberBindingHash;

	public static NavEventScope StaleEventScope { get; private set; }

	public bool HasSubscribers
	{
		get
		{
			NavEventSubscription[] array = registeredSubscriptions;
			if (array != null)
			{
				return array.Length != 0;
			}
			return false;
		}
	}

	public bool IsStale
	{
		get
		{
			return isStale;
		}
		set
		{
			if (!value)
			{
				throw new InvalidOperationException();
			}
			isStale = true;
		}
	}

	static NavEventScope()
	{
		StaleEventScope = new NavEventScope
		{
			isStale = true
		};
	}

	internal NavEventScope()
	{
	}

	public bool ClearEventScope()
	{
		registeredSubscriptions = null;
		subscriberBindingHash = 0L;
		return !IsStale;
	}

	internal void AddSubscriber(NavEventSubscription subscription)
	{
		lock (this)
		{
			if (IsStale)
			{
				throw new InvalidOperationException();
			}
			if (registeredSubscriptions == null)
			{
				registeredSubscriptions = new NavEventSubscription[1] { subscription };
			}
			else
			{
				NavEventSubscription[] array = new NavEventSubscription[registeredSubscriptions.Length + 1];
				for (int i = 0; i < registeredSubscriptions.Length; i++)
				{
					array[i] = registeredSubscriptions[i];
				}
				array[array.Length - 1] = subscription;
				registeredSubscriptions = array;
			}
			subscriberBindingHash ^= subscription.SubscriberBindingHash;
		}
	}

	public void CheckAndFireEvents(NavMethodScope originalMethodScope)
	{
		NavApplicationObjectBase publisherApplicationObject = originalMethodScope.ApplicationObject;
		PrepareParametersCallBack prepareParameters = (NavScope callerScope, NavEventSubscription subscriber) =>
		{
			object[] array = new object[subscriber.SubscriberParameters.Count];
			int num = 0;
			foreach (NavSubscriberParameter subscriberParameter in subscriber.SubscriberParameters)
			{
				object obj;
				if (subscriberParameter.ParameterIsSender)
				{
					obj = publisherApplicationObject.CreateHandle(callerScope);
				}
				else
				{
					object obj2 = (subscriberParameter.ParameterIsGlobalVar ? ((object)publisherApplicationObject) : ((object)originalMethodScope));
					obj = subscriberParameter.OriginalFieldInfo.GetValue(obj2);
					if (subscriberParameter.OriginalIsByRef != subscriberParameter.SubscriberIsByRef)
					{
						if (subscriberParameter.OriginalIsByRef)
						{
							obj = ((IByRef)obj).ObjectValue;
						}
						else
						{
							Type typeFromHandle = typeof(ByRef<>);
							Type type = typeFromHandle.MakeGenericType(subscriberParameter.OriginalFieldInfo.FieldType);
							obj = Activator.CreateInstance(type, subscriberParameter.OriginalFieldInfo, obj2);
						}
					}
				}
				NavOption navOption = obj as NavOption;
				if (navOption != null && !navOption.NavOptionMetadata.IsEnum && subscriberParameter.SubscriberParameterInfo.ParameterType != typeof(NavOption))
				{
					obj = navOption.ToInt32();
				}
				array[num++] = obj;
			}
			return array;
		};
		ProcessCallToAllSubscribers(publisherApplicationObject, prepareParameters);
	}

	public void CheckAndFireTriggerEvents(NavApplicationObjectBase triggerApplicationObject, Func<NavTriggerParameter, object> parameterResolver)
	{
		PrepareParametersCallBack prepareParameters = (NavScope callerScope, NavEventSubscription subscriber) =>
		{
			object[] array = new object[subscriber.SubscriberParameters.Count];
			int num = 0;
			foreach (NavSubscriberParameter subscriberParameter in subscriber.SubscriberParameters)
			{
				object obj = parameterResolver(subscriberParameter.OriginalTriggerParameter);
				array[num++] = obj;
			}
			return array;
		};
		using (new InjectedEventMethodScope(triggerApplicationObject))
		{
			ProcessCallToAllSubscribers(triggerApplicationObject, prepareParameters);
		}
	}

	private void ProcessCallToAllSubscribers(NavApplicationObjectBase callerApplicationObject, PrepareParametersCallBack prepareParameters)
	{
		if (IsStale)
		{
			throw new NavAdministratorMadeChangesException();
		}
		NavEventSubscription[] array = registeredSubscriptions;
		if (array != null && array.Length != 0)
		{
			NCLMetaApplicationObject originalApplicationObject = array[0].OriginalApplicationObject;
			CheckPublisherInstanceIsValid(callerApplicationObject, originalApplicationObject);
			ProcessCallToTypeAndManualSubscriptions(callerApplicationObject, array, prepareParameters);
		}
	}

	private void ProcessCallToTypeAndManualSubscriptions(NavApplicationObjectBase callerApplicationObject, NavEventSubscription[] subscriptions, PrepareParametersCallBack prepareParameters)
	{
		foreach (NavEventSubscription navEventSubscription in subscriptions)
		{
			int groupId = navEventSubscription.SubscriberNavAppGroup.GroupId;
			if (groupId != 0)
			{
				NavAppGroup navAppGroup = NavCurrentThread.ResolveAppGroup(callerApplicationObject.Session);
				if (groupId != navAppGroup.GroupId)
				{
					continue;
				}
			}
			if (navEventSubscription.IsEventManualBinding)
			{
				int subscriberId = navEventSubscription.SubscriberId;
				foreach (NavCodeunit item in callerApplicationObject.Session.EventBindings.ToList())
				{
					if (subscriberId != item.ObjectId.ObjectNumber)
					{
						continue;
					}
					Type type = item.GetType();
					Type declaringType = navEventSubscription.SubscriberMethodInfo.DeclaringType;
					if ((object)type != declaringType)
					{
						callerApplicationObject.Session.Diagnostics.SendTraceTag("00001GR", Category.EventSubscription, Verbosity.Warning, PrivacyClassification.SystemMetadata, "A manually bound event subscriber is stale and will no longer be called. This can be caused by the subscriber object being updated by an operation in the development environment.\r\nPublisher object and function: {0} - {1} - {2} {3}\r\nSubscriber object and function: {4} - {5} - {6}", navEventSubscription.OriginalApplicationObjectId.ObjectType, navEventSubscription.OriginalApplicationObjectId.ObjectNumber, navEventSubscription.OriginalMethodALName, (navEventSubscription.OriginalTriggerEventFieldId != 0) ? ("'Field" + navEventSubscription.OriginalTriggerEventFieldId + "'") : (string.IsNullOrEmpty(navEventSubscription.OriginalTriggerEventFieldName) ? string.Empty : ("'" + navEventSubscription.OriginalTriggerEventFieldName + "'")), item.ObjectId.ObjectType, item.ObjectId.ObjectNumber, navEventSubscription.SubscriberMethodALName);
					}
					else
					{
						using NavScope callerScope = new NavScope(callerApplicationObject.Session.CurrentMethodScope);
						object[] parameters = prepareParameters(callerScope, navEventSubscription);
						CallEventSubscriber(callerApplicationObject, navEventSubscription, item, parameters);
					}
				}
			}
			else
			{
				using NavScope navScope = new NavScope(callerApplicationObject.Session.CurrentMethodScope);
				NavCodeunitHandle navCodeunitHandle = new NavCodeunitHandle(navScope, navEventSubscription.Subscriber.ApplicationObjectId.ObjectNumber);
				NavCodeunit target = navCodeunitHandle.Target;
				object[] parameters2 = prepareParameters(navScope, navEventSubscription);
				CallEventSubscriber(callerApplicationObject, navEventSubscription, target, parameters2);
			}
		}
	}

	private void CheckPublisherInstanceIsValid(NavApplicationObjectBase publisherApplicationObject, NCLMetaApplicationObject originalMetaApplicationObject)
	{
		if (IsStale)
		{
			throw new NavAdministratorMadeChangesException();
		}
		originalMetaApplicationObject.CheckApplicationObjectIsValid(publisherApplicationObject);
	}

	private void CallEventSubscriber(NavApplicationObjectBase callingApplicationObject, NavEventSubscription subscriber, NavApplicationObjectBase subscriberInstance, object[] parameters)
	{
		if (SkipCallDueToLackOfPermissions(callingApplicationObject, subscriber, subscriberInstance) || subscriber.IsCallExcludedByExclusionFilters(subscriberInstance))
		{
			return;
		}
		if (subscriber.RunInIsolatedTransactionAndFailureScope)
		{
			if (NavCurrentThread.Session.IsTransactionActive())
			{
				Exception ex = new NavNCLEventInvocationException("An event subscriber configured to run in an isolated transaction/failure scope cannot be called in a context where a transaction is already active.");
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001EW", Category.EventSubscription, ex);
				throw ex;
			}
			try
			{
				string content = string.Format(CultureInfo.InvariantCulture, "Isolated Event -- PubObj:{0}; PubMethod:{1}; SubId:{2}; SubMethod:{3}", subscriber.OriginalApplicationObjectId, subscriber.OriginalMethodALName, subscriber.SubscriberId, subscriber.SubscriberMethodALName);
				using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("00001EY", DiagnosticParameter.SystemMetadata(content), MonitorType.ALStatementsExecuted, Category.EventSubscription, createNewActivity: false))
				{
					NavCurrentThread.Session.BeginTransaction();
					CallEventSubscriberInternal(subscriber, subscriberInstance, parameters);
					NavCurrentThread.Session.EndTransaction(commit: true);
					return;
				}
			}
			catch (Exception ex2)
			{
				NavCurrentThread.Session.EndTransaction(commit: false);
				string text = string.Format(CultureInfo.InvariantCulture, "An error occurred while executing an isolated event and its transaction has been rolled back. PubObj:{0}; PubMethod:{1}; SubId:{2}; SubMethod:{3}", subscriber.OriginalApplicationObjectId, subscriber.OriginalMethodALName, subscriber.SubscriberId, subscriber.SubscriberMethodALName) + Environment.NewLine + Environment.NewLine + ex2;
				subscriber.ReportErrors(text, ex2);
				EventLogWriter.Write(text, EventLogEntryType.Error, null, 0, null, 0);
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001EX", Category.EventSubscription, ex2);
				return;
			}
		}
		CallEventSubscriberInternal(subscriber, subscriberInstance, parameters);
	}

	private void CallEventSubscriberInternal(NavEventSubscription subscriber, NavApplicationObjectBase subscriberInstance, object[] parameters)
	{
		try
		{
			subscriber.BumpNoOfCalls();
			if (subscriber.MemberId == 0)
			{
				subscriber.SubscriberMethodInfo.Invoke(subscriberInstance, parameters);
			}
			else
			{
				subscriberInstance.Invoke(subscriber.MemberId, parameters);
			}
		}
		catch (NavNCLStackOverflowException)
		{
			throw NavCurrentThread.Session.Diagnostics.SendExceptionTag("00000KZ", Category.EventSubscription, NavNCLRecursiveEventCallException.Create(subscriber.SubscriberMethodALName, subscriber.OriginalTriggerEventType, subscriber.OriginalApplicationObject.ApplicationObjectId.ObjectType, subscriber.OriginalApplicationObject.ApplicationObjectId.ToString(), subscriber.OriginalTriggerEventFieldName, subscriber.OriginalTriggerEventFieldId));
		}
		catch (TargetException exception)
		{
			bool flag = subscriber.SubscriberMethodInfo.DeclaringType?.IsInstanceOfType(subscriberInstance) ?? false;
			string text = EventSubscriberDiagnosticMessage(subscriber.SubscriberMethodInfo, subscriberInstance, parameters);
			string content = "TypeValidation=" + flag + ", " + text;
			NavCurrentThread.Session.Diagnostics.SendExceptionTag("00000BA", Category.EventSubscription, exception, Verbosity.Error, DiagnosticParameter.SystemMetadata(content));
			throw;
		}
		catch (TargetInvocationException ex2)
		{
			ExceptionDispatchInfo exceptionDispatchInfo = ExceptionDispatchInfo.Capture(ex2.InnerException);
			if (exceptionDispatchInfo.SourceException is NavBaseException)
			{
				exceptionDispatchInfo.Throw();
			}
			string content2 = EventSubscriberDiagnosticMessage(subscriber.SubscriberMethodInfo, subscriberInstance, parameters);
			NavCurrentThread.Session.Diagnostics.SendExceptionTag("000008X", Category.EventSubscription, exceptionDispatchInfo.SourceException ?? ex2, Verbosity.Error, DiagnosticParameter.SystemMetadata(content2));
			exceptionDispatchInfo.Throw();
		}
		catch (Exception exception2)
		{
			string content3 = EventSubscriberDiagnosticMessage(subscriber.SubscriberMethodInfo, subscriberInstance, parameters);
			NavCurrentThread.Session.Diagnostics.SendExceptionTag("000008Y", Category.EventSubscription, exception2, Verbosity.Error, DiagnosticParameter.SystemMetadata(content3));
			throw;
		}
	}

	private static string EventSubscriberDiagnosticMessage(MethodInfo methodInfo, NavApplicationObjectBase subscriberCodeunitInstance, object[] parameters)
	{
		string text = methodInfo.DeclaringType?.AssemblyQualifiedName;
		string assemblyQualifiedName = subscriberCodeunitInstance.GetType().AssemblyQualifiedName;
		if (text != assemblyQualifiedName)
		{
			return null;
		}
		return string.Format(CultureInfo.CurrentCulture, "EventSignature failure: SubScriber assy = {0}, target assy = {1}, Subscriber signature = '{2}', target signature = '{3}'", text, assemblyQualifiedName, GetMethodSignature(methodInfo), GetParameterSignature(subscriberCodeunitInstance, parameters));
	}

	private static string GetMethodSignature(MethodInfo methodInfo)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"{methodInfo.ReturnType} {methodInfo.DeclaringType}.{methodInfo.Name}(");
		if (methodInfo.GetParameters().Any())
		{
			ParameterInfo[] parameters = methodInfo.GetParameters();
			foreach (ParameterInfo parameterInfo in parameters)
			{
				stringBuilder.Append($"{parameterInfo.ParameterType} {parameterInfo.Name}, ");
			}
			stringBuilder.Length -= 2;
		}
		stringBuilder.Append(')');
		return stringBuilder.ToString();
	}

	private static string GetParameterSignature(NavApplicationObjectBase subscriberInstance, object[] parameters)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(subscriberInstance.GetType()).Append(subscriberInstance.ObjectName).Append("[ID=")
			.Append(subscriberInstance.ObjectId)
			.Append("](");
		if (parameters != null && parameters.Any())
		{
			foreach (object obj in parameters)
			{
				stringBuilder.Append(obj.GetType()).Append(", ");
			}
			stringBuilder.Length -= 2;
		}
		stringBuilder.Append(')');
		return stringBuilder.ToString();
	}

	private static bool SkipCallDueToLackOfPermissions(NavApplicationObjectBase callingObject, NavEventSubscription subscriber, NavApplicationObjectBase subscriberInstance)
	{
		if (!subscriber.SubscriberMethodAttribute.SkipOnMissingPermission && !subscriber.SubscriberMethodAttribute.SkipOnMissingLicense)
		{
			return false;
		}
		if (subscriber.SubscriberMethodAttribute.SkipOnMissingLicense)
		{
			PermissionManager permissionManager = subscriberInstance.Tree.Session.Permissions.GetPermissionManager(subscriberInstance.Tree.Session.Company.CompanyNameToken);
			IPermissionSet licensePermissionSet = permissionManager.GetLicensePermissionSet(callingObject, subscriberInstance.ObjectId);
			if (!licensePermissionSet.HasPermissions(subscriberInstance, PermissionMask.Execute, subscriberInstance.Tree.Session.LimitedUserPermissionChecker))
			{
				return true;
			}
		}
		if (subscriber.SubscriberMethodAttribute.SkipOnMissingPermission && !subscriberInstance.Tree.Session.HasExecutePermission(subscriberInstance))
		{
			return true;
		}
		return false;
	}
}
