using System.Xml.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavXmlDeclaration : NavXmlNodeGeneric<XDeclaration>
{
	public new static NavXmlDeclaration Default => new NavXmlDeclaration(null);

	public string ALEncoding
	{
		get
		{
			return Value.Encoding;
		}
		set
		{
			Value.Encoding = value;
		}
	}

	public string ALStandalone
	{
		get
		{
			return Value.Standalone;
		}
		set
		{
			Value.Standalone = value;
		}
	}

	public string ALVersion
	{
		get
		{
			return Value.Version;
		}
		set
		{
			Value.Version = value;
		}
	}

	public override NavNclType NclType => NavNclType.NavXmlDeclaration;

	public static NavXmlDeclaration ALCreate(string version, string encoding, string standalone)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(() => new NavXmlDeclaration(new XDeclaration(version, encoding, standalone)));
	}

	internal NavXmlDeclaration(XDeclaration value)
		: base(value)
	{
	}
}
