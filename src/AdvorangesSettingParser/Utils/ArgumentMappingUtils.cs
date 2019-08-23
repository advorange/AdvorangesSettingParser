using System;
using AdvorangesUtils;

namespace AdvorangesSettingParser.Utils
{
	/// <summary>
	/// Methods for putting a setting and its arguments together.
	/// </summary>
	public static class ArgumentMappingUtils
	{
		/// <summary>
		/// Removes the prefix from the start of the string if it exists.
		/// </summary>
		/// <param name="prefix"></param>
		/// <param name="input"></param>
		/// <param name="state"></param>
		/// <returns></returns>
		public static string Deprefix(string prefix, string input, PrefixState state)
		{
			switch (state)
			{
				case PrefixState.Required:
					return input.CaseInsStartsWith(prefix) ? input.Substring(prefix.Length) : null;
				case PrefixState.Optional:
					return input.CaseInsStartsWith(prefix) ? input.Substring(prefix.Length) : input;
				case PrefixState.NotPrefixed:
					return input;
				default:
					throw new InvalidOperationException("Invalid prefix state provided.");
			}
		}
	}
}