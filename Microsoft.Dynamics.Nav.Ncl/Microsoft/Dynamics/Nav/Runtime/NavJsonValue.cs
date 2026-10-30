using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavJsonValue : NavJsonToken
{
	private const string NavDateSerializationFormat = "yyyy-MM-dd";

	private const string NavTimeSerializationFormat = "HH:mm:ss.FFFFFFF";

	private static readonly string[] AcceptedIso8601Formats = new string[16]
	{
		"yyyy-MM-ddTHH:mm:ss.fffffffK", "yyyyMMddTHH:mm:ss.fffffffK", "yyyy-MM-ddTHH:mm:ss.ffffffK", "yyyyMMddTHH:mm:ss.ffffffK", "yyyy-MM-ddTHH:mm:ss.fffffK", "yyyyMMddTHH:mm:ss.fffffK", "yyyy-MM-ddTHH:mm:ss.ffffK", "yyyyMMddTHH:mm:ss.ffffK", "yyyy-MM-ddTHH:mm:ss.fffK", "yyyyMMddTHH:mm:ss.fffK",
		"yyyy-MM-ddTHH:mm:ss.ffK", "yyyyMMddTHH:mm:ss.ffK", "yyyy-MM-ddTHH:mm:ss.fK", "yyyyMMddTHH:mm:ss.fK", "yyyy-MM-ddTHH:mm:ssK", "yyyyMMddTHH:mm:ssK"
	};

	public new static NavJsonValue Default => new NavJsonValue(JValue.CreateNull());

	public bool ALIsNull => IsNullValue;

	public bool ALIsUndefined => IsUndefined;

	public override NavNclType NclType => NavNclType.NavJsonValue;

	internal NavJsonValueType ValueType => GetNavJsonValueType(Value.Type);

	internal bool IsBoolean => ValueType == NavJsonValueType.Boolean;

	internal bool IsNumber => ValueType == NavJsonValueType.Number;

	internal bool IsString => ValueType == NavJsonValueType.String;

	private JValue Value => (JValue)BackingToken;

	private bool IsNullValue => ValueType == NavJsonValueType.Null;

	private bool IsUndefined => ValueType == NavJsonValueType.Undefined;

	public bool ALAsBoolean()
	{
		return AsBoolean();
	}

	public byte ALAsByte()
	{
		return AsByte();
	}

	public char ALAsChar()
	{
		return AsChar();
	}

	public NavOption ALAsOption()
	{
		return AsOption();
	}

	public int ALAsInteger()
	{
		return AsInteger();
	}

	public NavBigInteger ALAsBigInteger()
	{
		return AsBigInteger();
	}

	public Decimal18 ALAsDecimal()
	{
		return AsDecimal();
	}

	public NavText ALAsText()
	{
		return AsText();
	}

	public NavCode ALAsCode()
	{
		return AsCode();
	}

	public NavDate ALAsDate()
	{
		return AsDate();
	}

	public NavDateTime ALAsDateTime()
	{
		return AsDateTime();
	}

	public NavTime ALAsTime()
	{
		return AsTime();
	}

	public NavDuration ALAsDuration()
	{
		return AsDuration();
	}

	public void ALSetValue(bool value)
	{
		SetValue(value);
	}

	public void ALSetValue(int value)
	{
		SetValue(value);
	}

	public void ALSetValue(long value)
	{
		SetValue(value);
	}

	public void ALSetValue(Decimal18 value)
	{
		SetValue(value);
	}

	public void ALSetValue(string value)
	{
		SetValue(value);
	}

	public void ALSetValueToUndefined()
	{
		SetValueToUndefined();
	}

	public void ALSetValueToNull()
	{
		SetValueToNull();
	}

	public void ALSetValue(NavDate value)
	{
		SetValue(value);
	}

	public void ALSetValue(NavDateTime value)
	{
		SetValue(value);
	}

	public void ALSetValue(NavTime value)
	{
		SetValue(value);
	}

	public void ALSetValue(NavDuration value)
	{
		SetValue(value);
	}

	internal NavJsonValue(JValue val)
		: base(val)
	{
	}

	public new static NavJsonValue CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is NavJsonValue result)
		{
			return result;
		}
		if (value is JValue val)
		{
			return new NavJsonValue(val);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavJsonValue), value);
	}

	protected override bool ReadFrom(DataError errorLevel, string data)
	{
		return ReadFrom(errorLevel, data, verifyType: true);
	}

	protected override bool ReadFrom(DataError errorLevel, NavInStream data)
	{
		return ReadFrom(errorLevel, data, verifyType: true);
	}

	private static NavJsonValueType GetNavJsonValueType(JTokenType tokenType)
	{
		switch (tokenType)
		{
		case JTokenType.Boolean:
			return NavJsonValueType.Boolean;
		case JTokenType.Integer:
		case JTokenType.Float:
			return NavJsonValueType.Number;
		case JTokenType.Null:
			return NavJsonValueType.Null;
		case JTokenType.Undefined:
			return NavJsonValueType.Undefined;
		case JTokenType.Comment:
		case JTokenType.String:
		case JTokenType.Date:
		case JTokenType.Bytes:
		case JTokenType.Guid:
		case JTokenType.Uri:
		case JTokenType.TimeSpan:
			return NavJsonValueType.String;
		default:
			throw new ArgumentException(tokenType.ToString(), "tokenType");
		}
	}

	private static TTargetType ExecuteConversion<TTargetType>(Func<TTargetType> operation)
	{
		TTargetType result = default;
		TrappableOperationExecutor.Execute(DataError.ThrowError, () =>
		{
			result = operation();
			return true;
		}, (Exception exception, DataError error) =>
		{
			LogWriterHelper.LogExceptionEvent(exception);
			if (exception is FormatException || exception is ArgumentException || exception is OverflowException || exception is JsonReaderException)
			{
				throw new NavNCLConversionException(typeof(NavJsonValue), typeof(TTargetType));
			}
		});
		return result;
	}

	private void SetValue(bool value)
	{
		Initialize(new JValue(value));
	}

	private void SetValue(int value)
	{
		Initialize(new JValue(value));
	}

	private void SetValue(long value)
	{
		Initialize(new JValue(value));
	}

	private void SetValue(Decimal18 value)
	{
		Initialize(new JValue(Decimal18.ToDecimal(value)));
	}

	private void SetValue(string value)
	{
		Initialize(new JValue(value));
	}

	private void SetValueToUndefined()
	{
		Initialize(JValue.CreateUndefined());
	}

	private void SetValueToNull()
	{
		Initialize(JValue.CreateNull());
	}

	private void SetValue(NavDate value)
	{
		string value2 = value.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
		Initialize(new JValue(value2));
	}

	private void SetValue(NavDateTime value)
	{
		string value2 = value.Value.ToString("o", CultureInfo.InvariantCulture);
		Initialize(new JValue(value2));
	}

	private void SetValue(NavTime value)
	{
		string value2 = value.Value.ToString("HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture);
		Initialize(new JValue(value2));
	}

	private void SetValue(NavDuration value)
	{
		SetValue(value.Value);
	}

	private bool AsBoolean()
	{
		return ExecuteConversion(() => (bool)(JToken)Value);
	}

	private byte AsByte()
	{
		return ExecuteConversion(() => (byte)(JToken)Value);
	}

	private char AsChar()
	{
		return ExecuteConversion(() => (char)(JToken)Value);
	}

	private NavOption AsOption()
	{
		return ExecuteConversion(() =>
		{
			int value = (int)(JToken)Value;
			return NavOption.Create(NCLOptionMetadata.Default, value);
		});
	}

	private int AsInteger()
	{
		return ExecuteConversion(() => (int)(JToken)Value);
	}

	private NavBigInteger AsBigInteger()
	{
		return ExecuteConversion(() =>
		{
			long value = (long)(JToken)Value;
			return NavBigInteger.Create(value);
		});
	}

	private Decimal18 AsDecimal()
	{
		return ExecuteConversion(() =>
		{
			decimal value = (decimal)(JToken)Value;
			return new Decimal18(value);
		});
	}

	private NavText AsText()
	{
		return ExecuteConversion(() =>
		{
			if (IsUndefined || IsNullValue)
			{
				throw new NavNCLConversionException(typeof(NavJsonValue), typeof(NavText));
			}
			string text = (string)(JToken)Value;
			if (Value.Type == JTokenType.Boolean)
			{
				text = text.ToLowerInvariant();
			}
			return NavText.Create(text);
		});
	}

	private NavCode AsCode()
	{
		return ExecuteConversion(() =>
		{
			if (IsUndefined || IsNullValue)
			{
				throw new NavNCLConversionException(typeof(NavJsonValue), typeof(NavText));
			}
			string text = (string)(JToken)Value;
			if (Value.Type == JTokenType.Boolean)
			{
				text = text.ToLowerInvariant();
			}
			return new NavCode(2048, text);
		});
	}

	private NavDate AsDate()
	{
		return ExecuteConversion(() =>
		{
			string s = (string)(JToken)Value;
			DateTime initValue = DateTime.ParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
			return NavDate.Create(initValue);
		});
	}

	private NavTime AsTime()
	{
		return ExecuteConversion(() =>
		{
			string s = (string)(JToken)Value;
			DateTime dateTime = DateTime.ParseExact(s, "HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
			return NavTime.Create(dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond);
		});
	}

	private NavDateTime AsDateTime()
	{
		return ExecuteConversion(() =>
		{
			string s = (string)(JToken)Value;
			DateTime dateTime = DateTime.ParseExact(s, AcceptedIso8601Formats, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
			if (dateTime.Kind == DateTimeKind.Local)
			{
				dateTime = dateTime.ToUniversalTime();
			}
			else if (dateTime.Kind == DateTimeKind.Unspecified)
			{
				dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
			}
			return NavDateTime.Create(dateTime);
		});
	}

	private NavDuration AsDuration()
	{
		return ExecuteConversion(() =>
		{
			long initValue = (long)(JToken)Value;
			return new NavDuration(initValue);
		});
	}

	private void Initialize(JValue value)
	{
		SetBackingToken(value, verifyType: true);
	}
}
