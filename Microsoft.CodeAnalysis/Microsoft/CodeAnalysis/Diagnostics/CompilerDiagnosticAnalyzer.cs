using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal abstract class CompilerDiagnosticAnalyzer : DiagnosticAnalyzer
{
	private class CompilationAnalyzer
	{
		private class CompilerDiagnostic : Diagnostic
		{
			private readonly Diagnostic _original;

			private readonly ImmutableDictionary<string, string> _properties;

			public override DiagnosticDescriptor Descriptor => _original.Descriptor;

			internal override int Code => _original.Code;

			internal override IReadOnlyList<object> Arguments => _original.Arguments;

			public override string Id => _original.Id;

			public override DiagnosticSeverity Severity => _original.Severity;

			public override int WarningLevel => _original.WarningLevel;

			public override Location Location => _original.Location;

			public override IReadOnlyList<Location> AdditionalLocations => _original.AdditionalLocations;

			public override bool IsSuppressed => _original.IsSuppressed;

			public override ImmutableDictionary<string, string> Properties => _properties;

			public CompilerDiagnostic(Diagnostic original, ImmutableDictionary<string, string> properties)
			{
				_original = original;
				_properties = properties;
			}

			public override string GetMessage(IFormatProvider formatProvider = null)
			{
				return _original.GetMessage(formatProvider);
			}

			public override bool Equals(object obj)
			{
				return _original.Equals(obj);
			}

			public override int GetHashCode()
			{
				return _original.GetHashCode();
			}

			public override bool Equals(Diagnostic obj)
			{
				return _original.Equals(obj);
			}

			internal override Diagnostic WithLocation(Location location)
			{
				return new CompilerDiagnostic(_original.WithLocation(location), _properties);
			}

			internal override Diagnostic WithSeverity(DiagnosticSeverity severity)
			{
				return new CompilerDiagnostic(_original.WithSeverity(severity), _properties);
			}

			internal override Diagnostic WithIsSuppressed(bool isSuppressed)
			{
				return new CompilerDiagnostic(_original.WithIsSuppressed(isSuppressed), _properties);
			}
		}

		private readonly Compilation _compilation;

		public CompilationAnalyzer(Compilation compilation)
		{
			_compilation = compilation;
		}

		public void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
		{
			ReportDiagnostics(_compilation.GetSemanticModel(context.Tree).GetSyntaxDiagnostics(null, context.CancellationToken), ((SyntaxTreeAnalysisContext)context).ReportDiagnostic, IsSourceLocation, s_syntactic);
		}

		public static void AnalyzeSemanticModel(SemanticModelAnalysisContext context)
		{
			ReportDiagnostics(context.SemanticModel.GetDeclarationDiagnostics(null, context.CancellationToken), ((SemanticModelAnalysisContext)context).ReportDiagnostic, IsSourceLocation, s_declaration);
			ReportDiagnostics(context.SemanticModel.GetMethodBodyDiagnostics(null, context.CancellationToken), ((SemanticModelAnalysisContext)context).ReportDiagnostic, IsSourceLocation);
		}

		public static void AnalyzeCompilation(CompilationAnalysisContext context)
		{
			ReportDiagnostics(context.Compilation.GetDeclarationDiagnostics(context.CancellationToken), ((CompilationAnalysisContext)context).ReportDiagnostic, (Location location) => !IsSourceLocation(location), s_declaration);
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

	private static readonly ImmutableDictionary<string, string> s_syntactic = ImmutableDictionary<string, string>.Empty.Add("Origin", "Syntactic");

	private static readonly ImmutableDictionary<string, string> s_declaration = ImmutableDictionary<string, string>.Empty.Add("Origin", "Declaration");

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
			builder.Add(AnalyzerExecutor.GetAnalyzerExceptionDiagnosticDescriptor());
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
