namespace Microsoft.Cci;

internal interface IMetadataNamedArgument : IMetadataExpression
{
	string ArgumentName { get; }

	IMetadataExpression ArgumentValue { get; }

	bool IsField { get; }
}
