namespace AdvorangesSettingParser
{
	/// <summary>
	/// Allows resetting the targeted value.
	/// </summary>
	public interface IResettable
	{
		/// <summary>
		/// Sets the value back to its default value. This still validates.
		/// </summary>
		void ResetValue();
	}
}
