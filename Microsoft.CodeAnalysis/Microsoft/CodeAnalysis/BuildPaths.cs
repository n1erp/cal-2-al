namespace Microsoft.CodeAnalysis;

internal struct BuildPaths
{
	internal string ClientDirectory { get; }

	internal string WorkingDirectory { get; }

	internal string SdkDirectory { get; }

	internal string TempDirectory { get; }

	internal BuildPaths(string clientDir, string workingDir, string sdkDir, string tempDir)
	{
		ClientDirectory = clientDir;
		WorkingDirectory = workingDir;
		SdkDirectory = sdkDir;
		TempDirectory = tempDir;
	}
}
