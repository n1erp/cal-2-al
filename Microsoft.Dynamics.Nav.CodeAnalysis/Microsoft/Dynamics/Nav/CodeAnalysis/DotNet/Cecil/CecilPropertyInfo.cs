using System.Diagnostics;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Cecil;

[DebuggerDisplay("{Name}")]
internal class CecilPropertyInfo
{
	public string Name { get; }

	public Collection<CustomAttribute> CustomAttributes { get; }

	public CecilMethodInfo GetMethod { get; }

	public CecilMethodInfo SetMethod { get; }

	public string Signature { get; }

	public CecilPropertyInfo(PropertyDefinition definition, MethodReference getMethod, MethodReference setMethod)
	{
		Name = definition.Name;
		CustomAttributes = definition.CustomAttributes;
		GetMethod = ((getMethod != null) ? new CecilMethodInfo(definition.GetMethod, getMethod) : null);
		SetMethod = ((setMethod != null) ? new CecilMethodInfo(definition.SetMethod, setMethod) : null);
		Signature = GetSignature();
	}

	private string GetSignature()
	{
		return (GetMethod?.Signature ?? string.Empty) + (SetMethod?.Signature ?? string.Empty);
	}
}
