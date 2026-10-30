namespace Microsoft.CodeAnalysis.Operations;

public interface ITranslatedQueryOperation : IOperation
{
	IOperation Operation { get; }
}
