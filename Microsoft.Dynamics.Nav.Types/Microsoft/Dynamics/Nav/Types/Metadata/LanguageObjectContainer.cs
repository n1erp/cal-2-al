namespace Microsoft.Dynamics.Nav.Types.Metadata;

internal class LanguageObjectContainer
{
	public ObjectType ObjectType { get; private set; }

	public int ObjectId { get; private set; }

	public int StartElement { get; private set; }

	public int ElementCount { get; set; }

	public LanguageObjectContainer(ObjectType objectType, int objectId, int start, int count)
	{
		ObjectType = objectType;
		ObjectId = objectId;
		StartElement = start;
		ElementCount = count;
	}
}
