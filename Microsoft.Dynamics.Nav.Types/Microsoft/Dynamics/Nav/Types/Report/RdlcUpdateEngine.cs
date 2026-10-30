using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Report;

internal class RdlcUpdateEngine
{
	private readonly RdlcDocument document;

	public RdlcDocument Document => document;

	public RdlcUpdateEngine(string reportLayout, bool throwOnErrors)
	{
		document = new RdlcDocument(reportLayout, throwOnErrors);
	}

	public RdlcUpdateEngine(Stream reportLayout, bool throwOnErrors)
	{
		document = new RdlcDocument(reportLayout, throwOnErrors);
	}

	public IReportChangeLogCollection Update(IList<ReportChangeRdlc> changeList)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected Obj, but got Unknown
		ReportChangeLogCollection val = new ReportChangeLogCollection();
		HandleUpdatesWithDelete(changeList, (IReportChangeLogCollection)(object)val);
		HandleUpdatesWithRename(changeList, (IReportChangeLogCollection)(object)val);
		return (IReportChangeLogCollection)(object)val;
	}

	internal ReportChangeLogCollection DetectConflicts(IList<ReportChangeRdlc> changeList, bool ignoreDelete)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected Obj, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected Obj, but got Unknown
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Expected Obj, but got Unknown
		List<ReportChangeRdlc> fieldRenameList = changeList.ToList();
		ReportChangeLogCollection val = new ReportChangeLogCollection();
		foreach (ReportChangeRdlc item in fieldRenameList.Where((ReportChangeRdlc rdlcRenameEntry) =>
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			return Document.IsDefined(((ReportChange)rdlcRenameEntry).ItemType, ((ReportChange)rdlcRenameEntry).TargetName) && fieldRenameList.All((ReportChangeRdlc rename2) => ((ReportChange)rename2).SourceName != ((ReportChange)rdlcRenameEntry).TargetName);
		}))
		{
			val.Add((IReportChangeLog)new ReportChangeLog((UpgradeStatus)5, ((IReportChange)item).SourceName, string.Format(CultureInfo.CurrentCulture, Lang.ReportLayoutRenameToExisting, ((IReportChange)item).SourceName, ((IReportChange)item).TargetName)));
		}
		if (!ignoreDelete)
		{
			foreach (ReportChangeRdlc item2 in fieldRenameList.Where((ReportChangeRdlc rdlcRenameEntry) =>
			{
				//IL_0009: Unknown result type (might be due to invalid IL or missing references)
				//IL_001c: Unknown result type (might be due to invalid IL or missing references)
				return rdlcRenameEntry.IsDeleteAction() && (int)((ReportChange)rdlcRenameEntry).ActionOnDelete == 0 && Document.IsReferenceed(((ReportChange)rdlcRenameEntry).ItemType, ((ReportChange)rdlcRenameEntry).SourceName);
			}))
			{
				val.Add((IReportChangeLog)new ReportChangeLog((UpgradeStatus)5, ((IReportChange)item2).SourceName, string.Format(CultureInfo.CurrentCulture, Lang.ReportLayoutDeletedUsedField, ((IReportChange)item2).SourceName)));
			}
		}
		List<ReportChangeRdlc> source = (from rename in fieldRenameList
			from rename3 in from reportChangeRdlc in fieldRenameList
				where ((ReportChange)reportChangeRdlc).Id != ((ReportChange)rename).Id
				select reportChangeRdlc
			where ((ReportChange)rename3).SourceName == ((ReportChange)rename).SourceName || ((ReportChange)rename3).TargetName == ((ReportChange)rename).TargetName
			select rename).ToList();
		if (source.Any())
		{
			val.AddRange((IEnumerable<IReportChangeLog>)((IEnumerable<ReportChangeRdlc>)source).Select((Func<ReportChangeRdlc, ReportChangeLog>)((ReportChangeRdlc changeRdlc) =>
			{
				//IL_0022: Unknown result type (might be due to invalid IL or missing references)
				//IL_0028: Expected Obj, but got Unknown
				return new ReportChangeLog((UpgradeStatus)5, ((ReportChange)changeRdlc).SourceName, string.Format(CultureInfo.CurrentCulture, Lang.ReportLayoutRenameClash, ((ReportChange)changeRdlc).SourceName, ((ReportChange)changeRdlc).TargetName));
			})));
		}
		return val;
	}

	private void HandleUpdatesWithRename(ICollection<ReportChangeRdlc> changeList, IReportChangeLogCollection logCollection)
	{
		IList<ReportChangeRdlc> list = RemoveConflictingChangesFromList(changeList);
		try
		{
			if (changeList.Any())
			{
				foreach (ReportChangeRdlc change in changeList)
				{
					logCollection.AddRange(RenameItem((IReportChange)(object)change));
				}
			}
			if (list.Any())
			{
				SwapEntities(logCollection, list);
			}
		}
		catch (XmlException)
		{
		}
	}

	private void SwapEntities(IReportChangeLogCollection logCollection, IList<ReportChangeRdlc> confList)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected Obj, but got Unknown
		if (!RenameAllowed((IEnumerable<IReportChange>)confList))
		{
			List<string> values = confList.Select((ReportChangeRdlc change) => ((ReportChange)change).SourceName).ToList();
			logCollection.Add((IReportChangeLog)new ReportChangeLog((UpgradeStatus)5, string.Empty, string.Format(CultureInfo.CurrentCulture, Lang.ReportConflictingChange, string.Join(",", values))));
			return;
		}
		foreach (ReportChangeRdlc conf in confList)
		{
			SwapEntity((IReportChange)(object)conf, toToken: true);
		}
		foreach (ReportChangeRdlc conf2 in confList)
		{
			logCollection.AddRange((IEnumerable<IReportChangeLog>)SwapEntity((IReportChange)(object)conf2, toToken: false));
		}
	}

	private bool RenameAllowed(IEnumerable<IReportChange> changes)
	{
		List<IReportChange> source = changes.ToList();
		List<IReportChange> list = source.Where((IReportChange change) =>
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			return (int)change.ItemType == 0;
		}).ToList();
		if (list.Count > 0 && DetectChangeProblem(list, (ElementType)0))
		{
			return false;
		}
		if (list.Count == source.Count())
		{
			return true;
		}
		List<IReportChange> list2 = source.Where((IReportChange change) =>
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			return (int)change.ItemType == 2;
		}).ToList();
		if (list2.Count <= 0)
		{
			return true;
		}
		return !DetectChangeProblem(list2, (ElementType)2);
	}

	private bool DetectChangeProblem(List<IReportChange> changeList, ElementType type)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		bool flag = changeList.Any((IReportChange rename) =>
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			return Document.IsDefined(type, rename.TargetName) && changeList.All((IReportChange val) => val.SourceName != rename.TargetName);
		});
		bool flag2 = (from rename in changeList
			from rename2 in from val in changeList
				where val.Id != rename.Id
				select val
			where rename2.SourceName == rename.SourceName || rename2.TargetName == rename.TargetName
			select rename).Any();
		return flag | flag2;
	}

	private IEnumerable<IReportChangeLog> RenameItem(IReportChange change)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return (IEnumerable<IReportChangeLog>)Document.Rename(change.ItemType, change.SourceName, change.TargetName);
	}

	private void HandleUpdatesWithDelete(ICollection<ReportChangeRdlc> changeList, IReportChangeLogCollection reportChangeLogCollection)
	{
		List<ReportChangeRdlc> list = changeList.Where((ReportChangeRdlc change) => change.IsDeleteAction()).ToList();
		foreach (ReportChangeRdlc item in list)
		{
			changeList.Remove(item);
		}
		foreach (ReportChangeRdlc item2 in list.Where((ReportChangeRdlc changeElement) =>
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			return (int)((ReportChange)changeElement).ItemType != 1;
		}))
		{
			reportChangeLogCollection.AddRange(DeleteItem((IReportChange)(object)item2));
		}
	}

	private IEnumerable<IReportChangeLog> DeleteItem(IReportChange change)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		return (IEnumerable<IReportChangeLog>)Document.Delete(change.ItemType, change.SourceName, change.ActionOnDelete, change.ReplacementText);
	}

	private IList<ReportChangeRdlc> RemoveConflictingChangesFromList(ICollection<ReportChangeRdlc> changeList)
	{
		List<ReportChangeRdlc> list = changeList.Where((ReportChangeRdlc change) =>
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			return Document.IsDefined(((ReportChange)change).ItemType, ((ReportChange)change).TargetName) && changeList.Any((ReportChangeRdlc rename2) => string.CompareOrdinal(((ReportChange)rename2).SourceName, ((ReportChange)change).TargetName) == 0);
		}).ToList();
		foreach (ReportChangeRdlc item in list)
		{
			changeList.Remove(item);
		}
		return list;
	}

	private IReportChangeLogCollection SwapEntity(IReportChange change, bool toToken)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		IReportChangeLogCollection val = null;
		string text = "###" + change.Id + "###";
		if (toToken)
		{
			Document.Rename(change.ItemType, change.SourceName, text);
		}
		else
		{
			val = Document.Rename(change.ItemType, text, change.TargetName);
			UpdateLogFieldNames((IEnumerable<IReportChangeLog>)val, text, change.SourceName);
		}
		return val;
	}

	private static void UpdateLogFieldNames(IEnumerable<IReportChangeLog> logs, string token, string sourceName)
	{
		foreach (ReportChangeLog item in logs.Where((IReportChangeLog logItem) => logItem.ElementName.Equals(token)).Cast<ReportChangeLog>())
		{
			item.ElementName = sourceName;
			item.Message = item.Message.Replace(token, sourceName);
		}
	}
}
