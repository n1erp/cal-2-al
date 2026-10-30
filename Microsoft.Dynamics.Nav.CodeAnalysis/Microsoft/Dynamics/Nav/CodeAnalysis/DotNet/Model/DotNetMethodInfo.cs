using System;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;

[Serializable]
[DebuggerDisplay("{Name}")]
public class DotNetMethodInfo : DotNetTypeMemberInfo
{
	public bool IsStatic { get; set; }

	public DotNetReturnValueInfo ReturnValue { get; set; }

	public DotNetParameterInfo[] Parameters { get; set; }

	public string Signature { get; set; }
}
