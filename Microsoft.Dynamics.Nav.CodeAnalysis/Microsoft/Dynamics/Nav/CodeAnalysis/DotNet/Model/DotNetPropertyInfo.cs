using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;

[Serializable]
public sealed class DotNetPropertyInfo : DotNetTypeMemberInfo
{
	public DotNetMethodInfo Getter { get; set; }

	public DotNetMethodInfo Setter { get; set; }

	public string Signature { get; set; }
}
