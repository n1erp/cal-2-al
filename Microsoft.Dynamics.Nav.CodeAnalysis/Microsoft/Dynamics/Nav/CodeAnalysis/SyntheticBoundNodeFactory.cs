using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class SyntheticBoundNodeFactory
{
	public DiagnosticBag Diagnostics { get; }

	public ModuleCompilationState CompilationState { get; }

	public SyntaxNode Syntax { get; set; }

	public TypeSymbol CurrentType { get; set; }

	public MethodSymbol CurrentMethod { get; set; }

	public SyntheticBoundNodeFactory(MethodSymbol topLevelMethod, SyntaxNode node, ModuleCompilationState compilationState, DiagnosticBag diagnostics)
		: this(topLevelMethod, topLevelMethod.ContainingType, node, compilationState, diagnostics)
	{
	}

	public SyntheticBoundNodeFactory(MethodSymbol currentMethod, TypeSymbol currentClassOpt, SyntaxNode node, ModuleCompilationState compilationState, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(node != null);
		DebugAssertHelper.Assert(compilationState != null);
		DebugAssertHelper.Assert(diagnostics != null);
		CompilationState = compilationState;
		CurrentMethod = currentMethod;
		CurrentType = currentClassOpt;
		Syntax = node;
		Diagnostics = diagnostics;
	}

	public BoundSystemOptionType SystemOption(SystemOptionTypeSymbol option)
	{
		return new BoundSystemOptionType(Syntax, option, option);
	}

	public BoundOptionAccess OptionAccess(BoundExpression receiver, OptionSymbol option)
	{
		return new BoundOptionAccess(Syntax, receiver, option, LookupResultKind.Viable, option.GetNativeOptionType());
	}

	public BoundLiteral Literal(string value)
	{
		return new BoundLiteral(Syntax, ConstantValue.Create(value), NavCorLib.StringType);
	}

	public BoundLiteral Literal(int value)
	{
		return new BoundLiteral(Syntax, ConstantValue.Create(value), NavCorLib.IntegerType);
	}

	public BoundLiteral Literal(bool value)
	{
		return new BoundLiteral(Syntax, ConstantValue.Create(value), NavCorLib.BooleanType);
	}

	public BoundAttribute Attribute(params BoundExpression[] arguments)
	{
		return new BoundAttribute(Syntax, arguments.AsImmutableOrEmpty());
	}
}
