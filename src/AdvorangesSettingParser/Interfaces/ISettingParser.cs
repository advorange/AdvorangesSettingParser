using System.Collections.Generic;

namespace AdvorangesSettingParser
{
	/// <summary>
	/// This interface is for a setting parser which allows showing some additional information about a <see cref="IBasicSettingParser"/>.
	/// </summary>
	public interface ISettingParser : IBasicSettingParser
	{
		/// <summary>
		/// Valid prefixes for a setting.
		/// </summary>
		IEnumerable<string> Prefixes { get; }

		/// <summary>
		/// Returns the settings.
		/// </summary>
		/// <returns>All of the settings this parser holds.</returns>
		IEnumerable<ISetting> GetSettings();
		/// <summary>
		/// Returns a matching setting.
		/// </summary>
		/// <param name="name">The setting to get.</param>
		/// <param name="state">How required the prefix is.</param>
		/// <returns>The setting with either the specified name or alias.</returns>
		ISetting GetSetting(string name, PrefixState state);
		/// <summary>
		/// Returns information either about the settings in general, or the specified setting.
		/// </summary>
		/// <param name="name">The setting to target. Can be null if wanting to list out every setting.</param>
		/// <returns>Help information about either the setting or all settings.</returns>
		string GetHelp(string name);
	}
}