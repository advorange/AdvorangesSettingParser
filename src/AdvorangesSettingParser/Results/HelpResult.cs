namespace AdvorangesSettingParser.Results
{
	/// <summary>
	/// Indicates this is a result that contains information from a help command.
	/// </summary>
	public class HelpResult : Result
	{
		/// <summary>
		/// Creates an instance of <see cref="HelpResult"/>.
		/// </summary>
		/// <param name="help"></param>
		public HelpResult(string help) : base(true, help) { }
	}
}
