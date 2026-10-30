using System.Xml.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavXmlText : NavXmlNodeGeneric<XText>
{
	public new static NavXmlText Default => new NavXmlText(null);

	public string ALValue
	{
		get
		{
			return Value.Value;
		}
		set
		{
			Value.Value = value;
		}
	}

	public override NavNclType NclType => NavNclType.NavXmlText;

	public static NavXmlText ALCreate(string value)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(() => new NavXmlText(new XText(value)));
	}

	internal NavXmlText(XText value)
		: base(value)
	{
	}
}
