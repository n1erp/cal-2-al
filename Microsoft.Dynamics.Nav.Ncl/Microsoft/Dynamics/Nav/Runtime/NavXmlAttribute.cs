using System.Xml.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavXmlAttribute : NavXmlNodeGeneric<XAttribute>
{
	public new static NavXmlAttribute Default => new NavXmlAttribute(null);

	public string ALName => Name;

	public string ALLocalName => Value.Name.LocalName;

	public string ALNamespaceUri => Value.Name.NamespaceName;

	public string ALNamespacePrefix => Value.GetPrefix();

	public bool ALIsNamespaceDeclaration => Value.IsNamespaceDeclaration;

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

	public override NavNclType NclType => NavNclType.NavXmlAttribute;

	private string Name
	{
		get
		{
			if (string.IsNullOrEmpty(Value.Name.NamespaceName))
			{
				return Value.Name.LocalName;
			}
			return Value.GetPrefix() + ":" + Value.Name.LocalName;
		}
	}

	public static NavXmlAttribute ALCreate(string name, string value)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(() => new NavXmlAttribute(new XAttribute(name, value)));
	}

	public static NavXmlAttribute ALCreate(string localName, string namespaceUri, string value)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(() =>
		{
			XNamespace xNamespace = namespaceUri;
			XName name = xNamespace + localName;
			return new NavXmlAttribute(new XAttribute(name, value));
		});
	}

	public static NavXmlAttribute ALCreateNamespaceDeclaration(string prefix, string namespaceUri)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(() =>
		{
			XName name = XNamespace.Xmlns + prefix;
			return new NavXmlAttribute(new XAttribute(name, namespaceUri));
		});
	}

	internal NavXmlAttribute(XAttribute value)
		: base(value)
	{
	}
}
