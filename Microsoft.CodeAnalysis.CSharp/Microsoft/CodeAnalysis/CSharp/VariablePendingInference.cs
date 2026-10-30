using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class VariablePendingInference : BoundExpression
{
	protected abstract ErrorCode InferenceFailedError { get; }

	public Symbol VariableSymbol { get; }

	public BoundExpression ReceiverOpt { get; }

	internal BoundExpression SetInferredType(TypeSymbol type, DiagnosticBag diagnosticsOpt)
	{
		return SetInferredType(type, null, diagnosticsOpt);
	}

	internal BoundExpression SetInferredType(TypeSymbol type, Binder binderOpt, DiagnosticBag diagnosticsOpt)
	{
		bool flag = (object)type == null;
		if (flag)
		{
			type = binderOpt.CreateErrorType("var");
		}
		switch (VariableSymbol.Kind)
		{
		case SymbolKind.Local:
		{
			SourceLocalSymbol sourceLocalSymbol = (SourceLocalSymbol)VariableSymbol;
			if (diagnosticsOpt != null)
			{
				if (flag)
				{
					ReportInferenceFailure(diagnosticsOpt);
				}
				else
				{
					SyntaxNode syntax = ((Syntax.Kind() == SyntaxKind.DeclarationExpression) ? ((DeclarationExpressionSyntax)Syntax).Type : Syntax);
					Binder.CheckRestrictedTypeInAsync(sourceLocalSymbol.ContainingSymbol, type, diagnosticsOpt, syntax);
				}
			}
			sourceLocalSymbol.SetType(type);
			return new BoundLocal(Syntax, sourceLocalSymbol, isDeclaration: true, null, type, HasErrors | flag);
		}
		case SymbolKind.Field:
		{
			GlobalExpressionVariable globalExpressionVariable = (GlobalExpressionVariable)VariableSymbol;
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			if (flag)
			{
				ReportInferenceFailure(instance);
			}
			type = globalExpressionVariable.SetType(type, instance);
			instance.Free();
			return new BoundFieldAccess(Syntax, ReceiverOpt, globalExpressionVariable, null, LookupResultKind.Viable, isDeclaration: true, type, HasErrors | flag);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(VariableSymbol.Kind);
		}
	}

	internal BoundExpression FailInference(Binder binder, DiagnosticBag diagnosticsOpt)
	{
		return SetInferredType(null, binder, diagnosticsOpt);
	}

	private void ReportInferenceFailure(DiagnosticBag diagnostics)
	{
		SingleVariableDesignationSyntax singleVariableDesignationSyntax = Syntax.Kind() switch
		{
			SyntaxKind.DeclarationExpression => (SingleVariableDesignationSyntax)((DeclarationExpressionSyntax)Syntax).Designation, 
			SyntaxKind.SingleVariableDesignation => (SingleVariableDesignationSyntax)Syntax, 
			_ => throw ExceptionUtilities.Unreachable, 
		};
		Binder.Error(diagnostics, InferenceFailedError, singleVariableDesignationSyntax.Identifier, singleVariableDesignationSyntax.Identifier.ValueText);
	}

	protected VariablePendingInference(BoundKind kind, SyntaxNode syntax, Symbol variableSymbol, BoundExpression receiverOpt, bool hasErrors = false)
		: base(kind, syntax, null, hasErrors)
	{
		VariableSymbol = variableSymbol;
		ReceiverOpt = receiverOpt;
	}
}
