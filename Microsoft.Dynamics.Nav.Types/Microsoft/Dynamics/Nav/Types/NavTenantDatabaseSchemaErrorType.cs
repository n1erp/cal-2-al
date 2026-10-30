namespace Microsoft.Dynamics.Nav.Types;

public enum NavTenantDatabaseSchemaErrorType
{
	MissingTable,
	AdditionalTable,
	MissingKey,
	AdditionalKey,
	MissingField,
	AdditionalField,
	MissingConstraint,
	AdditionalConstraint,
	IncorrectFieldType,
	IncorrectFieldLength
}
