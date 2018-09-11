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
		/// <param name="input"></param>
		ISettingParserResults Parse(string input);
		/// <summary>
		/// Finds settings and then sets their value.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		ISettingParserResults Parse(string[] input);
	}
}