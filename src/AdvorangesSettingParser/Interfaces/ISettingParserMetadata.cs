using System.Collections.Generic;

namespace AdvorangesSettingParser
{
	/// <summary>
	/// Lets the prefixes and help get accessed.
	/// </summary>
	/// <remarks>
	/// This is split from <see cref="IBasicSettingParser"/> because this is not needed to run it.
	/// This only gives information about the parser's commands or the parser itself.
	/// </remarks>
	public interface ISettingParserMetadata
	{
		/// <summary>
		/// Valid prefixes for a setting.
		/// </summary>
		IEnumerable<string> Prefixes { get; }

		/// <summary>
		/// Returns information either about the settings in general, or the specified setting.
		/// </summary>
		/// <param name="name">The setting to target. Can be null if wanting to list out every setting.</param>
		/// <returns>Help information about either the setting or all settings.</returns>
		string GetHelp(string name);
	}
}