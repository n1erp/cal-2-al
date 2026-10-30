using System;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Types;

public static class ObjectOptionsHelper
{
	public const string ObjectOptionsDataColumnName = "ObjectOptions";

	public const string ObjectOptionsSourceExpression = "ParameterSet";

	public const string IsLatestSavedSettings = "IsLatestSavedSettings";

	public const string SavedSettingsGroupControlGuid = "{47328C3B-4E38-478E-8907-C188351EF109}";

	public const string SavedSettingsNameControlGuid = "{47889756-5016-46B4-8A54-C958239E96C8}";

	public const string ClearFilterActionGuid = "{D57E55A5-99AE-44c5-AA69-8C1F489C5CA2}";

	public static int ReportSettingsLookupForm = Convert.ToInt32("1560", CultureInfo.InvariantCulture);
}
