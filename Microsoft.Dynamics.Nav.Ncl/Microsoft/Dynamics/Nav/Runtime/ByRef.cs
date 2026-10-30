using System;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Runtime;

public class ByRef<T> : IByRef
{
	public delegate void SetValue(T value);

	public delegate T GetValue();

	internal SetValue setter;

	internal GetValue getter;

	public T Value
	{
		get
		{
			return getter();
		}
		set
		{
			setter(value);
		}
	}

	public object ObjectValue
	{
		get
		{
			return Value;
		}
		set
		{
			Value = (T)value;
		}
	}

	public Type ObjectType => typeof(T);

	public ByRef(GetValue getter, SetValue setter)
	{
		SetGetterSetter(getter, setter);
	}

	public ByRef(FieldInfo fieldInfo, object instance)
	{
		SetGetterSetter(() => (T)fieldInfo.GetValue(instance), (T v) =>
		{
			fieldInfo.SetValue(instance, v);
		});
	}

	public ByRef()
	{
	}

	public void SetGetterSetter(GetValue getterCallback, SetValue setterCallback)
	{
		getter = getterCallback;
		setter = setterCallback;
	}

	public static implicit operator T(ByRef<T> byRef)
	{
		return byRef.Value;
	}
}
