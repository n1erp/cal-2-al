namespace Microsoft.Dynamics.Nav.Diagnostic;

public sealed class NavPermissionTelemetryEvent
{
	public int EventId { get; internal set; }

	public string ServerInstance { get; private set; }

	public string NavTenantId { get; private set; }

	public int SessionId { get; private set; }

	public int ObjectType { get; private set; }

	public int ObjectId { get; private set; }

	public int Permissions { get; private set; }

	public int CallingObjectType { get; private set; }

	public int CallingObjectId { get; private set; }

	public int IndirectPermissionFromCaller { get; private set; }

	public int ValidationCategory { get; private set; }

	public static NavPermissionTelemetryEvent CreateEvent(int eventId, string serverInstanceName, string navTenantId, int sessionId, int objectType, int objectId, int permissions, int callingObjectType, int callingObjectId, int indirectPermissionFromCaller = 0, int validationCategory = 0)
	{
		return new NavPermissionTelemetryEvent
		{
			EventId = eventId,
			ServerInstance = serverInstanceName,
			NavTenantId = navTenantId,
			SessionId = sessionId,
			ObjectType = objectType,
			ObjectId = objectId,
			Permissions = permissions,
			CallingObjectType = callingObjectType,
			CallingObjectId = callingObjectId,
			IndirectPermissionFromCaller = indirectPermissionFromCaller,
			ValidationCategory = validationCategory
		};
	}
}
