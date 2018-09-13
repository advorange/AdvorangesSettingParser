using System.Collections.Generic;

namespace AdvorangesSettingParser
{
	/// <summary>
	/// This interface is for a setting parser which allows showing some additional information about a <see cref="IBasicSettingParser"/>.
	/// </summary>
	/// <typeparam name="T">The type of settings to use.</typeparam>
	public interface ISettingParser<T> : ISettingParserMetadata, IBasicSettingParser where T : IBasicSetting
	{
		/// <summary>
		/// Returns the settings.
		/// </summary>
		/// <returns>All of the settings this parser holds.</returns>
		IEnumerable<T> GetSettings();
		/// <summary>
		/// Returns a matching setting.
		/// </summary>
		/// <param name="name">The setting to get.</param>
		/// <param name="state">How required the prefix is.</param>
		/// <returns>The setting with either the specified name or alias.</returns>
		T GetSetting(string name, PrefixState state);
	}

	/// <summary>
	/// This interface is for a setting parser which allows showing some additional information about a <see cref="IBasicSettingParser"/>.
	/// </summary>
	public interface ISettingParser : ISettingParserMetadata, IBasicSettingParser
	{
		/// <summary>
		/// Returns the settings.
		/// </summary>
		/// <returns>All of the settings this parser holds.</returns>
		IEnumerable<IBasicSetting> GetSettings();
		/// <summary>
		/// Returns a matching setting.
		/// </summary>
		/// <param name="name">The setting to get.</param>
		/// <param name="state">How required the prefix is.</param>
		/// <returns>The setting with either the specified name or alias.</returns>
		IBasicSetting GetSetting(string name, PrefixState state);
	}
}