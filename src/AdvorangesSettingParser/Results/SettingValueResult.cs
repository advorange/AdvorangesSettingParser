using System;

namespace AdvorangesSettingParser
{
	/// <summary>
	/// Explains why a value was invalid or valid.
	/// </summary>
	public class SettingValueResult : SettingResult
	{
		/// <summary>
		/// The parameter's type.
		/// </summary>
		public Type ParameterType { get; }
		/// <summary>
		/// The passed in value.
		/// </summary>
		public object Value { get; }

		/// <summary>
		/// Creates an instance of <see cref="SettingValueResult"/>.
		/// </summary>
		/// <param name="isSuccess"></param>
		/// <param name="setting"></param>
		/// <param name="parameterType"></param>
		/// <param name="value"></param>
		/// <param name="response"></param>
		protected SettingValueResult(bool isSuccess, string response, IBasicSetting setting, Type parameterType, object value)
			: base(setting, false, GenerateResponse(setting, parameterType, value, response))
		{
			ParameterType = parameterType;
			Value = value;
		}

		/// <summary>
		/// Returns a successful result.
		/// </summary>
		/// <param name="setting"></param>
		/// <param name="parameterType"></param>
		/// <param name="value"></param>
		/// <param name="response"></param>
		/// <returns></returns>
		public static SettingValueResult FromError(IBasicSetting setting, Type parameterType, object value, string response)
			=> new SettingValueResult(false, response, setting, parameterType, value);
		/// <summary>
		/// Returns a successful result.
		/// </summary>
		/// <param name="setting"></param>
		/// <param name="parameterType"></param>
		/// <param name="value"></param>
		/// <param name="response"></param>
		/// <returns></returns>
		public static SettingValueResult FromSuccess(IBasicSetting setting, Type parameterType, object value, string response)
			=> new SettingValueResult(true, response, setting, parameterType, value);

		private static string GenerateResponse(IBasicSetting setting, Type parameterType, object value, string response)
			=> $"{response ?? throw new ArgumentException(nameof(response))} ({setting.MainName}, {parameterType.Name}, {value})".TrimStart();
	}
}