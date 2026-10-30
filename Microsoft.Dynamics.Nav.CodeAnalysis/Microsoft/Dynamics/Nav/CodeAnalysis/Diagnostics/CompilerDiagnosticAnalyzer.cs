using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal abstract class CompilerDiagnosticAnalyzer : DiagnosticAnalyzer
{
	private class CompilationAnalyzer
	{
		private class CompilerDiagnostic : Diagnostic
		{
			private readonly Diagnostic originalDiagnostic;

			private readonly ImmutableDictionary<string, string> properties;

			public override DiagnosticDescriptor Descriptor => originalDiagnostic.Descriptor;

			public override string Id => originalDiagnostic.Id;

			public override DiagnosticSeverity Severity => originalDiagnostic.Severity;

			public override int WarningLevel => originalDiagnostic.WarningLevel;

			public override Location Location => originalDiagnostic.Location;

			public override IReadOnlyList<Location> AdditionalLocations => originalDiagnostic.AdditionalLocations;

			public override bool IsSuppressed => originalDiagnostic.IsSuppressed;

			public override ImmutableDictionary<string, string> Properties => properties;

			public CompilerDiagnostic(Diagnostic originalDiagnostic, ImmutableDictionary<string, string> properties)
			{
				this.originalDiagnostic = originalDiagnostic;
				this.properties = properties;
			}

			public override string GetMessage(IFormatProvider formatProvider = null)
			{
				return originalDiagnostic.GetMessage(formatProvider);
			}

			public override bool Equals(object obj)
			{
				return originalDiagnostic.Equals(obj);
			}

			public override int GetHashCode()
			{
				return originalDiagnostic.GetHashCode();
			}

			public override bool Equals(Diagnostic obj)
			{
				return originalDiagnostic.Equals(obj);
			}

			internal override Diagnostic WithLocation(Location location)
			{
				return new CompilerDiagnostic(originalDiagnostic.WithLocation(location), properties);
			}

			internal override Diagnostic WithSeverity(DiagnosticSeverity severity)
			{
				return new CompilerDiagnostic(originalDiagnostic.WithSeverity(severity), properties);
			}

			internal override Diagnostic WithIsSuppressed(bool isSuppressed)
			{
				return new CompilerDiagnostic(originalDiagnostic.WithIsSuppressed(isSuppressed), properties);
			}
		}

		private readonly Compilation compilation;

		public CompilationAnalyzer(Compilation compilation)
		{
			this.compilation = compilation;
		}

		public void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
		{
			ReportDiagnostics(compilation.GetSemanticModel(context.Tree).GetSyntaxDiagnostics(null, context.CancellationToken), ((SyntaxTreeAnalysisContext)context).ReportDiagnostic, IsSourceLocation, SyntacticDictionary);
		}

		public static void AnalyzeSemanticModel(SemanticModelAnalysisContext context)
		{
			ReportDiagnostics(context.SemanticModel.GetDeclarationDiagnostics(null, context.CancellationToken), ((SemanticModelAnalysisContext)context).ReportDiagnostic, IsSourceLocation, DeclarationDictionary);
			ReportDiagnostics(context.SemanticModel.GetMethodBodyDiagnostics(null, context.CancellationToken), ((SemanticModelAnalysisContext)context).ReportDiagnostic, IsSourceLocation);
		}

		private static bool IsSourceLocation(Location location)
		{
			if (location != null)
			{
				return location.Kind == LocationKind.SourceFile;
			}
			return false;
		}

		private static void ReportDiagnostics(ImmutableArray<Diagnostic> diagnostics, Action<Diagnostic> reportDiagnostic, Func<Location, bool> locationFilter, ImmutableDictionary<string, string> properties = null)
		{
			foreach (Diagnostic item in diagnostics)
			{
				if (locationFilter(item.Location) && item.Severity != DiagnosticSeverity.Hidden)
				{
					Diagnostic obj = ((properties == null) ? item : new CompilerDiagnostic(item, properties));
					reportDiagnostic(obj);
				}
			}
		}
	}

	private const string Origin = "Origin";

	private const string Syntactic = "Syntactic";

	private const string Declaration = "Declaration";

	private static readonly ImmutableDictionary<string, string> SyntacticDictionary = ImmutableDictionary<string, string>.Empty.Add("Origin", "Syntactic");

	private static readonly ImmutableDictionary<string, string> DeclarationDictionary = ImmutableDictionary<string, string>.Empty.Add("Origin", "Declaration");

	internal abstract CommonMessageProvider MessageProvider { get; }

	public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
	{
		get
		{
			CommonMessageProvider messageProvider = MessageProvider;
			ImmutableArray<int> supportedErrorCodes = GetSupportedErrorCodes();
			ImmutableArray<DiagnosticDescriptor>.Builder builder = ImmutableArray.CreateBuilder<DiagnosticDescriptor>(supportedErrorCodes.Length);
			foreach (int item in supportedErrorCodes)
			{
				DiagnosticDescriptor descriptor = DiagnosticInfo.GetDescriptor(item, messageProvider);
				builder.Add(descriptor);
			}
			return builder.ToImmutable();
		}
	}

	internal abstract ImmutableArray<int> GetSupportedErrorCodes();

	public sealed override void Initialize(AnalysisContext context)
	{
		context.RegisterCompilationStartAction((CompilationStartAnalysisContext c) =>
		{
			CompilationAnalyzer compilationAnalyzer = new CompilationAnalyzer(c.Compilation);
			c.RegisterSyntaxTreeAction(compilationAnalyzer.AnalyzeSyntaxTree);
			c.RegisterSemanticModelAction(CompilationAnalyzer.AnalyzeSemanticModel);
		});
	}
}
