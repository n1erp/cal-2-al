using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emitter;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal abstract class PageBaseCodeBuilder<T> : ObjectCodeBuilder<T> where T : ObjectTypeSymbol
{
	protected abstract MemberDeclarationSyntax InitializeComponentMethod { get; }

	protected abstract bool HasRelatedTable { get; }

	protected abstract ImmutableArray<ControlSymbol> FlattenedControls { get; }

	protected virtual bool IsExtensionBuilder { get; }

	protected PageBaseCodeBuilder(T symbol, ModuleBuilder moduleBuilder)
		: base(symbol, moduleBuilder)
	{
	}

	public override ClassDeclarationSyntax AddInitializeComponentMethod(ClassDeclarationSyntax classDeclaration)
	{
		if (InitializeComponentMethod != null)
		{
			classDeclaration = classDeclaration.AddMembers(InitializeComponentMethod);
		}
		return classDeclaration;
	}

	public override ClassDeclarationSyntax AddEvaluateCaptionClass(ClassDeclarationSyntax classDeclaration)
	{
		return ApplicationObjectEmitHelper.EmitEvaluateCaptionClass(IsExtensionBuilder, EmitEvaluateCaptionClassSwitchSections, classDeclaration);
	}

	public override ClassDeclarationSyntax AddFrameworkMethods(ClassDeclarationSyntax classDeclaration)
	{
		ArrayBuilder<StatementSyntax> instance = ArrayBuilder<StatementSyntax>.GetInstance();
		PooledDictionary<int, Tuple<int, ExpressionSyntax>> instance2 = PooledDictionary<int, Tuple<int, ExpressionSyntax>>.GetInstance();
		PooledDictionary<int, string> instance3 = PooledDictionary<int, string>.GetInstance();
		try
		{
			ProcessControls(instance, instance2, instance3);
			classDeclaration = PageEmitHelper.EmitOnMetadataLoaded(classDeclaration, instance);
			classDeclaration = ApplicationObjectEmitHelper.EmitGetAutoFormatString(classDeclaration, instance2);
			classDeclaration = ApplicationObjectEmitHelper.EmitGetDecimalPlaces(classDeclaration, instance3);
			return classDeclaration;
		}
		finally
		{
			instance.Free();
			instance2.Free();
			instance3.Free();
		}
	}

	protected virtual void AppendToInitializeComponent(ArrayBuilder<StatementSyntax> builder)
	{
		PageEmitHelper.ProcessPageParts(GetParts(), builder);
	}

	protected ImmutableArray<ControlSymbol> GetParts()
	{
		return FlattenedControls.WhereAsArray((ControlSymbol c) => c.ControlKind == ControlKind.Part);
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	private void ProcessControls(ArrayBuilder<StatementSyntax> expressions, PooledDictionary<int, Tuple<int, ExpressionSyntax>> autoFormatStrings, PooledDictionary<int, string> decimalPlaceString)
	{
		PooledHashSet<string> instance = PooledHashSet<string>.GetInstance();
		try
		{
			PageEmitHelper.EmitRegisterDataCaptionExpression(Symbol, expressions, ModuleBuilder.CodeGenerator);
			ImmutableHashSet<Symbol> implicitInDataset = InDataSetVisitor.GetAllImplicitInDatasetVariables(Symbol);
			IEnumerable<VariableSymbol> inDataSetVariables = Symbol.GlobalVariables.Where((VariableSymbol g) => g.IsInDataSet || implicitInDataset.Contains(g));
			PageEmitHelper.ProcessInDataSetVariables(Symbol, inDataSetVariables, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), expressions);
			foreach (SourceControlSymbol item in FlattenedControls.Where((ControlSymbol c) => c.ControlKind != ControlKind.Area).Cast<SourceControlSymbol>())
			{
				PageEmitHelper.ProcessFieldControls(item, expressions, autoFormatStrings, decimalPlaceString, instance, ModuleBuilder.CodeGenerator);
			}
		}
		finally
		{
			instance.Free();
		}
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	protected void EmitEvaluateCaptionClassSwitchSections(ArrayBuilder<SwitchSectionSyntax> builder)
	{
		foreach (SourceControlSymbol item in FlattenedControls.Where((ControlSymbol c) => c.ControlKind == ControlKind.Field).Cast<SourceControlSymbol>())
		{
			PageEmitHelper.EmitEvaluateCaptionClassSwitchSection(builder, item, HasRelatedTable, ModuleBuilder.CodeGenerator);
		}
	}
}
