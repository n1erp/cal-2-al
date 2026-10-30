using System;
using System.Reflection;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference.DotNet;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

public static class ControlAddInModuleDefinitionExtensions
{
	public static void AddDotNetTypeDefinitions(this ModuleDefinition module)
	{
		if (module.DotNetPackages == null)
		{
			DotNetDeclarationExtractor dotNetDeclarationExtractor = new DotNetDeclarationExtractor();
			UpdateTypeDefinition(module, dotNetDeclarationExtractor);
			if (dotNetDeclarationExtractor.HasDeclarations)
			{
				module.DotNetPackages = new DotNetPackageDefinition[1] { dotNetDeclarationExtractor.GetDeclaration() };
			}
		}
	}

	private static void UpdateTypeDefinition(object containingObject, IDotNetDeclarationExtractor extractor)
	{
		if (containingObject == null)
		{
			return;
		}
		PropertyInfo[] properties = containingObject.GetType().GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			DebugAssertHelper.Assert(propertyInfo.GetMethod != null);
			Type returnType = propertyInfo.GetMethod.ReturnType;
			if (returnType == typeof(string) || typeof(ValueType).IsAssignableFrom(returnType))
			{
				continue;
			}
			if (returnType.IsArray)
			{
				if (!(propertyInfo.GetValue(containingObject) is Array array))
				{
					continue;
				}
				foreach (object item in array)
				{
					UpdateTypeDefinition(item, extractor);
				}
			}
			else if (returnType == typeof(TypeDefinition))
			{
				if (propertyInfo.GetValue(containingObject) is TypeDefinition typeDefinition && string.Equals(typeDefinition.Name, "DotNet", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(typeDefinition.Subtype?.Name))
				{
					if (IsControlAddInType(typeDefinition))
					{
						typeDefinition.Subtype = Subtype.Empty;
					}
					else
					{
						typeDefinition.Subtype = extractor.AddTypeDeclarationAndGetAlias(typeDefinition.Subtype.Name);
					}
				}
			}
			else
			{
				UpdateTypeDefinition(propertyInfo.GetValue(containingObject), extractor);
			}
		}
	}

	private static bool IsControlAddInType(TypeDefinition propertyValue)
	{
		return propertyValue.Subtype.Name.EndsWith("Microsoft.Dynamics.Nav.Client.BusinessChart.BusinessChartAddIn", StringComparison.Ordinal);
	}

	public static void AddBuiltInControlAddIns(this ModuleDefinition module)
	{
		module.ControlAddIns = new ControlAddInDefinition[5]
		{
			ControlAddIn("Microsoft.Dynamics.Nav.Client.WebPageViewer", new MethodDefinition[9]
			{
				Method("Navigate", Param("url", TextType())),
				Method("Navigate", Param("url", TextType()), Param("method", TextType()), Param("data", TextType())),
				Method("InitializeIFrame", Param("ratio", TextType())),
				Method("SetContent", Param("html", TextType())),
				Method("SetContent", Param("html", TextType()), Param("javaScript", TextType())),
				Method("PostMessage", Param("message", TextType()), Param("targetOrigin", TextType()), Param("convertToJson", Type("Boolean"))),
				Method("LinksOpenInNewWindow"),
				Method("InvokeEvent", Param("data", TextType())),
				Method("SubscribeToEvent", Param("eventName", TextType()), Param("origin", TextType()))
			}, new EventDefinition[4]
			{
				Event("ControlAddInReady", Param("callbackUrl", TextType())),
				Event("Refresh", Param("callbackUrl", TextType())),
				Event("DocumentReady"),
				Event("Callback", Param("data", TextType()))
			}),
			ControlAddIn("Microsoft.Dynamics.Nav.Client.BusinessChart", new MethodDefinition[0], new EventDefinition[3]
			{
				Event("AddInReady"),
				Event("Refresh"),
				Event("DataPointClicked", Param("point", Type("JsonObject")))
			}),
			ControlAddIn("Microsoft.Dynamics.Nav.Client.PageReady", new MethodDefinition[0], new EventDefinition[1] { Event("AddInReady") }),
			ControlAddIn("Microsoft.Dynamics.Nav.Client.SocialListening", new MethodDefinition[3]
			{
				Method("DetermineUserAuthentication", Param("url", TextType())),
				Method("ShowMessage", Param("message", TextType())),
				Method("ShowWidget", Param("url", TextType()))
			}, new EventDefinition[3]
			{
				Event("AddInReady"),
				Event("DetermineUserAuthenticationResult", Param("result", Type("Integer"))),
				Event("MessageLinkClick", Param("identifier", Type("Integer")))
			}),
			ControlAddIn("Microsoft.Dynamics.Nav.Client.VideoPlayer", new MethodDefinition[4]
			{
				Method("SetFrameAttribute", Param("attributeName", TextType()), Param("attributeValue", TextType())),
				Method("RemoveAttribute", Param("attributeName", TextType())),
				Method("SetWidth", Param("videoWidth", Type("Integer"))),
				Method("SetHeight", Param("videoHeight", Type("Integer")))
			}, new EventDefinition[1] { Event("AddInReady") })
		};
	}

	private static ControlAddInDefinition ControlAddIn(string name, MethodDefinition[] methods, EventDefinition[] events)
	{
		return new ControlAddInDefinition
		{
			Name = name,
			PublicKeyToken = "31bf3856ad364e35",
			Events = events,
			Methods = methods
		};
	}

	private static EventDefinition Event(string name, params ParameterDefinition[] parameters)
	{
		return new EventDefinition
		{
			Name = name,
			Parameters = parameters
		};
	}

	private static MethodDefinition Method(string name, params ParameterDefinition[] parameters)
	{
		return new MethodDefinition
		{
			Name = name,
			Parameters = parameters,
			MethodKind = MethodKind.DeclareMethod
		};
	}

	private static ParameterDefinition Param(string name, TypeDefinition type)
	{
		return new ParameterDefinition
		{
			Name = name,
			TypeDefinition = type
		};
	}

	private static TypeDefinition TextType()
	{
		return Type("Text");
	}

	private static TypeDefinition Type(string type)
	{
		return new TypeDefinition
		{
			Name = type
		};
	}
}
