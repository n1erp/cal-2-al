using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Dynamics.Nav.Apps.Runtime;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class OrderedAppCombination
{
	private const string BaseGroupStableCombinationId = "BASEGROUP";

	private readonly List<NavAppRuntimeMetadata> appMetadata;

	private readonly Lazy<string> lazyStableId;

	public IReadOnlyList<NavAppRuntimeMetadata> OrderedAppMetadata => appMetadata;

	public string CurrentProcessCombinationId { get; }

	public string StableCombinationId => lazyStableId.Value;

	public OrderedAppCombination(IEnumerable<NavAppRuntimeMetadata> orderedAppMetadata)
	{
		appMetadata = orderedAppMetadata.ToList();
		CurrentProcessCombinationId = string.Join(",", appMetadata.Select((NavAppRuntimeMetadata m) => m.RuntimeAppId));
		lazyStableId = new Lazy<string>(ComputeStableCombinationId);
	}

	private string ComputeStableCombinationId()
	{
		if (!appMetadata.Any())
		{
			return "BASEGROUP";
		}
		using SHA256 sHA = SHA256.Create();
		string s = string.Join("_", appMetadata.Select((NavAppRuntimeMetadata a) => a.PackageId));
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		byte[] bytes2 = sHA.ComputeHash(bytes);
		return ConvertByteArrayToHexString(bytes2);
	}

	private static string ConvertByteArrayToHexString(byte[] bytes)
	{
		StringBuilder stringBuilder = new StringBuilder(bytes.Length * 2);
		foreach (byte b in bytes)
		{
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", b);
		}
		return stringBuilder.ToString();
	}
}
