namespace Microsoft.Dynamics.Nav.Diagnostic;

public enum PrivacyClassification
{
	AccessControlData = 0,
	CustomerContent = 100,
	EndUserIdentifiableInformation = 200,
	ObjectMetadata = 250,
	CustomerSupportData = 300,
	AccountData = 400,
	PublicPersonalData = 500,
	EndUserPseudonymousIdentifiers = 600,
	OrganizationIdentifiableInformation = 700,
	SystemMetadata = 800,
	PublicNonPersonalData = 900,
	Invalid = 10000
}
