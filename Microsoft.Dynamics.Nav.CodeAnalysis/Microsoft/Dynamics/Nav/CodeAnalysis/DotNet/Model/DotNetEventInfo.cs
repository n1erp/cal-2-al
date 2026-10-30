using System;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;

[Serializable]
[DebuggerDisplay("{Name}")]
public class DotNetEventInfo : DotNetTypeMemberInfo
{
	public DotNetParameterInfo[] Parameters { get; set; }

	public string Signature { get; set; }
}
