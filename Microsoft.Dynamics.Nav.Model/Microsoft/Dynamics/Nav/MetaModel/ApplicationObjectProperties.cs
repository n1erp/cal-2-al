using System;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.MetaModel;

[DebuggerDisplay("{Date} {Time} {Modified} {VersionList}")]
public sealed class ApplicationObjectProperties : IEquatable<ApplicationObjectProperties>
{
	public string Date { get; set; }

	public string Time { get; set; }

	public string VersionList { get; set; }

	public string Modified { get; set; }

	public ApplicationObjectProperties()
	{
		Clear();
	}

	public void CopyValuesFrom(ApplicationObjectProperties other)
	{
		if (other == null)
		{
			Clear();
			return;
		}
		Date = other.Date;
		Time = other.Time;
		VersionList = other.VersionList;
		Modified = other.Modified;
	}

	public void Clear()
	{
		Date = string.Empty;
		Time = string.Empty;
		VersionList = string.Empty;
		Modified = string.Empty;
	}

	public bool Equals(ApplicationObjectProperties other)
	{
		if (other == null)
		{
			return false;
		}
		if (this != other)
		{
			if (Time == other.Time && Date == other.Date && VersionList == other.VersionList)
			{
				return Modified == other.Modified;
			}
			return false;
		}
		return true;
	}
}
