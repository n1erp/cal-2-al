namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface ILiteralExpression : IOperation
{
	string Text { get; }
}
