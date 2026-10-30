using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class NavEnumTypeEvaluator<EnumType, NavValueType> : NavValueEvaluator<NavValueType> where NavValueType : NavValue
{
	private readonly List<Tuple<string, EnumType, int>> enumTypeNames;

	private readonly string optionNames;

	private readonly Func<EnumType, NavValueType> creationFunction;

	protected NavEnumTypeEvaluator(Func<EnumType, NavValueType> creationFunction)
	{
		this.creationFunction = creationFunction;
		enumTypeNames = new List<Tuple<string, EnumType, int>>();
		StringBuilder stringBuilder = new StringBuilder();
		FieldInfo[] fields = typeof(EnumType).GetFields(BindingFlags.Static | BindingFlags.Public);
		foreach (FieldInfo fieldInfo in fields)
		{
			string name = fieldInfo.Name;
			int num = (int)fieldInfo.GetValue(null);
			EnumType item = (EnumType)(object)num;
			enumTypeNames.Add(Tuple.Create(name, item, num));
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(name);
		}
		optionNames = stringBuilder.ToString();
	}

	protected override bool InternalEvaluate(DataError dataError, out NavValueType value, INavValueMetadata metadata, string source, int number)
	{
		source = source.Trim();
		foreach (Tuple<string, EnumType, int> enumTypeName in enumTypeNames)
		{
			if (enumTypeName.Item1.Equals(source, StringComparison.OrdinalIgnoreCase))
			{
				value = creationFunction(enumTypeName.Item2);
				return true;
			}
		}
		foreach (Tuple<string, EnumType, int> enumTypeName2 in enumTypeNames)
		{
			if (enumTypeName2.Item1.StartsWith(source, StringComparison.OrdinalIgnoreCase))
			{
				value = creationFunction(enumTypeName2.Item2);
				return true;
			}
		}
		if (int.TryParse(source, out var result))
		{
			foreach (Tuple<string, EnumType, int> enumTypeName3 in enumTypeNames)
			{
				if (enumTypeName3.Item3 == result)
				{
					value = creationFunction(enumTypeName3.Item2);
					return true;
				}
			}
		}
		throw NavNCLInvalidOptionStringException.Create(CultureInfo.CurrentCulture, source, optionNames);
	}
}
