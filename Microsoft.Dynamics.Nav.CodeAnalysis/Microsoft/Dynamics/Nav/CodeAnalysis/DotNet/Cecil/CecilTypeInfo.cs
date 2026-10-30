using System.Diagnostics;
using Mono.Cecil;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Cecil;

[DebuggerDisplay("{ClosedType?.FullName ?? TypeDefinition?.FullName ?? string.Empty}")]
internal class CecilTypeInfo
{
	public TypeReference ClosedType { get; }

	public TypeDefinition TypeDefinition { get; }

	public TypeReference BaseClass { get; }

	public TypeReference[] Interfaces { get; }

	public bool IsSupportedType => ClosedType != null;

	public bool HasDefinition => TypeDefinition != null;

	public CecilTypeInfo(TypeDefinition definition, TypeReference closedReference, TypeReference baseClass, TypeReference[] interfaces)
	{
		TypeDefinition = definition;
		ClosedType = closedReference;
		BaseClass = baseClass;
		Interfaces = interfaces;
	}
}
