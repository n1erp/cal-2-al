using System;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.LicensingService.Model;
using Microsoft.Dynamics.Nav.LicensingService.Model.V10;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class UserSecurityTokenHelper
{
	public INavSecurityToken SecurityToken { get; set; }

	public string AuthenticationObjectId { get; set; }

	public Guid NavUserId { get; set; }

	public UserInfo User { get; set; }

	public string AuthenticationEmail
	{
		get
		{
			string text = string.Empty;
			if (SecurityToken != null)
			{
				text = SecurityTokenExtensionMethods.AuthenticationEmail(SecurityToken.GetClaimValue);
			}
			if (string.IsNullOrWhiteSpace(text) && User != null)
			{
				text = ((UserInfo)User).UserPrincipalName;
			}
			return text;
		}
	}

	public string FullName
	{
		get
		{
			string text = string.Empty;
			if (SecurityToken != null)
			{
				text = SecurityToken.GetClaimValue(SecurityTokenClaimType.DisplayName);
			}
			if (string.IsNullOrWhiteSpace(text) && User != null)
			{
				text = ((UserInfo)User).DisplayName;
			}
			return text;
		}
	}

	public string ContactEmail
	{
		get
		{
			string text = string.Empty;
			if (SecurityToken != null)
			{
				text = SecurityToken.GetClaimValue(SecurityTokenClaimType.EmailAddress);
			}
			if (string.IsNullOrWhiteSpace(text) && User != null)
			{
				text = ((((UserInfo)User).OtherMails != null && ((UserInfo)User).OtherMails.ToList().Count > 0) ? ((UserInfo)User).OtherMails.ToList()[0] : string.Empty);
			}
			return text;
		}
	}

	public string NameIdentifier => SecurityToken?.GetClaimValue(SecurityTokenClaimType.NameIdentifier);

	public string DirectoryRoleId
	{
		get
		{
			if (SecurityToken != null)
			{
				return SecurityToken.GetClaimValue(SecurityTokenClaimType.Wids);
			}
			return string.Empty;
		}
	}

	private UserSecurityTokenHelper()
	{
	}

	public static UserSecurityTokenHelper Create(string objectId, INavSecurityToken securityToken, UserInfo user = null)
	{
		return new UserSecurityTokenHelper
		{
			SecurityToken = securityToken,
			AuthenticationObjectId = (((user != null) ? ((UserInfo)user).ObjectId : null) ?? objectId),
			User = user
		};
	}

	public DiagnosticParameter GetDetailsForLog()
	{
		if (SecurityToken != null)
		{
			return SecurityToken.GetDetailsForLog();
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Authentication ObjectId: " + AuthenticationObjectId);
		stringBuilder.AppendLine("User ID: " + NavUserId.ToString());
		stringBuilder.AppendLine("Fullname: " + FullName);
		stringBuilder.AppendLine("AuthenticationEmail:" + AuthenticationEmail);
		stringBuilder.AppendLine("ContactEmail:" + ContactEmail);
		stringBuilder.AppendLine("NameIdentifier:" + NameIdentifier);
		return DiagnosticParameter.Euii(stringBuilder);
	}

	public string GetUserName()
	{
		string authenticationEmail = AuthenticationEmail;
		string text;
		if (authenticationEmail != null && authenticationEmail.Contains('@'))
		{
			text = AuthenticationEmail;
		}
		else
		{
			text = (string.IsNullOrEmpty(ContactEmail) ? FullName : ContactEmail);
		}
		string text2 = text.Split('@')[0].Trim();
		if (text2.Length > 50)
		{
			text2 = text2.Substring(0, 50);
		}
		return text2;
	}
}
