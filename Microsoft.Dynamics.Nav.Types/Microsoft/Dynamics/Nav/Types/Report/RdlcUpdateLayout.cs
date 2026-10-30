using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types.Report;

public static class RdlcUpdateLayout
{
	public static string ApplyFieldRenames(string oldMetadata, string newMetadata, string reportLayout, bool testLayout, bool ignoreDelete)
	{
		try
		{
			using MemoryStream memoryStream = new MemoryStream();
			using MemoryStream memoryStream2 = new MemoryStream();
			StreamWriter streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
			streamWriter.Write(reportLayout);
			streamWriter.Flush();
			memoryStream.Seek(0L, SeekOrigin.Begin);
			IReportChangeLogCollection val = ApplyFieldRenames(oldMetadata, newMetadata, memoryStream, memoryStream2, throwOnErrors: false, ignoreDelete);
			if (testLayout && val != null && val.Failures > 0)
			{
				throw new NavNCLReportDefinitionInvalidException(((object)val).ToString());
			}
			if (memoryStream2.CanRead)
			{
				memoryStream2.Seek(0L, SeekOrigin.Begin);
			}
			if (val != null && val.ImportantChanges == 0)
			{
				return string.Empty;
			}
			return new StreamReader(memoryStream2, Encoding.UTF8).ReadToEnd();
		}
		catch (XmlException)
		{
		}
		return string.Empty;
	}

	public static IReportChangeLogCollection ApplyFieldRenames(string oldMetadata, string newMetadata, Stream reportLayout, Stream reportLayoutUpdate, bool throwOnErrors, bool ignoreDelete)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		IList<ReportChangeRdlc> changeList;
		try
		{
			changeList = GetChangeList(oldMetadata, newMetadata);
		}
		catch (XmlException)
		{
			return (IReportChangeLogCollection)new ReportChangeLogCollection();
		}
		return ApplyFieldRenames(changeList, reportLayout, reportLayoutUpdate, throwOnErrors, ignoreDelete);
	}

	public static IReportChangeLogCollection ApplyFieldRenames(IList<ReportChangeRdlc> changeList, Stream reportLayout, Stream reportLayoutUpdate, bool throwOnErrors, bool ignoreDelete)
	{
		return ApplyFieldRenames(0, string.Empty, changeList, reportLayout, reportLayoutUpdate, throwOnErrors, ignoreDelete);
	}

	public static IReportChangeLogCollection ApplyFieldRenames(int reportId, string layoutName, IList<ReportChangeRdlc> changeList, Stream reportLayout, Stream reportLayoutUpdate, bool throwOnErrors, bool ignoreDelete)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
		try
		{
			if (!changeList.Any())
			{
				CopyStream(reportLayout, reportLayoutUpdate);
				return (IReportChangeLogCollection)new ReportChangeLogCollection();
			}
			RdlcUpdateEngine rdlcUpdateEngine = new RdlcUpdateEngine(reportLayout, throwOnErrors);
			IReportChangeLogCollection val = (IReportChangeLogCollection)(object)rdlcUpdateEngine.DetectConflicts(changeList, ignoreDelete);
			if (val != null && val.Failures > 0)
			{
				CopyStream(reportLayout, reportLayoutUpdate);
				return UpdateLogEntries(reportId, layoutName, val);
			}
			val = rdlcUpdateEngine.Update(changeList);
			if (val.ImportantChanges > 0)
			{
				rdlcUpdateEngine.Document.Save(reportLayoutUpdate);
			}
			else
			{
				CopyStream(reportLayout, reportLayoutUpdate);
			}
			return UpdateLogEntries(reportId, layoutName, val);
		}
		catch (XmlException)
		{
			throw;
		}
	}

	public static IReportChangeLogCollection UpdateLogEntries(int reportId, string layoutName, IReportChangeLogCollection designErrorLog)
	{
		if (designErrorLog == null)
		{
			return null;
		}
		foreach (IReportChangeLog item in (IEnumerable<IReportChangeLog>)designErrorLog)
		{
			if (item.ReportId == 0)
			{
				item.ReportId = reportId;
				item.LayoutName = layoutName;
			}
			item.LayoutFormat = (CustomLayout)1;
		}
		return designErrorLog;
	}

	private static void CopyStream(Stream sourceStream, Stream targetStream)
	{
		if (sourceStream.CanSeek)
		{
			sourceStream.Seek(0L, SeekOrigin.Begin);
		}
		sourceStream.CopyTo(targetStream);
	}

	private static IList<ReportChangeRdlc> GetChangeList(string oldMetadata, string newMetadata)
	{
		if (string.IsNullOrEmpty(oldMetadata) || string.IsNullOrEmpty(newMetadata))
		{
			return new List<ReportChangeRdlc>();
		}
		XElement oldXml = XElement.Parse(oldMetadata);
		XElement newXml = XElement.Parse(newMetadata);
		return RdlcMetadataDiff.GetChangedRdlcRenameList(oldXml, newXml);
	}
}
