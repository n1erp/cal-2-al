namespace Microsoft.CodeAnalysis.Operations;

internal interface INoPiaObjectCreationOperation : IOperation
{
	IObjectOrCollectionInitializerOperation Initializer { get; }
}
