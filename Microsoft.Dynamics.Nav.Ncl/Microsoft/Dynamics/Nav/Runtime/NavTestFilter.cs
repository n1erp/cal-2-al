using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavTestFilter
{
	private ITestFilter filter;

	public bool ALAscending
	{
		get
		{
			return filter.Ascending;
		}
		set
		{
			filter.Ascending = value;
		}
	}

	public string ALCurrentKey => filter.CurrentKey;

	internal NavTestFilter(ITestFilter filter)
	{
		this.filter = filter;
	}

	public void ALSetFilter(int fieldNo, string filterValue)
	{
		filter.SetFilter(fieldNo, filterValue);
	}

	public string ALGetFilter(int fieldNo)
	{
		return filter.GetFilter(fieldNo);
	}

	public void ALSetCurrentKey(params int[] fields)
	{
		ALSetCurrentKey(DataError.ThrowError, fields);
	}

	public bool ALSetCurrentKey(DataError errorLevel, params int[] fields)
	{
		if (fields == null)
		{
			throw new ArgumentNullException("fields");
		}
		try
		{
			filter.SetCurrentKeyFields(fields);
		}
		catch (NavClientPageAbortException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
		return true;
	}

	public string GetFieldName(int fieldNo)
	{
		return fieldNo.ToString(CultureInfo.InvariantCulture);
	}
}
