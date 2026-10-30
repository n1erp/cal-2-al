using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.CodeAnalysis.Diagnostics.CSharp;

[DiagnosticAnalyzer("C#", new string[] { })]
internal sealed class CSharpCompilerDiagnosticAnalyzer : CompilerDiagnosticAnalyzer
{
	internal override CommonMessageProvider MessageProvider => Microsoft.CodeAnalysis.CSharp.MessageProvider.Instance;

	internal override ImmutableArray<int> GetSupportedErrorCodes()
	{
		Array values = Enum.GetValues(typeof(ErrorCode));
		ImmutableArray<int>.Builder builder = ImmutableArray.CreateBuilder<int>(values.Length);
		foreach (int item in values)
		{
			switch (item)
			{
			case -2:
			case -1:
			case 17:
			case 28:
			case 67:
			case 148:
			case 169:
			case 204:
			case 402:
			case 414:
			case 518:
			case 570:
			case 649:
			case 656:
			case 1555:
			case 1556:
			case 1558:
			case 1607:
			case 1969:
			case 4007:
			case 4013:
			case 5001:
			case 7022:
			case 7038:
			case 8004:
			case 8005:
			case 8006:
			case 8008:
			case 8078:
			case 8178:
				continue;
			}
			builder.Add(item);
		}
		return builder.ToImmutable();
	}
}
