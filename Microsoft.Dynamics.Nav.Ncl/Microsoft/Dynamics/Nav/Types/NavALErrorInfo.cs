using System;
using System.Globalization;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime;

namespace Microsoft.Dynamics.Nav.Types;

public sealed class NavALErrorInfo : NavValue
{
	public ALErrorType ALErrorType { get; set; }

	public ALVerbosity ALVerbosity { get; set; }

	public DataClassification ALDataClassification { get; set; }

	public string ALMessage { get; set; }

	public override int GetBytesSize => 12 + ALMessage.Length * 2;

	public override NavNclType NclType => NavNclType.NavErrorInfo;

	public override bool IsZeroOrEmpty => string.IsNullOrEmpty(ALMessage);

	public static NavALErrorInfo Default => new NavALErrorInfo();

	internal Verbosity Verbosity => (Verbosity)ALVerbosity;

	public void Clear()
	{
		ALErrorType = ALErrorType.Client;
		ALVerbosity = ALVerbosity.Error;
		ALDataClassification = DataClassification.CustomerContent;
		ALMessage = string.Empty;
	}

	public NavALErrorInfo()
	{
		Clear();
	}

	public NavALErrorInfo(ALErrorType errorType, ALVerbosity verbosity, DataClassification classification, string message)
	{
		ALErrorType = errorType;
		ALVerbosity = verbosity;
		ALDataClassification = classification;
		ALMessage = message;
	}

	public static NavALErrorInfo CreateFromObject(object clrErrorInfo)
	{
		if (clrErrorInfo is NavALErrorInfo navALErrorInfo && !string.IsNullOrEmpty(navALErrorInfo.ALMessage))
		{
			return new NavALErrorInfo(navALErrorInfo.ALErrorType, navALErrorInfo.ALVerbosity, navALErrorInfo.ALDataClassification, navALErrorInfo.ALMessage);
		}
		throw new ArgumentException(Lang.ParameterNullOrEmpty, "clrErrorInfo");
	}

	public override int GetHashCode()
	{
		return HashCodeHelper.CombineHashCodes((int)ALErrorType, (int)ALVerbosity, (int)ALDataClassification, ALMessage.GetHashCode());
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (GetType() != obj.GetType())
		{
			return false;
		}
		NavALErrorInfo navALErrorInfo = (NavALErrorInfo)obj;
		if (ALDataClassification == navALErrorInfo.ALDataClassification && ALVerbosity == navALErrorInfo.ALVerbosity && ALErrorType == navALErrorInfo.ALErrorType && ALMessage.Length == navALErrorInfo.ALMessage.Length)
		{
			return ALMessage.Equals(navALErrorInfo.ALMessage, StringComparison.CurrentCulture);
		}
		return false;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "{0};{1};{2};{3}", (int)ALDataClassification, (int)ALVerbosity, (int)ALErrorType, ALMessage);
	}
}
