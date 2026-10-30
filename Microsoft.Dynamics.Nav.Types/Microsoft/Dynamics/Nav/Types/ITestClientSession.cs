using System;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types;

public interface ITestClientSession : IDisposable
{
	int OpenFormsCount { get; }

	ITestPage CreatePage(int id, ViewMode mode);

	ITestPage GetPage(Guid formHandle, bool forClose = false);

	bool ActivatePage(Guid formHandle, bool refresh);
}
