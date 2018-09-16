using System;
using AdvorangesSettingParser.Interfaces;

namespace AdvorangesSettingParser.Results
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
		protected SetValueResult(bool isSuccess, string response, ISettingMetadata setting, Type parameterType, object value)
			: base(setting, isSuccess, GenerateResponse(setting, parameterType, value, response))
		{
			ParameterType = parameterType;
			Value = value;
		}

		/// <summary>
		/// Returns a failed result.
		/// </summary>
		/// <param name="setting"></param>
		/// <param name="value"></param>
		/// <param name="response"></param>
		/// <returns></returns>
		public static SetValueResult FromError(ISettingMetadata setting, object value, string response)
			=> new SetValueResult(false, response, setting, setting.ValueType, value);
		/// <summary>
		/// Returns a successful result.
		/// </summary>
		/// <param name="setting"></param>
		/// <param name="value"></param>
		/// <param name="response"></param>
		/// <returns></returns>
		public static SetValueResult FromSuccess(ISettingMetadata setting, object value, string response)
			=> new SetValueResult(true, response, setting, setting.ValueType, value);

		private static string GenerateResponse(ISettingMetadata setting, Type parameterType, object value, string response)
			=> $"{response ?? throw new ArgumentException(nameof(response))} ({setting.MainName}, {value?.ToString() ?? "NULL"})".TrimStart();
	}
}