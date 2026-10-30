using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net.Security;
using System.Reflection;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types;

public class ConfigSettings : IDisposable
{
	public enum EmitSetting
	{
		Allow,
		Restricted,
		Denied
	}

	private static ConfigSettings instance;

	private bool canSetSettings = true;

	private Dictionary<string, string> dirtyValues;

	private string companyName;

	private System.Configuration.Configuration configuration;

	private bool debugMode;

	private readonly object configurationSyncRoot = new object();

	public const string NumberFormatSpecifier = "N";

	public const string MaxValueSetting = "MaxValue";

	public const int NoLimitOnLength = -1;

	public const int MaxHostNameLength = 255;

	public const int MaxServerInstanceNameLength = 250;

	public const int MaxDatabaseNameLength = 123;

	public const int MaxSqlInstanceNameLength = 16;

	public const int MaxALFunctionNameLength = 128;

	private const int MinPortNumber = 1;

	private const int MaxPortNumber = 65535;

	public const string ClientServicesPortSettingName = "ClientServicesPort";

	public const int ClientServicesPortDefault = 7046;

	private int clientServicesPort;

	public const string ManagementServicesPortSettingName = "ManagementServicesPort";

	public const int ManagementServicesPortDefault = 7045;

	private int managementServicesPort;

	public const string TraceLevelSettingName = "TraceLevel";

	internal const Verbosity TraceLevelSettingDefault = Verbosity.Normal;

	private Verbosity traceLevel;

	public const string IfxSessionNameSettingName = "IfxSessionName";

	private string ifxSessionName;

	public const string MdmAccountNameSettingName = "MdmAccountName";

	private string mdmAccountName;

	public const string ClientServicesProtectionLevelSettingName = "ClientServicesProtectionLevel";

	public const ProtectionLevel ClientServicesProtectionLevelDefault = ProtectionLevel.EncryptAndSign;

	public const string ServerInstanceSettingName = "ServerInstance";

	public const string ServerInstanceSettingDefault = "DynamicsNAV90";

	private string serverInstance;

	public const string DocumentServicesProviderDefault = "SHAREPOINTONLINE";

	public const string ClientCredentialTypeName = "ClientServicesCredentialType";

	public const NavClientCredentialType ClientCredentialTypeDefault = NavClientCredentialType.Windows;

	public const string ServicesCertificateValidationEnabledName = "ServicesCertificateValidationEnabled";

	public const bool ServicesCertificateValidationEnabledDefault = true;

	public const string DnsIdentityName = "DnsIdentity";

	public const int CompressionThresholdDefault = 64;

	private int clientServicesCompressionThreshold;

	internal const int MinCompressionThreshold = 16;

	internal const int MaxCompressionThreshold = 2000;

	public const string ClientServicesCompressionThresholdName = "ClientServicesCompressionThreshold";

	public const string MaxUploadSizeSettingName = "ClientServicesMaxUploadSize";

	internal const int MaxUploadFileSizeDefaultMB = 5;

	public const int MaxUploadFileSizeDefault = 5242880;

	public const int MinUploadFileSize = 1;

	public const string ClientServicesChunkSizeName = "ClientServicesChunkSize";

	private const int MaxChunkSizeKB = 80;

	public const int DefaultChunkSizeKB = 28;

	private const int MinChunkSizeKB = 4;

	public const int ChunkingDisabled = 0;

	private int currentChunkSize;

	public Action<Exception> ProvideConfigurationException { get; set; }

	public static ConfigSettings Instance
	{
		get
		{
			return instance;
		}
		set
		{
			instance = value;
		}
	}

	public string CompanyName
	{
		get
		{
			return companyName;
		}
		set
		{
			companyName = value;
		}
	}

	public bool DebugMode
	{
		get
		{
			return debugMode;
		}
		set
		{
			debugMode = value;
		}
	}

	public int ClientServicesPort
	{
		get
		{
			return clientServicesPort;
		}
		set
		{
			clientServicesPort = value;
			SetSetting(clientServicesPort, "ClientServicesPort");
		}
	}

	public int ManagementServicesPort
	{
		get
		{
			return managementServicesPort;
		}
		set
		{
			managementServicesPort = value;
			SetSetting(managementServicesPort, "ManagementServicesPort");
		}
	}

	public Verbosity TraceLevel
	{
		get
		{
			return traceLevel;
		}
		set
		{
			traceLevel = value;
			SetSetting(traceLevel, "TraceLevel");
		}
	}

	public string IfxSessionName
	{
		get
		{
			return ifxSessionName;
		}
		set
		{
			ifxSessionName = value;
			SetSetting(ifxSessionName, "IfxSessionName");
		}
	}

	public string MdmAccountName
	{
		get
		{
			return mdmAccountName;
		}
		set
		{
			mdmAccountName = value;
			SetSetting(mdmAccountName, "MdmAccountName");
		}
	}

	public int ClientServicesCompressionThreshold => clientServicesCompressionThreshold;

	public bool EnableCompactSerialization { get; set; }

	public string ShellType { get; private set; }

	public string OfficeSuiteShellCertificateThumbprint { get; protected set; }

	public int OfficeSuiteShellServiceClientTimeout { get; protected set; }

	public string OfficeSuiteShellEndpoint { get; protected set; }

	public string WalkMeUri { get; set; }

	public int ClientServicesChunkSize => currentChunkSize;

	public ProtectionLevel ClientServicesProtectionLevel { get; set; }

	public NavClientCredentialType ClientServicesCredentialType { get; set; }

	public bool ServicesCertificateValidationEnabled { get; set; }

	public bool CertificateValidationEnabled { get; set; }

	public string DnsIdentity { get; set; }

	public string ServerInstance
	{
		get
		{
			return serverInstance;
		}
		set
		{
			serverInstance = value;
			SetSetting(serverInstance, "ServerInstance");
		}
	}

	public System.Configuration.Configuration Configuration
	{
		get
		{
			return configuration;
		}
		protected set
		{
			configuration = value;
		}
	}

	protected virtual AppSettingsSection AppSettings => Configuration.AppSettings;

	public bool CanSetSettings
	{
		get
		{
			return canSetSettings;
		}
		protected set
		{
			canSetSettings = value;
		}
	}

	public bool Dirty
	{
		get
		{
			if (dirtyValues != null)
			{
				return dirtyValues.Count > 0;
			}
			return false;
		}
	}

	public static bool Exists()
	{
		return instance != null;
	}

	protected void MarkAsDirty(string settingName, string value, string oldValue)
	{
		if (!(value == oldValue))
		{
			MarkAsDirty(settingName, value);
		}
	}

	protected void MarkAsDirty(string settingName, string value)
	{
		if (CanSetSettings)
		{
			if (dirtyValues == null)
			{
				dirtyValues = new Dictionary<string, string>();
			}
			dirtyValues[settingName] = value;
		}
	}

	public void Save()
	{
		if (!CanSetSettings || dirtyValues == null)
		{
			return;
		}
		try
		{
			foreach (string key in dirtyValues.Keys)
			{
				string setting = dirtyValues[key];
				SetSetting(setting, key);
			}
			dirtyValues.Clear();
		}
		catch (NavConfigurationException obj)
		{
			if (ProvideConfigurationException != null)
			{
				ProvideConfigurationException(obj);
			}
		}
	}

	protected void LoadShellType(string shellType)
	{
		ShellType = (ShellTypes.KnownShellTypes.Contains(shellType) ? shellType : "DEFAULT");
	}

	public ConfigSettings(string configFileName)
		: this(GetConfiguration(configFileName))
	{
	}

	protected ConfigSettings(System.Configuration.Configuration configuration)
	{
		this.configuration = configuration;
	}

	protected void LoadProtectionLevel()
	{
		ClientServicesProtectionLevel = GetSetting("ClientServicesProtectionLevel", ProtectionLevel.EncryptAndSign);
	}

	protected void LoadClientCredentialType()
	{
		ClientServicesCredentialType = GetSetting("ClientServicesCredentialType", NavClientCredentialType.Windows);
	}

	protected void LoadCertificateValidationEnabled()
	{
		ServicesCertificateValidationEnabled = GetSetting("ServicesCertificateValidationEnabled", defaultValue: true);
	}

	protected void LoadDnsIdentity()
	{
		DnsIdentity = GetStringSetting("DnsIdentity");
	}

	protected virtual void LoadSettings()
	{
		LoadServerInstance();
	}

	protected void LoadServerInstance()
	{
		serverInstance = GetStringSetting("ServerInstance", "DynamicsNAV90");
	}

	protected virtual void LoadClientServicesPortSetting()
	{
		try
		{
			int setting = GetSetting("ClientServicesPort", 7046);
			ValidatePortNumber(setting, "ClientServicesPort");
			clientServicesPort = setting;
		}
		catch (NavConfigurationException ex) when (ex.InnerException is OverflowException)
		{
			throw new NavConfigurationException(Lang.PortNumberOutOfRange, "ClientServicesPort", ex);
		}
	}

	protected virtual void LoadManagementServicesPortSetting()
	{
		try
		{
			int setting = GetSetting("ManagementServicesPort", 7045);
			ValidatePortNumber(setting, "ManagementServicesPort");
			managementServicesPort = setting;
		}
		catch (NavConfigurationException ex) when (ex.InnerException is OverflowException)
		{
			throw new NavConfigurationException(Lang.PortNumberOutOfRange, "ManagementServicesPort", ex);
		}
	}

	protected virtual void LoadInstrumentationSettings()
	{
		traceLevel = GetSetting("TraceLevel", Verbosity.Normal);
		ifxSessionName = GetStringSetting("IfxSessionName");
		mdmAccountName = GetStringSetting("MdmAccountName");
	}

	protected int LoadMaxUploadFileSize()
	{
		int setting = GetSetting("ClientServicesMaxUploadSize", 5);
		SettingsHelper.ValidateInteger(setting, 1, 2047, "ClientServicesMaxUploadSize");
		return setting * 1024 * 1024;
	}

	protected void LoadChunkSize(int chunkSize)
	{
		if (chunkSize == 0)
		{
			currentChunkSize = 0;
			return;
		}
		ValidateChunkSize(chunkSize, 4, 80, 0, "ClientServicesChunkSize");
		currentChunkSize = chunkSize * 1024;
	}

	protected void LoadCompressionThresholdSetting(int threshold)
	{
		try
		{
			ValidateCompressionThreshold(threshold, "ClientServicesCompressionThreshold");
			clientServicesCompressionThreshold = threshold * 1024;
		}
		catch (NavConfigurationException ex) when (ex.InnerException is OverflowException)
		{
			throw new NavConfigurationException(Lang.CompressionThresholdOutOfRange, "ClientServicesCompressionThreshold", ex);
		}
	}

	public void ReloadSettings()
	{
		lock (configurationSyncRoot)
		{
			ResetConfiguration();
			LoadSettings();
		}
	}

	public static CultureInfo ParseCultureInfo(string value)
	{
		CultureInfo result = null;
		value = value.Trim();
		try
		{
			result = (int.TryParse(value, out var result2) ? CultureInfo.GetCultureInfo(result2) : CultureInfo.GetCultureInfo(value));
		}
		catch (FormatException)
		{
		}
		catch (OverflowException)
		{
		}
		return result;
	}

	protected static int ValidatePortNumber(int port, string settingName)
	{
		if (!IsValidPortNumber(port))
		{
			throw new NavConfigurationException(Lang.PortNumberOutOfRange, settingName);
		}
		return port;
	}

	private static bool IsValidPortNumber(int port)
	{
		if (port < 1 || port > 65535)
		{
			return false;
		}
		return true;
	}

	protected static void ValidateTimeSpan(TimeSpan timeSpan, string settingName)
	{
		if (timeSpan < TimeSpan.Zero)
		{
			throw new NavConfigurationException(Lang.NegativeTimeSpan, settingName, TimeSpan.Zero);
		}
	}

	public static void ValidateChunkSize(int value, int minValue, int maxValue, int chunkingDisabled, string settingName)
	{
		if (value != chunkingDisabled && !SettingsHelper.IsValidInteger(value, minValue, maxValue))
		{
			NumberFormatInfo numberFormatInfo = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
			numberFormatInfo.NumberDecimalDigits = 0;
			throw new NavConfigurationException(Lang.ChunkSizeOutOfRange, settingName, minValue.ToString("N", numberFormatInfo), maxValue.ToString("N", numberFormatInfo), chunkingDisabled.ToString("N", numberFormatInfo));
		}
	}

	protected static void ValidateCompressionThreshold(int compressionThreshold, string settingName)
	{
		if (compressionThreshold < 16 || compressionThreshold > 2000)
		{
			throw new NavConfigurationException(Lang.CompressionThresholdOutOfRange, settingName);
		}
	}

	public T GetSetting<T>(string settingName) where T : struct
	{
		return GetSetting<T>(settingName, default);
	}

	public T GetSetting<T>(string settingName, T defaultValue) where T : struct
	{
		bool specified = false;
		string rawSetting = GetRawSetting(settingName, ref specified);
		if (!string.IsNullOrEmpty(rawSetting))
		{
			Type typeFromHandle = typeof(T);
			if (typeFromHandle.IsEnum)
			{
				bool flag = false;
				string[] names = Enum.GetNames(typeFromHandle);
				foreach (string strB in names)
				{
					if (string.Compare(rawSetting, strB, StringComparison.Ordinal) == 0)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.UnableToParseConfigSetting, settingName, typeof(T).Name));
				}
				return (T)Enum.Parse(typeFromHandle, rawSetting);
			}
			if (TryParseAsMaxValue<T>(rawSetting, settingName, out var maxValue))
			{
				return maxValue;
			}
			if (typeof(T) == typeof(TimeSpan))
			{
				if (TimeSpan.TryParse(rawSetting, out var result))
				{
					return (T)(object)result;
				}
				throw new NavConfigurationException(Lang.UnableToParseConfigSetting, settingName, typeof(T).Name);
			}
			try
			{
				return (T)Convert.ChangeType(rawSetting, typeof(T), CultureInfo.CurrentCulture);
			}
			catch (OverflowException exception)
			{
				if (typeof(T) == typeof(int))
				{
					NumberFormatInfo numberFormatInfo = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
					numberFormatInfo.NumberDecimalDigits = 0;
					throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigurationSettingOutOfRange, settingName, int.MinValue.ToString("N", numberFormatInfo), int.MaxValue.ToString("N", numberFormatInfo)), exception);
				}
				throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.UnableToParseConfigSetting, settingName, typeof(T).Name), exception);
			}
			catch (FormatException exception2)
			{
				throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.UnableToParseConfigSetting, settingName, typeof(T).Name), exception2);
			}
		}
		if (specified && typeof(T) == typeof(TimeSpan))
		{
			throw new NavConfigurationException(Lang.UnableToParseConfigSetting, settingName, typeof(T).Name);
		}
		return defaultValue;
	}

	public string GetStringSetting(string settingName)
	{
		return GetStringSetting(settingName, null);
	}

	public Uri GetUriSetting(string settingName)
	{
		string stringSetting = GetStringSetting(settingName);
		if (!string.IsNullOrEmpty(stringSetting))
		{
			try
			{
				return new Uri(Uri.UnescapeDataString(stringSetting));
			}
			catch (UriFormatException ex)
			{
				throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigurationFileError, ex.Message), ex);
			}
		}
		return null;
	}

	public string GetStringSetting(string settingName, string defaultValue)
	{
		bool specified = false;
		string text = GetRawSetting(settingName, ref specified);
		if (string.IsNullOrEmpty(text))
		{
			text = defaultValue;
		}
		return text;
	}

	protected bool TryParseAsMaxValue<T>(string setting, string settingName, out T maxValue)
	{
		maxValue = default;
		if (string.Compare(setting, "MaxValue", ignoreCase: true, CultureInfo.CurrentCulture) == 0)
		{
			FieldInfo field = typeof(T).GetField("MaxValue", BindingFlags.Static | BindingFlags.Public);
			if (field != null)
			{
				maxValue = (T)field.GetValue(null);
				return true;
			}
			throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigurationParseError, settingName, configuration.FilePath));
		}
		return false;
	}

	protected virtual string GetRawSetting(string settingName, ref bool specified)
	{
		try
		{
			lock (configurationSyncRoot)
			{
				KeyValueConfigurationElement keyValueConfigurationElement = AppSettings.Settings[settingName];
				if (keyValueConfigurationElement != null)
				{
					specified = true;
					return keyValueConfigurationElement.Value;
				}
				specified = false;
				return null;
			}
		}
		catch (ConfigurationErrorsException ex)
		{
			throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigurationFileError, ex.Message), ex);
		}
	}

	protected void SetSetting<T>(T setting, string settingName) where T : struct
	{
		SetSetting(setting.ToString(), settingName);
	}

	protected virtual void SetSetting(string setting, string settingName)
	{
		if (!CanSetSettings)
		{
			throw new NotSupportedException("Updating the configuration file is not supported.");
		}
		lock (configurationSyncRoot)
		{
			AppSettingsSection appSettings = AppSettings;
			KeyValueConfigurationElement keyValueConfigurationElement = appSettings.Settings[settingName];
			if (keyValueConfigurationElement != null)
			{
				keyValueConfigurationElement.Value = setting;
			}
			else
			{
				appSettings.Settings.Add(settingName, setting);
			}
			try
			{
				configuration.Save(ConfigurationSaveMode.Modified);
			}
			catch (ConfigurationErrorsException ex)
			{
				throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigurationFileError, ex.Message), ex);
			}
			catch (UnauthorizedAccessException exception)
			{
				throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigSettingsUnauthorizedAccess, Configuration.FilePath), exception);
			}
		}
	}

	protected virtual void ResetConfiguration()
	{
	}

	public static System.Configuration.Configuration GetConfiguration(string fileName)
	{
		ExeConfigurationFileMap exeConfigurationFileMap = new ExeConfigurationFileMap();
		exeConfigurationFileMap.ExeConfigFilename = fileName;
		if (!File.Exists(exeConfigurationFileMap.ExeConfigFilename))
		{
			throw new NavConfigurationException(Lang.ConfigurationFileRequired, exeConfigurationFileMap.ExeConfigFilename);
		}
		try
		{
			return ConfigurationManager.OpenMappedExeConfiguration(exeConfigurationFileMap, ConfigurationUserLevel.None);
		}
		catch (ConfigurationErrorsException ex)
		{
			throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigurationFileError, ex.Message), ex);
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (disposing && instance != null && instance == this)
		{
			instance = null;
		}
	}
}
