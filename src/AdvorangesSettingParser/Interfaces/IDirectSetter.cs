using System;

namespace AdvorangesSettingParser
{
	/// <summary>
	/// Allows directly setting the value of a setting.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IDirectSetter<T>
	{
		/// <summary>
		/// Sets the value back to its default value. This still validates.
		/// </summary>
		void SetDefault();
		/// <summary>
		/// Sets the setting directly. This still validates.
		/// </summary>
		/// <param name="value"></param>
		void Set(T value);
	}

	/// <summary>
	/// Allows directly setting the value of a setting.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IDirectSetter
	{
		/// <summary>
		/// Sets the value back to its default value. This still validates.
		/// </summary>
		void SetDefault();
		/// <summary>
		/// Sets the setting directly. This still validates.
		/// </summary>
		/// <param name="value"></param>
		void Set(object value);
	}
}
