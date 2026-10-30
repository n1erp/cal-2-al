using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.Model.Tools.IO.Nea;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppPackageTableDataRetriever
{
	private readonly Lazy<NavAppPackageReader> packageReader;

	private readonly Lazy<IEnumerable<string>> tableDataFilePaths;

	public NavAppPackageTableDataRetriever(PackageId packageId)
	{
		NavAppPackageTableDataRetriever navAppPackageTableDataRetriever = this;
		packageReader = new Lazy<NavAppPackageReader>(() =>
		{
			Stream packageBlob = NavEnvironment.Instance.NavAppMetadataRetriever.GetPackageBlob(packageId);
			try
			{
				return NavAppPackageReader.Create(packageBlob);
			}
			catch (Exception ex)
			{
				string message = string.Format(CultureInfo.InvariantCulture, "Cannot open the package file due to the following error: {0}", ex.Message);
				throw new InvalidOperationException(message, ex);
			}
		});
		tableDataFilePaths = new Lazy<IEnumerable<string>>(() => navAppPackageTableDataRetriever.packageReader.Value.ReadTableDataPaths());
	}

	public Stream GetDataStreamForTable(int tableId)
	{
		string text = tableDataFilePaths.Value.FirstOrDefault((string path) => path.EndsWith("TAB" + tableId + ".navxdata", StringComparison.OrdinalIgnoreCase));
		if (text != null)
		{
			try
			{
				Stream stream = packageReader.Value.ReadFile(text);
				return NeaStreamReader.IsSupported(stream) ? new NeaStreamReader(stream) : stream;
			}
			catch (Exception ex)
			{
				string message = string.Format(CultureInfo.InvariantCulture, "Cannot open the package file due to the following error: {0}", ex.Message);
				throw new InvalidOperationException(message, ex);
			}
		}
		return null;
	}
}
