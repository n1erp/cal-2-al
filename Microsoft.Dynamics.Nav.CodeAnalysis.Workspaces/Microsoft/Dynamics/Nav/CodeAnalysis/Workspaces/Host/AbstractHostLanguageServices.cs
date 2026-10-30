using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

public abstract class AbstractHostLanguageServices
{
	[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
	public string Language => "AL";

	internal ISyntaxTreeFactoryService SyntaxTreeFactory => GetService<ISyntaxTreeFactoryService>();

	public abstract TLanguageService GetService<TLanguageService>();
}
