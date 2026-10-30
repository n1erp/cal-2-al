using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

[DebuggerDisplay("{Success ? \"Success\" : \"Fail: \" + Error.ErrorType.ToString()}")]
public class NavAppResult
{
	public bool Success { get; }

	public NavAppError NavAppError { get; }

	public bool Failure => !Success;

	protected NavAppResult(bool success, NavAppError navAppError)
	{
		if ((success && navAppError != null) || (!success && navAppError == null))
		{
			throw new ArgumentException("Result cannot be successful and have an error concurrently.");
		}
		Success = success;
		NavAppError = navAppError;
	}

	public static NavAppResult Fail(NavAppError navAppError)
	{
		return new NavAppResult(success: false, navAppError);
	}

	public static NavAppResult<T> Fail<T>(NavAppError navAppError)
	{
		return new NavAppResult<T>(default, success: false, navAppError);
	}

	public static NavAppResult Ok()
	{
		return new NavAppResult(success: true, null);
	}

	public static NavAppResult<T> Ok<T>(T value)
	{
		return new NavAppResult<T>(value, success: true, null);
	}
}
[SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Grouping Generic version with the base.")]
[DebuggerDisplay("{Success ? \"Success: \\\"\" + Value.ToString() + \"\\\"\" : \"Fail: \" + Error.ErrorType.ToString()}")]
public class NavAppResult<T> : NavAppResult
{
	private T resultValue;

	public T Value
	{
		get
		{
			if (!Success)
			{
				throw new InvalidOperationException("This Result has no value.");
			}
			return resultValue;
		}
		private set
		{
			resultValue = value;
		}
	}

	protected internal NavAppResult(T value, bool success, NavAppError navAppError)
		: base(success, navAppError)
	{
		if (success && value == null)
		{
			throw new ArgumentNullException("value", "Result<T> cannot accept a null value when success == true.");
		}
		if (value is NavAppResult)
		{
			throw new InvalidOperationException("A Result<T>'s Value should never itself be a Result.");
		}
		Value = value;
	}
}
