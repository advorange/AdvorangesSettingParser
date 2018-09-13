namespace AdvorangesSettingParser
{
	/// <summary>
	/// Result for something to do with a setting.
	/// </summary>
	public abstract class SettingResult : Result
	{
		/// <summary>
		/// The targeted setting.
		/// </summary>
		public IBasicSetting Setting { get; }

		/// <summary>
		/// Creates an instance of <see cref="SettingResult"/>.
		/// </summary>
		/// <param name="setting"></param>
		/// <param name="isSuccess"></param>
		/// <param name="response"></param>
		protected SettingResult(IBasicSetting setting, bool isSuccess, string response) : base(isSuccess, response)
		{
			Setting = setting;
		}
	}
}
