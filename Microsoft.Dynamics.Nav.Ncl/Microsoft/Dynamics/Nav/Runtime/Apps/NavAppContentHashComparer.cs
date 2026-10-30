using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppContentHashComparer
{
	private readonly Dictionary<Tuple<PackageId, NavAppHashAlgorithm>, string> cachedHashes = new Dictionary<Tuple<PackageId, NavAppHashAlgorithm>, string>();

	private readonly Func<PackageId, Stream> getPackageStream;

	private readonly NavAppHashAlgorithm latestHashAlgorithm;

	public NavAppContentHashComparer(Func<PackageId, Stream> getPackageStream, NavAppHashAlgorithm latestHashAlgorithm = (NavAppHashAlgorithm)1)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (getPackageStream == null)
		{
			throw new ArgumentNullException("getPackageStream");
		}
		this.getPackageStream = getPackageStream;
		this.latestHashAlgorithm = latestHashAlgorithm;
	}

	public bool CompareContentHashesForAppFromTenant(NavAppRuntimeMetadata publishedApp, string applicationContentHash, string tenantContentHash, NavAppHashAlgorithm tenantHashAlgorithm)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if ((int)tenantHashAlgorithm == 0)
		{
			return false;
		}
		string a = applicationContentHash;
		if (tenantHashAlgorithm != latestHashAlgorithm)
		{
			Tuple<PackageId, NavAppHashAlgorithm> key = Tuple.Create<PackageId, NavAppHashAlgorithm>(publishedApp.PackageId, tenantHashAlgorithm);
			if (!cachedHashes.ContainsKey(key))
			{
				using Stream stream = getPackageStream(publishedApp.PackageId);
				NavAppPackageReader reader = NavAppPackageReader.Create(stream);
				cachedHashes[key] = RecomputeV2ExtensionHash(reader, tenantHashAlgorithm);
			}
			a = cachedHashes[key];
		}
		return string.Equals(a, tenantContentHash, StringComparison.OrdinalIgnoreCase);
	}

	private static string RecomputeV2ExtensionHash(NavAppPackageReader reader, NavAppHashAlgorithm algorithm)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		List<byte[]> list = new List<byte[]>();
		foreach (string item in reader.ReadSourceContents())
		{
			list.Add(Encoding.Unicode.GetBytes(item));
		}
		return NavAppContentHasher.ComputeHash((IEnumerable<byte[]>)list, algorithm);
	}
}
