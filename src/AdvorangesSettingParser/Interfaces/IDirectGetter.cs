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

	/// <summary>
	/// Allows directly getting the value of a setting.
	/// </summary>
	public interface IDirectGetter
	{
		/// <summary>
		/// The current value of the setting.
		/// </summary>
		object GetValue();
	}
}
