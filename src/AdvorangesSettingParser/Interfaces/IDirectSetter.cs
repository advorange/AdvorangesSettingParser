namespace AdvorangesSettingParser
{
	/// <summary>
	/// Allows directly setting the value of a setting.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IDirectSetter<T>
	{
		/// <summary>
		/// Sets the setting directly. This still validates.
		/// </summary>
		/// <param name="value"></param>
		void SetValue(T value);
	}

	/// <summary>
	/// Allows directly setting the value of a setting.
	/// </summary>
	public interface IDirectSetter
	{
		/// <summary>
		/// Sets the setting directly. This still validates.
		/// </summary>
		/// <param name="value"></param>
		void SetValue(object value);
	}
}
