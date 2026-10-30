using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSystemCodeunitReportingTriggers : NavSystemCodeunit
{
	internal enum CodeunitMethod
	{
		GetPrinterName = 1,
		GetPaperTrayForReport,
		HasCustomLayout,
		MergeDocument,
		ReportGetCustomRdlc,
		ScheduleReport,
		SubstituteReport
	}

	private static readonly NCLOptionMetadata hasCustomLayoutOptionsMetadata = NCLOptionMetadata.Create("None,RDLC,Word");

	private static readonly NCLOptionMetadata reportRunModeOptionsMetadata = NCLOptionMetadata.Create("Normal,ParametersOnly,Execute,Print,SaveAs,RunModal");

	public NavSystemCodeunitReportingTriggers(TreeObject parent)
		: base(parent, 2000000005)
	{
	}

	protected override string GetMethodName(int methodId)
	{
		CodeunitMethod codeunitMethod = (CodeunitMethod)methodId;
		return codeunitMethod.ToString();
	}

	public NavText InvokeGetPrinterName(int reportId)
	{
		NavText result = NavText.Default(int.MaxValue);
		Invoke(1, new object[2]
		{
			reportId,
			new ByRef<NavText>(() => result, (NavText value) =>
			{
				result = value;
			})
		});
		return result;
	}

	public void InvokeGetPaperTrayForReport(int reportId, ref int firstPage, ref int defaultPage, ref int lastPage)
	{
		int firstPageTray = firstPage;
		int defaultPageTray = defaultPage;
		int lastPageTray = lastPage;
		object[] arguments = new object[4]
		{
			reportId,
			new ByRef<int>(() => firstPageTray, (int value) =>
			{
				firstPageTray = value;
			}),
			new ByRef<int>(() => defaultPageTray, (int value) =>
			{
				defaultPageTray = value;
			}),
			new ByRef<int>(() => lastPageTray, (int value) =>
			{
				lastPageTray = value;
			})
		};
		Invoke(2, arguments);
		firstPage = firstPageTray;
		defaultPage = defaultPageTray;
		lastPage = lastPageTray;
	}

	public CustomLayout InvokeHasCustomLayout(NavOption objectType, int objectId)
	{
		NavOption layoutType = NavOption.Create(hasCustomLayoutOptionsMetadata, 0);
		object[] args = new object[3]
		{
			objectType,
			objectId,
			new ByRef<NavOption>(() => layoutType, (NavOption value) =>
			{
				layoutType = value;
			})
		};
		NavCurrentThread.Session.TransactionScope(() =>
		{
			Invoke(3, args);
		});
		int num = layoutType.Value;
		if (num < 0 || num > 2)
		{
			num = 0;
		}
		return (CustomLayout)num;
	}

	public void InvokeMergeDocument(NavOption objectType, int objectId, NavOption action, NavInStream inStream, string fileName)
	{
		object[] args = new object[5] { objectType, objectId, action, inStream, fileName };
		NavCurrentThread.Session.TransactionScope(() =>
		{
			Invoke(4, args);
		});
	}

	public NavText InvokeReportGetCustomRdlc(int reportId)
	{
		NavText result = NavText.Default(0);
		object[] args = new object[2]
		{
			reportId,
			new ByRef<NavText>(() => result, (NavText value) =>
			{
				result = value;
			})
		};
		NavCurrentThread.Session.TransactionScope(() =>
		{
			Invoke(5, args);
		});
		return result;
	}

	internal bool InvokeScheduleReport(int reportId, string requestPageXml)
	{
		bool result = false;
		NavCurrentThread.Session.TransactionScope(() =>
		{
			Invoke(6, new object[3]
			{
				reportId,
				requestPageXml,
				new ByRef<bool>(() => result, (bool value) =>
				{
					result = value;
				})
			});
		});
		return result;
	}

	internal bool InvokeSubstituteReport(int reportId, NavReport.RunMode runMode, string requestPageParameters, NavRecord record, ref int newReportIdToRun)
	{
		int reportIdToRun = -1;
		using NavRecordRef navRecordRef = CreateRecordRefFromRecord(record);
		object[] arguments = new object[5]
		{
			reportId,
			NavOption.Create(reportRunModeOptionsMetadata, (int)runMode),
			new NavText(int.MaxValue, requestPageParameters),
			navRecordRef,
			new ByRef<int>(() => reportIdToRun, (int value) =>
			{
				reportIdToRun = value;
			})
		};
		Invoke(7, arguments);
		if (reportIdToRun != -1 && reportIdToRun != reportId)
		{
			newReportIdToRun = reportIdToRun;
			return true;
		}
		return false;
	}

	private static NavRecordRef CreateRecordRefFromRecord(NavRecord record)
	{
		if (record != null)
		{
			return new NavRecordRef(NavCurrentThread.Session.CurrentMethodScope, record, record.SecurityFiltering);
		}
		return new NavRecordRef(NavCurrentThread.Session.CurrentMethodScope);
	}
}
