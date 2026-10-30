using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Data;
using Microsoft.Reporting.WinForms;

namespace Microsoft.Dynamics.Nav.Runtime;

public class ReportSaveAsXmlRenderer : SaveAsXmlRenderer
{
	internal delegate KeyValuePair<string, string>[] GetReportParameters();

	private readonly string parametersFileName;

	private readonly GetReportParameters reportParameters;

	internal ReportSaveAsXmlRenderer(string parametersFileName, GetReportParameters reportParameters, string dataSetFileName, NavDataSet dataSet)
		: base(dataSet, dataSetFileName)
	{
		this.parametersFileName = parametersFileName;
		this.reportParameters = reportParameters;
	}

	public override void Start()
	{
		SerializeReportParameters();
		base.Start();
	}

	private void SerializeReportParameters()
	{
		try
		{
			using XmlWriter xmlWriter = new XmlTextWriter(parametersFileName, Encoding.UTF8)
			{
				Formatting = Formatting.Indented
			};
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ReportParameter[]));
			xmlSerializer.Serialize(xmlWriter, ((IEnumerable<KeyValuePair<string, string>>)reportParameters()).Select((Func<KeyValuePair<string, string>, ReportParameter>)((KeyValuePair<string, string> reportParameter) =>
			{
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0014: Expected Obj, but got Unknown
				return new ReportParameter(reportParameter.Key, reportParameter.Value);
			})).ToArray());
		}
		catch (Exception ex)
		{
			NavFileHelper.MapAndRethrowClrException(ex, writeExceptionToEventLog: true);
			throw;
		}
	}
}
