using System;
using System.Collections.Generic;

namespace AdvorangesSettingParser.Interfaces
{
	/// <summary>
	/// Parses and validates values for a setting.
	/// </summary>
	/// <typeparam name="TValue"></typeparam>
	public interface IValueParser<TValue> : ISettingMetadata
	{
		/// <summary>
		/// Compares for equality among the values.
		/// </summary>
		IEqualityComparer<TValue> EqualityComparer { get; }
		/// <summary>
		/// How to parse the value from a string.
		/// </summary>
		TryParseDelegate<TValue> Parser { get; set; }
		/// <summary>
		/// How to validate the parsed value.
		/// </summary>
		Func<TValue, IResult> Validation { get; set; }
	}
}