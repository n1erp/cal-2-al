using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class DialogClassTypeSymbol : LanguageClassTypeSymbol
{
	private static class DialogCallEmitter
	{
		public static ExpressionSyntax EmitCallWithAutomationAndParams(ExpressionSyntax expression, CodeGenerator generator, BoundCall call)
		{
			PooledList<ArgumentSyntax> instance = PooledList<ArgumentSyntax>.GetInstance();
			try
			{
				instance.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenerator.CreateAutomationId(call)));
				generator.EmitArgumentsForBuiltInMethodWithSpecialJokerHandling(call, instance);
				return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(expression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(call.Method.MetadataName)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SeparatedList(instance)));
			}
			finally
			{
				instance.Free();
			}
		}
	}

	internal class MessageStaticBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			return DialogCallEmitter.EmitCallWithAutomationAndParams(CodeGenFacts.NavDialogNameSyntax, generator, call);
		}

		public MessageStaticBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	internal class ErrorStaticBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			return DialogCallEmitter.EmitCallWithAutomationAndParams(CodeGenFacts.NavDialogNameSyntax, generator, call);
		}

		public ErrorStaticBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	internal class ConfirmStaticBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			return DialogCallEmitter.EmitCallWithAutomationAndParams(CodeGenFacts.NavDialogNameSyntax, generator, call);
		}

		public ConfirmStaticBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	internal class OpenBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			DebugAssertHelper.Assert(call.Arguments.Length >= 1);
			ArgumentSyntax argumentSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenerator.CreateAutomationId(call));
			ArgumentSyntax argumentSyntax2 = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(generator.EmitExpression(call.Arguments[0]));
			ArgumentSyntax[] items = (from arg in call.Arguments.Skip(1)
				select ToDelegateArgument(generator.EmitExpression(arg))).ToArray();
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(generator.EmitExpression(call.ReceiverOpt), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(call.Method.MetadataName))).AddArgumentListArguments(argumentSyntax, argumentSyntax2).AddArgumentListArguments(items);
		}

		private static ArgumentSyntax ToDelegateArgument(ExpressionSyntax expression)
		{
			ReturnStatementSyntax returnStatementSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(expression);
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AnonymousMethodExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(returnStatementSyntax)));
		}

		public OpenBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	internal class StrMenuStaticBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(CodeGenFacts.NavDialogNameSyntax, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(call.Method.MetadataName))).AddArgumentListArguments(call.Arguments.Select((BoundExpression arg) => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(generator.EmitExpression(arg))).ToArray()).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenerator.CreateAutomationId(call)));
		}

		public StrMenuStaticBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	private static readonly Lazy<DialogClassTypeSymbol> instance = new Lazy<DialogClassTypeSymbol>(() => new DialogClassTypeSymbol("Dialog", isAutoWith: true, (Symbol containingSymbol) => new MethodSymbol[9]
	{
		new ErrorStaticBuiltInMethodTypeSymbol(containingSymbol, "Error", SymbolCompilationScope.DefinedByOwner, isStatic: true, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Message", 0),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Value", 1, isVar: false, isOptional: true, isParams: true)
		}),
		new ErrorStaticBuiltInMethodTypeSymbol(containingSymbol, "Error", SymbolCompilationScope.Internal, isStatic: true, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.ErrorInfoType, "Message", 0)
		}, VersionCompatibility.Parse("3.0")),
		new MessageStaticBuiltInMethodTypeSymbol(containingSymbol, "Message", SymbolCompilationScope.DefinedByOwner, isStatic: true, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "String", 0),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Value", 1, isVar: false, isOptional: true, isParams: true)
		}),
		new ConfirmStaticBuiltInMethodTypeSymbol(containingSymbol, "Confirm", SymbolCompilationScope.DefinedByOwner, isStatic: true, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "String", 0),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "Default", 1, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Value1", 2, isVar: false, isOptional: true, isParams: true)
		}),
		new StrMenuStaticBuiltInMethodTypeSymbol(containingSymbol, "StrMenu", SymbolCompilationScope.DefinedByOwner, isStatic: true, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "OptionMembers", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "DefaultNumber", 1, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Instruction", 2, isVar: false, isOptional: true)
		}),
		new OpenBuiltInMethodTypeSymbol(containingSymbol, "Open", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "String", 0),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Variable1", 1, isVar: true, isOptional: true, isParams: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Update", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Value", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Close", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "HideSubsequentDialogs", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "HideSubsequentDialogs", 0, isVar: false, isOptional: true)
		})
	}), isThreadSafe: true);

	internal static DialogClassTypeSymbol Instance => instance.Value;

	private DialogClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
