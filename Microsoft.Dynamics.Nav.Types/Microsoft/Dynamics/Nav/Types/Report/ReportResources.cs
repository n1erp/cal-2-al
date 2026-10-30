using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.Types.Report;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class ReportResources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("Microsoft.Dynamics.Nav.Types.Report.ReportResources", typeof(ReportResources).Assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static byte[] ReportLayoutSln => (byte[])ResourceManager.GetObject("ReportLayoutSln", resourceCulture);

	internal static byte[] ReportRdlc => (byte[])ResourceManager.GetObject("ReportRdlc", resourceCulture);

	internal static string ReportXsd => ResourceManager.GetString("ReportXsd", resourceCulture);

	internal static string WebConfig => ResourceManager.GetString("WebConfig", resourceCulture);

	internal ReportResources()
	{
	}
}
