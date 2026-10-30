using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;

[Serializable]
public sealed class DotNetConstructorInfo : DotNetTypeMemberInfo
{
	public DotNetParameterInfo[] Parameters { get; set; }
}
