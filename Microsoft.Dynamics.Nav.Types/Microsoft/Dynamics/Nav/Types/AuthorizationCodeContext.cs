using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract]
public sealed class AuthorizationCodeContext : IObjectReference, IEquatable<AuthorizationCodeContext>
{
	public static AuthorizationCodeContext Empty { get; } = new AuthorizationCodeContext();

	[DataMember(Name = "a")]
	public string AuthorizationCode { get; private set; }

	[DataMember(Name = "r")]
	public string RedirectUrl { get; private set; }

	private AuthorizationCodeContext(string authorizationCode, string redirectUrl)
	{
		AuthorizationCode = authorizationCode;
		RedirectUrl = redirectUrl;
	}

	private AuthorizationCodeContext()
	{
	}

	public static AuthorizationCodeContext Create(string authorizationCode, Uri redirectUri)
	{
		return Create(authorizationCode, redirectUri?.GetLeftPart(UriPartial.Path));
	}

	public static AuthorizationCodeContext Create(string authorizationCode, string redirectUrl)
	{
		if (string.IsNullOrWhiteSpace(authorizationCode) || string.IsNullOrWhiteSpace(redirectUrl))
		{
			return Empty;
		}
		return new AuthorizationCodeContext(authorizationCode, redirectUrl);
	}

	public static bool IsNullOrEmpty(AuthorizationCodeContext authorizationCodeContext)
	{
		if (!(authorizationCodeContext == null))
		{
			return authorizationCodeContext == Empty;
		}
		return true;
	}

	object IObjectReference.GetRealObject(StreamingContext context)
	{
		if (this == Empty)
		{
			return Empty;
		}
		return this;
	}

	public bool Equals(AuthorizationCodeContext other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)other == this)
		{
			return true;
		}
		if (string.Equals(AuthorizationCode, other.AuthorizationCode, StringComparison.Ordinal))
		{
			return string.Equals(RedirectUrl, other.RedirectUrl, StringComparison.Ordinal);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as AuthorizationCodeContext);
	}

	public override int GetHashCode()
	{
		return (AuthorizationCode + RedirectUrl).GetHashCode();
	}

	public static bool operator ==(AuthorizationCodeContext objA, AuthorizationCodeContext objB)
	{
		return object.Equals(objA, objB);
	}

	public static bool operator !=(AuthorizationCodeContext objA, AuthorizationCodeContext objB)
	{
		return !object.Equals(objA, objB);
	}
}
