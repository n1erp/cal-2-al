using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSystemCodeunitUIHelperTriggers(TreeObject parent) : NavSystemCodeunit(parent, 2000000004)
{
	internal enum CodeunitMethod
	{
		AutoFormatTranslate = 1,
		GetDefaultRoundingPrecision,
		CaptionClassTranslate,
		GetCueStyle,
		GetSystemIndicator
	}

	private static NCLOptionMetadata systemIndicatorStylesMetadata;

	private NavText emptyFormatTranslateValue;

	private readonly Dictionary<Tuple<int, NavText>, NavText> autoFormatTranslateCache = new Dictionary<Tuple<int, NavText>, NavText>();

	private readonly Dictionary<Tuple<int, NavText>, NavText> captionClassTranslateCache = new Dictionary<Tuple<int, NavText>, NavText>();

	protected override string GetMethodName(int methodId)
	{
		CodeunitMethod codeunitMethod = (CodeunitMethod)methodId;
		return codeunitMethod.ToString();
	}

	public NavText InvokeAutoFormatTranslate(int autoFormatType, string autoFormatExpression)
	{
		return InvokeAutoFormatTranslate(autoFormatType, new NavText(autoFormatExpression));
	}

	public NavText InvokeAutoFormatTranslate(int autoFormatType, NavText autoFormatExpression)
	{
		NavText result = NavText.Default(int.MaxValue);
		if (autoFormatType == 1 && autoFormatExpression.Value.Length == 0)
		{
			if (emptyFormatTranslateValue == null)
			{
				Invoke(1, new object[3]
				{
					1,
					NavText.Empty,
					new ByRef<NavText>(() => result, (NavText navText) =>
					{
						result = navText;
					})
				});
				emptyFormatTranslateValue = result;
			}
			return emptyFormatTranslateValue;
		}
		Tuple<int, NavText> key = new Tuple<int, NavText>(autoFormatType, autoFormatExpression);
		if (autoFormatTranslateCache.TryGetValue(key, out var value))
		{
			return value;
		}
		Invoke(1, new object[3]
		{
			autoFormatType,
			autoFormatExpression,
			new ByRef<NavText>(() => result, (NavText navText) =>
			{
				result = navText;
			})
		});
		autoFormatTranslateCache[key] = result;
		return result;
	}

	public Decimal18 InvokeGetDefaultRoundingPrecision()
	{
		Decimal18 result = NavDecimalHelper.FallbackRoundPrecision();
		Invoke(2, new object[1]
		{
			new ByRef<Decimal18>(() => result, (Decimal18 value) =>
			{
				result = value;
			})
		});
		return result;
	}

	public NavText InvokeCaptionClassTranslate(int language, NavText captionExpression)
	{
		Tuple<int, NavText> key = new Tuple<int, NavText>(language, captionExpression);
		if (captionClassTranslateCache.TryGetValue(key, out var value))
		{
			return value;
		}
		NavText result = NavText.Default(int.MaxValue);
		Invoke(3, new object[3]
		{
			language,
			captionExpression,
			new ByRef<NavText>(() => result, (NavText navText) =>
			{
				result = navText;
			})
		});
		captionClassTranslateCache[key] = result;
		return result;
	}

	public void ResetCaptionClassCache()
	{
		captionClassTranslateCache.Clear();
	}

	public NavText InvokeGetCueStyle(NavInteger tableId, NavInteger fieldNo, NavDecimal cueValue)
	{
		NavText result = NavText.Default(int.MaxValue);
		Invoke(4, new object[4]
		{
			tableId,
			fieldNo,
			cueValue,
			new ByRef<NavText>(() => result, (NavText value) =>
			{
				result = value;
			})
		});
		return result;
	}

	public SystemIndicator InvokeGetSystemIndicator()
	{
		if (systemIndicatorStylesMetadata == null)
		{
			systemIndicatorStylesMetadata = NCLOptionMetadata.Create(SystemIndicator.StyleOptionString);
		}
		NavText text = NavText.Empty;
		NavOption style = NavOption.Create(systemIndicatorStylesMetadata, 0);
		object[] args = new object[2]
		{
			new ByRef<NavText>(() => text, (NavText value) =>
			{
				text = value;
			}),
			new ByRef<NavOption>(() => style, (NavOption value) =>
			{
				style = value;
			})
		};
		NavCurrentThread.Session.TransactionScope(() =>
		{
			Invoke(5, args);
		});
		return new SystemIndicator(text.Value, (SystemIndicatorStyle)style.Value);
	}
}
