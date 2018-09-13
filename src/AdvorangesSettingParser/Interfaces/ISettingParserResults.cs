using System.Collections.Generic;

namespace AdvorangesSettingParser
{
	/// <summary>
	/// Results gotten from parsing.
	/// </summary>
	public interface ISettingParserResult : IResult
	{
		/// <summary>
		/// Parts that were not used to set something.
		/// </summary>
		IEnumerable<IResult> UnusedParts { get; }
		/// <summary>
		/// All successfully set settings.
		/// </summary>
		IEnumerable<IResult> Successes { get; }
		/// <summary>
		/// Any errors which occurred when setting something.
		/// </summary>
		IEnumerable<IResult> Errors { get; }
		/// <summary>
		/// Result gotten via the help setting.
		/// </summary>
		IEnumerable<IResult> Help { get; }
	}
}