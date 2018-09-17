using System.Collections.Generic;
using AdvorangesSettingParser.Interfaces;

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
		public StaticSettingParser(IEnumerable<string> prefixes = default) : base(prefixes)
		{
			Add(new StaticSettingHelpCommand(this));
		}

		/// <inheritdoc />
		protected override ISettingParserResult Parse(object source, ParseArgs input)
			=> Parse((TSource)source, input);
		/// <inheritdoc />
		public ISettingParserResult Parse(TSource source, ParseArgs input)
			=> Parse(input, (setting, value) => setting.TrySetValue(source, value));

		private class StaticSettingHelpCommand : HelpCommand, IStaticSetting<TSource>
		{
			public StaticSettingHelpCommand(ISettingParser parent) : base(parent) { }

			//IStaticSetting
			object IStaticSetting<TSource>.GetValue(TSource source) => null;
			void IBasicStaticSetting<TSource>.ResetValue(TSource source) { }
			void IStaticSetting<TSource>.SetValue(TSource source, object value) { }
			IResult IBasicStaticSetting<TSource>.TrySetValue(TSource source, string value) => GetHelp(value);
			IResult IBasicStaticSetting<TSource>.TrySetValue(TSource source, string value, ITrySetValueContext context) => GetHelp(value);
		}
	}
}