using System.Text;
using Microsoft.Dynamics.Nav.Model.IO;
using Microsoft.Dynamics.Nav.Model.IO.Txt;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.Tools;

internal class ReadWriteModelToolOptions
{
	private static readonly ReadWriteModelToolOptions defaultOptions = new ReadWriteModelToolOptions();

	internal static ReadWriteModelToolOptions Default => defaultOptions;

	internal bool RunInParallel { get; set; }

	internal bool DevBaseCompatibility { get; set; }

	internal bool OverwriteExistingFiles { get; set; }

	internal bool IncludeInvalidObjects { get; set; }

	internal ReadWriteModelToolOptions()
	{
		RunInParallel = true;
	}

	private static Encoding GetEncoding()
	{
		return OemEncoding.Instance;
	}

	internal TxtExportOptions GetTxtExportOptions()
	{
		return new TxtExportOptions
		{
			DevBaseCompatibility = DevBaseCompatibility,
			Encoding = GetEncoding()
		};
	}

	internal virtual IApplicationObjectExporter GetApplicationObjectExporter()
	{
		return new TxtExporter(TxtFileModelInfo.Instance, GetTxtExportOptions());
	}

	internal TxtImportOptions GetTxtImportOptions()
	{
		return new TxtImportOptions
		{
			DevBaseCompatibility = DevBaseCompatibility,
			Encoding = GetEncoding(),
			IncludeInvalidObjects = IncludeInvalidObjects
		};
	}
}
