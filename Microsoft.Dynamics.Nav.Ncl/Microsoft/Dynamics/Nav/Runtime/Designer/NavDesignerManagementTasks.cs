using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Apps;
using Microsoft.Dynamics.Nav.Types.ClientDesigner;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

public class NavDesignerManagementTasks
{
	private static readonly List<string> PageControlReferenceProperties = new List<string> { "FreezeColumnID" };

	private NavSession Session { get; }

	private INavDesignerExtensionFactory DesignerExtensionFactory { get; }

	public InvokeDesignerActionResponse AddPageAction(int pageId, ActionContainerType containerType, string parentActionName, string relativeActionName, string actionName)
	{
		InvokeDesignerActionResponse invokeDesignerActionResponse = new InvokeDesignerActionResponse
		{
			Status = NavDesignerActionStatus.Error
		};
		if (Session.CurrentDesignerExtension != null)
		{
			try
			{
				INavDesignerPageOperations pageOperationInterfaceForPage = Session.CurrentDesignerExtension.GetPageOperationInterfaceForPage(pageId);
				if (pageOperationInterfaceForPage == null)
				{
					return invokeDesignerActionResponse;
				}
				MasterPage masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId);
				string actionALIdentifier = masterPageForDesigner.GetActionALIdentifier(parentActionName, !masterPageForDesigner.Name.Equals(parentActionName));
				string actionALIdentifier2 = masterPageForDesigner.GetActionALIdentifier(relativeActionName);
				string actionName2 = actionName;
				ActionAreaKind actionAreaKind = NavDesignerUtil.ActionContainerTypeToAreaKind(containerType);
				bool flag = false;
				NavDesignerActionContext actionContext = masterPageForDesigner.GetActionContext((ActionDefinition action) => string.Equals(action.Name, actionName));
				if (actionContext?.Action != null)
				{
					actionName2 = actionContext.Action.Name;
					if (pageOperationInterfaceForPage.SetPageActionProperty(actionContext.GetActionALIdentifier(), "Visible", true))
					{
						flag = pageOperationInterfaceForPage.MovePageAction(actionContext, actionAreaKind, actionALIdentifier, actionALIdentifier2);
					}
				}
				if (flag)
				{
					Session.CurrentDesignerExtension.Save();
					masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId);
					actionContext = masterPageForDesigner.GetActionContext(actionName2);
					invokeDesignerActionResponse.Info = new AddPageActionResponse
					{
						ActionDefinition = actionContext.Action,
						TargetName = actionContext.Action.Name,
						ParentTargetName = actionContext.ParentGroup?.Name,
						RelativeTargetName = actionContext.PreviousAction?.Name,
						ContainerType = actionContext.ActionContainerType
					};
					invokeDesignerActionResponse.Status = NavDesignerActionStatus.Success;
					Session.StalePages.Add(pageId);
				}
			}
			catch (NavBaseException ex) when (ShouldCatchException(ex))
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00007GC", Category.Extensions, ex);
				throw;
			}
		}
		return invokeDesignerActionResponse;
	}

	public InvokeDesignerActionResponse CreatePageActionGroup(int pageId, ActionContainerType containerType, string parentActionName, string relativeActionName, string groupName)
	{
		InvokeDesignerActionResponse invokeDesignerActionResponse = new InvokeDesignerActionResponse
		{
			Status = NavDesignerActionStatus.Error
		};
		if (Session.CurrentDesignerExtension != null)
		{
			try
			{
				INavDesignerPageOperations pageOperationInterfaceForPage = Session.CurrentDesignerExtension.GetPageOperationInterfaceForPage(pageId);
				if (pageOperationInterfaceForPage == null)
				{
					return invokeDesignerActionResponse;
				}
				MasterPage masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId);
				string actionALIdentifier = masterPageForDesigner.GetActionALIdentifier(parentActionName, !masterPageForDesigner.Name.Equals(parentActionName));
				string actionALIdentifier2 = masterPageForDesigner.GetActionALIdentifier(relativeActionName);
				string text = groupName;
				bool flag = masterPageForDesigner.ActionExistsOnPage(text);
				while (flag)
				{
					text = groupName + new Random().Next(10000, 99999);
					flag = masterPageForDesigner.ActionExistsOnPage(text);
				}
				ActionAreaKind actionAreaKind = NavDesignerUtil.ActionContainerTypeToAreaKind(containerType);
				if (pageOperationInterfaceForPage.AddPageAction(text, actionAreaKind, actionALIdentifier, actionALIdentifier2, isGroup: true))
				{
					pageOperationInterfaceForPage.SetPageActionProperty(text, "Caption", groupName);
					Session.CurrentDesignerExtension.Save();
					masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId);
					NavDesignerActionContext actionContext = masterPageForDesigner.GetActionContext(text);
					invokeDesignerActionResponse.Info = new CreatePageActionGroupResponse
					{
						ActionDefinition = actionContext.Action,
						TargetName = actionContext.Action.Name,
						ParentTargetName = actionContext.ParentGroup?.Name,
						RelativeTargetName = actionContext.PreviousAction?.Name,
						ContainerType = actionContext.ActionContainerType
					};
					invokeDesignerActionResponse.Status = NavDesignerActionStatus.Success;
					Session.StalePages.Add(pageId);
				}
			}
			catch (NavBaseException ex) when (ShouldCatchException(ex))
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00007ND", Category.Extensions, ex);
				throw;
			}
		}
		return invokeDesignerActionResponse;
	}

	public InvokeDesignerActionResponse MovePageAction(int pageId, string actionName, ActionContainerType containerType, string parentActionName, string relativeActionName)
	{
		InvokeDesignerActionResponse invokeDesignerActionResponse = new InvokeDesignerActionResponse
		{
			Status = NavDesignerActionStatus.Error
		};
		if (Session.CurrentDesignerExtension != null)
		{
			try
			{
				INavDesignerPageOperations pageOperationInterfaceForPage = Session.CurrentDesignerExtension.GetPageOperationInterfaceForPage(pageId);
				if (pageOperationInterfaceForPage == null)
				{
					return invokeDesignerActionResponse;
				}
				if (MovePageAction(pageOperationInterfaceForPage, pageId, actionName, containerType, parentActionName, relativeActionName))
				{
					Session.CurrentDesignerExtension.Save();
					MasterPage masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId);
					NavDesignerActionContext actionContext = masterPageForDesigner.GetActionContext(actionName);
					invokeDesignerActionResponse.Info = new MovePageActionResponse
					{
						TargetName = actionContext.Action.Name,
						ParentTargetName = actionContext.ParentGroup?.Name,
						RelativeTargetName = actionContext.PreviousAction?.Name,
						ContainerType = actionContext.ActionContainerType
					};
					invokeDesignerActionResponse.Status = NavDesignerActionStatus.Success;
				}
			}
			catch (NavBaseException ex) when (ShouldCatchException(ex))
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00007GD", Category.Extensions, ex);
				throw;
			}
		}
		return invokeDesignerActionResponse;
	}

	public InvokeDesignerActionResponse RemovePageAction(int pageId, string actionName)
	{
		InvokeDesignerActionResponse invokeDesignerActionResponse = new InvokeDesignerActionResponse
		{
			Info = new RemovePageActionResponse
			{
				TargetName = actionName
			},
			Status = NavDesignerActionStatus.Error
		};
		if (Session.CurrentDesignerExtension != null)
		{
			try
			{
				INavDesignerPageOperations pageOperationInterfaceForPage = Session.CurrentDesignerExtension.GetPageOperationInterfaceForPage(pageId);
				if (pageOperationInterfaceForPage == null)
				{
					return invokeDesignerActionResponse;
				}
				MasterPage masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId);
				NavDesignerActionContext actionContext = masterPageForDesigner.GetActionContext(actionName);
				if (pageOperationInterfaceForPage.RemovePageAction(actionContext))
				{
					Session.CurrentDesignerExtension.Save();
					invokeDesignerActionResponse.Status = NavDesignerActionStatus.Success;
				}
			}
			catch (NavBaseException ex) when (ShouldCatchException(ex))
			{
				Session.Diagnostics.SendExceptionTag("00007GE", Category.Extensions, ex);
				throw;
			}
		}
		return invokeDesignerActionResponse;
	}

	public InvokeDesignerActionResponse SetActionProperty(int pageId, string actionName, string propertyName, object propertyValue)
	{
		InvokeDesignerActionResponse result = new InvokeDesignerActionResponse
		{
			Status = NavDesignerActionStatus.Error
		};
		if (Session.CurrentDesignerExtension != null)
		{
			try
			{
				INavDesignerPageOperations pageOperationInterfaceForPage = Session.CurrentDesignerExtension.GetPageOperationInterfaceForPage(pageId);
				if (pageOperationInterfaceForPage == null)
				{
					return result;
				}
				if (pageOperationInterfaceForPage.SetPageActionProperty(actionName, propertyName, propertyValue?.ToString()))
				{
					Session.CurrentDesignerExtension.Save();
					MasterPage masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId);
					NavDesignerActionContext actionContext = masterPageForDesigner.GetActionContext(actionName);
					result = new InvokeDesignerActionResponse
					{
						Info = new SetActionPropertyResponse
						{
							TargetName = actionContext.Action.Name,
							TargetElementDefinition = actionContext.Action
						},
						Status = NavDesignerActionStatus.Success
					};
				}
			}
			catch (NavBaseException ex) when (ShouldCatchException(ex))
			{
				Session.Diagnostics.SendExceptionTag("00007GF", Category.Extensions, ex);
				throw;
			}
		}
		return result;
	}

	public InvokeDesignerActionResponse PromoteAction(int pageId, string actionName, string promotedCategory, bool? promotedOnly)
	{
		InvokeDesignerActionResponse result = new InvokeDesignerActionResponse
		{
			Status = NavDesignerActionStatus.Error
		};
		if (Session.CurrentDesignerExtension != null)
		{
			try
			{
				INavDesignerPageOperations pageOperationInterfaceForPage = Session.CurrentDesignerExtension.GetPageOperationInterfaceForPage(pageId);
				if (pageOperationInterfaceForPage == null)
				{
					return result;
				}
				if (pageOperationInterfaceForPage.SetPageActionProperty(actionName, "Promoted", true))
				{
					pageOperationInterfaceForPage.SetPageActionProperty(actionName, "PromotedCategory", promotedCategory);
					pageOperationInterfaceForPage.SetPageActionProperty(actionName, "PromotedOnly", promotedOnly);
					Session.CurrentDesignerExtension.Save();
					result = new InvokeDesignerActionResponse
					{
						Info = new PromotePageActionResponse
						{
							TargetName = actionName
						},
						Status = NavDesignerActionStatus.Success
					};
				}
			}
			catch (NavBaseException ex) when (ShouldCatchException(ex))
			{
				Session.Diagnostics.SendExceptionTag("00007NF", Category.Extensions, ex);
				throw;
			}
		}
		return result;
	}

	public InvokeDesignerActionResponse DemoteAction(int pageId, string actionName)
	{
		InvokeDesignerActionResponse result = new InvokeDesignerActionResponse
		{
			Status = NavDesignerActionStatus.Error
		};
		if (Session.CurrentDesignerExtension != null)
		{
			try
			{
				INavDesignerPageOperations pageOperationInterfaceForPage = Session.CurrentDesignerExtension.GetPageOperationInterfaceForPage(pageId);
				if (pageOperationInterfaceForPage == null)
				{
					return result;
				}
				if (!DemoteAction(pageOperationInterfaceForPage, actionName))
				{
					return result;
				}
				Session.CurrentDesignerExtension.Save();
				result = new InvokeDesignerActionResponse
				{
					Info = new DemotePageActionResponse
					{
						TargetName = actionName
					},
					Status = NavDesignerActionStatus.Success
				};
			}
			catch (NavBaseException ex) when (ShouldCatchException(ex))
			{
				Session.Diagnostics.SendExceptionTag("00007NG", Category.Extensions, ex);
				throw;
			}
		}
		return result;
	}

	public InvokeDesignerActionResponse DemoteAndMoveAction(int pageId, string actionName, ActionContainerType containerType, string parentActionName, string relativeActionName)
	{
		InvokeDesignerActionResponse invokeDesignerActionResponse = new InvokeDesignerActionResponse
		{
			Status = NavDesignerActionStatus.Error
		};
		if (Session.CurrentDesignerExtension != null)
		{
			try
			{
				INavDesignerPageOperations pageOperationInterfaceForPage = Session.CurrentDesignerExtension.GetPageOperationInterfaceForPage(pageId);
				if (pageOperationInterfaceForPage == null)
				{
					return invokeDesignerActionResponse;
				}
				if (!DemoteAction(pageOperationInterfaceForPage, actionName))
				{
					return invokeDesignerActionResponse;
				}
				if (!MovePageAction(pageOperationInterfaceForPage, pageId, actionName, containerType, parentActionName, relativeActionName))
				{
					return invokeDesignerActionResponse;
				}
				Session.CurrentDesignerExtension.Save();
				MasterPage masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId);
				NavDesignerActionContext actionContext = masterPageForDesigner.GetActionContext(actionName);
				invokeDesignerActionResponse.Info = new MovePageActionResponse
				{
					TargetName = actionContext.Action.Name,
					ParentTargetName = actionContext.ParentGroup?.Name,
					RelativeTargetName = actionContext.PreviousAction?.Name,
					ContainerType = actionContext.ActionContainerType
				};
				invokeDesignerActionResponse.Status = NavDesignerActionStatus.Success;
			}
			catch (NavBaseException ex) when (ShouldCatchException(ex))
			{
				Session.Diagnostics.SendExceptionTag("00007UO", Category.Extensions, ex);
				throw;
			}
		}
		return invokeDesignerActionResponse;
	}

	public InvokeDesignerActionResponse RestoreActionDefaults(int pageId)
	{
		ValidatePageIdForRestoreRequest(pageId);
		InvokeDesignerActionResponse invokeDesignerActionResponse = new InvokeDesignerActionResponse
		{
			Status = NavDesignerActionStatus.Error,
			Info = new RestoreDefaultsResponse()
		};
		try
		{
			NavDesignerExtension currentDesignerExtension = Session.CurrentDesignerExtension;
			ValidateExtensionLevelForRestoreRequest(currentDesignerExtension);
			RestorePageActionsDefaults(currentDesignerExtension, pageId);
			invokeDesignerActionResponse.Status = NavDesignerActionStatus.Success;
			return invokeDesignerActionResponse;
		}
		catch (NavDesignerCompilerException exception)
		{
			Session.Diagnostics.SendExceptionTag("00007NI", Category.Extensions, exception);
			throw;
		}
	}

	private static void RestorePageActionsDefaults(NavDesignerExtension extension, int pageId)
	{
		extension.GetPageOperationInterfaceForPage(pageId)?.ClearActionChanges();
		extension.Save();
	}

	private bool MovePageAction(INavDesignerPageOperations pageOperations, int pageId, string actionName, ActionContainerType containerType, string parentActionName, string relativeActionName)
	{
		ActionAreaKind actionAreaKind = NavDesignerUtil.ActionContainerTypeToAreaKind(containerType);
		MasterPage masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId);
		string actionALIdentifier = masterPageForDesigner.GetActionALIdentifier(parentActionName, !masterPageForDesigner.Name.Equals(parentActionName));
		string actionALIdentifier2 = masterPageForDesigner.GetActionALIdentifier(relativeActionName);
		NavDesignerActionContext actionContext = masterPageForDesigner.GetActionContext(actionName);
		return pageOperations.MovePageAction(actionContext, actionAreaKind, actionALIdentifier, actionALIdentifier2);
	}

	private static bool DemoteAction(INavDesignerPageOperations pageOperations, string actionName)
	{
		if (!pageOperations.SetPageActionProperty(actionName, "Promoted", false))
		{
			return false;
		}
		pageOperations.SetPageActionProperty(actionName, "PromotedCategory", null);
		pageOperations.SetPageActionProperty(actionName, "PromotedIsBig", null);
		pageOperations.SetPageActionProperty(actionName, "PromotedOnly", null);
		return true;
	}

	public InvokeDesignerActionResponse AddNavigationNodeAction(Guid nodeId, Guid relativeNodeId)
	{
		InvokeDesignerActionResponse result = new InvokeDesignerActionResponse
		{
			Info = new AddNavigationNodeResponse(),
			Status = NavDesignerActionStatus.Error
		};
		_ = Session.CurrentDesignerExtension;
		return result;
	}

	public InvokeDesignerActionResponse MoveNavigationNodeAction(Guid nodeId, Guid relativeNodeId)
	{
		InvokeDesignerActionResponse result = new InvokeDesignerActionResponse
		{
			Info = new MoveNavigationNodeResponse(),
			Status = NavDesignerActionStatus.Error
		};
		_ = Session.CurrentDesignerExtension;
		return result;
	}

	public InvokeDesignerActionResponse RemoveNavigationNodeAction(Guid nodeId)
	{
		InvokeDesignerActionResponse result = new InvokeDesignerActionResponse
		{
			Info = new RemoveNavigationNodeResponse(),
			Status = NavDesignerActionStatus.Error
		};
		_ = Session.CurrentDesignerExtension;
		return result;
	}

	public InvokeDesignerActionResponse SetNavigationNodePropertyAction(Guid nodeId, string propertyName, object propertyValue)
	{
		InvokeDesignerActionResponse result = new InvokeDesignerActionResponse
		{
			Info = new SetNavigationNodePropertyResponse(),
			Status = NavDesignerActionStatus.Error
		};
		_ = Session.CurrentDesignerExtension;
		return result;
	}

	public InvokeDesignerActionResponse SetViewFieldProperty(int pageId, string metadataViewName, string controlName, string propertyName, object propertyValue)
	{
		InvokeDesignerActionResponse result = new InvokeDesignerActionResponse
		{
			Info = new RemovePageFieldResponse
			{
				TargetName = metadataViewName
			},
			Status = NavDesignerActionStatus.Error
		};
		if (Session.CurrentDesignerExtension != null)
		{
			try
			{
				INavDesignerPageOperations pageOperationInterfaceForPage = Session.CurrentDesignerExtension.GetPageOperationInterfaceForPage(pageId);
				if (pageOperationInterfaceForPage == null)
				{
					return result;
				}
				string propertyValue2 = AdjustFieldPropertyValue(pageId, propertyName, propertyValue, metadataViewName);
				string aLViewName = GetALViewName(metadataViewName);
				if (pageOperationInterfaceForPage.SetPageViewFieldProperty(aLViewName, controlName, propertyName, propertyValue2))
				{
					Session.CurrentDesignerExtension.Save();
					MasterPage masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId, null, applyPersonalization: false, metadataViewName);
					NavDesignerControlContext controlContext = masterPageForDesigner.GetControlContext(controlName);
					result = new InvokeDesignerActionResponse
					{
						Info = new SetViewFieldPropertyResponse
						{
							TargetName = controlContext.Control.Name,
							TargetElementDefinition = controlContext.Control,
							ViewName = metadataViewName
						},
						Status = NavDesignerActionStatus.Success
					};
				}
			}
			catch (NavBaseException ex) when (ShouldCatchException(ex))
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00007VT", Category.Extensions, ex);
				throw;
			}
		}
		return result;
	}

	public InvokeDesignerActionResponse MoveViewPageField(int pageId, string metadataViewName, string controlName, string parentControlName, string relativeControlName)
	{
		InvokeDesignerActionResponse invokeDesignerActionResponse = new InvokeDesignerActionResponse
		{
			Status = NavDesignerActionStatus.Error
		};
		if (Session.CurrentDesignerExtension != null)
		{
			try
			{
				INavDesignerPageOperations pageOperationInterfaceForPage = Session.CurrentDesignerExtension.GetPageOperationInterfaceForPage(pageId);
				if (pageOperationInterfaceForPage == null)
				{
					return invokeDesignerActionResponse;
				}
				string aLViewName = GetALViewName(metadataViewName);
				MasterPage masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId, null, applyPersonalization: false, metadataViewName);
				string controlALIdentifier = masterPageForDesigner.GetControlALIdentifier(parentControlName, !masterPageForDesigner.Name.Equals(parentControlName));
				string controlALIdentifier2 = masterPageForDesigner.GetControlALIdentifier(relativeControlName);
				NavDesignerControlContext controlContext = masterPageForDesigner.GetControlContext(controlName);
				if (pageOperationInterfaceForPage.MovePageViewField(aLViewName, controlContext, controlALIdentifier, controlALIdentifier2))
				{
					Session.CurrentDesignerExtension.Save();
					masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId, null, applyPersonalization: false, metadataViewName);
					controlContext = masterPageForDesigner.GetControlContext(controlName);
					invokeDesignerActionResponse.Info = new MoveViewPageFieldResponse
					{
						TargetName = controlContext.Control.Name,
						ParentTargetName = controlContext.ParentGroup?.Name,
						RelativeTargetName = controlContext.PreviousControl?.Name,
						ViewName = metadataViewName
					};
					invokeDesignerActionResponse.Status = NavDesignerActionStatus.Success;
				}
			}
			catch (NavBaseException ex) when (ShouldCatchException(ex))
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00007VU", Category.Extensions, ex);
				throw;
			}
		}
		return invokeDesignerActionResponse;
	}

	public InvokeDesignerActionResponse RestoreViewControlDefaults(int pageId, string viewName)
	{
		ValidatePageIdForRestoreRequest(pageId);
		InvokeDesignerActionResponse invokeDesignerActionResponse = new InvokeDesignerActionResponse
		{
			Status = NavDesignerActionStatus.Error,
			Info = new RestoreDefaultsResponse()
		};
		if (Session.CurrentDesignerExtension != null)
		{
			try
			{
				INavDesignerPageOperations pageOperationInterfaceForPage = Session.CurrentDesignerExtension.GetPageOperationInterfaceForPage(pageId);
				if (pageOperationInterfaceForPage == null)
				{
					return invokeDesignerActionResponse;
				}
				pageOperationInterfaceForPage.ClearViewControlChanges(viewName);
				Session.CurrentDesignerExtension.Save();
				invokeDesignerActionResponse.Status = NavDesignerActionStatus.Success;
			}
			catch (NavDesignerCompilerException exception)
			{
				Session.Diagnostics.SendExceptionTag("00007VV", Category.Extensions, exception);
				throw;
			}
		}
		return invokeDesignerActionResponse;
	}

	public InvokeDesignerActionResponse CreatePageView(DesignerLevels? designerLevel, int pageId, string newViewCaption, string relativeMetadataViewName, ViewInformation information, bool softValidation)
	{
		InvokeDesignerActionResponse invokeDesignerActionResponse = new InvokeDesignerActionResponse
		{
			Status = NavDesignerActionStatus.Error
		};
		if (designerLevel.HasValue)
		{
			StartDesigner(designerLevel.Value);
		}
		try
		{
			if (Session.CurrentDesignerExtension != null)
			{
				INavDesignerPageOperations pageOperationInterfaceForPage = Session.CurrentDesignerExtension.GetPageOperationInterfaceForPage(pageId);
				if (pageOperationInterfaceForPage == null)
				{
					return invokeDesignerActionResponse;
				}
				string uniqueMetadataViewName = GetUniqueMetadataViewName(pageId, newViewCaption);
				string aLViewName = GetALViewName(uniqueMetadataViewName);
				string aLViewName2 = GetALViewName(relativeMetadataViewName);
				bool throwIfFieldNotFound = !softValidation && pageOperationInterfaceForPage.Kind() != SyntaxKind.PageCustomizationObject;
				(string filters, string sorting) filtersAndSorting = GetFiltersAndSorting(pageId, information, throwIfFieldNotFound);
				string item = filtersAndSorting.filters;
				string item2 = filtersAndSorting.sorting;
				PageExtensionLayoutSyntax viewLayoutSyntaxForCreateOperation = GetViewLayoutSyntaxForCreateOperation(pageOperationInterfaceForPage);
				if (!pageOperationInterfaceForPage.AddPageView(aLViewName, aLViewName2, viewLayoutSyntaxForCreateOperation, item, item2))
				{
					return invokeDesignerActionResponse;
				}
				if (!pageOperationInterfaceForPage.SetPageViewProperty(aLViewName, "Caption", newViewCaption))
				{
					return invokeDesignerActionResponse;
				}
				Session.CurrentDesignerExtension.Save();
				MasterPage masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId);
				NavDesignerActionContext actionViewContext = masterPageForDesigner.GetActionViewContext(uniqueMetadataViewName);
				invokeDesignerActionResponse.Status = NavDesignerActionStatus.Success;
				invokeDesignerActionResponse.Info = new AddPageActionResponse
				{
					ActionDefinition = actionViewContext.Action,
					ParentTargetName = actionViewContext.ParentGroup?.Name,
					RelativeTargetName = actionViewContext.PreviousAction?.Name,
					TargetName = actionViewContext.Action.Name,
					ContainerType = ActionContainerType.ViewActions
				};
				Session.StalePages.Add(pageId);
			}
			return invokeDesignerActionResponse;
		}
		catch (NavBaseException ex) when (ShouldCatchException(ex))
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00007VQ", Category.Extensions, ex);
			throw;
		}
		finally
		{
			if (designerLevel.HasValue)
			{
				StopDesigner();
			}
		}
	}

	private static PageExtensionLayoutSyntax GetViewLayoutSyntaxForCreateOperation(INavDesignerPageOperations pageOperations)
	{
		switch (pageOperations.Kind())
		{
		case SyntaxKind.PageObject:
			return null;
		case SyntaxKind.PageExtensionObject:
			return null;
		case SyntaxKind.PageCustomizationObject:
		{
			NavDesignerPageModifications navDesignerPageModifications = (NavDesignerPageModifications)pageOperations;
			return navDesignerPageModifications.LayoutSyntax;
		}
		default:
			throw new NavDesignerUnexpectedValueException(CultureInfo.CurrentCulture, pageOperations.Kind().ToString(), "PageOperationKind");
		}
	}

	public InvokeDesignerActionResponse CopyPageView(DesignerLevels? designerLevel, int pageId, string newViewCaption, string originalMetadataViewName, ViewInformation information)
	{
		InvokeDesignerActionResponse invokeDesignerActionResponse = new InvokeDesignerActionResponse
		{
			Status = NavDesignerActionStatus.Error
		};
		if (designerLevel.HasValue)
		{
			StartDesigner(designerLevel.Value);
		}
		try
		{
			if (Session.CurrentDesignerExtension != null)
			{
				INavDesignerPageOperations pageOperationInterfaceForPage = Session.CurrentDesignerExtension.GetPageOperationInterfaceForPage(pageId);
				if (pageOperationInterfaceForPage == null)
				{
					return invokeDesignerActionResponse;
				}
				string uniqueMetadataViewName = GetUniqueMetadataViewName(pageId, newViewCaption);
				string aLViewName = GetALViewName(uniqueMetadataViewName);
				string aLViewName2 = GetALViewName(originalMetadataViewName);
				bool throwIfFieldNotFound = pageOperationInterfaceForPage.Kind() != SyntaxKind.PageCustomizationObject;
				(string filters, string sorting) filtersAndSorting = GetFiltersAndSorting(pageId, information, throwIfFieldNotFound);
				string item = filtersAndSorting.filters;
				string item2 = filtersAndSorting.sorting;
				PageExtensionLayoutSyntax viewLayoutSyntaxForCopyOperation = GetViewLayoutSyntaxForCopyOperation(pageOperationInterfaceForPage, pageId, originalMetadataViewName);
				if (!pageOperationInterfaceForPage.AddPageView(aLViewName, aLViewName2, viewLayoutSyntaxForCopyOperation, item, item2))
				{
					return invokeDesignerActionResponse;
				}
				pageOperationInterfaceForPage.SetPageViewProperty(aLViewName, "Caption", newViewCaption);
				Session.CurrentDesignerExtension.Save();
				MasterPage masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId);
				NavDesignerActionContext actionViewContext = masterPageForDesigner.GetActionViewContext(uniqueMetadataViewName);
				invokeDesignerActionResponse.Status = NavDesignerActionStatus.Success;
				invokeDesignerActionResponse.Info = new AddPageActionResponse
				{
					ActionDefinition = actionViewContext.Action,
					ParentTargetName = actionViewContext.ParentGroup?.Name,
					RelativeTargetName = actionViewContext.PreviousAction?.Name,
					TargetName = actionViewContext.Action.Name,
					ContainerType = ActionContainerType.ViewActions
				};
				Session.StalePages.Add(pageId);
			}
		}
		catch (NavBaseException ex) when (ShouldCatchException(ex))
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00007VP", Category.Extensions, ex);
			throw;
		}
		finally
		{
			if (designerLevel.HasValue)
			{
				StopDesigner();
			}
		}
		return invokeDesignerActionResponse;
	}

	private static PageExtensionLayoutSyntax GetViewLayoutSyntaxForCopyOperation(INavDesignerPageOperations pageOperations, int pageId, string metatdataViewName)
	{
		GetViewInformationFromMetadataName(metatdataViewName, out var _, out var viewALName);
		PageViewSyntax viewSyntax = pageOperations.GetViewSyntax(viewALName);
		if (viewSyntax != null)
		{
			return viewSyntax.Layout;
		}
		return CreateViewLookupCompilation().GetPageViewSyntaxFromCompilation(pageId, viewALName)?.Layout;
	}

	public InvokeDesignerActionResponse UpdatePageView(DesignerLevels? designerLevel, int pageId, string metadataViewName, ViewInformation information)
	{
		InvokeDesignerActionResponse result = new InvokeDesignerActionResponse
		{
			Status = NavDesignerActionStatus.Error
		};
		if (designerLevel.HasValue)
		{
			StartDesigner(designerLevel.Value);
		}
		try
		{
			if (Session.CurrentDesignerExtension != null)
			{
				INavDesignerPageOperations pageOperationInterfaceForPage = Session.CurrentDesignerExtension.GetPageOperationInterfaceForPage(pageId);
				if (pageOperationInterfaceForPage == null)
				{
					return result;
				}
				string aLViewName = GetALViewName(metadataViewName);
				bool throwIfFieldNotFound = pageOperationInterfaceForPage.Kind() != SyntaxKind.PageCustomizationObject;
				var (propertyValue, propertyValue2) = GetFiltersAndSorting(pageId, information, throwIfFieldNotFound);
				if (!pageOperationInterfaceForPage.SetPageViewProperty(aLViewName, "Filters", propertyValue))
				{
					return result;
				}
				if (!pageOperationInterfaceForPage.SetPageViewProperty(aLViewName, "OrderBy", propertyValue2))
				{
					return result;
				}
				Session.CurrentDesignerExtension.Save();
				MasterPage masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId);
				NavDesignerActionContext actionViewContext = masterPageForDesigner.GetActionViewContext(metadataViewName);
				result = new InvokeDesignerActionResponse
				{
					Info = new SetViewPropertyResponse
					{
						TargetName = actionViewContext.Action.Name,
						TargetElementDefinition = actionViewContext.Action
					},
					Status = NavDesignerActionStatus.Success
				};
			}
			return result;
		}
		catch (NavBaseException ex) when (ShouldCatchException(ex))
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00007XD", Category.Extensions, ex);
			throw;
		}
		finally
		{
			if (designerLevel.HasValue)
			{
				StopDesigner();
			}
		}
	}

	public InvokeDesignerActionResponse MovePageView(int pageId, string metadataViewName, string relativeMetadataViewName)
	{
		InvokeDesignerActionResponse invokeDesignerActionResponse = new InvokeDesignerActionResponse
		{
			Status = NavDesignerActionStatus.Error
		};
		if (Session.CurrentDesignerExtension != null)
		{
			try
			{
				INavDesignerPageOperations pageOperationInterfaceForPage = Session.CurrentDesignerExtension.GetPageOperationInterfaceForPage(pageId);
				if (pageOperationInterfaceForPage == null)
				{
					return invokeDesignerActionResponse;
				}
				string aLViewName = GetALViewName(relativeMetadataViewName);
				MasterPage masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId);
				NavDesignerActionContext actionViewContext = masterPageForDesigner.GetActionViewContext(metadataViewName);
				if (pageOperationInterfaceForPage.MovePageView(actionViewContext, aLViewName))
				{
					Session.CurrentDesignerExtension.Save();
					masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId);
					actionViewContext = masterPageForDesigner.GetActionViewContext(metadataViewName);
					invokeDesignerActionResponse.Info = new MovePageViewResponse
					{
						TargetName = actionViewContext.Action.Name,
						RelativeTargetName = actionViewContext.PreviousAction?.Name
					};
					invokeDesignerActionResponse.Status = NavDesignerActionStatus.Success;
				}
			}
			catch (NavBaseException ex) when (ShouldCatchException(ex))
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00007VQ", Category.Extensions, ex);
				throw;
			}
		}
		return invokeDesignerActionResponse;
	}

	public InvokeDesignerActionResponse SetPageViewProperty(DesignerLevels? designerLevel, int pageId, string metadataViewName, string propertyName, object propertyValue)
	{
		InvokeDesignerActionResponse result = new InvokeDesignerActionResponse
		{
			Status = NavDesignerActionStatus.Error
		};
		if (designerLevel.HasValue)
		{
			StartDesigner(designerLevel.Value);
		}
		if (Session.CurrentDesignerExtension != null)
		{
			try
			{
				INavDesignerPageOperations pageOperationInterfaceForPage = Session.CurrentDesignerExtension.GetPageOperationInterfaceForPage(pageId);
				if (pageOperationInterfaceForPage == null)
				{
					return result;
				}
				string aLViewName = GetALViewName(metadataViewName);
				if (pageOperationInterfaceForPage.SetPageViewProperty(aLViewName, propertyName, propertyValue?.ToString()))
				{
					Session.CurrentDesignerExtension.Save();
					MasterPage masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId);
					NavDesignerActionContext actionViewContext = masterPageForDesigner.GetActionViewContext(metadataViewName);
					result = new InvokeDesignerActionResponse
					{
						Info = new SetViewPropertyResponse
						{
							TargetName = actionViewContext.Action.Name,
							TargetElementDefinition = actionViewContext.Action
						},
						Status = NavDesignerActionStatus.Success
					};
				}
			}
			catch (NavBaseException ex) when (ShouldCatchException(ex))
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00007VR", Category.Extensions, ex);
				throw;
			}
			finally
			{
				if (designerLevel.HasValue)
				{
					StopDesigner();
				}
			}
		}
		return result;
	}

	public InvokeDesignerActionResponse RemovePageView(int pageId, string metadataViewName)
	{
		InvokeDesignerActionResponse invokeDesignerActionResponse = new InvokeDesignerActionResponse
		{
			Info = new RemovePageFieldResponse
			{
				TargetName = metadataViewName
			},
			Status = NavDesignerActionStatus.Error
		};
		if (Session.CurrentDesignerExtension != null)
		{
			try
			{
				INavDesignerPageOperations pageOperationInterfaceForPage = Session.CurrentDesignerExtension.GetPageOperationInterfaceForPage(pageId);
				if (pageOperationInterfaceForPage == null)
				{
					return invokeDesignerActionResponse;
				}
				MasterPage masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId);
				NavDesignerActionContext actionViewContext = masterPageForDesigner.GetActionViewContext(metadataViewName);
				if (pageOperationInterfaceForPage.RemovePageView(actionViewContext))
				{
					Session.CurrentDesignerExtension.Save();
					invokeDesignerActionResponse.Status = NavDesignerActionStatus.Success;
				}
			}
			catch (NavBaseException ex) when (ShouldCatchException(ex))
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00007VS", Category.Extensions, ex);
				throw;
			}
		}
		return invokeDesignerActionResponse;
	}

	public InvokeDesignerActionResponse RestoreViewDefaults(int pageId)
	{
		ValidatePageIdForRestoreRequest(pageId);
		InvokeDesignerActionResponse invokeDesignerActionResponse = new InvokeDesignerActionResponse
		{
			Status = NavDesignerActionStatus.Error,
			Info = new RestoreDefaultsResponse()
		};
		try
		{
			NavDesignerExtension currentDesignerExtension = Session.CurrentDesignerExtension;
			ValidateExtensionLevelForRestoreRequest(currentDesignerExtension);
			RestorePageViewsDefaults(currentDesignerExtension, pageId);
			invokeDesignerActionResponse.Status = NavDesignerActionStatus.Success;
			return invokeDesignerActionResponse;
		}
		catch (NavDesignerCompilerException exception)
		{
			Session.Diagnostics.SendExceptionTag("00007VW", Category.Extensions, exception);
			throw;
		}
	}

	private static void RestorePageViewsDefaults(NavDesignerExtension extension, int pageId)
	{
		extension.GetPageOperationInterfaceForPage(pageId)?.ClearViewChanges();
		extension.Save();
	}

	internal (string filters, string sorting) GetFiltersAndSorting(int pageId, ViewInformation information, bool throwIfFieldNotFound)
	{
		if (information == null)
		{
			return (filters: null, sorting: null);
		}
		MasterPage masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId);
		int sourceTable = masterPageForDesigner.PageProperties.SourceObject.SourceTable;
		string item = ConvertFilter(sourceTable, information.Filters, throwIfFieldNotFound);
		string item2 = ConvertSorting(sourceTable, information.Sorting, throwIfFieldNotFound);
		return (filters: item, sorting: item2);
	}

	private string ConvertFilter(int tableId, ViewFilterInformation[] filters, bool throwIfFieldNotFound)
	{
		if (filters == null || filters.Length == 0)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("where (");
		bool flag = true;
		foreach (ViewFilterInformation viewFilterInformation in filters)
		{
			string fieldName = GetFieldName(tableId, viewFilterInformation.FieldId, throwIfFieldNotFound);
			if (fieldName == null)
			{
				continue;
			}
			if (!flag)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(fieldName.QuoteIdentifierIfNeeded());
			stringBuilder.Append(" = ");
			if (string.Equals(viewFilterInformation.FilterType, "filter", StringComparison.OrdinalIgnoreCase))
			{
				stringBuilder.Append("filter (");
			}
			else
			{
				if (!string.Equals(viewFilterInformation.FilterType, "const", StringComparison.OrdinalIgnoreCase))
				{
					throw new NavDesignerUnexpectedValueException(CultureInfo.CurrentCulture, viewFilterInformation.FilterType, "FilterType");
				}
				stringBuilder.Append("const (");
			}
			stringBuilder.Append(viewFilterInformation.FilterValue);
			stringBuilder.Append(")");
			flag = false;
		}
		stringBuilder.Append(")");
		return stringBuilder.ToString();
	}

	private string ConvertSorting(int tableId, ViewSortingInformation sorting, bool throwIfFieldNotFound)
	{
		if (sorting == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (sorting.Ascending)
		{
			stringBuilder.Append("ascending (");
		}
		else
		{
			stringBuilder.Append("descending (");
		}
		bool flag = true;
		int[] fieldIds = sorting.FieldIds;
		foreach (int fieldId in fieldIds)
		{
			string fieldName = GetFieldName(tableId, fieldId, throwIfFieldNotFound);
			if (fieldName != null)
			{
				if (!flag)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(fieldName.QuoteIdentifierIfNeeded());
				flag = false;
			}
		}
		stringBuilder.Append(")");
		return stringBuilder.ToString();
	}

	private string GetUniqueMetadataViewName(int pageId, string viewMetadataName)
	{
		NavAppManifest manifest = Session.CurrentDesignerExtension.Manifest;
		string text = CompilationUtilities.CreateEscapedViewName(manifest.AppId, viewMetadataName);
		MasterPage masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId);
		while (masterPageForDesigner.ViewExistsOnPage(text))
		{
			text = CompilationUtilities.CreateEscapedViewName(manifest.AppId, viewMetadataName + new Random().Next(10000, 99999));
		}
		return text;
	}

	internal static string GetALViewName(string metadataName)
	{
		GetViewInformationFromMetadataName(metadataName, out var _, out var viewALName);
		return viewALName;
	}

	private static void GetViewInformationFromMetadataName(string metadataName, out string appId, out string viewALName)
	{
		appId = null;
		viewALName = null;
		if (metadataName != null)
		{
			ImmutableArray<string> immutableArray = CompilationUtilities.UnescapeViewName(metadataName);
			if (immutableArray.Length != 2)
			{
				throw new NavDesignerUnexpectedValueException(CultureInfo.CurrentCulture, metadataName, "View Metadata Name");
			}
			appId = immutableArray[0];
			viewALName = immutableArray[1];
		}
	}

	private static Compilation CreateViewLookupCompilation()
	{
		Compilation compilation = Compilation.Create("ViewLookupCompilation", null, null, null, null, null, null, NavGlobal.SystemTenant.DotNetResolverFactory);
		return compilation.WithReferenceLoader(new NavAppSymbolReferenceLoader(compilation.ModuleInfo, NavGlobal.SystemTenant.SymbolReferenceCache, NavCurrentThread.Session.Tenant.Id)).WithReferences(NavDesignerUtil.GetPersonalizationReferences());
	}

	public NavDesignerManagementTasks(NavSession session)
		: this(session, new NavDesignerExtensionFactory())
	{
	}

	internal NavDesignerManagementTasks(NavSession session, INavDesignerExtensionFactory designerExtensionFactory)
	{
		Session = session ?? throw new ArgumentNullException("session");
		DesignerExtensionFactory = designerExtensionFactory ?? throw new ArgumentNullException("designerExtensionFactory");
	}

	private static bool IsPropertyAPageControlReferenceProperty(string propertyName)
	{
		return PageControlReferenceProperties.Exists((string prop) => prop.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
	}

	public InvokeDesignerActionResponse AddPageField(int pageId, string parentControlName, string relativeControlName, int fieldId, bool isTableField)
	{
		InvokeDesignerActionResponse invokeDesignerActionResponse = new InvokeDesignerActionResponse
		{
			Status = NavDesignerActionStatus.Error
		};
		if (Session.CurrentDesignerExtension != null)
		{
			try
			{
				INavDesignerPageOperations pageOperationInterfaceForPage = Session.CurrentDesignerExtension.GetPageOperationInterfaceForPage(pageId);
				if (pageOperationInterfaceForPage == null)
				{
					return invokeDesignerActionResponse;
				}
				MasterPage masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId);
				string controlALIdentifier = masterPageForDesigner.GetControlALIdentifier(parentControlName, !masterPageForDesigner.Name.Equals(parentControlName));
				string controlALIdentifier2 = masterPageForDesigner.GetControlALIdentifier(relativeControlName);
				int sourceTable = masterPageForDesigner.PageProperties.SourceObject.SourceTable;
				bool flag = false;
				string text;
				if (!isTableField || IsPersonalizationOrConfigurationLevel(NavCurrentThread.Session.CurrentDesignerLevel))
				{
					NavDesignerControlContext navDesignerControlContext = (isTableField ? masterPageForDesigner.GetControlContext((ControlDefinition control) => string.Equals(control.DataColumnName?.ToString(CultureInfo.InvariantCulture), fieldId.ToString(CultureInfo.InvariantCulture))) : masterPageForDesigner.GetControlContext((ControlDefinition control) => string.Equals(control.ID.ToString(CultureInfo.InvariantCulture), fieldId.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)));
					if (navDesignerControlContext?.Control == null)
					{
						return invokeDesignerActionResponse;
					}
					text = navDesignerControlContext.Control.Name;
					if (pageOperationInterfaceForPage.SetPageFieldProperty(navDesignerControlContext.GetControlALIdentifier(), "Visible", true))
					{
						flag = pageOperationInterfaceForPage.MovePageField(navDesignerControlContext, controlALIdentifier, controlALIdentifier2);
					}
				}
				else
				{
					string fieldName = GetFieldName(sourceTable, fieldId, throwIfNotFound: true);
					text = fieldName;
					bool flag2 = masterPageForDesigner.ControlExistsOnPage(fieldName);
					while (flag2)
					{
						text = fieldName + new Random().Next(10000, 99999);
						flag2 = masterPageForDesigner.ControlExistsOnPage(text);
					}
					flag = pageOperationInterfaceForPage.AddPageField(fieldName, text, controlALIdentifier, controlALIdentifier2);
				}
				if (flag)
				{
					Session.CurrentDesignerExtension.Save();
					masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId);
					NavDesignerControlContext controlContext = masterPageForDesigner.GetControlContext(text);
					SetControlDefaultCaption(masterPageForDesigner, controlContext);
					MetaField metaField = ((sourceTable != 0) ? Session.MetadataProvider.GetTableMetadata(sourceTable).GetFieldsById(fieldId) : null);
					invokeDesignerActionResponse.Info = new AddPageFieldResponse
					{
						TargetDefinition = controlContext.Control,
						TargetName = controlContext.Control.Name,
						ParentTargetName = controlContext.ParentGroup?.Name,
						RelativeTargetName = controlContext.PreviousControl?.Name,
						MetaField = metaField
					};
					invokeDesignerActionResponse.Status = NavDesignerActionStatus.Success;
				}
				Session.StalePages.Add(pageId);
			}
			catch (NavBaseException ex) when (ShouldCatchException(ex))
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000PO", Category.Extensions, ex);
				throw;
			}
		}
		return invokeDesignerActionResponse;
	}

	public InvokeDesignerActionResponse MovePageField(int pageId, string controlName, string parentControlName, string relativeControlName)
	{
		InvokeDesignerActionResponse invokeDesignerActionResponse = new InvokeDesignerActionResponse
		{
			Status = NavDesignerActionStatus.Error
		};
		if (Session.CurrentDesignerExtension != null)
		{
			try
			{
				INavDesignerPageOperations pageOperationInterfaceForPage = Session.CurrentDesignerExtension.GetPageOperationInterfaceForPage(pageId);
				if (pageOperationInterfaceForPage == null)
				{
					return invokeDesignerActionResponse;
				}
				MasterPage masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId);
				string controlALIdentifier = masterPageForDesigner.GetControlALIdentifier(parentControlName, !masterPageForDesigner.Name.Equals(parentControlName));
				string controlALIdentifier2 = masterPageForDesigner.GetControlALIdentifier(relativeControlName);
				NavDesignerControlContext controlContext = masterPageForDesigner.GetControlContext(controlName);
				if (pageOperationInterfaceForPage.MovePageField(controlContext, controlALIdentifier, controlALIdentifier2))
				{
					Session.CurrentDesignerExtension.Save();
					masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId);
					controlContext = masterPageForDesigner.GetControlContext(controlName);
					invokeDesignerActionResponse.Info = new MovePageFieldResponse
					{
						TargetName = controlContext.Control.Name,
						ParentTargetName = controlContext.ParentGroup?.Name,
						RelativeTargetName = controlContext.PreviousControl?.Name
					};
					invokeDesignerActionResponse.Status = NavDesignerActionStatus.Success;
				}
			}
			catch (NavBaseException ex) when (ShouldCatchException(ex))
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000PL", Category.Extensions, ex);
				throw;
			}
		}
		return invokeDesignerActionResponse;
	}

	public InvokeDesignerActionResponse RemovePageField(int pageId, string controlName)
	{
		InvokeDesignerActionResponse invokeDesignerActionResponse = new InvokeDesignerActionResponse
		{
			Info = new RemovePageFieldResponse
			{
				TargetName = controlName
			},
			Status = NavDesignerActionStatus.Error
		};
		if (Session.CurrentDesignerExtension != null)
		{
			try
			{
				INavDesignerPageOperations pageOperationInterfaceForPage = Session.CurrentDesignerExtension.GetPageOperationInterfaceForPage(pageId);
				if (pageOperationInterfaceForPage == null)
				{
					return invokeDesignerActionResponse;
				}
				MasterPage masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId);
				NavDesignerControlContext controlContext = masterPageForDesigner.GetControlContext(controlName);
				if (pageOperationInterfaceForPage.RemovePageField(controlContext))
				{
					Session.CurrentDesignerExtension.Save();
					invokeDesignerActionResponse.Status = NavDesignerActionStatus.Success;
				}
			}
			catch (NavBaseException ex) when (ShouldCatchException(ex))
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000PM", Category.Extensions, ex);
				throw;
			}
		}
		return invokeDesignerActionResponse;
	}

	public InvokeDesignerActionResponse SetPageProperty(int pageId, string propertyName, object propertyValue)
	{
		InvokeDesignerActionResponse result = new InvokeDesignerActionResponse
		{
			Status = NavDesignerActionStatus.Error
		};
		if (Session.CurrentDesignerExtension != null)
		{
			try
			{
				INavDesignerPageOperations pageOperationInterfaceForPage = Session.CurrentDesignerExtension.GetPageOperationInterfaceForPage(pageId);
				if (pageOperationInterfaceForPage == null)
				{
					return result;
				}
				if (pageOperationInterfaceForPage.SetPageProperty(propertyName, propertyValue))
				{
					Session.CurrentDesignerExtension.Save();
					result = new InvokeDesignerActionResponse
					{
						Info = new SetPagePropertyResponse
						{
							TargetName = pageId.ToString(CultureInfo.InvariantCulture),
							TargetElementDefinition = null
						},
						Status = NavDesignerActionStatus.Success
					};
				}
			}
			catch (NavBaseException ex) when (ShouldCatchException(ex))
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00007NE", Category.Extensions, ex);
				throw ex;
			}
		}
		return result;
	}

	public InvokeDesignerActionResponse SetFieldProperty(int pageId, string controlName, string propertyName, object propertyValue)
	{
		InvokeDesignerActionResponse result = new InvokeDesignerActionResponse
		{
			Status = NavDesignerActionStatus.Error
		};
		if (Session.CurrentDesignerExtension != null)
		{
			try
			{
				INavDesignerPageOperations pageOperationInterfaceForPage = Session.CurrentDesignerExtension.GetPageOperationInterfaceForPage(pageId);
				if (pageOperationInterfaceForPage == null)
				{
					return result;
				}
				MasterPage masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId);
				string controlALIdentifier = masterPageForDesigner.GetControlALIdentifier(controlName);
				string propertyValue2 = AdjustFieldPropertyValue(pageId, propertyName, propertyValue, null);
				if (pageOperationInterfaceForPage.SetPageFieldProperty(controlALIdentifier, propertyName, propertyValue2))
				{
					Session.CurrentDesignerExtension.Save();
					masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId);
					NavDesignerControlContext controlContext = masterPageForDesigner.GetControlContext(controlName);
					result = new InvokeDesignerActionResponse
					{
						Info = new SetFieldPropertyResponse
						{
							TargetName = controlContext.Control.Name,
							TargetElementDefinition = controlContext.Control
						},
						Status = NavDesignerActionStatus.Success
					};
				}
			}
			catch (NavBaseException ex) when (ShouldCatchException(ex))
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000011J", Category.Extensions, ex);
				throw;
			}
		}
		return result;
	}

	public InvokeDesignerActionResponse StartDesigner(DesignerLevels designerLevel, ConfigurationDesignerMode configurationMode = ConfigurationDesignerMode.Tenant)
	{
		InvokeDesignerActionResponse invokeDesignerActionResponse = new InvokeDesignerActionResponse
		{
			Status = NavDesignerActionStatus.Success,
			Info = new StartDesignerResponse()
		};
		try
		{
			switch (designerLevel)
			{
			case DesignerLevels.Personalization:
				Session.CurrentDesignerExtension = DesignerExtensionFactory.CreatePersonalizationDesignerExtension();
				break;
			case DesignerLevels.Configuration:
				Session.CurrentDesignerExtension = DesignerExtensionFactory.CreateConfigurationDesignerExtension(Session.ProfileKey, configurationMode);
				break;
			case DesignerLevels.Development:
			{
				if (Session.ApplicationExtension != Guid.Empty && Session.NavAppGroup.OrderedAppMetadata.Any((NavAppRuntimeMetadata x) => ((WrappedValue<Guid>)(object)x.AppId).Value == Session.ApplicationExtension))
				{
					Session.CurrentDesignerExtension = DesignerExtensionFactory.CreateDevelopmentDesignerExtension(AppId.op_Implicit(Session.ApplicationExtension));
					break;
				}
				NavAppRuntimeMetadata val = NavEnvironment.Instance.NavAppMetadataRetriever.GetAllMetadataVisibleToTenant(Session.Tenant.Id).SingleOrDefault((NavAppRuntimeMetadata x) =>
				{
					NavAppPublishProperties publishProperties = NavEnvironment.Instance.NavAppMetadataRetriever.GetPublishProperties(x);
					return publishProperties.PackageType == NavAppPackageType.Designer;
				});
				Session.CurrentDesignerExtension = DesignerExtensionFactory.CreateDevelopmentDesignerExtension((val != null) ? val.AppId : null);
				break;
			}
			case DesignerLevels.Inspector:
				Session.CurrentDesignerExtension = null;
				break;
			case DesignerLevels.None:
			case DesignerLevels.All:
				string.Format(CultureInfo.InvariantCulture, "The designer level '{0}' is not a valid value for creating a designer extension.", designerLevel.ToString());
				invokeDesignerActionResponse.Status = NavDesignerActionStatus.Error;
				break;
			}
		}
		catch (NavDesignerCompilerException exception)
		{
			Session.Diagnostics.SendExceptionTag("00001OO", Category.Extensions, exception);
			invokeDesignerActionResponse.Status = NavDesignerActionStatus.Error;
		}
		if (invokeDesignerActionResponse.Status == NavDesignerActionStatus.Success)
		{
			Session.CurrentDesignerLevel = designerLevel;
		}
		return invokeDesignerActionResponse;
	}

	public InvokeDesignerActionResponse RestoreDefaults(int pageId)
	{
		ValidatePageIdForRestoreRequest(pageId);
		InvokeDesignerActionResponse invokeDesignerActionResponse = new InvokeDesignerActionResponse
		{
			Status = NavDesignerActionStatus.Error,
			Info = new RestoreDefaultsResponse()
		};
		try
		{
			NavDesignerExtension currentDesignerExtension = Session.CurrentDesignerExtension;
			ValidateExtensionLevelForRestoreRequest(currentDesignerExtension);
			RestorePageControlsDefaults(currentDesignerExtension, pageId);
			RestorePageActionsDefaults(currentDesignerExtension, pageId);
			RestorePageViewsDefaults(currentDesignerExtension, pageId);
			invokeDesignerActionResponse.Status = NavDesignerActionStatus.Success;
			return invokeDesignerActionResponse;
		}
		catch (NavDesignerCompilerException exception)
		{
			Session.Diagnostics.SendExceptionTag("00001F7", Category.Extensions, exception);
			throw;
		}
	}

	public InvokeDesignerActionResponse RestoreControlDefaults(int pageId)
	{
		ValidatePageIdForRestoreRequest(pageId);
		InvokeDesignerActionResponse invokeDesignerActionResponse = new InvokeDesignerActionResponse
		{
			Status = NavDesignerActionStatus.Error,
			Info = new RestoreDefaultsResponse()
		};
		try
		{
			NavDesignerExtension currentDesignerExtension = Session.CurrentDesignerExtension;
			ValidateExtensionLevelForRestoreRequest(currentDesignerExtension);
			RestorePageControlsDefaults(currentDesignerExtension, pageId);
			invokeDesignerActionResponse.Status = NavDesignerActionStatus.Success;
			return invokeDesignerActionResponse;
		}
		catch (NavDesignerCompilerException exception)
		{
			Session.Diagnostics.SendExceptionTag("00007NH", Category.Extensions, exception);
			throw;
		}
	}

	private static void ValidatePageIdForRestoreRequest(int pageId)
	{
		string objectName = NavGlobal.NCLMetadata.GetObjectName(new ApplicationObjectId(ObjectType.Page, pageId));
		if (string.IsNullOrEmpty(objectName))
		{
			throw new NavMetadataNotFoundException(ObjectType.Page, pageId);
		}
	}

	private static void ValidateExtensionLevelForRestoreRequest(NavDesignerExtension extension)
	{
		if (extension is NavDevelopmentDesignerExtension)
		{
			throw new InvalidOperationException("The Restore Page Defaults action in not applicable in this context: {0}");
		}
	}

	private static void RestorePageControlsDefaults(NavDesignerExtension extension, int pageId)
	{
		List<int> list = new List<int>();
		list.Add(pageId);
		PageDefinition pageDefinition = extension.GetPageDefinition(pageId);
		if (pageDefinition != null)
		{
			IEnumerable<int> collection = from InfopartPageDefinition p in pageDefinition.FindAll((ElementDefinition ed) => ed is InfopartPageDefinition)
				select p.PagePartID;
			list.AddRange(collection);
		}
		foreach (int item in list)
		{
			extension.GetPageOperationInterfaceForPage(item)?.ClearControlChanges();
		}
		extension.Save();
	}

	public InvokeDesignerActionResponse StopDesigner()
	{
		InvokeDesignerActionResponse result = new InvokeDesignerActionResponse
		{
			Status = NavDesignerActionStatus.Success,
			Info = new StopDesignerResponse()
		};
		if (Session.ApplicationExtension == Guid.Empty)
		{
			Session.CurrentDesignerExtension = null;
		}
		Session.CurrentDesignerLevel = DesignerLevels.None;
		return result;
	}

	private string GetFieldName(int tableId, int fieldId, bool throwIfNotFound)
	{
		NavDesignerTableField navDesignerTableField;
		if (Session.CurrentDesignerExtension.Objects.Tables.ContainsKey(tableId))
		{
			NavDesignerTable navDesignerTable = Session.CurrentDesignerExtension.Objects.Tables[tableId];
			navDesignerTableField = navDesignerTable.Fields.SingleOrDefault((NavDesignerTableField f) => f.Number == fieldId);
		}
		else
		{
			string tableName = NavGlobal.NCLMetadata.GetObjectName(new ApplicationObjectId(ObjectType.Table, tableId));
			navDesignerTableField = Session.CurrentDesignerExtension.Objects.TableExtensions.Values.SingleOrDefault((NavDesignerTableExtension t) => t.ExtendsTableName.Equals(tableName))?.Fields.SingleOrDefault((NavDesignerTableField f) => f.Number == fieldId);
		}
		if (navDesignerTableField != null)
		{
			return navDesignerTableField.Name;
		}
		MetaTable tableMetadata = Session.MetadataProvider.GetTableMetadata(tableId);
		MetaField fieldsById = tableMetadata.GetFieldsById(fieldId);
		if ((fieldsById == null) & throwIfNotFound)
		{
			throw NavMetaFieldNotFoundException.Create(CultureInfo.CurrentCulture, fieldId, tableId);
		}
		return fieldsById.Name;
	}

	private static bool IsPersonalizationOrConfigurationLevel(DesignerLevels level)
	{
		return level.In(DesignerLevels.Personalization, DesignerLevels.Configuration);
	}

	private static void SetControlDefaultCaption(MasterPage masterPage, NavDesignerControlContext controlContext)
	{
		if (string.IsNullOrWhiteSpace(controlContext.Control.Caption) && controlContext.Control is ControlDefinition controlDefinition)
		{
			Dictionary<string, DataFieldDefinition> dictionary = masterPage.Expressions.ToDictionary((DataFieldDefinition ex) => ex.Name);
			DataFieldDefinition dataFieldDefinition = (dictionary.ContainsKey(controlDefinition.DataColumnName) ? dictionary[controlDefinition.DataColumnName] : null);
			controlDefinition.Caption = ((!string.IsNullOrEmpty(dataFieldDefinition?.SourceExpression)) ? dataFieldDefinition.SourceExpression : controlContext.GetControlALIdentifier());
		}
	}

	private string AdjustFieldPropertyValue(int pageId, string propertyName, object propertyValue, string viewName)
	{
		string text = propertyValue?.ToString();
		if (IsPropertyAPageControlReferenceProperty(propertyName))
		{
			MasterPage masterPageForDesigner = Session.MetadataProvider.GetMasterPageForDesigner(pageId, null, applyPersonalization: false, viewName);
			text = masterPageForDesigner.GetControlALIdentifier(text);
		}
		return text;
	}

	private static bool ShouldCatchException(NavBaseException ex)
	{
		if (ex is NavDesignerCompilerException || ex is NavDesignerUnexpectedValueException)
		{
			return true;
		}
		if (IsPersonalizationOrConfigurationLevel(NavCurrentThread.Session.CurrentDesignerLevel) && (ex is NavNCLControlMetadataNotFoundException || ex is NavNCLActionMetadataNotFoundException || ex is NavNCLViewMetadataNotFoundException))
		{
			return true;
		}
		return false;
	}
}
