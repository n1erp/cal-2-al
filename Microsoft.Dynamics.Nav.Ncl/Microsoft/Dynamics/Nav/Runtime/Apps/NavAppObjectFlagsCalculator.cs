using System;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class NavAppObjectFlagsCalculator
{
	public static NavAppObjectFlags CalculateFlags(ObjectType objectType, string userCode)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		return CalculateFlags(objectType, () => CodeunitParseHelper.HasEventsubscriber(userCode));
	}

	public static NavAppObjectFlags CalculateFlags(ObjectType objectType, byte[] astBytes)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		return CalculateFlags(objectType, () => CodeunitParseHelper.HasEventsubscriber(astBytes));
	}

	private static NavAppObjectFlags CalculateFlags(ObjectType objectType, Func<bool> hasEventSubscriber)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		NavAppObjectFlags val = (NavAppObjectFlags)0;
		if (objectType == ObjectType.CodeUnit)
		{
			val = (NavAppObjectFlags)(val | (hasEventSubscriber() ? 1 : 0));
		}
		return val;
	}
}
