using System;
using System.ServiceModel;
using Microsoft.Dynamics.Nav.Types.Channels;
using Microsoft.Dynamics.Nav.Types.Media;

namespace Microsoft.Dynamics.Nav.Types;

[ServiceContract(SessionMode = SessionMode.Required, Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public interface IOpenDynamicFileHandlerService
{
	[OperationContract(IsInitiating = true, IsTerminating = true)]
	[FaultContract(typeof(ServerFault), Action = "http://www.microsoft.com/Dynamics/NAV/51/ServerFault")]
	[ChunkingBehavior(ChunkOutput = true)]
	NavUrlAccessibleMedia GetUrlAccessibleMedia(Uri uri);
}
