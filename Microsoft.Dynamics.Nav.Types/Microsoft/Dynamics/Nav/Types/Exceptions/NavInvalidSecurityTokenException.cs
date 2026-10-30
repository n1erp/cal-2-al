using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.ServiceModel;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavInvalidSecurityTokenException : NavException
{
	public const string InvalidSecurityTokenFaultCode = "Invalid security token - do not show in UI!";

	public const string WrongTokenTypeFaultCode = "Lang.WrongTokenType";

	public const string TokenHasExpiredFaultCode = "Lang.TokenHasExpired";

	public const string IssuerNotTrustedFaultCode = "Lang.IssuerNotTrusted";

	public const string WrongTokenSignatureFaultCode = "Lang.WrongTokenSignature";

	public const string TokenReplayedFaultCode = "Lang.TokenReplayed";

	public const string TokenEmptyFaultCode = "Lang.ExchangeTokenEmpty";

	public const string TokenAudienceFaultCode = "Lang.ExchangeTokenAudience";

	public const string TokenMetadataFaultCode = "Lang.ExchangeTokenMetadata";

	public const string TokenUriFaultCode = "Lang.ExchangeTokenUri";

	public const string TokenContentFaultCode = "Lang.TokenContent";

	[NonSerialized]
	private string localFaultCode;

	public NavInvalidSecurityTokenException()
	{
	}

	public NavInvalidSecurityTokenException(string message)
		: base(message)
	{
	}

	public NavInvalidSecurityTokenException(string message, Exception exception)
		: base(message, exception)
	{
	}

	protected NavInvalidSecurityTokenException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}

	public static NavInvalidSecurityTokenException Create(string faultCode)
	{
		return Create(faultCode, null);
	}

	public static NavInvalidSecurityTokenException Create(string faultCode, Exception exception, params string[] faultArguments)
	{
		switch (faultCode)
		{
		case "Lang.WrongTokenSignature":
			return new NavInvalidSecurityTokenException(Lang.WrongTokenSignature, exception)
			{
				localFaultCode = faultCode
			};
		case "Lang.TokenHasExpired":
			return new NavInvalidSecurityTokenException(Lang.TokenHasExpired, exception)
			{
				localFaultCode = faultCode
			};
		case "Lang.WrongTokenType":
			return new NavInvalidSecurityTokenException(Lang.WrongTokenType, exception)
			{
				localFaultCode = faultCode
			};
		case "Lang.TokenReplayed":
			return new NavInvalidSecurityTokenException(Lang.TokenReplayed, exception)
			{
				localFaultCode = faultCode
			};
		case "Lang.IssuerNotTrusted":
			return new NavInvalidSecurityTokenException(Lang.IssuerNotTrusted, exception)
			{
				localFaultCode = faultCode
			};
		case "Lang.ExchangeTokenEmpty":
			return new NavInvalidSecurityTokenException(Lang.ExchangeTokenEmpty, exception)
			{
				localFaultCode = faultCode
			};
		case "Lang.ExchangeTokenAudience":
			return new NavInvalidSecurityTokenException(exception.Message, exception)
			{
				localFaultCode = faultCode
			};
		case "Lang.ExchangeTokenMetadata":
			return new NavInvalidSecurityTokenException(string.Format(CultureInfo.CurrentCulture, Lang.ExchangeTokenMetadata, faultArguments), exception)
			{
				localFaultCode = faultCode
			};
		case "Lang.ExchangeTokenUri":
			return new NavInvalidSecurityTokenException(Lang.ExchangeTokenUri, exception)
			{
				localFaultCode = faultCode
			};
		case "Lang.TokenContent":
			return new NavInvalidSecurityTokenException(Lang.ExchangeTokenContent, exception)
			{
				localFaultCode = faultCode
			};
		default:
			return new NavInvalidSecurityTokenException(Lang.AuthenticationTechnicalIssueMessage, exception)
			{
				localFaultCode = "Invalid security token - do not show in UI!"
			};
		}
	}

	public static FaultException CreateFaultException(NavInvalidSecurityTokenException exception)
	{
		FaultCode code = FaultCode.CreateSenderFaultCode(new FaultCode(exception.localFaultCode));
		return new FaultException(exception.Message, code);
	}
}
