namespace AdvorangesSettingParser
{
	/// <summary>
	/// Only allows parsing and nothing else.
	/// </summary>
	public interface IBasicSettingParser
	{
		/// <summary>
		/// Finds settings and then sets their value.
		/// </summary>
		/// <param name="input">Input arguments to parse.</param>
		/// <returns>The results of this parsing.</returns>
		ISettingParserResults Parse(string input);
		/// <summary>
		/// Finds settings and then sets their value.
		/// </summary>
		/// <param name="input">Input arguments to parse.</param>
		/// <returns>The results of this parsing.</returns>
		ISettingParserResults Parse(string[] input);
	}
}