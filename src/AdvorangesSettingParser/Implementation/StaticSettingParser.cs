using System.Collections.Generic;
using AdvorangesSettingParser.Interfaces;
using AdvorangesUtils;

namespace AdvorangesSettingParser.Implementation
{
	public class StaticSettingParser<TSource> : SettingParserBase<IStaticSetting<TSource>>
	{
		/// <summary>
		/// Creates an instance of <see cref="StaticSettingParser{TSource}"/> with the supplied prefixes.
		/// </summary>
		/// <param name="prefixes"></param>
		public StaticSettingParser(IEnumerable<string> prefixes = default) : base(prefixes) { }

		/// <inheritdoc />
		public ISettingParserResult Parse(TSource source, string input)
			=> Parse(source, input.SplitLikeCommandLine());
		/// <inheritdoc />
		public ISettingParserResult Parse(TSource source, string[] input)
			=> Parse(input, (setting, value) => setting.TrySetValue(source, value));
	}
}