using System;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppResource
{
	public static readonly NavAppResource None = new NavAppResource(new NavAppResourceId(PackageId.op_Implicit(Guid.Empty), NavAppResourceType.None, "NONE"), new byte[0], null);

	public NavAppResourceId Id { get; }

	public byte[] Content { get; }

	public Sha256Hash ContentHash { get; }

	public NavAppResource(NavAppResourceId id, byte[] content, Sha256Hash contentHash)
	{
		Id = id;
		Content = content;
		ContentHash = contentHash;
	}
}
