namespace Microsoft.CodeAnalysis.Operations;

public interface IMemberInitializerOperation : IOperation
{
	IOperation InitializedMember { get; }

	IObjectOrCollectionInitializerOperation Initializer { get; }
}
