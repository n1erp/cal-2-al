using System.Threading;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis;

public abstract class AdditionalText
{
	public abstract string Path { get; }

	public abstract SourceText GetText(CancellationToken cancellationToken = default(CancellationToken));
}
