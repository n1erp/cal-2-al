namespace Microsoft.CodeAnalysis.Operations;

public interface ILockOperation : IOperation
{
	IOperation LockedValue { get; }

	IOperation Body { get; }
}
