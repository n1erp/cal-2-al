using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class OutStreamClassTypeSymbol : LanguageClassTypeSymbol
{
	private static class WriteCallEmitter
	{
		public static ExpressionSyntax EmitWriteCall(CodeGenerator generator, BoundCall call)
		{
			BuiltInMethodTypeSymbol builtInMethodTypeSymbol = call.Method as BuiltInMethodTypeSymbol;
			DebugAssertHelper.Assert(builtInMethodTypeSymbol != null);
			InvocationExpressionSyntax invocationExpressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(builtInMethodTypeSymbol.StaticClassName), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(call.Method.MetadataName)));
			invocationExpressionSyntax = invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(generator.EmitExpression(call.ReceiverOpt)));
			if (builtInMethodTypeSymbol.AddDataError)
			{
				invocationExpressionSyntax = invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(call.IsStatement ? CodeGenFacts.DataErrorThrowErrorExpression : CodeGenFacts.DataErrorTrapErrorExpression));
			}
			invocationExpressionSyntax = generator.EmitArguments(invocationExpressionSyntax, call);
			if (call.Arguments.Length == 1)
			{
				invocationExpressionSyntax = invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenFacts.ALStreamDefaultLengthExpression));
			}
			return invocationExpressionSyntax;
		}
	}

	internal class WriteTextBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			return WriteCallEmitter.EmitWriteCall(generator, call);
		}

		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public WriteTextBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	internal class WriteBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			return WriteCallEmitter.EmitWriteCall(generator, call);
		}

		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public WriteBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	private static readonly Lazy<OutStreamClassTypeSymbol> instance = new Lazy<OutStreamClassTypeSymbol>(() => new OutStreamClassTypeSymbol("OutStream", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[24]
	{
		new WriteBuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", "ALWriteNavValue", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.VariantType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}, VersionCompatibility.Parse("2.0")),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.ByteType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.CharType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BigIntegerType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DecimalType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.GuidType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.CodeType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.LabelType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}, addCompilationTarget: false, VersionCompatibility.Parse("3.0")),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TextConstType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}, addCompilationTarget: false, VersionCompatibility.Parse("3.0")),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BigTextType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DateType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TimeType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DateTimeType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DateFormulaType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DurationType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.OptionType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.RecordIdType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", "ALWriteString", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new WriteTextBuiltInMethodTypeSymbol(containingSymbol, "WriteText", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Text", 0, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		})
	}), isThreadSafe: true);

	internal static OutStreamClassTypeSymbol Instance => instance.Value;

	private OutStreamClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
