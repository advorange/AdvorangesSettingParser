using System.Collections.Generic;
using AdvorangesSettingParser.Implementation;

namespace AdvorangesSettingParser.Interfaces
{
	/// <summary>
	/// Allows for parsing arguments and seeing the metadata of settings.
	/// </summary>
	public interface ISettingParser
	{
		/// <summary>
		/// The prefixes of this parser.
		/// </summary>
		IEnumerable<string> Prefixes { get; }

		/// <summary>
		/// The settings of this parser.
		/// </summary>
		/// <returns></returns>
		IEnumerable<ISettingMetadata> GetSettings();
		/// <summary>
		/// Attempts to get a setting with the specified name.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="state"></param>
		/// <param name="setting"></param>
		/// <returns></returns>
		bool TryGetSetting(string name, PrefixState state, out ISettingMetadata setting);
		/// <summary>
		/// Either gets a setting with the specified name or throws an exception.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="state"></param>
		/// <returns></returns>
		ISettingMetadata GetSetting(string name, PrefixState state);
		/// <summary>
		/// <paramref name="source"/> is required if this is a static setting parser, otherwise it is ignored.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		ISettingParserResult Parse(object source, ParseArgs input);
	}
}