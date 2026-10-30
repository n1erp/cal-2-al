using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;

[Serializable]
public sealed class DotNetReturnValueInfo
{
	public bool IsOptional { get; set; }

	public DotNetTypeInfo Type { get; set; }
}
