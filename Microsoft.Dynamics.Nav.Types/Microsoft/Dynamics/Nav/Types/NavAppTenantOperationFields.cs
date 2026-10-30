namespace Microsoft.Dynamics.Nav.Types;

public static class NavAppTenantOperationFields
{
	public enum NavAppOperationType
	{
		DeployTarget,
		DeployPackage
	}

	public enum NavAppOperationStatus
	{
		Unknown,
		InProgress,
		Failed,
		Completed,
		NotFound
	}

	public const int OperationId = 1;

	public const int StartedOn = 2;

	public const int OperationType = 3;

	public const int Status = 4;

	public const int Details = 5;

	public const int MetadataVersion = 6;

	public const int Metadata = 7;

	public const int MetadataKey = 8;

	public const int Description = 9;
}
