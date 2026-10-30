using System;

namespace Microsoft.Dynamics.Nav.Runtime;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class FieldTriggerHandlerAttribute : Attribute
{
	private readonly FieldTriggerType triggerType;

	private readonly int fieldNo;

	public int FieldNo => fieldNo;

	public FieldTriggerType TriggerType => triggerType;

	public FieldTriggerHandlerAttribute(FieldTriggerType triggerType, int fieldNo)
	{
		this.triggerType = triggerType;
		this.fieldNo = fieldNo;
	}
}
