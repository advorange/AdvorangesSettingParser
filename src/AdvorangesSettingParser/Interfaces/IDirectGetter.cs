namespace AdvorangesSettingParser
{
	/// <summary>
	/// Allows directly getting the value of a setting.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IDirectGetter<T>
	{
		/// <summary>
		/// The current value of the setting.
		/// </summary>
		T GetValue();
	}
}
