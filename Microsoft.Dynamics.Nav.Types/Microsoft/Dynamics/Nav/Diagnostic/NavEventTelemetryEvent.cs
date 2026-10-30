namespace Microsoft.Dynamics.Nav.Diagnostic;

public sealed class NavEventTelemetryEvent
{
	public int SessionId { get; private set; }

	public int ObjectType { get; private set; }

	public int ObjectId { get; private set; }

	public string EventName { get; private set; }

	public string ElementName { get; private set; }

	public int EventType { get; private set; }

	public int CallingObjectType { get; private set; }

	public int CallingObjectId { get; private set; }

	public string CallingMethodName { get; private set; }

	public static NavEventTelemetryEvent CreateEvent(int sessionId, int objectType, int objectId, string eventName, string elementName, int eventType, int callingObjectType, int callingObjectId, string callingMethodName)
	{
		return new NavEventTelemetryEvent
		{
			SessionId = sessionId,
			ObjectType = objectType,
			ObjectId = objectId,
			EventName = eventName,
			EventType = eventType,
			ElementName = elementName,
			CallingObjectType = callingObjectType,
			CallingObjectId = callingObjectId,
			CallingMethodName = callingMethodName
		};
	}
}
