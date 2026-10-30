using System;
using System.Text;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavTextBuilder : NavValue
{
	private readonly StringBuilder stringBuilder;

	public int ALCapacity
	{
		get
		{
			return stringBuilder.Capacity;
		}
		set
		{
			stringBuilder.Capacity = value;
		}
	}

	public int ALLength
	{
		get
		{
			return stringBuilder.Length;
		}
		set
		{
			stringBuilder.Length = value;
		}
	}

	public int ALMaxCapacity => stringBuilder.MaxCapacity;

	public static NavTextBuilder Default => new NavTextBuilder();

	public override NavNclType NclType => NavNclType.NavTextBuilder;

	public override int GetBytesSize
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override bool IsZeroOrEmpty => false;

	public bool ALAppend(DataError errorLevel, string str)
	{
		return Execute(errorLevel, () =>
		{
			stringBuilder.Append(str);
		});
	}

	public bool ALAppendLine(DataError errorLevel)
	{
		return Execute(errorLevel, () =>
		{
			stringBuilder.AppendLine();
		});
	}

	public bool ALAppendLine(DataError errorLevel, string str)
	{
		return Execute(errorLevel, () =>
		{
			stringBuilder.AppendLine(str);
		});
	}

	public void ALClear()
	{
		stringBuilder.Clear();
	}

	public bool ALEnsureCapacity(DataError errorLevel, int capacity)
	{
		return Execute(errorLevel, () =>
		{
			stringBuilder.EnsureCapacity(capacity);
		});
	}

	public bool ALInsert(DataError errorLevel, int position, string str)
	{
		return Execute(errorLevel, () =>
		{
			stringBuilder.Insert(position - 1, str);
		});
	}

	public bool ALRemove(DataError errorLevel, int startIndex, int count)
	{
		return Execute(errorLevel, () =>
		{
			stringBuilder.Remove(startIndex - 1, count);
		});
	}

	public bool ALReplace(DataError errorLevel, string oldValue, string newValue)
	{
		return Execute(errorLevel, () =>
		{
			stringBuilder.Replace(oldValue, newValue);
		});
	}

	public bool ALReplace(DataError errorLevel, string oldValue, string newValue, int startIndex, int count)
	{
		return Execute(errorLevel, () =>
		{
			stringBuilder.Replace(oldValue, newValue, startIndex - 1, count);
		});
	}

	public NavText ALToText()
	{
		return new NavText(stringBuilder.ToString());
	}

	public NavText ALToText(int startIndex, int count)
	{
		return new NavText(stringBuilder.ToString(startIndex - 1, count));
	}

	public static NavTextBuilder CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is NavTextBuilder result)
		{
			return result;
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavTextBuilder), value);
	}

	public NavTextBuilder()
	{
		stringBuilder = new StringBuilder();
	}

	public NavTextBuilder(string initialText)
	{
		stringBuilder = new StringBuilder(initialText);
	}

	public override int GetHashCode()
	{
		return stringBuilder.GetHashCode();
	}

	private static bool Execute(DataError errorLevel, Action operation)
	{
		return TrappableOperationExecutor.Execute(errorLevel, () =>
		{
			operation();
			return true;
		}, (Exception exception, DataError error) =>
		{
			if (error == DataError.ThrowError)
			{
				LogWriterHelper.LogExceptionEvent(exception);
			}
			if (exception is NavNCLArgumentOutOfRangeException)
			{
				throw new NavNCLArgumentOutOfRangeException();
			}
			if (exception is ArgumentException)
			{
				throw new NavNCLArgumentException();
			}
			throw exception;
		});
	}
}
