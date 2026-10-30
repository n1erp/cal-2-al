namespace Microsoft.CodeAnalysis.Operations;

public interface ITypeParameterObjectCreationOperation : IOperation
{
	IObjectOrCollectionInitializerOperation Initializer { get; }
}
