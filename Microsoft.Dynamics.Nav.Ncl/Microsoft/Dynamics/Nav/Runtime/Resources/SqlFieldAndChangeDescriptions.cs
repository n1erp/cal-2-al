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
internal class SqlFieldAndChangeDescriptions
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
				ResourceManager resourceManager = new ResourceManager("Microsoft.Dynamics.Nav.Runtime.Resources.SqlFieldAndChangeDescriptions", typeof(SqlFieldAndChangeDescriptions).Assembly);
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

	internal static string AutoIncrement => ResourceManager.GetString("AutoIncrement", resourceCulture);

	internal static string Compression => ResourceManager.GetString("Compression", resourceCulture);

	internal static string DataType => ResourceManager.GetString("DataType", resourceCulture);

	internal static string Excessive => ResourceManager.GetString("Excessive", resourceCulture);

	internal static string Length => ResourceManager.GetString("Length", resourceCulture);

	internal static string Missing => ResourceManager.GetString("Missing", resourceCulture);

	internal static string Name => ResourceManager.GetString("Name", resourceCulture);

	internal static string None => ResourceManager.GetString("None", resourceCulture);

	internal static string Sorting => ResourceManager.GetString("Sorting", resourceCulture);

	internal static string SqlDataType => ResourceManager.GetString("SqlDataType", resourceCulture);

	internal SqlFieldAndChangeDescriptions()
	{
	}
}
