using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Cecil;

internal class DotNetEventInfoSignatureComparer : IComparer<DotNetEventInfo>
{
	public static IComparer<DotNetEventInfo> Instance { get; } = new DotNetEventInfoSignatureComparer();

	public int Compare(DotNetEventInfo x, DotNetEventInfo y)
	{
		return string.CompareOrdinal(GetEventSignature(x), GetEventSignature(y));
	}

	private static string GetEventSignature(DotNetEventInfo x)
	{
		return x.Signature;
	}
}
