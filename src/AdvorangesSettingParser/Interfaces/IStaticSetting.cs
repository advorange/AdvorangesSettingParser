namespace AdvorangesSettingParser.Interfaces
{
	/// <summary>
	/// Allows attempting to set a value and holds information allowing about how to set the value.
	/// </summary>
	/// <typeparam name="TSource"></typeparam>
	public interface IBasicStaticSetting<TSource> : ISettingMetadata
	{
		/// <summary>
		/// Sets the value back to its default value. This still validates.
		/// </summary>
		void ResetValue(TSource source);
		/// <summary>
		/// Converts the value to the required type and sets the property/field.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="value">The passed in argument to convert.</param>
		/// <param name="response">Response which can either be a success or failure string.</param>
		/// <returns>Whether the value has successfully been set.</returns>
		IResult TrySetValue(TSource source, string value);
		/// <summary>
		/// Converts the value to the required type and sets the property/field.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="value">The passed in argument to convert.</param>
		/// <param name="context">Additional arguments provided.</param>
		/// <param name="response">Response which can either be a success or failure string.</param>
		/// <returns>Whether the value has successfully been set.</returns>
		IResult TrySetValue(TSource source, string value, ITrySetValueContext context);
	}

	/// <summary>
	/// Allows try setting, regular setting, getting, and resetting the value.
	/// </summary>
	/// <typeparam name="TSource"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public interface IStaticSetting<TSource, TValue> : IBasicStaticSetting<TSource>
	{
		/// <summary>
		/// The current value of the setting.
		/// </summary>
		/// <param name="source"></param>
		TValue GetValue(TSource source);
		/// <summary>
		/// Sets the setting directly. This still validates.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="value"></param>
		void SetValue(TSource source, TValue value);
	}

	/// <summary>
	/// Allows try setting, regular setting, getting, and resetting the value.
	/// </summary>
	/// <typeparam name="TSource"></typeparam>
	public interface IStaticSetting<TSource> : IBasicStaticSetting<TSource>
	{
		/// <summary>
		/// The current value of the setting.
		/// </summary>
		/// <param name="source"></param>
		object GetValue(TSource source);
		/// <summary>
		/// Sets the setting directly. This still validates.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="value"></param>
		void SetValue(TSource source, object value);
	}
}