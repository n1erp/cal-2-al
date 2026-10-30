using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

public class DefaultSerializationBinder : SerializationBinder, ISerializationBinder
{
	internal static readonly DefaultSerializationBinder Instance = new DefaultSerializationBinder();

	private readonly ThreadSafeStore<TypeNameKey, Type> _typeCache;

	public DefaultSerializationBinder()
	{
		_typeCache = new ThreadSafeStore<TypeNameKey, Type>(GetTypeFromTypeNameKey);
	}

	private Type GetTypeFromTypeNameKey(TypeNameKey typeNameKey)
	{
		string assemblyName = typeNameKey.AssemblyName;
		string typeName = typeNameKey.TypeName;
		if (assemblyName != null)
		{
			Assembly assembly = Assembly.LoadWithPartialName(assemblyName);
			if (assembly == null)
			{
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (Assembly assembly2 in assemblies)
				{
					if (assembly2.FullName == assemblyName || assembly2.GetName().Name == assemblyName)
					{
						assembly = assembly2;
						break;
					}
				}
			}
			if (assembly == null)
			{
				throw new JsonSerializationException("Could not load assembly '{0}'.".FormatWith(CultureInfo.InvariantCulture, assemblyName));
			}
			Type type = assembly.GetType(typeName);
			if (type == null)
			{
				if (typeName.IndexOf('`') >= 0)
				{
					try
					{
						type = GetGenericTypeFromTypeName(typeName, assembly);
					}
					catch (Exception innerException)
					{
						throw new JsonSerializationException("Could not find type '{0}' in assembly '{1}'.".FormatWith(CultureInfo.InvariantCulture, typeName, assembly.FullName), innerException);
					}
				}
				if (type == null)
				{
					throw new JsonSerializationException("Could not find type '{0}' in assembly '{1}'.".FormatWith(CultureInfo.InvariantCulture, typeName, assembly.FullName));
				}
			}
			return type;
		}
		return Type.GetType(typeName);
	}

	private Type GetGenericTypeFromTypeName(string typeName, Assembly assembly)
	{
		Type result = null;
		int num = typeName.IndexOf('[');
		if (num >= 0)
		{
			string name = typeName.Substring(0, num);
			Type type = assembly.GetType(name);
			if (type != null)
			{
				List<Type> list = new List<Type>();
				int num2 = 0;
				int num3 = 0;
				int num4 = typeName.Length - 1;
				for (int i = num + 1; i < num4; i++)
				{
					switch (typeName[i])
					{
					case '[':
						if (num2 == 0)
						{
							num3 = i + 1;
						}
						num2++;
						break;
					case ']':
						num2--;
						if (num2 == 0)
						{
							TypeNameKey typeNameKey = ReflectionUtils.SplitFullyQualifiedTypeName(typeName.Substring(num3, i - num3));
							list.Add(GetTypeByName(typeNameKey));
						}
						break;
					}
				}
				result = type.MakeGenericType(list.ToArray());
			}
		}
		return result;
	}

	private Type GetTypeByName(TypeNameKey typeNameKey)
	{
		return _typeCache.Get(typeNameKey);
	}

	public override Type BindToType(string assemblyName, string typeName)
	{
		return GetTypeByName(new TypeNameKey(assemblyName, typeName));
	}

	public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
	{
		assemblyName = serializedType.Assembly.FullName;
		typeName = serializedType.FullName;
	}
}
