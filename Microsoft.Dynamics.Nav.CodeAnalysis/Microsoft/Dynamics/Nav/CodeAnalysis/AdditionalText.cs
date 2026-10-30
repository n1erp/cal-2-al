using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public abstract class AdditionalText
{
	public abstract string Path { get; }

	public abstract SourceText GetText(CancellationToken cancellationToken = default(CancellationToken));
}
