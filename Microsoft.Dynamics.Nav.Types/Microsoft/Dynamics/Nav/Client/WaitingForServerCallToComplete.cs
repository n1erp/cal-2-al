using System;

namespace Microsoft.Dynamics.Nav.Client;

public delegate void WaitingForServerCallToComplete(Func<bool> isServerDoneProcessing);
