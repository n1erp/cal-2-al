using System;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavRuntimeHelpers
{
	public static NavArray<NavOption> AssignNewOptions(NavArray<NavOption> array, string newOptions)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		return array.AssignNewOptions(NCLOptionMetadata.Create(newOptions));
	}

	public static Exception ObsoleteMethod(string methodName, params string[] arguments)
	{
		if (methodName == null)
		{
			throw new ArgumentNullException("methodName");
		}
		if (arguments == null)
		{
			throw new ArgumentNullException("arguments");
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(methodName);
		stringBuilder.Append('(');
		bool flag = true;
		foreach (string value in arguments)
		{
			if (!flag)
			{
				stringBuilder.Append(", ");
			}
			else
			{
				flag = false;
			}
			stringBuilder.Append(value);
		}
		stringBuilder.Append(')');
		return new NavObsoleteMethodException(stringBuilder.ToString());
	}

	public static Exception ObsoleteType(string typeName, params string[] arguments)
	{
		if (typeName == null)
		{
			throw new ArgumentNullException("typeName");
		}
		if (arguments == null)
		{
			throw new ArgumentNullException("arguments");
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(typeName);
		stringBuilder.Append('(');
		bool flag = true;
		foreach (string value in arguments)
		{
			if (!flag)
			{
				stringBuilder.Append(", ");
			}
			else
			{
				flag = false;
			}
			stringBuilder.Append(value);
		}
		stringBuilder.Append(')');
		return new NavObsoleteTypeException(stringBuilder.ToString());
	}

	public static void CompilationError(string error, params object[] parameters)
	{
		throw new NavNCLCompilationException(CultureInfo.InvariantCulture, error, parameters);
	}

	public static void CompilationError(NavException exception)
	{
		if (exception == null)
		{
			throw new NavNCLArgumentException();
		}
		throw exception;
	}

	internal static int ByteCompare(byte[] left, byte[] right)
	{
		if (left == null)
		{
			throw new ArgumentNullException("left");
		}
		if (right == null)
		{
			throw new ArgumentNullException("left");
		}
		int num = Math.Min(left.Length, right.Length);
		for (int i = 0; i < num; i++)
		{
			byte b = right[i];
			byte b2 = left[i];
			if (b2 != b)
			{
				if (b2 >= b)
				{
					return 1;
				}
				return -1;
			}
		}
		if (left.Length != right.Length)
		{
			if (left.Length <= right.Length)
			{
				return -1;
			}
			return 1;
		}
		return 0;
	}
}
