using System;

namespace Microsoft.Dynamics.Nav.Client;

[Flags]
public enum ServerCallOptions
{
	Synchronous = 1,
	Asynchronous = 2,
	InheritThreadCredentials = 4,
	InheritProcessCredentials = 8,
	NoRetry = 0x10,
	SkipServerCallCount = 0x20,
	StandaloneDefault = Asynchronous | InheritProcessCredentials,
	WebClientDefault = Asynchronous | InheritThreadCredentials,
	ServerDefault = Synchronous,
	AggregateDataCall = Synchronous | NoRetry | SkipServerCallCount
}
