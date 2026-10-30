using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public struct EvaluateBag
{
	[DataMember]
	private DateTime dateTimeValue;

	[DataMember]
	private long millisecondsDurationValue;

	[DataMember]
	private string dateFormulaValue;

	[DataMember]
	private bool boolean;

	[DataMember]
	private long bigInt;

	[DataMember]
	private int integer;

	[DataMember]
	private string code;

	[DataMember]
	private decimal dec;

	[DataMember]
	private byte[] recordId;

	public DateTime DateTimeValue
	{
		get
		{
			return dateTimeValue;
		}
		set
		{
			dateTimeValue = value;
		}
	}

	public long MillisecondsDurationValue
	{
		get
		{
			return millisecondsDurationValue;
		}
		set
		{
			millisecondsDurationValue = value;
		}
	}

	public string DateFormulaValue
	{
		get
		{
			return dateFormulaValue;
		}
		set
		{
			dateFormulaValue = value;
		}
	}

	public long BigInt
	{
		get
		{
			return bigInt;
		}
		set
		{
			bigInt = value;
		}
	}

	public int Integer
	{
		get
		{
			return integer;
		}
		set
		{
			integer = value;
		}
	}

	public string Code
	{
		get
		{
			return code;
		}
		set
		{
			code = value;
		}
	}

	public byte[] RecordId
	{
		get
		{
			return recordId;
		}
		set
		{
			recordId = value;
		}
	}

	public decimal Decimal
	{
		get
		{
			return dec;
		}
		set
		{
			dec = value;
		}
	}

	public bool Boolean
	{
		get
		{
			return boolean;
		}
		set
		{
			boolean = value;
		}
	}

	public object GetValue(NavType navType)
	{
		switch (navType)
		{
		case NavType.Decimal:
			return Decimal;
		case NavType.Integer:
			return Integer;
		case NavType.RecordID:
			return RecordId;
		case NavType.Code:
		case NavType.OemCode:
			return Code;
		case NavType.Boolean:
			return Boolean;
		case NavType.DateFormula:
			return DateFormulaValue;
		case NavType.Date:
		case NavType.Time:
		case NavType.DateTime:
			return DateTimeValue;
		case NavType.Duration:
			return TimeSpan.FromMilliseconds(MillisecondsDurationValue);
		case NavType.BigInteger:
			return BigInt;
		default:
			return null;
		}
	}

	public override bool Equals(object obj)
	{
		if (!(obj is EvaluateBag evaluateBag))
		{
			return false;
		}
		if (DateTimeValue == evaluateBag.DateTimeValue)
		{
			return MillisecondsDurationValue == evaluateBag.MillisecondsDurationValue;
		}
		return false;
	}

	public static bool operator ==(EvaluateBag bag1, EvaluateBag bag2)
	{
		return bag1.Equals(bag2);
	}

	public static bool operator !=(EvaluateBag bag1, EvaluateBag bag2)
	{
		return !(bag1 == bag2);
	}

	public override int GetHashCode()
	{
		return dateTimeValue.GetHashCode() ^ millisecondsDurationValue.GetHashCode();
	}
}
