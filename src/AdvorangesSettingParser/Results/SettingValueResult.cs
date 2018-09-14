using System;

namespace AdvorangesSettingParser
{
	/// <summary>
	/// Explains why a value was invalid or valid when attempting to set it.
	/// </summary>
	public class SetValueResult : SettingResult
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
		/// Creates an instance of <see cref="SetValueResult"/>.
		/// </summary>
		/// <param name="isSuccess"></param>
		/// <param name="setting"></param>
		/// <param name="parameterType"></param>
		/// <param name="value"></param>
		/// <param name="response"></param>
		protected SetValueResult(bool isSuccess, string response, IBasicSetting setting, Type parameterType, object value)
			: base(setting, false, GenerateResponse(setting, parameterType, value, response))
		{
			ParameterType = parameterType;
			Value = value;
		}

		/// <summary>
		/// Returns a failed result.
		/// </summary>
		/// <param name="setting"></param>
		/// <param name="parameterType"></param>
		/// <param name="value"></param>
		/// <param name="response"></param>
		/// <returns></returns>
		public static SetValueResult FromError(IBasicSetting setting, Type parameterType, object value, string response)
			=> new SetValueResult(false, response, setting, parameterType, value);
		/// <summary>
		/// Returns a successful result.
		/// </summary>
		/// <param name="setting"></param>
		/// <param name="parameterType"></param>
		/// <param name="value"></param>
		/// <param name="response"></param>
		/// <returns></returns>
		public static SetValueResult FromSuccess(IBasicSetting setting, Type parameterType, object value, string response)
			=> new SetValueResult(true, response, setting, parameterType, value);
		/// <summary>
		/// Returns a failed result.
		/// </summary>
		/// <param name="setting"></param>
		/// <param name="value"></param>
		/// <param name="response"></param>
		/// <returns></returns>
		public static SetValueResult FromError<T>(ISetting<T> setting, object value, string response)
			=> FromError(setting, typeof(T), value, response);
		/// <summary>
		/// Returns a successful result.
		/// </summary>
		/// <param name="setting"></param>
		/// <param name="value"></param>
		/// <param name="response"></param>
		/// <returns></returns>
		public static SetValueResult FromSuccess<T>(ISetting<T> setting, object value, string response)
			=> FromSuccess(setting, typeof(T), value, response);

		private static string GenerateResponse(IBasicSetting setting, Type parameterType, object value, string response)
			=> $"{response ?? throw new ArgumentException(nameof(response))} ({setting.MainName}, {value})".TrimStart();
	}
}