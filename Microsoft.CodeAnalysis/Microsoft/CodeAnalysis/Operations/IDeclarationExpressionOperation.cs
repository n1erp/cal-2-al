namespace Microsoft.CodeAnalysis.Operations;

public interface IDeclarationExpressionOperation : IOperation
{
	IOperation Expression { get; }
}
