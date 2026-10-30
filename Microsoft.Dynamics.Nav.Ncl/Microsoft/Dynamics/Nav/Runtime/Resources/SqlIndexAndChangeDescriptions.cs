using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.Runtime.Resources;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class SqlIndexAndChangeDescriptions
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
				ResourceManager resourceManager = new ResourceManager("Microsoft.Dynamics.Nav.Runtime.Resources.SqlIndexAndChangeDescriptions", typeof(SqlIndexAndChangeDescriptions).Assembly);
				resourceMan = resourceManager;
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

	internal static string ClusterIndex => ResourceManager.GetString("ClusterIndex", resourceCulture);

	internal static string Excessive => ResourceManager.GetString("Excessive", resourceCulture);

	internal static string IndexFields => ResourceManager.GetString("IndexFields", resourceCulture);

	internal static string Missing => ResourceManager.GetString("Missing", resourceCulture);

	internal static string None => ResourceManager.GetString("None", resourceCulture);

	internal static string PrimaryKey => ResourceManager.GetString("PrimaryKey", resourceCulture);

	internal static string SiftExcessive => ResourceManager.GetString("SiftExcessive", resourceCulture);

	internal static string SiftMissing => ResourceManager.GetString("SiftMissing", resourceCulture);

	internal static string SumFields => ResourceManager.GetString("SumFields", resourceCulture);

	internal SqlIndexAndChangeDescriptions()
	{
	}
}
