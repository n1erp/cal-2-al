using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class NavTenantDatabaseSettings : IExtensibleDataObject
{
	private static readonly string[] EmptyVersionNumber = new string[0];

	private string[] applicationVersion;

	[DataMember]
	[TelemetryOii]
	public string Id { get; private set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public IEnumerable<string> ApplicationVersion
	{
		get
		{
			return applicationVersion;
		}
		private set
		{
			applicationVersion = value?.ToArray() ?? EmptyVersionNumber;
		}
	}

	[DataMember]
	[TelemetrySystemMetadata]
	public string ApplicationFamily { get; private set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public NavTenantDatabaseState State { get; set; }

	[DataMember]
	public NavTenantDatabaseSettingsDetailedState DetailedState { get; set; }

	[DataMember]
	public DatabaseInfo DatabaseInfo { get; set; }

	public ExtensionDataObject ExtensionData { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool? IsSharedSchema { get; set; }

	public NavTenantDatabaseSettings()
	{
	}

	public NavTenantDatabaseSettings(string id, DatabaseInfo databaseInfo, IEnumerable<string> applicationVersion = null, string applicationFamily = null, NavTenantDatabaseSettingsDetailedState initialState = null, bool? isSharedSchema = null)
	{
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentNullException("id");
		}
		if (databaseInfo == null)
		{
			throw new ArgumentNullException("databaseInfo");
		}
		if (string.IsNullOrEmpty(databaseInfo.Name))
		{
			throw new ArgumentException("A database name is required");
		}
		DatabaseInfo = databaseInfo;
		ApplicationVersion = applicationVersion;
		ApplicationFamily = applicationFamily;
		DetailedState = initialState ?? new NavTenantDatabaseSettingsDetailedState();
		State = DetailedState.State;
		Id = id;
		IsSharedSchema = isSharedSchema;
	}
}
