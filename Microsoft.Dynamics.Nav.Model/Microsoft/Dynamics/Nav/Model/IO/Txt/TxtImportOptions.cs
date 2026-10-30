using System;
using System.Text;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.IO.Txt;

public class TxtImportOptions
{
	private Encoding encoding;

	public bool DevBaseCompatibility { get; set; }

	public Encoding Encoding
	{
		get
		{
			return encoding ?? OemEncoding.Instance;
		}
		set
		{
			encoding = value;
		}
	}

	public bool IncludeInvalidObjects { get; set; }

	public Action<long, long> ProgressAction { get; set; }

	public Action<int, int, ElementType, int, string> WarningAction { get; set; }
}
