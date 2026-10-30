using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal sealed class PageExtensionCodeBuilder : PageExtensionBaseCodeBuilder<SourcePageExtensionTypeSymbol>
{
	protected override ImmutableArray<ControlSymbol> FlattenedControls { get; }

	public PageExtensionCodeBuilder(SourcePageExtensionTypeSymbol symbol, ModuleBuilder moduleBuilder)
		: base(symbol, moduleBuilder)
	{
		FlattenedControls = symbol.FlattenedControls;
	}

	public override ClassDeclarationSyntax AddEvaluateCaptionClass(ClassDeclarationSyntax classDeclaration)
	{
		IEnumerable<ChangeModifySymbol> modifiedFieldsToEmitCaptionClassFor = (from s in Symbol.GetMembersOfKind<ChangeSymbol>(SymbolKind.Change)
			where s.ChangeKind == ChangeKind.Modify && s.ChangeTargetKind == SymbolKind.Control
			select s).Cast<ChangeModifySymbol>();
		return ApplicationObjectEmitHelper.EmitEvaluateCaptionClass(isExtension: true, (ArrayBuilder<SwitchSectionSyntax> statementBuilder) =>
		{
			EmitEvaluateCaptionClassSwitchSections(statementBuilder);
			EmitEvaluateCaptionClassSwitchSectionsForModified(modifiedFieldsToEmitCaptionClassFor, statementBuilder);
		}, classDeclaration);
	}

	private void EmitEvaluateCaptionClassSwitchSectionsForModified(IEnumerable<ChangeModifySymbol> sourceModifyFields, ArrayBuilder<SwitchSectionSyntax> builder)
	{
		foreach (ChangeModifySymbol sourceModifyField in sourceModifyFields)
		{
			PropertySymbol property = sourceModifyField.GetProperty(PropertyKind.CaptionClass);
			if (property != null)
			{
				BoundCodeExpressionPropertyValue boundCodeExpressionPropertyValue = property.Property.PropertyValue as BoundCodeExpressionPropertyValue;
				builder.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SwitchSection().AddLabels(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CaseSwitchLabel(CodeGenHelper.LiteralExpression(((ISymbolWithId)sourceModifyField.Target).Id))).AddStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(ModuleBuilder.CodeGenerator.EmitExpression(boundCodeExpressionPropertyValue.Expression))));
			}
		}
	}
}
