using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class ArrayScaffolding : Scaffolding<IArrayAccess>
{
	private readonly int dimensionIndex;

	private readonly Type elementType;

	private int[] dimensions;

	private int[] indexes;

	private string typeString;

	public override string TypeString => typeString;

	protected override bool ProbablyHasChildren => true;

	public ArrayScaffolding(string name, Type type, ValueBinder valueBinder, bool promotedChildren)
		: this(name, type, valueBinder, type.GetGenericArguments()[0], 0, null, promotedChildren)
	{
	}

	private ArrayScaffolding(string name, Type targetType, ValueBinder valueBinder, Type elementType, int dimensionIndex, int[] indexes, bool promotedChildren)
		: base(name, targetType, valueBinder, true, promotedChildren)
	{
		this.elementType = elementType;
		this.dimensionIndex = dimensionIndex;
		this.indexes = indexes;
	}

	protected override IEnumerable<Scaffolding> CreateChildren(IArrayAccess instance)
	{
		if (dimensions == null)
		{
			return Array.Empty<Scaffolding>();
		}
		int num = dimensions[dimensionIndex];
		Scaffolding[] array = new Scaffolding[num];
		if (indexes == null)
		{
			indexes = new int[dimensions.Length];
		}
		if (dimensionIndex == dimensions.Length - 1)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = ScaffoldingFactory.GetScaffolding(IndexName(i), elementType, CreatevalueBinder(i), instance, false);
			}
			return array;
		}
		for (int j = 0; j < array.Length; j++)
		{
			int[] array2 = new int[dimensions.Length];
			Array.Copy(indexes, array2, array2.Length - 1);
			array2[dimensionIndex] = j;
			array[j] = new ArrayScaffolding(IndexName(j), TargetType, new IdentityValueBinder(), elementType, dimensionIndex + 1, array2, promotedChildren: false);
		}
		return array;
	}

	protected override string GetStringValue(IArrayAccess instance)
	{
		return null;
	}

	protected override bool IsCompatibleOverride(object instance)
	{
		if (dimensions == null)
		{
			return true;
		}
		if (instance is IArrayAccess arrayAccess && arrayAccess.Dimensions.Length == dimensions.Length)
		{
			for (int i = 0; i < dimensions.Length; i++)
			{
				int num = dimensions[i];
				int num2 = arrayAccess.Dimensions[i];
				if (num != num2)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	protected override void ReadMetadata(IArrayAccess instance)
	{
		if (instance != null)
		{
			dimensions = instance.Dimensions;
			typeString = "ARRAY[" + string.Join("; ", (from d in dimensions.Skip(dimensionIndex)
				select d.ToString(CultureInfo.InvariantCulture)).ToArray()) + "] OF " + ScaffoldingHelper.GetTypeString(elementType, instance);
		}
		else
		{
			typeString = string.Empty;
		}
	}

	private static string IndexName(int i)
	{
		return string.Concat('[', i + 1, ']');
	}

	private ValueBinder CreatevalueBinder(int i)
	{
		return new DelegateValueBinder((object parent) =>
		{
			int[] array = new int[dimensions.Length];
			Array.Copy(indexes, array, array.Length - 1);
			array[array.Length - 1] = i;
			return ((IArrayAccess)parent)[array];
		}, (object parent, object newValue) =>
		{
			int[] array = new int[dimensions.Length];
			Array.Copy(indexes, array, array.Length - 1);
			array[array.Length - 1] = i;
			try
			{
				((IArrayAccess)parent)[array] = newValue;
				return true;
			}
			catch (InvalidCastException)
			{
				return false;
			}
		}, elementType);
	}
}
