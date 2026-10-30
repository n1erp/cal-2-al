using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class PageClassTypeSymbol : LanguageClassTypeSymbol
{
	internal class RunModalStaticBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			BuiltInMethodTypeSymbol obj = call.Method as BuiltInMethodTypeSymbol;
			DebugAssertHelper.Assert(obj != null);
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(obj.StaticClassName), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(call.Method.MetadataName))).AddArgumentListArguments(EmitRunModalArguments(generator, call));
		}

		private static ArgumentSyntax[] EmitRunModalArguments(CodeGenerator generator, BoundCall call)
		{
			int parameterCount = call.Method.ParameterCount;
			DebugAssertHelper.Assert(parameterCount == 3, "The Page.RunModal signature has changed. Please update the emitter.");
			ArgumentSyntax[] array = new ArgumentSyntax[2 + call.Arguments.Length];
			bool value = IsOnLookupTrigger(generator);
			array[0] = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(value));
			array[1] = (call.IsStatement ? Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenFacts.FalseLiteralExpression) : Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenFacts.TrueLiteralExpression));
			for (int i = 0; i < parameterCount && i < call.Arguments.Length; i++)
			{
				ArgumentSyntax argumentSyntax;
				if (i < 2)
				{
					argumentSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(generator.EmitExpression(call.Arguments[i]));
				}
				else
				{
					BoundExpression boundExpression = call.Arguments[2].SkipConversion();
					argumentSyntax = ((boundExpression is BoundFieldAccess boundFieldAccess) ? Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(boundFieldAccess.FieldSymbol.Id)) : Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(generator.EmitExpression(boundExpression)));
				}
				array[2 + i] = argumentSyntax;
			}
			return array;
		}

		private static bool IsOnLookupTrigger(CodeGenerator generator)
		{
			SourceTriggerSymbol sourceTriggerSymbol = ((MethodCodeGenerator)generator).Method as SourceTriggerSymbol;
			if (sourceTriggerSymbol?.TriggerInfo != null)
			{
				if (sourceTriggerSymbol.TriggerInfo.Kind != TriggerTypeKind.ControlOnLookup && sourceTriggerSymbol.TriggerInfo.Kind != TriggerTypeKind.ControlOnDrillDown)
				{
					return sourceTriggerSymbol.TriggerInfo.Kind == TriggerTypeKind.FieldOnLookup;
				}
				return true;
			}
			return false;
		}

		public RunModalStaticBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	internal class RunStaticBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			BuiltInMethodTypeSymbol obj = call.Method as BuiltInMethodTypeSymbol;
			DebugAssertHelper.Assert(obj != null);
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(obj.StaticClassName), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(call.Method.MetadataName))).AddArgumentListArguments(EmitRunArguments(generator, call));
		}

		private static ArgumentSyntax[] EmitRunArguments(CodeGenerator generator, BoundCall call)
		{
			int parameterCount = call.Method.ParameterCount;
			DebugAssertHelper.Assert(parameterCount == 3, "The Page.Run signature has changed. Please update the emitter.");
			ArgumentSyntax[] array = new ArgumentSyntax[call.Arguments.Length];
			for (int i = 0; i < parameterCount && i < call.Arguments.Length; i++)
			{
				ArgumentSyntax argumentSyntax;
				if (i < 2)
				{
					argumentSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(generator.EmitExpression(call.Arguments[i]));
				}
				else
				{
					BoundExpression boundExpression = call.Arguments[2].SkipConversion();
					argumentSyntax = ((boundExpression is BoundFieldAccess boundFieldAccess) ? Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(boundFieldAccess.FieldSymbol.Id)) : Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(generator.EmitExpression(boundExpression)));
				}
				array[i] = argumentSyntax;
			}
			return array;
		}

		public RunStaticBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	private static readonly Lazy<PageClassTypeSymbol> instance = new Lazy<PageClassTypeSymbol>(() => new PageClassTypeSymbol("Page", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[19]
	{
		new RunModalStaticBuiltInMethodTypeSymbol(containingSymbol, "RunModal", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavForm", "RunModal", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.ActionType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Record", 1, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 2, isVar: false, isOptional: true, isParams: false, isMember: false, isMemberReference: true)
		}),
		new RunModalStaticBuiltInMethodTypeSymbol(containingSymbol, "RunModal", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavForm", "RunModal", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.ActionType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Record", 1),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "FieldNo", 2)
		}),
		new RunModalStaticBuiltInMethodTypeSymbol(containingSymbol, "RunModal", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavForm", "RunModal", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.ActionType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Record", 1),
			new BuiltInParameterSymbol(method, NavCorLib.FieldRefType, "FieldRef", 2)
		}, VersionCompatibility.Parse("3.0")),
		new RunStaticBuiltInMethodTypeSymbol(containingSymbol, "Run", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavForm", "Run", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Record", 1, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 2, isVar: false, isOptional: true, isParams: false, isMember: false, isMemberReference: true)
		}),
		new RunStaticBuiltInMethodTypeSymbol(containingSymbol, "Run", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavForm", "Run", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Record", 1),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "FieldNo", 2)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Editable", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "Editable", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "NewEditable", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Caption", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "PageCaption", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "NewCaption", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "LookupMode", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "LookupMode", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "NewLookupMode", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ObjectId", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ObjectID", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "UseNames", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SaveRecord", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "SaveRecord", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Update", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "Update", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "SaveRecord", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetRecord", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "GetRecord", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Record", 0, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetRecord", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "SetRecord", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Record", 0, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetTableView", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "SetTableView", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Record", 0, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetSelectionFilter", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "SetSelectionFilter", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Record", 0, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Activate", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "Activate", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "Refresh", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Close", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "Close", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "RunModal", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "RunModal", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.ActionType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, null, addCompilationTarget: false, null, suppressTarget: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Run", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "Run", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, null, addCompilationTarget: false, null, suppressTarget: true)
	}), isThreadSafe: true);

	internal static PageClassTypeSymbol Instance => instance.Value;

	private PageClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.ApplicationObject, NavTypeKind.Page, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
