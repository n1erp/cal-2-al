using System.Globalization;
using System.Xml;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

public static class XmlTypeHelper
{
	public static XmlQualifiedName XmlTypeNameFrom(NavType type)
	{
		SimpleTypesSupportedInWebservice(type);
		string name;
		switch (type)
		{
		case NavType.Byte:
			name = "unsignedByte";
			break;
		case NavType.Char:
			name = "char";
			break;
		case NavType.TableFilter:
		case NavType.RecordID:
		case NavType.OemText:
		case NavType.DateFormula:
		case NavType.TextConst:
		case NavType.Text:
		case NavType.Code:
		case NavType.OemCode:
		case NavType.Option:
		case NavType.GUID:
		case NavType.BigText:
		case NavType.String:
			name = "string";
			break;
		case NavType.Integer:
			name = "int";
			break;
		case NavType.BigInteger:
			name = "long";
			break;
		case NavType.Boolean:
			name = "boolean";
			break;
		case NavType.DateTime:
			name = "dateTime";
			break;
		case NavType.Date:
			name = "date";
			break;
		case NavType.Time:
			name = "time";
			break;
		case NavType.Decimal:
			name = "decimal";
			break;
		case NavType.Duration:
			name = "duration";
			break;
		default:
			throw NavNCLNotSupportedTypeException.CreateUnsupportedType(CultureInfo.CurrentCulture, type.ToString());
		}
		return new XmlQualifiedName(name, "http://www.w3.org/2001/XMLSchema");
	}

	public static XmlQualifiedName ClassicXmlPortXmlTypeNameFrom(NavType type)
	{
		SimpleTypesSupportedInWebservice(type);
		string name;
		switch (type)
		{
		case NavType.Byte:
			name = "byte";
			break;
		case NavType.Char:
			name = "char";
			break;
		case NavType.TableFilter:
		case NavType.OemText:
		case NavType.Date:
		case NavType.Time:
		case NavType.DateFormula:
		case NavType.Decimal:
		case NavType.Text:
		case NavType.Code:
		case NavType.Boolean:
		case NavType.OemCode:
		case NavType.Option:
		case NavType.Duration:
		case NavType.GUID:
		case NavType.DateTime:
		case NavType.BigText:
		case NavType.String:
			name = "string";
			break;
		case NavType.Integer:
			name = "int";
			break;
		case NavType.BigInteger:
			name = "long";
			break;
		default:
			throw NavNCLNotSupportedTypeException.CreateUnsupportedType(CultureInfo.CurrentCulture, type.ToString());
		}
		return new XmlQualifiedName(name, "http://www.w3.org/2001/XMLSchema");
	}

	public static bool SimpleTypesSupportedInWebservice(NavType navType)
	{
		switch (navType)
		{
		case NavType.TableFilter:
		case NavType.RecordID:
		case NavType.OemText:
		case NavType.Date:
		case NavType.Time:
		case NavType.DateFormula:
		case NavType.Decimal:
		case NavType.TextConst:
		case NavType.Media:
		case NavType.MediaSet:
		case NavType.Char:
		case NavType.Text:
		case NavType.Code:
		case NavType.BLOB:
		case NavType.Boolean:
		case NavType.Integer:
		case NavType.Byte:
		case NavType.OemCode:
		case NavType.Option:
		case NavType.BigInteger:
		case NavType.Duration:
		case NavType.GUID:
		case NavType.DateTime:
		case NavType.BigText:
		case NavType.String:
			return true;
		default:
			throw NavNCLNotSupportedTypeException.CreateUnsupportedType(CultureInfo.CurrentCulture, navType.ToString());
		}
	}

	public static string XmlDefaultValueFromNavType(NavType navType)
	{
		switch (navType)
		{
		case NavType.Media:
		case NavType.MediaSet:
		case NavType.GUID:
			return "{00000000-0000-0000-0000-000000000000}";
		case NavType.DateTime:
			return "0001-01-01T00:00:00";
		case NavType.Date:
			return "0001-01-01";
		case NavType.Time:
			return "00:00:00";
		case NavType.Decimal:
		case NavType.Integer:
		case NavType.BigInteger:
			return "0";
		case NavType.Boolean:
			return "false";
		case NavType.Duration:
			return "P0D";
		default:
			return null;
		}
	}

	public static string ClassicXmlPortXmlDefaultValueFromNavType(NavType navType)
	{
		if (navType == NavType.Integer || navType == NavType.BigInteger)
		{
			return "0";
		}
		return null;
	}
}
