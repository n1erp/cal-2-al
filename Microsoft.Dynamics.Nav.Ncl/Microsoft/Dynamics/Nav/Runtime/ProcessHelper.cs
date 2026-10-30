using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class ProcessHelper
{
	internal static void RunProcess(string args, string fileName)
	{
		RunProcess(args, fileName, throwOnError: true, null);
	}

	internal static void RunProcess(string args, string fileName, bool throwOnError, Func<string, bool> outputEvaluator)
	{
		using Process process = new Process();
		StringBuilder standardOutput = new StringBuilder();
		StringBuilder standardError = new StringBuilder();
		process.StartInfo = new ProcessStartInfo();
		process.StartInfo.FileName = fileName;
		process.StartInfo.CreateNoWindow = true;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.Arguments = args;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;
		process.OutputDataReceived += (object sendingProcess, DataReceivedEventArgs outLine) =>
		{
			if (!string.IsNullOrEmpty(outLine.Data))
			{
				standardOutput.AppendLine(outLine.Data);
			}
		};
		process.ErrorDataReceived += (object sendingProcess, DataReceivedEventArgs outLine) =>
		{
			if (!string.IsNullOrEmpty(outLine.Data))
			{
				standardError.AppendLine(outLine.Data);
			}
		};
		process.Start();
		process.BeginOutputReadLine();
		process.BeginErrorReadLine();
		process.WaitForExit();
		outputEvaluator?.Invoke(standardOutput.ToString());
		int exitCode = process.ExitCode;
		if (throwOnError && exitCode != 0)
		{
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Running the command '{0}' failed. Output message: '{1}'. Error message: '{2}'. Process exit code: '{3}'.", args, standardOutput.ToString(), standardError.ToString(), exitCode));
		}
	}
}
