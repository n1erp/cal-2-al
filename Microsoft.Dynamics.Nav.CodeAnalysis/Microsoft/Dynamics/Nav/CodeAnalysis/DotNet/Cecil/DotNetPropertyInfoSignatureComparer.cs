using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Cecil;

internal class DotNetPropertyInfoSignatureComparer : IComparer<DotNetPropertyInfo>
{
	public static IComparer<DotNetPropertyInfo> Instance { get; } = new DotNetPropertyInfoSignatureComparer();

	public int Compare(DotNetPropertyInfo x, DotNetPropertyInfo y)
	{
		if (x == null && y == null)
		{
			return 0;
		}
		return string.CompareOrdinal(x.Signature, y.Signature);
	}
}
