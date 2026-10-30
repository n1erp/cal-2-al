using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Translation;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal class CodeGenerator
{
	private static readonly Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax IdentityConversion = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("Identity"));

	private static readonly Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax SpecialConversion = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("Special"));

	private static readonly GenericNameSyntax NavIndirectValueToNavValue_NavDotNetName = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.GenericName(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("NavIndirectValueToNavValue"), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.TypeArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SingletonSeparatedList(CodeGenFacts.NavDotNetType)));

	private static Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax ALCompilerVariantToNavDotNet = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(CodeGenFacts.ALCompilerClassNameSyntax, NavIndirectValueToNavValue_NavDotNetName));

	private static readonly Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax[,] DotNetToAlConversions;

	private static readonly Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax[,] AlToDotNetConversions;

	private static readonly Microsoft.CodeAnalysis.CSharp.SyntaxKind[] DefaultFieldDeclarationSyntaxKinds;

	private static readonly Microsoft.CodeAnalysis.CSharp.SyntaxKind[] TextConstFieldDeclarationSyntaxKinds;

	private static readonly Microsoft.CodeAnalysis.CSharp.SyntaxKind[] DefaultMethodLocalFieldDeclarationSyntaxKinds;

	private static readonly Microsoft.CodeAnalysis.CSharp.SyntaxKind[] TextConstMethodLocalFieldDeclarationSyntaxKinds;

	private CompilationOptions CompilationOptions { get; }

	protected EmitOptions EmitOptions { get; }

	protected bool EmitBaseParent { get; set; }

	private static int DotNetTypeToIndex(DotNetSpecialType specialType)
	{
		return specialType switch
		{
			DotNetSpecialType.None => 0, 
			DotNetSpecialType.System_Object => 1, 
			DotNetSpecialType.System_Enum => 2, 
			DotNetSpecialType.System_Void => 3, 
			DotNetSpecialType.System_Boolean => 4, 
			DotNetSpecialType.System_Char => 5, 
			DotNetSpecialType.System_SByte => 6, 
			DotNetSpecialType.System_Byte => 7, 
			DotNetSpecialType.System_Int16 => 8, 
			DotNetSpecialType.System_UInt16 => 9, 
			DotNetSpecialType.System_Int32 => 10, 
			DotNetSpecialType.System_UInt32 => 11, 
			DotNetSpecialType.System_Int64 => 12, 
			DotNetSpecialType.System_UInt64 => 13, 
			DotNetSpecialType.System_Decimal => 14, 
			DotNetSpecialType.System_Single => 15, 
			DotNetSpecialType.System_Double => 16, 
			DotNetSpecialType.System_String => 17, 
			DotNetSpecialType.System_TimeSpan => 18, 
			DotNetSpecialType.System_DateTime => 19, 
			DotNetSpecialType.System_Guid => 20, 
			DotNetSpecialType.System_Stream => 21, 
			_ => -1, 
		};
	}

	private static int AlTypeToIndex(NavTypeKind Kind)
	{
		return Kind switch
		{
			NavTypeKind.Boolean => 0, 
			NavTypeKind.Byte => 1, 
			NavTypeKind.Char => 2, 
			NavTypeKind.Option => 3, 
			NavTypeKind.Integer => 4, 
			NavTypeKind.BigInteger => 5, 
			NavTypeKind.Decimal => 6, 
			NavTypeKind.Duration => 7, 
			NavTypeKind.Code => 8, 
			NavTypeKind.Text => 9, 
			NavTypeKind.Date => 10, 
			NavTypeKind.Time => 11, 
			NavTypeKind.DateTime => 12, 
			NavTypeKind.InStream => 13, 
			NavTypeKind.OutStream => 14, 
			NavTypeKind.Guid => 15, 
			NavTypeKind.Variant => 16, 
			NavTypeKind.String => 17, 
			NavTypeKind.Label => 18, 
			NavTypeKind.BigText => 19, 
			_ => -1, 
		};
	}

	internal static TypeSyntax GetClrValueType(DotNetSpecialType specialType)
	{
		switch (specialType)
		{
		case DotNetSpecialType.System_Object:
			return CodeGenFacts.ClrObjectType;
		case DotNetSpecialType.System_Enum:
			return null;
		case DotNetSpecialType.System_Void:
			return CodeGenFacts.NavVoidType;
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
			return CodeGenFacts.RuntimeDecimalType;
		case DotNetSpecialType.System_Single:
			return CodeGenFacts.ClrSingleType;
		case DotNetSpecialType.System_Double:
			return CodeGenFacts.ClrDoubleType;
		case DotNetSpecialType.System_String:
			return CodeGenFacts.ClrStringType;
		case DotNetSpecialType.System_TimeSpan:
			return CodeGenFacts.NavDurationType;
		case DotNetSpecialType.System_DateTime:
			return CodeGenFacts.NavDateTimeType;
		case DotNetSpecialType.System_Guid:
			return CodeGenFacts.ClrGuidType;
		case DotNetSpecialType.None:
		case DotNetSpecialType.System_Stream:
		case DotNetSpecialType.System_Nullable:
			return null;
		default:
			throw ExceptionUtilities.UnexpectedValue(specialType);
		}
	}

	private static TypeSyntax GetDotNetByRefTypeArgument(DotNetSpecialType specialType)
	{
		switch (specialType)
		{
		case DotNetSpecialType.System_Object:
			return CodeGenFacts.ClrObjectType;
		case DotNetSpecialType.System_Void:
			return CodeGenFacts.NavVoidType;
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
			return CodeGenFacts.RuntimeDecimalType;
		case DotNetSpecialType.System_Single:
			return CodeGenFacts.ClrSingleType;
		case DotNetSpecialType.System_Double:
			return CodeGenFacts.ClrDoubleType;
		case DotNetSpecialType.System_String:
			return CodeGenFacts.ClrStringType;
		case DotNetSpecialType.System_TimeSpan:
			return CodeGenFacts.NavDurationType;
		case DotNetSpecialType.System_DateTime:
			return CodeGenFacts.NavDateTimeType;
		case DotNetSpecialType.System_Guid:
			return CodeGenFacts.ClrGuidType;
		case DotNetSpecialType.None:
		case DotNetSpecialType.System_Enum:
		case DotNetSpecialType.System_Stream:
		case DotNetSpecialType.System_Nullable:
			return CodeGenFacts.NavDotNetType;
		default:
			throw ExceptionUtilities.UnexpectedValue(specialType);
		}
	}

	private static NavTypeKind GetNavIndirectValueTargetKind(DotNetSpecialType toDotNetSpecialType)
	{
		return toDotNetSpecialType switch
		{
			DotNetSpecialType.System_TimeSpan => NavTypeKind.Duration, 
			DotNetSpecialType.System_DateTime => NavTypeKind.DateTime, 
			_ => NavTypeKind.DotNet, 
		};
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitBoxingConversion(BoundExpression argumentExpression)
	{
		if (argumentExpression.Kind != BoundKind.Conversion)
		{
			return EmitExpression(argumentExpression);
		}
		BoundConversion boundConversion = (BoundConversion)argumentExpression;
		if (boundConversion.Operand.Type.NavTypeKind != NavTypeKind.DotNet)
		{
			argumentExpression = argumentExpression.SkipConversion();
		}
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression = EmitExpression(argumentExpression);
		if (boundConversion.Operand.Type.NavTypeKind == NavTypeKind.Variant)
		{
			return ALCompilerVariantToNavDotNet.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
		}
		return CodeGenFacts.ALCompilerToNavDotNetCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitFromDotNetConversion(BoundConversion conversion)
	{
		DebugAssertHelper.Assert(conversion.Operand.Type.NavTypeKind == NavTypeKind.DotNet);
		TypeSymbol type = conversion.Operand.Type;
		TypeSymbol type2 = conversion.Type;
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax emittedSourceDotNetExpresssion = EmitGetUnderlyingValue(type, EmitExpression(conversion.Operand));
		return EmitFromDotNetConversion(type, emittedSourceDotNetExpresssion, type2, conversion.TargetOpt);
	}

	internal static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitDotNetToJokerConversion(TypeSymbol sourceType, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		DebugAssertHelper.Assert(sourceType.NavTypeKind == NavTypeKind.DotNet);
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expressionSyntax = EmitGetUnderlyingValue(sourceType, expression);
		DotNetSpecialType specialType = ((DotNetTypeSymbol)sourceType).DotNetTypeInfo.SpecialType;
		switch (specialType)
		{
		case DotNetSpecialType.None:
		case DotNetSpecialType.System_Enum:
		case DotNetSpecialType.System_Void:
		case DotNetSpecialType.System_TimeSpan:
		case DotNetSpecialType.System_DateTime:
		case DotNetSpecialType.System_Stream:
		case DotNetSpecialType.System_Nullable:
			return expressionSyntax;
		case DotNetSpecialType.System_Object:
			return CodeGenFacts.ALCompilerObjectToNavValueCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expressionSyntax));
		case DotNetSpecialType.System_Boolean:
		case DotNetSpecialType.System_Char:
		case DotNetSpecialType.System_SByte:
		case DotNetSpecialType.System_Byte:
		case DotNetSpecialType.System_Int16:
		case DotNetSpecialType.System_UInt16:
		case DotNetSpecialType.System_Int32:
		case DotNetSpecialType.System_UInt32:
		case DotNetSpecialType.System_Int64:
		case DotNetSpecialType.System_UInt64:
		case DotNetSpecialType.System_Decimal:
		case DotNetSpecialType.System_Single:
		case DotNetSpecialType.System_Double:
		case DotNetSpecialType.System_String:
		case DotNetSpecialType.System_Guid:
			return CodeGenFacts.ALCompilerToNavValueCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expressionSyntax));
		default:
			throw ExceptionUtilities.UnexpectedValue(specialType);
		}
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitFromDotNetConversion(TypeSymbol dotNetType, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax emittedSourceDotNetExpresssion, TypeSymbol targetAlType, BoundExpression targetAlExpression)
	{
		DotNetSpecialType dotNetSpecialType = ConversionDotNet.GetDotNetSpecialType(dotNetType);
		int num = DotNetTypeToIndex(dotNetSpecialType);
		int num2 = AlTypeToIndex(targetAlType.NavTypeKind);
		if (IsGenericFromDotNetObjectToAlConversion(dotNetSpecialType, num2))
		{
			return EmitObjectToTypeConversion(emittedSourceDotNetExpresssion, targetAlType);
		}
		DebugAssertHelper.Assert(num >= 0);
		DebugAssertHelper.Assert(num2 >= 0);
		Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax invocationExpressionSyntax = DotNetToAlConversions[num, num2];
		DebugAssertHelper.Assert(invocationExpressionSyntax != null);
		if (invocationExpressionSyntax == IdentityConversion)
		{
			return emittedSourceDotNetExpresssion;
		}
		if (invocationExpressionSyntax == SpecialConversion)
		{
			return EmitSpecialDotNetToAlConversion(dotNetType, emittedSourceDotNetExpresssion, targetAlType, targetAlExpression);
		}
		return invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(emittedSourceDotNetExpresssion));
	}

	private static bool IsGenericFromDotNetObjectToAlConversion(DotNetSpecialType fromDotNetSpecialType, int alIndex)
	{
		if (fromDotNetSpecialType == DotNetSpecialType.System_Object)
		{
			return alIndex < 0;
		}
		return false;
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitToDotNetConversion(BoundConversion conversion)
	{
		DebugAssertHelper.Assert(conversion.Type.NavTypeKind == NavTypeKind.DotNet);
		TypeSymbol type = conversion.Operand.Type;
		int num = AlTypeToIndex(type.NavTypeKind);
		DebugAssertHelper.Assert(num >= 0);
		DotNetSpecialType dotNetSpecialType = ConversionDotNet.GetDotNetSpecialType(conversion.Type);
		int num2 = DotNetTypeToIndex(dotNetSpecialType);
		DebugAssertHelper.Assert(num2 >= 0);
		Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax invocationExpressionSyntax = AlToDotNetConversions[num, num2];
		DebugAssertHelper.Assert(invocationExpressionSyntax != null);
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expressionSyntax = EmitExpression(conversion.Operand);
		if (invocationExpressionSyntax == IdentityConversion)
		{
			return expressionSyntax;
		}
		if (invocationExpressionSyntax == SpecialConversion)
		{
			return EmitSpecialAlToDotNetConversion(expressionSyntax, type, dotNetSpecialType);
		}
		return invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expressionSyntax));
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitSpecialAlToDotNetConversion(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax operand, TypeSymbol fromAlType, DotNetSpecialType toDotNetSpecialType)
	{
		return fromAlType.NavTypeKind switch
		{
			NavTypeKind.Variant => (Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax)EmitFromVariantConversionToNavType(operand, GetNavIndirectValueTargetKind(toDotNetSpecialType)), 
			NavTypeKind.InStream => CodeGenFacts.ALCompilerInStreamToDotNetCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(operand)), 
			NavTypeKind.OutStream => CodeGenFacts.ALCompilerOutStreamToDotNetCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(operand)), 
			_ => throw ExceptionUtilities.UnexpectedValue(fromAlType.NavTypeKind), 
		};
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitSpecialDotNetToAlConversion(TypeSymbol dotNetType, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax emittedSourceDotNetExpression, TypeSymbol targetAlType, BoundExpression targetAlExpression)
	{
		DotNetSpecialType dotNetSpecialType = ConversionDotNet.GetDotNetSpecialType(dotNetType);
		switch (dotNetSpecialType)
		{
		case DotNetSpecialType.System_Object:
			return EmitObjectToTypeConversion(emittedSourceDotNetExpression, targetAlType);
		case DotNetSpecialType.System_String:
			return EmitStringToTextConversion(emittedSourceDotNetExpression, targetAlType);
		default:
			switch ((int)targetAlType.NavTypeKind)
			{
			case 12451848:
				if (targetAlType.IsNativeOptionTypeSymbol())
				{
					if (dotNetSpecialType == DotNetSpecialType.System_Enum)
					{
						emittedSourceDotNetExpression = CodeGenFacts.ALCompilerToIntegerCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(emittedSourceDotNetExpression));
					}
					return EmitDotNetToOptionConversion(dotNetSpecialType, emittedSourceDotNetExpression, targetAlType, targetAlExpression);
				}
				break;
			case 393261:
			case 393262:
				return EmitToStreamConversion(targetAlType, emittedSourceDotNetExpression);
			}
			throw ExceptionUtilities.UnexpectedValue(dotNetSpecialType.ToString() + targetAlType.NavTypeKind);
		}
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitStringToTextConversion(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax emittedSourceDotNetExpression, TypeSymbol targetAlType)
	{
		if (targetAlType.NavTypeKind == NavTypeKind.String)
		{
			return emittedSourceDotNetExpression;
		}
		return EmitToTextConversion(targetAlType, emittedSourceDotNetExpression);
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitToStreamConversion(TypeSymbol alType, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax accessDotNetRawValue)
	{
		return alType.NavTypeKind switch
		{
			NavTypeKind.InStream => (Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax)CodeGenFacts.ALCompilerDotNetToNavInStreamCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(accessDotNetRawValue)), 
			NavTypeKind.OutStream => CodeGenFacts.ALCompilerDotNetToNavOutStreamCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(accessDotNetRawValue)), 
			_ => throw ExceptionUtilities.UnexpectedValue(alType.NavTypeKind), 
		};
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitDotNetByRefSetValueConversion(TypeSymbol sourceDotNetType, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax emittedSourceDotNetExpression, BoundExpression targetAlExpression)
	{
		return EmitFromDotNetConversion(sourceDotNetType, emittedSourceDotNetExpression, targetAlExpression.Type, targetAlExpression);
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitDotNetToDotNetByRefSetValueConversion(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax emittedSourceDotNetExpression, BoundExpression targetAlExpression)
	{
		return emittedSourceDotNetExpression;
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitDotNetToOptionConversion(DotNetSpecialType dotNetSpecialType, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax emittedSourceDotNetExpression, TypeSymbol targetAlType, BoundExpression targetAlExpression)
	{
		if (dotNetSpecialType == DotNetSpecialType.System_Int64 || dotNetSpecialType == DotNetSpecialType.System_Decimal || dotNetSpecialType == DotNetSpecialType.System_TimeSpan)
		{
			emittedSourceDotNetExpression = CodeGenFacts.ALCompilerToIntegerCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(emittedSourceDotNetExpression));
		}
		return EmitToOptionConversion(EmitToOptionConversionMetadata(targetAlExpression, targetAlType), emittedSourceDotNetExpression);
	}

	internal static bool IsDotNetTypeRepresentedAsAClrType(DotNetTypeSymbol dotNetType)
	{
		DotNetSpecialType specialType = dotNetType.DotNetTypeInfo.SpecialType;
		switch (specialType)
		{
		case DotNetSpecialType.System_Object:
		case DotNetSpecialType.System_Boolean:
		case DotNetSpecialType.System_Char:
		case DotNetSpecialType.System_SByte:
		case DotNetSpecialType.System_Byte:
		case DotNetSpecialType.System_Int16:
		case DotNetSpecialType.System_UInt16:
		case DotNetSpecialType.System_Int32:
		case DotNetSpecialType.System_UInt32:
		case DotNetSpecialType.System_Int64:
		case DotNetSpecialType.System_UInt64:
		case DotNetSpecialType.System_Decimal:
		case DotNetSpecialType.System_Single:
		case DotNetSpecialType.System_Double:
		case DotNetSpecialType.System_String:
		case DotNetSpecialType.System_Guid:
			return true;
		case DotNetSpecialType.None:
		case DotNetSpecialType.System_Enum:
		case DotNetSpecialType.System_Void:
		case DotNetSpecialType.System_TimeSpan:
		case DotNetSpecialType.System_DateTime:
		case DotNetSpecialType.System_Stream:
		case DotNetSpecialType.System_Nullable:
			return false;
		default:
			throw ExceptionUtilities.UnexpectedValue(specialType);
		}
	}

	internal static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitGetUnderlyingValue(TypeSymbol dotNetType, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax emittedDotNetValueAccess)
	{
		TypeSyntax clrValueType = GetClrValueType(ConversionDotNet.GetDotNetSpecialType(dotNetType));
		if (clrValueType != null)
		{
			TypeArgumentListSyntax typeArgumentList = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.TypeArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SingletonSeparatedList(clrValueType));
			emittedDotNetValueAccess = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(emittedDotNetValueAccess, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.GenericName(CodeGenFacts.NavDotNetAsMethodNameIdentifier, typeArgumentList)));
		}
		return emittedDotNetValueAccess;
	}

	internal static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitVariableFieldInitializer(VariableSymbol variable)
	{
		return EmitFieldInitializer(variable.Type);
	}

	internal static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitReturnValueFieldInitializer(MethodSymbol method)
	{
		if (method.IsTrigger())
		{
			SourceTriggerSymbol sourceTriggerSymbol = (SourceTriggerSymbol)method;
			if (sourceTriggerSymbol.TriggerInfo.DefaultReturnValueEmittedSyntax != null)
			{
				return sourceTriggerSymbol.TriggerInfo.DefaultReturnValueEmittedSyntax;
			}
		}
		return EmitFieldInitializer(method.ReturnValue.Type);
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	internal static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitFieldInitializer(TypeSymbol typeSymbol)
	{
		if (typeSymbol.IsArray())
		{
			return null;
		}
		switch (typeSymbol.NavTypeKind)
		{
		case NavTypeKind.WebServiceActionResultCode:
		case NavTypeKind.NotificationScope:
		case NavTypeKind.Action:
		case NavTypeKind.TableConnectionType:
		case NavTypeKind.SecurityFilter:
		case NavTypeKind.TransactionType:
		case NavTypeKind.DefaultLayout:
		case NavTypeKind.TextEncoding:
		case NavTypeKind.ExecutionMode:
		case NavTypeKind.ObjectType:
		case NavTypeKind.ClientType:
		case NavTypeKind.ReportFormat:
		case NavTypeKind.FieldType:
		case NavTypeKind.FieldClass:
		case NavTypeKind.TestPermissions:
		case NavTypeKind.TransactionModel:
		case NavTypeKind.ExecutionContext:
		case NavTypeKind.ErrorType:
		case NavTypeKind.Byte:
		case NavTypeKind.Char:
		case NavTypeKind.Boolean:
		case NavTypeKind.Integer:
		case NavTypeKind.BigInteger:
		case NavTypeKind.Guid:
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.DefaultExpression(CodeGenFacts.GetRuntimeTypeSyntax(typeSymbol));
		case NavTypeKind.Decimal:
			return CodeGenFacts.Decimal18Zero;
		case NavTypeKind.Option:
		{
			SourceOptionTypeSymbol sourceOptionTypeSymbol = typeSymbol as SourceOptionTypeSymbol;
			return CodeGenFacts.NavOptionCreateCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenFacts.NclOptionMetadataCreateCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression((sourceOptionTypeSymbol != null) ? sourceOptionTypeSymbol.OptionMembers : string.Empty)))), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(0)));
		}
		case NavTypeKind.Enum:
			return CreateNavOptionWithEnumMetadata(typeSymbol as EnumTypeSymbol);
		case NavTypeKind.Text:
		case NavTypeKind.Code:
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(CodeGenFacts.GetRuntimeTypeSyntax(typeSymbol), CodeGenFacts.NavValueDefaultMethodNameSyntax)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(typeSymbol.HasLength ? typeSymbol.Length : 0)));
		case NavTypeKind.Label:
			return GetLabelDataTypeFieldInitializer(typeSymbol);
		case NavTypeKind.TextConst:
			return GetTextConstDataTypeFieldInitializer(typeSymbol);
		case NavTypeKind.DateFormula:
		case NavTypeKind.RecordId:
		case NavTypeKind.DateTime:
		case NavTypeKind.Time:
		case NavTypeKind.Date:
		case NavTypeKind.Duration:
			return CodeGenHelper.MemberAccessExpression(CodeGenFacts.GetRuntimeTypeSyntax(typeSymbol), CodeGenFacts.NavValueDefaultMethodNameSyntax);
		case NavTypeKind.InStream:
		case NavTypeKind.OutStream:
		case NavTypeKind.Variant:
		case NavTypeKind.DotNet:
			return null;
		case NavTypeKind.Notification:
		case NavTypeKind.DataScope:
		case NavTypeKind.ErrorInfo:
		case NavTypeKind.Verbosity:
		case NavTypeKind.DataClassification:
			return null;
		case NavTypeKind.Record:
		case NavTypeKind.Codeunit:
		case NavTypeKind.Page:
		case NavTypeKind.Report:
		case NavTypeKind.XmlPort:
		case NavTypeKind.Query:
		case NavTypeKind.ControlAddIn:
			return null;
		case NavTypeKind.Dialog:
		case NavTypeKind.FieldRef:
		case NavTypeKind.KeyRef:
		case NavTypeKind.File:
		case NavTypeKind.FilterPageBuilder:
		case NavTypeKind.SessionSettings:
			return null;
		case NavTypeKind.BigText:
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.GetRuntimeTypeSyntax(typeSymbol)).AddArgumentListArguments();
		case NavTypeKind.RecordRef:
			return null;
		case NavTypeKind.TestPage:
			return null;
		case NavTypeKind.HttpClient:
		case NavTypeKind.HttpRequestMessage:
		case NavTypeKind.HttpResponseMessage:
		case NavTypeKind.HttpContent:
			return null;
		case NavTypeKind.List:
		case NavTypeKind.Dictionary:
		case NavTypeKind.WebServiceActionContext:
		case NavTypeKind.JsonToken:
		case NavTypeKind.JsonObject:
		case NavTypeKind.JsonArray:
		case NavTypeKind.JsonValue:
		case NavTypeKind.HttpHeaders:
		case NavTypeKind.TextBuilder:
		case NavTypeKind.XmlAttribute:
		case NavTypeKind.XmlAttributeCollection:
		case NavTypeKind.XmlComment:
		case NavTypeKind.XmlCData:
		case NavTypeKind.XmlDeclaration:
		case NavTypeKind.XmlDocument:
		case NavTypeKind.XmlDocumentType:
		case NavTypeKind.XmlElement:
		case NavTypeKind.XmlNamespaceManager:
		case NavTypeKind.XmlNameTable:
		case NavTypeKind.XmlNode:
		case NavTypeKind.XmlNodeList:
		case NavTypeKind.XmlProcessingInstruction:
		case NavTypeKind.XmlText:
		case NavTypeKind.XmlReadOptions:
		case NavTypeKind.XmlWriteOptions:
		case NavTypeKind.Version:
		case NavTypeKind.ModuleInfo:
		case NavTypeKind.ModuleDependencyInfo:
			return CodeGenHelper.MemberAccessExpression(CodeGenFacts.GetRuntimeTypeSyntax(typeSymbol), CodeGenFacts.NavValueDefaultMethodNameSyntax);
		default:
			throw ExceptionUtilities.UnexpectedValue(typeSymbol.NavTypeKind);
		}
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax CreateNavOptionWithEnumMetadata(EnumTypeSymbol enumType, int value = 0)
	{
		return CodeGenFacts.NavOptionCreateCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenFacts.NclEnumMetadataCreateCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(enumType.Id)))), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(value)));
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax GetTextConstDataTypeFieldInitializer(TypeSymbol typeSymbol)
	{
		ArrayCreationExpressionSyntax navTextConstCreateLangIdArray = CodeGenFacts.NavTextConstCreateLangIdArray;
		ArrayCreationExpressionSyntax navTextConstCreateLangTextArray = CodeGenFacts.NavTextConstCreateLangTextArray;
		SourceTextConstTypeSymbol sourceTextConstTypeSymbol = typeSymbol as SourceTextConstTypeSymbol;
		DebugAssertHelper.Assert(sourceTextConstTypeSymbol != null);
		InitializerExpressionSyntax initializerExpressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InitializerExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ArrayInitializerExpression);
		InitializerExpressionSyntax initializerExpressionSyntax2 = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InitializerExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ArrayInitializerExpression);
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax[] array = new Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax[sourceTextConstTypeSymbol.LanguageTexts.Length];
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax[] array2 = new Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax[sourceTextConstTypeSymbol.LanguageTexts.Length];
		if (sourceTextConstTypeSymbol.LanguageTexts.Length != 0)
		{
			int num = 0;
			foreach (BoundMultilanguageExpression languageText in sourceTextConstTypeSymbol.LanguageTexts)
			{
				array[num] = CodeGenHelper.LiteralExpression(languageText.LanguageCultureId.Int32Value);
				array2[num++] = CodeGenHelper.LiteralExpression(languageText.LanguageValue.StringValue);
			}
		}
		navTextConstCreateLangIdArray = navTextConstCreateLangIdArray.WithInitializer(initializerExpressionSyntax.AddExpressions(array));
		navTextConstCreateLangTextArray = navTextConstCreateLangTextArray.WithInitializer(initializerExpressionSyntax2.AddExpressions(array2));
		return CodeGenFacts.NavTextConstCreateCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(navTextConstCreateLangIdArray), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(navTextConstCreateLangTextArray));
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax GetLabelDataTypeFieldInitializer(TypeSymbol typeSymbol)
	{
		ArrayCreationExpressionSyntax navTextConstCreateLangIdArray = CodeGenFacts.NavTextConstCreateLangIdArray;
		ArrayCreationExpressionSyntax navTextConstCreateLangTextArray = CodeGenFacts.NavTextConstCreateLangTextArray;
		InitializerExpressionSyntax initializerExpressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InitializerExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ArrayInitializerExpression);
		InitializerExpressionSyntax initializerExpressionSyntax2 = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InitializerExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ArrayInitializerExpression);
		SourceLabelTypeSymbol sourceLabelTypeSymbol = (SourceLabelTypeSymbol)typeSymbol;
		DebugAssertHelper.Assert(sourceLabelTypeSymbol != null);
		LanguagesMetadata languagesMetadata = typeSymbol.DeclaringCompilation?.LanguagesMetadata;
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax[] array = new Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax[0];
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax[] array2 = new Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax[0];
		if (languagesMetadata != null)
		{
			List<(int, string)> languagesForSymbolByLanguageId = languagesMetadata.GetLanguagesForSymbolByLanguageId(typeSymbol, sourceLabelTypeSymbol.BoundLabel);
			array = new Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax[languagesForSymbolByLanguageId.Count];
			array2 = new Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax[languagesForSymbolByLanguageId.Count];
			for (int i = 0; i < languagesForSymbolByLanguageId.Count; i++)
			{
				if (languagesForSymbolByLanguageId[i].Item2 != null)
				{
					array[i] = CodeGenHelper.LiteralExpression(languagesForSymbolByLanguageId[i].Item1);
					array2[i] = CodeGenHelper.LiteralExpression(languagesForSymbolByLanguageId[i].Item2);
				}
			}
		}
		navTextConstCreateLangIdArray = navTextConstCreateLangIdArray.WithInitializer(initializerExpressionSyntax.AddExpressions(array));
		navTextConstCreateLangTextArray = navTextConstCreateLangTextArray.WithInitializer(initializerExpressionSyntax2.AddExpressions(array2));
		return CodeGenFacts.NavTextConstCreateCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(navTextConstCreateLangIdArray), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(navTextConstCreateLangTextArray));
	}

	internal static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitVariableArrayInitializer(VariableSymbol variable, EmitOptions emitOptions)
	{
		ArrayTypeSymbol arrayTypeSymbol = variable.Type as ArrayTypeSymbol;
		DebugAssertHelper.Assert(arrayTypeSymbol != null);
		ArrayBuilder<ArgumentSyntax> instance = ArrayBuilder<ArgumentSyntax>.GetInstance();
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expressionSyntax = EmitFieldInitializer(arrayTypeSymbol.ElementType);
		if (expressionSyntax == null)
		{
			expressionSyntax = EmitVariableTypeInitializer(variable, arrayTypeSymbol.ElementType, emitOptions, asArrayFactory: true);
		}
		else
		{
			instance.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()));
		}
		instance.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expressionSyntax));
		for (int i = 0; i < arrayTypeSymbol.Dimensions.Length; i++)
		{
			instance.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(arrayTypeSymbol.Dimensions[i])));
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(((Symbol)variable).GetRuntimeTypeSyntax()).AddArgumentListArguments(instance.ToArrayAndFree());
	}

	internal static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitVariableInitializer(VariableSymbol variable, EmitOptions emitOptions)
	{
		if (variable.Type.IsArray())
		{
			return EmitVariableArrayInitializer(variable, emitOptions);
		}
		return EmitVariableTypeInitializer(variable, variable.Type, emitOptions);
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
	internal static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitVariableTypeInitializer(VariableSymbol variable, TypeSymbol variableType, EmitOptions emitOptions, bool asArrayFactory = false)
	{
		switch (variableType.NavTypeKind)
		{
		case NavTypeKind.WebServiceActionResultCode:
		case NavTypeKind.NotificationScope:
		case NavTypeKind.Action:
		case NavTypeKind.TableConnectionType:
		case NavTypeKind.SecurityFilter:
		case NavTypeKind.TransactionType:
		case NavTypeKind.DefaultLayout:
		case NavTypeKind.TextEncoding:
		case NavTypeKind.ExecutionMode:
		case NavTypeKind.ObjectType:
		case NavTypeKind.ClientType:
		case NavTypeKind.ReportFormat:
		case NavTypeKind.FieldType:
		case NavTypeKind.FieldClass:
		case NavTypeKind.TestPermissions:
		case NavTypeKind.TransactionModel:
		case NavTypeKind.ExecutionContext:
		case NavTypeKind.ErrorType:
		case NavTypeKind.Byte:
		case NavTypeKind.Char:
		case NavTypeKind.Boolean:
		case NavTypeKind.Integer:
		case NavTypeKind.BigInteger:
		case NavTypeKind.Guid:
			return null;
		case NavTypeKind.Decimal:
			return null;
		case NavTypeKind.Option:
			return null;
		case NavTypeKind.Enum:
			return null;
		case NavTypeKind.Text:
		case NavTypeKind.Code:
			return null;
		case NavTypeKind.TextConst:
		case NavTypeKind.Label:
			return null;
		case NavTypeKind.DateFormula:
		case NavTypeKind.RecordId:
		case NavTypeKind.DateTime:
		case NavTypeKind.Time:
		case NavTypeKind.Date:
		case NavTypeKind.Duration:
			return null;
		case NavTypeKind.InStream:
		case NavTypeKind.OutStream:
		case NavTypeKind.Variant:
			if (asArrayFactory)
			{
				return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(AsFactory(CodeGenFacts.GetRuntimeTypeSyntax(variableType), asArrayFactory)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()));
			}
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(CodeGenFacts.GetRuntimeTypeSyntax(variableType), CodeGenFacts.NavValueDefaultMethodNameSyntax)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()));
		case NavTypeKind.List:
		case NavTypeKind.Dictionary:
		case NavTypeKind.WebServiceActionContext:
		case NavTypeKind.JsonToken:
		case NavTypeKind.JsonObject:
		case NavTypeKind.JsonArray:
		case NavTypeKind.JsonValue:
		case NavTypeKind.HttpHeaders:
		case NavTypeKind.TextBuilder:
		case NavTypeKind.XmlAttribute:
		case NavTypeKind.XmlAttributeCollection:
		case NavTypeKind.XmlComment:
		case NavTypeKind.XmlCData:
		case NavTypeKind.XmlDeclaration:
		case NavTypeKind.XmlDocument:
		case NavTypeKind.XmlDocumentType:
		case NavTypeKind.XmlElement:
		case NavTypeKind.XmlNamespaceManager:
		case NavTypeKind.XmlNameTable:
		case NavTypeKind.XmlNode:
		case NavTypeKind.XmlNodeList:
		case NavTypeKind.XmlProcessingInstruction:
		case NavTypeKind.XmlText:
		case NavTypeKind.XmlReadOptions:
		case NavTypeKind.XmlWriteOptions:
		case NavTypeKind.Version:
		case NavTypeKind.ModuleInfo:
		case NavTypeKind.ModuleDependencyInfo:
			return CodeGenHelper.MemberAccessExpression(CodeGenFacts.GetRuntimeTypeSyntax(variableType), CodeGenFacts.NavValueDefaultMethodNameSyntax);
		case NavTypeKind.Codeunit:
		case NavTypeKind.Page:
		case NavTypeKind.Report:
		case NavTypeKind.XmlPort:
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(AsFactory(CodeGenFacts.GetRuntimeTypeSyntax(variableType), asArrayFactory), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(((ISymbolWithId)variableType).Id))), null);
		case NavTypeKind.Query:
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(AsFactory(CodeGenFacts.GetRuntimeTypeSyntax(variableType), asArrayFactory), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(((ISymbolWithId)variableType).Id)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(EmitSecurityFiltering(variable, variableType))), null);
		case NavTypeKind.Record:
		{
			RecordTypeSymbol recordTypeSymbol = (RecordTypeSymbol)variableType;
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(AsFactory(CodeGenFacts.NavRecordHandleType, asArrayFactory), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(recordTypeSymbol.Id)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(recordTypeSymbol.Temporary)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(EmitSecurityFiltering(variable, variableType))), null);
		}
		case NavTypeKind.ControlAddIn:
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.NavDotNetType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NullLiteralExpression)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.TrueLiteralExpression)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.FalseLiteralExpression))), null);
		case NavTypeKind.Dialog:
		case NavTypeKind.FieldRef:
		case NavTypeKind.KeyRef:
		case NavTypeKind.File:
		case NavTypeKind.FilterPageBuilder:
		case NavTypeKind.HttpClient:
		case NavTypeKind.HttpRequestMessage:
		case NavTypeKind.HttpResponseMessage:
		case NavTypeKind.HttpContent:
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(AsFactory(CodeGenFacts.GetRuntimeTypeSyntax(variableType), asArrayFactory), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression())), null);
		case NavTypeKind.BigText:
			return null;
		case NavTypeKind.RecordRef:
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(AsFactory(CodeGenFacts.GetRuntimeTypeSyntax(variableType), asArrayFactory), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(EmitSecurityFiltering(variable, variableType))), null);
		case NavTypeKind.TestPage:
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.GetRuntimeTypeSyntax(variableType), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(((TestPageTypeSymbol)variableType).Id))), null);
		case NavTypeKind.Notification:
		case NavTypeKind.DataScope:
		case NavTypeKind.ErrorInfo:
		case NavTypeKind.Verbosity:
		case NavTypeKind.DataClassification:
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.GetRuntimeTypeSyntax(variableType), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList(), null);
		case NavTypeKind.SessionSettings:
			if (asArrayFactory)
			{
				return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(AsFactory(CodeGenFacts.GetRuntimeTypeSyntax(variableType), asArrayFactory), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression())), null);
			}
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.GetRuntimeTypeSyntax(variableType), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList(), null);
		case NavTypeKind.DotNet:
			return EmitDotNetVariableTypeInitializer(variable, variableType, asArrayFactory, emitOptions);
		default:
			throw ExceptionUtilities.UnexpectedValue(variableType.NavTypeKind);
		}
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitDotNetVariableTypeInitializer(VariableSymbol variable, TypeSymbol variableType, bool asArrayFactory, EmitOptions emitOptions)
	{
		bool flag = variable.HasRunOnClientAttribute();
		DotNetTypeSymbol dotNetTypeSymbol = (DotNetTypeSymbol)variableType;
		if (asArrayFactory)
		{
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(AsFactory(CodeGenFacts.NavDotNetType, asArrayFactory), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(dotNetTypeSymbol.DotNetTypeInfo.Assembly.FullName)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(dotNetTypeSymbol.DotNetTypeInfo.TypeName)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(flag))), null);
		}
		bool withEvents = variable.HasWithEventsAttribute();
		return CodeGenHelper.NavDotNetObjectCreationExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), dotNetTypeSymbol.DotNetTypeInfo.Assembly.FullName, dotNetTypeSymbol.DotNetTypeInfo.TypeName, flag, dotNetTypeSymbol.DotNetTypeInfo.IsInterface, variable.HasSuppressDisposeAttribute(), EmitDotNetVariableEventSink(variable, flag, withEvents, emitOptions));
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitDotNetVariableEventSink(VariableSymbol variable, bool runOnClient, bool withEvents, EmitOptions emitOptions)
	{
		if (!withEvents)
		{
			return null;
		}
		if (runOnClient)
		{
			return CodeGenHelper.LiteralExpression(variable.Name);
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenHelper.NavEventHandlerClassName(variable, emitOptions)).WithArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SingletonSeparatedList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()))));
	}

	private static TypeSyntax AsFactory(TypeSyntax type, bool asFactory)
	{
		if (!asFactory)
		{
			return type;
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName(string.Concat(type, ".Factory"));
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax EmitSecurityFiltering(VariableSymbol variable, TypeSymbol type)
	{
		string name = GetDefaultSecurityFilteringForType(type);
		AttributeSymbol attributeSymbol = variable.Attributes.FirstOrDefault((AttributeSymbol attr) => attr.AttributeInfo.Kind == AttributeKind.SecurityFiltering);
		SystemOptionKinds.SecurityFilterKind? securityFilterKind = null;
		if (attributeSymbol != null)
		{
			securityFilterKind = attributeSymbol.Arguments[0].GetValueAsEnum<SystemOptionKinds.SecurityFilterKind>();
		}
		switch (securityFilterKind)
		{
		case SystemOptionKinds.SecurityFilterKind.Disallowed:
			name = "Disallowed";
			break;
		case SystemOptionKinds.SecurityFilterKind.Validated:
			name = "Validated";
			break;
		case SystemOptionKinds.SecurityFilterKind.Ignored:
			name = "Ignored";
			break;
		case SystemOptionKinds.SecurityFilterKind.Filtered:
			name = "Filtered";
			break;
		}
		return CodeGenHelper.MemberAccessExpression("SecurityFiltering", name);
	}

	private static string GetDefaultSecurityFilteringForType(TypeSymbol type)
	{
		switch (type.NavTypeKind)
		{
		case NavTypeKind.RecordRef:
		case NavTypeKind.Record:
			return "Validated";
		case NavTypeKind.Query:
			return "Filtered";
		default:
			throw ExceptionUtilities.UnexpectedValue(type.NavTypeKind);
		}
	}

	internal static Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax EmitVariableClearStatement(VariableSymbol global)
	{
		return EmitClearStatement(global, CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(global.MetadataName)));
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax EmitClearStatement(VariableSymbol variable, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		if (variable.Type.IsArray() || variable.Type.IsUserType())
		{
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(expression, CodeGenFacts.ClearMethodNameSyntax)));
		}
		if (variable.Type.NavTypeKind.UseStaticMethodToClear())
		{
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, expression, CodeGenHelper.MemberAccessExpression(((Symbol)variable).GetRuntimeTypeSyntax(), CodeGenFacts.NavValueDefaultMethodNameSyntax)));
		}
		if (variable.Type.NavTypeKind.UseInstanceMethodToClear())
		{
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(CodeGenFacts.ALSystemVariableClearCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression)));
		}
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expressionSyntax = EmitVariableFieldInitializer(variable);
		if (expressionSyntax != null)
		{
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, expression, expressionSyntax));
		}
		throw ExceptionUtilities.UnreachableWithMessage(string.Format(CultureInfo.InvariantCulture, "{0} variable of type {1}", variable.VariableKind, variable.GetNavTypeKind()));
	}

	internal static Microsoft.CodeAnalysis.CSharp.SyntaxKind[] EmitFieldDeclarationModifiers(TypeSymbol typeSymbol)
	{
		NavTypeKind navTypeKind = typeSymbol.NavTypeKind;
		if ((uint)(navTypeKind - 2228235) <= 1u)
		{
			return TextConstFieldDeclarationSyntaxKinds;
		}
		return DefaultFieldDeclarationSyntaxKinds;
	}

	internal static Microsoft.CodeAnalysis.CSharp.SyntaxKind[] EmitMethodLocalFieldDeclarationModifiers(TypeSymbol typeSymbol)
	{
		NavTypeKind navTypeKind = typeSymbol.NavTypeKind;
		if ((uint)(navTypeKind - 2228235) <= 1u)
		{
			return TextConstMethodLocalFieldDeclarationSyntaxKinds;
		}
		return DefaultMethodLocalFieldDeclarationSyntaxKinds;
	}

	public CodeGenerator(CompilationOptions compilationOptions, EmitOptions emitOptions)
	{
		CompilationOptions = compilationOptions;
		EmitOptions = emitOptions;
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitByRefSetValueConversion(BoundConversion boundConversion, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		TypeSymbol type = boundConversion.Type;
		TypeSymbol type2 = boundConversion.Operand.Type;
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if (type.NavTypeKind == NavTypeKind.Joker)
		{
			return expression;
		}
		switch (ClassifyByRefSetConversion(type, type2, ref useSiteDiagnostics).Kind)
		{
		case ConversionKind.ImplicitTextToSmaller:
		case ConversionKind.ToText:
		case ConversionKind.ImplicitText:
			return EmitToTextConversion(type2, type, expression);
		case ConversionKind.ImplicitRecord:
		case ConversionKind.ImplicitCodeunit:
		case ConversionKind.ToDecimal:
		case ConversionKind.Identity:
			return expression;
		case ConversionKind.ImplicitNumeric:
			if (type2.NavTypeKind == NavTypeKind.Char)
			{
				return EmitToNumericConversion(type2.NavTypeKind, expression);
			}
			return expression;
		case ConversionKind.ImplicitNumericToSmaller:
			return EmitToNumericConversion(type2.NavTypeKind, expression);
		case ConversionKind.ToOption:
		case ConversionKind.ImplicitOption:
			return EmitToOptionConversion(CodeGenHelper.MemberAccessExpression(EmitExpression(boundConversion.Operand), CodeGenFacts.NavOptionMetadataPropertyNameSyntax), expression);
		case ConversionKind.ToVariant:
			return expression;
		case ConversionKind.NarrowingFromDotNet:
		case ConversionKind.WideningFromDotNet:
		case ConversionKind.PreservingFromDotNet:
			return EmitDotNetByRefSetValueConversion(type, expression, boundConversion.Operand);
		case ConversionKind.NarrowingFromDotNetToDotNet:
		case ConversionKind.WideningFromDotNetToDotNet:
		case ConversionKind.PreservingFromDotNetToDotNet:
			return EmitDotNetToDotNetByRefSetValueConversion(expression, boundConversion.Operand);
		case ConversionKind.NarrowingToDotNet:
		case ConversionKind.WideningToDotNet:
		case ConversionKind.PreservingToDotNet:
			return EmitAlToDotNetByRefSetValueConversion(type, expression);
		default:
			throw ExceptionUtilities.UnexpectedValue(type2.NavTypeKind);
		}
	}

	private static Conversion ClassifyByRefSetConversion(TypeSymbol sourceType, TypeSymbol targetType, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		Conversions.ApplyCompatibilityEnumTypeSubstitution(ref sourceType, ref targetType);
		return Conversions.ClassifyStandardImplicitConversion(sourceType, targetType, ref useSiteDiagnostics);
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitAlToDotNetByRefSetValueConversion(TypeSymbol fromAlType, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax alExpression)
	{
		return fromAlType.NavTypeKind switch
		{
			NavTypeKind.Code => (Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax)CodeGenFacts.ALCompilerNavCodeToDotNetCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(alExpression)), 
			NavTypeKind.Text => CodeGenFacts.ALCompilerNavTextToDotNetCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(alExpression)), 
			NavTypeKind.DateTime => CodeGenFacts.ALCompilerDateTimeToDotNetCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(alExpression)), 
			_ => throw ExceptionUtilities.UnexpectedValue(fromAlType.NavTypeKind), 
		};
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitConversion(BoundConversion conversion, bool suppressTarget = false)
	{
		switch (conversion.ConversionKind)
		{
		case ConversionKind.ImplicitTextToSmaller:
		case ConversionKind.ToText:
		case ConversionKind.ImplicitText:
			return EmitToTextConversion(conversion);
		case ConversionKind.ToDecimal:
		case ConversionKind.ImplicitNumeric:
			return EmitToNumericConversion(conversion);
		case ConversionKind.ImplicitNumericToSmaller:
			return EmitExplicitNumericConversion(conversion);
		case ConversionKind.ImplicitJoker:
			return EmitToJokerConversion(conversion);
		case ConversionKind.ToOption:
		case ConversionKind.ImplicitOption:
			return EmitToOptionConversion(conversion);
		case ConversionKind.ToGuid:
			return EmitToGuidConversion(conversion);
		case ConversionKind.ToChar:
			return EmitToCharConversion(conversion);
		case ConversionKind.ImplicitConstant:
			return EmitConstantConversion(conversion);
		case ConversionKind.ImplicitArray:
			return EmitArrayConversion(conversion);
		case ConversionKind.ImplicitRecord:
		case ConversionKind.ImplicitCodeunit:
		case ConversionKind.Identity:
			return EmitExpression(conversion.Operand, suppressTarget);
		case ConversionKind.FromVariant:
			return EmitFromVariantConversion(conversion, suppressTarget);
		case ConversionKind.ToVariant:
			return EmitToVariantConversion(conversion);
		case ConversionKind.FromJoker:
			return EmitFromJokerConversion(conversion);
		case ConversionKind.NarrowingFromDotNet:
		case ConversionKind.WideningFromDotNet:
		case ConversionKind.PreservingFromDotNet:
			return EmitFromDotNetConversion(conversion);
		case ConversionKind.NarrowingToDotNet:
		case ConversionKind.WideningToDotNet:
		case ConversionKind.PreservingToDotNet:
			return EmitToDotNetConversion(conversion);
		case ConversionKind.NarrowingFromDotNetToDotNet:
		case ConversionKind.WideningFromDotNetToDotNet:
		case ConversionKind.PreservingFromDotNetToDotNet:
			return EmitExpression(conversion.Operand);
		default:
			throw ExceptionUtilities.UnexpectedValue(conversion.ConversionKind);
		}
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitToTextConversion(BoundConversion conversion)
	{
		return EmitToTextConversion(conversion.Type, conversion.Operand.Type, EmitExpression(conversion.Operand));
	}

	internal static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitToTextConversion(TypeSymbol targetType, TypeSymbol sourceType, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return targetType.NavTypeKind switch
		{
			NavTypeKind.String => EmitToStringConversion(sourceType, expression), 
			NavTypeKind.Code => EmitToNavCodeConversion(targetType, sourceType, expression), 
			NavTypeKind.Text => EmitToNavTextConversion(targetType, sourceType, expression), 
			_ => throw ExceptionUtilities.UnexpectedValue(targetType.NavTypeKind), 
		};
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitToNavTextConversion(TypeSymbol targetType, TypeSymbol sourceType, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return sourceType.NavTypeKind switch
		{
			NavTypeKind.Guid => (Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax)CodeGenFacts.ALCompilerGuidToNavTextCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression)), 
			NavTypeKind.Char => EmitToTextConversion(targetType, EmitCharToStringConversion(expression)), 
			_ => EmitToTextConversion(targetType, expression), 
		};
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitToNavCodeConversion(TypeSymbol targetType, TypeSymbol sourceType, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return sourceType.NavTypeKind switch
		{
			NavTypeKind.Guid => (Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax)CodeGenFacts.ALCompilerGuidToNavCodeCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression)), 
			NavTypeKind.Char => EmitToTextConversion(targetType, EmitCharToStringConversion(expression)), 
			_ => EmitToTextConversion(targetType, expression), 
		};
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitToStringConversion(TypeSymbol sourceType, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		switch (sourceType.NavTypeKind)
		{
		case NavTypeKind.Guid:
			return CodeGenFacts.ALCompilerGuidToStringCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
		case NavTypeKind.Char:
			return EmitCharToStringConversion(expression);
		case NavTypeKind.String:
		case NavTypeKind.TextConst:
		case NavTypeKind.Label:
		case NavTypeKind.Text:
		case NavTypeKind.Code:
			return expression;
		default:
			throw ExceptionUtilities.UnexpectedValue(sourceType.NavTypeKind);
		}
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitCharToStringConversion(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		if (expression is Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax)
		{
			return CodeGenHelper.LiteralExpression(((Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax)expression).Token.ValueText);
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(expression, CodeGenFacts.ToStringMethodNameSyntax));
	}

	protected static ObjectCreationExpressionSyntax EmitToTextConversion(TypeSymbol textType, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		DebugAssertHelper.Assert(textType != null);
		DebugAssertHelper.Assert(textType.NavTypeKind == NavTypeKind.Text || textType.NavTypeKind == NavTypeKind.Code);
		ObjectCreationExpressionSyntax objectCreationExpressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(textType.NavTypeKind.RuntimeTypeSyntax());
		if (textType.HasLength)
		{
			objectCreationExpressionSyntax = objectCreationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(textType.Length)));
		}
		return objectCreationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitToNumericConversion(BoundConversion conversion)
	{
		return EmitExpression(conversion.Operand);
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitExplicitNumericConversion(BoundConversion conversion)
	{
		return EmitToNumericConversion(conversion.Type.NavTypeKind, EmitExpression(conversion.Operand));
	}

	protected internal static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitExplicitNumericConversion(NavTypeKind sourceKind, NavTypeKind targetKind, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax operand)
	{
		if (Conversions.GetNumericTypeIndex(sourceKind) <= Conversions.GetNumericTypeIndex(targetKind))
		{
			return operand;
		}
		return EmitToNumericConversion(targetKind, operand);
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitToNumericConversion(NavTypeKind typeKind, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax operand)
	{
		return typeKind switch
		{
			NavTypeKind.Byte => (Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax)CodeGenFacts.ALCompilerToByteCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(operand)), 
			NavTypeKind.Char => CodeGenFacts.ALCompilerToCharCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(operand)), 
			NavTypeKind.Integer => CodeGenFacts.ALCompilerToIntegerCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(operand)), 
			NavTypeKind.BigInteger => CodeGenFacts.ALCompilerToBigIntegerCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(operand)), 
			NavTypeKind.Decimal => CodeGenFacts.ALCompilerToDecimalCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(operand)), 
			NavTypeKind.Duration => CodeGenFacts.ALCompilerToDurationCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(operand)), 
			_ => throw ExceptionUtilities.UnexpectedValue(typeKind), 
		};
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitToJokerConversion(BoundConversion conversion)
	{
		if (conversion.Operand.IsNavOptionAccess())
		{
			return EmitNavOptionToJokerConversion(conversion);
		}
		return EmitToJokerConversion(conversion.Operand.Type, EmitExpression(conversion.Operand));
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitNavOptionToJokerConversion(BoundConversion conversion)
	{
		BoundOptionAccess boundOptionAccess = (BoundOptionAccess)conversion.Operand;
		return boundOptionAccess.ReceiverOpt.Kind switch
		{
			BoundKind.QueryElementAccess => EmitQueryElementOptionToJokerConversion(conversion, boundOptionAccess), 
			BoundKind.EnumType => CreateNavOptionWithEnumMetadata(((BoundEnumType)boundOptionAccess.ReceiverOpt).EnumType, boundOptionAccess.OptionSymbol.Ordinal), 
			_ => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(EmitExpression(boundOptionAccess.ReceiverOpt), CodeGenFacts.NavOptionCreateInstanceMethodIdentifier)).WithArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SingletonSeparatedList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(EmitExpression(conversion.Operand))))), 
		};
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitQueryElementOptionToJokerConversion(BoundConversion conversion, BoundOptionAccess optionAccess)
	{
		BoundQueryElementAccess boundQueryElementAccess = (BoundQueryElementAccess)optionAccess.ReceiverOpt;
		if (boundQueryElementAccess.ExpressionSymbol.Kind == SymbolKind.QueryFilter)
		{
			return CodeGenFacts.ALCompilerToNavValueCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(EmitExpression(conversion.Operand)));
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(EmitQueryElementAccess(boundQueryElementAccess, isOptionValueAccess: true), CodeGenFacts.NavOptionCreateInstanceMethodIdentifier)).WithArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SingletonSeparatedList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(EmitExpression(conversion.Operand)))));
	}

	protected internal static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitToJokerConversion(TypeSymbol sourceType, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		if (!sourceType.NavTypeKind.IsNavValueType())
		{
			return CodeGenFacts.ALCompilerToNavValueCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
		}
		if (ConversionDotNet.IsStronglyTypedDotNetSymbol(sourceType))
		{
			return EmitDotNetToJokerConversion(sourceType, expression);
		}
		return expression;
	}

	protected internal static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitToJokerConversion(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return CodeGenFacts.ALCompilerToNavValueCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitToOptionConversion(BoundConversion conversion)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax metadata = EmitToOptionConversionMetadata(conversion.TargetOpt, conversion.Type);
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax ordinal = EmitToOptionConversionOrdinal(conversion.Operand, conversion.TargetOpt, conversion.ConstantValue);
		return EmitToOptionConversion(metadata, ordinal);
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitToOptionConversionMetadata(BoundExpression targetOpt, TypeSymbol targetOptionType)
	{
		if (targetOpt == null)
		{
			if (targetOptionType.Kind == SymbolKind.Enum)
			{
				EnumTypeSymbol enumTypeSymbol = (EnumTypeSymbol)targetOptionType;
				return CodeGenFacts.NclEnumMetadataCreateCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(enumTypeSymbol.Id)));
			}
			string value = (targetOptionType as OptionTypeSymbol)?.OptionMembers ?? string.Empty;
			return CodeGenFacts.NclOptionMetadataCreateCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(value)));
		}
		return CodeGenHelper.MemberAccessExpression(EmitExpression(targetOpt), CodeGenFacts.NavOptionMetadataPropertyNameSyntax);
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitToOptionConversionOrdinal(BoundExpression operand, BoundExpression targetOpt, ConstantValue constantOptionValue)
	{
		if (targetOpt == null)
		{
			return EmitExpression(operand);
		}
		return EmitToOptionConversionOrdinal(operand, constantOptionValue);
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitToOptionConversionOrdinal(BoundExpression operand, ConstantValue constantOptionValue)
	{
		if (constantOptionValue != null)
		{
			return CodeGenHelper.LiteralExpression(constantOptionValue.Int32Value);
		}
		switch (operand.Type.NavTypeKind)
		{
		case NavTypeKind.Option:
			return EmitExpression(operand);
		case NavTypeKind.Variant:
		{
			BoundConversion conversion = BoundConversion.Synthesized(operand.Syntax, operand, Conversion.FromVariant, null, NavCorLib.IntegerType);
			return EmitFromVariantConversion(conversion, suppressTarget: false);
		}
		default:
			return EmitExplicitNumericConversion(operand.Type.NavTypeKind, NavTypeKind.Integer, EmitExpression(operand));
		}
	}

	internal static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitToOptionConversion(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax metadata, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax ordinal)
	{
		return CodeGenFacts.NavOptionCreateCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(metadata), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(ordinal));
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitToGuidConversion(BoundConversion conversion)
	{
		DebugAssertHelper.Assert(conversion.Operand.Type.IsTextType());
		return CodeGenFacts.ALCompilerStringToGuidCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(EmitExpression(conversion.Operand)));
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitToCharConversion(BoundConversion conversion)
	{
		return CodeGenFacts.ALCompilerToCharCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(EmitExpression(conversion.Operand)));
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitArrayConversion(BoundConversion conversion)
	{
		DebugAssertHelper.Assert(conversion.Type.IsArray());
		return EmitExpression(conversion.Operand);
	}

	internal static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitConstantConversion(BoundConversion conversion)
	{
		return conversion.ConstantValueOpt.Discriminator switch
		{
			ConstantValueTypeDiscriminator.Char => (Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax)CodeGenHelper.LiteralExpression(conversion.ConstantValueOpt.CharValue), 
			ConstantValueTypeDiscriminator.Int32 => CodeGenHelper.LiteralExpression(conversion.ConstantValueOpt.Int32Value), 
			ConstantValueTypeDiscriminator.Int64 => CodeGenHelper.LiteralExpression(conversion.ConstantValueOpt.Int64Value), 
			_ => throw ExceptionUtilities.UnexpectedValue(conversion.ConstantValueOpt.Discriminator), 
		};
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitFromJokerConversion(BoundConversion conversion)
	{
		return EmitJokerToTypeConversionSlow(EmitExpression(conversion.Operand), conversion.Type);
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitFromVariantConversion(BoundConversion conversion, bool suppressTarget)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax invocationExpressionSyntax = EmitSpecializedFromVariantConversion(conversion);
		if (invocationExpressionSyntax == null)
		{
			invocationExpressionSyntax = EmitFromVariantConversionToNavType(EmitExpression(conversion.Operand), conversion.Type.NavTypeKind);
			int length = conversion.Type.Length;
			if (length != 0)
			{
				ArgumentSyntax argumentSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(conversion.Type.NavTypeKind.GetNavTypeEnumExpression());
				ArgumentSyntax argumentSyntax2 = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(length));
				ObjectCreationExpressionSyntax expression = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.NavValueDefinedLengthMetadataType).AddArgumentListArguments(argumentSyntax, argumentSyntax2);
				invocationExpressionSyntax = invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
			}
		}
		return EmitHandleAccess(conversion.Type, invocationExpressionSyntax, suppressTarget);
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax EmitFromVariantConversionToNavType(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax operandExpression, NavTypeKind targetNavTypeKind)
	{
		return (targetNavTypeKind.GetVariantConversionInvocation() ?? throw ExceptionUtilities.UnexpectedValue(targetNavTypeKind)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(operandExpression));
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax EmitSpecializedFromVariantConversion(BoundConversion conversion)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax result = null;
		if (conversion.Type.NavTypeKind == NavTypeKind.Option)
		{
			ArgumentSyntax argumentSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(EmitExpression(conversion.Operand));
			Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax expression = CodeGenFacts.ALCompilerNavIndirectValueToInt32Call.AddArgumentListArguments(argumentSyntax);
			ArgumentSyntax argumentSyntax2 = ((conversion.TargetOpt != null) ? Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.MemberAccessExpression(EmitExpression(conversion.TargetOpt), CodeGenFacts.NavOptionMetadataPropertyNameSyntax)) : Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenFacts.NCLOptionMetadataDefault));
			result = CodeGenFacts.NavOptionCreateCall.AddArgumentListArguments(argumentSyntax2, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
		}
		return result;
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitObjectToTypeConversion(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, TypeSymbol type)
	{
		switch (type.NavTypeKind)
		{
		case NavTypeKind.InStream:
			return CodeGenFacts.ALCompilerObjectToNavInStreamCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
		case NavTypeKind.OutStream:
			return CodeGenFacts.ALCompilerObjectToNavOutStreamCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
		default:
		{
			if (type.NavTypeKind.IsNavValueType())
			{
				GenericNameSyntax name = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.GenericName("ObjectToExactNavValue").AddTypeArgumentListArguments(((Symbol)type).GetRuntimeTypeSyntax());
				return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(CodeGenFacts.ALCompilerClassNameSyntax, name)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
			}
			Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax objectConversionInvocation = type.GetObjectConversionInvocation();
			if (objectConversionInvocation != null)
			{
				return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(((Symbol)type).GetRuntimeTypeSyntax(), objectConversionInvocation.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression))));
			}
			throw ExceptionUtilities.UnreachableWithMessage(type.GetNavTypeKindSafe().ToString());
		}
		}
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitToVariantConversion(BoundConversion conversion)
	{
		TypeSymbol type = conversion.Operand.Type;
		NavTypeKind navTypeKind = type.NavTypeKind;
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expressionSyntax = EmitExpression(conversion.Operand, suppressTarget: true);
		Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax invocationExpressionSyntax;
		if (navTypeKind == NavTypeKind.Record)
		{
			invocationExpressionSyntax = CodeGenFacts.ALCompilerNavRecordToVariantCall;
		}
		else if (navTypeKind.IsClrType() || navTypeKind.IsClrOption())
		{
			invocationExpressionSyntax = CodeGenFacts.ALCompilerToVariantCall;
		}
		else if (ConversionDotNet.IsStronglyTypedDotNetSymbol(conversion.Operand.Type))
		{
			DotNetTypeSymbol dotNetType = (DotNetTypeSymbol)type;
			expressionSyntax = EmitGetUnderlyingValue(dotNetType, expressionSyntax);
			invocationExpressionSyntax = ((!IsDotNetTypeRepresentedAsAClrType(dotNetType)) ? CodeGenFacts.ALCompilerNavValueToVariantCall : CodeGenFacts.ALCompilerToVariantCall);
		}
		else
		{
			invocationExpressionSyntax = CodeGenFacts.ALCompilerNavValueToVariantCall;
		}
		return invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expressionSyntax));
	}

	internal static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitFastJokerToTypeConversion(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, TypeSymbol type)
	{
		if (type.NavTypeKind.IsNavValueType())
		{
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(((Symbol)type).GetRuntimeTypeSyntax(), expression));
		}
		return type.NavTypeKind switch
		{
			NavTypeKind.Boolean => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(expression, CodeGenFacts.NavValueToBooleanMethodNameSyntax)), 
			NavTypeKind.Byte => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(expression, CodeGenFacts.NavValueToByteMethodNameSyntax)), 
			NavTypeKind.Char => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(CodeGenFacts.ALCompilerClassNameSyntax, CodeGenFacts.NavValueToCharMethodNameSyntax)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression)), 
			NavTypeKind.Integer => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(expression, CodeGenFacts.NavValueToInt32MethodNameSyntax)), 
			NavTypeKind.BigInteger => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(expression, CodeGenFacts.NavValueToInt64MethodNameSyntax)), 
			NavTypeKind.Decimal => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(expression, CodeGenFacts.NavValueToDecimalMethodNameSyntax)), 
			NavTypeKind.Guid => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(expression, CodeGenFacts.NavValueToGuidMethodNameSyntax)), 
			NavTypeKind.String => CodeGenFacts.ALCompilerObjectToStringCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression)), 
			_ => EmitJokerToTypeConversionSlow(expression, type), 
		};
	}

	internal static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitJokerToTypeConversionSlow(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, TypeSymbol type)
	{
		if (type.NavTypeKind.IsNavValueType() && !type.NavTypeKind.IsGenericType() && !type.IsTextType())
		{
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(CodeGenFacts.ALCompilerClassNameSyntax, CodeGenFacts.NavValueToNavValueMethodNameSyntax.WithTypeArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.TypeArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SingletonSeparatedList(((Symbol)type).GetRuntimeTypeSyntax())))), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SingletonSeparatedList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression))));
		}
		switch (type.NavTypeKind)
		{
		case NavTypeKind.Boolean:
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(expression, CodeGenFacts.NavValueToBooleanMethodNameSyntax));
		case NavTypeKind.Byte:
			return CodeGenFacts.ALCompilerObjectToByteCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
		case NavTypeKind.Char:
			return CodeGenFacts.ALCompilerObjectToCharCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
		case NavTypeKind.Integer:
			return CodeGenFacts.ALCompilerObjectToIntegerCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
		case NavTypeKind.BigInteger:
			return CodeGenFacts.ALCompilerObjectToBigIntegerCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
		case NavTypeKind.Decimal:
			return CodeGenFacts.ALCompilerObjectToDecimalCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
		case NavTypeKind.Guid:
			return CodeGenFacts.ALCompilerObjectToGuidCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
		case NavTypeKind.String:
			return CodeGenFacts.ALCompilerObjectToStringCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
		case NavTypeKind.Text:
		case NavTypeKind.Code:
			return EmitJokerToTextConversion(expression, type);
		default:
			return CodeGenFacts.GetObjectToTypeConversionInvocation_Generated(type.NavTypeKind).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
		}
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitJokerToTextConversion(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, TypeSymbol type)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax invocationExpressionSyntax = type.NavTypeKind switch
		{
			NavTypeKind.Code => CodeGenFacts.NavCodeCreateFromObjectCall, 
			NavTypeKind.Text => CodeGenFacts.NavTextCreateFromObjectCall, 
			_ => throw ExceptionUtilities.UnexpectedValue(type.NavTypeKind), 
		};
		if (type.HasLength)
		{
			return invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(type.Length)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
		}
		return invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitExpression(BoundExpression expr, bool suppressTarget = false, bool isStatement = false)
	{
		return expr.Kind switch
		{
			BoundKind.Call => EmitCall((BoundCall)expr, suppressTarget, isStatement), 
			BoundKind.Literal => EmitLiteral((BoundLiteral)expr), 
			BoundKind.Local => EmitLocal((BoundLocal)expr, suppressTarget), 
			BoundKind.Parameter => EmitParameter((BoundParameter)expr, ignoreByRef: false, suppressTarget), 
			BoundKind.ReturnValue => EmitReturnValue((BoundReturnValue)expr), 
			BoundKind.Global => EmitGlobal((BoundGlobal)expr, suppressTarget), 
			BoundKind.OptionAccess => EmitOptionAccess((BoundOptionAccess)expr), 
			BoundKind.Conversion => EmitConversion((BoundConversion)expr, suppressTarget), 
			BoundKind.UnaryOperator => EmitUnaryOperator((BoundUnaryOperator)expr), 
			BoundKind.BinaryOperator => EmitBinaryOperator((BoundBinaryOperator)expr), 
			BoundKind.TextIndexAccess => EmitTextIndexAccess((BoundTextIndexAccess)expr), 
			BoundKind.ArrayAccess => EmitArrayAccess((BoundArrayAccess)expr, suppressTarget), 
			BoundKind.FieldAccess => EmitFieldAccess((BoundFieldAccess)expr), 
			BoundKind.InListExpression => EmitInListExpression((BoundInListExpression)expr), 
			BoundKind.ApplicationObjectAccess => EmitApplicationObjectAccess((BoundApplicationObjectAccess)expr), 
			BoundKind.ParenthesizedExpression => EmitParenthesis((BoundParenthesizedExpression)expr), 
			BoundKind.TestActionAccess => EmitTestActionAccess((BoundTestActionAccess)expr), 
			BoundKind.TestFieldAccess => EmitTestFieldAccess((BoundTestFieldAccess)expr), 
			BoundKind.TestPartAccess => EmitTestPartAccess((BoundTestPartAccess)expr), 
			BoundKind.TestFilterAccess => EmitTestFilterAccess((BoundTestFilterAccess)expr), 
			BoundKind.TestFilterFieldAccess => EmitTestFilterFieldAccess((BoundTestFilterFieldAccess)expr), 
			BoundKind.ControlAccess => EmitControlAccess((BoundControlAccess)expr, suppressTarget), 
			BoundKind.QueryElementAccess => EmitQueryElementAccess((BoundQueryElementAccess)expr), 
			BoundKind.ReportDataItemAccess => EmitReportDataItemAccess((BoundReportDataItemAccess)expr), 
			BoundKind.XmlPortNodeAccess => EmitXmlPortNodeAccess((BoundXmlPortNodeAccess)expr), 
			BoundKind.ObjectAccess => EmitObjectAccess((BoundObjectAccess)expr), 
			BoundKind.QueryDataItemAccess => EmitQueryDataItemAccess((BoundQueryDataItemAccess)expr), 
			BoundKind.Temporary => EmitTemporaryAccess((BoundTemporary)expr), 
			_ => throw ExceptionUtilities.UnexpectedValue(expr.Kind), 
		};
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitQueryDataItemAccess(BoundQueryDataItemAccess dataItemAccess)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax name = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(dataItemAccess.QueryDataItemSymbol.MetadataName);
		return EmitHandleAccess(expression: (dataItemAccess.ReceiverOpt == null) ? CodeGenHelper.MemberAccessExpression(EmitGlobalScopeAccess(dataItemAccess), name) : CodeGenHelper.MemberAccessExpression(EmitExpression(dataItemAccess.ReceiverOpt), name), type: dataItemAccess.QueryDataItemSymbol.RelatedTable);
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitQueryDataItemAccessReceiver(BoundQueryElementAccess queryColumnAccess)
	{
		if (queryColumnAccess.ReceiverOpt == null)
		{
			if (!EmitBaseParent)
			{
				return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression();
			}
			return EmitDotParentObjectExpression(EmitBaseDotParentExpression());
		}
		return EmitExpression(queryColumnAccess.ReceiverOpt);
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitConditionExpression(BoundExpression expr)
	{
		return EmitConditionExpression(EmitExpression(expr), expr);
	}

	protected virtual Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitConditionExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, BoundNode node)
	{
		return expression;
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitCall(BoundCall call, bool suppressTarget, bool isStatement)
	{
		return call.Method.MethodKind switch
		{
			MethodKind.BuiltInMethod => EmitBuiltInMethodCall(call, isStatement), 
			MethodKind.Method => EmitUserMethodCall(call, isStatement), 
			MethodKind.Property => EmitPropertyCall(call, suppressTarget), 
			MethodKind.DeclareMethod => EmitDeclareMethodCall(call), 
			_ => throw ExceptionUtilities.UnexpectedValue(call.Method.MethodKind), 
		};
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitPropertyCall(BoundCall call, bool suppressTarget)
	{
		return EmitExpression(call.ReceiverOpt, suppressTarget);
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitBuiltInMethodCall(BoundCall call, bool isStatement)
	{
		IMethodCallEmitter callEmitter = call.Method.GetCallEmitter();
		if (call.IsStatement != isStatement)
		{
			call = call.Update(call.ReceiverOpt, call.Method, call.Arguments, call.EffectiveParameters, call.ResultKind, isStatement || call.IsStatement, call.Type);
		}
		if (callEmitter != null)
		{
			return callEmitter.EmitCall(this, call);
		}
		return EmitBuiltInMethodOrPropertyCall(call);
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitBuiltInMethodCallWithJokerArgumentHandling(BoundCall call)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax expression = EmitBuiltInMemberAccessExpression(call, (BuiltInMethodTypeSymbol)call.Method);
		PooledList<ArgumentSyntax> instance = PooledList<ArgumentSyntax>.GetInstance();
		try
		{
			EmitArgumentsForBuiltInMethodWithSpecialJokerHandling(call, instance);
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(expression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SeparatedList(instance)));
		}
		finally
		{
			instance.Free();
		}
	}

	internal void EmitArgumentsForBuiltInMethodWithSpecialJokerHandling(BoundCall call, List<ArgumentSyntax> arguments)
	{
		ImmutableArray<BoundExpression> arguments2 = call.Arguments;
		ImmutableArray<TypeSymbol> parameterTypes = call.EffectiveParameters.ParameterTypes;
		for (int i = 0; i < arguments2.Length; i++)
		{
			BoundExpression boundExpression = arguments2[i];
			if (parameterTypes[i].NavTypeKind != NavTypeKind.Joker)
			{
				arguments.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(EmitExpression(boundExpression)));
				continue;
			}
			BoundExpression boundExpression2 = boundExpression.SkipConversion();
			if (boundExpression2.Kind == BoundKind.FieldAccess && boundExpression2.Type.NavTypeKind == NavTypeKind.Decimal)
			{
				arguments.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(EmitFieldAccessToJokerConversion((BoundFieldAccess)boundExpression2)));
			}
			else
			{
				arguments.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(EmitExpression(boundExpression)));
			}
		}
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitFieldAccessToJokerConversion(BoundFieldAccess fieldAccess)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression = EmitFieldAccessReceiver(fieldAccess);
		TypeSymbol typeSymbol = GetFieldType(fieldAccess?.FieldSymbol) ?? fieldAccess?.Type;
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(expression, CodeGenFacts.GetFieldRefSafeMethodNameSyntax)).AddExtensionAndFieldArguments(fieldAccess).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(typeSymbol.NavTypeKind.GetNavTypeEnumExpression()));
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitBuiltInMethodOrPropertyCall(BoundCall call)
	{
		BuiltInMethodTypeSymbol builtInMethodTypeSymbol = call.Method as BuiltInMethodTypeSymbol;
		DebugAssertHelper.Assert(builtInMethodTypeSymbol != null);
		Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax memberAccessExpressionSyntax = EmitBuiltInMemberAccessExpression(call, builtInMethodTypeSymbol);
		if (builtInMethodTypeSymbol.IsProperty)
		{
			if (call.Arguments.Length == 1)
			{
				return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, memberAccessExpressionSyntax, EmitExpression(call.Arguments[0]));
			}
			return memberAccessExpressionSyntax;
		}
		Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax invocation = EmitBuiltInMethodInvocation(call, builtInMethodTypeSymbol, memberAccessExpressionSyntax);
		return EmitArguments(invocation, call);
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax EmitBuiltInMethodInvocation(BoundCall call, BuiltInMethodTypeSymbol builtInMethod, Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax memberAccess)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax invocationExpressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(memberAccess);
		if (IsExtensionMethod(builtInMethod) && call.ReceiverOpt != null && call.ReceiverOpt.Kind != BoundKind.ClassType)
		{
			invocationExpressionSyntax = invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(EmitExpression(call.ReceiverOpt)));
		}
		if (builtInMethod.AddThis)
		{
			invocationExpressionSyntax = invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()));
		}
		if (builtInMethod.AddDataError)
		{
			invocationExpressionSyntax = invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(call.IsStatement ? CodeGenFacts.DataErrorThrowErrorExpression : CodeGenFacts.DataErrorTrapErrorExpression));
		}
		if (builtInMethod.AddCompilationTarget)
		{
			CompilationTarget target = CompilationOptions.Target;
			invocationExpressionSyntax = invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(EmitCompilationTarget(target)));
		}
		return invocationExpressionSyntax;
	}

	private static bool IsExtensionMethod(BuiltInMethodTypeSymbol builtInMethod)
	{
		if (!string.IsNullOrEmpty(builtInMethod.StaticClassName))
		{
			return !builtInMethod.IsStatic;
		}
		return false;
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax EmitBuiltInMemberAccessExpression(BoundCall call, BuiltInMethodTypeSymbol builtinMethod)
	{
		if (!string.IsNullOrEmpty(builtinMethod.StaticClassName))
		{
			return CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(builtinMethod.StaticClassName), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(builtinMethod.MetadataName));
		}
		bool suppressTarget = builtinMethod.SuppressTarget || HasSuppressTarget(call);
		return CodeGenHelper.MemberAccessExpression((call.ReceiverOpt != null) ? EmitExpression(call.ReceiverOpt, suppressTarget) : EmitDotParentObjectExpression(EmitGlobalScopeAccess(call)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(builtinMethod.MetadataName));
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitCompilationTarget(CompilationTarget target)
	{
		return target switch
		{
			CompilationTarget.Personalization => (Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax)CodeGenFacts.CompilationTargetPersonalization, 
			CompilationTarget.Extension => CodeGenFacts.CompilationTargetExtension, 
			CompilationTarget.Solution => CodeGenFacts.CompilationTargetSolution, 
			CompilationTarget.Internal => CodeGenFacts.CompilationTargetInternal, 
			_ => throw ExceptionUtilities.UnexpectedValue(target), 
		};
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitBuiltInMethodArgument(BoundExpression argument)
	{
		return EmitExpression(argument);
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitUserMethodCall(BoundCall call, bool isStatement)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expressionSyntax = (ShouldEmitLocalCall(call) ? EmitUserLocalMethodCall(call) : EmitUserOnInvokeMethodCall(call, isStatement));
		if (!isStatement && call.Method.IsTryFunction)
		{
			expressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("TryInvoke")).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedLambdaExpression(expressionSyntax)));
		}
		return expressionSyntax;
	}

	protected virtual bool ShouldEmitLocalCall(BoundCall call)
	{
		if (call.ReceiverOpt != null)
		{
			return false;
		}
		ApplicationObjectSyntax applicationObjectSyntax = call.Method.GetFirstSyntax()?.GetContainingApplicationObjectSyntax();
		if (applicationObjectSyntax == null)
		{
			return false;
		}
		return applicationObjectSyntax == call.Syntax.GetContainingApplicationObjectSyntax();
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax EmitUserLocalMethodCall(BoundCall call)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax invocation = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression((call.ReceiverOpt == null) ? EmitGlobalScopeAccess(call) : EmitExpression(call.ReceiverOpt), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(call.Method.MetadataName)));
		return EmitArguments(invocation, call);
	}

	protected internal static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitBaseDotParentExpression()
	{
		return CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BaseExpression(), CodeGenFacts.NavScopeParentPropertyNameSyntax);
	}

	protected internal static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitBaseDotParentObjectExpression()
	{
		return CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BaseExpression(), CodeGenFacts.ParentObjectPropertyNameSyntax);
	}

	protected internal virtual Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitDotParentObjectExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		return expression;
	}

	protected internal virtual Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitDotParentExpression()
	{
		return EmitBaseDotParentObjectExpression();
	}

	protected virtual Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitGlobalScopeAccess(BoundExpression node)
	{
		if (IsExpressionNestedAndAccessingParent(node))
		{
			return EmitAccessToParentScopeThroughCurrObject(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), node);
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression();
	}

	protected static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitAccessToParentScopeThroughCurrObject(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax scopeAccess, BoundExpression node)
	{
		string currObjectName = GetContainingTypeSymbolOfNestedExpression(node).GetCurrObjectName();
		return CodeGenHelper.MemberAccessExpression(scopeAccess, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(currObjectName));
	}

	protected static bool IsExpressionNestedAndAccessingParent(BoundExpression node)
	{
		if (!node.Syntax.GetContainingObjectSyntax().Kind.IsNestedObjectSyntaxKind())
		{
			return false;
		}
		NavTypeKind navTypeKind = GetContainingTypeSymbolOfNestedExpression(node).NavTypeKind;
		if ((uint)(navTypeKind - 393319) <= 1u)
		{
			return true;
		}
		return false;
	}

	private static TypeSymbol GetContainingTypeSymbolOfNestedExpression(BoundExpression node)
	{
		TypeSymbol containingType = node.ExpressionSymbol.ContainingType;
		NavTypeKind navTypeKind = containingType.NavTypeKind;
		if ((uint)(navTypeKind - 393319) <= 1u)
		{
			return containingType;
		}
		return node.ExpressionSymbol.GetContainingSymbolOfType<ObjectTypeSymbol>();
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax EmitArguments(Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax invocation, BoundCall call)
	{
		if (call.Arguments.Length != 0)
		{
			ArrayBuilder<ArgumentSyntax> instance = ArrayBuilder<ArgumentSyntax>.GetInstance();
			for (int i = 0; i < call.Arguments.Length; i++)
			{
				ParameterSymbol parameterSymbol = call.EffectiveParameters.Parameters[i];
				if (parameterSymbol.IsMember && !parameterSymbol.IsSameVar)
				{
					BoundExpression boundExpression = call.Arguments[i].SkipConversion();
					BoundKind kind = boundExpression.Kind;
					if (kind != BoundKind.FieldAccess)
					{
						throw ExceptionUtilities.UnexpectedValue(boundExpression.Kind);
					}
					BoundFieldAccess boundFieldAccess = (BoundFieldAccess)boundExpression;
					if (boundFieldAccess.ReceiverOpt != null)
					{
						instance.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(EmitExpression(boundFieldAccess.ReceiverOpt)));
					}
					else
					{
						instance.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.MemberAccessExpression(CodeGenHelper.MemberAccessExpression(EmitBaseDotParentExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(CodeGenFacts.RecFieldIdentifier)), CodeGenFacts.TargetPropertyNameSyntax)));
					}
				}
				instance.Add(EmitArgument(parameterSymbol, call.Arguments[i]));
				if (parameterSymbol.AddExpectedType)
				{
					TypeSymbol memberType = call.Arguments[i].SkipConversion().MemberType;
					instance.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(memberType.NavTypeKind.GetNavTypeEnumExpression()));
				}
			}
			invocation = invocation.AddArgumentListArguments(instance.ToArrayAndFree());
		}
		return invocation;
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitUserOnInvokeMethodCall(BoundCall call, bool isStatement)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression;
		if (call.Arguments.Length == 0)
		{
			expression = CodeGenFacts.EmptyObjectArrayCreation;
		}
		else
		{
			ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax> instance = ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax>.GetInstance();
			for (int i = 0; i < call.Arguments.Length; i++)
			{
				instance.Add(EmitArgumentExpression(call.Method.Parameters[i], call.Arguments[i]));
			}
			expression = CodeGenFacts.ObjectArrayCreation.WithInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InitializerExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ArrayInitializerExpression).AddExpressions(instance.ToArrayAndFree()));
		}
		bool suppressTarget = HasSuppressTarget(call);
		Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax invocationExpressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression((call.ReceiverOpt != null) ? EmitExpression(call.ReceiverOpt, suppressTarget) : EmitDotParentExpression(), CodeGenFacts.InvokeMethodNameSyntax));
		ApplicationObjectExtensionTypeSymbol parentExtensionTypeSymbol = call.Method.GetParentExtensionTypeSymbol();
		if (parentExtensionTypeSymbol != null)
		{
			invocationExpressionSyntax = invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(parentExtensionTypeSymbol.Id)));
		}
		invocationExpressionSyntax = invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(call.Method.Id)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expressionSyntax = invocationExpressionSyntax;
		if (!isStatement && !call.Method.IsTryFunction && call.Method.HasReturnValue())
		{
			expressionSyntax = EmitObjectToTypeConversion(expressionSyntax, call.Method.ReturnType);
		}
		return expressionSyntax;
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitDeclareMethodCall(BoundCall call)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression;
		if (call.Arguments.Length == 0)
		{
			expression = CodeGenFacts.EmptyObjectArrayCreation;
		}
		else
		{
			ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax> instance = ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax>.GetInstance();
			for (int i = 0; i < call.Arguments.Length; i++)
			{
				instance.Add(EmitArgumentExpression(call.Method.Parameters[i], call.Arguments[i]));
			}
			expression = CodeGenFacts.ObjectArrayCreation.WithInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InitializerExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ArrayInitializerExpression).AddExpressions(instance.ToArrayAndFree()));
		}
		GenericNameSyntax name = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.GenericName("InvokeMethod").AddTypeArgumentListArguments(call.Type.NavTypeKind.RuntimeTypeSyntax());
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(EmitControlAddInAccess(call.ReceiverOpt), name)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(call.Method.MetadataName)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression((uint)call.Method.Id)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitControlAddInAccess(BoundExpression expression)
	{
		if (expression is BoundControlAccess boundControlAccess)
		{
			return CodeGenHelper.MemberAccessExpression(EmitBaseDotParentExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(boundControlAccess.ControlSymbol.MetadataName.MangleUserDefinedIdentifier()));
		}
		return EmitExpression(expression);
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitArgumentExpression(ParameterSymbol parameterSymbol, BoundExpression argumentExpression)
	{
		BuiltInParameterSymbol builtInParameterSymbol = parameterSymbol as BuiltInParameterSymbol;
		if (builtInParameterSymbol != null)
		{
			if (builtInParameterSymbol.IsMemberReference || builtInParameterSymbol.IsMember)
			{
				argumentExpression = argumentExpression.SkipConversion();
				return argumentExpression.Kind switch
				{
					BoundKind.FieldAccess => (Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax)CodeGenHelper.LiteralExpression(((BoundFieldAccess)argumentExpression).FieldSymbol.Id), 
					BoundKind.QueryElementAccess => CodeGenHelper.LiteralExpression(((BoundQueryElementAccess)argumentExpression).QueryElementSymbol.Id), 
					BoundKind.TestFieldAccess => CodeGenHelper.LiteralExpression(((BoundTestFieldAccess)argumentExpression).FieldSymbol.Id), 
					BoundKind.TestFilterFieldAccess => CodeGenHelper.LiteralExpression(((BoundTestFilterFieldAccess)argumentExpression).TestFilterField.RelatedField.Id), 
					_ => throw ExceptionUtilities.UnexpectedValue(argumentExpression.Kind), 
				};
			}
			if (argumentExpression.Kind == BoundKind.Conversion)
			{
				BoundConversion boundConversion = (BoundConversion)argumentExpression;
				if (boundConversion.Type.NavTypeKind == boundConversion.Operand.Type.NavTypeKind && (boundConversion.ConversionKind == ConversionKind.ImplicitText || boundConversion.ConversionKind == ConversionKind.ImplicitTextToSmaller))
				{
					argumentExpression = boundConversion.Operand;
				}
			}
		}
		else
		{
			if (ShouldSkipToOptionConversion(parameterSymbol, argumentExpression))
			{
				argumentExpression = argumentExpression.SkipConversion();
				return EmitToOptionConversionOrdinal(argumentExpression, argumentExpression.ConstantValue);
			}
			if (IsBoxingConversionToNavDotNet(parameterSymbol, argumentExpression))
			{
				return EmitBoxingConversion((BoundConversion)argumentExpression);
			}
		}
		bool suppressTarget = builtInParameterSymbol == null;
		if (!parameterSymbol.ShouldBePassedByRef())
		{
			return EmitExpression(argumentExpression, suppressTarget);
		}
		return EmitByRefExpression(parameterSymbol.Type, argumentExpression);
	}

	private static bool IsBoxingConversionToNavDotNet(ParameterSymbol parameterSymbol, BoundExpression argumentExpression)
	{
		if (!parameterSymbol.IsVar && parameterSymbol.Type.Kind == SymbolKind.DotNet)
		{
			return argumentExpression.Kind == BoundKind.Conversion;
		}
		return false;
	}

	private static bool ShouldSkipToOptionConversion(ParameterSymbol parameterSymbol, BoundExpression argumentExpression)
	{
		if (parameterSymbol.IsVar || parameterSymbol.Type.NavTypeKind != NavTypeKind.Option)
		{
			return false;
		}
		if (argumentExpression.Kind != BoundKind.Conversion)
		{
			return true;
		}
		return ((BoundConversion)argumentExpression).Operand.Type.Kind != SymbolKind.DotNet;
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitByRefExpression(TypeSymbol parameterType, BoundExpression argumentExpression)
	{
		BoundConversion boundConversion = argumentExpression as BoundConversion;
		BoundParameter boundParameter;
		if (boundConversion != null)
		{
			if (boundConversion.Type.NavTypeKind == NavTypeKind.Joker)
			{
				argumentExpression = boundConversion.Operand;
			}
			boundParameter = boundConversion.Operand as BoundParameter;
		}
		else
		{
			boundParameter = argumentExpression as BoundParameter;
		}
		if (boundParameter != null && boundParameter.ParameterSymbol.IsVar && CanByRefParameterBeForwarded(boundParameter, argumentExpression))
		{
			return EmitParameter(boundParameter, ignoreByRef: true);
		}
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax body = EmitExpression(argumentExpression);
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax sourceSyntax = CodeGenFacts.ByRefSetValueNameSyntax;
		if (boundConversion != null)
		{
			argumentExpression = boundConversion.Operand;
			sourceSyntax = EmitByRefSetValueConversion(boundConversion, CodeGenFacts.ByRefSetValueNameSyntax);
		}
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax body2 = EmitAssignmentExpression(argumentExpression.Type, argumentExpression, boundConversion?.Operand.Type ?? argumentExpression.Type, sourceSyntax);
		TypeSyntax typeArgumentForByRefObject = GetTypeArgumentForByRefObject(parameterType, argumentExpression);
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.GenericName(CodeGenFacts.ByRefTypeIdentifier).AddTypeArgumentListArguments(typeArgumentForByRefObject)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedLambdaExpression(body)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SimpleLambdaExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(CodeGenFacts.ByRefSetValueIdentifier), body2)));
	}

	private static bool CanByRefParameterBeForwarded(BoundParameter boundParameter, BoundExpression argumentExpression)
	{
		return boundParameter.ParameterSymbol.Type.NavTypeKind == argumentExpression.Type.NavTypeKind;
	}

	private static TypeSyntax GetTypeArgumentForByRefObject(TypeSymbol parameterType, BoundExpression argumentExpression)
	{
		return parameterType.NavTypeKind switch
		{
			NavTypeKind.Joker => argumentExpression.ExpressionSymbol.GetRuntimeTypeSyntax(), 
			NavTypeKind.DotNet => GetTypeArgumentForByRefDotNetObject(parameterType, argumentExpression), 
			_ => ((Symbol)parameterType).GetRuntimeTypeSyntax(), 
		};
	}

	private static TypeSyntax GetTypeArgumentForByRefDotNetObject(TypeSymbol parameterType, BoundExpression argumentExpression)
	{
		if (ConversionDotNet.IsDotNetSymbol(argumentExpression.Type) && ConversionDotNet.IsDotNetSymbol(parameterType))
		{
			return CodeGenFacts.NavDotNetType;
		}
		return GetDotNetByRefTypeArgument(ConversionDotNet.GetDotNetSpecialType(parameterType));
	}

	internal ArgumentSyntax EmitArgument(ParameterSymbol parameterSymbol, BoundExpression argumentExpression)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(EmitArgumentExpression(parameterSymbol, argumentExpression));
	}

	[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitLiteral(BoundLiteral literal)
	{
		switch (literal.ConstantValue.Discriminator)
		{
		case ConstantValueTypeDiscriminator.Char:
			return CodeGenHelper.LiteralExpression(literal.ConstantValue.CharValue);
		case ConstantValueTypeDiscriminator.String:
			return CodeGenHelper.LiteralExpression(literal.ConstantValue.StringValue);
		case ConstantValueTypeDiscriminator.Int32:
			return CodeGenHelper.LiteralExpression(literal.ConstantValue.Int32Value);
		case ConstantValueTypeDiscriminator.Int64:
			return CodeGenHelper.LiteralExpression(literal.ConstantValue.Int64Value);
		case ConstantValueTypeDiscriminator.Decimal:
			return CodeGenHelper.LiteralExpression(literal.ConstantValue.DecimalValue);
		case ConstantValueTypeDiscriminator.Boolean:
			if (!literal.ConstantValue.BooleanValue)
			{
				return CodeGenFacts.FalseLiteralExpression;
			}
			return CodeGenFacts.TrueLiteralExpression;
		case ConstantValueTypeDiscriminator.Time:
			return CodeGenFacts.NavTimeCreateCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression((uint)literal.ConstantValue.TimeValue)));
		case ConstantValueTypeDiscriminator.Date:
			return CodeGenFacts.NavDateCreateCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression((uint)literal.ConstantValue.DateValue)));
		case ConstantValueTypeDiscriminator.DateTime:
			return CodeGenFacts.NavDateTimeCreateCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression((uint)literal.ConstantValue.DateTimeValue.Ticks)));
		default:
			throw ExceptionUtilities.UnexpectedValue(literal.ConstantValue.Discriminator);
		}
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitHandleAccess(TypeSymbol type, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, bool suppressTarget = false)
	{
		if (!type.IsHandleType() || suppressTarget)
		{
			return expression;
		}
		return CodeGenHelper.MemberAccessExpression(expression, CodeGenFacts.TargetPropertyNameSyntax);
	}

	internal static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitLocal(BoundLocal local, bool suppressTarget)
	{
		NavTypeKind navTypeKind = local.Type.NavTypeKind;
		if ((uint)(navTypeKind - 2228235) <= 1u)
		{
			return EmitHandleAccess(local.Type, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(local.LocalSymbol.MetadataName), suppressTarget: true);
		}
		return EmitHandleAccess(local.Type, CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(local.LocalSymbol.MetadataName)), suppressTarget);
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitTemporaryAccess(BoundTemporary expr)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(expr.Name);
	}

	internal static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitParameter(BoundParameter parameter, bool ignoreByRef = false, bool suppressTarget = false)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax expression = CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(parameter.ParameterSymbol.MetadataName));
		if (!ignoreByRef && parameter.ParameterSymbol.ShouldBePassedByRef())
		{
			expression = CodeGenHelper.MemberAccessExpression(expression, CodeGenFacts.ByRefValuePropertyNameSyntax);
		}
		return EmitHandleAccess(parameter.Type, expression, suppressTarget);
	}

	internal static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitReturnValue(BoundReturnValue returnValue)
	{
		return EmitHandleAccess(returnValue.Type, CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(returnValue.ReturnValueSymbol.MetadataName)));
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitGlobal(BoundGlobal global, bool suppressTarget)
	{
		if (global.GlobalSymbol.VariableKind == VariableKind.Synthesized)
		{
			SynthesizedGlobalVariableSymbol synthesizedGlobalVariableSymbol = (SynthesizedGlobalVariableSymbol)global.GlobalSymbol;
			suppressTarget = suppressTarget || synthesizedGlobalVariableSymbol.SuppressTarget;
		}
		NavTypeKind navTypeKind = global.Type.NavTypeKind;
		if ((uint)(navTypeKind - 2228235) <= 1u)
		{
			return EmitHandleAccess(global.Type, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(global.GlobalSymbol.MetadataName), suppressTarget: true);
		}
		return EmitHandleAccess(global.Type, CodeGenHelper.MemberAccessExpression(EmitGlobalScopeAccess(global), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(global.GlobalSymbol.MetadataName)), suppressTarget);
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitOptionAccess(BoundOptionAccess optionAccess)
	{
		if (optionAccess.OptionSymbol.Kind == SymbolKind.EnumValue)
		{
			if (optionAccess.ReceiverOpt.Kind == BoundKind.EnumType)
			{
				return CreateNavOptionWithEnumMetadata(((BoundEnumType)optionAccess.ReceiverOpt).EnumType, optionAccess.OptionSymbol.Ordinal);
			}
			return EmitToOptionConversion(EmitToOptionConversionMetadata(optionAccess.ReceiverOpt, optionAccess.OptionSymbol.Type), CodeGenHelper.LiteralExpression(optionAccess.OptionSymbol.Ordinal));
		}
		switch (optionAccess.OptionSymbol.Type.NavTypeKind)
		{
		case NavTypeKind.EnumExtension:
		case NavTypeKind.Option:
		case NavTypeKind.Enum:
			return CodeGenHelper.LiteralExpression(optionAccess.OptionSymbol.Ordinal);
		case NavTypeKind.DotNet:
			return EmitDotNetEnumMemberAccess(optionAccess);
		default:
			if (optionAccess.OptionSymbol.Type.TypeCategory == TypeCategoryKind.Option)
			{
				return CodeGenHelper.MemberAccessExpression(optionAccess.ReceiverOpt.Type.NavTypeKind.RuntimeTypeSyntax(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(optionAccess.ExpressionSymbol.MetadataName));
			}
			throw ExceptionUtilities.UnreachableWithMessage(optionAccess.Syntax.ToReportingString());
		}
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitDotNetEnumMemberAccess(BoundOptionAccess optionAccess)
	{
		DotNetEnumMemberSymbol dotNetEnumMemberSymbol = (DotNetEnumMemberSymbol)optionAccess.OptionSymbol;
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(EmitExpression(optionAccess.ReceiverOpt), CodeGenFacts.InvokeEnumeratorMethodNameSyntax)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(dotNetEnumMemberSymbol.MetadataName)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression((uint)dotNetEnumMemberSymbol.Id)));
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitUnaryOperator(BoundUnaryOperator unaryOperator)
	{
		Microsoft.CodeAnalysis.CSharp.SyntaxKind syntaxKind;
		switch (unaryOperator.OperatorKind)
		{
		case UnaryOperatorKind.BoolLogicalNegation:
			syntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind.LogicalNotExpression;
			break;
		case UnaryOperatorKind.IntUnaryMinus:
		case UnaryOperatorKind.LongUnaryMinus:
		case UnaryOperatorKind.DecimalUnaryMinus:
			syntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind.UnaryMinusExpression;
			break;
		case UnaryOperatorKind.IntUnaryPlus:
		case UnaryOperatorKind.LongUnaryPlus:
		case UnaryOperatorKind.DecimalUnaryPlus:
			syntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind.UnaryPlusExpression;
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(unaryOperator.OperatorKind);
		}
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expressionSyntax = EmitExpression(unaryOperator.Operand);
		if (syntaxKind == Microsoft.CodeAnalysis.CSharp.SyntaxKind.UnaryPlusExpression)
		{
			return expressionSyntax;
		}
		if (unaryOperator.Operand.Kind == BoundKind.BinaryOperator)
		{
			expressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedExpression(expressionSyntax);
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PrefixUnaryExpression(syntaxKind, expressionSyntax);
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitParenthesis(BoundParenthesizedExpression node)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedExpression(EmitExpression(node.Expression));
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitBinaryOperator(BoundBinaryOperator binaryOperator)
	{
		binaryOperator = ApplyCompatibilityRewrite(binaryOperator);
		PooledStack<BoundExpression> instance = PooledStack<BoundExpression>.GetInstance();
		PooledStack<Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax> instance2 = PooledStack<Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax>.GetInstance();
		try
		{
			BoundExpression boundExpression = null;
			instance.Push(binaryOperator);
			while (!instance.IsEmpty())
			{
				BoundExpression boundExpression2 = instance.Peek();
				if (boundExpression2.Kind == BoundKind.BinaryOperator)
				{
					BoundBinaryOperator boundBinaryOperator = (BoundBinaryOperator)boundExpression2;
					if (boundBinaryOperator.Left == boundExpression)
					{
						Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax item = EmitExpressionSyntaxWithBinaryOperator(boundBinaryOperator, instance2.Pop(), instance2.Pop());
						instance2.Push(item);
						instance.Pop();
					}
					else
					{
						instance.Push(boundBinaryOperator.Left);
						instance.Push(boundBinaryOperator.Right);
					}
				}
				else
				{
					Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax item2 = EmitExpression(boundExpression2);
					instance2.Push(item2);
					instance.Pop();
				}
				boundExpression = boundExpression2;
			}
			return instance2.Pop();
		}
		finally
		{
			instance.Free();
			instance2.Free();
		}
	}

	private BoundBinaryOperator ApplyCompatibilityRewrite(BoundBinaryOperator binaryOperator)
	{
		if (binaryOperator.OperatorKind != BinaryOperatorKind.StringEqual && binaryOperator.OperatorKind != BinaryOperatorKind.StringNotEqual)
		{
			return binaryOperator;
		}
		if (binaryOperator.Left.Kind != BoundKind.Conversion && binaryOperator.Right.Kind != BoundKind.Conversion)
		{
			return binaryOperator;
		}
		BoundExpression left = binaryOperator.Left;
		BoundExpression right = binaryOperator.Right;
		if (left.Kind == BoundKind.Conversion && left is BoundConversion boundConversion && boundConversion.Operand.Type.NavTypeKind == NavTypeKind.Guid)
		{
			left = left.SkipConversion();
			right = new BoundConversion(right.Syntax, right, ConversionKind.ToGuid, NavCorLib.GuidType, null);
		}
		else
		{
			if (right.Kind != BoundKind.Conversion || !(right is BoundConversion boundConversion2) || boundConversion2.Operand.Type.NavTypeKind != NavTypeKind.Guid)
			{
				return binaryOperator;
			}
			left = new BoundConversion(left.Syntax, left, ConversionKind.ToGuid, NavCorLib.GuidType, null);
			right = right.SkipConversion();
		}
		BinaryOperatorKind operatorKind = BinaryOperatorKind.GuidEqual;
		if (binaryOperator.OperatorKind == BinaryOperatorKind.StringNotEqual)
		{
			operatorKind = BinaryOperatorKind.GuidNotEqual;
		}
		return binaryOperator.Update(operatorKind, left, right, binaryOperator.ConstantValueOpt, binaryOperator.MethodOpt, binaryOperator.ResultKind, binaryOperator.Type);
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitExpressionSyntaxWithBinaryOperator(BoundBinaryOperator binaryOperator, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax left, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax right)
	{
		Microsoft.CodeAnalysis.CSharp.SyntaxKind syntaxKind = MapBinaryOperatorKindToSyntaxKind(binaryOperator);
		if (binaryOperator.OperatorKind.Operator() == BinaryOperatorKind.Division && (binaryOperator.Right.Type.NavTypeKind != NavTypeKind.Decimal || binaryOperator.Right.Kind == BoundKind.Conversion))
		{
			if (right.Kind() == Microsoft.CodeAnalysis.CSharp.SyntaxKind.UnaryMinusExpression || right.Kind() == Microsoft.CodeAnalysis.CSharp.SyntaxKind.UnaryPlusExpression)
			{
				right = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedExpression(right);
			}
			right = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(CodeGenFacts.RuntimeDecimalType, right));
		}
		if (syntaxKind == Microsoft.CodeAnalysis.CSharp.SyntaxKind.ExclusiveOrExpression)
		{
			if (left is Microsoft.CodeAnalysis.CSharp.Syntax.BinaryExpressionSyntax binaryExpressionSyntax && binaryExpressionSyntax.Kind() == Microsoft.CodeAnalysis.CSharp.SyntaxKind.BitwiseOrExpression)
			{
				left = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedExpression(left);
			}
		}
		else if (binaryOperator.OperatorKind.IsComparison())
		{
			if (binaryOperator.Left is BoundBinaryOperator boundBinaryOperator && boundBinaryOperator.OperatorKind.IsLogical())
			{
				left = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedExpression(left);
			}
			if (binaryOperator.Right is BoundBinaryOperator boundBinaryOperator2 && boundBinaryOperator2.OperatorKind.IsLogical())
			{
				right = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedExpression(right);
			}
			if (binaryOperator.OperatorKind.OperandTypes() == BinaryOperatorKind.Bool)
			{
				switch (binaryOperator.OperatorKind)
				{
				case BinaryOperatorKind.BoolLessThan:
				case BinaryOperatorKind.BoolLessThanOrEqual:
					left = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PrefixUnaryExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.LogicalNotExpression, left);
					break;
				case BinaryOperatorKind.BoolGreaterThan:
				case BinaryOperatorKind.BoolGreaterThanOrEqual:
					right = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PrefixUnaryExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.LogicalNotExpression, right);
					break;
				}
			}
			if (ShouldUseStringHelperComparison(binaryOperator))
			{
				left = CodeGenFacts.StringHelperCompareCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(left), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(right));
				right = CodeGenHelper.LiteralExpression(0);
			}
		}
		if (IsJokerEqualityComparison(binaryOperator))
		{
			return ConstructJokerEqualityComparisionExpression(binaryOperator, left, right);
		}
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(syntaxKind, left, right);
		if (binaryOperator.OperatorKind.Operator() == BinaryOperatorKind.IDivision && binaryOperator.Type.NavTypeKind == NavTypeKind.Decimal)
		{
			expressionSyntax = CodeGenFacts.ALSystemNumericTruncateCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expressionSyntax));
		}
		return expressionSyntax;
	}

	private static bool ShouldUseStringHelperComparison(BoundBinaryOperator binaryOperator)
	{
		if (binaryOperator.OperatorKind.OperandTypes() != BinaryOperatorKind.String)
		{
			return false;
		}
		if (binaryOperator.OperatorKind.Operator() == BinaryOperatorKind.Equal || binaryOperator.OperatorKind.Operator() == BinaryOperatorKind.NotEqual)
		{
			return false;
		}
		if (IsNavStringValue(binaryOperator.Left) || IsNavStringValue(binaryOperator.Right))
		{
			return false;
		}
		return true;
	}

	private static bool IsNavStringValue(BoundExpression expression)
	{
		NavTypeKind navTypeKind = expression.Type.NavTypeKind;
		if (navTypeKind == NavTypeKind.TextConst || navTypeKind == NavTypeKind.DateFormula || (uint)(navTypeKind - 16646153) <= 1u)
		{
			return true;
		}
		if (expression.Kind == BoundKind.Conversion)
		{
			return IsNavStringValue(((BoundConversion)expression).Operand);
		}
		return false;
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax ConstructJokerEqualityComparisionExpression(BoundBinaryOperator binaryOperator, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax left, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax right)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax invocationExpressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(CodeGenFacts.ALCompilerClassNameSyntax, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("CompareNavValues")));
		invocationExpressionSyntax = invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(left), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(right));
		if (binaryOperator.OperatorKind.Operator() != BinaryOperatorKind.NotEqual)
		{
			return invocationExpressionSyntax;
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PrefixUnaryExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.LogicalNotExpression, invocationExpressionSyntax);
	}

	private static bool IsJokerEqualityComparison(BoundBinaryOperator binaryOperator)
	{
		if (binaryOperator.OperatorKind.IsEqualityComparison())
		{
			return binaryOperator.OperatorKind.OperandTypes() == BinaryOperatorKind.Joker;
		}
		return false;
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	private static Microsoft.CodeAnalysis.CSharp.SyntaxKind MapBinaryOperatorKindToSyntaxKind(BoundBinaryOperator binaryOperator)
	{
		switch (binaryOperator.OperatorKind)
		{
		case BinaryOperatorKind.BoolGreaterThanOrEqual:
		case BinaryOperatorKind.BoolLessThanOrEqual:
		case BinaryOperatorKind.LogicalBoolOr:
			return Microsoft.CodeAnalysis.CSharp.SyntaxKind.BitwiseOrExpression;
		case BinaryOperatorKind.BoolGreaterThan:
		case BinaryOperatorKind.BoolLessThan:
		case BinaryOperatorKind.LogicalBoolAnd:
			return Microsoft.CodeAnalysis.CSharp.SyntaxKind.BitwiseAndExpression;
		case BinaryOperatorKind.LogicalBoolXor:
			return Microsoft.CodeAnalysis.CSharp.SyntaxKind.ExclusiveOrExpression;
		default:
			switch (binaryOperator.OperatorKind.Operator())
			{
			case BinaryOperatorKind.Equal:
				return Microsoft.CodeAnalysis.CSharp.SyntaxKind.EqualsExpression;
			case BinaryOperatorKind.NotEqual:
				return Microsoft.CodeAnalysis.CSharp.SyntaxKind.NotEqualsExpression;
			case BinaryOperatorKind.LessThan:
				return Microsoft.CodeAnalysis.CSharp.SyntaxKind.LessThanExpression;
			case BinaryOperatorKind.LessThanOrEqual:
				return Microsoft.CodeAnalysis.CSharp.SyntaxKind.LessThanOrEqualExpression;
			case BinaryOperatorKind.GreaterThan:
				return Microsoft.CodeAnalysis.CSharp.SyntaxKind.GreaterThanExpression;
			case BinaryOperatorKind.GreaterThanOrEqual:
				return Microsoft.CodeAnalysis.CSharp.SyntaxKind.GreaterThanOrEqualExpression;
			case BinaryOperatorKind.Addition:
				return Microsoft.CodeAnalysis.CSharp.SyntaxKind.AddExpression;
			case BinaryOperatorKind.Subtraction:
				return Microsoft.CodeAnalysis.CSharp.SyntaxKind.SubtractExpression;
			case BinaryOperatorKind.Multiplication:
				return Microsoft.CodeAnalysis.CSharp.SyntaxKind.MultiplyExpression;
			case BinaryOperatorKind.Division:
			case BinaryOperatorKind.IDivision:
				return Microsoft.CodeAnalysis.CSharp.SyntaxKind.DivideExpression;
			case BinaryOperatorKind.Remainder:
				return Microsoft.CodeAnalysis.CSharp.SyntaxKind.ModuloExpression;
			default:
				throw ExceptionUtilities.UnexpectedValue(binaryOperator.OperatorKind);
			}
		}
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitTextIndexAccess(BoundTextIndexAccess textIndexAccess)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ElementAccessExpression(EmitExpression(textIndexAccess.Expression)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SubtractExpression, EmitExpression(textIndexAccess.Index), CodeGenHelper.LiteralExpression(1))));
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitArrayAccess(BoundArrayAccess arrayAccess, bool suppressTarget)
	{
		ArrayBuilder<ArgumentSyntax> instance = ArrayBuilder<ArgumentSyntax>.GetInstance();
		BoundExpression boundExpression = BuildIndicesList(arrayAccess, instance);
		ArrayTypeSymbol arrayTypeSymbol = boundExpression.Type as ArrayTypeSymbol;
		DebugAssertHelper.Assert(arrayTypeSymbol != null);
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression = EmitExpression(boundExpression);
		if (instance.Count != arrayTypeSymbol.Dimensions.Length)
		{
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(expression, CodeGenFacts.GetSubArrayMethodNameSyntax)).AddArgumentListArguments(instance.ToArrayAndFree());
		}
		return EmitHandleAccess(arrayTypeSymbol.ElementType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ElementAccessExpression(expression).AddArgumentListArguments(instance.ToArrayAndFree()), suppressTarget);
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitFieldAccess(BoundFieldAccess fieldAccess)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression = EmitFieldAccessReceiver(fieldAccess);
		TypeSymbol typeSymbol = GetFieldType(fieldAccess?.FieldSymbol) ?? fieldAccess?.Type;
		return EmitFastJokerToTypeConversion(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(expression, CodeGenFacts.GetFieldValueSafeMethodNameSyntax)).AddExtensionAndFieldArguments(fieldAccess).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(typeSymbol.NavTypeKind.GetNavTypeEnumExpression())), typeSymbol);
	}

	private static TypeSymbol GetFieldType(FieldSymbol field)
	{
		if (field != null && IsOemTextFieldOnObjectTable(field))
		{
			return TypeSymbol.GetNavTypeSymbol(NavTypeKind.OemText);
		}
		return field?.Type;
	}

	private static bool IsOemTextFieldOnObjectTable(FieldSymbol field)
	{
		if (!field.ContainingSymbol.IsKind(SymbolKind.Table) || ((TableTypeSymbol)field.ContainingSymbol).Id != 2000000001)
		{
			return false;
		}
		switch (field.Id)
		{
		case 2:
		case 4:
		case 12:
		case 50:
			return true;
		default:
			return false;
		}
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitFieldAccessReceiver(BoundFieldAccess fieldAccess)
	{
		if (fieldAccess?.ReceiverOpt == null)
		{
			if (!EmitBaseParent)
			{
				return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression();
			}
			return EmitDotParentObjectExpression(EmitBaseDotParentExpression());
		}
		return EmitExpression(fieldAccess.ReceiverOpt);
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitQueryElementAccess(BoundQueryElementAccess queryElementAccess, bool isOptionValueAccess = false)
	{
		TypeSymbol type = queryElementAccess.QueryElementSymbol.Type;
		if (type.NavTypeKind == NavTypeKind.QueryFilter)
		{
			type = queryElementAccess.QueryElementSymbol.RelatedField.Type;
		}
		Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax name = CodeGenFacts.GetColumnValueSafeMethodNameSyntax;
		if (isOptionValueAccess)
		{
			name = CodeGenFacts.GetColumnOptionSafeMethodNameSyntax;
		}
		return EmitFastJokerToTypeConversion(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(EmitQueryColumnAccessReceiver(queryElementAccess), name)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(queryElementAccess.QueryElementSymbol.Id)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(type.NavTypeKind.GetNavTypeEnumExpression())), type);
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitQueryColumnAccessReceiver(BoundQueryElementAccess queryColumnAccess)
	{
		if (queryColumnAccess.ReceiverOpt == null)
		{
			if (!EmitBaseParent)
			{
				return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression();
			}
			return EmitDotParentObjectExpression(EmitBaseDotParentExpression());
		}
		return EmitExpression(queryColumnAccess.ReceiverOpt);
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitReportDataItemAccess(BoundReportDataItemAccess reportDataItemAccess)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax name = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(reportDataItemAccess.ReportDataItemSymbol.MetadataName);
		return EmitHandleAccess(expression: (reportDataItemAccess.ReceiverOpt == null) ? CodeGenHelper.MemberAccessExpression(EmitGlobalScopeAccess(reportDataItemAccess), name) : CodeGenHelper.MemberAccessExpression(EmitExpression(reportDataItemAccess.ReceiverOpt), name), type: reportDataItemAccess.ReportDataItemSymbol.RelatedTable);
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitXmlPortNodeAccess(BoundXmlPortNodeAccess expr)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax name = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(expr.XmlPortNodeSymbol.MetadataName);
		return EmitHandleAccess(expression: (expr.ReceiverOpt == null) ? CodeGenHelper.MemberAccessExpression(EmitGlobalScopeAccess(expr), name) : CodeGenHelper.MemberAccessExpression(EmitExpression(expr.ReceiverOpt), name), type: expr.XmlPortNodeSymbol.Type.BaseType);
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitInListExpression(BoundInListExpression inList)
	{
		BoundKind kind = inList.Expression.Kind;
		if (kind - 3 <= BoundKind.Local)
		{
			return EmitComparisonExpression(EmitExpression(inList.Expression), inList.Expression, inList.List);
		}
		Microsoft.CodeAnalysis.SyntaxToken identifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("candidate");
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(inList.Expression.Type.NavTypeKind.IsNavValueType() ? CodeGenFacts.InListNavValueType : Microsoft.CodeAnalysis.CSharp.SyntaxFactory.GenericName("InList").AddTypeArgumentListArguments(((Symbol)inList.Expression.Type).GetRuntimeTypeSyntax())).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SimpleLambdaExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(identifier), EmitComparisonExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(identifier), inList.Expression, inList.List))))).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(EmitExpression(inList.Expression)));
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitComparisonExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax emittedComparator, BoundExpression comparator, IList<BoundExpression> comparatorList)
	{
		bool flag = comparator.Type.NavTypeKind.SupportsNativeComparison();
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expressionSyntax = null;
		for (int i = 0; i < comparatorList.Count; i++)
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expressionSyntax2 = (flag ? EmitInListNativeComparison(emittedComparator, comparatorList[i]) : EmitInListCompareToComparison(emittedComparator, comparatorList[i]));
			expressionSyntax = ((expressionSyntax == null) ? expressionSyntax2 : Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.LogicalOrExpression, expressionSyntax, expressionSyntax2));
		}
		return expressionSyntax;
	}

	private static Func<BoundExpression, BoundExpression> ExtractConversionFromListItem(ref BoundExpression inListItem)
	{
		if (inListItem.Kind == BoundKind.Conversion)
		{
			BoundConversion conversion = inListItem as BoundConversion;
			DebugAssertHelper.Assert(conversion != null, "conversion != null");
			inListItem = conversion.Operand;
			return (BoundExpression value) => new BoundConversion(value.Syntax, value, conversion.ConversionKind, LookupResultKind.Viable, conversion.SymbolOpt, conversion.TargetOpt, conversion.IsArrayIndex, conversion.ConstantValueOpt, conversion.Type, conversion.HasErrors);
		}
		return (BoundExpression value) => value;
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitApplicationObjectAccess(BoundApplicationObjectAccess expr)
	{
		return CodeGenHelper.LiteralExpression(expr.ApplicationObjectTypeSymbol.Id);
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitObjectAccess(BoundObjectAccess expr)
	{
		return CodeGenHelper.MemberAccessExpression(EmitGlobalScopeAccess(expr), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(expr.ObjectTypeSymbol.MetadataName));
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitTestActionAccess(BoundTestActionAccess expr)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(EmitExpression(expr.ReceiverOpt), CodeGenFacts.GetActionMethodNameSyntax)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(expr.ActionSymbol.Id)));
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitTestFieldAccess(BoundTestFieldAccess expr)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(EmitExpression(expr.ReceiverOpt), CodeGenFacts.GetFieldMethodNameSyntax)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(expr.FieldSymbol.Id)));
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitTestPartAccess(BoundTestPartAccess expr)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(EmitExpression(expr.ReceiverOpt), CodeGenFacts.GetPartMethodNameSyntax)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(expr.PartSymbol.Id)));
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitTestFilterAccess(BoundTestFilterAccess expr)
	{
		if (expr.TestFilter.RelatedPage.IsKind(SymbolKind.RequestPage))
		{
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(EmitExpression(expr.ReceiverOpt), CodeGenFacts.GetDataItemMethodNameSyntax)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(expr.TestFilter.Id)));
		}
		return CodeGenHelper.MemberAccessExpression(EmitExpression(expr.ReceiverOpt), CodeGenFacts.ALFilterPropertyNameSyntax);
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitTestFilterFieldAccess(ITestFilterFieldAccess expr)
	{
		return CodeGenHelper.LiteralExpression(expr.FieldSymbol.Id);
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitControlAccess(BoundControlAccess expr, bool suppressTarget)
	{
		switch (expr.ControlSymbol.ControlKind)
		{
		case ControlKind.Part:
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(EmitExpression(expr.ReceiverOpt), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("GetPart"))).WithArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SingletonSeparatedList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(expr.ControlSymbol.Id))))), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("CreateNavFormHandle")), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SingletonSeparatedList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()))));
			if (!suppressTarget)
			{
				expressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleMemberAccessExpression, expressionSyntax, CodeGenFacts.TargetPropertyNameSyntax);
			}
			return expressionSyntax;
		}
		case ControlKind.UserControl:
			return CodeGenHelper.MemberAccessExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BaseExpression(), CodeGenFacts.NavScopeParentPropertyNameSyntax), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(expr.ControlSymbol.MetadataName.MangleUserDefinedIdentifier()));
		default:
			throw ExceptionUtilities.UnexpectedValue(expr.ControlSymbol.ControlKind);
		}
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitInListNativeComparison(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax candidate, BoundExpression expression)
	{
		Func<BoundExpression, BoundExpression> func = ExtractConversionFromListItem(ref expression);
		if (expression.Kind == BoundKind.BinaryOperator)
		{
			BoundBinaryOperator boundBinaryOperator = expression as BoundBinaryOperator;
			DebugAssertHelper.Assert(boundBinaryOperator != null, "binaryOp != null");
			if (boundBinaryOperator.OperatorKind.Operator() == BinaryOperatorKind.Range)
			{
				return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.LogicalAndExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GreaterThanOrEqualExpression, candidate, EmitExpression(func(boundBinaryOperator.Left))), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.LessThanOrEqualExpression, candidate, EmitExpression(func(boundBinaryOperator.Right)))));
			}
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.EqualsExpression, candidate, EmitExpression(func(expression))));
	}

	protected internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitAssignmentExpression(TypeSymbol type, BoundExpression target, TypeSymbol sourceType, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax sourceSyntax)
	{
		if (CodeGenFacts.UseAssignMethod(type))
		{
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(EmitExpression(target, suppressTarget: true), CodeGenFacts.ALAssignMethodNameSyntax)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(sourceSyntax));
		}
		return target.Kind switch
		{
			BoundKind.TextIndexAccess => EmitTextIndexAccessAssignmentExpression((BoundTextIndexAccess)target, sourceSyntax), 
			BoundKind.FieldAccess => EmitFieldAccessAssignmentExpression((BoundFieldAccess)target, sourceType, sourceSyntax), 
			_ => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, EmitExpression(target, suppressTarget: true), sourceSyntax), 
		};
	}

	protected internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitTextIndexAccessAssignmentExpression(BoundTextIndexAccess textIndex, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax source)
	{
		DebugAssertHelper.Assert(textIndex.Type.NavTypeKind == NavTypeKind.Char);
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expressionSyntax = EmitExpression(textIndex.Expression);
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression = ((source is Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax) ? EmitToNumericConversion(NavTypeKind.Char, source) : source);
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expressionSyntax2 = CodeGenHelper.CreateNavStringValue(textIndex, expressionSyntax, CodeGenFacts.ALSystemStringSetCharCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expressionSyntax), BuildArrayIndex(textIndex.Index), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression)));
		BoundExpression expression2 = textIndex.Expression;
		switch (expression2.Kind)
		{
		case BoundKind.FieldAccess:
			return EmitFieldAccessAssignmentExpression((BoundFieldAccess)expression2, expression2.Type, expressionSyntax2);
		case BoundKind.Local:
		case BoundKind.Global:
		case BoundKind.Parameter:
		case BoundKind.ReturnValue:
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, expressionSyntax, expressionSyntax2);
		default:
			throw ExceptionUtilities.UnexpectedValue(expression2.Kind);
		}
	}

	protected internal Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitFieldAccessAssignmentExpression(BoundFieldAccess fieldAccess, TypeSymbol type, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax source)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax invocationExpressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(EmitFieldAccessReceiver(fieldAccess), CodeGenFacts.SetFieldValueSafeMethodNameSyntax)).AddExtensionAndFieldArguments(fieldAccess);
		TypeSymbol fieldType = GetFieldType(fieldAccess.FieldSymbol);
		return invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(fieldType.NavTypeKind.GetNavTypeEnumExpression()), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(EmitToJokerConversion(type, source)));
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitInListCompareToComparison(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax candidate, BoundExpression expression)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax invocationExpressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(candidate, CodeGenFacts.CompareToMethodNameSyntax));
		Func<BoundExpression, BoundExpression> func = ExtractConversionFromListItem(ref expression);
		if (expression.Kind == BoundKind.BinaryOperator)
		{
			BoundBinaryOperator boundBinaryOperator = expression as BoundBinaryOperator;
			DebugAssertHelper.Assert(boundBinaryOperator != null, "binaryOp != null");
			if (boundBinaryOperator.OperatorKind.Operator() == BinaryOperatorKind.Range)
			{
				return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.LogicalAndExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GreaterThanOrEqualExpression, invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(EmitExpression(func(boundBinaryOperator.Left)))), CodeGenHelper.LiteralExpression(0)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.LessThanOrEqualExpression, invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(EmitExpression(func(boundBinaryOperator.Right)))), CodeGenHelper.LiteralExpression(0))));
			}
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.EqualsExpression, invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(EmitExpression(func(expression)))), CodeGenHelper.LiteralExpression(0)));
	}

	private static bool HasSuppressTarget(BoundCall call)
	{
		if (call.ReceiverOpt != null && call.ReceiverOpt.Kind == BoundKind.Global && call.ReceiverOpt.Type == call.Method.ContainingType)
		{
			return !call.ReceiverOpt.Type.IsHandleType();
		}
		return false;
	}

	private BoundExpression BuildIndicesList(BoundArrayAccess arrayAccess, ArrayBuilder<ArgumentSyntax> builder)
	{
		BoundExpression result = arrayAccess.Expression;
		if (arrayAccess.Expression.Kind == BoundKind.ArrayAccess)
		{
			result = BuildIndicesList((BoundArrayAccess)arrayAccess.Expression, builder);
		}
		foreach (BoundExpression index in arrayAccess.Indices)
		{
			builder.Add(BuildArrayIndex(index));
		}
		return result;
	}

	internal ArgumentSyntax BuildArrayIndex(BoundExpression index)
	{
		if (index.Kind == BoundKind.Literal && index.ConstantValue.IsIntegral)
		{
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(index.ConstantValue.Int32Value - 1));
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SubtractExpression, EmitExpression(index), CodeGenHelper.LiteralExpression(1)));
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "call")]
	internal static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax CreateAutomationId(BoundCall call)
	{
		BoundExpression receiverOpt = call.ReceiverOpt;
		if (receiverOpt == null || receiverOpt.ExpressionSymbol.GetNavTypeKind() != NavTypeKind.Dialog)
		{
			return CodeGenFacts.GuidEmptyExpression;
		}
		string value = "a18a31bd-d3fc-e30e-a31f-d9fa3b27ce27";
		if (call.ReceiverOpt != null)
		{
			PooledStringBuilder instance = PooledStringBuilder.GetInstance();
			try
			{
				StringBuilder builder = instance.Builder;
				Symbol receiverOperatorsSymbol = GetReceiverOperatorsSymbol(call);
				if (receiverOperatorsSymbol != null)
				{
					builder.Append(receiverOperatorsSymbol.ContainingType.Name);
				}
				if (receiverOperatorsSymbol is SourceGlobalVariableSymbol || receiverOperatorsSymbol is ReferenceGlobalVariableSymbol)
				{
					builder.Append(receiverOperatorsSymbol.Name);
				}
				else if (receiverOperatorsSymbol is VariableSymbol)
				{
					Symbol symbol = receiverOperatorsSymbol;
					while (symbol != null && symbol.ContainingSymbol != symbol)
					{
						builder.Append(symbol.Name);
						symbol = symbol.ContainingSymbol;
					}
				}
				short kindShort = GetKindShort(receiverOperatorsSymbol);
				value = new Guid(Hash.GetFNVHashCode(builder.ToString()), kindShort, 3, 5, 7, 11, 13, 17, 19, 23, 29).ToString();
			}
			finally
			{
				instance.Free();
			}
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("Guid"), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("Parse"))).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(value)));
	}

	private static Symbol GetReceiverOperatorsSymbol(BoundCall call)
	{
		if (call.ReceiverOpt.Kind == BoundKind.ArrayAccess)
		{
			return ((BoundArrayAccess)call.ReceiverOpt).Expression?.ExpressionSymbol;
		}
		return call.ReceiverOpt.ExpressionSymbol;
	}

	private static short GetKindShort(Symbol receiverOptSymbol)
	{
		return ((short?)receiverOptSymbol?.ContainingType?.Kind) ?? 2;
	}

	internal static TypeSyntax EmitVariableType(VariableSymbol variable, bool asGlobal = false)
	{
		return variable.GetRuntimeTypeSyntax(asGlobal);
	}

	internal static PropertyDeclarationSyntax EmitBooleanPropertyOverloading(Microsoft.CodeAnalysis.SyntaxToken name, bool isTrue)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax expression = (isTrue ? CodeGenFacts.TrueLiteralExpression : CodeGenFacts.FalseLiteralExpression);
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PropertyDeclaration(CodeGenFacts.ClrBoolType, name).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OverrideKeyword)).AddAccessorListAccessors(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AccessorDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(expression))));
	}

	static CodeGenerator()
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax[,] array = new Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax[22, 20];
		array[1, 0] = CodeGenFacts.ALCompilerObjectToBooleanCall;
		array[1, 1] = CodeGenFacts.ALCompilerObjectToByteCall;
		array[1, 2] = CodeGenFacts.ALCompilerObjectToCharCall;
		array[1, 3] = SpecialConversion;
		array[1, 4] = CodeGenFacts.ALCompilerObjectToIntegerCall;
		array[1, 5] = CodeGenFacts.ALCompilerObjectToBigIntegerCall;
		array[1, 6] = CodeGenFacts.ALCompilerObjectToDecimalCall;
		array[1, 7] = SpecialConversion;
		array[1, 8] = SpecialConversion;
		array[1, 9] = SpecialConversion;
		array[1, 10] = SpecialConversion;
		array[1, 11] = SpecialConversion;
		array[1, 12] = SpecialConversion;
		array[1, 13] = SpecialConversion;
		array[1, 14] = SpecialConversion;
		array[1, 15] = CodeGenFacts.ALCompilerObjectToGuidCall;
		array[1, 16] = IdentityConversion;
		array[1, 17] = SpecialConversion;
		array[1, 19] = CodeGenFacts.NavBigTextCreateFromObjectCall;
		array[2, 1] = CodeGenFacts.ALCompilerToByteCall;
		array[2, 2] = CodeGenFacts.ALCompilerDotNetToCharCall;
		array[2, 3] = SpecialConversion;
		array[2, 4] = CodeGenFacts.ALCompilerToIntegerCall;
		array[2, 5] = CodeGenFacts.ALCompilerToBigIntegerCall;
		array[2, 13] = SpecialConversion;
		array[2, 14] = SpecialConversion;
		array[2, 16] = IdentityConversion;
		array[4, 0] = IdentityConversion;
		array[4, 16] = IdentityConversion;
		array[5, 1] = CodeGenFacts.ALCompilerToByteCall;
		array[5, 2] = IdentityConversion;
		array[5, 4] = IdentityConversion;
		array[5, 5] = IdentityConversion;
		array[5, 6] = CodeGenFacts.ALCompilerToDecimalCall;
		array[5, 7] = IdentityConversion;
		array[5, 16] = IdentityConversion;
		array[6, 1] = CodeGenFacts.ALCompilerToByteCall;
		array[6, 2] = CodeGenFacts.ALCompilerToCharCall;
		array[6, 4] = IdentityConversion;
		array[6, 5] = IdentityConversion;
		array[6, 6] = CodeGenFacts.ALCompilerToDecimalCall;
		array[6, 7] = IdentityConversion;
		array[6, 16] = IdentityConversion;
		array[7, 1] = IdentityConversion;
		array[7, 2] = CodeGenFacts.ALCompilerToCharCall;
		array[7, 3] = SpecialConversion;
		array[7, 4] = IdentityConversion;
		array[7, 5] = IdentityConversion;
		array[7, 6] = CodeGenFacts.ALCompilerToDecimalCall;
		array[7, 7] = CodeGenFacts.ALCompilerToDurationCall;
		array[7, 16] = IdentityConversion;
		array[8, 1] = CodeGenFacts.ALCompilerToByteCall;
		array[8, 2] = CodeGenFacts.ALCompilerToCharCall;
		array[8, 3] = SpecialConversion;
		array[8, 4] = IdentityConversion;
		array[8, 5] = IdentityConversion;
		array[8, 6] = CodeGenFacts.ALCompilerToDecimalCall;
		array[8, 7] = IdentityConversion;
		array[8, 16] = IdentityConversion;
		array[9, 1] = CodeGenFacts.ALCompilerToByteCall;
		array[9, 2] = CodeGenFacts.ALCompilerToCharCall;
		array[9, 4] = CodeGenFacts.ALCompilerToIntegerCall;
		array[9, 5] = IdentityConversion;
		array[9, 6] = CodeGenFacts.ALCompilerToDecimalCall;
		array[9, 7] = IdentityConversion;
		array[9, 16] = IdentityConversion;
		array[10, 1] = CodeGenFacts.ALCompilerToByteCall;
		array[10, 2] = CodeGenFacts.ALCompilerToCharCall;
		array[10, 3] = SpecialConversion;
		array[10, 4] = IdentityConversion;
		array[10, 5] = IdentityConversion;
		array[10, 6] = CodeGenFacts.ALCompilerToDecimalCall;
		array[10, 7] = CodeGenFacts.ALCompilerToDurationCall;
		array[10, 16] = IdentityConversion;
		array[11, 1] = CodeGenFacts.ALCompilerToByteCall;
		array[11, 2] = CodeGenFacts.ALCompilerToCharCall;
		array[11, 4] = CodeGenFacts.ALCompilerToIntegerCall;
		array[11, 5] = IdentityConversion;
		array[11, 6] = CodeGenFacts.ALCompilerToDecimalCall;
		array[11, 7] = IdentityConversion;
		array[11, 16] = IdentityConversion;
		array[12, 1] = CodeGenFacts.ALCompilerToByteCall;
		array[12, 2] = CodeGenFacts.ALCompilerToCharCall;
		array[12, 3] = SpecialConversion;
		array[12, 4] = CodeGenFacts.ALCompilerToIntegerCall;
		array[12, 5] = IdentityConversion;
		array[12, 6] = CodeGenFacts.ALCompilerToDecimalCall;
		array[12, 7] = CodeGenFacts.ALCompilerToDurationCall;
		array[12, 16] = IdentityConversion;
		array[13, 1] = CodeGenFacts.ALCompilerToByteCall;
		array[13, 2] = CodeGenFacts.ALCompilerToCharCall;
		array[13, 4] = CodeGenFacts.ALCompilerToIntegerCall;
		array[13, 5] = CodeGenFacts.ALCompilerToBigIntegerCall;
		array[13, 6] = CodeGenFacts.ALCompilerToDecimalCall;
		array[13, 16] = IdentityConversion;
		array[14, 1] = CodeGenFacts.ALCompilerToByteCall;
		array[14, 2] = CodeGenFacts.ALCompilerToCharCall;
		array[14, 3] = SpecialConversion;
		array[14, 4] = CodeGenFacts.ALCompilerToIntegerCall;
		array[14, 5] = CodeGenFacts.ALCompilerToBigIntegerCall;
		array[14, 6] = IdentityConversion;
		array[14, 7] = CodeGenFacts.ALCompilerToDurationCall;
		array[14, 16] = IdentityConversion;
		array[15, 1] = CodeGenFacts.ALCompilerToByteCall;
		array[15, 2] = CodeGenFacts.ALCompilerToCharCall;
		array[15, 4] = CodeGenFacts.ALCompilerToIntegerCall;
		array[15, 5] = CodeGenFacts.ALCompilerToBigIntegerCall;
		array[15, 6] = CodeGenFacts.ALCompilerToDecimalCall;
		array[15, 16] = IdentityConversion;
		array[16, 1] = CodeGenFacts.ALCompilerToByteCall;
		array[16, 2] = CodeGenFacts.ALCompilerToCharCall;
		array[16, 4] = CodeGenFacts.ALCompilerToIntegerCall;
		array[16, 5] = CodeGenFacts.ALCompilerToBigIntegerCall;
		array[16, 6] = CodeGenFacts.ALCompilerToDecimalCall;
		array[16, 16] = IdentityConversion;
		array[17, 8] = SpecialConversion;
		array[17, 9] = SpecialConversion;
		array[17, 15] = CodeGenFacts.ALCompilerStringToGuidCall;
		array[17, 16] = IdentityConversion;
		array[17, 17] = SpecialConversion;
		array[17, 19] = CodeGenFacts.NavBigTextCreateFromObjectCall;
		array[18, 1] = CodeGenFacts.ALCompilerToByteCall;
		array[18, 2] = CodeGenFacts.ALCompilerToCharCall;
		array[18, 3] = SpecialConversion;
		array[18, 4] = CodeGenFacts.ALCompilerToIntegerCall;
		array[18, 5] = CodeGenFacts.ALCompilerToBigIntegerCall;
		array[18, 6] = IdentityConversion;
		array[18, 7] = IdentityConversion;
		array[18, 16] = IdentityConversion;
		array[19, 12] = IdentityConversion;
		array[19, 16] = IdentityConversion;
		array[20, 8] = CodeGenFacts.ALCompilerGuidToNavCodeCall;
		array[20, 9] = CodeGenFacts.ALCompilerGuidToNavTextCall;
		array[20, 15] = IdentityConversion;
		array[20, 16] = IdentityConversion;
		array[20, 17] = CodeGenFacts.ALCompilerGuidToNavTextCall;
		array[21, 13] = SpecialConversion;
		array[21, 14] = SpecialConversion;
		array[21, 16] = IdentityConversion;
		DotNetToAlConversions = array;
		AlToDotNetConversions = new Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax[20, 22]
		{
			{
				null, IdentityConversion, null, null, IdentityConversion, null, null, null, null, null,
				null, null, null, null, null, null, null, null, null, null,
				null, null
			},
			{
				null,
				IdentityConversion,
				IdentityConversion,
				null,
				null,
				CodeGenFacts.ALCompilerToCharCall,
				CodeGenFacts.ALCompilerToSByteCall,
				IdentityConversion,
				IdentityConversion,
				IdentityConversion,
				IdentityConversion,
				IdentityConversion,
				IdentityConversion,
				IdentityConversion,
				CodeGenFacts.ALCompilerToDecimalCall,
				IdentityConversion,
				IdentityConversion,
				null,
				CodeGenFacts.ALCompilerToDurationCall,
				null,
				null,
				null
			},
			{
				null,
				IdentityConversion,
				IdentityConversion,
				null,
				null,
				IdentityConversion,
				CodeGenFacts.ALCompilerToSByteCall,
				CodeGenFacts.ALCompilerToByteCall,
				IdentityConversion,
				IdentityConversion,
				IdentityConversion,
				IdentityConversion,
				IdentityConversion,
				IdentityConversion,
				CodeGenFacts.ALCompilerToDecimalCall,
				IdentityConversion,
				IdentityConversion,
				CodeGenFacts.ALCompilerObjectToStringCall,
				IdentityConversion,
				null,
				null,
				null
			},
			{
				null,
				IdentityConversion,
				IdentityConversion,
				null,
				null,
				IdentityConversion,
				IdentityConversion,
				CodeGenFacts.ALCompilerToByteCall,
				IdentityConversion,
				IdentityConversion,
				IdentityConversion,
				IdentityConversion,
				IdentityConversion,
				IdentityConversion,
				IdentityConversion,
				IdentityConversion,
				IdentityConversion,
				null,
				CodeGenFacts.ALCompilerToDurationCall,
				null,
				null,
				null
			},
			{
				null,
				IdentityConversion,
				IdentityConversion,
				null,
				null,
				CodeGenFacts.ALCompilerToCharCall,
				CodeGenFacts.ALCompilerToSByteCall,
				CodeGenFacts.ALCompilerToByteCall,
				CodeGenFacts.ALCompilerToInt16Call,
				CodeGenFacts.ALCompilerToUInt16Call,
				IdentityConversion,
				CodeGenFacts.ALCompilerToUInt32Call,
				IdentityConversion,
				CodeGenFacts.ALCompilerToUInt64Call,
				CodeGenFacts.ALCompilerToDecimalCall,
				IdentityConversion,
				IdentityConversion,
				null,
				CodeGenFacts.ALCompilerToDurationCall,
				null,
				null,
				null
			},
			{
				null,
				IdentityConversion,
				IdentityConversion,
				null,
				null,
				CodeGenFacts.ALCompilerToCharCall,
				CodeGenFacts.ALCompilerToSByteCall,
				CodeGenFacts.ALCompilerToByteCall,
				CodeGenFacts.ALCompilerToInt16Call,
				CodeGenFacts.ALCompilerToUInt16Call,
				CodeGenFacts.ALCompilerToIntegerCall,
				CodeGenFacts.ALCompilerToUInt32Call,
				IdentityConversion,
				CodeGenFacts.ALCompilerToUInt64Call,
				CodeGenFacts.ALCompilerToDecimalCall,
				IdentityConversion,
				IdentityConversion,
				null,
				CodeGenFacts.ALCompilerToDurationCall,
				null,
				null,
				null
			},
			{
				null,
				IdentityConversion,
				null,
				null,
				null,
				CodeGenFacts.ALCompilerToCharCall,
				CodeGenFacts.ALCompilerToSByteCall,
				CodeGenFacts.ALCompilerToByteCall,
				CodeGenFacts.ALCompilerToInt16Call,
				CodeGenFacts.ALCompilerToUInt16Call,
				CodeGenFacts.ALCompilerToIntegerCall,
				CodeGenFacts.ALCompilerToUInt32Call,
				CodeGenFacts.ALCompilerToBigIntegerCall,
				CodeGenFacts.ALCompilerToUInt64Call,
				IdentityConversion,
				CodeGenFacts.ALCompilerToSingleCall,
				CodeGenFacts.ALCompilerToDoubleCall,
				null,
				CodeGenFacts.ALCompilerToDurationCall,
				null,
				null,
				null
			},
			{
				null,
				IdentityConversion,
				null,
				null,
				null,
				CodeGenFacts.ALCompilerToCharCall,
				IdentityConversion,
				CodeGenFacts.ALCompilerToByteCall,
				IdentityConversion,
				IdentityConversion,
				CodeGenFacts.ALCompilerToIntegerCall,
				IdentityConversion,
				CodeGenFacts.ALCompilerToBigIntegerCall,
				IdentityConversion,
				IdentityConversion,
				IdentityConversion,
				IdentityConversion,
				null,
				IdentityConversion,
				null,
				null,
				null
			},
			{
				null,
				IdentityConversion,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				IdentityConversion,
				null,
				null,
				CodeGenFacts.ALCompilerStringToGuidCall,
				null
			},
			{
				null,
				IdentityConversion,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				IdentityConversion,
				null,
				null,
				CodeGenFacts.ALCompilerStringToGuidCall,
				null
			},
			{
				null, IdentityConversion, null, null, null, null, null, null, null, null,
				null, null, null, null, null, null, null, null, null, null,
				null, null
			},
			{
				null, IdentityConversion, null, null, null, null, null, null, null, null,
				null, null, null, null, null, null, null, null, null, null,
				null, null
			},
			{
				null, IdentityConversion, null, null, null, null, null, null, null, null,
				null, null, null, null, null, null, null, null, null, IdentityConversion,
				null, null
			},
			{
				null, IdentityConversion, null, null, null, null, null, null, null, null,
				null, null, null, null, null, null, null, null, null, null,
				null, SpecialConversion
			},
			{
				null, IdentityConversion, null, null, null, null, null, null, null, null,
				null, null, null, null, null, null, null, null, null, null,
				null, SpecialConversion
			},
			{
				null,
				IdentityConversion,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				CodeGenFacts.ALCompilerGuidToStringCall,
				null,
				null,
				IdentityConversion,
				null
			},
			{
				ALCompilerVariantToNavDotNet,
				IdentityConversion,
				SpecialConversion,
				null,
				CodeGenFacts.ALCompilerNavIndirectValueToBooleanCall,
				CodeGenFacts.ALCompilerNavIndirectValueToCharCall,
				IdentityConversion,
				CodeGenFacts.ALCompilerNavIndirectValueToByteCall,
				IdentityConversion,
				IdentityConversion,
				CodeGenFacts.ALCompilerNavIndirectValueToInt32Call,
				IdentityConversion,
				CodeGenFacts.ALCompilerNavIndirectValueToInt64Call,
				IdentityConversion,
				CodeGenFacts.ALCompilerNavIndirectValueToDecimalCall,
				IdentityConversion,
				IdentityConversion,
				CodeGenFacts.ALCompilerNavIndirectValueToStringCall,
				SpecialConversion,
				SpecialConversion,
				CodeGenFacts.ALCompilerNavIndirectValueToGuidCall,
				SpecialConversion
			},
			{
				null,
				IdentityConversion,
				null,
				null,
				null,
				CodeGenFacts.ALCompilerToCharCall,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				IdentityConversion,
				null,
				null,
				CodeGenFacts.ALCompilerStringToGuidCall,
				null
			},
			{
				null,
				CodeGenFacts.ALCompilerObjectToStringCall,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				CodeGenFacts.ALCompilerObjectToStringCall,
				null,
				null,
				CodeGenFacts.ALCompilerStringToGuidCall,
				null
			},
			{
				null,
				CodeGenFacts.ALCompilerObjectToStringCall,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				CodeGenFacts.ALCompilerObjectToStringCall,
				null,
				null,
				null,
				null
			}
		};
		DefaultFieldDeclarationSyntaxKinds = new Microsoft.CodeAnalysis.CSharp.SyntaxKind[1] { Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword };
		TextConstFieldDeclarationSyntaxKinds = new Microsoft.CodeAnalysis.CSharp.SyntaxKind[3]
		{
			Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword,
			Microsoft.CodeAnalysis.CSharp.SyntaxKind.StaticKeyword,
			Microsoft.CodeAnalysis.CSharp.SyntaxKind.ReadOnlyKeyword
		};
		DefaultMethodLocalFieldDeclarationSyntaxKinds = new Microsoft.CodeAnalysis.CSharp.SyntaxKind[1] { Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword };
		TextConstMethodLocalFieldDeclarationSyntaxKinds = new Microsoft.CodeAnalysis.CSharp.SyntaxKind[3]
		{
			Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword,
			Microsoft.CodeAnalysis.CSharp.SyntaxKind.StaticKeyword,
			Microsoft.CodeAnalysis.CSharp.SyntaxKind.ReadOnlyKeyword
		};
	}
}
