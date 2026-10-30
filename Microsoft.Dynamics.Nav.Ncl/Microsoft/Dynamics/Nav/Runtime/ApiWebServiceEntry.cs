using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class ApiWebServiceEntry
{
	public string Publisher { get; private set; }

	public string Group { get; private set; }

	public string Version { get; private set; }

	public string Name { get; private set; }

	public ObjectType ObjectType { get; private set; }

	public int ObjectId { get; private set; }

	public PackageId PackageId { get; private set; }

	public int TableId { get; private set; }

	public bool TableTemporary { get; private set; }

	public string TableFilter { get; private set; }

	public string KeyFields { get; private set; }

	public bool ODataKeySpecified { get; private set; }

	public ApiWebServiceEntry(string publisher, string group, string version, string name, int tableId, bool tableTemporary, string tableFilter, string keyFields, bool oDataKeySpecified, ObjectType objectType, int objectId, PackageId packageId)
	{
		Publisher = publisher;
		Group = group;
		Version = version;
		Name = name;
		TableId = tableId;
		TableTemporary = tableTemporary;
		TableFilter = tableFilter;
		KeyFields = keyFields;
		ODataKeySpecified = oDataKeySpecified;
		ObjectType = objectType;
		ObjectId = objectId;
		PackageId = packageId;
	}
}
