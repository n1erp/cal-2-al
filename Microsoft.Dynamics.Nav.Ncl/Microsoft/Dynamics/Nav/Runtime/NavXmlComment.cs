using System.Xml.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavXmlComment : NavXmlNodeGeneric<XComment>
{
	public new static NavXmlComment Default => new NavXmlComment(null);

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

	public override NavNclType NclType => NavNclType.NavXmlComment;

	public static NavXmlComment ALCreate(string value)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(() => new NavXmlComment(new XComment(value)));
	}

	internal NavXmlComment(XComment value)
		: base(value)
	{
	}
}
