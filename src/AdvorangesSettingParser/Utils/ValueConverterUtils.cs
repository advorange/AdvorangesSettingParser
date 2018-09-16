using System;
using AdvorangesSettingParser.Interfaces;
using AdvorangesSettingParser.Results;
using AdvorangesUtils;

namespace AdvorangesSettingParser.Utils
{
	/// <summary>
	/// Utilities for <see cref="IValueParser{TValue}"/>.
	/// </summary>
	public static class ValueConverterUtils
	{
		/// <summary>
		/// Throws an argument exception if the value is invalid.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="converter"></param>
		/// <param name="value"></param>
		public static void ThrowIfInvalid<T>(this IValueParser<T> converter, T value)
		{
			var validationResult = converter.Validation(value);
			if (!validationResult.IsSuccess)
			{
				throw new ArgumentException(validationResult.ToString(), converter.MainName);
			}
		}
		/// <summary>
		/// Attempts to convert the value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="converter"></param>
		/// <param name="value"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static IResult TryConvertValue<T>(this IValueParser<T> converter, string value, out T result)
		{
			result = default;
			//Let default just pass on through and set the default value
			if (!value.CaseInsEquals("default") && !converter.Parser(value, out result))
			{
				return SetValueResult.FromError(converter, value, "Unable to convert.");
			}
			if (result == null && converter.CannotBeNull)
			{
				return SetValueResult.FromError(converter, value, "Cannot be set to 'NULL'.");
			}

			return converter.Validation(result) ?? Result.FromSuccess("Successfully converted");
		}
	}
}