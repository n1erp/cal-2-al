using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq.JsonPath;

internal class BooleanQueryExpression : QueryExpression
{
	public object Left { get; set; }

	public object Right { get; set; }

	private IEnumerable<JToken> GetResult(JToken root, JToken t, object o)
	{
		if (o is JToken jToken)
		{
			return new JToken[1] { jToken };
		}
		if (o is List<PathFilter> filters)
		{
			return JPath.Evaluate(filters, root, t, errorWhenNoMatch: false);
		}
		return CollectionUtils.ArrayEmpty<JToken>();
	}

	public override bool IsMatch(JToken root, JToken t)
	{
		if (Operator == QueryOperator.Exists)
		{
			return GetResult(root, t, Left).Any();
		}
		using (IEnumerator<JToken> enumerator = GetResult(root, t, Left).GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				IEnumerable<JToken> result = GetResult(root, t, Right);
				ICollection<JToken> collection = (result as ICollection<JToken>) ?? result.ToList();
				do
				{
					JToken current = enumerator.Current;
					foreach (JToken item in collection)
					{
						if (MatchTokens(current, item))
						{
							return true;
						}
					}
				}
				while (enumerator.MoveNext());
			}
		}
		return false;
	}

	private bool MatchTokens(JToken leftResult, JToken rightResult)
	{
		JValue jValue = leftResult as JValue;
		JValue jValue2 = rightResult as JValue;
		if (jValue != null && jValue2 != null)
		{
			switch (Operator)
			{
			case QueryOperator.Equals:
				if (EqualsWithStringCoercion(jValue, jValue2))
				{
					return true;
				}
				break;
			case QueryOperator.NotEquals:
				if (!EqualsWithStringCoercion(jValue, jValue2))
				{
					return true;
				}
				break;
			case QueryOperator.GreaterThan:
				if (jValue.CompareTo(jValue2) > 0)
				{
					return true;
				}
				break;
			case QueryOperator.GreaterThanOrEquals:
				if (jValue.CompareTo(jValue2) >= 0)
				{
					return true;
				}
				break;
			case QueryOperator.LessThan:
				if (jValue.CompareTo(jValue2) < 0)
				{
					return true;
				}
				break;
			case QueryOperator.LessThanOrEquals:
				if (jValue.CompareTo(jValue2) <= 0)
				{
					return true;
				}
				break;
			case QueryOperator.Exists:
				return true;
			}
		}
		else
		{
			QueryOperator queryOperator = Operator;
			if (queryOperator == QueryOperator.NotEquals || queryOperator == QueryOperator.Exists)
			{
				return true;
			}
		}
		return false;
	}

	private bool EqualsWithStringCoercion(JValue value, JValue queryValue)
	{
		if (value.Equals(queryValue))
		{
			return true;
		}
		if (queryValue.Type != JTokenType.String)
		{
			return false;
		}
		string b = (string)queryValue.Value;
		string a;
		switch (value.Type)
		{
		case JTokenType.Date:
		{
			using (StringWriter stringWriter = StringUtils.CreateStringWriter(64))
			{
				if (value.Value is DateTimeOffset)
				{
					DateTimeUtils.WriteDateTimeOffsetString(stringWriter, (DateTimeOffset)value.Value, DateFormatHandling.IsoDateFormat, null, CultureInfo.InvariantCulture);
				}
				else
				{
					DateTimeUtils.WriteDateTimeString(stringWriter, (DateTime)value.Value, DateFormatHandling.IsoDateFormat, null, CultureInfo.InvariantCulture);
				}
				a = stringWriter.ToString();
			}
			break;
		}
		case JTokenType.Bytes:
			a = Convert.ToBase64String((byte[])value.Value);
			break;
		case JTokenType.Guid:
		case JTokenType.TimeSpan:
			a = value.Value.ToString();
			break;
		case JTokenType.Uri:
			a = ((Uri)value.Value).OriginalString;
			break;
		default:
			return false;
		}
		return string.Equals(a, b, StringComparison.Ordinal);
	}
}
