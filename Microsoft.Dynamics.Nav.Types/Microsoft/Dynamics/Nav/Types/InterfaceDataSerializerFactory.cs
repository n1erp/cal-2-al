using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

public static class InterfaceDataSerializerFactory
{
	public static class Serializer
	{
		[DebuggerStepThrough]
		public static TData Serialize<TData>(TData value)
		{
			DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(TData));
			MemoryStream memoryStream = new MemoryStream();
			dataContractSerializer.WriteObject(memoryStream, value);
			memoryStream.Position = 0L;
			return (TData)dataContractSerializer.ReadObject(memoryStream);
		}
	}

	private const string InnerFieldName = "__inner";

	private static readonly object lockObject = new object();

	private static Dictionary<Type, Type> cache = new Dictionary<Type, Type>();

	private static Type CreateWrapperType<T>() where T : class
	{
		MethodInfo method = typeof(Serializer).GetMethod("Serialize", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		string text = typeof(T).Name + "Wrapper";
		ModuleBuilder moduleBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(text), AssemblyBuilderAccess.Run).DefineDynamicModule("Default");
		Type[] interfaces = new Type[1] { typeof(T) };
		TypeBuilder typeBuilder = moduleBuilder.DefineType(text, TypeAttributes.Public, typeof(object), interfaces);
		FieldBuilder field = typeBuilder.DefineField("__inner", typeof(T), FieldAttributes.Private);
		ILGenerator iLGenerator = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[1] { typeof(T) }).GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Call, typeof(object).GetConstructor(new Type[0]));
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Stfld, field);
		iLGenerator.Emit(OpCodes.Ret);
		MethodInfo[] methods = typeof(T).GetMethods();
		foreach (MethodInfo methodInfo in methods)
		{
			ParameterInfo[] parameters = methodInfo.GetParameters();
			MethodBuilder methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual, methodInfo.ReturnType, parameters.Select((ParameterInfo p) => p.ParameterType).ToArray());
			bool flag = methodInfo.ReturnType != typeof(void);
			ILGenerator iLGenerator2 = methodBuilder.GetILGenerator();
			LocalBuilder[] array = new LocalBuilder[parameters.Length];
			for (int num = 0; num < parameters.Length; num++)
			{
				Type parameterType = parameters[num].ParameterType;
				array[num] = iLGenerator2.DeclareLocal(parameterType.IsByRef ? parameterType.GetElementType() : parameterType);
				iLGenerator2.Emit(OpCodes.Ldarg, num + 1);
				if (parameterType.IsByRef)
				{
					iLGenerator2.Emit(OpCodes.Ldind_Ref);
					iLGenerator2.EmitCall(OpCodes.Call, method.MakeGenericMethod(parameterType.GetElementType()), null);
				}
				else
				{
					iLGenerator2.EmitCall(OpCodes.Call, method.MakeGenericMethod(parameterType), null);
				}
				iLGenerator2.Emit(OpCodes.Stloc, array[num]);
			}
			LocalBuilder localBuilder = (flag ? iLGenerator2.DeclareLocal(methodBuilder.ReturnType) : null);
			iLGenerator2.Emit(OpCodes.Ldarg_0);
			iLGenerator2.Emit(OpCodes.Ldfld, field);
			for (int num2 = 0; num2 < parameters.Length; num2++)
			{
				if (parameters[num2].ParameterType.IsByRef)
				{
					iLGenerator2.Emit(OpCodes.Ldloca, array[num2]);
				}
				else
				{
					iLGenerator2.Emit(OpCodes.Ldloc, array[num2]);
				}
			}
			iLGenerator2.Emit(OpCodes.Callvirt, methodInfo);
			if (flag)
			{
				iLGenerator2.Emit(OpCodes.Stloc, localBuilder);
			}
			for (int num3 = 0; num3 < parameters.Length; num3++)
			{
				ParameterInfo parameterInfo = parameters[num3];
				if (parameterInfo.ParameterType.IsByRef)
				{
					iLGenerator2.Emit(OpCodes.Ldarg, num3 + 1);
					iLGenerator2.Emit(OpCodes.Ldloc, array[num3]);
					iLGenerator2.EmitCall(OpCodes.Call, method.MakeGenericMethod(parameterInfo.ParameterType.IsByRef ? parameterInfo.ParameterType.GetElementType() : parameterInfo.ParameterType), null);
					iLGenerator2.Emit(OpCodes.Stind_Ref);
				}
			}
			if (flag)
			{
				iLGenerator2.Emit(OpCodes.Ldloc, localBuilder);
				iLGenerator2.EmitCall(OpCodes.Call, method.MakeGenericMethod(localBuilder.LocalType), null);
			}
			iLGenerator2.Emit(OpCodes.Ret);
		}
		return typeBuilder.CreateType();
	}

	public static T CreateWrapperInstance<T>(T inner) where T : class
	{
		Type value;
		lock (lockObject)
		{
			if (!cache.TryGetValue(typeof(T), out value))
			{
				value = CreateWrapperType<T>();
				cache.Add(typeof(T), value);
			}
		}
		return (T)Activator.CreateInstance(value, inner);
	}
}
