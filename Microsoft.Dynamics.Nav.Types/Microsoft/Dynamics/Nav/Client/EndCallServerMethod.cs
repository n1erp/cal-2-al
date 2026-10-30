using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Client;

public delegate T EndCallServerMethod<T>(IAsyncNavService service, IAsyncResult result);
