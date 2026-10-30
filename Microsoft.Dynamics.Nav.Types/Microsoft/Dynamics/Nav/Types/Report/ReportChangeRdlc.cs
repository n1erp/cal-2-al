using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Types.Report;

public class ReportChangeRdlc : ReportChange
{
	public ReportChangeRdlc(string id, string sourceName, string targetName, bool isParameter)
		: base(id, sourceName, targetName, string.Empty, string.Empty, (ElementType)(isParameter ? 2 : 0), "None")
	{
	}

	public ReportChangeRdlc(IReportChange reportChange, bool addAsFormat = false)
		: base(reportChange)
	{
		if (addAsFormat)
		{
			((ReportChange)this).SourceName = ((ReportChange)this).SourceName + "Format";
			if (!string.IsNullOrEmpty(((ReportChange)this).TargetName))
			{
				((ReportChange)this).TargetName = ((ReportChange)this).TargetName + "Format";
			}
		}
	}

	public static IList<ReportChangeRdlc> MapChangeList(IEnumerable<IReportChange> changeList)
	{
		List<ReportChangeRdlc> list = new List<ReportChangeRdlc>();
		foreach (IReportChange change in changeList)
		{
			Map(change, list);
		}
		return list;
	}

	public static IList<ReportChangeRdlc> MapChangeList(IDictionary<string, IReportChange> changeList)
	{
		List<ReportChangeRdlc> list = new List<ReportChangeRdlc>();
		foreach (KeyValuePair<string, IReportChange> change in changeList)
		{
			Map(change.Value, list);
		}
		return list;
	}

	private static void Map(IReportChange reportChange, List<ReportChangeRdlc> changes)
	{
		ReportChange val = (ReportChange)(object)((reportChange is ReportChange) ? reportChange : null);
		if (val != null)
		{
			ReportChangeRdlc item = new ReportChangeRdlc((IReportChange)(object)val);
			changes.Add(item);
			if (val.DataType.Equals(NavType.Decimal.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				item = new ReportChangeRdlc((IReportChange)(object)val, addAsFormat: true);
				changes.Add(item);
			}
		}
	}

	internal bool IsDeleteAction()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if ((int)((ReportChange)this).ActionOnChange != 0)
		{
			return string.IsNullOrWhiteSpace(((ReportChange)this).TargetName);
		}
		return true;
	}
}
