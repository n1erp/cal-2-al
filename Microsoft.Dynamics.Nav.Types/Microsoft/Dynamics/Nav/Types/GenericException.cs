using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public sealed class GenericException : Exception, ISerializable
{
	private string type;

	private Dictionary<string, string> dictionary;

	public string Type => type;

	public Dictionary<string, string> Dictionary
	{
		get
		{
			if (dictionary == null)
			{
				dictionary = new Dictionary<string, string>();
			}
			return dictionary;
		}
	}

	public GenericException()
	{
	}

	private GenericException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		type = info.GetString("Type");
		dictionary = (Dictionary<string, string>)info.GetValue("Dictionary", typeof(Dictionary<string, string>));
	}

	public GenericException(string message)
		: base(message)
	{
	}

	public GenericException(string message, Exception inner)
		: base(message, inner)
	{
	}

	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Type", type);
		info.AddValue("Dictionary", dictionary, typeof(Dictionary<string, string>));
		GetObjectData(info, context);
	}

	public static GenericException CreateFromException(Exception exception, Exception innerException)
	{
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		if (exception is GenericException result)
		{
			return result;
		}
		GenericException ex = new GenericException(exception.Message, innerException);
		ex.type = exception.GetType().FullName;
		AddPropertyValues(ex.Dictionary, exception);
		CopySerializableElements(exception.Data, ex.Data);
		return ex;
	}

	private static void AddPropertyValues(IDictionary<string, string> dictionary, object o)
	{
		PropertyInfo[] properties = o.GetType().GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			Type propertyType = propertyInfo.PropertyType;
			if (!propertyType.IsValueType && !(propertyType == typeof(string)))
			{
				continue;
			}
			object value = propertyInfo.GetValue(o, null);
			if (value != null)
			{
				string value2 = value.ToString();
				if (!string.IsNullOrEmpty(value2))
				{
					dictionary.Add(propertyInfo.Name, value2);
				}
			}
		}
	}

	private static void CopySerializableElements(IDictionary fromDictionary, IDictionary toDictionary)
	{
		foreach (DictionaryEntry item in fromDictionary)
		{
			if (item.Key == null)
			{
				continue;
			}
			string key = item.Key.ToString();
			if (!toDictionary.Contains(key))
			{
				object obj = item.Value;
				if (obj != null && obj.GetType().Assembly != typeof(string).Assembly && obj.GetType().Assembly != typeof(NavBaseException).Assembly)
				{
					obj = obj.ToString();
				}
				toDictionary.Add(key, obj);
			}
		}
	}
}
