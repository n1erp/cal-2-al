using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class CalcFieldsCacheItem : DataCacheRequestItem
{
	private FieldDictionary<NavValue> calculatedValues;

	internal FieldDictionary<NavValue> CalculatedValues => calculatedValues;

	internal unsafe CalcFieldsCacheItem(ResultSetGroupCacheItem groupParent, DataCacheRequest request, FieldDictionary<NavValue> calculatedValuesParameter, DateTime created)
		: base(groupParent, request, created, calculatedValuesParameter.Items.Length * (sizeof(IntPtr) + 24))
	{
		calculatedValues = calculatedValuesParameter;
	}
}
