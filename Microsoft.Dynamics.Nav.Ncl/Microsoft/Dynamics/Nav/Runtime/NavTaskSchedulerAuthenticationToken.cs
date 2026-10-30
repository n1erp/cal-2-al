using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavTaskSchedulerAuthenticationToken
{
	internal Guid UserId { get; }

	internal string UserName { get; }

	public NavTaskSchedulerAuthenticationToken(Guid userId, string userName)
	{
		UserId = userId;
		UserName = userName;
	}
}
