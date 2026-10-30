using System;
using System.Reflection;
using CommandLine.Infrastructure;

namespace CommandLine;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class HelpOptionAttribute : BaseOptionAttribute
{
	private const string DefaultHelpText = "Display this help screen.";

	public override bool Required
	{
		get
		{
			return false;
		}
		set
		{
			throw new InvalidOperationException();
		}
	}

	public HelpOptionAttribute()
		: this("help")
	{
		HelpText = "Display this help screen.";
	}

	public HelpOptionAttribute(char shortName)
		: base(shortName, null)
	{
		HelpText = "Display this help screen.";
	}

	public HelpOptionAttribute(string longName)
		: base(null, longName)
	{
		HelpText = "Display this help screen.";
	}

	public HelpOptionAttribute(char shortName, string longName)
		: base(shortName, longName)
	{
		HelpText = "Display this help screen.";
	}

	internal static void InvokeMethod(object target, Pair<MethodInfo, HelpOptionAttribute> pair, out string text)
	{
		text = null;
		MethodInfo left = pair.Left;
		if (!CheckMethodSignature(left))
		{
			throw new MemberAccessException();
		}
		text = (string)left.Invoke(target, null);
	}

	private static bool CheckMethodSignature(MethodInfo value)
	{
		if (value.ReturnType == typeof(string))
		{
			return value.GetParameters().Length == 0;
		}
		return false;
	}
}
