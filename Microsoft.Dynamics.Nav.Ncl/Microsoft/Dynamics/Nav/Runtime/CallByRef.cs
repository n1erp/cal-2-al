using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public class CallByRef
{
	public class TempWrapper<T>
	{
		public T Var;
	}

	private object[] values = new object[10];

	public TempWrapper<T> TempVar<T>(int variableNumber)
	{
		if (variableNumber > values.Length - 1)
		{
			Expand();
		}
		if (values[variableNumber] == null)
		{
			values[variableNumber] = new TempWrapper<T>();
		}
		return (TempWrapper<T>)values[variableNumber];
	}

	private void Expand()
	{
		object[] destinationArray = new object[values.Length * 2];
		Array.Copy(values, destinationArray, values.Length);
		values = destinationArray;
	}

	public static object SideEffect(params object[] expressions)
	{
		return 0;
	}

	public static T Call<T>(object sideEffect1, T value, object sideEffect2)
	{
		return value;
	}
}
