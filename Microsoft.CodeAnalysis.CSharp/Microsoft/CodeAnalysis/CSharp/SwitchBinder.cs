using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class SwitchBinder : LocalScopeBinder
{
	protected readonly SwitchStatementSyntax SwitchSyntax;

	private readonly GeneratedLabelSymbol _breakLabel;

	private BoundExpression _switchGoverningExpression;

	private DiagnosticBag _switchGoverningDiagnostics;

	private Dictionary<object, SourceLabelSymbol> _lazySwitchLabelsMap;

	private static readonly object s_defaultKey = new object();

	private static readonly object s_nullKey = new object();

	private Dictionary<SyntaxNode, LabelSymbol> _labelsByNode;

	protected bool PatternsEnabled => ((CSharpParseOptions)SwitchSyntax.SyntaxTree.Options)?.IsFeatureEnabled(MessageID.IDS_FeaturePatternMatching) ?? true;

	protected BoundExpression SwitchGoverningExpression
	{
		get
		{
			if (_switchGoverningExpression == null)
			{
				EnsureSwitchGoverningExpressionAndDiagnosticsBound();
			}
			return _switchGoverningExpression;
		}
	}

	protected TypeSymbol SwitchGoverningType => SwitchGoverningExpression.Type;

	protected DiagnosticBag SwitchGoverningDiagnostics
	{
		get
		{
			EnsureSwitchGoverningExpressionAndDiagnosticsBound();
			return _switchGoverningDiagnostics;
		}
	}

	private Dictionary<object, SourceLabelSymbol> LabelsByValue
	{
		get
		{
			if (_lazySwitchLabelsMap == null && Labels.Length > 0)
			{
				_lazySwitchLabelsMap = BuildLabelsByValue(Labels);
			}
			return _lazySwitchLabelsMap;
		}
	}

	internal override bool IsLocalFunctionsScopeBinder => true;

	internal override GeneratedLabelSymbol BreakLabel => _breakLabel;

	internal override bool IsLabelsScopeBinder => true;

	internal override SyntaxNode ScopeDesignator => SwitchSyntax;

	protected Dictionary<SyntaxNode, LabelSymbol> LabelsByNode
	{
		get
		{
			if (_labelsByNode == null)
			{
				Dictionary<SyntaxNode, LabelSymbol> dictionary = new Dictionary<SyntaxNode, LabelSymbol>();
				foreach (LabelSymbol label in Labels)
				{
					SyntaxNode syntaxNode = ((SourceLabelSymbol)label).IdentifierNodeOrToken.AsNode();
					if (syntaxNode != null)
					{
						dictionary.Add(syntaxNode, label);
					}
				}
				_labelsByNode = dictionary;
			}
			return _labelsByNode;
		}
	}

	protected SwitchBinder(Binder next, SwitchStatementSyntax switchSyntax)
		: base(next)
	{
		SwitchSyntax = switchSyntax;
		_breakLabel = new GeneratedLabelSymbol("break");
	}

	internal static SwitchBinder Create(Binder next, SwitchStatementSyntax switchSyntax)
	{
		if (switchSyntax?.SyntaxTree?.Options is CSharpParseOptions cSharpParseOptions && !cSharpParseOptions.IsFeatureEnabled(MessageID.IDS_FeaturePatternMatching) && (object)cSharpParseOptions != null && !cSharpParseOptions.Features.ContainsKey("testV7SwitchBinder") && (!switchSyntax.HasErrors || !HasPatternSwitchSyntax(switchSyntax)))
		{
			return new SwitchBinder(next, switchSyntax);
		}
		return new PatternSwitchBinder(next, switchSyntax);
	}

	internal static bool HasPatternSwitchSyntax(SwitchStatementSyntax switchSyntax)
	{
		foreach (SwitchSectionSyntax section in switchSyntax.Sections)
		{
			if (section.Labels.Any(SyntaxKind.CasePatternSwitchLabel))
			{
				return true;
			}
		}
		return false;
	}

	private void EnsureSwitchGoverningExpressionAndDiagnosticsBound()
	{
		DiagnosticBag diagnosticBag = new DiagnosticBag();
		BoundExpression value = BindSwitchExpression(diagnosticBag);
		_switchGoverningDiagnostics = diagnosticBag;
		Interlocked.CompareExchange(ref _switchGoverningExpression, value, null);
	}

	private static Dictionary<object, SourceLabelSymbol> BuildLabelsByValue(ImmutableArray<LabelSymbol> labels)
	{
		Dictionary<object, SourceLabelSymbol> dictionary = new Dictionary<object, SourceLabelSymbol>(labels.Length, new SwitchConstantValueHelper.SwitchLabelsComparer());
		foreach (SourceLabelSymbol item in labels)
		{
			SyntaxKind syntaxKind = item.IdentifierNodeOrToken.Kind();
			if (syntaxKind != SyntaxKind.IdentifierToken)
			{
				ConstantValue switchCaseLabelConstant = item.SwitchCaseLabelConstant;
				object key;
				if ((object)switchCaseLabelConstant != null && !switchCaseLabelConstant.IsBad)
				{
					key = KeyForConstant(switchCaseLabelConstant);
				}
				else
				{
					key = ((syntaxKind != SyntaxKind.DefaultSwitchLabel) ? item.IdentifierNodeOrToken.AsNode() : s_defaultKey);
				}
				if (!dictionary.ContainsKey(key))
				{
					dictionary.Add(key, item);
				}
			}
		}
		return dictionary;
	}

	protected override ImmutableArray<LocalSymbol> BuildLocals()
	{
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		foreach (SwitchSectionSyntax section in SwitchSyntax.Sections)
		{
			instance.AddRange(BuildLocals(section.Statements, GetBinder(section)));
		}
		return instance.ToImmutableAndFree();
	}

	protected override ImmutableArray<LocalFunctionSymbol> BuildLocalFunctions()
	{
		ArrayBuilder<LocalFunctionSymbol> instance = ArrayBuilder<LocalFunctionSymbol>.GetInstance();
		foreach (SwitchSectionSyntax section in SwitchSyntax.Sections)
		{
			instance.AddRange(BuildLocalFunctions(section.Statements));
		}
		return instance.ToImmutableAndFree();
	}

	protected override ImmutableArray<LabelSymbol> BuildLabels()
	{
		ArrayBuilder<LabelSymbol> labels = ArrayBuilder<LabelSymbol>.GetInstance();
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		foreach (SwitchSectionSyntax section in SwitchSyntax.Sections)
		{
			BuildSwitchLabels(section.Labels, GetBinder(section), labels, instance);
			BuildLabels(section.Statements, ref labels);
		}
		instance.Free();
		return labels.ToImmutableAndFree();
	}

	private void BuildSwitchLabels(SyntaxList<SwitchLabelSyntax> labelsSyntax, Binder sectionBinder, ArrayBuilder<LabelSymbol> labels, DiagnosticBag tempDiagnosticBag)
	{
		foreach (SwitchLabelSyntax item in labelsSyntax)
		{
			ConstantValue constantValueOpt = null;
			switch (item.Kind())
			{
			case SyntaxKind.CaseSwitchLabel:
			{
				CaseSwitchLabelSyntax caseSwitchLabelSyntax = (CaseSwitchLabelSyntax)item;
				BoundExpression caseExpression = sectionBinder.BindValue(caseSwitchLabelSyntax.Value, tempDiagnosticBag, BindValueKind.RValue);
				ConvertCaseExpression(item, caseExpression, sectionBinder, ref constantValueOpt, tempDiagnosticBag);
				break;
			}
			case SyntaxKind.CasePatternSwitchLabel:
			{
				CasePatternSwitchLabelSyntax casePatternSwitchLabelSyntax = (CasePatternSwitchLabelSyntax)item;
				sectionBinder.BindPattern(SwitchGoverningExpression, casePatternSwitchLabelSyntax.Pattern, SwitchGoverningType, item.HasErrors, tempDiagnosticBag);
				break;
			}
			}
			labels.Add(new SourceLabelSymbol((MethodSymbol)ContainingMemberOrLambda, item, constantValueOpt));
		}
	}

	protected BoundExpression ConvertCaseExpression(CSharpSyntaxNode node, BoundExpression caseExpression, Binder sectionBinder, ref ConstantValue constantValueOpt, DiagnosticBag diagnostics, bool isGotoCaseExpr = false)
	{
		if (isGotoCaseExpr)
		{
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			Conversion conversion = Conversions.ClassifyConversionFromExpression(caseExpression, SwitchGoverningType, ref useSiteDiagnostics);
			diagnostics.Add(node, useSiteDiagnostics);
			if (!conversion.IsValid)
			{
				GenerateImplicitConversionError(diagnostics, node, conversion, caseExpression, SwitchGoverningType);
			}
			else if (!conversion.IsImplicit)
			{
				diagnostics.Add(ErrorCode.WRN_GotoCaseShouldConvert, node.Location, SwitchGoverningType);
			}
			caseExpression = CreateConversion(caseExpression, conversion, SwitchGoverningType, diagnostics);
		}
		return ConvertPatternExpression(SwitchGoverningType, node, caseExpression, ref constantValueOpt, diagnostics);
	}

	protected static object KeyForConstant(ConstantValue constantValue)
	{
		if (!constantValue.IsNull)
		{
			return constantValue.Value;
		}
		return s_nullKey;
	}

	protected SourceLabelSymbol FindMatchingSwitchCaseLabel(ConstantValue constantValue, CSharpSyntaxNode labelSyntax)
	{
		object key = (((object)constantValue == null || constantValue.IsBad) ? labelSyntax : KeyForConstant(constantValue));
		return FindMatchingSwitchLabel(key);
	}

	private SourceLabelSymbol GetDefaultLabel()
	{
		return FindMatchingSwitchLabel(s_defaultKey);
	}

	private SourceLabelSymbol FindMatchingSwitchLabel(object key)
	{
		Dictionary<object, SourceLabelSymbol> labelsByValue = LabelsByValue;
		if (labelsByValue != null && labelsByValue.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	internal override ImmutableArray<LocalSymbol> GetDeclaredLocalsForScope(SyntaxNode scopeDesignator)
	{
		if (SwitchSyntax == scopeDesignator)
		{
			return Locals;
		}
		throw ExceptionUtilities.Unreachable;
	}

	internal override ImmutableArray<LocalFunctionSymbol> GetDeclaredLocalFunctionsForScope(CSharpSyntaxNode scopeDesignator)
	{
		if (SwitchSyntax == scopeDesignator)
		{
			return LocalFunctions;
		}
		throw ExceptionUtilities.Unreachable;
	}

	internal override BoundStatement BindSwitchExpressionAndSections(SwitchStatementSyntax node, Binder originalBinder, DiagnosticBag diagnostics)
	{
		BoundExpression switchGoverningExpression = SwitchGoverningExpression;
		diagnostics.AddRange(SwitchGoverningDiagnostics);
		LabelSymbol constantTargetOpt = null;
		ConstantValue constantValue = switchGoverningExpression.ConstantValue;
		if (constantValue != null)
		{
			constantTargetOpt = BindConstantJumpTarget(constantValue, node);
		}
		else if (!node.Sections.Any())
		{
			constantTargetOpt = BreakLabel;
		}
		ImmutableArray<BoundSwitchSection> switchSections = BindSwitchSections(node.Sections, originalBinder, diagnostics);
		return new BoundSwitchStatement(node, null, switchGoverningExpression, constantTargetOpt, GetDeclaredLocalsForScope(node), GetDeclaredLocalFunctionsForScope(node), switchSections, BreakLabel, null);
	}

	internal override void BindPatternSwitchLabelForInference(CasePatternSwitchLabelSyntax node, DiagnosticBag diagnostics)
	{
		throw ExceptionUtilities.Unreachable;
	}

	private BoundExpression BindSwitchExpression(DiagnosticBag diagnostics)
	{
		ExpressionSyntax expression = SwitchSyntax.Expression;
		Binder binder = GetBinder(expression);
		BoundExpression boundExpression = binder.BindValue(expression, diagnostics, BindValueKind.RValue);
		TypeSymbol typeSymbol = boundExpression.Type;
		if ((object)typeSymbol != null && !typeSymbol.IsErrorType())
		{
			if (typeSymbol.IsValidV6SwitchGoverningType())
			{
				if (typeSymbol.SpecialType == SpecialType.System_Boolean)
				{
					Binder.CheckFeatureAvailability(expression, MessageID.IDS_FeatureSwitchOnBool, diagnostics);
				}
				return boundExpression;
			}
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			Conversion conversion = binder.Conversions.ClassifyImplicitUserDefinedConversionForV6SwitchGoverningType(typeSymbol, out var switchGoverningType, ref useSiteDiagnostics);
			diagnostics.Add(expression, useSiteDiagnostics);
			if (conversion.IsValid)
			{
				return binder.CreateConversion(expression, boundExpression, conversion, isCast: false, switchGoverningType, diagnostics);
			}
			if (typeSymbol.SpecialType != SpecialType.System_Void)
			{
				if (!PatternsEnabled)
				{
					diagnostics.Add(ErrorCode.ERR_V6SwitchGoverningTypeValueExpected, expression.Location);
				}
				return boundExpression;
			}
			typeSymbol = CreateErrorType(typeSymbol.Name);
		}
		if (!boundExpression.HasAnyErrors)
		{
			diagnostics.Add(ErrorCode.ERR_SwitchExpressionValueExpected, expression.Location, boundExpression.Display);
		}
		return new BoundBadExpression(expression, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray.Create(boundExpression), typeSymbol ?? CreateErrorType());
	}

	private LabelSymbol BindConstantJumpTarget(ConstantValue constantValue, CSharpSyntaxNode syntax)
	{
		SourceLabelSymbol sourceLabelSymbol = FindMatchingSwitchCaseLabel(constantValue, syntax);
		if ((object)sourceLabelSymbol != null)
		{
			return sourceLabelSymbol;
		}
		sourceLabelSymbol = GetDefaultLabel();
		if ((object)sourceLabelSymbol != null)
		{
			return sourceLabelSymbol;
		}
		return BreakLabel;
	}

	private ImmutableArray<BoundSwitchSection> BindSwitchSections(SyntaxList<SwitchSectionSyntax> switchSections, Binder originalBinder, DiagnosticBag diagnostics)
	{
		ArrayBuilder<BoundSwitchSection> instance = ArrayBuilder<BoundSwitchSection>.GetInstance();
		foreach (SwitchSectionSyntax item in switchSections)
		{
			instance.Add(BindSwitchSection(item, originalBinder, diagnostics));
		}
		return instance.ToImmutableAndFree();
	}

	private BoundSwitchSection BindSwitchSection(SwitchSectionSyntax node, Binder originalBinder, DiagnosticBag diagnostics)
	{
		Binder binder = originalBinder.GetBinder(node);
		ImmutableArray<LocalSymbol> declaredLocalsForScope = binder.GetDeclaredLocalsForScope(node);
		ArrayBuilder<BoundSwitchLabel> instance = ArrayBuilder<BoundSwitchLabel>.GetInstance();
		foreach (SwitchLabelSyntax label2 in node.Labels)
		{
			LabelSymbol label = LabelsByNode[label2];
			BoundSwitchLabel item = BindSwitchSectionLabel(label2, binder, label, diagnostics);
			instance.Add(item);
		}
		ArrayBuilder<BoundStatement> instance2 = ArrayBuilder<BoundStatement>.GetInstance();
		foreach (StatementSyntax statement in node.Statements)
		{
			instance2.Add(binder.BindStatement(statement, diagnostics));
		}
		return new BoundSwitchSection(node, declaredLocalsForScope, instance.ToImmutableAndFree(), instance2.ToImmutableAndFree());
	}

	private BoundSwitchLabel BindSwitchSectionLabel(SwitchLabelSyntax node, Binder sectionBinder, LabelSymbol label, DiagnosticBag diagnostics)
	{
		_ = SwitchGoverningType;
		BoundExpression boundExpression = null;
		ConstantValue constantValueOpt = null;
		bool flag = node.HasErrors;
		switch (node.Kind())
		{
		case SyntaxKind.CaseSwitchLabel:
		{
			CaseSwitchLabelSyntax caseSwitchLabelSyntax = (CaseSwitchLabelSyntax)node;
			boundExpression = sectionBinder.BindValue(caseSwitchLabelSyntax.Value, diagnostics, BindValueKind.RValue);
			boundExpression = ConvertCaseExpression(caseSwitchLabelSyntax, boundExpression, sectionBinder, ref constantValueOpt, diagnostics);
			flag = flag || boundExpression.HasAnyErrors;
			if (!flag && constantValueOpt == null)
			{
				diagnostics.Add(ErrorCode.ERR_ConstantExpected, caseSwitchLabelSyntax.Value.Location);
				flag = true;
			}
			if (!flag && (object)constantValueOpt != null && FindMatchingSwitchCaseLabel(constantValueOpt, caseSwitchLabelSyntax) != label)
			{
				diagnostics.Add(ErrorCode.ERR_DuplicateCaseLabel, node.Location, constantValueOpt?.GetValueToDisplay() ?? label.Name);
				flag = true;
			}
			SyntaxNode syntaxNode = caseSwitchLabelSyntax.Value.SkipParens();
			if (syntaxNode.Kind() == SyntaxKind.DefaultLiteralExpression)
			{
				diagnostics.Add(ErrorCode.ERR_DefaultInSwitch, syntaxNode.Location);
			}
			break;
		}
		case SyntaxKind.CasePatternSwitchLabel:
			if (!node.HasErrors)
			{
				throw ExceptionUtilities.UnexpectedValue(node.Kind());
			}
			break;
		case SyntaxKind.DefaultSwitchLabel:
			if (GetDefaultLabel() != label)
			{
				diagnostics.Add(ErrorCode.ERR_DuplicateCaseLabel, node.Location, label.Name);
				flag = true;
			}
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(node.Kind());
		}
		return new BoundSwitchLabel(node, label, boundExpression, constantValueOpt, flag);
	}

	internal BoundStatement BindGotoCaseOrDefault(GotoStatementSyntax node, Binder gotoBinder, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = null;
		if (!node.HasErrors)
		{
			ConstantValue constantValueOpt = null;
			bool flag = false;
			SourceLabelSymbol sourceLabelSymbol;
			if (node.Expression != null)
			{
				boundExpression = gotoBinder.BindValue(node.Expression, diagnostics, BindValueKind.RValue);
				boundExpression = ConvertCaseExpression(node, boundExpression, gotoBinder, ref constantValueOpt, diagnostics, isGotoCaseExpr: true);
				flag = flag || boundExpression.HasAnyErrors;
				if (!flag && constantValueOpt == null)
				{
					diagnostics.Add(ErrorCode.ERR_ConstantExpected, node.Location);
					flag = true;
				}
				sourceLabelSymbol = FindMatchingSwitchCaseLabel(constantValueOpt, node);
			}
			else
			{
				sourceLabelSymbol = GetDefaultLabel();
			}
			if ((object)sourceLabelSymbol != null)
			{
				return new BoundGotoStatement(node, sourceLabelSymbol, boundExpression, null, flag);
			}
			if (!flag)
			{
				string text = SyntaxFacts.GetText(node.CaseOrDefaultKeyword.Kind());
				if (node.Kind() == SyntaxKind.GotoCaseStatement)
				{
					text = text + " " + constantValueOpt.Value;
				}
				text += ":";
				diagnostics.Add(ErrorCode.ERR_LabelNotFound, node.Location, text);
			}
		}
		return new BoundBadStatement(node, (boundExpression != null) ? ImmutableArray.Create((BoundNode)boundExpression) : ImmutableArray<BoundNode>.Empty, hasErrors: true);
	}
}
