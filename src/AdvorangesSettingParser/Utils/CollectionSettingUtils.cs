using System;
using System.Collections.Generic;
using System.Linq;
using AdvorangesSettingParser.Implementation;
using AdvorangesSettingParser.Interfaces;
using AdvorangesSettingParser.Results;
using AdvorangesUtils;
using Context = AdvorangesSettingParser.Implementation.CollectionModificationContext;

namespace AdvorangesSettingParser.Utils
{
	/// <summary>
	/// Utilities for collection settings.
	/// </summary>
	public static class CollectionSettingUtils
	{
		/// <summary>
		/// Attempst to modify the collection in the specified way.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="setting"></param>
		/// <param name="value"></param>
		/// <param name="context"></param>
		/// <param name="getter"></param>
		/// <returns></returns>
		public static IResult TrySetValue<T>(this IValueParser<T> setting, string value, Context context, Func<ICollection<T>> getter)
		{
			var cmActionResult = setting.TryGetCMAction(context, ref value);
			if (!cmActionResult.IsSuccess)
			{
				return cmActionResult;
			}
			var convertResult = setting.TryConvertValue(value, out var result);
			if (!convertResult.IsSuccess)
			{
				return convertResult;
			}
			return setting.ModifyCollection(getter(), result, context);
		}
		/// <summary>
		/// Sets the action to use on the context.
		/// </summary>
		/// <param name="setting"></param>
		/// <param name="context"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static IResult TryGetCMAction<T>(this IValueParser<T> setting, Context context, ref string value)
		{
			var split = value.Split(new[] { ' ' }, 2);
			//Do not allow numbers being parsed as the enum in case someone wants to add numbers to a collection
			var names = Enum.GetNames(typeof(CMAction));
			if (split.Length == 1)
			{
				//Only return success if this only value is NOT CMAction
				//b/c if someone messes up quotes it would attempt to set this otherwise
				var valid = !names.CaseInsContains(split[0]);
				return valid
					? SetValueResult.FromSuccess(setting, value, $"Defaulting action to {CMAction.Toggle}.")
					: SetValueResult.FromError(setting, value, "Cannot provide only an action.");
			}
			if (names.CaseInsContains(split[0]))
			{
				context.Action = (CMAction)Enum.Parse(typeof(CMAction), split[0], true);
				value = split[1];
				return SetValueResult.FromSuccess(setting, value, $"Set action to {context.Action}.");
			}
			else
			{
				return SetValueResult.FromSuccess(setting, value, $"Defaulting action to {CMAction.Toggle}.");
			}
		}
		/// <summary>
		/// Modifies the collection in the specified way.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="setting"></param>
		/// <param name="source"></param>
		/// <param name="value"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public static IResult ModifyCollection<T>(this IValueParser<T> setting, ICollection<T> source, T value, Context context)
		{
			setting.ThrowIfInvalid(value);

			bool success;
			switch (context.Action)
			{
				case CMAction.Toggle:
					var rCount = source.RemoveAll(value, setting.EqualityComparer, context.MaxRemovalCount);
					if (rCount <= 0) //Only add if nothing was removed
					{
						source.Add(value);
					}
					success = true;
					break;
				case CMAction.Add:
					source.Add(value);
					success = true;
					break;
				case CMAction.AddIfMissing:
					var contains = source.Contains(value, setting.EqualityComparer);
					if (!contains) //Only add if not contained in collection
					{
						source.Add(value);
					}
					success = !contains;
					break;
				case CMAction.Remove:
					success = source.RemoveAll(value, setting.EqualityComparer, context.MaxRemovalCount) > 0; //Failure if removed nothing
					break;
				default:
					throw new ArgumentException("Invalid action supplied.", nameof(context));
			}

			return success
				? SetValueResult.FromSuccess(setting, value, $"Successfully {context.ActionString}.")
				: SetValueResult.FromError(setting, value, $"Already {context.ActionString}.");
		}
		/// <summary>
		/// Removes all values which equal the supplied value using the supplied equality comparer.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="value"></param>
		/// <param name="equalityComparer"></param>
		/// <param name="limit"></param>
		/// <returns></returns>
		public static int RemoveAll<T>(this ICollection<T> source, T value, IEqualityComparer<T> equalityComparer, int limit)
		{
			var rCount = 0;
			for (int i = source.Count - 1; i >= 0; --i)
			{
				var sourceValue = source.ElementAt(i);
				if (equalityComparer.Equals(sourceValue, value))
				{
					source.Remove(sourceValue);
					++rCount;
				}
				if (rCount >= limit)
				{
					break;
				}
			}
			return rCount;
		}
		/// <summary>
		/// Clears the source then copies over the new values while validating every new value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="setting"></param>
		/// <param name="source"></param>
		/// <param name="newValues"></param>
		public static void SetCollection<T>(this IValueParser<T> setting, ICollection<T> source, ICollection<T> newValues)
		{
			source.Clear();
			foreach (var singleValue in newValues)
			{
				setting.ThrowIfInvalid(singleValue);
				source.Add(singleValue);
			}
		}
	}
}