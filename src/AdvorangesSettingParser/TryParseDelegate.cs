namespace AdvorangesSettingParser
{
	/// <summary>
	/// Attempts to convert the string to the supplied type.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="s">Supplied string.</param>
	/// <param name="value">Converted value.</param>
	/// <returns></returns>
	public delegate bool TryParseDelegate<T>(string s, out T value);
}