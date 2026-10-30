using System;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSessionSettings : NavValue, IEquatable<NavSessionSettings>
{
	public class Factory : ElementFactory<NavSessionSettings>
	{
		public Factory(ITreeObject parentForArray)
			: base(parentForArray)
		{
		}

		public override NavSessionSettings CreateElement()
		{
			return new NavSessionSettings();
		}

		public override void AssignElement(NavSessionSettings destinationElement, NavSessionSettings sourceElement)
		{
			if (destinationElement == null)
			{
				throw new ArgumentNullException("destinationElement");
			}
			destinationElement.ALAssign(sourceElement);
		}

		public override ElementFactory<NavSessionSettings> Clone(ITreeObject parentOfClone)
		{
			return new Factory(parentOfClone);
		}
	}

	private SessionSettingsInfo sessionSettingsInfo;

	public string ALCompany
	{
		get
		{
			return sessionSettingsInfo.SessionCompany;
		}
		set
		{
			sessionSettingsInfo.SessionCompany = value;
		}
	}

	public string ALProfileId
	{
		get
		{
			return sessionSettingsInfo.ProfileId;
		}
		set
		{
			sessionSettingsInfo.ProfileId = value;
		}
	}

	public Guid ALProfileAppId
	{
		get
		{
			return sessionSettingsInfo.ProfileAppId;
		}
		set
		{
			sessionSettingsInfo.ProfileAppId = value;
		}
	}

	public bool ALProfileSystemScope
	{
		get
		{
			return sessionSettingsInfo.ProfileSystemScope;
		}
		set
		{
			sessionSettingsInfo.ProfileSystemScope = value;
		}
	}

	public int ALLanguageId
	{
		get
		{
			return sessionSettingsInfo.LanguageId;
		}
		set
		{
			sessionSettingsInfo.LanguageId = value;
		}
	}

	public int ALLocaleId
	{
		get
		{
			return sessionSettingsInfo.LocaleId;
		}
		set
		{
			sessionSettingsInfo.LocaleId = value;
		}
	}

	public string ALTimeZone
	{
		get
		{
			return sessionSettingsInfo.TimeZone;
		}
		set
		{
			sessionSettingsInfo.TimeZone = value;
		}
	}

	public override NavNclType NclType => NavNclType.NavSessionSettings;

	public override int GetBytesSize => 0;

	public static NavSessionSettings Default => new NavSessionSettings();

	public override bool IsZeroOrEmpty => sessionSettingsInfo.IsZeroOrEmpty;

	public SessionSettingsInfo Value => sessionSettingsInfo;

	public void ALInit()
	{
		NavSession session = NavCurrentThread.Session;
		session.Diagnostics.SendTraceTag("000017F", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Loading the user personalization settings from the database into the SessionSettings instance.");
		LoadSessionSettings(session);
	}

	public void ALRequestSessionUpdate(bool saveSettings)
	{
		NavSession session = NavCurrentThread.Session;
		session.Diagnostics.SendTraceTag("000017G", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, FormattableString.Invariant($"Sending session update request. Saving settings: {saveSettings}."));
		if (session.ClientConnectionType == ConnectionType.Nas || session.ClientConnectionType == ConnectionType.Background)
		{
			session.Diagnostics.SendTraceTag("000017H", Category.AL, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Session update request cannot be sent on background or Nas sessions.");
			return;
		}
		if (saveSettings)
		{
			SaveSessionSettings(session);
		}
		sessionSettingsInfo.SettingsSaved = saveSettings;
		if (!NavCurrentThread.Session.TestExecution.TestHandleSessionSettings(this))
		{
			IClientCallback clientCallbackOrNull = session.ClientCallbackOrNull;
			if (clientCallbackOrNull != null)
			{
				session.Diagnostics.SendTraceTag("000017I", Category.Runtime, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, FormattableString.Invariant($"Sending session update request to the client: {sessionSettingsInfo.ToString()}."));
				clientCallbackOrNull.SendSessionUpdateRequest(sessionSettingsInfo.Clone());
			}
			else
			{
				session.Diagnostics.SendTraceTag("000017J", Category.AL, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Session update request cannot be sent on without a client callback.");
			}
		}
	}

	private void SaveSessionSettings(NavSession session)
	{
		using NavRecord navRecord = new NavRecord(session, 2000000073);
		bool flag = false;
		NavValue navValue = ALCompiler.ToNavValue(ALDatabase.ALUserSecurityId());
		if (!navRecord.ALGet(DataError.TrapError, navValue))
		{
			flag = true;
			navRecord.ALInit();
			navRecord.SetFieldValueSafe(3, NavType.GUID, navValue);
		}
		navRecord.SetFieldValueSafe(12, NavType.Integer, NavInteger.Create(sessionSettingsInfo.LanguageId));
		navRecord.SetFieldValueSafe(27, NavType.Integer, NavInteger.Create(sessionSettingsInfo.LocaleId));
		navRecord.SetFieldValueSafe(9, NavType.Code, NavText.Create(sessionSettingsInfo.ProfileId));
		navRecord.SetFieldValueSafe(10, NavType.GUID, ALCompiler.ToNavValue(sessionSettingsInfo.ProfileAppId));
		navRecord.SetFieldValueSafe(15, NavType.Text, NavText.Create(sessionSettingsInfo.SessionCompany));
		navRecord.SetFieldValueSafe(30, NavType.Text, NavText.Create(sessionSettingsInfo.TimeZone));
		NCLOptionMetadata navOptionMetadata = navRecord.GetFieldValueSafe(11, NavType.Option).NavOptionMetadata;
		NavOption newValue = NavOption.Create(navOptionMetadata, (!sessionSettingsInfo.ProfileSystemScope) ? 1 : 0);
		navRecord.SetFieldValueSafe(11, NavType.Option, newValue);
		if (flag)
		{
			navRecord.ALInsert(DataError.ThrowError);
		}
		else
		{
			navRecord.ALModify(DataError.ThrowError);
		}
	}

	private void LoadSessionSettings(NavSession session)
	{
		using NavRecord navRecord = new NavRecord(session, 2000000073);
		Guid guid = ALDatabase.ALUserSecurityId();
		if (!navRecord.ALGet(DataError.TrapError, ALCompiler.ToNavValue(guid)))
		{
			session.Diagnostics.SendTraceTag("000017K", Category.Runtime, Verbosity.Warning, "No user personalization found in the database for user {0}.", DiagnosticParameter.Eupi(guid));
			return;
		}
		ALLanguageId = (NavInteger)navRecord.GetFieldValueSafe(12, NavType.Integer);
		ALLocaleId = (NavInteger)navRecord.GetFieldValueSafe(27, NavType.Integer);
		ALProfileId = (NavCode)navRecord.GetFieldValueSafe(9, NavType.Code);
		ALProfileAppId = navRecord.GetFieldValueSafe(10, NavType.GUID).ToGuid();
		ALCompany = (NavText)navRecord.GetFieldValueSafe(15, NavType.Text);
		ALTimeZone = (NavText)navRecord.GetFieldValueSafe(30, NavType.Text);
		ALProfileSystemScope = ((NavOption)navRecord.GetFieldValueSafe(11, NavType.Option)).Value == 0;
	}

	public void ALAssign(NavSessionSettings other)
	{
		Clone(other);
	}

	public void ALAssign(NavValue value)
	{
		NavSessionSettings navSessionSettings = value as NavSessionSettings;
		if (navSessionSettings != null)
		{
			ALAssign(navSessionSettings);
			return;
		}
		throw new NavNCLArgumentException(value, "ALAssign", 1);
	}

	public NavSessionSettings()
	{
		sessionSettingsInfo = new SessionSettingsInfo();
	}

	public NavSessionSettings(NavSessionSettings other)
	{
		Clone(other);
	}

	public NavSessionSettings(string formattedValue)
	{
		sessionSettingsInfo = new SessionSettingsInfo(formattedValue);
	}

	public NavSessionSettings Clone()
	{
		return new NavSessionSettings(this);
	}

	public override int GetHashCode()
	{
		return sessionSettingsInfo.GetHashCode();
	}

	public bool Equals(NavSessionSettings other)
	{
		if (other != null)
		{
			return sessionSettingsInfo.Equals(other.sessionSettingsInfo);
		}
		return false;
	}

	public override bool Equals(NavValue other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (other.NavType == NavType)
		{
			return sessionSettingsInfo.Equals((other as NavSessionSettings)?.sessionSettingsInfo);
		}
		return ThrowUnableToCompareException(other);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is NavValue other)
		{
			return Equals(other);
		}
		return ThrowUnableToCompareException(obj);
	}

	public static bool operator ==(NavSessionSettings left, NavSessionSettings right)
	{
		if ((object)left == right)
		{
			return true;
		}
		if ((object)left == null || (object)right == null)
		{
			return false;
		}
		return left.Equals(right);
	}

	public static bool operator !=(NavSessionSettings left, NavSessionSettings right)
	{
		return !(left == right);
	}

	private void Clone(NavSessionSettings other)
	{
		sessionSettingsInfo = other.sessionSettingsInfo.Clone();
	}

	public void Clear()
	{
		sessionSettingsInfo = new SessionSettingsInfo();
	}
}
