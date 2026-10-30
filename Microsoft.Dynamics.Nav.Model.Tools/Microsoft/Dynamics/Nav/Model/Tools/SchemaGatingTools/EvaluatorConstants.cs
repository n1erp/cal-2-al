using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Model.Tools.SchemaGatingTools;

internal class EvaluatorConstants
{
	public static readonly string[] ParamSplitString = new string[1] { "," };

	internal static readonly string DataTypePropertyName = "Data type";

	internal static readonly string DataLengthName = "DataLength";

	internal static readonly List<string> subElementsWithCheckedProperties = new List<string> { "Field", "Key" };

	internal static readonly List<string> objectVersions = new List<string> { "Before", "After" };
}
