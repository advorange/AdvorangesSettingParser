using System.Collections.Generic;

namespace AdvorangesSettingParser
{
	/// <summary>
	/// This interface is for a setting parser which allows showing some additional information about a <see cref="IBasicSettingParser"/>.
	/// </summary>
	public interface ISettingParser : IBasicSettingParser
	{
		/// <summary>
		/// Returns true if every setting has been set or is optional.
		/// </summary>
		/// <returns></returns>
		bool AllSet { get; }
		/// <summary>
		/// Valid prefixes for a setting.
		/// </summary>
		IEnumerable<string> Prefixes { get; }

		/// <summary>
		/// Gets the help information associated with this setting name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		string GetHelp(string name);
		/// <summary>
		/// Returns a string asking for unset settings.
		/// </summary>
		/// <returns></returns>
		string GetNeededSettings();
		/// <summary>
		/// Gets a setting with the supplied name. The setting must start with a prefix.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="state"></param>
		/// <returns></returns>
		ISetting GetSetting(string name, PrefixState state);
	}
}