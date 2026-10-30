using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal static class WeakEventHandler<TArgs>
{
	public static EventHandler<TArgs> Create<TTarget>(TTarget target, Action<TTarget, object, TArgs> invoker) where TTarget : class
	{
		WeakReference<TTarget> weakTarget = new WeakReference<TTarget>(target);
		return (object sender, TArgs args) =>
		{
			if (weakTarget.TryGetTarget(out var target2))
			{
				invoker(target2, sender, args);
			}
		};
	}
}
