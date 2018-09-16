using System.Collections.Generic;
using AdvorangesSettingParser.Interfaces;
using AdvorangesUtils;

namespace AdvorangesSettingParser.Implementation.Static
{
	/// <summary>
	/// Allows creation of the settings statically, then providing an instance of the target type to fill up.
	/// </summary>
	/// <typeparam name="TSource"></typeparam>
	public class StaticSettingParser<TSource> : SettingParserBase<IStaticSetting<TSource>>
	{
		/// <summary>
		/// Creates an instance of <see cref="StaticSettingParser{TSource}"/> with the supplied prefixes.
		/// </summary>
		/// <param name="prefixes"></param>
		public StaticSettingParser(IEnumerable<string> prefixes = default) : base(prefixes) { }

		/// <inheritdoc />
		protected override ISettingParserResult Parse(object source, ParseArgs input)
			=> Parse((TSource)source, input);
		/// <inheritdoc />
		public ISettingParserResult Parse(TSource source, ParseArgs input)
			=> Parse(input, (setting, value) => setting.TrySetValue(source, value));
	}
}