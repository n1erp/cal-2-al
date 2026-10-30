using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal static class PageEmitHelper
{
	public static ClassDeclarationSyntax AddRecAndCurrPage(ClassDeclarationSyntax classDeclaration, bool hasRelatedTable, TypeSyntax currPageType)
	{
		if (hasRelatedTable)
		{
			TypeSyntax navRecordType = CodeGenFacts.NavRecordType;
			classDeclaration = classDeclaration.AddMembers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PropertyDeclaration(navRecordType, "Rec").AddAccessorListAccessors(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AccessorDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(navRecordType, CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), CodeGenFacts.SourceTablePropertyNameSyntax)))))).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PropertyDeclaration(navRecordType, "xRec").AddAccessorListAccessors(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AccessorDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(navRecordType, CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), CodeGenFacts.OldRecordPropertyNameSyntax)))))).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword)));
		}
		classDeclaration = classDeclaration.AddMembers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PropertyDeclaration(currPageType, "CurrPage").AddAccessorListAccessors(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AccessorDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(currPageType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()))))).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword)));
		return classDeclaration;
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public static ClassDeclarationSyntax AddUserControlProperties(ClassDeclarationSyntax classDeclaration, ImmutableArray<ControlSymbol> flattenedControls)
	{
		foreach (ControlSymbol item in flattenedControls.Where((ControlSymbol x) => x.ControlKind == ControlKind.UserControl))
		{
			string identifier = item.MetadataName.MangleUserDefinedIdentifier();
			string text = item.MetadataName.MangleUserDefinedIdentifier(uppercase: false);
			MemberAccessExpressionSyntax memberAccessExpressionSyntax = CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(text));
			ObjectCreationExpressionSyntax right = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.NavDotNetType).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("CurrPage"))), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(item.Id)));
			AssignmentExpressionSyntax expression = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, memberAccessExpressionSyntax, right);
			BinaryExpressionSyntax condition = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.EqualsExpression, memberAccessExpressionSyntax, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NullLiteralExpression));
			classDeclaration = classDeclaration.AddMembers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.FieldDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclaration(CodeGenFacts.NavDotNetType).AddVariables(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclarator(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(text)))).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword)).AddAttributeLists(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeList().AddAttributes(CodeGenFacts.DebuggerBrowsableNeverAttribute)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PropertyDeclaration(CodeGenFacts.NavDotNetType, identifier).AddAccessorListAccessors(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AccessorDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IfStatement(condition, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(expression))), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(memberAccessExpressionSyntax)))).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword))
				.AddAttributeLists(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeList().AddAttributes(CodeGenFacts.DebuggerBrowsableNeverAttribute)));
		}
		return classDeclaration;
	}

	public static ClassDeclarationSyntax EmitOnMetadataLoaded(ClassDeclarationSyntax classDecl, ArrayBuilder<StatementSyntax> statements)
	{
		return classDecl.AddMembers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MethodDeclaration(CodeGenFacts.ClrVoidType, "OnMetadataLoaded").AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ProtectedKeyword), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OverrideKeyword)).AddBodyStatements(CodeGenFacts.InvokeCallOnMetadataLoadedExtensionStatement)
			.AddBodyStatements(statements.ToArray()));
	}

	internal static void EmitRegisterDataCaptionExpression(ContainerSymbol symbol, ArrayBuilder<StatementSyntax> expressions, CodeGenerator codeGenerator)
	{
		BoundCodeExpressionPropertyValue boundPropertyValue = symbol.GetBoundPropertyValue<BoundCodeExpressionPropertyValue>(PropertyKind.DataCaptionExpression);
		if (boundPropertyValue != null)
		{
			expressions.Add(CreateRegisterSourceExpressionSyntax("DataCaptionExprCode", "DataCaptionExprCode", boundPropertyValue.Expression, CodeGenFacts.SourceExpressionTypeDataCaptionExpression, codeGenerator, isWritable: false));
		}
	}

	internal static void ProcessPageParts(ImmutableArray<ControlSymbol> pageParts, ArrayBuilder<StatementSyntax> builder)
	{
		foreach (ControlSymbol item in pageParts)
		{
			if (item.RelatedPartSymbol != null)
			{
				builder.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), CodeGenFacts.RegisterUIPartMethodNameSyntax)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(item.Id.ToString(CultureInfo.InvariantCulture))), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(item.RelatedPartSymbol.Id)))));
			}
		}
	}

	internal static void ProcessInDataSetVariables(Symbol pageSymbol, IEnumerable<VariableSymbol> inDataSetVariables, ExpressionSyntax variableScopeAccess, ArrayBuilder<StatementSyntax> builder)
	{
		foreach (VariableSymbol inDataSetVariable in inDataSetVariables)
		{
			string prefixedExpressionName = PageMetadataEmitHelper.GetPrefixedExpressionName(inDataSetVariable.Name, pageSymbol);
			builder.Add(CreateRegisterSourceExpressionSyntax(prefixedExpressionName, inDataSetVariable.Type, CodeGenHelper.MemberAccessExpression(variableScopeAccess, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(inDataSetVariable.MetadataName)), isAssignable: true, CodeGenFacts.SourceExpressionTypeVariableIncludedInDataSetExpression));
		}
	}

	internal static void ProcessFieldControlsRecurse(SourceControlSymbol control, ArrayBuilder<StatementSyntax> expressions, PooledDictionary<int, Tuple<int, ExpressionSyntax>> autoFormatStrings, PooledDictionary<int, string> decimalPlaceString, PooledHashSet<string> sourceExpressions, CodeGenerator codeGenerator)
	{
		ProcessFieldControls(control, expressions, autoFormatStrings, decimalPlaceString, sourceExpressions, codeGenerator);
		foreach (SourceControlSymbol item in control.Controls.Cast<SourceControlSymbol>())
		{
			ProcessFieldControlsRecurse(item, expressions, autoFormatStrings, decimalPlaceString, sourceExpressions, codeGenerator);
		}
	}

	internal static void ProcessFieldControls(SourceControlSymbol control, ArrayBuilder<StatementSyntax> expressions, PooledDictionary<int, Tuple<int, ExpressionSyntax>> autoFormatStrings, PooledDictionary<int, string> decimalPlaceString, PooledHashSet<string> sourceExpressions, CodeGenerator codeGenerator)
	{
		switch (control.ControlKind)
		{
		case ControlKind.Field:
			ProcessFieldControl(control, expressions, autoFormatStrings, decimalPlaceString, sourceExpressions, codeGenerator);
			break;
		case ControlKind.Repeater:
			ProcessRepeaterControl(control, expressions, codeGenerator);
			break;
		}
	}

	private static void ProcessFieldControl(SourceControlSymbol control, ArrayBuilder<StatementSyntax> expressions, PooledDictionary<int, Tuple<int, ExpressionSyntax>> autoFormatStrings, PooledDictionary<int, string> decimalPlaceString, PooledHashSet<string> sourceExpressions, CodeGenerator codeGenerator)
	{
		DebugAssertHelper.Assert(control != null);
		if (!control.IsSourceRecordField)
		{
			string text = control.SourceExpression.Syntax.ToString();
			if (!sourceExpressions.Contains(text) || ObjectMetadataEmitHelper.AllowControlSourceExpressionDuplicate(control.SourceExpression))
			{
				expressions.Add(CreateRegisterSourceExpressionSyntax("Control" + control.Id.ToString(CultureInfo.InvariantCulture), text, control.SourceExpression, CodeGenFacts.SourceExpressionTypeSourceExpressionExpression, codeGenerator));
				sourceExpressions.Add(text);
			}
		}
		BoundFieldAccess recordFieldAccess = control.GetRecordFieldAccess();
		if (control.SourceExpression != null && control.SourceExpression.Type.NavTypeKind == NavTypeKind.Decimal)
		{
			ProcessDecimalField(control, expressions, autoFormatStrings, decimalPlaceString, recordFieldAccess, codeGenerator);
		}
		if (control.GetBoundPropertyValue<BoundCodeExpressionPropertyValue>(PropertyKind.CaptionClass) != null || recordFieldAccess != null)
		{
			expressions.Add(CreateRegisterDynamicCaptionExpressionSyntax(control.Id));
		}
		if (recordFieldAccess != null && control.IsCueControl())
		{
			expressions.Add(CreateRegisterDynamicCueStyleExpressionSyntax(control, recordFieldAccess));
		}
	}

	private static void ProcessDecimalField(SourceControlSymbol control, ArrayBuilder<StatementSyntax> expressions, PooledDictionary<int, Tuple<int, ExpressionSyntax>> autoFormatStrings, PooledDictionary<int, string> decimalPlaceString, BoundFieldAccess fieldAccess, CodeGenerator codeGenerator)
	{
		BoundCodeExpressionPropertyValue boundPropertyValue = control.GetBoundPropertyValue<BoundCodeExpressionPropertyValue>(PropertyKind.AutoFormatExpression);
		int num = control.GetBoundPropertyValue<BoundLiteralPropertyValue>(PropertyKind.AutoFormatType)?.Value?.Int32Value ?? 0;
		string decimalPlacesFormatString = ApplicationObjectEmitHelper.GetDecimalPlacesFormatString(control);
		if (boundPropertyValue != null && num > 0)
		{
			ExpressionSyntax expressionSyntax = codeGenerator.EmitExpression(boundPropertyValue.Expression);
			expressions.Add(CreateRegisterAutoFormatSourceExpressionSyntax(control.Id, num, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedLambdaExpression(expressionSyntax)));
			autoFormatStrings.Add(control.Id, new Tuple<int, ExpressionSyntax>(num, expressionSyntax));
		}
		else if (fieldAccess != null)
		{
			expressions.Add(CreateRegisterGetDecimalStringExpressionSyntax(control.Id, fieldAccess.ReceiverOpt, fieldAccess.FieldSymbol.Id, codeGenerator, control.ContainingType.Kind.IsExtensionObject()));
			if (decimalPlacesFormatString != null)
			{
				decimalPlaceString.Add(control.Id, decimalPlacesFormatString);
			}
		}
		else if (num == 0 && decimalPlacesFormatString != null)
		{
			expressions.Add(CreateRegisterFormatStringExpressionSyntax(control.Id, decimalPlacesFormatString));
			decimalPlaceString.Add(control.Id, decimalPlacesFormatString);
		}
		else
		{
			expressions.Add(CreateRegisterAutoFormatSourceExpressionSyntax(control.Id, 1, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedLambdaExpression(CodeGenHelper.LiteralExpression(string.Empty))));
		}
	}

	internal static ExpressionStatementSyntax CreateRegisterSourceExpressionSyntax(string controlId, string sourceExprName, BoundExpression targetExpression, ExpressionSyntax sourceExpressionSyntax, CodeGenerator codeGenerator, bool isWritable = true)
	{
		ExpressionSyntax getter = CreateRegisterSourceExpressionGetter(codeGenerator.EmitExpression(targetExpression), targetExpression.Type);
		ExpressionSyntax setter = CreateRegisterSourceExpressionSetter(targetExpression, codeGenerator, isWritable);
		return CreateRegisterSourceExpressionSyntax(controlId, sourceExprName, targetExpression.Type, sourceExpressionSyntax, getter, setter);
	}

	internal static StatementSyntax CreateRegisterSourceExpressionSyntax(string controlId, TypeSymbol type, ExpressionSyntax targetExpression, bool isAssignable, ExpressionSyntax sourceExpressionSyntax)
	{
		ExpressionSyntax getter = CreateRegisterSourceExpressionGetter(targetExpression, type);
		ExpressionSyntax setter = CreateRegisterSourceExpressionSetter(targetExpression, type, isAssignable);
		return CreateRegisterSourceExpressionSyntax(controlId, controlId, type, sourceExpressionSyntax, getter, setter);
	}

	internal static ExpressionSyntax CreateRegisterSourceExpressionGetter(ExpressionSyntax expression, TypeSymbol expressionType)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedLambdaExpression(CodeGenerator.EmitToJokerConversion(expressionType, expression));
	}

	internal static void EmitEvaluateCaptionClassSwitchSection(ArrayBuilder<SwitchSectionSyntax> builder, SourceControlSymbol c, bool isTableBased, CodeGenerator codeGenerator)
	{
		PropertySymbol property = c.GetProperty(PropertyKind.CaptionClass);
		if (property != null)
		{
			if (property.Property.PropertyValue is BoundCodeExpressionPropertyValue boundCodeExpressionPropertyValue)
			{
				builder.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SwitchSection().AddLabels(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CaseSwitchLabel(CodeGenHelper.LiteralExpression(c.Id))).AddStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(codeGenerator.EmitExpression(boundCodeExpressionPropertyValue.Expression))));
			}
		}
		else
		{
			if (!isTableBased)
			{
				return;
			}
			BoundExpression sourceExpression = c.SourceExpression;
			if (sourceExpression.Kind == BoundKind.FieldAccess)
			{
				BoundFieldAccess boundFieldAccess = (BoundFieldAccess)sourceExpression;
				InvocationExpressionSyntax invocationExpressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression("Rec", "EvaluateCaptionClass"));
				ApplicationObjectExtensionTypeSymbol parentExtensionTypeSymbol = boundFieldAccess.FieldSymbol.GetParentExtensionTypeSymbol();
				if (parentExtensionTypeSymbol != null)
				{
					invocationExpressionSyntax = invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(parentExtensionTypeSymbol.Id)));
				}
				invocationExpressionSyntax = invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(boundFieldAccess.FieldSymbol.Id)));
				builder.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SwitchSection().AddLabels(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CaseSwitchLabel(CodeGenHelper.LiteralExpression(c.Id))).AddStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(invocationExpressionSyntax)));
			}
		}
	}

	internal static ExpressionStatementSyntax CreateRegisterSourceExpressionSyntax(string controlId, string sourceExprName, TypeSymbol targetType, ExpressionSyntax sourceExpressionSyntax, ExpressionSyntax getter, ExpressionSyntax setter)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), CodeGenFacts.RegisterSourceExpressionMethodNameSyntax)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(controlId)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(sourceExprName)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.TypeOfExpression(((Symbol)targetType).GetRuntimeTypeSyntax())), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(sourceExpressionSyntax), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(getter), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(setter)));
	}

	internal static ExpressionStatementSyntax CreateRegisterSourceExpressionSyntax(string controlId, string sourceExprName, TypeSyntax targetType, ExpressionSyntax sourceExpressionSyntax, ExpressionSyntax getter, ExpressionSyntax setter)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), CodeGenFacts.RegisterSourceExpressionMethodNameSyntax)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(controlId)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(sourceExprName)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.TypeOfExpression(targetType)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(sourceExpressionSyntax), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(getter), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(setter)));
	}

	internal static ExpressionStatementSyntax CreateRegisterSourceExpressionSyntax(string id, string name, ExpressionSyntax expression, TypeSymbol expressionType, bool isAssignable, ExpressionSyntax sourceExpressionType)
	{
		ExpressionSyntax expression2 = ((!isAssignable) ? ((ExpressionSyntax)Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NullLiteralExpression)) : ((ExpressionSyntax)Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedLambdaExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParameterList().AddParameters(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("value"))), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, expression, CodeGenerator.EmitFastJokerToTypeConversion(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("value"), expressionType)))));
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), CodeGenFacts.RegisterSourceExpressionMethodNameSyntax)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(id)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(name)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.TypeOfExpression(((Symbol)expressionType).GetRuntimeTypeSyntax())), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(sourceExpressionType), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedLambdaExpression(CodeGenerator.EmitToJokerConversion(expressionType, expression))), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression2)));
	}

	internal static StatementSyntax CreateIndentExpressionSyntax(ContainerSymbol controlOrModification, CodeGenerator codeGenerator)
	{
		DebugAssertHelper.Assert(controlOrModification.GetExtendedPageControlOrSelf().ControlKind == ControlKind.Repeater);
		BoundCodeExpressionPropertyValue boundPropertyValue = controlOrModification.GetBoundPropertyValue<BoundCodeExpressionPropertyValue>(PropertyKind.IndentationColumn);
		if (boundPropertyValue != null && MetadataEmitterHelper.IsIndentationColumnExpression(boundPropertyValue.Expression))
		{
			string indentExpressionName = PageMetadataEmitHelper.GetIndentExpressionName(controlOrModification);
			return CreateRegisterSourceExpressionSyntax(indentExpressionName, indentExpressionName, boundPropertyValue.Expression, CodeGenFacts.SourceExpressionTypePropertyExpression, codeGenerator, isWritable: false);
		}
		return null;
	}

	private static void ProcessRepeaterControl(SourceControlSymbol repeaterControl, ArrayBuilder<StatementSyntax> expressions, CodeGenerator codeGenerator)
	{
		DebugAssertHelper.Assert(repeaterControl.ControlKind == ControlKind.Repeater);
		StatementSyntax statementSyntax = CreateIndentExpressionSyntax(repeaterControl, codeGenerator);
		if (statementSyntax != null)
		{
			expressions.Add(statementSyntax);
		}
	}

	private static StatementSyntax CreateRegisterGetDecimalStringExpressionSyntax(int controlId, BoundExpression receiverRecord, int fieldId, CodeGenerator codeGenerator, bool isExtension)
	{
		DebugAssertHelper.Assert(receiverRecord.Type.NavTypeKind == NavTypeKind.Record, "Receiver must be a Record");
		ArgumentSyntax argumentSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(MetadataEmitterHelper.GetFormatExpressionName(controlId)));
		ArgumentSyntax argumentSyntax2 = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.TypeOfExpression(CodeGenFacts.ClrStringType));
		ArgumentSyntax argumentSyntax3 = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenFacts.SourceExpressionTypePropertyExpression);
		ExpressionSyntax expressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleMemberAccessExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), CodeGenFacts.ParentObjectPropertyNameSyntax);
		ExpressionSyntax expression = (isExtension ? expressionSyntax : Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression());
		InvocationExpressionSyntax invocationExpressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleMemberAccessExpression, expression, CodeGenFacts.GetDecimalStringMethodNameSyntax));
		ArgumentSyntax argumentSyntax4 = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedLambdaExpression(CodeGenFacts.ALCompilerConvertToDotNetFormatStringCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(codeGenerator.EmitExpression(receiverRecord, suppressTarget: true)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NumericLiteralExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Literal(fieldId))), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NumericLiteralExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Literal(controlId))))))));
		ArgumentSyntax argumentSyntax5 = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NullLiteralExpression));
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), CodeGenFacts.RegisterSourceExpressionMethodNameSyntax)).AddArgumentListArguments(argumentSyntax, argumentSyntax, argumentSyntax2, argumentSyntax3, argumentSyntax4, argumentSyntax5));
	}

	private static ExpressionStatementSyntax CreateRegisterAutoFormatSourceExpressionSyntax(int controlId, int autoFormatType, ExpressionSyntax expression)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), CodeGenFacts.RegisterAutoFormatExpressionMethodNameSyntax)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(controlId)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(autoFormatType)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression)));
	}

	private static StatementSyntax CreateRegisterFormatStringExpressionSyntax(int controlId, string formatString)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), CodeGenFacts.RegisterFormatStringExpressionMethodNameSyntax)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(controlId)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(formatString))));
	}

	private static StatementSyntax CreateRegisterDynamicCaptionExpressionSyntax(int controlId)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), CodeGenFacts.RegisterDynamicCaptionExpressionMethodNameSyntax)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(controlId))));
	}

	private static StatementSyntax CreateRegisterDynamicCueStyleExpressionSyntax(SourceControlSymbol control, BoundFieldAccess fieldAccess)
	{
		string dynamicCueStyleExpressionName = PageMetadataEmitHelper.GetDynamicCueStyleExpressionName(control);
		ParenthesizedLambdaExpressionSyntax getter = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedLambdaExpression(CodeGenFacts.GetDynamicCueStyleCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(control.Id)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(fieldAccess.FieldSymbol.Id))));
		return CreateRegisterSourceExpressionSyntax(dynamicCueStyleExpressionName, dynamicCueStyleExpressionName, NavCorLib.TextType, CodeGenFacts.SourceExpressionTypeDynamicCueStyleExpression, getter, CodeGenFacts.NullLiteralExpression);
	}

	private static ExpressionSyntax CreateRegisterSourceExpressionSetter(ExpressionSyntax expression, TypeSymbol expressionType, bool isAssignable)
	{
		if (isAssignable)
		{
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedLambdaExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParameterList().AddParameters(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("value"))), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, expression, CodeGenerator.EmitFastJokerToTypeConversion(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("value"), expressionType)));
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NullLiteralExpression);
	}

	private static ExpressionSyntax CreateRegisterSourceExpressionSetter(BoundExpression target, CodeGenerator codeGenerator, bool isWritable = true)
	{
		if (target.IsAssignable & isWritable)
		{
			TypeSymbol type = target.Type;
			ExpressionSyntax sourceSyntax = CodeGenerator.EmitFastJokerToTypeConversion(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("value"), type);
			ExpressionSyntax body = codeGenerator.EmitAssignmentExpression(type, target, type, sourceSyntax);
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedLambdaExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParameterList().AddParameters(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("value"))), body);
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NullLiteralExpression);
	}
}
