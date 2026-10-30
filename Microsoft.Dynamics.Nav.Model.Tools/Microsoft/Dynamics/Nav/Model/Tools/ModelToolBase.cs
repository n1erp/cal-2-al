namespace Microsoft.Dynamics.Nav.Model.Tools;

internal abstract class ModelToolBase
{
	private readonly ILogger logger;

	protected ILogger Logger => logger;

	internal ModelToolBase(ILogger logger)
	{
		this.logger = logger;
	}
}
