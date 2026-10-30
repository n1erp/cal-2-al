using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

public abstract class SyntaxReference
{
	public abstract SyntaxTree SyntaxTree { get; }

	public abstract TextSpan Span { get; }

	public abstract SyntaxNode GetSyntax(CancellationToken cancellationToken = default(CancellationToken));

	public virtual Task<SyntaxNode> GetSyntaxAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return Task.FromResult(GetSyntax(cancellationToken));
	}

	internal Location GetLocation()
	{
		return SyntaxTree.GetLocation(Span);
	}
}
