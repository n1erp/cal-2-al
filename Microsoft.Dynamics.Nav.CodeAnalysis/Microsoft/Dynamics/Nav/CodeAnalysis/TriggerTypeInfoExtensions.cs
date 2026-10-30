namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class TriggerTypeInfoExtensions
{
	internal static bool IsCodeunitUpgradeTriggerTypeInfo(this TriggerTypeInfo triggerTypeInfo)
	{
		TriggerTypeKind? triggerTypeKind = triggerTypeInfo?.Kind;
		if (triggerTypeKind.HasValue)
		{
			TriggerTypeKind valueOrDefault = triggerTypeKind.GetValueOrDefault();
			if ((uint)(valueOrDefault - 1) <= 5u)
			{
				return true;
			}
		}
		return false;
	}

	internal static bool IsCodeunitInstallTriggerTypeInfo(this TriggerTypeInfo triggerTypeInfo)
	{
		TriggerTypeKind? triggerTypeKind = triggerTypeInfo?.Kind;
		if (triggerTypeKind.HasValue)
		{
			TriggerTypeKind valueOrDefault = triggerTypeKind.GetValueOrDefault();
			if ((uint)(valueOrDefault - 7) <= 1u)
			{
				return true;
			}
		}
		return false;
	}

	internal static bool IsCodeunitTestRunnerTriggerTypeInfo(this TriggerTypeInfo triggerTypeInfo)
	{
		TriggerTypeKind? triggerTypeKind = triggerTypeInfo?.Kind;
		if (triggerTypeKind.HasValue)
		{
			TriggerTypeKind valueOrDefault = triggerTypeKind.GetValueOrDefault();
			if ((uint)(valueOrDefault - 9) <= 1u)
			{
				return true;
			}
		}
		return false;
	}

	internal static bool IsCodeunitOnRunTriggerTypeInfo(this TriggerTypeInfo triggerTypeInfo)
	{
		if (triggerTypeInfo == null)
		{
			return false;
		}
		return triggerTypeInfo.Kind == TriggerTypeKind.CodeunitOnRun;
	}
}
