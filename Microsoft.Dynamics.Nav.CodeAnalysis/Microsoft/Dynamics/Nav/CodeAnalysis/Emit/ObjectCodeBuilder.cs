using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal abstract class ObjectCodeBuilder<T> : CodeBuilder where T : ObjectTypeSymbol
{
	private class DotNetEventHandlerEmitter
	{
		private static readonly MemberAccessExpressionSyntax ApplicationObjectFieldAccessExpression = CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), CodeGenFacts.EventHandlerParentApplicationObjectFieldIdentifierNameSyntax);

		private readonly ObjectCodeBuilder<T> Parent;

		private T Symbol { get; }

		public DotNetEventHandlerEmitter(T symbol, ObjectCodeBuilder<T> parentBuilder)
		{
			Symbol = symbol;
			Parent = parentBuilder;
		}

		public ClassDeclarationSyntax AddEventHandler(ClassDeclarationSyntax objectClassDeclaration)
		{
			ArrayBuilder<MemberDeclarationSyntax> instance = ArrayBuilder<MemberDeclarationSyntax>.GetInstance();
			foreach (VariableSymbol item2 in Symbol.GlobalVariables.Where((VariableSymbol x) => x.Type.NavTypeKind == NavTypeKind.DotNet && x.HasWithEventsAttribute()))
			{
				ClassDeclarationSyntax item = EmitEventHandler(item2);
				instance.Add(item);
			}
			return objectClassDeclaration.AddMembers(instance.ToArrayAndFree());
		}

		private static bool IsEventTriggerForEventPublishedByVariable(Symbol method, VariableSymbol variable)
		{
			if (method.IsKind(SymbolKind.Method) && ((MethodSymbol)method).MethodKind == MethodKind.EventTrigger)
			{
				return (object)((SourceDotNetEventTriggerSymbol)method).Publisher == variable;
			}
			return false;
		}

		private static string GetEventHandlerParameterName(ParameterSymbol parameter)
		{
			return parameter.Name + "Param";
		}

		private static ParameterListSyntax EmitEventHandlerMethodParameters(DotNetEventSymbol eventSymbol)
		{
			ArrayBuilder<ParameterSyntax> instance = ArrayBuilder<ParameterSyntax>.GetInstance();
			foreach (ParameterSymbol dotNetParameter in eventSymbol.DotNetParameters)
			{
				ParameterSyntax parameterSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(GetEventHandlerParameterName(dotNetParameter))).WithType(GetDotNetEventHandlerParameterType(dotNetParameter));
				if (dotNetParameter.IsVar)
				{
					parameterSyntax = parameterSyntax.WithModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.TokenList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.RefKeyword)));
				}
				instance.Add(parameterSyntax);
			}
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParameterList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SeparatedList(instance.ToArrayAndFree()));
		}

		private static TypeSyntax GetDotNetEventHandlerParameterType(ParameterSymbol parameter)
		{
			DotNetTypeSymbol dotNetTypeSymbol = (DotNetTypeSymbol)parameter.Type;
			switch (dotNetTypeSymbol.DotNetTypeInfo.SpecialType)
			{
			case DotNetSpecialType.None:
			case DotNetSpecialType.System_Object:
			case DotNetSpecialType.System_Enum:
			case DotNetSpecialType.System_Stream:
			case DotNetSpecialType.System_Nullable:
				return CodeGenFacts.ClrObjectType;
			case DotNetSpecialType.System_Boolean:
				return CodeGenFacts.ClrBoolType;
			case DotNetSpecialType.System_Char:
				return CodeGenFacts.ClrCharType;
			case DotNetSpecialType.System_SByte:
				return CodeGenFacts.ClrSByteType;
			case DotNetSpecialType.System_Byte:
				return CodeGenFacts.ClrByteType;
			case DotNetSpecialType.System_Int16:
				return CodeGenFacts.ClrInt16Type;
			case DotNetSpecialType.System_UInt16:
				return CodeGenFacts.ClrUInt16Type;
			case DotNetSpecialType.System_Int32:
				return CodeGenFacts.ClrInt32Type;
			case DotNetSpecialType.System_UInt32:
				return CodeGenFacts.ClrUInt32Type;
			case DotNetSpecialType.System_Int64:
				return CodeGenFacts.ClrInt64Type;
			case DotNetSpecialType.System_UInt64:
				return CodeGenFacts.ClrUInt64Type;
			case DotNetSpecialType.System_Decimal:
				return CodeGenFacts.ClrDecimalType;
			case DotNetSpecialType.System_Single:
				return CodeGenFacts.ClrSingleType;
			case DotNetSpecialType.System_Double:
				return CodeGenFacts.ClrDoubleType;
			case DotNetSpecialType.System_String:
				return CodeGenFacts.ClrStringType;
			case DotNetSpecialType.System_TimeSpan:
				return CodeGenFacts.ClrTimeSpanType;
			case DotNetSpecialType.System_DateTime:
				return CodeGenFacts.ClrDateTimeType;
			case DotNetSpecialType.System_Guid:
				return CodeGenFacts.ClrGuidType;
			default:
				throw ExceptionUtilities.UnexpectedValue(dotNetTypeSymbol.DotNetTypeInfo);
			}
		}

		private ClassDeclarationSyntax EmitEventHandler(VariableSymbol variable)
		{
			string text = CodeGenHelper.NavEventHandlerClassName(variable, Parent.ModuleBuilder.Options).ToString();
			ClassDeclarationSyntax eventHandlerClass = ApplicationObjectEmitHelper.CreateClassDeclaration(text, CodeGenFacts.NavInteropEventHandlerBaseType).WithModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.TokenList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword)));
			eventHandlerClass = AddApplicationObjectField(eventHandlerClass);
			eventHandlerClass = AddEventHandlerConstructor(text, eventHandlerClass);
			return AddEventHandlerMethods(variable, eventHandlerClass);
		}

		private ClassDeclarationSyntax AddEventHandlerMethods(VariableSymbol variable, ClassDeclarationSyntax eventHandlerClass)
		{
			ArrayBuilder<MemberDeclarationSyntax> instance = ArrayBuilder<MemberDeclarationSyntax>.GetInstance();
			foreach (SourceDotNetEventTriggerSymbol item in GetEventHandlersForEventsPublishedByInstance(variable))
			{
				AddEventHandlerMethod(item, instance);
			}
			return eventHandlerClass.AddMembers(instance.ToArrayAndFree());
		}

		private void AddEventHandlerMethod(SourceDotNetEventTriggerSymbol eventTrigger, ArrayBuilder<MemberDeclarationSyntax> memberBuilder)
		{
			DotNetEventSymbol dotNetEventSymbol = eventTrigger.Event;
			MethodDeclarationSyntax methodDeclarationSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MethodDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PredefinedType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.VoidKeyword)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(dotNetEventSymbol.Name)).WithModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.TokenList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword)));
			methodDeclarationSyntax = methodDeclarationSyntax.WithParameterList(EmitEventHandlerMethodParameters(dotNetEventSymbol));
			methodDeclarationSyntax = methodDeclarationSyntax.WithBody(EmitEventHandlerMethodBody(eventTrigger));
			memberBuilder.Add(methodDeclarationSyntax);
		}

		private IEnumerable<SourceDotNetEventTriggerSymbol> GetEventHandlersForEventsPublishedByInstance(VariableSymbol variable)
		{
			return (from m in Symbol.GetMembers()
				where IsEventTriggerForEventPublishedByVariable(m, variable)
				select m).Cast<SourceDotNetEventTriggerSymbol>();
		}

		private BlockSyntax EmitEventHandlerMethodBody(SourceDotNetEventTriggerSymbol eventTrigger)
		{
			ObjectCreationExpressionSyntax value = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.NavScopeClassNameSyntax).WithArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SingletonSeparatedList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(ApplicationObjectFieldAccessExpression))));
			VariableDeclaratorSyntax node = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclarator(CodeGenFacts.ScopeLocalIdentifier).WithInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.EqualsValueClause(value));
			VariableDeclarationSyntax declaration = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclaration(CodeGenFacts.NavScopeClassNameSyntax).WithVariables(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SingletonSeparatedList(node));
			UsingStatementSyntax usingStatementSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.UsingStatement(EmitEventHandlerMethodParameterProcessingAndDelegation(eventTrigger)).WithDeclaration(declaration);
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(usingStatementSyntax);
		}

		private StatementSyntax EmitEventHandlerMethodParameterProcessingAndDelegation(SourceDotNetEventTriggerSymbol eventTrigger)
		{
			ArrayBuilder<StatementSyntax> instance = ArrayBuilder<StatementSyntax>.GetInstance();
			ArrayBuilder<StatementSyntax> instance2 = ArrayBuilder<StatementSyntax>.GetInstance();
			ExpressionSyntax[] array = new ExpressionSyntax[eventTrigger.Event.Parameters.Length];
			for (int i = 0; i < eventTrigger.Event.Parameters.Length; i++)
			{
				ParameterSymbol parameterSymbol = eventTrigger.Event.DotNetParameters[i];
				ParameterSymbol alParameterSymbol = eventTrigger.Event.Parameters[i];
				IdentifierNameSyntax dotNetParameterName = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(GetEventHandlerParameterName(parameterSymbol));
				(LocalDeclarationStatementSyntax, IdentifierNameSyntax) tuple = EmitConversionFromEventHandlerParameterToTriggerParameter(parameterSymbol, dotNetParameterName);
				if (tuple.Item1 != null)
				{
					instance.Add(tuple.Item1);
				}
				array[i] = EmitConversionByRefIfNeeded(parameterSymbol, alParameterSymbol, tuple.Item2);
				StatementSyntax statementSyntax = EmitSetParameterValueIfNeeded(parameterSymbol, tuple.Item2);
				if (statementSyntax != null)
				{
					instance2.Add(statementSyntax);
				}
			}
			instance.Add(EmitCallToAlEventTriggerStub(eventTrigger, array));
			instance.AddRange(instance2.ToArrayAndFree());
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(instance.ToArrayAndFree());
		}

		private ExpressionSyntax EmitConversionByRefIfNeeded(ParameterSymbol dotNetParameterSymbol, ParameterSymbol alParameterSymbol, IdentifierNameSyntax variableName)
		{
			if (!dotNetParameterSymbol.IsVar)
			{
				return variableName;
			}
			string name = variableName.ToString();
			BoundTemporary argumentExpression = new BoundTemporary(SyntaxFactory.IdentifierName(name), name, alParameterSymbol.Type);
			return Parent.ModuleBuilder.CodeGenerator.EmitByRefExpression(alParameterSymbol.Type, argumentExpression);
		}

		private StatementSyntax EmitSetParameterValueIfNeeded(ParameterSymbol dotNetParameterSymbol, IdentifierNameSyntax variableName)
		{
			if (!dotNetParameterSymbol.IsVar)
			{
				return null;
			}
			ExpressionSyntax right = EmitConversionFromClrTypeToAlType(dotNetParameterSymbol, variableName);
			IdentifierNameSyntax left = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(GetEventHandlerParameterName(dotNetParameterSymbol));
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, left, right));
		}

		private ExpressionSyntax EmitConversionFromClrTypeToAlType(ParameterSymbol dotNetParameterSymbol, IdentifierNameSyntax variableName)
		{
			DotNetTypeInfo dotNetTypeInfo = ((DotNetTypeSymbol)dotNetParameterSymbol.Type).DotNetTypeInfo;
			switch (dotNetTypeInfo.SpecialType)
			{
			case DotNetSpecialType.System_Object:
			case DotNetSpecialType.System_Boolean:
			case DotNetSpecialType.System_Char:
			case DotNetSpecialType.System_Byte:
			case DotNetSpecialType.System_Int32:
			case DotNetSpecialType.System_Int64:
			case DotNetSpecialType.System_String:
			case DotNetSpecialType.System_TimeSpan:
			case DotNetSpecialType.System_DateTime:
			case DotNetSpecialType.System_Guid:
				return variableName;
			case DotNetSpecialType.System_SByte:
				return CodeGenFacts.ALCompilerToSByteCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(variableName));
			case DotNetSpecialType.System_Int16:
				return CodeGenFacts.ALCompilerToInt16Call.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(variableName));
			case DotNetSpecialType.System_UInt16:
				return CodeGenFacts.ALCompilerToUInt16Call.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(variableName));
			case DotNetSpecialType.System_UInt32:
				return CodeGenFacts.ALCompilerToUInt32Call.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(variableName));
			case DotNetSpecialType.System_UInt64:
				return CodeGenFacts.ALCompilerToUInt64Call.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(variableName));
			case DotNetSpecialType.System_Decimal:
				return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(CodeGenFacts.ClrDecimalType, variableName);
			case DotNetSpecialType.System_Single:
				return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(CodeGenFacts.ClrSingleType, variableName);
			case DotNetSpecialType.System_Double:
				return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(CodeGenFacts.ClrDoubleType, variableName);
			default:
				throw ExceptionUtilities.UnexpectedValue(dotNetTypeInfo);
			}
		}

		private StatementSyntax EmitCallToAlEventTriggerStub(SourceDotNetEventTriggerSymbol eventTrigger, ExpressionSyntax[] namesOfArgumentsToPassToTrigger)
		{
			IEnumerable<ArgumentSyntax> nodes = namesOfArgumentsToPassToTrigger.Select((ExpressionSyntax x) => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(x));
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(ApplicationObjectFieldAccessExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(eventTrigger.MetadataName))).WithArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SeparatedList(nodes))));
		}

		private static (LocalDeclarationStatementSyntax Declaration, IdentifierNameSyntax VariableName) EmitConversionFromEventHandlerParameterToTriggerParameter(ParameterSymbol dotNetParameterSymbol, IdentifierNameSyntax dotNetParameterName)
		{
			ExpressionSyntax expressionSyntax = EmitConversionFromEventHandlerParameterToTriggerParameterType((DotNetTypeSymbol)dotNetParameterSymbol.Type, dotNetParameterName);
			if (expressionSyntax == null && dotNetParameterSymbol.IsVar)
			{
				expressionSyntax = dotNetParameterName;
			}
			if (expressionSyntax == null)
			{
				return (Declaration: null, VariableName: dotNetParameterName);
			}
			return (Declaration: Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LocalDeclarationStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclaration(CodeGenFacts.ClrVarType).WithVariables(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SingletonSeparatedList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclarator(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(dotNetParameterSymbol.Name)).WithInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.EqualsValueClause(expressionSyntax))))), VariableName: Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(dotNetParameterSymbol.Name));
		}

		private static ExpressionSyntax EmitConversionFromEventHandlerParameterToTriggerParameterType(DotNetTypeSymbol dotNetParameterType, IdentifierNameSyntax dotNetParameterName)
		{
			switch (dotNetParameterType.DotNetTypeInfo.SpecialType)
			{
			case DotNetSpecialType.None:
			case DotNetSpecialType.System_Enum:
			case DotNetSpecialType.System_Stream:
			case DotNetSpecialType.System_Nullable:
				return CodeGenHelper.NavDotNetObjectCreationExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("\\u03b2scope"), dotNetParameterName, runOnClient: false, isInterface: true);
			case DotNetSpecialType.System_Object:
			{
				ObjectCreationExpressionSyntax expression = CodeGenHelper.NavDotNetObjectCreationExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("\\u03b2scope"), dotNetParameterName, runOnClient: false, isInterface: true);
				return CodeGenFacts.ALCompilerNavValueToVariantCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("\\u03b2scope")), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
			}
			case DotNetSpecialType.System_UInt16:
				return CodeGenFacts.ALCompilerToIntegerCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(dotNetParameterName));
			case DotNetSpecialType.System_UInt64:
				return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(CodeGenFacts.RuntimeDecimalType, dotNetParameterName);
			case DotNetSpecialType.System_Single:
			case DotNetSpecialType.System_Double:
				return CodeGenFacts.ALCompilerToDecimalCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(dotNetParameterName));
			case DotNetSpecialType.System_String:
				return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.RuntimeTextType).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(dotNetParameterName));
			case DotNetSpecialType.System_TimeSpan:
				return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.RuntimeDurationType).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(dotNetParameterName));
			case DotNetSpecialType.System_DateTime:
				return CodeGenFacts.NavDateTimeCreateCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(dotNetParameterName), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(value: false)));
			case DotNetSpecialType.System_Boolean:
			case DotNetSpecialType.System_Char:
			case DotNetSpecialType.System_SByte:
			case DotNetSpecialType.System_Byte:
			case DotNetSpecialType.System_Int16:
			case DotNetSpecialType.System_Int32:
			case DotNetSpecialType.System_UInt32:
			case DotNetSpecialType.System_Int64:
			case DotNetSpecialType.System_Decimal:
			case DotNetSpecialType.System_Guid:
				return null;
			default:
				throw ExceptionUtilities.UnexpectedValue(dotNetParameterType.DotNetTypeInfo);
			}
		}

		private ClassDeclarationSyntax AddEventHandlerConstructor(string eventHandlerClassName, ClassDeclarationSyntax eventHandlerClass)
		{
			ParameterListSyntax parameterList = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParameterList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SingletonSeparatedList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(CodeGenFacts.EventHandlerParentApplicationObjectFieldNameSyntax).WithType(Parent.ClassType)));
			BlockSyntax body = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SingletonList((StatementSyntax)Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, ApplicationObjectFieldAccessExpression, CodeGenFacts.EventHandlerParentApplicationObjectFieldIdentifierNameSyntax))));
			ConstructorDeclarationSyntax constructorDeclarationSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ConstructorDeclaration(eventHandlerClassName).WithModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.TokenList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword))).WithParameterList(parameterList)
				.WithBody(body);
			return eventHandlerClass.AddMembers(constructorDeclarationSyntax);
		}

		private ClassDeclarationSyntax AddApplicationObjectField(ClassDeclarationSyntax eventHandlerClass)
		{
			FieldDeclarationSyntax fieldDeclarationSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.FieldDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclaration(Parent.ClassType).WithVariables(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SingletonSeparatedList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclarator(CodeGenFacts.EventHandlerParentApplicationObjectFieldNameSyntax).WithInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.EqualsValueClause(CodeGenFacts.NullLiteralExpression))))).WithModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.TokenList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword)));
			return eventHandlerClass.AddMembers(fieldDeclarationSyntax);
		}
	}

	protected virtual ImmutableArray<VariableSymbol> GlobalVariables => Symbol.GlobalVariables;

	protected internal T Symbol { get; }

	protected abstract TypeSyntax BaseClassType { get; }

	protected abstract TypeSyntax ClassType { get; }

	protected virtual Microsoft.CodeAnalysis.CSharp.SyntaxKind OnClearMethodAccessModifier { get; } = Microsoft.CodeAnalysis.CSharp.SyntaxKind.ProtectedKeyword;

	private DotNetEventHandlerEmitter EventHandler { get; }

	protected ObjectCodeBuilder(T symbol, ModuleBuilder moduleBuilder)
		: base(moduleBuilder)
	{
		Symbol = symbol;
		EventHandler = new DotNetEventHandlerEmitter(symbol, this);
	}

	public override ClassDeclarationSyntax AddGlobalVariables(ClassDeclarationSyntax classDeclaration)
	{
		return ApplicationObjectEmitHelper.EmitGlobalVariables(GlobalVariables, OnClearMethodAccessModifier, classDeclaration, ModuleBuilder.Options.NonDebuggableEmit);
	}

	public override ClassDeclarationSyntax AddMethods(ClassDeclarationSyntax classDeclaration)
	{
		return AddCodeMethods(classDeclaration);
	}

	public override ClassDeclarationSyntax CreateClassDeclaration()
	{
		return ApplicationObjectEmitHelper.CreateClassDeclaration(ClassType.ToString(), BaseClassType);
	}

	public override ClassDeclarationSyntax AddPermissionList(ClassDeclarationSyntax classDeclaration)
	{
		return ApplicationObjectEmitHelper.EmitPermissionsList(Symbol, classDeclaration);
	}

	public override ClassDeclarationSyntax AddProperties(ClassDeclarationSyntax classDeclaration)
	{
		PropertyDeclarationSyntax propertyDeclarationSyntax = ApplicationObjectEmitHelper.EmitObjectNameProperty(Symbol.Name);
		if (propertyDeclarationSyntax != null)
		{
			classDeclaration = classDeclaration.AddMembers(propertyDeclarationSyntax);
		}
		foreach (VariableSymbol globalVariable in GlobalVariables)
		{
			if (globalVariable.GetNavTypeKind() == NavTypeKind.Dialog)
			{
				classDeclaration = AddDialogPropertyDeclaration(classDeclaration, globalVariable);
			}
		}
		return classDeclaration;
	}

	public override ClassDeclarationSyntax AddNonUserCode(ClassDeclarationSyntax classDeclaration)
	{
		return EventHandler.AddEventHandler(classDeclaration);
	}

	protected virtual ClassDeclarationSyntax AddCodeMethods(ClassDeclarationSyntax classDeclaration)
	{
		foreach (EmittedMethod item in ModuleBuilder.GetEmittedMethods(Symbol).Select(BeforeEmitMethod))
		{
			ClassDeclarationSyntax classDeclarationSyntax = classDeclaration;
			MemberDeclarationSyntax[] items = item.Fields.ToArray();
			classDeclaration = classDeclarationSyntax.AddMembers(items).AddMembers(item.MethodDeclaration, item.MethodScope);
		}
		return classDeclaration;
	}

	protected virtual EmittedMethod BeforeEmitMethod(EmittedMethod emittedMethod)
	{
		return emittedMethod;
	}

	private static ClassDeclarationSyntax AddDialogPropertyDeclaration(ClassDeclarationSyntax classDeclaration, VariableSymbol globalVariable)
	{
		MemberAccessExpressionSyntax memberAccessExpressionSyntax = CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(ApplicationObjectEmitHelper.GetFieldName(globalVariable)));
		BlockSyntax body = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IfStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.EqualsExpression, memberAccessExpressionSyntax, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NullLiteralExpression)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, memberAccessExpressionSyntax, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.RuntimeDialogType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression())), null))))), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(memberAccessExpressionSyntax));
		BlockSyntax body2 = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, memberAccessExpressionSyntax, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("value"))));
		classDeclaration = classDeclaration.AddMembers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PropertyDeclaration(CodeGenFacts.RuntimeDialogType, globalVariable.MetadataName).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword)).AddAccessorListAccessors(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AccessorDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration, body), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AccessorDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SetAccessorDeclaration, body2)));
		return classDeclaration;
	}
}
