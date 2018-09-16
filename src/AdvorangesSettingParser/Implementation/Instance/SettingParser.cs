using System.Collections.Generic;
using AdvorangesSettingParser.Interfaces;
using AdvorangesUtils;

namespace AdvorangesSettingParser.Implementation.Instance
{
	/// <summary>
	/// Parses settings and then sets them.
	/// This implementation is case-insensitive.
	/// </summary>
	/// <remarks>Putting a command with the name help or h will overwrite the help command.</remarks>
	public class SettingParser : SettingParserBase<ISetting>
	{
		/// <summary>
		/// Creates an instance of <see cref="SettingParser"/> with the supplied prefixes.
		/// </summary>
		/// <param name="prefixes"></param>
		public SettingParser(IEnumerable<string> prefixes = default) : base(prefixes) { }

		/// <inheritdoc />
		public ISettingParserResult Parse(string input)
			=> Parse(input.SplitLikeCommandLine());
		/// <inheritdoc />
		public ISettingParserResult Parse(string[] input)
			=> Parse(input, (setting, value) => setting.TrySetValue(value));
	}
}