namespace Microsoft.Dynamics.Nav.Runtime;

public interface IHttpClientRequestFilterService
{
	bool IsRequestFilteringEnabled { get; }

	bool ApplyRequestFiltering(NavMethodScope callingContext);
}
