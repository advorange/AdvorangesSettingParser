using System.Collections.Generic;

namespace AdvorangesSettingParser.Interfaces
{
	/// <summary>
	/// Only allows parsing and nothing else.
	/// </summary>
	public interface IBasicSettingParser
	{
		/// <summary>
		/// Valid prefixes for a setting.
		/// </summary>
		IEnumerable<string> Prefixes { get; }

		/// <summary>
		/// Finds settings and then sets their value.
		/// </summary>
		/// <param name="input">Input arguments to parse.</param>
		/// <returns>The results of this parsing.</returns>
		ISettingParserResult Parse(string input);
		/// <summary>
		/// Finds settings and then sets their value.
		/// </summary>
		/// <param name="input">Input arguments to parse.</param>
		/// <returns>The results of this parsing.</returns>
		ISettingParserResult Parse(string[] input);
	}
}