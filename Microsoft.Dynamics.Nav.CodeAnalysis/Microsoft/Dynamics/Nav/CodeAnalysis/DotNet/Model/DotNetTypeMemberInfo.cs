using System;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;

[Serializable]
[DebuggerDisplay("{Name}")]
public abstract class DotNetTypeMemberInfo
{
	public string Name { get; set; }

	public bool IsVisibleInControlAddIns { get; set; }
}
