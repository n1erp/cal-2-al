using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class SystemOptionKinds
{
	[CompilerGenerated]
	internal enum ActionKind
	{
		None,
		OK,
		Cancel,
		LookupOK,
		LookupCancel,
		Yes,
		No,
		RunObject,
		RunSystem
	}

	[CompilerGenerated]
	internal enum ClientTypeKind
	{
		Background,
		Desktop,
		Management,
		NAS,
		OData,
		Phone,
		SOAP,
		Tablet,
		Web,
		Windows,
		Current,
		Default,
		ODataV4,
		Api
	}

	[CompilerGenerated]
	internal enum DataClassificationKind
	{
		CustomerContent,
		ToBeClassified,
		EndUserIdentifiableInformation,
		AccountData,
		EndUserPseudonymousIdentifiers,
		OrganizationIdentifiableInformation,
		SystemMetadata
	}

	[CompilerGenerated]
	internal enum DataScopeKind
	{
		Module,
		Company,
		User,
		CompanyAndUser
	}

	[CompilerGenerated]
	internal enum DefaultLayoutKind
	{
		None,
		RDLC,
		Word
	}

	[CompilerGenerated]
	internal enum ErrorTypeKind
	{
		Client,
		Internal
	}

	[CompilerGenerated]
	internal enum ExecutionContextKind
	{
		Normal,
		Install,
		Uninstall,
		Upgrade
	}

	[CompilerGenerated]
	internal enum ExecutionModeKind
	{
		Standard,
		Debug
	}

	[CompilerGenerated]
	internal enum FieldClassKind
	{
		Normal,
		FlowField,
		FlowFilter
	}

	[CompilerGenerated]
	internal enum FieldTypeKind
	{
		Boolean,
		Integer,
		BigInteger,
		Decimal,
		Option,
		Text,
		Code,
		DateTime,
		Time,
		Date,
		DateFormula,
		Duration,
		Guid,
		RecordId,
		TableFilter,
		Blob,
		Media,
		MediaSet
	}

	[CompilerGenerated]
	internal enum NotificationScopeKind
	{
		GlobalScope,
		LocalScope
	}

	[CompilerGenerated]
	internal enum ObjectTypeKind
	{
		Codeunit,
		MenuSuite,
		Page,
		Query,
		Report,
		Table,
		XmlPort
	}

	[CompilerGenerated]
	internal enum ReportFormatKind
	{
		Excel,
		Html,
		Pdf,
		Word,
		Xml
	}

	[CompilerGenerated]
	internal enum SecurityFilterKind
	{
		Validated,
		Filtered,
		Ignored,
		Disallowed
	}

	[CompilerGenerated]
	internal enum TableConnectionTypeKind
	{
		CRM,
		ExternalSQL,
		Exchange,
		MicrosoftGraph
	}

	[CompilerGenerated]
	internal enum TestPermissionsKind
	{
		InheritFromTestCodeunit,
		Restrictive,
		NonRestrictive,
		Disabled
	}

	[CompilerGenerated]
	internal enum TextEncodingKind
	{
		MSDos,
		UTF8,
		UTF16,
		Windows
	}

	[CompilerGenerated]
	internal enum TransactionModelKind
	{
		AutoCommit,
		AutoRollback,
		None
	}

	[CompilerGenerated]
	internal enum TransactionTypeKind
	{
		UpdateNoLocks,
		Update,
		Snapshot,
		Browse,
		Report
	}

	[CompilerGenerated]
	internal enum VerbosityKind
	{
		Critical,
		Error,
		Warning,
		Normal,
		Verbose
	}

	[CompilerGenerated]
	internal enum WebServiceActionResultCodeKind
	{
		None,
		Get,
		Created,
		Updated,
		Deleted
	}
}
