using System;

namespace Microsoft.Dynamics.Nav.Types;

[AttributeUsage(AttributeTargets.Method)]
public sealed class NavEventSubscriberAttribute : Attribute
{
	private ApplicationObjectId targetObjectId;

	private string targetMethodName;

	private string targetFieldName;

	private int targetFieldId;

	private int memberId;

	private EventSubscriberCallOptions callOptions;

	public ApplicationObjectId TargetObjectId => targetObjectId;

	public string TargetMethodName => targetMethodName;

	public int TargetFieldId => targetFieldId;

	public string TargetFieldName => targetFieldName;

	public int MemberId => memberId;

	public bool SkipOnMissingLicense => (callOptions & EventSubscriberCallOptions.SkipOnMissingLicense) != 0;

	public bool SkipOnMissingPermission => (callOptions & EventSubscriberCallOptions.SkipOnMissingPermission) != 0;

	public NavEventSubscriberAttribute(ObjectType targetObjectType, int targetObjectNo, string targetMethodName, string fieldName = "", EventSubscriberCallOptions options = (EventSubscriberCallOptions)0)
		: this(targetObjectType, targetObjectNo, targetMethodName, 0, fieldName, options)
	{
	}

	public NavEventSubscriberAttribute(ObjectType targetObjectType, int targetObjectNo, string targetMethodName, int memberId, string fieldName = "", EventSubscriberCallOptions options = (EventSubscriberCallOptions)0)
	{
		if (targetMethodName == null)
		{
			throw new ArgumentNullException("targetMethodName");
		}
		targetObjectId = new ApplicationObjectId(targetObjectType, targetObjectNo);
		this.targetMethodName = targetMethodName;
		this.memberId = memberId;
		targetFieldName = fieldName;
		callOptions = options;
	}

	public NavEventSubscriberAttribute(ObjectType targetObjectType, int targetObjectNo, string targetMethodName, int fieldId, EventSubscriberCallOptions options = (EventSubscriberCallOptions)0)
		: this(targetObjectType, targetObjectNo, targetMethodName, 0, "", options)
	{
		targetFieldId = fieldId;
	}

	public NavEventSubscriberAttribute(ObjectType targetObjectType, int targetObjectNo, string targetMethodName, int memberId, int fieldId, EventSubscriberCallOptions options = (EventSubscriberCallOptions)0)
		: this(targetObjectType, targetObjectNo, targetMethodName, memberId, "", options)
	{
		targetFieldId = fieldId;
	}
}
