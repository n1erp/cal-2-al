using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class SystemClassTypeSymbol : LanguageClassTypeSymbol
{
	internal class IsNullStaticBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			return CodeGenHelper.MemberAccessExpression(generator.EmitExpression(call.Arguments[0]), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(call.Method.MetadataName));
		}

		public IsNullStaticBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	internal class GetDotNetTypeStaticBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			DebugAssertHelper.Assert(call.Arguments.Length == 1);
			ExpressionSyntax expression = EmitArgumentConversion(generator, call);
			return CodeGenFacts.NavDotNetGetDotNetTypeCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
		}

		private static ExpressionSyntax EmitArgumentConversion(CodeGenerator generator, BoundCall call)
		{
			BoundExpression boundExpression = call.Arguments[0];
			if (boundExpression.Kind != BoundKind.Conversion)
			{
				return generator.EmitExpression(boundExpression);
			}
			BoundConversion boundConversion = (BoundConversion)boundExpression;
			if (ConversionDotNet.IsStronglyTypedDotNetSymbol(boundConversion.Operand.Type))
			{
				ExpressionSyntax emittedDotNetValueAccess = generator.EmitExpression(boundConversion.Operand);
				return CodeGenerator.EmitGetUnderlyingValue((DotNetTypeSymbol)boundConversion.Operand.Type, emittedDotNetValueAccess);
			}
			return generator.EmitExpression(boundExpression.SkipConversion());
		}

		public GetDotNetTypeStaticBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	internal class ArrayLenStaticBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			InvocationExpressionSyntax invocationExpressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(generator.EmitExpression(call.Arguments[0]), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(call.Method.MetadataName)));
			if (call.Arguments.Length > 1)
			{
				invocationExpressionSyntax = invocationExpressionSyntax.AddArgumentListArguments(generator.BuildArrayIndex(call.Arguments[1]));
			}
			return invocationExpressionSyntax;
		}

		public ArrayLenStaticBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	internal class ClearStaticBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			DebugAssertHelper.Assert(call.Arguments.Length == 1);
			BoundExpression boundExpression = call.Arguments[0].SkipConversion();
			switch (boundExpression.Kind)
			{
			case BoundKind.ArrayAccess:
				return EmitArrayIndexClear(generator, (BoundArrayAccess)boundExpression);
			case BoundKind.FieldAccess:
				return EmitClearFieldValue(generator, (BoundFieldAccess)boundExpression);
			default:
			{
				if (boundExpression.Type.IsArray() || boundExpression.Type.NavTypeKind.UseInstanceMethodToClear())
				{
					return EmitInstanceClear(generator, boundExpression);
				}
				if (boundExpression.Type.IsUserType())
				{
					switch (boundExpression.Type.NavTypeKind)
					{
					case NavTypeKind.Record:
					case NavTypeKind.Page:
					case NavTypeKind.Report:
					case NavTypeKind.XmlPort:
					case NavTypeKind.Query:
						return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(generator.EmitExpression(boundExpression, suppressTarget: true), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(call.Method.Name)));
					case NavTypeKind.TestRequestPage:
					case NavTypeKind.TestPage:
					case NavTypeKind.Codeunit:
						return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(generator.EmitExpression(boundExpression, suppressTarget: true), CodeGenFacts.ClearReferenceMethodNameSyntax));
					default:
						throw ExceptionUtilities.Unreachable;
					}
				}
				ExpressionSyntax expressionSyntax = CodeGenerator.EmitFieldInitializer(boundExpression.Type);
				if (expressionSyntax != null)
				{
					return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, generator.EmitExpression(boundExpression), expressionSyntax);
				}
				return CodeGenFacts.ALSystemVariableClearCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(generator.EmitExpression(boundExpression)));
			}
			}
		}

		private static ExpressionSyntax EmitClearFieldValue(CodeGenerator generator, BoundFieldAccess arg)
		{
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(generator.EmitFieldAccessReceiver(arg), CodeGenFacts.ClearFieldValueMethodNameSyntax)).AddExtensionAndFieldArguments(arg);
		}

		private static ExpressionSyntax EmitInstanceClear(CodeGenerator generator, BoundExpression arg)
		{
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(generator.EmitExpression(arg), CodeGenFacts.ClearMethodNameSyntax));
		}

		private static ExpressionSyntax EmitArrayIndexClear(CodeGenerator generator, BoundArrayAccess arg)
		{
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(generator.EmitExpression(arg.Expression), CodeGenFacts.ClearMethodNameSyntax)).AddArgumentListArguments(arg.Indices.Select(generator.BuildArrayIndex).ToArray());
		}

		public ClearStaticBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	internal class IsNullGuidStaticBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			DebugAssertHelper.Assert(call.Arguments.Length == 1);
			MemberAccessExpressionSyntax expression = CodeGenHelper.MemberAccessExpression(generator.EmitExpression(call.Arguments[0]), CodeGenFacts.RuntimeEqualsMethodNameSyntax);
			ArgumentSyntax argumentSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenFacts.GuidEmptyExpression);
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(expression).AddArgumentListArguments(argumentSyntax);
		}

		public IsNullGuidStaticBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	internal class HyperlinkStaticBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			DebugAssertHelper.Assert(call.Arguments.Length == 1);
			BuiltInMethodTypeSymbol obj = call.Method as BuiltInMethodTypeSymbol;
			DebugAssertHelper.Assert(obj != null);
			MemberAccessExpressionSyntax expression = CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(obj.StaticClassName), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(call.Method.MetadataName));
			ArgumentSyntax argumentSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(generator.EmitExpression(call.Arguments[0]));
			ArgumentSyntax argumentSyntax2 = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenFacts.GuidEmptyExpression);
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(expression).AddArgumentListArguments(argumentSyntax, argumentSyntax2);
		}

		public HyperlinkStaticBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	internal class FormatStaticBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			return generator.EmitBuiltInMethodCallWithJokerArgumentHandling(call);
		}

		public FormatStaticBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	internal class EvaluateStaticBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			DebugAssertHelper.Assert(call.Arguments.Length > 0, "Evaluate cannot be called without parameters");
			if (call.Arguments[0].SkipConversion().Type.Equals(NavCorLib.FieldRefType))
			{
				return EmitEvaluateFieldRefExpression(generator, call);
			}
			return generator.EmitBuiltInMethodOrPropertyCall(call);
		}

		private ExpressionSyntax EmitEvaluateFieldRefExpression(CodeGenerator generator, BoundCall call)
		{
			PooledList<ArgumentSyntax> instance = PooledList<ArgumentSyntax>.GetInstance();
			try
			{
				BoundConversion boundConversion = (BoundConversion)call.Arguments[0];
				instance.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(EmitFieldRefByRefArgument(generator, boundConversion.Operand)));
				for (int i = 1; i < call.Arguments.Length; i++)
				{
					ParameterSymbol parameterSymbol = call.EffectiveParameters.Parameters[i];
					instance.Add(generator.EmitArgument(parameterSymbol, call.Arguments[i]));
				}
				BuiltInMethodTypeSymbol builtInMethodTypeSymbol = (BuiltInMethodTypeSymbol)call.Method;
				MemberAccessExpressionSyntax memberAccess = generator.EmitBuiltInMemberAccessExpression(call, builtInMethodTypeSymbol);
				return generator.EmitBuiltInMethodInvocation(call, builtInMethodTypeSymbol, memberAccess).AddArgumentListArguments(instance.ToArray());
			}
			finally
			{
				instance.Free();
			}
		}

		private ExpressionSyntax EmitFieldRefByRefArgument(CodeGenerator generator, BoundExpression fieldRefExpression)
		{
			MemberAccessExpressionSyntax memberAccessExpressionSyntax = CodeGenHelper.MemberAccessExpression(generator.EmitExpression(fieldRefExpression), CodeGenFacts.ALValuePropertyNameSyntax);
			AssignmentExpressionSyntax body = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, memberAccessExpressionSyntax, CodeGenFacts.ByRefSetValueNameSyntax);
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.GenericName(CodeGenFacts.ByRefTypeIdentifier).AddTypeArgumentListArguments(CodeGenFacts.NavValueType)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedLambdaExpression(memberAccessExpressionSyntax)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SimpleLambdaExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(CodeGenFacts.ByRefSetValueIdentifier), body)));
		}

		public EvaluateStaticBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	private static readonly Lazy<SystemClassTypeSymbol> instance = new Lazy<SystemClassTypeSymbol>(() => new SystemClassTypeSymbol("System", isAutoWith: true, (Symbol containingSymbol) => new MethodSymbol[71]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "Round", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemNumeric", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.DecimalType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DecimalType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.DecimalType, "Precision", 1, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Direction", 2, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Abs", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemNumeric", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.DecimalType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DecimalType, "Number", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Power", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemNumeric", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.DecimalType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DecimalType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.DecimalType, "Power", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Randomize", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemNumeric", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Seed", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Random", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemNumeric", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "MaxNumber", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Today", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemDate", null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.DateType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Time", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemDate", null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.TimeType),
		new BuiltInMethodTypeSymbol(containingSymbol, "WorkDate", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemDate", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.DateType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DateType, "NewDate", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "CalcDate", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemDate", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.DateType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "DateExpression", 0),
			new BuiltInParameterSymbol(method, NavCorLib.DateType, "Date", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "CalcDate", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemDate", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.DateType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DateFormulaType, "DateExpression", 0),
			new BuiltInParameterSymbol(method, NavCorLib.DateType, "Date", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "NormalDate", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemDate", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.DateType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DateType, "Date", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ClosingDate", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemDate", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.DateType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DateType, "Date", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Date2DMY", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemDate", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DateType, "Date", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Value", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Date2DWY", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemDate", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DateType, "Date", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Value", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "DMY2Date", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemDate", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.DateType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Day", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Month", 1, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Year", 2, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "DWY2Date", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemDate", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.DateType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "WeekDay", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Week", 1, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Year", 2, isVar: false, isOptional: true)
		}),
		new HyperlinkStaticBuiltInMethodTypeSymbol(containingSymbol, "Hyperlink", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemOperatingSystem", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "URL", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Sleep", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavSession", "Sleep", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Duration", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GuiAllowed", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemOperatingSystem", null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType),
		new ArrayLenStaticBuiltInMethodTypeSymbol(containingSymbol, "ArrayLen", SymbolCompilationScope.DefinedByOwner, isStatic: true, null, "ArrayLen", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Array", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: false, typeFromFirstArgument: false, isArray: true),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Dimension", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "CompressArray", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemArray", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "StringArray", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: false, typeFromFirstArgument: false, isArray: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "CopyArray", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemArray", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[4]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "NewArray", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: false, typeFromFirstArgument: false, isArray: true),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Array", 1, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: false, typeFromFirstArgument: true, isArray: true),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Position", 2),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 3, isVar: false, isOptional: true)
		}),
		new ClearStaticBuiltInMethodTypeSymbol(containingSymbol, "Clear", SymbolCompilationScope.DefinedByOwner, isStatic: true, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Variable", 0, isVar: true, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: true)
		}),
		new ClearStaticBuiltInMethodTypeSymbol(containingSymbol, "Clear", SymbolCompilationScope.DefinedByOwner, isStatic: true, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Variable", 0, isVar: true, isOptional: false, isParams: false, isMember: false, isMemberReference: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ClearAll", SymbolCompilationScope.DefinedByOwner, isStatic: true, null, "OnClear", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new EvaluateStaticBuiltInMethodTypeSymbol(containingSymbol, "Evaluate", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemVariable", "ALEvaluate", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Variable", 0, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "String", 1),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 2, isVar: false, isOptional: true)
		}),
		new FormatStaticBuiltInMethodTypeSymbol(containingSymbol, "Format", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavFormatEvaluateHelper", "Format", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "FormatNumber", 2, isVar: false, isOptional: true)
		}),
		new FormatStaticBuiltInMethodTypeSymbol(containingSymbol, "Format", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavFormatEvaluateHelper", "Format", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FormatString", 2)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "WindowsLanguage", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemLanguage", null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType),
		new BuiltInMethodTypeSymbol(containingSymbol, "GlobalLanguage", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemLanguage", null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "NewLanguageID", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "DaTi2Variant", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemDate", null, isProperty: false, addDataError: false, addThis: true, isLocal: false, isObsolete: false, NavCorLib.VariantType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DateType, "Date", 0),
			new BuiltInParameterSymbol(method, NavCorLib.TimeType, "Time", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Variant2Date", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemDate", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.DateType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.VariantType, "Variant", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Variant2Time", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemDate", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.TimeType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.VariantType, "Variant", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "CopyStream", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemVariable", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.OutStreamType, "OutStream", 0),
			new BuiltInParameterSymbol(method, NavCorLib.InStreamType, "InStream", 1),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "BytesToRead", 2, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "CreateGuid", SymbolCompilationScope.DefinedByOwner, isStatic: true, "Guid", "NewGuid", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.GuidType),
		new BuiltInMethodTypeSymbol(containingSymbol, "CurrentDateTime", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemDate", null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.DateTimeType),
		new IsNullGuidStaticBuiltInMethodTypeSymbol(containingSymbol, "IsNullGuid", SymbolCompilationScope.DefinedByOwner, isStatic: true, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.GuidType, "Guid", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "RoundDateTime", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemDate", "ALRoundDateTime", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.DateTimeType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DateTimeType, "Datetime", 0),
			new BuiltInParameterSymbol(method, NavCorLib.BigIntegerType, "Precision", 1, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Direction", 2, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "CreateDateTime", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemDate", "ALCreateDateTime", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.DateTimeType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DateType, "Date", 0),
			new BuiltInParameterSymbol(method, NavCorLib.TimeType, "Time", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "DT2Date", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemDate", "ALDT2Date", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.DateType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DateTimeType, "Datetime", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "DT2Time", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemDate", "ALDT2Time", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.TimeType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DateTimeType, "Datetime", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetLastErrorText", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemErrorHandling", null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetLastErrorCode", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemErrorHandling", null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetLastErrorObject", SymbolCompilationScope.Internal, isStatic: true, "ALSystemErrorHandling", null, isProperty: false, addDataError: false, addThis: true, isLocal: false, isObsolete: false, NavCorLib.DotNetType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, null, addCompilationTarget: false, VersionCompatibility.Parse("2.0")),
		new BuiltInMethodTypeSymbol(containingSymbol, "ClearLastError", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemErrorHandling", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "ApplicationPath", SymbolCompilationScope.Internal, isStatic: true, "ALSystemOperatingSystem", null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "TemporaryPath", SymbolCompilationScope.Internal, isStatic: true, "ALSystemOperatingSystem", null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "IsServiceTier", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavEnvironment", "IsServiceTier", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: true, NavCorLib.BooleanType),
		new BuiltInMethodTypeSymbol(containingSymbol, "ExportObjects", SymbolCompilationScope.Internal, isStatic: true, "ALSystemObject", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: true, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FileName", 0),
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "ObjectRecord", 1, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Format", 2, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ImportObjects", SymbolCompilationScope.Internal, isStatic: true, "ALSystemObject", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: true, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FileName", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Format", 1, isVar: false, isOptional: true)
		}),
		new IsNullStaticBuiltInMethodTypeSymbol(containingSymbol, "IsNull", SymbolCompilationScope.Internal, isStatic: true, null, "IsNull", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DotNetType, "DotNet", 0)
		}, VersionCompatibility.Parse("2.0")),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetLastErrorCallStack", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemErrorHandling", null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "CodeCoverageLog", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALCodeCoverage", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "NewIsActive", 0, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "MultiSession", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "CodeCoverageInclude", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALCodeCoverage", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "ObjectRecord", 0, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "CodeCoverageRefresh", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALCodeCoverage", "ALCodeCoverageRefreshTable", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "CodeCoverageLoad", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALCodeCoverage", "ALCodeCoverageLoadFromTable", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new GetDotNetTypeStaticBuiltInMethodTypeSymbol(containingSymbol, "GetDotNetType", SymbolCompilationScope.Internal, isStatic: true, "NavDotNet", "GetDotNetType", isProperty: false, addDataError: false, addThis: true, isLocal: false, isObsolete: false, NavCorLib.DotNetType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Expression", 0)
		}, VersionCompatibility.Parse("2.0")),
		new BuiltInMethodTypeSymbol(containingSymbol, "CanLoadType", SymbolCompilationScope.Internal, isStatic: true, "NavDotNet", "CanLoadType", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DotNetType, "DotNet", 0)
		}, addCompilationTarget: false, VersionCompatibility.Parse("2.0")),
		new BuiltInMethodTypeSymbol(containingSymbol, "CaptionClassTranslate", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemObject", "ALCaptionClassTranslate", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "CaptionClassText", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetUrl", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemOperatingSystem", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[6]
		{
			new BuiltInParameterSymbol(method, NavCorLib.ClientTypeType, "ClientType", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Company", 1, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.ObjectTypeType, "ObjectType", 2, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "ObjectId", 3, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Record", 4, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "UseFilters", 5, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetUrl", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemOperatingSystem", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[6]
		{
			new BuiltInParameterSymbol(method, NavCorLib.ClientTypeType, "ClientType", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Company", 1),
			new BuiltInParameterSymbol(method, NavCorLib.ObjectTypeType, "ObjectType", 2),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "ObjectId", 3),
			new BuiltInParameterSymbol(method, NavCorLib.RecordRefType, "RecordRef", 4),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "UseFilters", 5, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Encrypt", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemEncryption", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "PlainTextString", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Decrypt", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemEncryption", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "EncryptedString", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ExportEncryptionKey", SymbolCompilationScope.Internal, isStatic: true, "ALSystemEncryption", "ALExportKey", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Password", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ImportEncryptionKey", SymbolCompilationScope.Internal, isStatic: true, "ALSystemEncryption", "ALImportKey", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Path", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Password", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "CreateEncryptionKey", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemEncryption", "ALCreateKey", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "DeleteEncryptionKey", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemEncryption", "ALDeleteKey", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "EncryptionKeyExists", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemEncryption", "ALKeyExists", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType),
		new BuiltInMethodTypeSymbol(containingSymbol, "EncryptionEnabled", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemEncryption", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetDocumentUrl", SymbolCompilationScope.Internal, isStatic: true, "NavMedia", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.GuidType, "ID", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ImportStreamWithUrlAccess", SymbolCompilationScope.Internal, isStatic: true, "NavMedia", "ALImportWithUrlAccess", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.GuidType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.InStreamType, "InStream", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Filename", 1),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "MinutesToExpire", 2, isVar: false, isOptional: true)
		})
	}), isThreadSafe: true);

	internal static SystemClassTypeSymbol Instance => instance.Value;

	private SystemClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
