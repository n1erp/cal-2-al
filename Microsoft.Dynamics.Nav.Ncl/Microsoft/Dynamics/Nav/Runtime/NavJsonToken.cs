using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavJsonToken : NavValue, IEquatable<NavJsonToken>
{
	private static class TrappableJsonOperationExecutor
	{
		public static bool ExecuteOperation(DataError errorLevel, Action operation)
		{
			return TrappableOperationExecutor.Execute(errorLevel, operation, MapNativeException);
		}

		private static Exception MapNativeException(Exception exception)
		{
			if (exception is ArgumentOutOfRangeException)
			{
				return new NavNCLArgumentOutOfRangeException();
			}
			if (exception is JsonReaderException)
			{
				return new NavNCLInvalidJsonDataException();
			}
			if (exception is JsonException)
			{
				return new NavNCLInvalidOperationException();
			}
			return exception;
		}
	}

	private static readonly JsonLoadSettings DefaultJsonLoadSettings = new JsonLoadSettings
	{
		CommentHandling = CommentHandling.Ignore,
		LineInfoHandling = LineInfoHandling.Ignore
	};

	public string ALPath => Path;

	public bool ALIsObject => IsObject;

	public bool ALIsValue => IsValue;

	public bool ALIsArray => IsArray;

	public static NavJsonToken Default => Create(JValue.CreateNull());

	internal JToken BackingToken { get; private set; }

	private string Path => BackingToken.Path;

	private NavJsonTokenType Type => GetNavJsonTokenType(BackingToken);

	private bool IsObject => Type == NavJsonTokenType.Object;

	private bool IsValue => Type == NavJsonTokenType.Value;

	private bool IsArray => Type == NavJsonTokenType.Array;

	public override int GetBytesSize => 0;

	public override bool IsZeroOrEmpty => BackingToken == null;

	public override NavNclType NclType => NavNclType.NavJsonToken;

	public bool ALReadFrom(DataError errorLevel, string data)
	{
		return ReadFrom(errorLevel, data);
	}

	public bool ALReadFrom(DataError errorLevel, NavInStream data)
	{
		return ReadFrom(errorLevel, data);
	}

	public bool ALWriteTo(DataError errorLevel, ByRef<NavText> data)
	{
		return WriteTo(errorLevel, data);
	}

	public bool ALWriteTo(DataError errorLevel, NavOutStream data)
	{
		return WriteTo(errorLevel, data);
	}

	public bool ALSelectToken(DataError errorLevel, string path, ByRef<NavJsonToken> result)
	{
		return SelectToken(errorLevel, path, result);
	}

	public bool ALSelectTokens(DataError errorLevel, string path, ByRef<NavList<NavJsonToken>> result)
	{
		return SelectTokens(errorLevel, path, result);
	}

	public NavJsonArray ALAsArray()
	{
		return AsArray();
	}

	public NavJsonValue ALAsValue()
	{
		return AsValue();
	}

	public NavJsonObject ALAsObject()
	{
		return AsObject();
	}

	public virtual NavJsonToken ALClone()
	{
		return Clone();
	}

	public virtual void ALClear()
	{
		Clear();
	}

	public NavJsonToken ALAsToken()
	{
		return Create(BackingToken);
	}

	protected NavJsonToken(JToken token)
	{
		SetBackingToken(token);
	}

	public bool Equals(NavJsonToken other)
	{
		if (other == null)
		{
			return false;
		}
		return JToken.DeepEquals(BackingToken, other.BackingToken);
	}

	public static NavJsonToken Create(JToken token)
	{
		if (token == null)
		{
			throw new NavNCLArgumentException();
		}
		switch (token.Type)
		{
		case JTokenType.Object:
		case JTokenType.Array:
		case JTokenType.Comment:
		case JTokenType.Integer:
		case JTokenType.Float:
		case JTokenType.String:
		case JTokenType.Boolean:
		case JTokenType.Null:
		case JTokenType.Undefined:
		case JTokenType.Date:
		case JTokenType.Bytes:
		case JTokenType.Guid:
		case JTokenType.Uri:
		case JTokenType.TimeSpan:
			return new NavJsonToken(token);
		default:
			throw new NavNCLInvalidJsonTypeException(token.Type);
		}
	}

	public static NavJsonToken CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is NavJsonToken result)
		{
			return result;
		}
		if (value is JToken token)
		{
			return Create(token);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavJsonToken), value);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as NavJsonToken);
	}

	protected virtual bool ReadFrom(DataError errorLevel, string data)
	{
		return ReadFrom(errorLevel, data, verifyType: false);
	}

	protected virtual bool ReadFrom(DataError errorLevel, NavInStream data)
	{
		return ReadFrom(errorLevel, data, verifyType: false);
	}

	protected virtual NavJsonToken Clone()
	{
		JToken token = BackingToken.DeepClone();
		return Create(token);
	}

	protected virtual void Clear()
	{
		SetBackingToken(Default.BackingToken);
	}

	protected bool ReadFrom(DataError errorLevel, string data, bool verifyType)
	{
		return TrappableJsonOperationExecutor.ExecuteOperation(errorLevel, () =>
		{
			using StringReader reader = new StringReader(data);
			using JsonTextReader reader2 = new JsonTextReader(reader)
			{
				DateParseHandling = DateParseHandling.None
			};
			JToken value = JToken.ReadFrom(reader2);
			SetBackingToken(value, verifyType);
		});
	}

	protected bool ReadFrom(DataError errorLevel, NavInStream data, bool verifyType)
	{
		return TrappableJsonOperationExecutor.ExecuteOperation(errorLevel, () =>
		{
			using StreamReader reader = new StreamReader(data.InternalStream);
			using JsonTextReader reader2 = new JsonTextReader(reader)
			{
				DateParseHandling = DateParseHandling.None
			};
			JToken value = JToken.ReadFrom(reader2, DefaultJsonLoadSettings);
			SetBackingToken(value, verifyType);
		});
	}

	protected void SetBackingToken(JToken value, bool verifyType = false)
	{
		if (value == null)
		{
			throw new NavNCLArgumentException("value");
		}
		if (GetNavJsonTokenType(value) == NavJsonTokenType.Unsupported)
		{
			throw new NavNCLInvalidJsonTypeException(value.Type);
		}
		if (verifyType && GetNavJsonTokenType(value) != Type)
		{
			throw new NavNCLInvalidJsonTypeException(value.Type);
		}
		BackingToken = value;
	}

	private bool WriteTo(DataError errorLevel, ByRef<NavText> data)
	{
		return TrappableJsonOperationExecutor.ExecuteOperation(errorLevel, () =>
		{
			using StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
			using JsonWriter writer = GetJsonWriter(stringWriter);
			BackingToken.WriteTo(writer);
			data.Value = new NavText(data.Value.MaxLength, stringWriter.ToString());
		});
	}

	private bool WriteTo(DataError errorLevel, NavOutStream data)
	{
		return TrappableJsonOperationExecutor.ExecuteOperation(errorLevel, () =>
		{
			using StreamWriter textWriter = new StreamWriter(data.InternalStream);
			using JsonWriter writer = GetJsonWriter(textWriter);
			BackingToken.WriteTo(writer);
		});
	}

	private bool SelectToken(DataError errorLevel, string path, ByRef<NavJsonToken> result)
	{
		return TrappableJsonOperationExecutor.ExecuteOperation(errorLevel, () =>
		{
			List<JToken> list = BackingToken.SelectTokens(path, errorWhenNoMatch: false).Where(IsSupportedTokenType).ToList();
			switch (list.Count)
			{
			case 0:
				throw new NavNCLInvalidJsonPathSelectionException(string.Format(CultureInfo.CurrentCulture, Lang.NavJsonPathQueryDidNotProduceAnyResults, path));
			case 1:
				result.Value = Create(list[0]);
				break;
			default:
				throw new NavNCLInvalidJsonPathSelectionException(string.Format(CultureInfo.CurrentCulture, Lang.NavJsonPathQueryProducedMoreThanOneResult, path));
			}
		});
	}

	private bool SelectTokens(DataError errorLevel, string path, ByRef<NavList<NavJsonToken>> result)
	{
		return TrappableJsonOperationExecutor.ExecuteOperation(errorLevel, () =>
		{
			IEnumerable<JToken> tokens = BackingToken.SelectTokens(path, errorWhenNoMatch: false);
			result.Value = ConvertToList(tokens);
		});
	}

	private NavJsonArray AsArray()
	{
		if (!IsArray)
		{
			throw new NavNCLConversionException(GetType(), typeof(NavJsonArray));
		}
		return NavJsonArray.Create((JArray)BackingToken);
	}

	private NavJsonValue AsValue()
	{
		if (!IsValue)
		{
			throw new NavNCLConversionException(GetType(), typeof(NavJsonValue));
		}
		return new NavJsonValue((JValue)BackingToken);
	}

	private NavJsonObject AsObject()
	{
		if (!IsObject)
		{
			throw new NavNCLConversionException(GetType(), typeof(NavJsonObject));
		}
		return NavJsonObject.Create((JObject)BackingToken);
	}

	private static JsonWriter GetJsonWriter(TextWriter textWriter)
	{
		return new JsonTextWriter(textWriter)
		{
			DateFormatHandling = DateFormatHandling.IsoDateFormat,
			DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
			Formatting = Formatting.None
		};
	}

	private static NavJsonTokenType GetNavJsonTokenType(JToken token)
	{
		switch (token.Type)
		{
		case JTokenType.None:
		case JTokenType.Constructor:
		case JTokenType.Property:
		case JTokenType.Raw:
			return NavJsonTokenType.Unsupported;
		case JTokenType.Object:
			return NavJsonTokenType.Object;
		case JTokenType.Array:
			return NavJsonTokenType.Array;
		case JTokenType.Comment:
		case JTokenType.Integer:
		case JTokenType.Float:
		case JTokenType.String:
		case JTokenType.Boolean:
		case JTokenType.Null:
		case JTokenType.Undefined:
		case JTokenType.Date:
		case JTokenType.Bytes:
		case JTokenType.Guid:
		case JTokenType.Uri:
		case JTokenType.TimeSpan:
			return NavJsonTokenType.Value;
		default:
			throw new ArgumentOutOfRangeException("token", token.Type.ToString());
		}
	}

	private static bool IsSupportedTokenType(JToken token)
	{
		switch (token.Type)
		{
		case JTokenType.Object:
		case JTokenType.Array:
		case JTokenType.Integer:
		case JTokenType.Float:
		case JTokenType.String:
		case JTokenType.Boolean:
		case JTokenType.Null:
		case JTokenType.Undefined:
		case JTokenType.Date:
		case JTokenType.Bytes:
		case JTokenType.Guid:
		case JTokenType.Uri:
		case JTokenType.TimeSpan:
			return true;
		default:
			return false;
		}
	}

	private static NavList<NavJsonToken> ConvertToList(IEnumerable<JToken> tokens)
	{
		if (tokens == null)
		{
			return NavList<NavJsonToken>.Default;
		}
		List<NavJsonToken> list = new List<NavJsonToken>();
		foreach (JToken token in tokens)
		{
			if (IsSupportedTokenType(token))
			{
				list.Add(Create(token));
			}
		}
		return new NavList<NavJsonToken>(list);
	}

	public override int GetHashCode()
	{
		return BackingToken?.GetHashCode() ?? 0;
	}

	public static implicit operator NavJsonToken(bool value)
	{
		NavJsonValue navJsonValue = NavJsonValue.Default;
		navJsonValue.ALSetValue(value);
		return navJsonValue;
	}

	public static implicit operator NavJsonToken(byte value)
	{
		NavJsonValue navJsonValue = NavJsonValue.Default;
		navJsonValue.ALSetValue(value);
		return navJsonValue;
	}

	public static implicit operator NavJsonToken(char value)
	{
		NavJsonValue navJsonValue = NavJsonValue.Default;
		navJsonValue.ALSetValue(value);
		return navJsonValue;
	}

	public static implicit operator NavJsonToken(int value)
	{
		NavJsonValue navJsonValue = NavJsonValue.Default;
		navJsonValue.ALSetValue(value);
		return navJsonValue;
	}

	public static implicit operator NavJsonToken(NavBigInteger value)
	{
		NavJsonValue navJsonValue = NavJsonValue.Default;
		navJsonValue.ALSetValue(value);
		return navJsonValue;
	}

	public static implicit operator NavJsonToken(long value)
	{
		NavJsonValue navJsonValue = NavJsonValue.Default;
		navJsonValue.ALSetValue(value);
		return navJsonValue;
	}

	public static implicit operator NavJsonToken(decimal value)
	{
		NavJsonValue navJsonValue = NavJsonValue.Default;
		navJsonValue.ALSetValue(value);
		return navJsonValue;
	}

	public static implicit operator NavJsonToken(Decimal18 value)
	{
		NavJsonValue navJsonValue = NavJsonValue.Default;
		navJsonValue.ALSetValue(value);
		return navJsonValue;
	}

	public static implicit operator NavJsonToken(NavOption value)
	{
		NavJsonValue navJsonValue = NavJsonValue.Default;
		navJsonValue.ALSetValue(value);
		return navJsonValue;
	}

	public static implicit operator NavJsonToken(NavCode value)
	{
		NavJsonValue navJsonValue = NavJsonValue.Default;
		navJsonValue.ALSetValue(value);
		return navJsonValue;
	}

	public static implicit operator NavJsonToken(NavText value)
	{
		NavJsonValue navJsonValue = NavJsonValue.Default;
		navJsonValue.ALSetValue(value);
		return navJsonValue;
	}

	public static implicit operator NavJsonToken(NavTextConstant value)
	{
		NavJsonValue navJsonValue = NavJsonValue.Default;
		navJsonValue.ALSetValue(value);
		return navJsonValue;
	}

	public static implicit operator NavJsonToken(string value)
	{
		NavJsonValue navJsonValue = NavJsonValue.Default;
		navJsonValue.ALSetValue(value);
		return navJsonValue;
	}

	public static implicit operator NavJsonToken(NavDateTime value)
	{
		NavJsonValue navJsonValue = NavJsonValue.Default;
		navJsonValue.ALSetValue(value);
		return navJsonValue;
	}

	public static implicit operator NavJsonToken(NavTime value)
	{
		NavJsonValue navJsonValue = NavJsonValue.Default;
		navJsonValue.ALSetValue(value);
		return navJsonValue;
	}

	public static implicit operator NavJsonToken(NavDate value)
	{
		NavJsonValue navJsonValue = NavJsonValue.Default;
		navJsonValue.ALSetValue(value);
		return navJsonValue;
	}

	public static implicit operator NavJsonToken(NavDuration value)
	{
		NavJsonValue navJsonValue = NavJsonValue.Default;
		navJsonValue.ALSetValue(value);
		return navJsonValue;
	}
}
