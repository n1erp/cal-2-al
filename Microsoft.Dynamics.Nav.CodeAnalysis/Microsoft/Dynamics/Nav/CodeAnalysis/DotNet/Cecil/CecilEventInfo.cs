using System.Diagnostics;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Cecil;

[DebuggerDisplay("event {ClosedEventType.FullName} {Name}")]
internal class CecilEventInfo
{
	public string Name { get; }

	public bool IsValid => EventType != null;

	public TypeDefinition EventType { get; }

	public TypeReference ClosedEventType { get; }

	public string Signature { get; }

	public Collection<CustomAttribute> CustomAttributes { get; }

	public CecilEventInfo(EventDefinition eventDefinition, TypeDefinition eventType, TypeReference closedEventType)
	{
		DebugAssertHelper.Assert(closedEventType != null);
		Name = eventDefinition.Name;
		EventType = eventType;
		ClosedEventType = closedEventType;
		Signature = Name + " " + closedEventType.FullName;
		CustomAttributes = eventDefinition.CustomAttributes;
	}
}
