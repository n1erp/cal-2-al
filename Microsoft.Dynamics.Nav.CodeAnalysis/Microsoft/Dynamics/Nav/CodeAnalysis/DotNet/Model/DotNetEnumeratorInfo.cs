using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;

[Serializable]
public sealed class DotNetEnumeratorInfo : DotNetTypeMemberInfo
{
	public long Ordinal { get; set; }
}
