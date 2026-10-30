using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

internal static class NavAppCheckerResultExtensions
{
	public static NavAppCheckerResult VerifyAgainst(this NavAppCheckerResult result, NavAppChecker checker)
	{
		return checker.Verify(result);
	}

	public static void AddError(this NavAppCheckerResult result, NavAppError navAppError)
	{
		if (navAppError.NavAppErrorType.Equals(NavAppErrorType.Multiple))
		{
			navAppError.NestedErrors.Do((NavAppError e) =>
			{
				result.PassErrors.Add(e.Message);
			});
		}
		else
		{
			result.PassErrors.Add(navAppError.Message);
		}
	}

	public static void AddError(this NavAppSourceFile source, string category, NavAppError navAppError)
	{
		if (navAppError.NavAppErrorType.Equals(NavAppErrorType.Multiple))
		{
			navAppError.NestedErrors.Do((NavAppError e) =>
			{
				source.ErrorCategories[category].Add(e.Message);
			});
		}
		else
		{
			source.ErrorCategories[category].Add(navAppError.Message);
		}
	}
}
