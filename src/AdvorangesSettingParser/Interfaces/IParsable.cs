using AdvorangesSettingParser.Implementation.Instance;

namespace AdvorangesSettingParser.Interfaces
{
	/// <summary>
	/// Indicates this setting can be parsed.
	/// </summary>
	public interface IParsable
	{
		/// <summary>
		/// Specifies how to parse settings into this class.
		/// </summary>
		SettingParser SettingParser { get; }
	}
}
