namespace AdvorangesSettingParser.Interfaces
{
	/// <summary>
	/// Acts as the ref keyword for multiple types other than fields.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IRef<T>
	{
		/// <summary>
		/// The name of the targeted value.
		/// </summary>
		string Name { get; }

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
}