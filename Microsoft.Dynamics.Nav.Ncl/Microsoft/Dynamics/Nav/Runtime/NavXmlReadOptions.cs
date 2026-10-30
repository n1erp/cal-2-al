using System.Xml.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavXmlReadOptions : NavValue
{
	public static NavXmlReadOptions Default => new NavXmlReadOptions();

	public bool ALPreserveWhitespace
	{
		get
		{
			return (LoadOptions & LoadOptions.PreserveWhitespace) != 0;
		}
		set
		{
			if (value)
			{
				LoadOptions |= LoadOptions.PreserveWhitespace;
			}
			else
			{
				LoadOptions ^= LoadOptions.PreserveWhitespace;
			}
		}
	}

	internal LoadOptions LoadOptions { get; private set; }

	public override NavNclType NclType { get; } = NavNclType.NavXmlReadOptions;

	public override int GetBytesSize { get; }

	public override bool IsZeroOrEmpty { get; }

	public static NavXmlReadOptions CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is NavXmlReadOptions result)
		{
			return result;
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavXmlReadOptions), value);
	}

	public override int GetHashCode()
	{
		return LoadOptions.GetHashCode();
	}
}
