using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NCLCodeLoader : INCLCodeLoader
{
	private readonly bool enableStaticAssemblies;

	private readonly AssemblyGetter assemblyGetter;

	private Task compileBusinessApplicationOnce;

	private readonly bool compileBusinessApplicationAtStartup;

	public NCLCodeLoader(TempPathHelper tempPathHelper)
	{
		enableStaticAssemblies = ServerUserSettings.Instance.EnableStaticAssemblies.Value;
		assemblyGetter = new AssemblyGetter(tempPathHelper);
		compileBusinessApplicationAtStartup = NavEnvironment.Instance.CompileBusinessApplicationAtStartup;
	}

	public Task CompileBusinessAssembliesAsync()
	{
		if (compileBusinessApplicationAtStartup)
		{
			lock (assemblyGetter)
			{
				if (compileBusinessApplicationOnce == null)
				{
					compileBusinessApplicationOnce = NavTaskFactory.StartNewTask(assemblyGetter.CompileBusinessApplication);
				}
				return compileBusinessApplicationOnce;
			}
		}
		return Task.CompletedTask;
	}

	public void DeleteUnneededSources()
	{
		lock (assemblyGetter)
		{
			assemblyGetter.DeleteUnneededSources();
		}
	}

	public void CompileBusinessEventAssemblies()
	{
		lock (assemblyGetter)
		{
			assemblyGetter.CompileBusinessApplicationEvents();
		}
	}

	public Type GetBaseObjectClrType(ObjectType objectType, int objectId)
	{
		string className = ClassName(objectType, objectId);
		return GetApplicationObjectClrType(objectType, objectId, () => assemblyGetter.GetAssemblyForBaseObject(objectType, objectId), className);
	}

	private Type GetApplicationObjectClrType(ObjectType objectType, int objectId, Func<Assembly> assemblyGetterFunc, string className)
	{
		if (!enableStaticAssemblies)
		{
			Assembly assembly = assemblyGetterFunc();
			Module[] modules = assembly.GetModules();
			if (modules.Length != 1)
			{
				throw new NavNCLMetadataVersionMismatchException(CultureInfo.CurrentCulture, objectType, objectId);
			}
			Type[] array = Array.FindAll(modules[0].GetTypes(), (Type t) => !t.IsNested && typeof(NavApplicationObjectBase).IsAssignableFrom(t));
			if (array.Length != 1)
			{
				throw new NavNCLMetadataVersionMismatchException(CultureInfo.CurrentCulture, objectType, objectId);
			}
			return array[0];
		}
		string text = "Microsoft.Dynamics.Nav.BusinessApplication." + className;
		foreach (Assembly applicationAssembly in NavEnvironment.Instance.ApplicationAssemblies)
		{
			Type type = applicationAssembly.GetType(text, throwOnError: false, ignoreCase: true);
			if (type != null)
			{
				return type;
			}
		}
		throw NavNCLClassNotFoundException.Create(CultureInfo.CurrentCulture, text);
	}

	public static string ClassName(ObjectType objectType, int objectId)
	{
		return ClassNamePrefix(objectType) + objectId.ToString(CultureInfo.InvariantCulture);
	}

	internal static string ClassNamePrefix(ObjectType objectType)
	{
		return objectType switch
		{
			ObjectType.Table => "Record", 
			ObjectType.Page => "Page", 
			ObjectType.Report => "Report", 
			ObjectType.CodeUnit => "Codeunit", 
			ObjectType.XmlPort => "XMLport", 
			ObjectType.Query => "Query", 
			ObjectType.MenuSuite => "MenuSuite", 
			ObjectType.PageExtension => "PageExtension", 
			ObjectType.TableExtension => "TableExtension", 
			_ => "Undefined object", 
		};
	}

	internal static Delegate CreateDelegateToConstructor(Type delegateType, Type legacyDelegateType, Type objectType, out bool usedLegacyConstructor)
	{
		Type[] types = Array.ConvertAll(delegateType.GetMethod("Invoke").GetParameters(), (ParameterInfo p) => p.ParameterType);
		MethodInfo method = objectType.GetMethod("__Construct", BindingFlags.Static | BindingFlags.Public, null, types, null);
		usedLegacyConstructor = false;
		if (method == null)
		{
			types = Array.ConvertAll(legacyDelegateType.GetMethod("Invoke").GetParameters(), (ParameterInfo p) => p.ParameterType);
			method = objectType.GetMethod("__Construct", BindingFlags.Static | BindingFlags.Public, null, types, null);
			if (method == null)
			{
				throw new NavNCLCompilationException(CultureInfo.CurrentCulture, Lang.InvalidObjectCodeRecompile);
			}
			usedLegacyConstructor = true;
			return Delegate.CreateDelegate(legacyDelegateType, method);
		}
		return Delegate.CreateDelegate(delegateType, method);
	}

	public ReadOnlyRecordBuffer GetBaseApplicationObjectRecord(ApplicationObjectId objectId)
	{
		return assemblyGetter.GetBaseApplicationObjectRecord(objectId);
	}
}
