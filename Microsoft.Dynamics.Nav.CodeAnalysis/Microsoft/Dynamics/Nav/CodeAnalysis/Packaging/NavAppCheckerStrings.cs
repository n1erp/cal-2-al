using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class NavAppCheckerStrings
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
				resourceMan = new ResourceManager("Microsoft.Dynamics.Nav.CodeAnalysis.Packaging.Checking.NavAppCheckerStrings", typeof(NavAppCheckerStrings).GetTypeInfo().GetTypeInfo().Assembly);
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

	internal static string DuplicatePermissionSetFound => ResourceManager.GetString("DuplicatePermissionSetFound", resourceCulture);

	internal static string DuplicateTranslationLineError => ResourceManager.GetString("DuplicateTranslationLineError", resourceCulture);

	internal static string InvalidTenantWebServiceName => ResourceManager.GetString("InvalidTenantWebServiceName", resourceCulture);

	internal static string InvalidTranslationLineError => ResourceManager.GetString("InvalidTranslationLineError", resourceCulture);

	internal static string NavAppsCannotValidateTableData => ResourceManager.GetString("NavAppsCannotValidateTableData", resourceCulture);

	internal static string NavAppsInvalidTableData => ResourceManager.GetString("NavAppsInvalidTableData", resourceCulture);

	internal static string NoSourcesToVerify => ResourceManager.GetString("NoSourcesToVerify", resourceCulture);

	internal static string SecurityFilterMissingExpressionAfterSeparator => ResourceManager.GetString("SecurityFilterMissingExpressionAfterSeparator", resourceCulture);

	internal static string SecurityFilterObjectNameMissing => ResourceManager.GetString("SecurityFilterObjectNameMissing", resourceCulture);

	internal static string SecurityFilterParseHeader => ResourceManager.GetString("SecurityFilterParseHeader", resourceCulture);

	internal static string SecurityFilterValueMissing => ResourceManager.GetString("SecurityFilterValueMissing", resourceCulture);

	internal static string XmlLoadError => ResourceManager.GetString("XmlLoadError", resourceCulture);

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	internal NavAppCheckerStrings()
	{
	}
}
