using System.Xml.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavXmlWriteOptions : NavValue
{
	public static NavXmlWriteOptions Default => new NavXmlWriteOptions();

	public bool ALPreserveWhitespace
	{
		get
		{
			return (SaveOptions & SaveOptions.DisableFormatting) != 0;
		}
		set
		{
			if (value)
			{
				SaveOptions |= SaveOptions.DisableFormatting;
			}
			else
			{
				SaveOptions ^= SaveOptions.DisableFormatting;
			}
		}
	}

	internal SaveOptions SaveOptions { get; private set; }

	public override NavNclType NclType { get; } = NavNclType.NavXmlWriteOptions;

	public override int GetBytesSize { get; }

	public override bool IsZeroOrEmpty { get; }

	public static NavXmlWriteOptions CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is NavXmlWriteOptions result)
		{
			return result;
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavXmlWriteOptions), value);
	}

	public override int GetHashCode()
	{
		return SaveOptions.GetHashCode();
	}
}
