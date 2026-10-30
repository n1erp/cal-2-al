using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Policy;
using Microsoft.Dynamics.Nav.Types.Data;
using Microsoft.Reporting.WinForms;

namespace Microsoft.Dynamics.Nav.Types;

public interface ILocalReportHandle : IDisposable
{
	byte[] ReportLayoutBytes { get; }

	MemoryStream ReportLayout { get; }

	bool EnableHyperlinks { get; }

	bool EnableExternalImages { get; }

	bool EnableExternalAssemblies { get; }

	bool EnableReportViewerInternalAppDomainIsolation { get; }

	IList<StrongName> StrongNames { get; }

	void AddDataSource(string name, IList<NavDataRow> dataSource);

	void AddDataSource(string name, NavDataRowCollection dataSource);

	void AddDataSource(string name, DataTable dataSource);

	void SetReportParameters(KeyValuePair<string, string>[] reportParameters);

	ReportPageSettings GetDefaultPageSettings();

	string[] Render(string format, string deviceInfo, CreateStreamCallback createStream);

	string[] Render(string format, string deviceInfo, out MemoryStream[] streams);

	byte[] Render(string format, string deviceInfo, out string mimeType, out string encoding, out string fileNameExtension, out string[] streams, out string[] errors);

	void ClearDataSources();

	void AddReference();
}
