namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class NameofBinder : Binder
{
	private readonly SyntaxNode _nameofArgument;

	protected override SyntaxNode EnclosingNameofArgument => _nameofArgument;

	public NameofBinder(SyntaxNode nameofArgument, Binder next)
		: base(next)
	{
		_nameofArgument = nameofArgument;
	}
}
