using System.Reflection;

namespace Microsoft.Dynamics.Nav.EventSubscription;

public class NavSubscriberParameter
{
	public NavSubscriberParameterMatchInfo ParameterType { get; private set; }

	public ParameterInfo SubscriberParameterInfo { get; private set; }

	public FieldInfo OriginalFieldInfo { get; private set; }

	public NavTriggerParameter OriginalTriggerParameter { get; private set; }

	public bool SubscriberIsByRef => (ParameterType & NavSubscriberParameterMatchInfo.SubscriberIsByRef) != 0;

	public bool OriginalIsByRef => (ParameterType & NavSubscriberParameterMatchInfo.OriginalIsByRef) != 0;

	public bool ParameterIsGlobalVar => (ParameterType & NavSubscriberParameterMatchInfo.ParameterIsGlobalVar) != 0;

	public bool ParameterIsSender => (ParameterType & NavSubscriberParameterMatchInfo.ParameterIsSender) != 0;

	public NavSubscriberParameter(NavSubscriberParameterMatchInfo parameterType, ParameterInfo subscriberParameterInfo, FieldInfo originalFieldInfo)
	{
		ParameterType = parameterType;
		SubscriberParameterInfo = subscriberParameterInfo;
		OriginalFieldInfo = originalFieldInfo;
	}

	public NavSubscriberParameter(NavSubscriberParameterMatchInfo parameterType, ParameterInfo subscriberParameterInfo, NavTriggerParameter originalTriggerParameter)
	{
		ParameterType = parameterType;
		SubscriberParameterInfo = subscriberParameterInfo;
		OriginalTriggerParameter = originalTriggerParameter;
	}
}
