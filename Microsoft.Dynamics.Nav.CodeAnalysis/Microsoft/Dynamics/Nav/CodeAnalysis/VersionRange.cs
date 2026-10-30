using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class VersionRange
{
	public Version From { get; }

	public Version To { get; }

	public bool StrictUpperBound { get; }

	public VersionRange(Version from, Version to = null, bool strictUpperBound = false)
	{
		if (from == null)
		{
			throw new ArgumentNullException("from");
		}
		From = from;
		To = to;
		StrictUpperBound = strictUpperBound;
	}

	public bool Contains(Version target)
	{
		if (target < From)
		{
			return false;
		}
		if (To != null)
		{
			if (StrictUpperBound)
			{
				return target < To;
			}
			return target <= To;
		}
		return true;
	}

	public override string ToString()
	{
		if (To == null)
		{
			return string.Format(CultureInfo.InvariantCulture, CodeAnalysisResources.VersionOrGreater, From.ToMajorMinorString());
		}
		if (StrictUpperBound)
		{
			return string.Format(CultureInfo.InvariantCulture, CodeAnalysisResources.VersionFromToExcluded, From.ToMajorMinorString(), To.ToMajorMinorString());
		}
		return string.Format(CultureInfo.InvariantCulture, CodeAnalysisResources.VersionFromTo, From.ToMajorMinorString(), To.ToMajorMinorString());
	}

	public static VersionRange Parse(string str)
	{
		List<string> list = (from x in str.Split('-')
			select x.Trim()).ToList();
		Version version = null;
		Version to = null;
		if (list.Count == 1)
		{
			version = Version.Parse(list[0]);
		}
		else
		{
			if (list.Count != 2)
			{
				throw new InvalidOperationException("Could not parse version range " + str);
			}
			if (list[0].Length != 0)
			{
				version = Version.Parse(list[0]);
			}
			if (list[1].Length != 0)
			{
				to = Version.Parse(list[1]);
			}
		}
		return new VersionRange(version, to);
	}
}
