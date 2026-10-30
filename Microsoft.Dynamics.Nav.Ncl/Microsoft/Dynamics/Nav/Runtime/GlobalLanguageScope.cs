using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal sealed class GlobalLanguageScope : IDisposable
{
	private bool ShouldPopLocalLanguangeOnDispose { get; set; }

	private NavSession Session { get; set; }

	public GlobalLanguageScope(NavSession navSession)
	{
		Session = navSession;
		int globalLanguage = navSession.GlobalLanguage;
		if (globalLanguage != navSession.LocalLanguage)
		{
			navSession.PushLocalLanguage(globalLanguage);
			ShouldPopLocalLanguangeOnDispose = true;
		}
		else
		{
			ShouldPopLocalLanguangeOnDispose = false;
		}
	}

	public void Dispose()
	{
		if (ShouldPopLocalLanguangeOnDispose)
		{
			Session.PopLocalLanguage();
		}
	}
}
