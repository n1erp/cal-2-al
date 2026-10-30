using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using Microsoft.Dynamics.Nav.EventSubscription;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLMetaCodeunit : NCLMetaApplicationObject, INavEventSubscriber
{
	private delegate NavCodeunit CreateNavCodeunit(ITreeObject parent);

	private int? tableId;

	private CodeunitSubType? subtype;

	private NavCodeunitOptions? codeUnitOptions;

	private NavCodeunitOptionsAttribute options;

	private readonly Lazy<NavEventSubscriberReflectionWrapper> subscriberReflectionWrapper;

	protected override Type ApplicationObjectConstructorDelegateType => typeof(CreateNavCodeunit);

	public int CodeunitId => ObjectId;

	public NavCodeunitOptionsAttribute Options
	{
		get
		{
			if (options == null)
			{
				LoadOptionsFromAttributeOrInstance();
			}
			return options;
		}
	}

	public bool IsSingleInstance
	{
		get
		{
			if (CodeunitOptions.HasValue)
			{
				return (CodeunitOptions.Value & NavCodeunitOptions.SingleInstance) != 0;
			}
			return Options.IsSingleInstance;
		}
	}

	public int TableId
	{
		get
		{
			if (!tableId.HasValue)
			{
				LoadLegacyStyleAttributes();
			}
			return tableId.GetValueOrDefault();
		}
	}

	private NavCodeunitOptions? CodeunitOptions
	{
		get
		{
			if (!codeUnitOptions.HasValue)
			{
				LoadLegacyStyleAttributes();
			}
			return codeUnitOptions;
		}
	}

	public CodeunitSubType Subtype
	{
		get
		{
			if (!subtype.HasValue)
			{
				LoadLegacyStyleAttributes();
			}
			return subtype.GetValueOrDefault();
		}
	}

	public bool IsEventManualBinding
	{
		get
		{
			if (options == null)
			{
				LoadOptionsFromAttributeOrInstance(onlyLoadFromAttribute: true);
			}
			if (options != null)
			{
				return options.IsEventManualBinding;
			}
			return false;
		}
	}

	public bool OriginatesFromBase => MetadataAppGroup == NavAppGroup.BaseGroup;

	public NavEventSubscriberReflectionWrapper SubscriberReflectionWrapper => subscriberReflectionWrapper.Value;

	private NCLMetaCodeunit(INCLMetaApplicationObjectLoader loader, int codeunitId, NavAppGroup metadataAppGroup, int appDependencyOrder)
		: base(loader, ObjectType.CodeUnit, codeunitId, metadataAppGroup, appDependencyOrder)
	{
		subscriberReflectionWrapper = new Lazy<NavEventSubscriberReflectionWrapper>(() => new NavEventSubscriberReflectionWrapper(ApplicationObjectClrType), LazyThreadSafetyMode.PublicationOnly);
	}

	internal static NCLMetaCodeunit CreateEmptyNCLMetaCodeunit(INCLMetaApplicationObjectLoader loader, int codeunitId, NavAppGroup appGroup, int appDependencyOrder = -1)
	{
		return new NCLMetaCodeunit(loader, codeunitId, appGroup, appDependencyOrder);
	}

	private void LoadOptionsFromAttributeOrInstance(bool onlyLoadFromAttribute = false)
	{
		if (options != null)
		{
			return;
		}
		lock (this)
		{
			if (options == null)
			{
				options = ApplicationObjectClrType.GetCustomAttribute<NavCodeunitOptionsAttribute>();
				if ((options != null) | onlyLoadFromAttribute)
				{
					tableId = options?.TableId;
					subtype = options?.Subtype;
				}
				else
				{
					options = GetCodeunitAttributesFromNewInstance();
				}
			}
		}
	}

	private NavCodeunitOptionsAttribute GetCodeunitAttributesFromNewInstance()
	{
		NavCompany navCompany = NavCurrentThread.Session?.Company;
		bool isSingleInstance;
		int num;
		if (navCompany == null)
		{
			NavCodeunit navCodeunit = (NavCodeunit)FormatterServices.GetUninitializedObject(ApplicationObjectClrType);
			isSingleInstance = navCodeunit.IsSingleInstance;
			num = navCodeunit.TableId;
		}
		else
		{
			NavCodeunit navCodeunit2 = InternalCreateInstance(null);
			isSingleInstance = navCodeunit2.IsSingleInstance;
			num = navCodeunit2.TableId;
			if (!isSingleInstance)
			{
				navCodeunit2.Dispose();
			}
		}
		return new NavCodeunitOptionsAttribute(isSingleInstance ? NavCodeunitOptions.SingleInstance : ((NavCodeunitOptions)0), num, CodeunitSubType.Normal);
	}

	private NavCodeunit InternalCreateInstance(bool? isSingleInstance)
	{
		CreateNavCodeunit createNavCodeunit = (CreateNavCodeunit)ApplicationObjectConstructor;
		NavCompany company = NavCurrentThread.Session.Company;
		NavCodeunit navCodeunit = createNavCodeunit(company.SharedObjects);
		if (!isSingleInstance.HasValue)
		{
			isSingleInstance = navCodeunit.IsSingleInstance;
		}
		if (isSingleInstance.Value)
		{
			lock (company.SingleInstanceCodeunits)
			{
				NavCodeunitHandle value = new NavCodeunitHandle(company, navCodeunit);
				company.SingleInstanceCodeunits.Add(ApplicationObjectClrType, value);
			}
		}
		return navCodeunit;
	}

	public NavCodeunit CreateObjectInstance()
	{
		if (Options.IsSingleInstance)
		{
			NavCompany company = NavCurrentThread.Session.Company;
			lock (company.SingleInstanceCodeunits)
			{
				if (company.SingleInstanceCodeunits.TryGetValue(ApplicationObjectClrType, out var value))
				{
					return value.Target;
				}
				return InternalCreateInstance(true);
			}
		}
		return InternalCreateInstance(false);
	}

	private void LoadLegacyStyleAttributes()
	{
		CodeunitParseHelper.TryParseCodeunitOptionsAttribute(CodeunitId, out subtype, out tableId, out codeUnitOptions);
	}
}
