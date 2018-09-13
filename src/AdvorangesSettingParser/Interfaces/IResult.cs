namespace AdvorangesSettingParser
{
	/// <summary>
	/// Indicates 
	/// </summary>
	public interface IResult
	{
		/// <summary>
		/// The response as a string.
		/// </summary>
		string Response { get; }
		/// <summary>
		/// Returns true if the result is a success.
		/// </summary>
		bool IsSuccess { get; }
	}
}