using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

[DebuggerDisplay("{value}")]
public sealed class NavAppErrorType
{
	private static readonly Dictionary<Type, NavAppErrorType> ExceptionToErrorTypeLookup = new Dictionary<Type, NavAppErrorType>
	{
		{
			typeof(Exception),
			Unspecified
		},
		{
			typeof(ArgumentException),
			Argument
		},
		{
			typeof(ArgumentOutOfRangeException),
			Argument
		},
		{
			typeof(ArgumentNullException),
			ArgumentNull
		},
		{
			typeof(UnauthorizedAccessException),
			Permissions
		},
		{
			typeof(SecurityException),
			Permissions
		},
		{
			typeof(FileNotFoundException),
			FileNotFound
		},
		{
			typeof(DirectoryNotFoundException),
			DirectoryNotFound
		},
		{
			typeof(AggregateException),
			Multiple
		},
		{
			typeof(InvalidOperationException),
			InvalidOperation
		}
	};

	private readonly string value;

	public static NavAppErrorType Argument { get; } = new NavAppErrorType("Argument");

	public static NavAppErrorType ArgumentNull { get; } = new NavAppErrorType("ArgumentNull");

	public static NavAppErrorType Unspecified { get; } = new NavAppErrorType("Unspecified");

	public static NavAppErrorType Multiple { get; } = new NavAppErrorType("Multiple");

	public static NavAppErrorType DirectoryNotFound { get; } = new NavAppErrorType("DirectoryNotFound");

	public static NavAppErrorType FileNotFound { get; } = new NavAppErrorType("FileNotFound");

	public static NavAppErrorType InvalidOperation { get; } = new NavAppErrorType("InvalidOperation");

	public static NavAppErrorType Permissions { get; } = new NavAppErrorType("Permissions");

	private NavAppErrorType(string value)
	{
		this.value = value;
	}

	public static implicit operator string(NavAppErrorType navAppErrorType)
	{
		return navAppErrorType.value;
	}

	public override bool Equals(object obj)
	{
		if (obj is NavAppErrorType navAppErrorType)
		{
			return navAppErrorType.value == value;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	public override string ToString()
	{
		return value;
	}

	internal static NavAppErrorType FromExceptionType(Type exceptionType)
	{
		Type type = exceptionType;
		NavAppErrorType result;
		while (!ExceptionToErrorTypeLookup.TryGetValue(type, out result))
		{
			type = type.GetTypeInfo().BaseType;
		}
		return result;
	}
}
