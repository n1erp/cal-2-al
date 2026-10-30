using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class PatternSwitchBinder : SwitchBinder
{
	private bool UseV7SwitchBinder
	{
		get
		{
			CSharpParseOptions obj = SwitchSyntax?.SyntaxTree?.Options as CSharpParseOptions;
			if (((object)obj == null || !obj.Features.ContainsKey("testV7SwitchBinder")) && !SwitchBinder.HasPatternSwitchSyntax(SwitchSyntax))
			{
				return !SwitchGoverningType.IsValidV6SwitchGoverningType();
			}
			return true;
		}
	}

	internal PatternSwitchBinder(Binder next, SwitchStatementSyntax switchSyntax)
		: base(next, switchSyntax)
	{
	}

	internal override BoundStatement BindSwitchExpressionAndSections(SwitchStatementSyntax node, Binder originalBinder, DiagnosticBag diagnostics)
	{
		if (!UseV7SwitchBinder)
		{
			return base.BindSwitchExpressionAndSections(node, originalBinder, diagnostics);
		}
		BoundExpression switchGoverningExpression = SwitchGoverningExpression;
		diagnostics.AddRange(SwitchGoverningDiagnostics);
		ImmutableArray<BoundPatternSwitchSection> switchSections = BindPatternSwitchSections(originalBinder, out var defaultLabel, out var isComplete, out var someCaseMatches, diagnostics);
		ImmutableArray<LocalSymbol> declaredLocalsForScope = GetDeclaredLocalsForScope(node);
		ImmutableArray<LocalFunctionSymbol> declaredLocalFunctionsForScope = GetDeclaredLocalFunctionsForScope(node);
		return new BoundPatternSwitchStatement(node, switchGoverningExpression, someCaseMatches, declaredLocalsForScope, declaredLocalFunctionsForScope, switchSections, defaultLabel, BreakLabel, this, isComplete);
	}

	internal override void BindPatternSwitchLabelForInference(CasePatternSwitchLabelSyntax node, DiagnosticBag diagnostics)
	{
		BoundPatternSwitchLabel defaultLabel = null;
		BindPatternSwitchSectionLabel(GetBinder(node.Parent), node, LabelsByNode[node], ref defaultLabel, diagnostics);
	}

	private ImmutableArray<BoundPatternSwitchSection> BindPatternSwitchSections(Binder originalBinder, out BoundPatternSwitchLabel defaultLabel, out bool isComplete, out bool someCaseMatches, DiagnosticBag diagnostics)
	{
		defaultLabel = null;
		someCaseMatches = false;
		ArrayBuilder<BoundPatternSwitchSection> instance = ArrayBuilder<BoundPatternSwitchSection>.GetInstance();
		SubsumptionDiagnosticBuilder subsumptionDiagnosticBuilder = new SubsumptionDiagnosticBuilder(ContainingMemberOrLambda, SwitchSyntax, Conversions, SwitchGoverningType);
		foreach (SwitchSectionSyntax section in SwitchSyntax.Sections)
		{
			BoundPatternSwitchSection item = BindPatternSwitchSection(section, originalBinder, ref defaultLabel, ref someCaseMatches, subsumptionDiagnosticBuilder, diagnostics);
			instance.Add(item);
		}
		isComplete = (defaultLabel != null || subsumptionDiagnosticBuilder.IsComplete) | someCaseMatches;
		return instance.ToImmutableAndFree();
	}

	private BoundPatternSwitchSection BindPatternSwitchSection(SwitchSectionSyntax node, Binder originalBinder, ref BoundPatternSwitchLabel defaultLabel, ref bool someCaseMatches, SubsumptionDiagnosticBuilder subsumption, DiagnosticBag diagnostics)
	{
		ArrayBuilder<BoundPatternSwitchLabel> instance = ArrayBuilder<BoundPatternSwitchLabel>.GetInstance();
		Binder binder = originalBinder.GetBinder(node);
		Dictionary<SyntaxNode, LabelSymbol> labelsByNode = LabelsByNode;
		foreach (SwitchLabelSyntax label2 in node.Labels)
		{
			LabelSymbol label = labelsByNode[label2];
			BoundPatternSwitchLabel boundPatternSwitchLabel = BindPatternSwitchSectionLabel(binder, label2, label, ref defaultLabel, diagnostics);
			bool flag = subsumption.AddLabel(boundPatternSwitchLabel, diagnostics);
			bool flag2 = boundPatternSwitchLabel.Guard == null || boundPatternSwitchLabel.Guard.ConstantValue == ConstantValue.True;
			bool? flag3;
			if (label2.Kind() != SyntaxKind.DefaultSwitchLabel)
			{
				if (boundPatternSwitchLabel.Pattern.Kind != BoundKind.WildcardPattern)
				{
					if (boundPatternSwitchLabel.Pattern is BoundDeclarationPattern boundDeclarationPattern)
					{
						flag3 = (boundDeclarationPattern.IsVar ? new bool?(true) : inputMatchesType(boundDeclarationPattern.DeclaredType.Type));
					}
					else
					{
						flag3 = ((!(boundPatternSwitchLabel.Pattern is BoundConstantPattern boundConstantPattern)) ? ((bool?)null) : SwitchGoverningExpression.ConstantValue?.Equals(boundConstantPattern.ConstantValue));
					}
				}
				else
				{
					flag3 = true;
				}
			}
			else
			{
				flag3 = null;
			}
			bool isReachable = flag && !someCaseMatches && flag3 != false;
			boundPatternSwitchLabel = boundPatternSwitchLabel.Update(boundPatternSwitchLabel.Label, boundPatternSwitchLabel.Pattern, boundPatternSwitchLabel.Guard, isReachable);
			instance.Add(boundPatternSwitchLabel);
			bool flag4 = flag2 && flag3 == true;
			someCaseMatches |= flag4;
		}
		ArrayBuilder<BoundStatement> instance2 = ArrayBuilder<BoundStatement>.GetInstance();
		foreach (StatementSyntax statement in node.Statements)
		{
			instance2.Add(binder.BindStatement(statement, diagnostics));
		}
		return new BoundPatternSwitchSection(node, binder.GetDeclaredLocalsForScope(node), instance.ToImmutableAndFree(), instance2.ToImmutableAndFree());
		bool? inputMatchesType(TypeSymbol patternType)
		{
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			Conversion conversion;
			return Binder.ExpressionOfTypeMatchesPatternType(Conversions, SwitchGoverningType, patternType, ref useSiteDiagnostics, out conversion, SwitchGoverningExpression.ConstantValue, operandCouldBeNull: true);
		}
	}

	private BoundPatternSwitchLabel BindPatternSwitchSectionLabel(Binder sectionBinder, SwitchLabelSyntax node, LabelSymbol label, ref BoundPatternSwitchLabel defaultLabel, DiagnosticBag diagnostics)
	{
		switch (node.Kind())
		{
		case SyntaxKind.CaseSwitchLabel:
		{
			CaseSwitchLabelSyntax caseSwitchLabelSyntax = (CaseSwitchLabelSyntax)node;
			BoundConstantPattern boundConstantPattern = sectionBinder.BindConstantAsPattern(node, SwitchGoverningType, caseSwitchLabelSyntax.Value, node.HasErrors, diagnostics, out var _);
			bool flag = boundConstantPattern.HasErrors;
			SyntaxNode syntaxNode = caseSwitchLabelSyntax.Value.SkipParens();
			if (syntaxNode.Kind() == SyntaxKind.DefaultLiteralExpression)
			{
				diagnostics.Add(ErrorCode.ERR_DefaultInSwitch, syntaxNode.Location);
				flag = true;
			}
			boundConstantPattern.WasCompilerGenerated = true;
			ConstantValue constantValue = boundConstantPattern.ConstantValue;
			if (!flag && (object)constantValue != null && boundConstantPattern.Value.Type == SwitchGoverningType && FindMatchingSwitchCaseLabel(constantValue, caseSwitchLabelSyntax) != label)
			{
				diagnostics.Add(ErrorCode.ERR_DuplicateCaseLabel, node.Location, boundConstantPattern.ConstantValue.GetValueToDisplay() ?? label.Name);
				flag = true;
			}
			return new BoundPatternSwitchLabel(node, label, boundConstantPattern, null, isReachable: true, flag);
		}
		case SyntaxKind.DefaultSwitchLabel:
		{
			_ = (DefaultSwitchLabelSyntax)node;
			BoundWildcardPattern boundWildcardPattern = new BoundWildcardPattern(node);
			bool hasErrors = boundWildcardPattern.HasErrors;
			if (defaultLabel != null)
			{
				diagnostics.Add(ErrorCode.ERR_DuplicateCaseLabel, node.Location, label.Name);
				hasErrors = true;
			}
			defaultLabel = new BoundPatternSwitchLabel(node, label, boundWildcardPattern, null, isReachable: true, hasErrors);
			return defaultLabel;
		}
		case SyntaxKind.CasePatternSwitchLabel:
		{
			CasePatternSwitchLabelSyntax casePatternSwitchLabelSyntax = (CasePatternSwitchLabelSyntax)node;
			BoundPattern pattern = sectionBinder.BindPattern(SwitchGoverningExpression, casePatternSwitchLabelSyntax.Pattern, SwitchGoverningType, node.HasErrors, diagnostics);
			return new BoundPatternSwitchLabel(node, label, pattern, (casePatternSwitchLabelSyntax.WhenClause != null) ? sectionBinder.BindBooleanExpression(casePatternSwitchLabelSyntax.WhenClause.Condition, diagnostics) : null, isReachable: true, node.HasErrors);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(node);
		}
	}
}
