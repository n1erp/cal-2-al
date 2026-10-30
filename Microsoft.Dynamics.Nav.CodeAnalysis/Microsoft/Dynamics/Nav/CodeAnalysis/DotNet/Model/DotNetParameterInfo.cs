using System;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;

[Serializable]
[DebuggerDisplay("{Name}")]
public sealed class DotNetParameterInfo
{
	public DotNetTypeInfo ParameterType { get; set; }

	public string Name { get; set; }

	public int Ordinal { get; set; }

	public bool IsParams { get; set; }

	public bool IsOptional { get; set; }

	public bool IsVar { get; set; }
}
