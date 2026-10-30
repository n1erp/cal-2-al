using System;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavTestField : NavValue
{
	protected delegate void FieldOperationVoid();

	protected delegate T FieldOperationReturnValue<T>();

	private ITestField testField;

	private NavTestPageBase parent;

	public string ALValue
	{
		get
		{
			return CheckError(() => testField.Value);
		}
		set
		{
			CheckError(() =>
			{
				testField.Activate();
				testField.Value = value;
			});
		}
	}

	public string ALCaption => testField.Caption;

	internal NavType FieldType => testField.FieldType;

	internal string Name => testField.Name;

	public override int GetBytesSize => 0;

	public override bool IsZeroOrEmpty => testField == null;

	public override NavNclType NclType => NavNclType.NavTestField;

	public void ALActivate()
	{
		CheckError(() =>
		{
			testField.Activate();
		});
	}

	public int ALValidationErrorCount()
	{
		return testField.ValidationErrorCount;
	}

	public string ALGetValidationError()
	{
		if (ALValidationErrorCount() == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < ALValidationErrorCount(); i++)
		{
			if (i > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(ALGetValidationError(i));
		}
		return stringBuilder.ToString();
	}

	public string ALGetValidationError(int index)
	{
		try
		{
			return testField.GetValidationError(index - 1);
		}
		catch (IndexOutOfRangeException)
		{
			throw new NavNCLIndexOutOfBoundsException();
		}
	}

	public void ALLookup()
	{
		CheckError(() =>
		{
			testField.Lookup();
		});
	}

	public void ALAssistEdit()
	{
		CheckError(() =>
		{
			testField.AssistEdit();
		});
	}

	public void ALDrilldown()
	{
		CheckError(() =>
		{
			testField.Drilldown();
		});
	}

	public void ALInvoke()
	{
		CheckError(() =>
		{
			testField.Invoke();
		});
	}

	public void ALAssertEquals(NavValue value)
	{
		string text;
		if (!(value is NavStringValue))
		{
			value = NavValue.CreateNavValueFromObject(NavValueMetadata.DefaultMetadata(testField.FieldType), value);
			text = testField.ValueToString(value.ClientObject);
		}
		else
		{
			text = value.ToString();
		}
		if (string.CompareOrdinal(ALValue, text) != 0)
		{
			throw NavTestAssertEqualsException.Create(CultureInfo.CurrentCulture, testField.Name, text, ALValue);
		}
	}

	public void ALSetValue(NavValue value)
	{
		if (value is NavStringValue)
		{
			ALValue = value.ToString();
			return;
		}
		value = NavValue.CreateNavValueFromObject(NavValueMetadata.DefaultMetadata(testField.FieldType), value);
		ALValue = testField.ValueToString((value.NclType == NavNclType.NavDateTime) ? ((object)((NavDateTime)value).ClientLocalValue) : value.ClientObject);
	}

	public int ALAsInteger()
	{
		try
		{
			return NavInteger.CreateFromObject(testField.ObjectValue);
		}
		catch (ArgumentException)
		{
			throw new NavNCLConversionException(testField.ObjectValue.GetType(), typeof(int));
		}
	}

	public bool ALAsBoolean()
	{
		try
		{
			return NavBoolean.CreateFromObject(testField.ObjectValue);
		}
		catch (ArgumentException)
		{
			throw new NavNCLConversionException(testField.ObjectValue.GetType(), typeof(bool));
		}
	}

	public Decimal18 ALAsDecimal()
	{
		try
		{
			return NavDecimal.CreateFromObject(testField.ObjectValue);
		}
		catch (ArgumentException)
		{
			throw new NavNCLConversionException(testField.ObjectValue.GetType(), typeof(decimal));
		}
	}

	private static object SetToLocalKind(object value)
	{
		if (value is DateTime)
		{
			return DateTime.SpecifyKind((DateTime)value, DateTimeKind.Local);
		}
		return value;
	}

	public NavDate ALAsDate()
	{
		try
		{
			return NavDate.CreateFromObject(SetToLocalKind(testField.ObjectValue));
		}
		catch (ArgumentException)
		{
			throw new NavNCLConversionException(testField.ObjectValue.GetType(), typeof(NavDate));
		}
	}

	public NavTime ALAsTime()
	{
		try
		{
			return NavTime.CreateFromObject(SetToLocalKind(testField.ObjectValue));
		}
		catch (ArgumentException)
		{
			throw new NavNCLConversionException(testField.ObjectValue.GetType(), typeof(NavTime));
		}
	}

	public NavDateTime ALAsDateTime()
	{
		try
		{
			return NavDateTime.CreateFromClientTimeAllowingForNonexistentTime((DateTime)testField.ObjectValue);
		}
		catch (ArgumentException)
		{
			throw new NavNCLConversionException(testField.ObjectValue.GetType(), typeof(NavDateTime));
		}
	}

	public int ALOptionCount()
	{
		return testField.OptionCount;
	}

	public string ALGetOption(int index)
	{
		try
		{
			return testField.GetOption(index);
		}
		catch (IndexOutOfRangeException)
		{
			throw new NavNCLIndexOutOfBoundsException();
		}
	}

	public bool ALEnabled()
	{
		return testField.Enabled;
	}

	public bool ALEditable()
	{
		return testField.Editable;
	}

	public bool ALVisible()
	{
		return testField.Visible;
	}

	public bool ALHideValue()
	{
		return testField.HideValue;
	}

	public bool ALShowMandatory()
	{
		return testField.ShowMandatory;
	}

	public NavTestField(NavTestPageBase parent, ITestField testField)
	{
		this.parent = parent;
		this.testField = testField;
	}

	private void CheckError(FieldOperationVoid operation)
	{
		CheckError(() =>
		{
			operation();
			return 0;
		});
	}

	private T CheckError<T>(FieldOperationReturnValue<T> operation)
	{
		long lastUsedValidationErrorId = testField.LastUsedValidationErrorId;
		int validationErrorCount = testField.ValidationErrorCount;
		int num = parent.ALValidationErrorCount();
		T result = operation();
		if (testField.MaxValidationErrorId > lastUsedValidationErrorId)
		{
			throw NavTestValidationException.Create(CultureInfo.CurrentCulture, testField.Name, testField.GetValidationError(ALValidationErrorCount() - 1));
		}
		int num2 = num - (validationErrorCount - testField.ValidationErrorCount);
		if (parent.ALValidationErrorCount() > num2)
		{
			throw NavTestValidationException.Create(CultureInfo.CurrentCulture, parent.Name, parent.ALGetValidationError());
		}
		return result;
	}

	public override int GetHashCode()
	{
		if (testField != null)
		{
			return testField.GetHashCode();
		}
		return 0;
	}

	public override string ToString()
	{
		return ALValue;
	}
}
