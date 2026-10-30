using System;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavFormSourceExpression
{
	private string id;

	private string name;

	private Type type;

	private bool writable;

	private NavFormSourceExpressionGetter get;

	private NavFormSourceExpressionSetter set;

	private SourceExpressionType sourceExpressionType;

	private bool isInRepeaterGroup;

	public SourceExpressionType SourceExpressionType => sourceExpressionType;

	public string Id => id;

	public string Name => name;

	public Type Type => type;

	public bool SkipOnSaveValues { get; set; }

	public bool Writable => writable;

	public bool IsInRepeaterGroup => isInRepeaterGroup;

	internal NavFormSourceExpression(string id, string name, SourceExpressionType sourceExpressionType, Type type, NavFormSourceExpressionGetter get, NavFormSourceExpressionSetter set)
	{
		this.id = id;
		this.name = name;
		this.sourceExpressionType = sourceExpressionType;
		this.type = type;
		this.get = get;
		this.set = set;
		writable = set != null;
	}

	public NavValue Get()
	{
		return TryExecuteAndHandleExceptionMappingSourceExpressionGetter(get);
	}

	public void Set(NavValue value)
	{
		TryExecuteAndHandleExceptionMappingSourceExpressionSetter(set, value);
	}

	internal void SetIsInRepeaterGroup(bool value)
	{
		isInRepeaterGroup = value;
	}

	internal static NavValue TryExecuteAndHandleExceptionMappingSourceExpressionGetter(NavFormSourceExpressionGetter getter)
	{
		try
		{
			return getter();
		}
		catch (DivideByZeroException ex)
		{
			throw new NavALException(ex.Message, ex);
		}
		catch (IndexOutOfRangeException ex2)
		{
			throw new NavALException(ex2.Message, ex2);
		}
		catch (FormatException ex3)
		{
			throw new NavALException(ex3.Message, ex3);
		}
		catch (OverflowException ex4)
		{
			throw new NavALException(ex4.Message, ex4);
		}
	}

	internal static void TryExecuteAndHandleExceptionMappingSourceExpressionSetter(NavFormSourceExpressionSetter setter, NavValue value)
	{
		try
		{
			setter(value);
		}
		catch (DivideByZeroException ex)
		{
			throw new NavALException(ex.Message, ex);
		}
		catch (IndexOutOfRangeException ex2)
		{
			throw new NavALException(ex2.Message, ex2);
		}
		catch (FormatException ex3)
		{
			throw new NavALException(ex3.Message, ex3);
		}
		catch (OverflowException ex4)
		{
			throw new NavALException(ex4.Message, ex4);
		}
	}
}
