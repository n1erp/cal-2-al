using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class VersionCompatibility
{
	private static readonly ConcurrentDictionary<string, VersionCompatibility> cache;

	private readonly IEnumerable<VersionRange> ranges;

	public static readonly VersionCompatibility Spring2019OrGreater;

	public static readonly VersionCompatibility LowerThanSpring2019;

	public static readonly VersionCompatibility Fall2018CU2OrGreater;

	public static readonly VersionCompatibility Fall2018CU4OrGreater;

	static VersionCompatibility()
	{
		cache = new ConcurrentDictionary<string, VersionCompatibility>();
		Spring2019OrGreater = new VersionCompatibility(new VersionRange[1]
		{
			new VersionRange(RuntimeVersion.Spring2019)
		});
		LowerThanSpring2019 = new VersionCompatibility(new VersionRange[1]
		{
			new VersionRange(RuntimeVersion.Spring2018, RuntimeVersion.Spring2019, strictUpperBound: true)
		});
		Fall2018CU2OrGreater = new VersionCompatibility(new VersionRange[1]
		{
			new VersionRange(RuntimeVersion.Fall2018CU2)
		});
		Fall2018CU4OrGreater = new VersionCompatibility(new VersionRange[1]
		{
			new VersionRange(RuntimeVersion.Fall2018CU4)
		});
	}

	public VersionCompatibility(IEnumerable<VersionRange> ranges)
	{
		if (ranges == null)
		{
			throw new ArgumentNullException("ranges");
		}
		this.ranges = ranges;
	}

	public bool IsSupported(Version target)
	{
		foreach (VersionRange range in ranges)
		{
			if (range.Contains(target))
			{
				return true;
			}
		}
		return false;
	}

	public override string ToString()
	{
		return string.Join(", ", ranges.Select((VersionRange x) => x.ToString()));
	}

	public static VersionCompatibility Parse(string str)
	{
		return cache.GetOrAdd(str, ParseInternal);
	}

	private static VersionCompatibility ParseInternal(string str)
	{
		return new VersionCompatibility(from x in str.Split(',')
			select VersionRange.Parse(x));
	}
}
