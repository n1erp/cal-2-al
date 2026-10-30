using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.Text;
using System.Xml;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Common;

public static class ExceptionHandler
{
	public const string AdditionalExceptionKey = "AdditionalExceptionKey";

	private static readonly FieldInfo innerExceptionField = typeof(Exception).GetField("_innerException", BindingFlags.Instance | BindingFlags.NonPublic);

	private const string StackTraceProperty = "StackTrace";

	private const string MessageProperty = "Message";

	private static readonly List<string> cleanProperties = new List<string> { "MessageWithoutPrivateInformation", "ModuleNumber", "Source" };

	public static Exception GetOriginalException(FaultException<ServerFault> exception)
	{
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		object ex;
		if (exception.Detail != null)
		{
			ex = GetOriginalException(exception.Detail);
			if (ex == null)
			{
				return exception;
			}
		}
		else
		{
			ex = exception;
		}
		return (Exception)ex;
	}

	public static Exception GetOriginalException(ServerFault serverFault)
	{
		if (serverFault == null)
		{
			throw new ArgumentNullException("serverFault");
		}
		string exceptionData = serverFault.ExceptionData;
		if (exceptionData != null)
		{
			return WrapOriginalException(DeserializeFromByteArray(Convert.FromBase64String(exceptionData)));
		}
		return null;
	}

	public static Exception CreateExceptionFromTargetInvocationException(TargetInvocationException targetInvocationException)
	{
		if (targetInvocationException == null)
		{
			throw new ArgumentNullException("targetInvocationException");
		}
		Exception ex = WrapOriginalExceptionCore(targetInvocationException.InnerException, targetInvocationException);
		if (ex == null)
		{
			ex = new InvalidOperationException(targetInvocationException.Message, targetInvocationException);
		}
		return ex;
	}

	public static Exception UnwrapTargetInvocationException(Exception exception)
	{
		while (exception != null && exception is TargetInvocationException && exception.InnerException != null)
		{
			exception = exception.InnerException;
		}
		return exception;
	}

	public static NavBaseException UnwrapNavBaseException(Exception exception)
	{
		while (exception != null)
		{
			if (exception is NavBaseException result)
			{
				return result;
			}
			exception = exception.InnerException;
		}
		return null;
	}

	public static FaultException<ServerFault> CreateFaultException(Exception exception)
	{
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		return new FaultException<ServerFault>(new ServerFault(MakeOriginalExceptionNode(exception)), new FaultReason(new FaultReasonText(exception.Message, CultureInfo.CurrentCulture)));
	}

	private static string MakeOriginalExceptionNode(Exception exception)
	{
		return Convert.ToBase64String(SerializeToByteArray(CompatibleException(exception)));
	}

	public static Exception WrapOriginalException(Exception originalException)
	{
		Exception ex = WrapOriginalExceptionCore(originalException);
		if (ex != null)
		{
			return ex;
		}
		return ConstructException(originalException.GetType(), originalException.Message, originalException);
	}

	public static Exception WrapOriginalException(Exception topException, Exception originalException)
	{
		return WrapOriginalExceptionCore(topException, originalException);
	}

	public static Exception GetInnermostException(Exception exception)
	{
		if (exception == null)
		{
			return null;
		}
		Exception ex = exception;
		while (ex.InnerException != null)
		{
			ex = ex.InnerException;
		}
		return ex;
	}

	private static Exception WrapOriginalExceptionCore(Exception exception)
	{
		return WrapOriginalExceptionCore(exception, exception);
	}

	private static Exception WrapOriginalExceptionCore(Exception exception, Exception innerException)
	{
		if (exception == null)
		{
			return new ArgumentNullException("exception");
		}
		if (!IsCompatibleException(exception))
		{
			return null;
		}
		if (exception is NavBaseException ex)
		{
			return ex.Clone(innerException);
		}
		return ConstructException(exception.GetType(), exception.Message, innerException);
	}

	private static Exception ConstructException(Type exceptionType, string message, Exception innerException)
	{
		ConstructorInfo constructor = exceptionType.GetConstructor(new Type[2]
		{
			typeof(string),
			typeof(Exception)
		});
		if (constructor == null)
		{
			return new GenericException(message, innerException);
		}
		return (Exception)constructor.Invoke(new object[2] { message, innerException });
	}

	public static byte[] SerializeToByteArray(Exception value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		using MemoryStream memoryStream = new MemoryStream(1000);
		new BinaryFormatter().Serialize(memoryStream, value);
		return memoryStream.ToArray();
	}

	public static Exception DeserializeFromByteArray(byte[] bytes, Type type = null)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes");
		}
		using MemoryStream serializationStream = new MemoryStream(bytes);
		return new BinaryFormatter
		{
			Binder = new NavExceptionSerializationBinder(type)
		}.Deserialize(serializationStream) as Exception;
	}

	public static Exception CompatibleException(Exception exception)
	{
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		return CompatibleExceptionCore(exception);
	}

	private static Exception CompatibleExceptionCore(Exception exception)
	{
		if (exception == null)
		{
			return null;
		}
		if (!IsCompatibleException(exception))
		{
			return GenericException.CreateFromException(exception, CompatibleExceptionCore(exception.InnerException));
		}
		if (exception.InnerException == null)
		{
			return exception;
		}
		Exception ex = CompatibleExceptionCore(exception.InnerException);
		if (ex != exception.InnerException)
		{
			innerExceptionField.SetValue(exception, ex);
		}
		return exception;
	}

	public static bool IsCompatibleException(Exception exception)
	{
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		Assembly assembly = exception.GetType().Assembly;
		if (assembly == typeof(NavBaseException).Assembly)
		{
			return true;
		}
		if (assembly == typeof(InvalidOperationException).Assembly)
		{
			return true;
		}
		if (assembly == typeof(XmlException).Assembly)
		{
			return true;
		}
		if (assembly == typeof(ArgumentNullException).Assembly)
		{
			return true;
		}
		if (assembly == typeof(Win32Exception).Assembly)
		{
			return true;
		}
		return false;
	}

	public static void AddExtraException(Exception exception, Exception additionalException)
	{
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		if (additionalException == null)
		{
			throw new ArgumentNullException("additionalException");
		}
		if (!exception.Data.Contains("AdditionalExceptionKey"))
		{
			exception.Data.Add("AdditionalExceptionKey", additionalException);
		}
	}

	public static string FormatException(Exception exception)
	{
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		StringBuilder stringBuilder = new StringBuilder();
		FormatException(stringBuilder, exception, omitPrivateInformation: false);
		return stringBuilder.ToString();
	}

	public static void FormatExceptionClean(StringBuilder outFormattedException, Exception exception)
	{
		FormatException(outFormattedException, exception, omitPrivateInformation: true);
	}

	private static void FormatException(StringBuilder outFormattedException, Exception exception, bool omitPrivateInformation)
	{
		if (outFormattedException == null)
		{
			throw new ArgumentNullException("outFormattedException");
		}
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		if (exception is GenericException exception2)
		{
			FormatGenericExceptionProperties(outFormattedException, exception2);
		}
		else
		{
			FormatExceptionProperties(outFormattedException, exception, omitPrivateInformation);
		}
		if (exception.InnerException != null)
		{
			outFormattedException.AppendLine("----------------------------------");
			FormatException(outFormattedException, exception.InnerException, omitPrivateInformation);
		}
		if (exception.Data.Contains("AdditionalExceptionKey"))
		{
			outFormattedException.AppendLine("-----Additional Exception---------");
			if (exception.Data["AdditionalExceptionKey"] is Exception exception3)
			{
				FormatException(outFormattedException, exception3, omitPrivateInformation);
			}
		}
	}

	private static void FormatGenericExceptionProperties(StringBuilder stringBuilder, GenericException exception)
	{
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}:", exception.GetType().Name);
		stringBuilder.AppendLine();
		FormatEntry(stringBuilder, "Type", exception.Type);
		foreach (string key in exception.Dictionary.Keys)
		{
			string value = exception.Dictionary[key];
			FormatEntry(stringBuilder, key, value);
			if (key == "StackTrace" && !string.IsNullOrEmpty(exception.StackTrace))
			{
				WriteString(stringBuilder, exception.StackTrace);
			}
		}
	}

	private static void FormatExceptionProperties(StringBuilder stringBuilder, Exception exception, bool omitPrivateInformation)
	{
		FormatEntry(stringBuilder, "Type", exception.GetType().FullName);
		PropertyInfo[] properties = exception.GetType().GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			Type propertyType = propertyInfo.PropertyType;
			if (propertyType.IsValueType || (propertyType == typeof(string) && (!omitPrivateInformation || cleanProperties.Contains(propertyInfo.Name))))
			{
				object obj = null;
				try
				{
					obj = propertyInfo.GetValue(exception, null);
				}
				catch (Exception)
				{
				}
				FormatEntry(stringBuilder, propertyInfo.Name, (obj == null) ? string.Empty : obj.ToString());
			}
		}
		FormatEntry(stringBuilder, "StackTrace", exception.StackTrace);
	}

	private static void FormatEntry(StringBuilder stringBuilder, string name, string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			stringBuilder.Append(name).Append(':');
			WriteString(stringBuilder, value);
		}
	}

	private static void WriteString(StringBuilder stringBuilder, string value)
	{
		if (value.Contains(Environment.NewLine))
		{
			stringBuilder.AppendLine();
			string[] array = value.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
			foreach (string value2 in array)
			{
				stringBuilder.Append("  ");
				stringBuilder.Append(value2);
				stringBuilder.AppendLine();
			}
		}
		else
		{
			stringBuilder.Append(" ");
			stringBuilder.AppendLine(value);
		}
	}
}
