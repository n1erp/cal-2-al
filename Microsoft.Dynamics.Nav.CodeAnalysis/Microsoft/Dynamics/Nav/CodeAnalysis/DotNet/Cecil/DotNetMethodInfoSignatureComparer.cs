using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Cecil;

internal class DotNetMethodInfoSignatureComparer : IComparer<DotNetMethodInfo>
{
	public static IComparer<DotNetMethodInfo> Instance { get; } = new DotNetMethodInfoSignatureComparer();

	public int Compare(DotNetMethodInfo x, DotNetMethodInfo y)
	{
		return string.CompareOrdinal(x.Signature, y.Signature);
	}
}
