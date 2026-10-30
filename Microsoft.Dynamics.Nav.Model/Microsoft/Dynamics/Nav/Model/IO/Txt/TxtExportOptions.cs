using System;
using System.Text;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.IO.Txt;

public class TxtExportOptions
{
	private Encoding encoding;

	public bool DevBaseCompatibility { get; set; }

	public Action ProgressAction { get; set; }

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

	public bool ClearObjectProperties { get; set; }
}
