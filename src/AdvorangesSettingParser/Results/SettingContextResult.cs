using System;
using AdvorangesSettingParser.Interfaces;

namespace AdvorangesSettingParser.Results
{
	/// <summary>
	/// Result about the context of try set value.
	/// </summary>
	public class SettingContextResult : SettingResult
	{
		/// <summary>
		/// The expected type.
		/// </summary>
		public Type ExpectedType { get; }
		/// <summary>
		/// The given type.
		/// </summary>
		public Type GivenType { get; }

		/// <summary>
		/// Creates an instance of <see cref="SettingContextResult"/>.
		/// </summary>
		/// <param name="isSuccess"></param>
		/// <param name="setting"></param>
		/// <param name="expected"></param>
		/// <param name="given"></param>
		/// <param name="response"></param>
		protected SettingContextResult(bool isSuccess, string response, ISettingMetadata setting, Type expected, Type given)
			: base(setting, isSuccess, GenerateResponse(setting, expected, given, response))
		{
			ExpectedType = expected;
			GivenType = given;
		}

		/// <summary>
		/// Returns a successful result.
		/// </summary>
		/// <param name="setting"></param>
		/// <param name="expected"></param>
		/// <param name="given"></param>
		/// <param name="response"></param>
		/// <returns></returns>
		public static SettingContextResult FromError(ISettingMetadata setting, Type expected, Type given, string response)
			=> new SettingContextResult(false, response, setting, expected, given);
		/// <summary>
		/// Returns a failed result.
		/// </summary>
		/// <param name="setting"></param>
		/// <param name="expected"></param>
		/// <param name="given"></param>
		/// <param name="response"></param>
		/// <returns></returns>
		public static SettingContextResult FromSuccess(ISettingMetadata setting, Type expected, Type given, string response)
			=> new SettingContextResult(true, response, setting, expected, given);
		private static string GenerateResponse(ISettingMetadata setting, Type expected, Type given, string response)
			=> $"{response ?? throw new ArgumentException(nameof(response))} ({setting.MainName}, {expected.Name}, {given?.Name ?? "Nothing"})".TrimStart();
	}
}