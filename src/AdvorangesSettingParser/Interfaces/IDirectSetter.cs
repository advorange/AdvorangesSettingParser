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
		/// Validates values. Throws an exception if invalid.
		/// </summary>
		Func<T, bool> Validation { get; set; }
		/// <summary>
		/// Factory for generating the default value. Setting this will also invoke it meaning this setting has already been set.
		/// </summary>
		Func<T> DefaultValueFactory { get; set; }

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
}
