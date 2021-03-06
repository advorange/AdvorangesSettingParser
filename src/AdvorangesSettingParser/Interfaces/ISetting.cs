﻿namespace AdvorangesSettingParser.Interfaces
{
	/// <summary>
	/// Allows attempting to set a value and holds information allowing about how to set the value.
	/// </summary>
	public interface IBasicSetting : ISettingMetadata
	{
		/// <summary>
		/// Sets the value back to its default value. This still validates.
		/// </summary>
		void ResetValue();
		/// <summary>
		/// Converts the value to the required type and sets the property/field.
		/// </summary>
		/// <param name="value">The passed in argument to convert.</param>
		/// <returns>Whether the value has successfully been set.</returns>
		IResult TrySetValue(string value);
		/// <summary>
		/// Converts the value to the required type and sets the property/field.
		/// </summary>
		/// <param name="value">The passed in argument to convert.</param>
		/// <param name="context">Additional arguments provided.</param>
		/// <returns>Whether the value has successfully been set.</returns>
		IResult TrySetValue(string value, ITrySetValueContext context);
	}

	/// <summary>
	/// Allows try setting, regular setting, getting, and resetting the value.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ISetting<T> : IBasicSetting
	{
		/// <summary>
		/// The current value of the setting.
		/// </summary>
		T GetValue();
		/// <summary>
		/// Sets the setting directly. This still validates.
		/// </summary>
		/// <param name="value"></param>
		void SetValue(T value);
	}

	/// <summary>
	/// Allows try setting, regular setting, getting, and resetting the value.
	/// </summary>
	public interface ISetting : IBasicSetting
	{
		/// <summary>
		/// The current value of the setting.
		/// </summary>
		object GetValue();
		/// <summary>
		/// Sets the setting directly. This still validates.
		/// </summary>
		/// <param name="value"></param>
		void SetValue(object value);
	}
}