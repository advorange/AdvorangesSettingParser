using System.Collections.Generic;

namespace AdvorangesSettingParser.Interfaces
{
	/// <summary>
	/// Results gotten from parsing.
	/// </summary>
	public interface ISettingParserResult : IResult
	{
		/// <summary>
		/// Parts that were not used to set something.
		/// </summary>
		IReadOnlyCollection<IResult> UnusedParts { get; }
		/// <summary>
		/// All successfully set settings.
		/// </summary>
		IReadOnlyCollection<IResult> Successes { get; }
		/// <summary>
		/// Any errors which occurred when setting something.
		/// </summary>
		IReadOnlyCollection<IResult> Errors { get; }
		/// <summary>
		/// Result gotten via the help setting.
		/// </summary>
		IReadOnlyCollection<IResult> Help { get; }
	}
}