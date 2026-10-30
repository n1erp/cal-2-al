using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using Microsoft.Dynamics.Nav.Runtime;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.EventSubscription;

public class NavEventSubscription
{
	internal const string ScopePostfix = "_Scope";

	private const string SenderIdentifierName = "Sender";

	private readonly NavEventSubscriptionStatus status;

	private readonly IReadOnlyList<NavSubscriberParameter> subscriberParameters;

	private readonly int subscriberId;

	private readonly int memberId;

	private readonly INavEventSubscriber subscriber;

	private readonly MethodInfo subscriberMethodInfo;

	private readonly NavEventSubscriberAttribute subscriberMethodAttribute;

	private readonly string subscriberMethodAlName;

	private readonly NavAppGroup subscriberNavAppGroup;

	private readonly NavEventAttribute originalEventAttribute;

	private readonly NavEventPublisherMethodInfo originalTriggerMethodInfo;

	private readonly NavTriggerEventIdentity originalTriggerFieldIdentity;

	private readonly NCLMetaField originalTriggerField;

	private readonly NavEventExclusionFilter[] exclusionFilters;

	private readonly INavErrorReporter[] errorReporters;

	private readonly NCLMetaApplicationObject originalApplicationObject;

	private readonly Type originalMethodScopeType;

	private long noOfCalls;

	public bool RunInIsolatedTransactionAndFailureScope { get; }

	public NavEventSubscriptionStatus Status => status;

	public bool Resolved => (status & NavEventSubscriptionStatus.Resolved) != 0;

	public bool IsEventManualBinding => (status & NavEventSubscriptionStatus.ManualBinding) != 0;

	public long NoOfCalls
	{
		get
		{
			return Interlocked.Read(ref noOfCalls);
		}
		internal set
		{
			noOfCalls = value;
		}
	}

	public long NoOfCallsKeyHash
	{
		get
		{
			int num = ((OriginalTriggerEventFieldId != 0) ? OriginalTriggerEventFieldId : OriginalTriggerEventFieldName.GetHashCode());
			return ((long)SubscriberId << 31) ^ ((long)SubscriberNavAppGroup.GroupId << 26) ^ ((long)SubscriberMethodALName.GetHashCode() << 22) ^ ((long)OriginalEventType << 18) ^ ((long)OriginalApplicationObjectId.GetHashCode() << 14) ^ ((long)OriginalMethodALName.GetHashCode() << 10) ^ ((long)num << 6);
		}
	}

	public long SubscriberBindingHash
	{
		get
		{
			long num = NoOfCallsKeyHash;
			int num2 = 0;
			foreach (NavSubscriberParameter subscriberParameter in subscriberParameters)
			{
				num ^= (long)subscriberParameter.GetHashCode() << num2;
				num2 = (num2 + 1) % 30;
			}
			return num;
		}
	}

	public bool HasError => (status & NavEventSubscriptionStatus.ErrorMask) != 0;

	public int SubscriberId => subscriberId;

	public INavEventSubscriber Subscriber => subscriber;

	public string SubscriberMethodALName => subscriberMethodAlName;

	public NavAppGroup SubscriberNavAppGroup => subscriberNavAppGroup;

	public NavEventSubscriberAttribute SubscriberMethodAttribute => subscriberMethodAttribute;

	public ApplicationObjectId OriginalApplicationObjectId => subscriberMethodAttribute.TargetObjectId;

	public string OriginalMethodALName => subscriberMethodAttribute.TargetMethodName;

	public NCLMetaApplicationObject OriginalApplicationObject => originalApplicationObject;

	public Type OriginalMethodScopeType => originalMethodScopeType;

	public NavEventAttribute OriginalEventAttribute => originalEventAttribute;

	public int MemberId => memberId;

	public NavEventType OriginalEventType
	{
		get
		{
			if (originalEventAttribute != null)
			{
				return originalEventAttribute.EventType;
			}
			return NavEventType.Business;
		}
	}

	public NavTriggerEventType OriginalTriggerEventType
	{
		get
		{
			if (originalEventAttribute != null)
			{
				return originalEventAttribute.TriggerEventType;
			}
			return NavTriggerEventType.None;
		}
	}

	public int OriginalTriggerEventFieldId => subscriberMethodAttribute?.TargetFieldId ?? 0;

	public string OriginalTriggerEventFieldName
	{
		get
		{
			if (subscriberMethodAttribute != null && subscriberMethodAttribute.TargetFieldName != null)
			{
				return subscriberMethodAttribute.TargetFieldName;
			}
			return string.Empty;
		}
	}

	public NavTriggerEventIdentity OriginalTriggerFieldIdentity => originalTriggerFieldIdentity;

	public NCLMetaField OriginalTriggerEventField => originalTriggerField;

	public IReadOnlyList<NavSubscriberParameter> SubscriberParameters => subscriberParameters;

	public MethodInfo SubscriberMethodInfo => subscriberMethodInfo;

	public NavEventSubscription(INavEventSubscriber subscriber, NavEventSubscriberMethodInfo subscriberMethodInfo, NavAppGroup subscriberNavAppGroup, NavEventSubscriptionModifiers subscriptionModifiers, int memberId)
	{
		if (subscriber == null)
		{
			throw new ArgumentNullException("subscriber");
		}
		if (subscriberMethodInfo == null)
		{
			throw new ArgumentNullException("subscriberMethodInfo");
		}
		subscriberId = subscriber.ApplicationObjectId.ObjectNumber;
		this.subscriber = subscriber;
		this.memberId = memberId;
		this.subscriberMethodInfo = subscriberMethodInfo.RawMethodInfo;
		subscriberMethodAttribute = subscriberMethodInfo.Attribute;
		subscriberMethodAlName = subscriberMethodInfo.ALName;
		this.subscriberNavAppGroup = subscriberNavAppGroup ?? NavAppGroup.BaseGroup;
		exclusionFilters = new NavEventExclusionFilter[0];
		RunInIsolatedTransactionAndFailureScope = false;
		if (this.subscriber.IsEventManualBinding)
		{
			status = NavEventSubscriptionStatus.ManualBinding;
		}
		ApplicationObjectId targetObjectId = subscriberMethodAttribute.TargetObjectId;
		if (SenderParameterTypeFromObjectType(targetObjectId.ObjectType) == null)
		{
			status |= NavEventSubscriptionStatus.ErrorOriginalApplicationObjectTypeNotSupported;
			return;
		}
		if (!NavGlobal.NCLMetadata.TryGetMetaApplicationObject(targetObjectId.ObjectType, targetObjectId.ObjectNumber, out originalApplicationObject, this.subscriberNavAppGroup.GroupId))
		{
			status |= NavEventSubscriptionStatus.ErrorOriginalApplicationObjectNotFound;
			return;
		}
		originalMethodScopeType = originalApplicationObject.PublisherReflectionWrapper.GetScopeType(subscriberMethodAttribute.TargetMethodName);
		if (targetObjectId.ObjectType == ObjectType.Page || targetObjectId.ObjectType == ObjectType.Table)
		{
			foreach (NCLMetaApplicationObject extensionObject in originalApplicationObject.ExtensionObjects)
			{
				Type scopeType = extensionObject.PublisherReflectionWrapper.GetScopeType(subscriberMethodAttribute.TargetMethodName);
				if (scopeType != null)
				{
					if (!(originalMethodScopeType == null))
					{
						string message = string.Format(CultureInfo.InvariantCulture, "Extension '{3}' includes the event publisher '{0}' in the application object of type '{1}' with ID '{2}'. An event publisher with the same name already exists in '{4}'.", subscriberMethodAttribute.TargetMethodName, targetObjectId.ObjectType, targetObjectId.ObjectNumber, extensionObject.OwningApp.FriendlyDisplayName, (originalApplicationObject.OwningApp == null) ? "Base" : originalApplicationObject.OwningApp.FriendlyDisplayName);
						throw NavAppObjectMetadataException.Create(message);
					}
					originalMethodScopeType = scopeType;
					originalApplicationObject = extensionObject;
				}
			}
		}
		if (originalMethodScopeType == null)
		{
			originalTriggerMethodInfo = FindSubscriberTriggerMethodInfo(subscriberMethodAttribute);
			if (originalTriggerMethodInfo == null)
			{
				status |= NavEventSubscriptionStatus.ErrorOriginalMethodNotFound;
				return;
			}
			originalEventAttribute = originalTriggerMethodInfo.Attribute;
			if (TriggerEventTypeIsFieldBased(OriginalTriggerEventType))
			{
				switch (OriginalApplicationObject.ApplicationObjectType)
				{
				case ObjectType.Table:
				{
					NCLMetaTable nCLMetaTable2 = (NCLMetaTable)OriginalApplicationObject;
					if (nCLMetaTable2 != null)
					{
						if (OriginalTriggerEventFieldId != 0)
						{
							nCLMetaTable2.TryGetFieldByNo(OriginalTriggerEventFieldId, out originalTriggerField);
						}
						else
						{
							nCLMetaTable2.TryGetFieldByName(OriginalTriggerEventFieldName, out originalTriggerField);
						}
					}
					break;
				}
				case ObjectType.Page:
				{
					NCLMetaForm nCLMetaForm = (NCLMetaForm)OriginalApplicationObject;
					if (nCLMetaForm == null)
					{
						break;
					}
					int fieldIdToSearchFor = OriginalTriggerEventFieldId;
					string fieldNameToSearchFor = OriginalTriggerEventFieldName;
					if (OriginalTriggerEventType == NavTriggerEventType.OnBeforeValidateEvent || OriginalTriggerEventType == NavTriggerEventType.OnAfterValidateEvent)
					{
						ControlDataboundDefinition controlDataboundDefinition = ((fieldIdToSearchFor == 0) ? nCLMetaForm.Fields.FirstOrDefault((ControlDataboundDefinition field) => string.Compare(fieldNameToSearchFor, field.Name, StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(fieldNameToSearchFor, field.DataColumnName, StringComparison.OrdinalIgnoreCase) == 0) : nCLMetaForm.Fields.FirstOrDefault((ControlDataboundDefinition field) => field.ID == fieldIdToSearchFor));
						if (controlDataboundDefinition != null)
						{
							controlDataboundDefinition.HasOnValidateEventSubscriber = true;
							originalTriggerFieldIdentity = new NavTriggerEventIdentity(OriginalTriggerEventType, controlDataboundDefinition.ID.ToString(CultureInfo.InvariantCulture), controlDataboundDefinition.Name, OriginalApplicationObject.ObjectId, ObjectType.Page);
							break;
						}
						NavGlobal.NCLMetadata.TryGetMetaApplicationObject(ObjectType.Table, nCLMetaForm.SourceTable, out var metaApplicationObject, this.subscriberNavAppGroup.GroupId, requireCompiled: false);
						if (metaApplicationObject == null)
						{
							break;
						}
						NCLMetaTable nCLMetaTable = (NCLMetaTable)metaApplicationObject;
						if ((fieldIdToSearchFor == 0) ? nCLMetaTable.TryGetFieldByName(fieldNameToSearchFor, out var result) : nCLMetaTable.TryGetFieldByNo(fieldIdToSearchFor, out result))
						{
							string tableFieldNo = result.FieldNo.ToString(CultureInfo.InvariantCulture);
							controlDataboundDefinition = nCLMetaForm.Fields.FirstOrDefault((ControlDataboundDefinition field) => string.Compare(tableFieldNo, field.DataColumnName, StringComparison.OrdinalIgnoreCase) == 0);
							if (controlDataboundDefinition != null)
							{
								controlDataboundDefinition.HasOnValidateEventSubscriber = true;
								originalTriggerFieldIdentity = new NavTriggerEventIdentity(OriginalTriggerEventType, controlDataboundDefinition.ID.ToString(CultureInfo.InvariantCulture), controlDataboundDefinition.Name, OriginalApplicationObject.ObjectId, ObjectType.Page);
							}
						}
					}
					else
					{
						if (OriginalTriggerEventType != NavTriggerEventType.OnBeforeActionEvent && OriginalTriggerEventType != NavTriggerEventType.OnAfterActionEvent)
						{
							break;
						}
						ActionDefinition actionDefinition = ((fieldIdToSearchFor == 0) ? nCLMetaForm.Actions.FirstOrDefault((ActionDefinition actionDefinition2) => string.Compare(fieldNameToSearchFor, actionDefinition2.Name ?? actionDefinition2.DefaultName, StringComparison.OrdinalIgnoreCase) == 0) : nCLMetaForm.Actions.FirstOrDefault((ActionDefinition actionDefinition2) => actionDefinition2.ID == fieldIdToSearchFor));
						if (actionDefinition != null)
						{
							if (actionDefinition.RunObjectType != RunObjectType.None && OriginalTriggerEventType == NavTriggerEventType.OnBeforeActionEvent)
							{
								status |= NavEventSubscriptionStatus.ErrorOnBeforeActionEventDisabledOnRunObject;
								return;
							}
							originalTriggerFieldIdentity = new NavTriggerEventIdentity(OriginalTriggerEventType, actionDefinition.ID.ToString(CultureInfo.InvariantCulture), actionDefinition.Name, OriginalApplicationObject.ObjectId, ObjectType.Page);
							if (actionDefinition.Trigger == null)
							{
								actionDefinition.Trigger = new ActionTriggerDefinition
								{
									TriggerType = ActionTriggerDefinitionTriggerType.OnAction,
									Name = "_a45_OnValidate"
								};
							}
						}
					}
					break;
				}
				}
				if (originalTriggerField == null && !originalTriggerFieldIdentity.HasValue)
				{
					status |= NavEventSubscriptionStatus.ErrorFieldNotFound;
					return;
				}
			}
			bool excludeSourceTableRelatedParameters = OriginalApplicationObject.ApplicationObjectType == ObjectType.Page && ((NCLMetaForm)OriginalApplicationObject).SourceTable == 0;
			subscriberParameters = FindTriggerParameters(originalTriggerMethodInfo.RawMethodInfo, this.subscriberMethodInfo, excludeSourceTableRelatedParameters);
		}
		else
		{
			originalEventAttribute = originalApplicationObject.PublisherReflectionWrapper.GetMethodInfoByScopeType(originalMethodScopeType)?.Attribute;
			subscriberParameters = FindSubscriberParameters(originalApplicationObject, originalMethodScopeType, this.subscriberMethodInfo);
			if (subscriberParameters != null && originalEventAttribute != null)
			{
				if (!originalEventAttribute.AllowGlobalVarAccess && subscriberParameters.Any((NavSubscriberParameter sp) => sp.ParameterIsGlobalVar))
				{
					status |= NavEventSubscriptionStatus.ErrorGlobalVariableAccessNotAllowed;
					return;
				}
				foreach (NavSubscriberParameter item in subscriberParameters.Where((NavSubscriberParameter sp) => sp.ParameterIsSender))
				{
					if (!originalEventAttribute.IncludeSender)
					{
						status |= NavEventSubscriptionStatus.ErrorIncludeSenderNotAllowed;
						return;
					}
					NavObjectIdAttribute navObjectIdAttribute = item.SubscriberParameterInfo.GetCustomAttributes<NavObjectIdAttribute>().FirstOrDefault();
					if (navObjectIdAttribute == null || navObjectIdAttribute.ObjectId != OriginalApplicationObjectId.ObjectNumber)
					{
						status |= NavEventSubscriptionStatus.ErrorSenderParameterIsWrongType;
						return;
					}
				}
				ApplicationObjectId targetObjectId2 = subscriberMethodAttribute.TargetObjectId;
				string targetMethodName = SubscriberMethodAttribute.TargetMethodName;
				exclusionFilters = subscriptionModifiers.GetExclusionFiltersForPublisher(targetObjectId2, targetMethodName);
				errorReporters = subscriptionModifiers.GetErrorReportersForPublisher(targetObjectId2, targetMethodName);
				NavEventSubscriptionSettings settingsForPublisher = subscriptionModifiers.GetSettingsForPublisher(targetObjectId2, targetMethodName);
				RunInIsolatedTransactionAndFailureScope = settingsForPublisher.HasFlag(NavEventSubscriptionSettings.IsolatedMode);
			}
		}
		if (originalEventAttribute == null)
		{
			status |= NavEventSubscriptionStatus.ErrorOriginalMethodNotFound;
		}
		else if (subscriberParameters == null)
		{
			status |= NavEventSubscriptionStatus.ErrorParameterMismatch;
		}
		else
		{
			status |= NavEventSubscriptionStatus.Resolved;
		}
	}

	public bool IsCallExcludedByExclusionFilters(NavApplicationObjectBase subscriberInstance)
	{
		return exclusionFilters.Any((NavEventExclusionFilter f) => f.ShouldExcludeCall(this, subscriberInstance));
	}

	public void ReportErrors(string errorMessage, Exception error)
	{
		errorReporters.ForEach((INavErrorReporter r) =>
		{
			r.ReportError(errorMessage, error);
		});
	}

	public static bool TriggerEventTypeIsFieldBased(NavTriggerEventType triggerEventType)
	{
		if ((uint)(triggerEventType - 9) <= 1u || (uint)(triggerEventType - 22) <= 1u)
		{
			return true;
		}
		return false;
	}

	private static Type SenderParameterTypeFromObjectType(ObjectType appObjType)
	{
		switch (appObjType)
		{
		case ObjectType.CodeUnit:
			return typeof(NavCodeunitHandle);
		case ObjectType.Page:
		case ObjectType.PageExtension:
			return typeof(NavFormHandle);
		case ObjectType.Query:
			return typeof(NavQueryHandle);
		case ObjectType.Report:
			return typeof(NavReportHandle);
		case ObjectType.Table:
		case ObjectType.TableExtension:
			return typeof(INavRecordHandle);
		case ObjectType.XmlPort:
			return typeof(NavXmlPortHandle);
		default:
			return null;
		}
	}

	private static NavEventPublisherMethodInfo FindSubscriberTriggerMethodInfo(NavEventSubscriberAttribute subscriberEventAttribute)
	{
		if (subscriberEventAttribute == null)
		{
			throw new ArgumentNullException("subscriberEventAttribute");
		}
		return subscriberEventAttribute.TargetObjectId.ObjectType switch
		{
			ObjectType.Table => NavTableTriggerEventHandler.FindStaticTriggerMethodInfoByName(subscriberEventAttribute.TargetMethodName), 
			ObjectType.Page => NavPageTriggerEventHandler.FindStaticTriggerMethodInfoByName(subscriberEventAttribute.TargetMethodName), 
			_ => null, 
		};
	}

	private static IReadOnlyList<NavSubscriberParameter> FindSubscriberParameters(NCLMetaApplicationObject originalApplicationObject, Type originalMethodScopeType, MethodInfo subscriberMethodInfo)
	{
		Type applicationObjectClrType = originalApplicationObject.ApplicationObjectClrType;
		ConstructorInfo constructorInfo = originalMethodScopeType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single();
		ParameterInfo[] parameters = constructorInfo.GetParameters();
		ParameterInfo[] parameters2 = subscriberMethodInfo.GetParameters();
		List<NavSubscriberParameter> list = new List<NavSubscriberParameter>(parameters2.Length);
		ParameterInfo[] array = parameters2;
		foreach (ParameterInfo subscriberParameterInfo in array)
		{
			bool flag = parameters.Any((ParameterInfo pi) => string.Equals(pi.Name, subscriberParameterInfo.Name, StringComparison.OrdinalIgnoreCase));
			FieldInfo fieldInfo = null;
			NavSubscriberParameterMatchInfo navSubscriberParameterMatchInfo = (NavSubscriberParameterMatchInfo)0;
			Type originalType;
			if (flag)
			{
				fieldInfo = originalMethodScopeType.GetField(subscriberParameterInfo.Name, BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
				originalType = ((fieldInfo == null) ? null : fieldInfo.FieldType);
			}
			else if (string.Compare(subscriberParameterInfo.Name, "Sender", StringComparison.OrdinalIgnoreCase) == 0)
			{
				originalType = SenderParameterTypeFromObjectType(originalApplicationObject.ApplicationObjectType);
				navSubscriberParameterMatchInfo |= NavSubscriberParameterMatchInfo.ParameterIsSender;
			}
			else
			{
				fieldInfo = applicationObjectClrType.GetField(subscriberParameterInfo.Name, BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic);
				originalType = ((fieldInfo == null) ? null : fieldInfo.FieldType);
				navSubscriberParameterMatchInfo |= NavSubscriberParameterMatchInfo.ParameterIsGlobalVar;
			}
			Type subscriberType = subscriberParameterInfo.ParameterType;
			NavSubscriberParameterMatchInfo navSubscriberParameterMatchInfo2 = CalculateSubscriberParameterMatchInfo(ref originalType, ref subscriberType);
			if (navSubscriberParameterMatchInfo2 == NavSubscriberParameterMatchInfo.ParameterMismatch)
			{
				return null;
			}
			navSubscriberParameterMatchInfo |= navSubscriberParameterMatchInfo2;
			NavSubscriberParameter item = new NavSubscriberParameter(navSubscriberParameterMatchInfo, subscriberParameterInfo, fieldInfo);
			list.Add(item);
		}
		return list;
	}

	private static IReadOnlyList<NavSubscriberParameter> FindTriggerParameters(MethodInfo originalTriggerMethodInfo, MethodInfo subscriberMethodInfo, bool excludeSourceTableRelatedParameters)
	{
		ParameterInfo[] parameters = originalTriggerMethodInfo.GetParameters();
		ParameterInfo[] parameters2 = subscriberMethodInfo.GetParameters();
		List<NavSubscriberParameter> list = new List<NavSubscriberParameter>(parameters2.Length);
		ParameterInfo[] array = parameters2;
		foreach (ParameterInfo subscriberParameterInfo in array)
		{
			ParameterInfo parameterInfo = parameters.FirstOrDefault((ParameterInfo pi) => string.Equals(pi.Name, subscriberParameterInfo.Name, StringComparison.OrdinalIgnoreCase));
			if (parameterInfo == null)
			{
				return null;
			}
			if (!Enum.TryParse<NavTriggerParameter>(subscriberParameterInfo.Name, ignoreCase: true, out var result))
			{
				throw new InvalidOperationException();
			}
			Type originalType = parameterInfo.ParameterType;
			Type subscriberType = subscriberParameterInfo.ParameterType;
			NavSubscriberParameterMatchInfo navSubscriberParameterMatchInfo = CalculateSubscriberParameterMatchInfo(ref originalType, ref subscriberType);
			if (navSubscriberParameterMatchInfo == NavSubscriberParameterMatchInfo.ParameterMismatch)
			{
				return null;
			}
			if (excludeSourceTableRelatedParameters && subscriberType == typeof(INavRecordHandle))
			{
				return null;
			}
			NavSubscriberParameter navSubscriberParameter = new NavSubscriberParameter(navSubscriberParameterMatchInfo, subscriberParameterInfo, result);
			if (navSubscriberParameter.OriginalIsByRef != navSubscriberParameter.SubscriberIsByRef)
			{
				return null;
			}
			list.Add(navSubscriberParameter);
		}
		return list;
	}

	private static NavSubscriberParameterMatchInfo CalculateSubscriberParameterMatchInfo(ref Type originalType, ref Type subscriberType)
	{
		NavSubscriberParameterMatchInfo navSubscriberParameterMatchInfo = (NavSubscriberParameterMatchInfo)0;
		if (TypeIsNavByRef(ref originalType))
		{
			navSubscriberParameterMatchInfo |= NavSubscriberParameterMatchInfo.OriginalIsByRef;
		}
		if (TypeIsNavByRef(ref subscriberType))
		{
			navSubscriberParameterMatchInfo |= NavSubscriberParameterMatchInfo.SubscriberIsByRef;
		}
		if (originalType == typeof(NavOption) && subscriberType == typeof(int))
		{
			return navSubscriberParameterMatchInfo;
		}
		if (originalType == typeof(NavOption) && subscriberType == typeof(int))
		{
			return navSubscriberParameterMatchInfo;
		}
		if (originalType == typeof(NavRecordHandle) && subscriberType == typeof(INavRecordHandle))
		{
			return navSubscriberParameterMatchInfo;
		}
		if (originalType == null || originalType != subscriberType)
		{
			return NavSubscriberParameterMatchInfo.ParameterMismatch;
		}
		return navSubscriberParameterMatchInfo;
	}

	internal void BindSubscription()
	{
		NavEventScope eventScope = GetEventScope(EventScopeGetOption.CreateIfNotFound);
		eventScope.AddSubscriber(this);
	}

	internal NavEventScope GetEventScope(EventScopeGetOption getOption = EventScopeGetOption.None)
	{
		if (OriginalEventType != NavEventType.Trigger)
		{
			if (OriginalMethodScopeType != null)
			{
				NavMethodScope navMethodScope = (NavMethodScope)FormatterServices.GetUninitializedObject(OriginalMethodScopeType);
				return navMethodScope.GetEventScope(getOption);
			}
		}
		else if (TriggerEventTypeIsFieldBased(OriginalTriggerEventType))
		{
			if (OriginalApplicationObject.ApplicationObjectType == ObjectType.Table)
			{
				if (OriginalTriggerEventField != null)
				{
					return OriginalTriggerEventField.GetEventScope(OriginalTriggerEventType, getOption);
				}
			}
			else if (OriginalTriggerFieldIdentity.HasValue)
			{
				NavTriggerEventHandler navTriggerEventHandler = OriginalApplicationObject.TriggerEventHandler;
				if (navTriggerEventHandler == null && getOption == EventScopeGetOption.CreateIfNotFound)
				{
					navTriggerEventHandler = OriginalApplicationObject.GetOrCreateTriggerEventHandler();
				}
				return navTriggerEventHandler?.GetNamedEventScope(OriginalTriggerFieldIdentity, getOption);
			}
		}
		else if (OriginalApplicationObject != null)
		{
			return OriginalApplicationObject.GetEventScope(OriginalTriggerEventType, getOption);
		}
		if (getOption == EventScopeGetOption.CreateIfNotFound)
		{
			throw new InvalidOperationException();
		}
		return null;
	}

	public static IEnumerable<ApplicationObjectId> FindOriginalApplicationObjectIds(Type eventSubscriber)
	{
		foreach (MethodInfo item in from mi in eventSubscriber.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			where mi.IsDefined(typeof(NavEventSubscriberAttribute))
			select mi)
		{
			yield return item.GetCustomAttribute<NavEventSubscriberAttribute>().TargetObjectId;
		}
	}

	private static bool TypeIsNavByRef(ref Type t)
	{
		if (t != null && t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ByRef<>))
		{
			t = t.GetGenericArguments()[0];
			return true;
		}
		return false;
	}

	public void BumpNoOfCalls()
	{
		Interlocked.Increment(ref noOfCalls);
	}
}
